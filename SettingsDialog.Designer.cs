namespace MarkupEditor
{
    internal sealed partial class SettingsDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _editorGroup = new System.Windows.Forms.GroupBox();
            _fontSizeLabel = new System.Windows.Forms.Label();
            _fontSizeUpDown = new System.Windows.Forms.NumericUpDown();
            _wordWrapCheckBox = new System.Windows.Forms.CheckBox();
            _showLineNumbersCheckBox = new System.Windows.Forms.CheckBox();
            _previewGroup = new System.Windows.Forms.GroupBox();
            _livePreviewCheckBox = new System.Windows.Forms.CheckBox();
            _horizontalSplitCheckBox = new System.Windows.Forms.CheckBox();
            _allowRawHtmlCheckBox = new System.Windows.Forms.CheckBox();
            _okButton = new System.Windows.Forms.Button();
            _cancelButton = new System.Windows.Forms.Button();
            _editorGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_fontSizeUpDown).BeginInit();
            _previewGroup.SuspendLayout();
            SuspendLayout();
            // 
            // _editorGroup
            // 
            _editorGroup.Controls.Add(_fontSizeLabel);
            _editorGroup.Controls.Add(_fontSizeUpDown);
            _editorGroup.Controls.Add(_wordWrapCheckBox);
            _editorGroup.Controls.Add(_showLineNumbersCheckBox);
            _editorGroup.Location = new System.Drawing.Point(12, 12);
            _editorGroup.Name = "_editorGroup";
            _editorGroup.Size = new System.Drawing.Size(290, 106);
            _editorGroup.TabIndex = 0;
            _editorGroup.TabStop = false;
            _editorGroup.Text = "Editor";
            // 
            // _fontSizeLabel
            // 
            _fontSizeLabel.AutoSize = true;
            _fontSizeLabel.Location = new System.Drawing.Point(10, 24);
            _fontSizeLabel.Name = "_fontSizeLabel";
            _fontSizeLabel.Size = new System.Drawing.Size(57, 15);
            _fontSizeLabel.TabIndex = 0;
            _fontSizeLabel.Text = "Font Size:";
            // 
            // _fontSizeUpDown
            // 
            _fontSizeUpDown.DecimalPlaces = 0;
            _fontSizeUpDown.Location = new System.Drawing.Point(76, 22);
            _fontSizeUpDown.Maximum = new decimal(new int[] { 28, 0, 0, 0 });
            _fontSizeUpDown.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            _fontSizeUpDown.Name = "_fontSizeUpDown";
            _fontSizeUpDown.Size = new System.Drawing.Size(56, 23);
            _fontSizeUpDown.TabIndex = 1;
            _fontSizeUpDown.Value = new decimal(new int[] { 11, 0, 0, 0 });
            // 
            // _wordWrapCheckBox
            // 
            _wordWrapCheckBox.AutoSize = true;
            _wordWrapCheckBox.Location = new System.Drawing.Point(10, 54);
            _wordWrapCheckBox.Name = "_wordWrapCheckBox";
            _wordWrapCheckBox.Size = new System.Drawing.Size(82, 19);
            _wordWrapCheckBox.TabIndex = 2;
            _wordWrapCheckBox.Text = "Word &Wrap";
            // 
            // _showLineNumbersCheckBox
            // 
            _showLineNumbersCheckBox.AutoSize = true;
            _showLineNumbersCheckBox.Location = new System.Drawing.Point(10, 79);
            _showLineNumbersCheckBox.Name = "_showLineNumbersCheckBox";
            _showLineNumbersCheckBox.Size = new System.Drawing.Size(100, 19);
            _showLineNumbersCheckBox.TabIndex = 3;
            _showLineNumbersCheckBox.Text = "&Line Numbers";
            // 
            // _previewGroup
            // 
            _previewGroup.Controls.Add(_livePreviewCheckBox);
            _previewGroup.Controls.Add(_horizontalSplitCheckBox);
            _previewGroup.Controls.Add(_allowRawHtmlCheckBox);
            _previewGroup.Location = new System.Drawing.Point(12, 128);
            _previewGroup.Name = "_previewGroup";
            _previewGroup.Size = new System.Drawing.Size(290, 104);
            _previewGroup.TabIndex = 1;
            _previewGroup.TabStop = false;
            _previewGroup.Text = "Preview";
            // 
            // _livePreviewCheckBox
            // 
            _livePreviewCheckBox.AutoSize = true;
            _livePreviewCheckBox.Location = new System.Drawing.Point(10, 24);
            _livePreviewCheckBox.Name = "_livePreviewCheckBox";
            _livePreviewCheckBox.Size = new System.Drawing.Size(91, 19);
            _livePreviewCheckBox.TabIndex = 0;
            _livePreviewCheckBox.Text = "&Live Preview";
            // 
            // _horizontalSplitCheckBox
            // 
            _horizontalSplitCheckBox.AutoSize = true;
            _horizontalSplitCheckBox.Location = new System.Drawing.Point(10, 49);
            _horizontalSplitCheckBox.Name = "_horizontalSplitCheckBox";
            _horizontalSplitCheckBox.Size = new System.Drawing.Size(109, 19);
            _horizontalSplitCheckBox.TabIndex = 1;
            _horizontalSplitCheckBox.Text = "&Horizontal Split";
            // 
            // _allowRawHtmlCheckBox
            // 
            _allowRawHtmlCheckBox.AutoSize = true;
            _allowRawHtmlCheckBox.Location = new System.Drawing.Point(10, 74);
            _allowRawHtmlCheckBox.Name = "_allowRawHtmlCheckBox";
            _allowRawHtmlCheckBox.Size = new System.Drawing.Size(163, 19);
            _allowRawHtmlCheckBox.TabIndex = 2;
            _allowRawHtmlCheckBox.Text = "Allow &Raw HTML in Preview";
            // 
            // _okButton
            // 
            _okButton.Location = new System.Drawing.Point(146, 244);
            _okButton.Name = "_okButton";
            _okButton.Size = new System.Drawing.Size(75, 26);
            _okButton.TabIndex = 2;
            _okButton.Text = "OK";
            _okButton.Click += _okButton_Click;
            // 
            // _cancelButton
            // 
            _cancelButton.Location = new System.Drawing.Point(227, 244);
            _cancelButton.Name = "_cancelButton";
            _cancelButton.Size = new System.Drawing.Size(75, 26);
            _cancelButton.TabIndex = 3;
            _cancelButton.Text = "Cancel";
            _cancelButton.Click += _cancelButton_Click;
            // 
            // SettingsDialog
            // 
            AcceptButton = _okButton;
            CancelButton = _cancelButton;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(314, 282);
            Controls.Add(_editorGroup);
            Controls.Add(_previewGroup);
            Controls.Add(_okButton);
            Controls.Add(_cancelButton);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsDialog";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Settings";
            _editorGroup.ResumeLayout(false);
            _editorGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_fontSizeUpDown).EndInit();
            _previewGroup.ResumeLayout(false);
            _previewGroup.PerformLayout();
            ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox _editorGroup;
        private System.Windows.Forms.Label _fontSizeLabel;
        private System.Windows.Forms.NumericUpDown _fontSizeUpDown;
        private System.Windows.Forms.CheckBox _wordWrapCheckBox;
        private System.Windows.Forms.CheckBox _showLineNumbersCheckBox;
        private System.Windows.Forms.GroupBox _previewGroup;
        private System.Windows.Forms.CheckBox _livePreviewCheckBox;
        private System.Windows.Forms.CheckBox _horizontalSplitCheckBox;
        private System.Windows.Forms.CheckBox _allowRawHtmlCheckBox;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
    }
}
