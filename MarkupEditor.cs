using System;
using System.Collections.Generic;

// ReSharper disable once RedundantUsingDirective
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using MarkupEditor.Properties;

// ReSharper disable InvalidXmlDocComment

// ReSharper disable UnusedParameter.Local

namespace MarkupEditor;

internal sealed partial class MarkupEditor : Form
{
    #region Fields and construction

    private const int EmRedo = 0x0454;
    private const float DefaultEditorFontSize = 11f;
    private const int RecentDocumentsCapacity = 10;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    private string _currentFilePath;
    private bool _isDirty;
    private bool _suppressDirtyFlag;
    private bool _livePreviewEnabled;
    private int _pendingPreviewScrollLine = -1;

    /// <summary>
    /// Actions to run on the UI thread after <see cref="Control.Handle"/> exists, queued from <see cref="BeginInvokeWhenHandleReady"/>.
    /// </summary>
    private readonly Queue<Action> _deferredUntilHandleCreated = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkupEditor"/> form.
    /// </summary>
    /// <param name="commandLineFilePath">Optional path from the command line to open on startup.</param>
    public MarkupEditor(string commandLineFilePath = null)
    {
        InitializeComponent();
        TryApplyApplicationIconFromExecutable();

        // Wire up event handlers to designer-created controls
        fileRecentDocuments.DropDownOpening += fileRecentDocuments_DropDownOpening;
        _editorTextBox.TextChanged += _editorTextBox_TextChanged;
        _editorTextBox.KeyUp += _editorTextBox_KeyUp;
        _editorTextBox.MouseUp += _editorTextBox_MouseUp;
        _previewBrowser.DocumentCompleted += _previewBrowser_DocumentCompleted;
        _previewTimer.Tick += _previewTimer_Tick;
        Shown += MarkupEditor_Shown;
        Load += MarkupEditor_Load;

        LoadEditorSettingsFromStorage();
        RefreshRecentDocumentsMenu();

        if (!string.IsNullOrWhiteSpace(commandLineFilePath))
        {
            string trimmedPath = commandLineFilePath.Trim().Trim('"');
            if (!TryOpenFileAtPath(trimmedPath)) LoadSampleDocument();
        }
        else
            LoadSampleDocument();

        UpdateWindowState();
    }

    #endregion

    #region Form appearance and split layout

    /// <summary>
    /// Assigns the window icon from the application icon embedded in the executable.
    /// </summary>
    private void TryApplyApplicationIconFromExecutable()
    {
        try
        {
            string path = Application.ExecutablePath;

            if (string.IsNullOrEmpty(path)) return;

            using Icon extracted = Icon.ExtractAssociatedIcon(path);
            if (extracted != null) Icon = (Icon)extracted.Clone();
        }
        catch (ArgumentException)
        {
            // No icon resource; keep the default.
        }
    }

    /// <summary>
    /// Handles the <see cref="Form.Load"/> event; restores window position and size from user settings.
    /// </summary>
    private void MarkupEditor_Load(object sender, EventArgs e) => ApplyWindowSettingsFromStorage();

    /// <summary>
    /// Handles the <see cref="Form.Shown"/> event; configures the split panel layout after initial rendering.
    /// </summary>
    private void MarkupEditor_Shown(object sender, EventArgs e)
    {
        ConfigureSafePanelMinimums();
        SetSafeSplitterDistance(Settings.Default.SplitterDistance);
    }

    /// <summary>
    /// Calculates and applies safe minimum panel sizes for the split container.
    /// </summary>
    private void ConfigureSafePanelMinimums()
    {
        int available = Math.Max(0, _mainSplit.Width - 20);
        const int desired = 250;
        int capped = available / 2;
        int safeMin = Math.Max(50, Math.Min(desired, capped));

        _mainSplit.Panel1MinSize = safeMin;
        _mainSplit.Panel2MinSize = safeMin;
    }

