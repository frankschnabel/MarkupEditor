using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

// ReSharper disable LocalizableElement

namespace MarkupEditor;

/// <summary>
/// Modal dialog for editing all application settings.
/// </summary>
internal sealed partial class SettingsDialog : Form
{
    private readonly DisplayFontSettings[] _fontSettings;
    private readonly ComboBox _fontFamilyComboBox = new();
    private readonly ComboBox _headingLevelComboBox = new();
    private readonly NumericUpDown _fontSizeUpDown = new();
    private readonly Button _fontColorButton = new();
    private readonly CheckBox _livePreviewCheckBox = new();
    private readonly CheckBox _wordWrapCheckBox = new();
    private readonly CheckBox _horizontalSplitCheckBox = new();
    private readonly CheckBox _showLineNumbersCheckBox = new();
    private readonly CheckBox _allowRawHtmlCheckBox = new();
    private readonly CheckBox _windowsNewMarkdownDocumentCheckBox = new();
    private readonly Label _headingLevelLabel = new();
    private readonly Label _fontPageHeadingLabel = new();
    private Color _selectedFontColor;
    private Int32 _selectedCategoryIndex;
    private Int32 _selectedHeadingLevelIndex = -1;
    private Boolean _changingHeadingLevel;

    /// <summary>
    /// Initializes a new instance of <see cref="SettingsDialog"/> populated with the supplied values.
    /// </summary>
    /// <param name="livePreview">Whether live preview is currently enabled.</param>
    /// <param name="wordWrap">Whether word wrap is currently enabled.</param>
    /// <param name="horizontalSplit">Whether horizontal split is currently active.</param>
    /// <param name="showLineNumbers">Whether the line number gutter is visible.</param>
    /// <param name="allowRawHtml">Whether raw HTML passthrough is enabled in the preview.</param>
    /// <param name="windowsNewMarkdownDocument">Whether Windows New/Markdown Document integration is enabled.</param>
    /// <param name="fontSettings">Appearance settings for editor, gutter, preview text, code, and H1-H6 headings.</param>
    public SettingsDialog(Boolean livePreview, Boolean wordWrap, Boolean horizontalSplit, Boolean showLineNumbers,
        Boolean allowRawHtml, Boolean windowsNewMarkdownDocument, DisplayFontSettings[] fontSettings)
    {
        InitializeComponent();

        _fontSettings = (DisplayFontSettings[])fontSettings.Clone();

        InitializeGeneralPage(livePreview, wordWrap, horizontalSplit, showLineNumbers, allowRawHtml,
            windowsNewMarkdownDocument);

        InitializeFontControls();
        _categoriesListBox.SelectedIndexChanged += _categoriesListBox_SelectedIndexChanged;
        _categoriesListBox.SelectedIndex = 0;
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

    /// <summary>Gets the font appearance settings chosen by the user.</summary>
    public DisplayFontSettings[] FontSettings
    {
        get
        {
            SaveCurrentFontSettings();

            return (DisplayFontSettings[])_fontSettings.Clone();
        }
    }

    /// <summary>
    /// Adds general behavior settings to the General category page.
    /// </summary>
    private void InitializeGeneralPage(Boolean livePreview, Boolean wordWrap, Boolean horizontalSplit,
        Boolean showLineNumbers, Boolean allowRawHtml, Boolean windowsNewMarkdownDocument)
    {
        _livePreviewCheckBox.Text = "Live Preview";
        _wordWrapCheckBox.Text = "Word Wrap";
        _horizontalSplitCheckBox.Text = "Horizontal Split";
        _showLineNumbersCheckBox.Text = "Show Line Numbers";
        _allowRawHtmlCheckBox.Text = "Allow Raw HTML in Preview";
        _windowsNewMarkdownDocumentCheckBox.Text = "Add \"New > Markdown Document\" menu entry";

        CheckBox[] checkBoxes =
        [
            _livePreviewCheckBox, _wordWrapCheckBox, _horizontalSplitCheckBox,
            _showLineNumbersCheckBox, _allowRawHtmlCheckBox, _windowsNewMarkdownDocumentCheckBox
        ];

        Boolean[] values =
            [livePreview, wordWrap, horizontalSplit, showLineNumbers, allowRawHtml, windowsNewMarkdownDocument];

        for (Int32 index = 0; index < checkBoxes.Length; index++)
        {
            checkBoxes[index].AutoSize = true;
            checkBoxes[index].Location = new Point(20, 24 + index * 34);
            checkBoxes[index].Checked = values[index];
            _settingsPanel.Controls.Add(checkBoxes[index]);
        }
    }

    /// <summary>
    /// Creates the shared font controls used by each appearance category.
    /// </summary>
    private void InitializeFontControls()
    {
        _fontPageHeadingLabel.AutoSize = true;
        _fontPageHeadingLabel.Font = new Font(Font, FontStyle.Bold);
        _fontPageHeadingLabel.Location = new Point(20, 20);
        _fontPageHeadingLabel.Text = "Text appearance";

        Label familyLabel = new() { AutoSize = true, Location = new Point(20, 72), Text = "Font face" };
        Label sizeLabel = new() { AutoSize = true, Location = new Point(20, 126), Text = "Size (pt)" };
        Label colorLabel = new() { AutoSize = true, Location = new Point(20, 180), Text = "Text color" };
        _headingLevelLabel.AutoSize = true;
        _headingLevelLabel.Location = new Point(20, 30);
        _headingLevelLabel.Text = "Heading";
        _headingLevelComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _headingLevelComboBox.Items.AddRange(["H1", "H2", "H3", "H4", "H5", "H6"]);
        _headingLevelComboBox.Location = new Point(110, 26);
        _headingLevelComboBox.Size = new Size(100, 23);
        _headingLevelComboBox.SelectedIndexChanged += _headingLevelComboBox_SelectedIndexChanged;
        _fontFamilyComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _fontFamilyComboBox.Location = new Point(110, 68);
        _fontFamilyComboBox.Size = new Size(250, 23);

        foreach (FontFamily family in new InstalledFontCollection().Families)
            _fontFamilyComboBox.Items.Add(family.Name);

        _fontSizeUpDown.DecimalPlaces = 1;
        _fontSizeUpDown.Increment = 1;
        _fontSizeUpDown.Minimum = 6;
        _fontSizeUpDown.Maximum = 36;
        _fontSizeUpDown.Location = new Point(110, 122);
        _fontSizeUpDown.Size = new Size(90, 23);
        _fontColorButton.Location = new Point(110, 176);
        _fontColorButton.Size = new Size(100, 36);
        _fontColorButton.Text = "Choose...";
        _fontColorButton.Click += _fontColorButton_Click;

        _settingsPanel.Controls.AddRange([
            _fontPageHeadingLabel, _headingLevelLabel, _headingLevelComboBox, familyLabel, sizeLabel, colorLabel,
            _fontFamilyComboBox, _fontSizeUpDown, _fontColorButton
        ]);
    }

    /// <summary>
    /// Saves the active appearance page before changing categories or accepting the dialog.
    /// </summary>
    private void SaveCurrentFontSettings()
    {
        if (_selectedCategoryIndex <= 0 || _fontFamilyComboBox.SelectedItem == null) return;

        Int32 styleIndex = GetCurrentFontSettingsIndex();

        _fontSettings[styleIndex] =
            new DisplayFontSettings((String)_fontFamilyComboBox.SelectedItem,
                _selectedFontColor, (Single)_fontSizeUpDown.Value);
    }

    /// <summary>
    /// Displays the settings page selected in the category list.
    /// </summary>
    private void _categoriesListBox_SelectedIndexChanged(Object sender, EventArgs e)
    {
        SaveCurrentFontSettings();
        _selectedCategoryIndex = _categoriesListBox.SelectedIndex;
        Boolean generalPage = _selectedCategoryIndex == 0;

        foreach (Control control in _settingsPanel.Controls)
            control.Visible = generalPage ? control is CheckBox : control is not CheckBox;

        if (generalPage) return;

        Boolean headersPage = _selectedCategoryIndex == 5;
        _fontPageHeadingLabel.Visible = !headersPage;
        _headingLevelLabel.Visible = headersPage;
        _headingLevelComboBox.Visible = headersPage;

        if (headersPage && _selectedHeadingLevelIndex < 0)
        {
            _changingHeadingLevel = true;
            _headingLevelComboBox.SelectedIndex = 0;
            _changingHeadingLevel = false;
            _selectedHeadingLevelIndex = 0;
        }

        LoadCurrentFontSettings();
    }

    /// <summary>
    /// Saves the current heading style and loads the newly selected heading level.
    /// </summary>
    private void _headingLevelComboBox_SelectedIndexChanged(Object sender, EventArgs e)
    {
        if (_changingHeadingLevel) return;

        SaveCurrentFontSettings();
        _selectedHeadingLevelIndex = _headingLevelComboBox.SelectedIndex;
        LoadCurrentFontSettings();
    }

    /// <summary>
    /// Loads the selected element's saved font settings into the shared controls.
    /// </summary>
    private void LoadCurrentFontSettings()
    {
        if (_selectedCategoryIndex <= 0) return;

        DisplayFontSettings settings = _fontSettings[GetCurrentFontSettingsIndex()];
        Int32 familyIndex = _fontFamilyComboBox.Items.IndexOf(settings.FontFamily);

        _fontFamilyComboBox.SelectedIndex =
            familyIndex >= 0 ? familyIndex : _fontFamilyComboBox.Items.IndexOf("Segoe UI");

        if (_fontFamilyComboBox.SelectedIndex < 0 && _fontFamilyComboBox.Items.Count > 0)
            _fontFamilyComboBox.SelectedIndex = 0;

        _fontSizeUpDown.Value =
            Math.Max(_fontSizeUpDown.Minimum,
                Math.Min(_fontSizeUpDown.Maximum, (Decimal)settings.FontSize));

        _selectedFontColor = settings.FontColor;
        _fontColorButton.BackColor = _selectedFontColor;
        _fontColorButton.ForeColor = _selectedFontColor.GetBrightness() < 0.5f ? Color.White : Color.Black;
    }

    /// <summary>
    /// Returns the font-settings array index for the currently selected category and heading level.
    /// </summary>
    private Int32 GetCurrentFontSettingsIndex() =>
        _selectedCategoryIndex == 5 ? 4 + Math.Max(0, _selectedHeadingLevelIndex) : _selectedCategoryIndex - 1;

    /// <summary>
    /// Opens the standard color picker for the active text element.
    /// </summary>
    private void _fontColorButton_Click(Object sender, EventArgs e)
    {
        using ColorDialog dialog = new();
        dialog.Color = _selectedFontColor;
        dialog.FullOpen = true;

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        _selectedFontColor = dialog.Color;
        _fontColorButton.BackColor = _selectedFontColor;
        _fontColorButton.ForeColor = _selectedFontColor.GetBrightness() < 0.5f ? Color.White : Color.Black;
    }

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