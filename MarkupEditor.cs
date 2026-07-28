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
using Microsoft.Web.WebView2.Core;

// ReSharper disable InvalidXmlDocComment

// ReSharper disable UnusedParameter.Local

namespace MarkupEditor;

internal sealed partial class MarkupEditor : Form
{
    #region Fields and construction

    private const int EmRedo = 0x0454;
    private const int RecentDocumentsCapacity = 10;
    private const int SplitterWidthAtDesignDpi = 5;
    private const int DesignDpi = 96;
    private const int SelfSaveWatcherSuppressMillisecondsDefault = 2000;
    private const int SelfSaveWatcherSuppressMillisecondsMin = 0;
    private const int SelfSaveWatcherSuppressMillisecondsMax = 30000;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    private string _currentFilePath;
    private bool _isDirty;
    private bool _suppressDirtyFlag;
    private bool _livePreviewEnabled;
    private bool _previewReady;
    private bool _allowRawHtml;
    private int _selfSaveWatcherSuppressMilliseconds = SelfSaveWatcherSuppressMillisecondsDefault;
    private int _pendingPreviewScrollLine = -1;
    private double _splitterDistanceRatio = 0.5d;
    private FileSystemWatcher _fileWatcher;
    private bool _externalChangePending;
    private DateTime _ignoreFileWatcherEventsUntilUtc = DateTime.MinValue;

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
        _previewBrowser.NavigationCompleted += _previewBrowser_NavigationCompleted;
        _previewBrowser.CoreWebView2InitializationCompleted += _previewBrowser_CoreWebView2InitializationCompleted;
        _previewTimer.Tick += _previewTimer_Tick;
        _mainSplit.SizeChanged += _mainSplit_SizeChanged;
        _mainSplit.SplitterMoved += _mainSplit_SplitterMoved;
        ClientSizeChanged += MarkupEditor_ClientSizeChanged;
        DpiChanged += MarkupEditor_DpiChanged;
        Shown += MarkupEditor_Shown;
        Load += MarkupEditor_Load;
        Activated += MarkupEditor_Activated;

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
    /// Handles the <see cref="Form.Shown"/> event; configures the split panel layout and starts WebView2 initialization.
    /// </summary>
    private void MarkupEditor_Shown(object sender, EventArgs e)
    {
        LayoutMainSplitToVisibleClientArea();
        ApplySplitterWidthForCurrentDpi();
        ConfigureSafePanelMinimums();
        int preferredDistance = GetPreferredSplitterDistanceForCurrentSize();
        SetSafeSplitterDistance(preferredDistance);
        UpdateSplitterRatioFromCurrentDistance();
        _ = _previewBrowser.EnsureCoreWebView2Async(null);
    }

    /// <summary>
    /// Applies the split orientation and resets the splitter to half the container size.
    /// </summary>
    /// <param name="horizontal"><see langword="true"/> for horizontal (top/bottom); <see langword="false"/> for vertical (left/right).</param>
    private void ApplySplitOrientation(bool horizontal)
    {
        _mainSplit.Orientation = horizontal ? Orientation.Horizontal : Orientation.Vertical;
        ConfigureSafePanelMinimums();
        int half = GetSplitAxisLength() / 2;
        SetSafeSplitterDistance(half);
        UpdateSplitterRatioFromCurrentDistance();
    }

    /// <summary>
    /// Calculates and applies safe minimum panel sizes for the split container.
    /// </summary>
    private void ConfigureSafePanelMinimums()
    {
        bool horizontal = _mainSplit.Orientation == Orientation.Horizontal;
        int available = Math.Max(0, (horizontal ? _mainSplit.Height : _mainSplit.Width) - 20);
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
        bool horizontal = _mainSplit.Orientation == Orientation.Horizontal;
        int min = _mainSplit.Panel1MinSize;
        int max = (horizontal ? _mainSplit.Height : _mainSplit.Width) - _mainSplit.Panel2MinSize - _mainSplit.SplitterWidth;

        if (max < min) return;

        int clamped = Math.Min(Math.Max(preferred, min), max);
        _mainSplit.SplitterDistance = clamped;
    }