    /// <summary>
    /// Sets the splitter position clamped within safe panel bounds.
    /// </summary>
    /// <param name="preferred">The desired splitter distance in pixels.</param>
    private void SetSafeSplitterDistance(int preferred)
    {
        int min = _mainSplit.Panel1MinSize;
        int max = _mainSplit.Width - _mainSplit.Panel2MinSize;

        if (max < min) return;

        int clamped = Math.Min(Math.Max(preferred, min), max);
        _mainSplit.SplitterDistance = clamped;
    }

    #endregion

    #region Editor, debounce timer, and preview sync

    /// <summary>
    /// Handles text changes in the editor; marks the document dirty and restarts the live-preview debounce timer.
    /// </summary>
    private void _editorTextBox_TextChanged(object sender, EventArgs e)
    {
        if (!_suppressDirtyFlag)
        {
            _isDirty = true;
            UpdateWindowState();
        }

        if (!_livePreviewEnabled) return;

        _previewTimer.Stop();
        _previewTimer.Start();
    }

    /// <summary>
    /// Handles the debounce timer tick; stops the timer and triggers a preview render.
    /// </summary>
    private void _previewTimer_Tick(object sender, EventArgs e)
    {
        _previewTimer.Stop();
        DoRenderPreview();
    }

    /// <summary>
    /// Handles key up in the editor; syncs preview scroll and active line to the caret.
    /// </summary>
    private void _editorTextBox_KeyUp(object sender, KeyEventArgs e) =>
        SyncPreviewToCaretLine(GetEditorCaretLine());

    /// <summary>
    /// Handles mouse up in the editor; syncs preview when the caret moves via clicking or selection.
    /// </summary>
    private void _editorTextBox_MouseUp(object sender, MouseEventArgs e) =>
        SyncPreviewToCaretLine(GetEditorCaretLine());

    /// <summary>
    /// Handles completion of preview document loading; applies any pending scroll-to-line request.
    /// </summary>
    private void _previewBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
    {
        if (_previewBrowser.ReadyState != WebBrowserReadyState.Complete) return;

        if (_pendingPreviewScrollLine < 0) return;

        int line = _pendingPreviewScrollLine;
        _pendingPreviewScrollLine = -1;
        BeginInvokeWhenHandleReady(() => SyncPreviewToCaretLine(line));
    }

    /// <summary>
    /// Returns the zero-based logical line index of the caret (newline-delimited), matching <see cref="MarkupParser"/> line numbering.
    /// </summary>
    /// <returns>The current caret line.</returns>
    private int GetEditorCaretLine()
    {
        string text = _editorTextBox.Text ?? string.Empty;
        int caret = Math.Min(Math.Max(0, _editorTextBox.SelectionStart), text.Length);
        int line = 0;

        for (int i = 0; i < caret; i++)
        {
            char c = text[i];

            switch (c)
            {
                case '\n':
                    line++;

                    break;

                case '\r':
                {
                    line++;
                    if (i + 1 < text.Length && text[i + 1] == '\n') i++;

                    break;
                }
            }
        }

        return line;
    }

    /// <summary>
    /// Scrolls the preview to the source line and applies the active-line highlight in the rendered HTML.
    /// </summary>
    /// <param name="editorLineIndex">Zero-based source line index.</param>
    private void SyncPreviewToCaretLine(int editorLineIndex)
    {
        ScrollPreviewToEditorLine(editorLineIndex);
        TryInvokePreviewActiveLineHighlight(editorLineIndex);
    }

    /// <summary>
    /// Scrolls the preview so the HTML block for the given source line is visible, aligned with the editor line.
    /// </summary>
    /// <param name="editorLineIndex">Zero-based source line index.</param>
    private void ScrollPreviewToEditorLine(int editorLineIndex)
    {
        if (editorLineIndex < 0) return;

        HtmlDocument document = _previewBrowser.Document;

        if (document == null) return;

        for (int n = editorLineIndex; n >= 0; n--)
        {
            HtmlElement element = document.GetElementById("me-line-" + n);

            if (element == null) continue;

            element.ScrollIntoView(true);

            break;
        }
    }

