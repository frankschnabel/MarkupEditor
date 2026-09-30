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
            _categoriesListBox = new System.Windows.Forms.ListBox();
            _settingsPanel = new System.Windows.Forms.Panel();
            _okButton = new System.Windows.Forms.Button();
            _cancelButton = new System.Windows.Forms.Button();
            SuspendLayout();
            //
            // _categoriesListBox
            //
            _categoriesListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _categoriesListBox.FormattingEnabled = true;
            _categoriesListBox.IntegralHeight = false;
            _categoriesListBox.ItemHeight = 20;
            _categoriesListBox.Items.AddRange(new object[] {
                "General",
                "Editor Text",
                "Line Numbers",
                "Preview Text",
                "Preview Code",
                "Preview Headers"});
            _categoriesListBox.Location = new System.Drawing.Point(12, 12);
            _categoriesListBox.Name = "_categoriesListBox";
            _categoriesListBox.Size = new System.Drawing.Size(154, 344);
            _categoriesListBox.TabIndex = 0;
            //
            // _settingsPanel
            //
            _settingsPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _settingsPanel.Location = new System.Drawing.Point(176, 12);
            _settingsPanel.Name = "_settingsPanel";
            _settingsPanel.Size = new System.Drawing.Size(420, 344);
            _settingsPanel.TabIndex = 1;
            // 
            // _okButton
            // 
            _okButton.Location = new System.Drawing.Point(440, 368);
            _okButton.Name = "_okButton";
            _okButton.Size = new System.Drawing.Size(75, 26);
            _okButton.TabIndex = 3;
            _okButton.Text = "OK";
            _okButton.Click += _okButton_Click;
            // 
            // _cancelButton
            // 
            _cancelButton.Location = new System.Drawing.Point(521, 368);
            _cancelButton.Name = "_cancelButton";
            _cancelButton.Size = new System.Drawing.Size(75, 26);
            _cancelButton.TabIndex = 4;
            _cancelButton.Text = "Cancel";
            _cancelButton.Click += _cancelButton_Click;
            // 
            // SettingsDialog
            // 
            AcceptButton = _okButton;
            CancelButton = _cancelButton;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(608, 407);
            Controls.Add(_categoriesListBox);
            Controls.Add(_settingsPanel);
            Controls.Add(_okButton);
            Controls.Add(_cancelButton);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsDialog";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Settings";
            ResumeLayout(false);
        }

        private System.Windows.Forms.ListBox _categoriesListBox;
        private System.Windows.Forms.Panel _settingsPanel;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
    }
}