    /// <summary>
    /// Handles split-container size changes; reapplies the tracked split ratio to keep panel proportions stable.
    /// </summary>
    private void _mainSplit_SizeChanged(object sender, EventArgs e)
    {
        if (!IsHandleCreated) return;

        ConfigureSafePanelMinimums();
        SetSafeSplitterDistance(GetPreferredDistanceFromRatio());
        UpdateSplitterRatioFromCurrentDistance();
    }

    /// <summary>
    /// Handles splitter drag completion; stores the current splitter ratio for future resizes.
    /// </summary>
    private void _mainSplit_SplitterMoved(object sender, SplitterEventArgs e) =>
        UpdateSplitterRatioFromCurrentDistance();

    /// <summary>
    /// Handles client-size changes; re-lays out the split container to fill the visible client area.
    /// </summary>
    private void MarkupEditor_ClientSizeChanged(object sender, EventArgs e) =>
        LayoutMainSplitToVisibleClientArea();

    /// <summary>
    /// Handles form DPI changes when moving across monitors; keeps splitter metrics stable.
    /// </summary>
    private void MarkupEditor_DpiChanged(object sender, DpiChangedEventArgs e)
    {
        LayoutMainSplitToVisibleClientArea();
        ApplySplitterWidthForDpi(e.DeviceDpiNew);
        ConfigureSafePanelMinimums();
        SetSafeSplitterDistance(GetPreferredDistanceFromRatio());
        UpdateSplitterRatioFromCurrentDistance();
    }

    /// <summary>
    /// Sets split-container bounds so it fills exactly the visible area between menu and status bars.
    /// This avoids stale anchored bounds after monitor DPI/zoom transitions.
    /// </summary>
    private void LayoutMainSplitToVisibleClientArea()
    {
        int top = _menuStrip.Bottom;
        int bottom = _statusStrip.Top;
        int width = ClientSize.Width;
        int height = Math.Max(0, bottom - top);

        _mainSplit.Bounds = new Rectangle(0, top, width, height);
    }

    /// <summary>
    /// Applies a deterministic splitter width for the current form DPI.
    /// </summary>
    private void ApplySplitterWidthForCurrentDpi() => ApplySplitterWidthForDpi(DeviceDpi);

    /// <summary>
    /// Applies a deterministic splitter width for a target DPI.
    /// </summary>
    /// <param name="dpi">The target monitor DPI.</param>
    private void ApplySplitterWidthForDpi(int dpi)
    {
        int scaled = (int)Math.Round((double)SplitterWidthAtDesignDpi * dpi / DesignDpi);
        _mainSplit.SplitterWidth = Math.Max(3, scaled);
    }

    /// <summary>
    /// Returns the split axis length in pixels for the current orientation.
    /// </summary>
    /// <returns>Width for vertical split or height for horizontal split.</returns>
    private int GetSplitAxisLength() =>
        _mainSplit.Orientation == Orientation.Horizontal ? _mainSplit.Height : _mainSplit.Width;

    /// <summary>
    /// Updates the in-memory splitter ratio based on the current splitter distance and axis length.
    /// </summary>
    private void UpdateSplitterRatioFromCurrentDistance()
    {
        int axis = GetSplitAxisLength();

        if (axis <= 0) return;

        double ratio = (double)_mainSplit.SplitterDistance / axis;
        _splitterDistanceRatio = Math.Max(0d, Math.Min(1d, ratio));
    }

    /// <summary>
    /// Computes a preferred splitter distance for the current size from the tracked split ratio.
    /// </summary>
    /// <returns>A preferred splitter distance in pixels.</returns>
    private int GetPreferredDistanceFromRatio() =>
        (int)Math.Round(GetSplitAxisLength() * _splitterDistanceRatio);

