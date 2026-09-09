using System;
using System.Windows.Forms;

namespace MarkupEditor;

/// <summary>
/// Modal dialog for editing all application settings.
/// </summary>
internal sealed partial class SettingsDialog : Form
{
    /// <summary>
    /// Initializes a new instance of <see cref="SettingsDialog"/> populated with the supplied values.
    /// </summary>
    /// <param name="livePreview">Whether live preview is currently enabled.</param>
    /// <param name="wordWrap">Whether word wrap is currently enabled.</param>
    /// <param name="horizontalSplit">Whether horizontal split is currently active.</param>
    /// <param name="showLineNumbers">Whether the line number gutter is visible.</param>
    /// <param name="allowRawHtml">Whether raw HTML passthrough is enabled in the preview.</param>
    /// <param name="windowsNewMarkdownDocument">Whether Windows New/Markdown Document integration is enabled.</param>
    /// <param name="fontSize">Current editor font size in points.</param>
    public SettingsDialog(Boolean livePreview, Boolean wordWrap, Boolean horizontalSplit, Boolean showLineNumbers,
        Boolean allowRawHtml, Boolean windowsNewMarkdownDocument, Single fontSize)
    {
        InitializeComponent();

        _livePreviewCheckBox.Checked = livePreview;
        _wordWrapCheckBox.Checked = wordWrap;
        _horizontalSplitCheckBox.Checked = horizontalSplit;
        _showLineNumbersCheckBox.Checked = showLineNumbers;
        _allowRawHtmlCheckBox.Checked = allowRawHtml;
        _windowsNewMarkdownDocumentCheckBox.Checked = windowsNewMarkdownDocument;
        _fontSizeUpDown.Value = (Decimal)Math.Max(8f, Math.Min(28f, fontSize));
    }

    /// <summary>Gets the live preview setting chosen by the user.</summary>
    public Boolean LivePreview => _livePreviewCheckBox.Checked;

    /// <summary>Gets the word wrap setting chosen by the user.</summary>
    public Boolean WordWrap => _wordWrapCheckBox.Checked;

    /// <summary>Gets the horizontal split setting chosen by the user.</summary>
    public Boolean HorizontalSplit => _horizontalSplitCheckBox.Checked;

    /// <summary>Gets the show line numbers setting chosen by the user.</summary>
    public Boolean ShowLineNumbers => _showLineNumbersCheckBox.Checked;

    /// <summary>Gets the allow raw HTML setting chosen by the user.</summary>
    public Boolean AllowRawHtml => _allowRawHtmlCheckBox.Checked;

    /// <summary>Gets whether Windows New/Markdown Document integration should be enabled.</summary>
    public Boolean WindowsNewMarkdownDocument => _windowsNewMarkdownDocumentCheckBox.Checked;

    /// <summary>Gets the editor font size chosen by the user.</summary>
    public Single FontSize => (Single)_fontSizeUpDown.Value;

    /// <summary>
    /// Closes the dialog, accepting the current values.
    /// </summary>
    private void _okButton_Click(Object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    /// <summary>
    /// Closes the dialog, discarding any changes.
    /// </summary>
    private void _cancelButton_Click(Object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}