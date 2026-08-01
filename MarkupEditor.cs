using MarkupEditor.Properties;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
// ReSharper disable once RedundantUsingDirective
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// ReSharper disable InvalidXmlDocComment

// ReSharper disable UnusedParameter.Local

namespace MarkupEditor;

internal sealed partial class MarkupEditor : Form
{
    #region Fields and construction

    private const Int32 EmRedo = 0x0454;
    private const Int32 RecentDocumentsCapacity = 10;
    private const Int32 SplitterWidthAtDesignDpi = 5;
    private const Int32 DesignDpi = 96;
    private const Int32 SelfSaveWatcherSuppressMillisecondsDefault = 2000;
    private const Int32 SelfSaveWatcherSuppressMillisecondsMin = 0;
    private const Int32 SelfSaveWatcherSuppressMillisecondsMax = 30000;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, Int32 msg, IntPtr wParam, IntPtr lParam);

    private String _currentFilePath;
    private Boolean _isDirty;
    private Boolean _suppressDirtyFlag;
    private Boolean _livePreviewEnabled;
    private Boolean _previewReady;
    private Boolean _allowRawHtml;
    private Int32 _selfSaveWatcherSuppressMilliseconds = SelfSaveWatcherSuppressMillisecondsDefault;
    private Int32 _pendingPreviewScrollLine = -1;
    private Boolean _pendingPrintAfterRender;
    private String _pendingPdfExportPath;
    private Double _splitterDistanceRatio = 0.5d;
    private FileSystemWatcher _fileWatcher;
    private Boolean _externalChangePending;
    private DateTime _ignoreFileWatcherEventsUntilUtc = DateTime.MinValue;

    /// <summary>
    /// Actions to run on the UI thread after <see cref="Control.Handle"/> exists, queued from <see cref="BeginInvokeWhenHandleReady"/>.
    /// </summary>
    private readonly Queue<Action> _deferredUntilHandleCreated = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkupEditor"/> form.
    /// </summary>
    /// <param name="commandLineFilePath">Optional path from the command line to open on startup.</param>
    public MarkupEditor(String commandLineFilePath = null)
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

        if (!String.IsNullOrWhiteSpace(commandLineFilePath))
        {
            String trimmedPath = commandLineFilePath.Trim().Trim('"');
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
            String path = Application.ExecutablePath;

            if (String.IsNullOrEmpty(path)) return;

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
    private void MarkupEditor_Load(Object sender, EventArgs e) => ApplyWindowSettingsFromStorage();

    /// <summary>
    /// Handles the <see cref="Form.Shown"/> event; configures the split panel layout and starts WebView2 initialization.
    /// </summary>
    private void MarkupEditor_Shown(Object sender, EventArgs e)
    {
        LayoutMainSplitToVisibleClientArea();
        ApplySplitterWidthForCurrentDpi();
        ConfigureSafePanelMinimums();
        Int32 preferredDistance = GetPreferredSplitterDistanceForCurrentSize();
        SetSafeSplitterDistance(preferredDistance);
        UpdateSplitterRatioFromCurrentDistance();
        _ = _previewBrowser.EnsureCoreWebView2Async(null);
    }

    /// <summary>
    /// Applies the split orientation and resets the splitter to half the container size.
    /// </summary>
    /// <param name="horizontal"><see langword="true"/> for horizontal (top/bottom); <see langword="false"/> for vertical (left/right).</param>
    private void ApplySplitOrientation(Boolean horizontal)
    {
        _mainSplit.Orientation = horizontal ? Orientation.Horizontal : Orientation.Vertical;
        ConfigureSafePanelMinimums();
        Int32 half = GetSplitAxisLength() / 2;
        SetSafeSplitterDistance(half);
        UpdateSplitterRatioFromCurrentDistance();
    }

    /// <summary>
    /// Calculates and applies safe minimum panel sizes for the split container.
    /// </summary>
    private void ConfigureSafePanelMinimums()
    {
        Boolean horizontal = _mainSplit.Orientation == Orientation.Horizontal;
        Int32 available = Math.Max(0, (horizontal ? _mainSplit.Height : _mainSplit.Width) - 20);
        const Int32 desired = 250;
        Int32 capped = available / 2;
        Int32 safeMin = Math.Max(50, Math.Min(desired, capped));

        _mainSplit.Panel1MinSize = safeMin;
        _mainSplit.Panel2MinSize = safeMin;
    }

    /// <summary>
    /// Sets the splitter position clamped within safe panel bounds.
    /// </summary>
    /// <param name="preferred">The desired splitter distance in pixels.</param>
    private void SetSafeSplitterDistance(Int32 preferred)
    {
        Boolean horizontal = _mainSplit.Orientation == Orientation.Horizontal;
        Int32 min = _mainSplit.Panel1MinSize;

        Int32 max =
            (horizontal ? _mainSplit.Height : _mainSplit.Width) - _mainSplit.Panel2MinSize - _mainSplit.SplitterWidth;

        if (max < min) return;

        Int32 clamped = Math.Min(Math.Max(preferred, min), max);
        _mainSplit.SplitterDistance = clamped;
    }

    /// <summary>
    /// Handles split-container size changes; reapplies the tracked split ratio to keep panel proportions stable.
    /// </summary>
    private void _mainSplit_SizeChanged(Object sender, EventArgs e)
    {
        if (!IsHandleCreated) return;

        ConfigureSafePanelMinimums();
        SetSafeSplitterDistance(GetPreferredDistanceFromRatio());
        UpdateSplitterRatioFromCurrentDistance();
    }

    /// <summary>
    /// Handles splitter drag completion; stores the current splitter ratio for future resizes.
    /// </summary>
    private void _mainSplit_SplitterMoved(Object sender, SplitterEventArgs e) =>
        UpdateSplitterRatioFromCurrentDistance();

    /// <summary>
    /// Handles client-size changes; re-lays out the split container to fill the visible client area.
    /// </summary>
    private void MarkupEditor_ClientSizeChanged(Object sender, EventArgs e) =>
        LayoutMainSplitToVisibleClientArea();

    /// <summary>
    /// Handles form DPI changes when moving across monitors; keeps splitter metrics stable.
    /// </summary>
    private void MarkupEditor_DpiChanged(Object sender, DpiChangedEventArgs e)
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
        Int32 top = _menuStrip.Bottom;
        Int32 bottom = _statusStrip.Top;
        Int32 width = ClientSize.Width;
        Int32 height = Math.Max(0, bottom - top);

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
    private void ApplySplitterWidthForDpi(Int32 dpi)
    {
        Int32 scaled = (Int32)Math.Round((Double)SplitterWidthAtDesignDpi * dpi / DesignDpi);
        _mainSplit.SplitterWidth = Math.Max(3, scaled);
    }

    /// <summary>
    /// Returns the split axis length in pixels for the current orientation.
    /// </summary>
    /// <returns>Width for vertical split or height for horizontal split.</returns>
    private Int32 GetSplitAxisLength() =>
        _mainSplit.Orientation == Orientation.Horizontal ? _mainSplit.Height : _mainSplit.Width;

    /// <summary>
    /// Updates the in-memory splitter ratio based on the current splitter distance and axis length.
    /// </summary>
    private void UpdateSplitterRatioFromCurrentDistance()
    {
        Int32 axis = GetSplitAxisLength();

        if (axis <= 0) return;

        Double ratio = (Double)_mainSplit.SplitterDistance / axis;
        _splitterDistanceRatio = Math.Max(0d, Math.Min(1d, ratio));
    }

    /// <summary>
    /// Computes a preferred splitter distance for the current size from the tracked split ratio.
    /// </summary>
    /// <returns>A preferred splitter distance in pixels.</returns>
    private Int32 GetPreferredDistanceFromRatio() =>
        (Int32)Math.Round(GetSplitAxisLength() * _splitterDistanceRatio);

    /// <summary>
    /// Computes the startup splitter distance for the current window size.
    /// If saved window bounds are available, scales the persisted splitter by the saved axis.
    /// </summary>
    /// <returns>A preferred splitter distance in pixels for startup.</returns>
    private Int32 GetPreferredSplitterDistanceForCurrentSize()
    {
        Settings s = Settings.Default;
        Int32 axis = GetSplitAxisLength();

        if (axis <= 0 || !s.HasSavedWindowLayout) return s.SplitterDistance;

        Int32 savedAxis =
            _mainSplit.Orientation == Orientation.Horizontal
                ? s.MainWindowBounds.Height
                : s.MainWindowBounds.Width;

        if (savedAxis <= 0) return s.SplitterDistance;

        Double ratio = (Double)s.SplitterDistance / savedAxis;
        _splitterDistanceRatio = Math.Max(0d, Math.Min(1d, ratio));

        return GetPreferredDistanceFromRatio();
    }

    #endregion

    #region Editor, debounce timer, and preview sync

    /// <summary>
    /// Handles text changes in the editor; marks the document dirty and restarts the live-preview debounce timer.
    /// </summary>
    private void _editorTextBox_TextChanged(Object sender, EventArgs e)
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
    private void _previewTimer_Tick(Object sender, EventArgs e)
    {
        _previewTimer.Stop();
        DoRenderPreview();
    }

    /// <summary>
    /// Handles key up in the editor; syncs preview scroll and active line to the caret.
    /// </summary>
    private void _editorTextBox_KeyUp(Object sender, KeyEventArgs e) =>
        SyncPreviewToCaretLine(GetEditorCaretLine());

    /// <summary>
    /// Handles mouse up in the editor; syncs preview when the caret moves via clicking or selection.
    /// </summary>
    private void _editorTextBox_MouseUp(Object sender, MouseEventArgs e) =>
        SyncPreviewToCaretLine(GetEditorCaretLine());

    /// <summary>
    /// Handles WebView2 navigation completion; applies any pending scroll-to-line request.
    /// </summary>
    private void _previewBrowser_NavigationCompleted(Object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (_pendingPreviewScrollLine >= 0)
        {
            Int32 line = _pendingPreviewScrollLine;
            _pendingPreviewScrollLine = -1;
            BeginInvokeWhenHandleReady(() => SyncPreviewToCaretLine(line));
        }

        TryPrintPreviewIfPending();
        _ = TryExportPdfIfPendingAsync();
    }

    /// <summary>
    /// Handles WebView2 initialization completion; enables preview rendering once the embedded browser is ready.
    /// </summary>
    private void _previewBrowser_CoreWebView2InitializationCompleted(Object sender,
        CoreWebView2InitializationCompletedEventArgs e)
    {
        if (!e.IsSuccess) return;

        _previewReady = true;
        DoRenderPreview();
    }

    /// <summary>
    /// Returns the zero-based logical line index of the caret (newline-delimited), matching <see cref="MarkupParser"/> line numbering.
    /// </summary>
    /// <returns>The current caret line.</returns>
    private Int32 GetEditorCaretLine()
    {
        String text = _editorTextBox.Text;
        Int32 caret = Math.Min(Math.Max(0, _editorTextBox.SelectionStart), text.Length);
        Int32 line = 0;

        for (Int32 i = 0; i < caret; i++)
        {
            Char c = text[i];

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
    private void SyncPreviewToCaretLine(Int32 editorLineIndex) =>
        TryInvokePreviewActiveLineHighlight(editorLineIndex);

    /// <summary>
    /// Calls the preview script to scroll to and highlight the block for the given source line.
    /// </summary>
    /// <param name="editorLineIndex">Zero-based source line index.</param>
    private void TryInvokePreviewActiveLineHighlight(Int32 editorLineIndex)
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
    private void StartWatchingFile(String filePath)
    {
        StopWatchingFile();

        if (String.IsNullOrWhiteSpace(filePath)) return;

        String directory = Path.GetDirectoryName(filePath);
        String fileName = Path.GetFileName(filePath);

        if (String.IsNullOrEmpty(directory) || String.IsNullOrEmpty(fileName)) return;

        _externalChangePending = false;

        _fileWatcher =
            new FileSystemWatcher(directory, fileName)
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
    private void _fileWatcher_ExternalChange(Object sender, FileSystemEventArgs e)
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
    private void MarkupEditor_Activated(Object sender, EventArgs e)
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

        Boolean fileExists = !String.IsNullOrWhiteSpace(_currentFilePath) && File.Exists(_currentFilePath);

        String message =
            fileExists
                ? $@"The file has been modified by another process.\r\n\r\n{_currentFilePath}\r\n\r\nDo you want to reload it?"
                : $@"The file has been moved or deleted by another process.\r\n\r\n{_currentFilePath}";

        if (fileExists)
        {
            DialogResult result =
                MessageBox.Show(this, message, @"File Changed Externally",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
                TryOpenFileAtPath(_currentFilePath);
        }
        else
        {
            StopWatchingFile();

            MessageBox.Show(this, message, @"File Changed Externally",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    #endregion

    #region Form closing

    /// <summary>
    /// Handles the <see cref="Form.FormClosing"/> event; prompts to save unsaved changes, then persists user settings.
    /// </summary>
    private void MarkupEditor_FormClosing(Object sender, FormClosingEventArgs e)
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

        Single fontSize = s.EditorFontSize;
        fontSize = Math.Max(8f, Math.Min(28f, fontSize));
        _editorTextBox.Font = new Font(_editorTextBox.Font.FontFamily, fontSize, _editorTextBox.Font.Style);

        Boolean horizontalSplit = s.HorizontalSplit;
        _mainSplit.Orientation = horizontalSplit ? Orientation.Horizontal : Orientation.Vertical;

        _lineNumberPanel.Visible = s.ShowLineNumbers;

        _allowRawHtml = s.AllowRawHtml;

        _selfSaveWatcherSuppressMilliseconds =
            ClampSelfSaveWatcherSuppressMilliseconds(s.SelfSaveWatcherSuppressMilliseconds);
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
        s.MainWindowState = (Int32)WindowState;

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
    private Int32 GetSplitterDistanceForPersistedWindowBounds(Rectangle persistedBounds)
    {
        Int32 persistedAxis =
            _mainSplit.Orientation == Orientation.Horizontal
                ? persistedBounds.Height
                : persistedBounds.Width;

        if (persistedAxis <= 0) return _mainSplit.SplitterDistance;

        Int32 currentAxis = GetSplitAxisLength();

        if (currentAxis <= 0) return _mainSplit.SplitterDistance;

        Double ratio = (Double)_mainSplit.SplitterDistance / currentAxis;
        ratio = Math.Max(0d, Math.Min(1d, ratio));

        return (Int32)Math.Round(persistedAxis * ratio);
    }

    /// <summary>
    /// Clamps the self-save watcher suppression window to a safe range.
    /// </summary>
    /// <param name="milliseconds">Requested suppression duration in milliseconds.</param>
    /// <returns>A bounded suppression duration.</returns>
    private static Int32 ClampSelfSaveWatcherSuppressMilliseconds(Int32 milliseconds) =>
        Math.Max(SelfSaveWatcherSuppressMillisecondsMin,
            Math.Min(SelfSaveWatcherSuppressMillisecondsMax, milliseconds));

    /// <summary>
    /// Converts a stored integer to <see cref="FormWindowState"/>, treating unknown values as normal.
    /// </summary>
    /// <param name="raw">The persisted enum underlying value.</param>
    /// <returns>A valid window state for startup (never minimized).</returns>
    private static FormWindowState ToFormWindowState(Int32 raw)
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
    private static Boolean IsRecoverableOnScreen(Rectangle bounds)
    {
        const Int32 minVisible = 80;

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

        _editorTextBox.Text =
            // ReSharper disable LocalizableElement
            "# Welcome to Markup Editor\r\n\r\n" +
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
        // ReSharper restore LocalizableElement

        _suppressDirtyFlag = false;
        _isDirty = false;
    }

    /// <summary>
    /// Creates a new empty document, prompting to save unsaved changes first.
    /// </summary>
    private void fileNew_Click(Object sender, EventArgs args)
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
    private void fileOpen_Click(Object sender, EventArgs args)
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
    private void fileReload_Click(Object sender, EventArgs args)
    {
        if (String.IsNullOrWhiteSpace(_currentFilePath))
        {
            MessageBox.Show(this,
                @"The current document has not been saved yet, so there is no file to reload.",
                @"Reload",
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
    private Boolean PromptToDiscardChangesForReload()
    {
        if (!_isDirty) return true;

        DialogResult result =
            MessageBox.Show(this,
                @"Reloading will discard unsaved changes in this document. Continue?",
                @"Unsaved Changes",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

        return result == DialogResult.Yes;
    }

    /// <summary>
    /// Rebuilds the Recent Documents submenu before it is shown.
    /// </summary>
    private void fileRecentDocuments_DropDownOpening(Object sender, EventArgs e) => RefreshRecentDocumentsMenu();

    /// <summary>
    /// Opens a path chosen from the Recent Documents list.
    /// </summary>
    private void fileRecentDocumentsEntry_Click(Object sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;

        if (item.Tag is not String path || String.IsNullOrWhiteSpace(path)) return;

        if (!PromptToSaveChanges()) return;

        TryOpenFileAtPath(path);
    }

    /// <summary>
    /// Populates the Recent Documents submenu from user settings.
    /// </summary>
    private void RefreshRecentDocumentsMenu()
    {
        fileRecentDocuments.DropDownItems.Clear();
        List<String> paths = ParseRecentDocumentsFromSettings();

        if (paths.Count == 0)
        {
            ToolStripMenuItem placeholder = new("(No recent documents)");
            placeholder.Enabled = false;
            fileRecentDocuments.DropDownItems.Add(placeholder);

            return;
        }

        foreach (String fullPath in paths)
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
    private static List<String> ParseRecentDocumentsFromSettings()
    {
        String raw = Settings.Default.RecentDocuments ?? String.Empty;
        List<String> list = [];

        if (String.IsNullOrWhiteSpace(raw)) return list;

        String[] parts = raw.Split(['\n'], StringSplitOptions.RemoveEmptyEntries);
        list.AddRange(parts.Select(part => part.Trim()).Where(trimmed => trimmed.Length > 0));

        return list;
    }

    /// <summary>
    /// Persists the recent-document list as newline-separated full paths.
    /// </summary>
    private static String SerializeRecentDocumentsPaths(IReadOnlyList<String> paths) => String.Join("\n", paths);

    /// <summary>
    /// Inserts a path at the front of the recent list and trims it to the configured capacity.
    /// </summary>
    /// <param name="fullPath">The path that was opened successfully.</param>
    private void RememberRecentDocument(String fullPath)
    {
        if (String.IsNullOrWhiteSpace(fullPath)) return;

        String normalized;

        try
        {
            normalized = Path.GetFullPath(fullPath);
        }
        catch (Exception)
        {
            return;
        }

        List<String> paths = ParseRecentDocumentsFromSettings();

        for (Int32 i = paths.Count - 1; i >= 0; i--)
        {
            if (String.Equals(paths[i], normalized, StringComparison.OrdinalIgnoreCase))
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
    private void RemoveRecentDocumentFromSettings(String filePath)
    {
        if (String.IsNullOrWhiteSpace(filePath)) return;

        String normalized;

        try
        {
            normalized = Path.GetFullPath(filePath);
        }
        catch (Exception)
        {
            return;
        }

        List<String> paths = ParseRecentDocumentsFromSettings();
        Boolean changed = false;

        for (Int32 i = paths.Count - 1; i >= 0; i--)
        {
            if (!String.Equals(paths[i], normalized, StringComparison.OrdinalIgnoreCase)) continue;

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
    private static String FormatRecentMenuCaption(String fullPath)
    {
        const Int32 maxDisplay = 72;

        if (fullPath.Length <= maxDisplay) return fullPath;

        return "..." + fullPath[^(maxDisplay - 3)..];
    }

    /// <summary>
    /// Loads a markup file from disk into the editor and refreshes the preview.
    /// </summary>
    /// <param name="filePath">The path to the file to open.</param>
    /// <returns><see langword="true"/> if the file was loaded; otherwise <see langword="false"/>.</returns>
    private Boolean TryOpenFileAtPath(String filePath)
    {
        try
        {
            if (String.IsNullOrWhiteSpace(filePath)) return false;

            String fullPath = Path.GetFullPath(filePath);

            if (!File.Exists(fullPath))
            {
                RemoveRecentDocumentFromSettings(fullPath);

                MessageBox.Show(this, $@"The file was not found.\r\n\r\n{fullPath}", @"Open Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            String contents = File.ReadAllText(fullPath, Encoding.UTF8);
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
            MessageBox.Show(this, $@"Could not open file.\r\n\r\n{ex.Message}", @"Open Failed", MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return false;
        }
    }

    /// <summary>
    /// Saves the document, prompting for a path if it has not been saved before.
    /// </summary>
    private void fileSave_Click(Object sender, EventArgs args) => DoSaveDocument();

    /// <summary>
    /// Saves the document to a new path chosen by the user.
    /// </summary>
    private void fileSaveAs_Click(Object sender, EventArgs args)
    {
        String originalPath = _currentFilePath;
        _currentFilePath = null;

        fileSave_Click(sender, args);

        if (!String.IsNullOrWhiteSpace(_currentFilePath)) return;

        _currentFilePath = originalPath;
    }

    /// <summary>
    /// Prints the rendered preview using the browser print dialog.
    /// </summary>
    private void filePrint_Click(Object sender, EventArgs args)
    {
        _pendingPrintAfterRender = true;

        if (_previewReady)
            DoRenderPreview();
        else
            _ = _previewBrowser.EnsureCoreWebView2Async(null);
    }

    /// <summary>
    /// Exports the rendered preview to a PDF file.
    /// </summary>
    private void fileExportPdf_Click(Object sender, EventArgs args)
    {
        using SaveFileDialog dialog = new();

        dialog.Filter = @"PDF Files|*.pdf|All Files|*.*";
        dialog.Title = @"Export as PDF";
        String baseName = Path.GetFileNameWithoutExtension(_currentFilePath ?? "document");
        dialog.FileName = baseName + ".pdf";

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        _pendingPdfExportPath = dialog.FileName;

        if (_previewReady)
            DoRenderPreview();
        else
            _ = _previewBrowser.EnsureCoreWebView2Async(null);
    }

    /// <summary>
    /// Exports the current document as a standalone LaTeX file.
    /// </summary>
    private void fileExportTex_Click(Object sender, EventArgs args)
    {
        using SaveFileDialog dialog = new();

        dialog.Filter = @"TeX Files|*.tex|All Files|*.*";
        dialog.Title = @"Export as TeX";
        String baseName = Path.GetFileNameWithoutExtension(_currentFilePath ?? "document");
        dialog.FileName = baseName + ".tex";

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        String tex = MarkupParser.ConvertMarkupToLatexDocument(_editorTextBox.Text);

        try
        {
            File.WriteAllText(dialog.FileName, tex, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                $@"Could not export file.\r\n\r\n{ex.Message}",
                @"Export Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Handles Exit; closes the form so <see cref="MarkupEditor_FormClosing"/> runs the save prompt and persists settings once.
    /// </summary>
    private void fileExit_Click(Object sender, EventArgs args) => Close();

    #endregion

    #region Edit menu

    /// <summary>Cuts the selected text to the clipboard.</summary>
    private void editCut_Click(Object sender, EventArgs args) => _editorTextBox.Cut();

    /// <summary>Copies the selected text to the clipboard.</summary>
    private void editCopy_Click(Object sender, EventArgs args) => _editorTextBox.Copy();

    /// <summary>Pastes clipboard text at the current cursor position.</summary>
    private void editPaste_Click(Object sender, EventArgs args) => _editorTextBox.Paste();

    /// <summary>Selects all text in the editor.</summary>
    private void editSelectAll_Click(Object sender, EventArgs args) => _editorTextBox.SelectAll();

    /// <summary>
    /// Opens the find/replace dialog with focus on the Find field.
    /// </summary>
    private void editFind_Click(Object sender, EventArgs args)
    {
        using FindReplaceDialog dialog = new(_editorTextBox, initialFocusOnReplace: false);

        dialog.ShowDialog(this);
    }

    /// <summary>
    /// Opens the find/replace dialog with focus on the Replace field.
    /// </summary>
    private void editReplace_Click(Object sender, EventArgs args)
    {
        using FindReplaceDialog dialog = new(_editorTextBox, initialFocusOnReplace: true);

        dialog.ShowDialog(this);
    }

    /// <summary>Undoes the last edit operation if one is available.</summary>
    private void editUndo_Click(Object sender, EventArgs args)
    {
        if (_editorTextBox.CanUndo) _editorTextBox.Undo();
    }

    /// <summary>Redoes the last undone edit via the Win32 EM_REDO message.</summary>
    private void editRedo_Click(Object sender, EventArgs args) =>
        SendMessage(_editorTextBox.Handle, EmRedo, IntPtr.Zero, IntPtr.Zero);

    #endregion

    #region Tools menu and editor appearance

    /// <summary>Manually triggers a preview render from the Tools menu (Render Preview).</summary>
    private void toolsRender_Click(Object sender, EventArgs args) => DoRenderPreview();

    /// <summary>
    /// Opens the Settings dialog; applies accepted changes immediately.
    /// </summary>
    private void toolsSettings_Click(Object sender, EventArgs args)
    {
        using SettingsDialog dialog =
            new(
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
        Boolean livePreviewChanged = _livePreviewEnabled != dialog.LivePreview;
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

        Boolean newHorizontal = dialog.HorizontalSplit;

        if (_mainSplit.Orientation == Orientation.Horizontal != newHorizontal)
            ApplySplitOrientation(newHorizontal);

        _lineNumberPanel.Visible = dialog.ShowLineNumbers;

        Boolean allowRawHtmlChanged = _allowRawHtml != dialog.AllowRawHtml;
        _allowRawHtml = dialog.AllowRawHtml;

        Single newFontSize = dialog.FontSize;

        if (Math.Abs(newFontSize - _editorTextBox.Font.Size) > 0.01f)
            _editorTextBox.Font = new Font(_editorTextBox.Font.FontFamily, newFontSize, _editorTextBox.Font.Style);

        if (allowRawHtmlChanged) DoRenderPreview();
    }

    #endregion

    #region Help

    /// <summary>
    /// Displays a dialog listing the supported markup syntax.
    /// </summary>
    private void helpSyntax_Click(Object sender, EventArgs args) =>
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
            @"Supported Markup",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

    /// <summary>
    /// Displays the About dialog.
    /// </summary>
    private void helpAbout_Click(Object sender, EventArgs args) =>
        MessageBox.Show(this,
            @"MarkupEditor\r\n\r\nSimple markup editor with live preview.\r\n",
            @"About MarkupEditor",
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
    private Boolean PromptToSaveChanges()
    {
        if (!_isDirty) return true;

        DialogResult result =
            MessageBox.Show(this,
                @"You have unsaved changes. Save before continuing?",
                @"Unsaved Changes",
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
        String previousPath = _currentFilePath;

        if (String.IsNullOrWhiteSpace(_currentFilePath))
        {
            using SaveFileDialog dialog = new();

            dialog.Filter = @"Markdown|*.md|Text|*.txt|All Files|*.*";
            dialog.Title = @"Save Markup File";
            dialog.FileName = "document.md";

            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            _currentFilePath = dialog.FileName;
        }

        try
        {
            String fullPath = Path.GetFullPath(_currentFilePath);
            StopWatchingFile();
            _externalChangePending = false;

            _ignoreFileWatcherEventsUntilUtc =
                DateTime.UtcNow +
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

            if (!String.IsNullOrWhiteSpace(previousPath))
                StartWatchingFile(previousPath);

            MessageBox.Show(this, $@"Could not save file.\r\n\r\n{ex.Message}", @"Save Failed", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Exports the current document as a standalone HTML file.
    /// </summary>
    private void fileExportHtml_Click(Object sender, EventArgs args)
    {
        using SaveFileDialog dialog = new();

        dialog.Filter = @"HTML Files|*.html;*.htm|All Files|*.*";
        dialog.Title = @"Export as HTML";
        String baseName = Path.GetFileNameWithoutExtension(_currentFilePath ?? "document");
        dialog.FileName = baseName + ".html";

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        String html =
            MarkupParser.BuildHtmlDocument(MarkupParser.ConvertMarkupToHtml(_editorTextBox.Text, _allowRawHtml));

        try
        {
            File.WriteAllText(dialog.FileName, html, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $@"Could not export file.\r\n\r\n{ex.Message}", @"Export Failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Prints the preview when a render-triggered print request is waiting.
    /// </summary>
    private void TryPrintPreviewIfPending()
    {
        if (!_pendingPrintAfterRender) return;

        _pendingPrintAfterRender = false;

        if (!_previewReady || _previewBrowser.CoreWebView2 == null)
        {
            MessageBox.Show(this,
                @"The preview is not ready to print yet.",
                @"Print",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        try
        {
            _ = _previewBrowser.ExecuteScriptAsync("window.print();");
        }
        catch (InvalidOperationException)
        {
            MessageBox.Show(this,
                @"Could not open the print dialog for the preview.",
                @"Print Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Exports the current preview page to PDF when a render-triggered request is waiting.
    /// </summary>
    private async Task TryExportPdfIfPendingAsync()
    {
        if (String.IsNullOrWhiteSpace(_pendingPdfExportPath)) return;

        String outputPath = _pendingPdfExportPath;
        _pendingPdfExportPath = null;

        if (!_previewReady || _previewBrowser.CoreWebView2 == null)
        {
            MessageBox.Show(this,
                @"The preview is not ready to export yet.",
                @"Export as PDF",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        try
        {
            Boolean success = await _previewBrowser.CoreWebView2.PrintToPdfAsync(outputPath);

            if (success) return;

            MessageBox.Show(this,
                @"Could not export the preview to PDF.",
                @"Export Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                $@"Could not export the preview to PDF.\r\n\r\n{ex.Message}",
                @"Export Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Converts the editor text to HTML and displays it in the preview browser.
    /// Does nothing if the WebView2 browser is not yet initialized.
    /// </summary>
    private void DoRenderPreview()
    {
        if (!_previewReady) return;

        Int32 scrollLine = GetEditorCaretLine();
        _pendingPreviewScrollLine = scrollLine;

        String html =
            MarkupParser.BuildHtmlDocument(MarkupParser.ConvertMarkupToHtml(_editorTextBox.Text, _allowRawHtml));

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
    private void MarkupEditor_HandleCreated(Object sender, EventArgs e)
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
        String fileName =
            String.IsNullOrWhiteSpace(_currentFilePath)
                ? "Untitled.md"
                : Path.GetFileName(_currentFilePath);

        fileReload.Enabled = !String.IsNullOrWhiteSpace(_currentFilePath);
        Text = fileName + (_isDirty ? " *" : String.Empty) + @" - Markup Editor";
        _fileStatusLabel.Text = String.IsNullOrWhiteSpace(_currentFilePath) ? @"Unsaved document" : _currentFilePath;
        _modifiedStatusLabel.Text = _isDirty ? @"Modified" : @"Saved";
    }

    #endregion
}