    /// <summary>
    /// Computes the startup splitter distance for the current window size.
    /// If saved window bounds are available, scales the persisted splitter by the saved axis.
    /// </summary>
    /// <returns>A preferred splitter distance in pixels for startup.</returns>
    private int GetPreferredSplitterDistanceForCurrentSize()
    {
        Settings s = Settings.Default;
        int axis = GetSplitAxisLength();

        if (axis <= 0) return s.SplitterDistance;

        if (!s.HasSavedWindowLayout) return s.SplitterDistance;

        int savedAxis = _mainSplit.Orientation == Orientation.Horizontal
            ? s.MainWindowBounds.Height
            : s.MainWindowBounds.Width;

        if (savedAxis <= 0) return s.SplitterDistance;

        double ratio = (double)s.SplitterDistance / savedAxis;
        _splitterDistanceRatio = Math.Max(0d, Math.Min(1d, ratio));

        return GetPreferredDistanceFromRatio();
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
    /// Handles WebView2 navigation completion; applies any pending scroll-to-line request.
    /// </summary>
    private void _previewBrowser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (_pendingPreviewScrollLine < 0) return;

        int line = _pendingPreviewScrollLine;
        _pendingPreviewScrollLine = -1;
        BeginInvokeWhenHandleReady(() => SyncPreviewToCaretLine(line));
    }

    /// <summary>
    /// Handles WebView2 initialization completion; enables preview rendering once the embedded browser is ready.
    /// </summary>
    private void _previewBrowser_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        if (!e.IsSuccess) return;