    /// <summary>
    /// Calls the preview script to highlight the block for <c>me-line-{n}</c> and clear previous highlights.
    /// </summary>
    /// <param name="editorLineIndex">Zero-based source line index.</param>
    private void TryInvokePreviewActiveLineHighlight(int editorLineIndex)
    {
        if (editorLineIndex < 0) return;

        if (_previewBrowser.Document == null) return;

        if (_previewBrowser.ReadyState != WebBrowserReadyState.Complete) return;

        try
        {
            _previewBrowser.Document.InvokeScript("MarkupSetActiveLine", new object[] { editorLineIndex });
        }
        catch (InvalidOperationException)
        {
            // Document or scripting surface not ready for InvokeScript.
        }
        catch (COMException)
        {
            // Legacy WebBrowser host may surface COM failures when the document is busy.
        }
#if DEBUG
        catch (Exception ex)
        {
            Debug.WriteLine("[MarkupEditor] MarkupSetActiveLine failed: " + ex.Message);
        }
#else
        catch (Exception)
        {
            // ignored
        }
#endif
    }

    #endregion

    #region Form closing

    /// <summary>
    /// Handles the <see cref="Form.FormClosing"/> event; prompts to save unsaved changes, then persists user settings.
    /// </summary>
    private void MarkupEditor_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!PromptToSaveChanges())
        {
            e.Cancel = true;

            return;
        }

        SaveApplicationSettings();
    }

    #endregion

    #region Application settings (persisted)

    /// <summary>
    /// Applies editor and preview settings from persisted user settings.
    /// </summary>
    private void LoadEditorSettingsFromStorage()
    {
        Settings s = Settings.Default;
        _livePreviewEnabled = s.LivePreviewEnabled;
        _livePreviewMenuItem.Checked = _livePreviewEnabled;

        _editorTextBox.WordWrap = s.WordWrap;
        _wordWrapMenuItem.Checked = s.WordWrap;
        _editorTextBox.ScrollBars = _editorTextBox.WordWrap ? ScrollBars.Vertical : ScrollBars.Both;

        float fontSize = s.EditorFontSize;
        fontSize = Math.Max(8f, Math.Min(28f, fontSize));
        _editorTextBox.Font = new Font(_editorTextBox.Font.FontFamily, fontSize, _editorTextBox.Font.Style);
    }

    /// <summary>
    /// Restores the main form bounds and window state when the user has a saved layout.
    /// </summary>
    private void ApplyWindowSettingsFromStorage()
    {
        Settings s = Settings.Default;

        if (!s.HasSavedWindowLayout) return;

        StartPosition = FormStartPosition.Manual;
        Rectangle bounds = s.MainWindowBounds;

        if (bounds.Width < MinimumSize.Width) bounds.Width = MinimumSize.Width;

        if (bounds.Height < MinimumSize.Height) bounds.Height = MinimumSize.Height;

        if (IsRecoverableOnScreen(bounds))
            Bounds = bounds;
        else
        {
            Size = bounds.Size;
            CenterToScreen();
        }

        FormWindowState state = ToFormWindowState(s.MainWindowState);
        WindowState = state;
    }

    /// <summary>
    /// Persists editor, splitter, and window settings for the next session.
    /// </summary>
    private void SaveApplicationSettings()
    {
        Settings s = Settings.Default;
        s.LivePreviewEnabled = _livePreviewEnabled;
        s.WordWrap = _editorTextBox.WordWrap;
        s.EditorFontSize = _editorTextBox.Font.Size;
        s.SplitterDistance = _mainSplit.SplitterDistance;
        s.HasSavedWindowLayout = true;
        s.MainWindowState = (int)WindowState;

        s.MainWindowBounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;

        s.Save();
    }

    /// <summary>
    /// Converts a stored integer to <see cref="FormWindowState"/>, treating unknown values as normal.
    /// </summary>
    /// <param name="raw">The persisted enum underlying value.</param>
    /// <returns>A valid window state for startup (never minimized).</returns>
    private static FormWindowState ToFormWindowState(int raw)
    {
        if (raw is < 0 or > 2) return FormWindowState.Normal;

        FormWindowState state = (FormWindowState)raw;

        return state == FormWindowState.Minimized ? FormWindowState.Normal : state;
    }

    /// <summary>
    /// Returns whether at least part of the rectangle is usable on a monitor's working area.
    /// </summary>
    /// <param name="bounds">The proposed window bounds.</param>
    /// <returns><see langword="true"/> if the window can be placed here without being fully off-screen.</returns>
    private static bool IsRecoverableOnScreen(Rectangle bounds)
    {
        const int minVisible = 80;

        return Screen.AllScreens.Select(screen => Rectangle.Intersect(screen.WorkingArea, bounds))
            .Any(visible => visible is { Width: >= minVisible, Height: >= minVisible });
    }

    #endregion

    #region File menu and recent documents

    /// <summary>
    /// Populates the editor with a built-in sample document on startup.
    /// </summary>
    private void LoadSampleDocument()
    {
        _suppressDirtyFlag = true;

        _editorTextBox.Text = "# Welcome to Markup Editor\r\n\r\n" +
                              "This editor uses **CommonMark**-style Markdown with **GFM-style** extras " +
                              "(tables, task lists, strikethrough, autolinks, footnotes). Raw `<html>` in the " +
                              "source is shown as text in the preview.\r\n\r\n" +
                              "## Try it\r\n\r\n" +
                              "| Feature | Example |\r\n" +
                              "|---------|---------|\r\n" +
                              "| Task | - [ ] Todo |\r\n\r\n" +
                              "```\r\nfenced code block\r\n```\r\n\r\n" +
                              "> Block quote\r\n\r\n" +
                              "[Link](https://example.com)\r\n\r\n" +
                              "~~strikethrough~~\r\n\r\n" +
                              "Footnote[^1]\r\n\r\n" +
                              "[^1]: Footnote text.\r\n";

        _suppressDirtyFlag = false;
        _isDirty = false;
    }

    /// <summary>
    /// Creates a new empty document, prompting to save unsaved changes first.
    /// </summary>
    private void fileNew_Click(object sender, EventArgs args)
    {
        if (!PromptToSaveChanges()) return;

        _suppressDirtyFlag = true;
        _editorTextBox.Clear();
        _suppressDirtyFlag = false;

        _currentFilePath = null;
        _isDirty = false;
        DoRenderPreview();
        UpdateWindowState();
    }

    /// <summary>
    /// Opens a markup file from disk, prompting to save unsaved changes first.
    /// </summary>
    private void fileOpen_Click(object sender, EventArgs args)
    {
        if (!PromptToSaveChanges()) return;

        using OpenFileDialog dialog = new();

        dialog.Filter = @"Markup Files|*.md;*.mdc;*.markup|All Files|*.*";
        dialog.Title = @"Open Markup File";

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        TryOpenFileAtPath(dialog.FileName);
    }

    /// <summary>
    /// Rebuilds the Recent Documents submenu before it is shown.
    /// </summary>
    private void fileRecentDocuments_DropDownOpening(object sender, EventArgs e) => RefreshRecentDocumentsMenu();

    /// <summary>
    /// Opens a path chosen from the Recent Documents list.
    /// </summary>
    private void fileRecentDocumentsEntry_Click(object sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;

        if (item.Tag is not string path || string.IsNullOrWhiteSpace(path)) return;

        if (!PromptToSaveChanges()) return;

        TryOpenFileAtPath(path);
    }

    /// <summary>
    /// Populates the Recent Documents submenu from user settings.
    /// </summary>
    private void RefreshRecentDocumentsMenu()
    {
        fileRecentDocuments.DropDownItems.Clear();
        List<string> paths = ParseRecentDocumentsFromSettings();

        if (paths.Count == 0)
        {
            ToolStripMenuItem placeholder = new("(No recent documents)");
            placeholder.Enabled = false;
            fileRecentDocuments.DropDownItems.Add(placeholder);

            return;
        }

        foreach (string fullPath in paths)
        {
            ToolStripMenuItem entry = new(FormatRecentMenuCaption(fullPath));
            entry.Tag = fullPath;
            entry.ToolTipText = fullPath;
            entry.Enabled = File.Exists(fullPath);
            entry.Click += fileRecentDocumentsEntry_Click;
            fileRecentDocuments.DropDownItems.Add(entry);
        }
    }

    /// <summary>
    /// Returns stored recent document paths, most recent first.
    /// </summary>
    /// <returns>A mutable list of full paths.</returns>
    private static List<string> ParseRecentDocumentsFromSettings()
    {
        string raw = Settings.Default.RecentDocuments ?? string.Empty;
        List<string> list = new();

        if (string.IsNullOrWhiteSpace(raw)) return list;

        string[] parts = raw.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        list.AddRange(parts.Select(part => part.Trim()).Where(trimmed => trimmed.Length > 0));

        return list;
    }

    /// <summary>
    /// Persists the recent-document list as newline-separated full paths.
    /// </summary>
    private static string SerializeRecentDocumentsPaths(IReadOnlyList<string> paths) => string.Join("\n", paths);

    /// <summary>
    /// Inserts a path at the front of the recent list and trims it to the configured capacity.
    /// </summary>
    /// <param name="fullPath">The path that was opened successfully.</param>
    private void RememberRecentDocument(string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath)) return;

        string normalized;

        try
        {
            normalized = Path.GetFullPath(fullPath);
        }
        catch (Exception)
        {
            return;
        }

        List<string> paths = ParseRecentDocumentsFromSettings();

        for (int i = paths.Count - 1; i >= 0; i--)
        {
            if (string.Equals(paths[i], normalized, StringComparison.OrdinalIgnoreCase))
                paths.RemoveAt(i);
        }

        paths.Insert(0, normalized);
        while (paths.Count > RecentDocumentsCapacity) paths.RemoveAt(paths.Count - 1);

        Settings.Default.RecentDocuments = SerializeRecentDocumentsPaths(paths);
        Settings.Default.Save();
        RefreshRecentDocumentsMenu();
    }

    /// <summary>
    /// Removes a path from the recent list (for example when it no longer exists).
    /// </summary>
    /// <param name="filePath">The path to remove.</param>
    private void RemoveRecentDocumentFromSettings(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;

        string normalized;

        try
        {
            normalized = Path.GetFullPath(filePath);
        }
        catch (Exception)
        {
            return;
        }

        List<string> paths = ParseRecentDocumentsFromSettings();
        bool changed = false;

        for (int i = paths.Count - 1; i >= 0; i--)
        {
            if (!string.Equals(paths[i], normalized, StringComparison.OrdinalIgnoreCase)) continue;

            paths.RemoveAt(i);
            changed = true;
        }

        if (!changed) return;

        Settings.Default.RecentDocuments = SerializeRecentDocumentsPaths(paths);
        Settings.Default.Save();
        RefreshRecentDocumentsMenu();
    }

    /// <summary>
    /// Shortens a full path for display on a menu item.
    /// </summary>
    private static string FormatRecentMenuCaption(string fullPath)
    {
        const int maxDisplay = 72;

        if (fullPath.Length <= maxDisplay) return fullPath;

        return "..." + fullPath.Substring(fullPath.Length - (maxDisplay - 3));
    }

    /// <summary>
    /// Loads a markup file from disk into the editor and refreshes the preview.
    /// </summary>
    /// <param name="filePath">The path to the file to open.</param>
    /// <returns><see langword="true"/> if the file was loaded; otherwise <see langword="false"/>.</returns>
    private bool TryOpenFileAtPath(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath)) return false;

            string fullPath = Path.GetFullPath(filePath);

            if (!File.Exists(fullPath))
            {
                RemoveRecentDocumentFromSettings(fullPath);

                MessageBox.Show(this, $"The file was not found.\r\n\r\n{fullPath}", "Open Failed", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            string contents = File.ReadAllText(fullPath, Encoding.UTF8);
            _suppressDirtyFlag = true;
            _editorTextBox.Text = contents;
            _suppressDirtyFlag = false;

            _currentFilePath = fullPath;
            _isDirty = false;
            DoRenderPreview();
            UpdateWindowState();
            RememberRecentDocument(fullPath);

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not open file.\r\n\r\n{ex.Message}", "Open Failed", MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return false;
        }
    }

    /// <summary>
    /// Saves the document, prompting for a path if it has not been saved before.
    /// </summary>
    private void fileSave_Click(object sender, EventArgs args) => DoSaveDocument();

    /// <summary>
    /// Saves the document to a new path chosen by the user.
    /// </summary>
    private void fileSaveAs_Click(object sender, EventArgs args)
    {
        string originalPath = _currentFilePath;
        _currentFilePath = null;

        fileSave_Click(sender, args);

        if (!string.IsNullOrWhiteSpace(_currentFilePath)) return;

        _currentFilePath = originalPath;
    }

    /// <summary>
    /// Handles Exit; closes the form so <see cref="MarkupEditor_FormClosing"/> runs the save prompt and persists settings once.
    /// </summary>
    private void fileExit_Click(object sender, EventArgs args) => Close();

    #endregion

    #region Edit menu

    /// <summary>Cuts the selected text to the clipboard.</summary>
    private void editCut_Click(object sender, EventArgs args) => _editorTextBox.Cut();

    /// <summary>Copies the selected text to the clipboard.</summary>
    private void editCopy_Click(object sender, EventArgs args) => _editorTextBox.Copy();

    /// <summary>Pastes clipboard text at the current cursor position.</summary>
    private void editPaste_Click(object sender, EventArgs args) => _editorTextBox.Paste();

    /// <summary>Selects all text in the editor.</summary>
    private void editSelectAll_Click(object sender, EventArgs args) => _editorTextBox.SelectAll();

    /// <summary>
    /// Opens the find/replace dialog with focus on the Find field.
    /// </summary>
    private void editFind_Click(object sender, EventArgs args)
    {
        using FindReplaceDialog dialog = new FindReplaceDialog(_editorTextBox, initialFocusOnReplace: false);

        dialog.ShowDialog(this);
    }

    /// <summary>
    /// Opens the find/replace dialog with focus on the Replace field.
    /// </summary>
    private void editReplace_Click(object sender, EventArgs args)
    {
        using FindReplaceDialog dialog = new FindReplaceDialog(_editorTextBox, initialFocusOnReplace: true);

        dialog.ShowDialog(this);
    }

    /// <summary>Undoes the last edit operation if one is available.</summary>
    private void editUndo_Click(object sender, EventArgs args)
    {
        if (_editorTextBox.CanUndo) _editorTextBox.Undo();
    }

    /// <summary>Redoes the last undone edit via the Win32 EM_REDO message.</summary>
    private void editRedo_Click(object sender, EventArgs args) =>
        SendMessage(_editorTextBox.Handle, EmRedo, IntPtr.Zero, IntPtr.Zero);

    #endregion

    #region Tools menu and editor appearance

    /// <summary>Manually triggers a preview render from the Tools menu (Render Preview).</summary>
    private void toolsRender_Click(object sender, EventArgs args) => DoRenderPreview();

    /// <summary>
    /// Toggles automatic live preview on or off and updates the menu check state.
    /// </summary>
    private void _livePreviewMenuItem_Click(object sender, EventArgs args)
    {
        _livePreviewEnabled = !_livePreviewEnabled;
        _livePreviewMenuItem.Checked = _livePreviewEnabled;

        if (_livePreviewEnabled) DoRenderPreview();
        else _previewTimer.Stop();
    }

    /// <summary>
    /// Toggles word wrap in the editor and updates the menu check state.
    /// </summary>
    private void _wordWrapMenuItem_Click(object sender, EventArgs args)
    {
        _editorTextBox.WordWrap = !_editorTextBox.WordWrap;
        _wordWrapMenuItem.Checked = _editorTextBox.WordWrap;
        _editorTextBox.ScrollBars = _editorTextBox.WordWrap ? ScrollBars.Vertical : ScrollBars.Both;
    }

    /// <summary>
    /// Adjusts the editor font size by the given delta, clamped to a safe range.
    /// </summary>
    /// <param name="delta">Points to add to the current font size.</param>
    private void ChangeEditorFontSize(float delta)
    {
        float newSize = Math.Max(8f, Math.Min(28f, _editorTextBox.Font.Size + delta));
        _editorTextBox.Font = new Font(_editorTextBox.Font.FontFamily, newSize, _editorTextBox.Font.Style);
    }

    /// <summary>Resets the editor font size to the default.</summary>
    private void ResetEditorFontSize() =>
        _editorTextBox.Font =
            new Font(_editorTextBox.Font.FontFamily, DefaultEditorFontSize, _editorTextBox.Font.Style);

    /// <summary>Increases the editor font size by one point.</summary>
    private void fontIncrease_Click(object sender, EventArgs args) => ChangeEditorFontSize(1f);

    /// <summary>Decreases the editor font size by one point.</summary>
    private void fontDecrease_Click(object sender, EventArgs args) => ChangeEditorFontSize(-1f);

    /// <summary>Resets the editor font size to the default.</summary>
    private void fontReset_Click(object sender, EventArgs args) => ResetEditorFontSize();

    #endregion

    #region Help

    /// <summary>
    /// Displays a dialog listing the supported markup syntax.
    /// </summary>
    private void helpSyntax_Click(object sender, EventArgs args) =>
        MessageBox.Show(this,
            """
                Supported markup (CommonMark + GFM-style extensions via Markdig):

                Blocks: #–###### headings, > block quotes, -/*/+ lists, 1. ordered lists,
                --- (or ***) thematic breaks, ``` fenced code ```, tables (| pipes |),
                - [ ] / - [x] task lists, [^id] footnotes with [^id]: definitions.

                Inline: **bold**, *italic*, ~~strikethrough~~, `code`, [text](url),
                [ref][label] with [label]: url definitions, ![alt](url), images by reference.

                Autolinks: bare https:// URLs become links. Raw HTML in the source is
                escaped in the preview (not executed as HTML).

                See https://spec.commonmark.org/ and GitHub Flavored Markdown for full rules.
                """.Replace("\n", "\r\n"),
            "Supported Markup",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

    /// <summary>
    /// Displays the About dialog.
    /// </summary>
    private void helpAbout_Click(object sender, EventArgs args) =>
        MessageBox.Show(this,
            "MarkupEditor\r\n\r\nSimple markup editor with live preview.\r\n",
            "About MarkupEditor",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

    #endregion

    #region Document save, render, and UI state

    /// <summary>
    /// Prompts the user to save if there are unsaved changes.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the caller may proceed; <see langword="false"/> if the user cancelled or chose
    /// <b>Yes</b> but the document could not be saved.
    /// </returns>
    private bool PromptToSaveChanges()
    {
        if (!_isDirty) return true;

        DialogResult result = MessageBox.Show(this,
            "You have unsaved changes. Save before continuing?",
            "Unsaved Changes",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Warning);

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (result)
        {
            case DialogResult.Cancel:
                return false;

            case DialogResult.Yes:
                DoSaveDocument();

                return !_isDirty;

            case DialogResult.No:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Saves the document to its current path, or prompts for a path if the document is new.
    /// </summary>
    private void DoSaveDocument()
    {
        if (string.IsNullOrWhiteSpace(_currentFilePath))
        {
            using SaveFileDialog dialog = new();

            dialog.Filter = "Markdown|*.md|Text|*.txt|All Files|*.*";
            dialog.Title = "Save Markup File";
            dialog.FileName = "document.md";

            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            _currentFilePath = dialog.FileName;
        }

        try
        {
            File.WriteAllText(_currentFilePath, _editorTextBox.Text, Encoding.UTF8);
            _isDirty = false;
            UpdateWindowState();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not save file.\r\n\r\n{ex.Message}", "Save Failed", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Converts the editor text to HTML and displays it in the preview browser.
    /// </summary>
    private void DoRenderPreview()
    {
        const String style = "<style>body{font-family:Segoe UI,Tahoma,sans-serif;margin:18px;color:#222;line-height:1.5;}" +
                             "h1,h2,h3,h4,h5,h6{margin:0.8em 0 0.4em;}" +
                             "p{margin:0 0 0.8em;}" +
                             "ul,ol{margin-left: 25px;}" +
                             "li{margin-left: 25px;}" +
                             "code{font-family:Consolas,monospace;background:#f3f3f3;padding:2px 4px;border-radius:3px;}" +
                             "pre{font-family:Consolas,monospace;background:#f3f3f3;padding:10px;border-radius:4px;overflow-x:auto;}" +
                             "pre code{background:transparent;padding:0;}" +
                             "a{color:#0b63ce;}" +
                             "hr{border:none;border-top:1px solid #ccc;margin:1.2em 0;}" +
                             "blockquote{border-left:4px solid #ddd;margin:0 0 1em;padding-left:1em;color:#444;}" +
                             "table{border-collapse:collapse;margin:0 0 1em;}" +
                             "th,td{border:1px solid #ccc;padding:4px 8px;}" +
                             "th{background:#f5f5f5;}" +
                             "img{max-width:100%;height:auto;}" +
                             "del{text-decoration:line-through;}</style>";

        int scrollLine = GetEditorCaretLine();
        _pendingPreviewScrollLine = scrollLine;
        string body = style + MarkupParser.ConvertMarkupToHtml(_editorTextBox.Text);
        _previewBrowser.DocumentText = MarkupParser.BuildHtmlDocument(body);
        BeginInvokeWhenHandleReady(() => SyncPreviewToCaretLine(scrollLine));
    }

    /// <summary>
    /// Queues an action on the UI thread after the window handle exists (required before <see cref="Control.BeginInvoke"/> is valid).
    /// Multiple calls before the handle exists enqueue actions in order; each runs via <see cref="BeginInvoke"/>.
    /// </summary>
    /// <param name="action">The work to run asynchronously on the UI thread.</param>
    private void BeginInvokeWhenHandleReady(Action action)
    {
        if (IsDisposed) return;

        if (!IsHandleCreated)
        {
            _deferredUntilHandleCreated.Enqueue(action);
            HandleCreated -= MarkupEditor_HandleCreated;
            HandleCreated += MarkupEditor_HandleCreated;

            return;
        }

        BeginInvoke(action);
    }

    /// <summary>
    /// Runs all actions queued in <see cref="_deferredUntilHandleCreated"/> after the native handle is created.
    /// </summary>
    private void MarkupEditor_HandleCreated(object sender, EventArgs e)
    {
        HandleCreated -= MarkupEditor_HandleCreated;

        if (IsDisposed) return;

        while (_deferredUntilHandleCreated.Count > 0)
        {
            Action run = _deferredUntilHandleCreated.Dequeue();

            BeginInvoke(run);
        }
    }

    /// <summary>
    /// Updates the window title and status bar to reflect the current file and dirty state.
    /// </summary>
    private void UpdateWindowState()
    {
        string fileName = string.IsNullOrWhiteSpace(_currentFilePath)
            ? "Untitled.md"
            : Path.GetFileName(_currentFilePath);

        Text = fileName + (_isDirty ? " *" : string.Empty) + " - Markup Editor";
        _fileStatusLabel.Text = string.IsNullOrWhiteSpace(_currentFilePath) ? "Unsaved document" : _currentFilePath;
        _modifiedStatusLabel.Text = _isDirty ? "Modified" : "Saved";
    }

    #endregion
}