        _previewReady = true;
        DoRenderPreview();
    }

    /// <summary>
    /// Returns the zero-based logical line index of the caret (newline-delimited), matching <see cref="MarkupParser"/> line numbering.
    /// </summary>
    /// <returns>The current caret line.</returns>
    private int GetEditorCaretLine()
    {
        string text = _editorTextBox.Text;
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
    /// Both scroll and highlight are handled by the injected <c>MarkupSetActiveLine</c> JavaScript function.
    /// </summary>
    /// <param name="editorLineIndex">Zero-based source line index.</param>
    private void SyncPreviewToCaretLine(int editorLineIndex) =>
        TryInvokePreviewActiveLineHighlight(editorLineIndex);

    /// <summary>
    /// Calls the preview script to scroll to and highlight the block for the given source line.
    /// </summary>
    /// <param name="editorLineIndex">Zero-based source line index.</param>
    private void TryInvokePreviewActiveLineHighlight(int editorLineIndex)
    {
        if (!_previewReady || editorLineIndex < 0) return;

        try
        {
            _ = _previewBrowser.ExecuteScriptAsync($"MarkupSetActiveLine({editorLineIndex})");
        }
        catch (InvalidOperationException)
        {
            // WebView2 not ready for scripting.
        }
    }

    #endregion

    #region File watching

    /// <summary>
    /// Starts monitoring <paramref name="filePath"/> for external modifications.
    /// Any previous watcher is stopped first.
    /// </summary>
    /// <param name="filePath">Absolute path of the file to watch.</param>
    private void StartWatchingFile(string filePath)
    {
        StopWatchingFile();

        if (string.IsNullOrWhiteSpace(filePath)) return;

        string directory = Path.GetDirectoryName(filePath);
        string fileName = Path.GetFileName(filePath);

        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName)) return;

        _externalChangePending = false;

        _fileWatcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
            EnableRaisingEvents = true
        };

        _fileWatcher.Changed += _fileWatcher_ExternalChange;
        _fileWatcher.Deleted += _fileWatcher_ExternalChange;
        _fileWatcher.Renamed += _fileWatcher_ExternalChange;
    }

    /// <summary>
    /// Stops and disposes the current file watcher, if any.
    /// </summary>
    private void StopWatchingFile()
    {
        if (_fileWatcher == null) return;

        _fileWatcher.EnableRaisingEvents = false;
        _fileWatcher.Changed -= _fileWatcher_ExternalChange;
        _fileWatcher.Deleted -= _fileWatcher_ExternalChange;
        _fileWatcher.Renamed -= _fileWatcher_ExternalChange;
        _fileWatcher.Dispose();
        _fileWatcher = null;
    }

    /// <summary>
    /// Handles file-system change events raised by the watcher on a background thread;
    /// marshals a single alert to the UI thread, but only shows it when the form is focused.
    /// </summary>
    private void _fileWatcher_ExternalChange(object sender, FileSystemEventArgs e)
    {
        if (DateTime.UtcNow < _ignoreFileWatcherEventsUntilUtc) return;

        if (_externalChangePending) return;

        _externalChangePending = true;

        BeginInvokeWhenHandleReady(() =>
        {
            if (DateTime.UtcNow < _ignoreFileWatcherEventsUntilUtc)
            {
                _externalChangePending = false;

                return;
            }

            if (ContainsFocus)
                OnFileChangedExternally();

            // else: _externalChangePending stays true; MarkupEditor_Activated will fire OnFileChangedExternally.
        });
    }

    /// <summary>
    /// Handles the <see cref="Form.Activated"/> event; shows any deferred external-change alert.
    /// </summary>
    private void MarkupEditor_Activated(object sender, EventArgs e)
    {
        if (_externalChangePending)
            OnFileChangedExternally();
    }

    /// <summary>
    /// Runs on the UI thread; notifies the user that the file was changed externally
    /// and offers to reload it.
    /// </summary>
    private void OnFileChangedExternally()
    {
        _externalChangePending = false;

        bool fileExists = !string.IsNullOrWhiteSpace(_currentFilePath) && File.Exists(_currentFilePath);

        string message = fileExists
            ? $"The file has been modified by another process.\r\n\r\n{_currentFilePath}\r\n\r\nDo you want to reload it?"
            : $"The file has been moved or deleted by another process.\r\n\r\n{_currentFilePath}";

        if (fileExists)
        {
            DialogResult result = MessageBox.Show(this, message, "File Changed Externally",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
                TryOpenFileAtPath(_currentFilePath);
        }
        else
        {
            StopWatchingFile();

            MessageBox.Show(this, message, "File Changed Externally",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
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

        StopWatchingFile();
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

        _editorTextBox.WordWrap = s.WordWrap;
        _editorTextBox.ScrollBars = _editorTextBox.WordWrap ? ScrollBars.Vertical : ScrollBars.Both;

        float fontSize = s.EditorFontSize;
        fontSize = Math.Max(8f, Math.Min(28f, fontSize));
        _editorTextBox.Font = new Font(_editorTextBox.Font.FontFamily, fontSize, _editorTextBox.Font.Style);

        bool horizontalSplit = s.HorizontalSplit;
        _mainSplit.Orientation = horizontalSplit ? Orientation.Horizontal : Orientation.Vertical;

        _lineNumberPanel.Visible = s.ShowLineNumbers;

        _allowRawHtml = s.AllowRawHtml;
        _selfSaveWatcherSuppressMilliseconds = ClampSelfSaveWatcherSuppressMilliseconds(s.SelfSaveWatcherSuppressMilliseconds);
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
        s.HorizontalSplit = _mainSplit.Orientation == Orientation.Horizontal;
        s.ShowLineNumbers = _lineNumberPanel.Visible;
        s.AllowRawHtml = _allowRawHtml;
        s.SelfSaveWatcherSuppressMilliseconds =
            ClampSelfSaveWatcherSuppressMilliseconds(_selfSaveWatcherSuppressMilliseconds);
        s.HasSavedWindowLayout = true;
        s.MainWindowState = (int)WindowState;

        s.MainWindowBounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
        s.SplitterDistance = GetSplitterDistanceForPersistedWindowBounds(s.MainWindowBounds);

        s.Save();
    }

    /// <summary>
    /// Computes the splitter distance to persist in the same coordinate basis as persisted window bounds.
    /// This keeps startup restoration stable when the form is closed while maximized.
    /// </summary>
    /// <param name="persistedBounds">The window bounds that will be written to settings.</param>
    /// <returns>A splitter distance aligned to <paramref name="persistedBounds"/>.</returns>
    private int GetSplitterDistanceForPersistedWindowBounds(Rectangle persistedBounds)
    {
        int persistedAxis = _mainSplit.Orientation == Orientation.Horizontal
            ? persistedBounds.Height
            : persistedBounds.Width;

        if (persistedAxis <= 0) return _mainSplit.SplitterDistance;

        int currentAxis = GetSplitAxisLength();

        if (currentAxis <= 0) return _mainSplit.SplitterDistance;

        double ratio = (double)_mainSplit.SplitterDistance / currentAxis;
        ratio = Math.Max(0d, Math.Min(1d, ratio));

        return (int)Math.Round(persistedAxis * ratio);
    }

    /// <summary>
    /// Clamps the self-save watcher suppression window to a safe range.
    /// </summary>
    /// <param name="milliseconds">Requested suppression duration in milliseconds.</param>
    /// <returns>A bounded suppression duration.</returns>
    private static int ClampSelfSaveWatcherSuppressMilliseconds(int milliseconds) =>
        Math.Max(SelfSaveWatcherSuppressMillisecondsMin,
            Math.Min(SelfSaveWatcherSuppressMillisecondsMax, milliseconds));

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

        StopWatchingFile();
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
    /// Reloads the current file from disk, warning before discarding unsaved editor changes.
    /// </summary>
    private void fileReload_Click(object sender, EventArgs args)
    {
        if (string.IsNullOrWhiteSpace(_currentFilePath))
        {
            MessageBox.Show(this,
                "The current document has not been saved yet, so there is no file to reload.",
                "Reload",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        if (!PromptToDiscardChangesForReload()) return;

        TryOpenFileAtPath(_currentFilePath);
    }

    /// <summary>
    /// Warns when reloading would discard unsaved changes in the current editor buffer.
    /// </summary>
    /// <returns><see langword="true"/> when reload can continue; otherwise <see langword="false"/>.</returns>
    private bool PromptToDiscardChangesForReload()
    {
        if (!_isDirty) return true;

        DialogResult result = MessageBox.Show(this,
            "Reloading will discard unsaved changes in this document. Continue?",
            "Unsaved Changes",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        return result == DialogResult.Yes;
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

        return "..." + fullPath[^(maxDisplay - 3)..];
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
            StartWatchingFile(fullPath);

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
        using FindReplaceDialog dialog = new(_editorTextBox, initialFocusOnReplace: false);

        dialog.ShowDialog(this);
    }

    /// <summary>
    /// Opens the find/replace dialog with focus on the Replace field.
    /// </summary>
    private void editReplace_Click(object sender, EventArgs args)
    {
        using FindReplaceDialog dialog = new(_editorTextBox, initialFocusOnReplace: true);

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
    /// Opens the Settings dialog; applies accepted changes immediately.
    /// </summary>
    private void toolsSettings_Click(object sender, EventArgs args)
    {
        using SettingsDialog dialog = new(
            _livePreviewEnabled,
            _editorTextBox.WordWrap,
            _mainSplit.Orientation == Orientation.Horizontal,
            _lineNumberPanel.Visible,
            _allowRawHtml,
            _editorTextBox.Font.Size);

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        ApplySettingsFromDialog(dialog);
    }

    /// <summary>
    /// Applies all values returned by the Settings dialog to the application state.
    /// </summary>
    /// <param name="dialog">The closed dialog whose property values are authoritative.</param>
    private void ApplySettingsFromDialog(SettingsDialog dialog)
    {
        bool livePreviewChanged = _livePreviewEnabled != dialog.LivePreview;
        _livePreviewEnabled = dialog.LivePreview;

        if (livePreviewChanged)
        {
            if (_livePreviewEnabled) DoRenderPreview();
            else _previewTimer.Stop();
        }

        if (_editorTextBox.WordWrap != dialog.WordWrap)
        {
            _editorTextBox.WordWrap = dialog.WordWrap;
            _editorTextBox.ScrollBars = _editorTextBox.WordWrap ? ScrollBars.Vertical : ScrollBars.Both;
        }

        bool newHorizontal = dialog.HorizontalSplit;

        if (_mainSplit.Orientation == Orientation.Horizontal != newHorizontal)
            ApplySplitOrientation(newHorizontal);

        _lineNumberPanel.Visible = dialog.ShowLineNumbers;

        bool allowRawHtmlChanged = _allowRawHtml != dialog.AllowRawHtml;
        _allowRawHtml = dialog.AllowRawHtml;

        float newFontSize = dialog.FontSize;

        if (Math.Abs(newFontSize - _editorTextBox.Font.Size) > 0.01f)
            _editorTextBox.Font = new Font(_editorTextBox.Font.FontFamily, newFontSize, _editorTextBox.Font.Style);

        if (allowRawHtmlChanged) DoRenderPreview();
    }

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

                Autolinks: bare https:// URLs become links. By default, raw HTML in
                the source is escaped in the preview; enable "Allow Raw HTML" in
                Settings to render it.

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
        string previousPath = _currentFilePath;

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
            string fullPath = Path.GetFullPath(_currentFilePath);
            StopWatchingFile();
            _externalChangePending = false;
            _ignoreFileWatcherEventsUntilUtc = DateTime.UtcNow +
                                               TimeSpan.FromMilliseconds(_selfSaveWatcherSuppressMilliseconds);
            File.WriteAllText(fullPath, _editorTextBox.Text, Encoding.UTF8);
            _currentFilePath = fullPath;
            _isDirty = false;
            RememberRecentDocument(fullPath);
            StartWatchingFile(fullPath);
            UpdateWindowState();
        }
        catch (Exception ex)
        {
            _currentFilePath = previousPath;

            if (!string.IsNullOrWhiteSpace(previousPath))
                StartWatchingFile(previousPath);

            MessageBox.Show(this, $"Could not save file.\r\n\r\n{ex.Message}", "Save Failed", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Exports the current document as a standalone HTML file.
    /// </summary>
    private void fileExportHtml_Click(object sender, EventArgs args)
    {
        using SaveFileDialog dialog = new();

        dialog.Filter = "HTML Files|*.html;*.htm|All Files|*.*";
        dialog.Title = "Export as HTML";
        string baseName = Path.GetFileNameWithoutExtension(_currentFilePath ?? "document");
        dialog.FileName = baseName + ".html";

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        string html = MarkupParser.BuildHtmlDocument(MarkupParser.ConvertMarkupToHtml(_editorTextBox.Text, _allowRawHtml));

        try
        {
            File.WriteAllText(dialog.FileName, html, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not export file.\r\n\r\n{ex.Message}", "Export Failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Converts the editor text to HTML and displays it in the preview browser.
    /// Does nothing if the WebView2 browser is not yet initialized.
    /// </summary>
    private void DoRenderPreview()
    {
        if (!_previewReady) return;

        int scrollLine = GetEditorCaretLine();
        _pendingPreviewScrollLine = scrollLine;
        string html = MarkupParser.BuildHtmlDocument(MarkupParser.ConvertMarkupToHtml(_editorTextBox.Text, _allowRawHtml));
        _previewBrowser.NavigateToString(html);
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

        fileReload.Enabled = !string.IsNullOrWhiteSpace(_currentFilePath);
        Text = fileName + (_isDirty ? " *" : string.Empty) + " - Markup Editor";
        _fileStatusLabel.Text = string.IsNullOrWhiteSpace(_currentFilePath) ? "Unsaved document" : _currentFilePath;
        _modifiedStatusLabel.Text = _isDirty ? "Modified" : "Saved";
    }

    #endregion
}