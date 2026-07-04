namespace MarkupEditor
{
    partial class FindReplaceDialog
    {
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support — do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._findLabel = new System.Windows.Forms.Label();
            this._findTextBox = new System.Windows.Forms.TextBox();
            this._replaceLabel = new System.Windows.Forms.Label();
            this._replaceTextBox = new System.Windows.Forms.TextBox();
            this._matchCaseCheckBox = new System.Windows.Forms.CheckBox();
            this._wholeWordCheckBox = new System.Windows.Forms.CheckBox();
            this._findNextButton = new System.Windows.Forms.Button();
            this._replaceButton = new System.Windows.Forms.Button();
            this._replaceAllButton = new System.Windows.Forms.Button();
            this._closeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _findLabel
            // 
            this._findLabel.AutoSize = true;
            this._findLabel.Location = new System.Drawing.Point(12, 15);
            this._findLabel.Name = "_findLabel";
            this._findLabel.Size = new System.Drawing.Size(30, 13);
            this._findLabel.TabIndex = 0;
            this._findLabel.Text = "&Find:";
            // 
            // _findTextBox
            // 
            this._findTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._findTextBox.Location = new System.Drawing.Point(90, 12);
            this._findTextBox.Name = "_findTextBox";
            this._findTextBox.Size = new System.Drawing.Size(292, 20);
            this._findTextBox.TabIndex = 1;
            // 
            // _replaceLabel
            // 
            this._replaceLabel.AutoSize = true;
            this._replaceLabel.Location = new System.Drawing.Point(12, 44);
            this._replaceLabel.Name = "_replaceLabel";
            this._replaceLabel.Size = new System.Drawing.Size(72, 13);
            this._replaceLabel.TabIndex = 2;
            this._replaceLabel.Text = "Replace w&ith:";
            // 
            // _replaceTextBox
            // 
            this._replaceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._replaceTextBox.Location = new System.Drawing.Point(90, 41);
            this._replaceTextBox.Name = "_replaceTextBox";
            this._replaceTextBox.Size = new System.Drawing.Size(292, 20);
            this._replaceTextBox.TabIndex = 3;
            // 
            // _matchCaseCheckBox
            // 
            this._matchCaseCheckBox.AutoSize = true;
            this._matchCaseCheckBox.Location = new System.Drawing.Point(90, 72);
            this._matchCaseCheckBox.Name = "_matchCaseCheckBox";
            this._matchCaseCheckBox.Size = new System.Drawing.Size(82, 17);
            this._matchCaseCheckBox.TabIndex = 4;
            this._matchCaseCheckBox.Text = "Match &case";
            this._matchCaseCheckBox.UseVisualStyleBackColor = true;
            // 
            // _wholeWordCheckBox
            // 
            this._wholeWordCheckBox.AutoSize = true;
            this._wholeWordCheckBox.Location = new System.Drawing.Point(90, 95);
            this._wholeWordCheckBox.Name = "_wholeWordCheckBox";
            this._wholeWordCheckBox.Size = new System.Drawing.Size(83, 17);
            this._wholeWordCheckBox.TabIndex = 5;
            this._wholeWordCheckBox.Text = "Whole &word";
            this._wholeWordCheckBox.UseVisualStyleBackColor = true;
            // 
            // _findNextButton
            // 
            this._findNextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._findNextButton.Location = new System.Drawing.Point(398, 10);
            this._findNextButton.Name = "_findNextButton";
            this._findNextButton.Size = new System.Drawing.Size(88, 23);
            this._findNextButton.TabIndex = 6;
            this._findNextButton.Text = "Find &Next";
            this._findNextButton.UseVisualStyleBackColor = true;
            this._findNextButton.Click += new System.EventHandler(this._findNextButton_Click);
            // 
            // _replaceButton
            // 
            this._replaceButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._replaceButton.Location = new System.Drawing.Point(398, 39);
            this._replaceButton.Name = "_replaceButton";
            this._replaceButton.Size = new System.Drawing.Size(88, 23);
            this._replaceButton.TabIndex = 7;
            this._replaceButton.Text = "&Replace";
            this._replaceButton.UseVisualStyleBackColor = true;
            this._replaceButton.Click += new System.EventHandler(this._replaceButton_Click);
            // 
            // _replaceAllButton
            // 
            this._replaceAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._replaceAllButton.Location = new System.Drawing.Point(398, 68);
            this._replaceAllButton.Name = "_replaceAllButton";
            this._replaceAllButton.Size = new System.Drawing.Size(88, 23);
            this._replaceAllButton.TabIndex = 8;
            this._replaceAllButton.Text = "Replace &All";
            this._replaceAllButton.UseVisualStyleBackColor = true;
            this._replaceAllButton.Click += new System.EventHandler(this._replaceAllButton_Click);
            // 
            // _closeButton
            // 
            this._closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._closeButton.Location = new System.Drawing.Point(398, 97);
            this._closeButton.Name = "_closeButton";
            this._closeButton.Size = new System.Drawing.Size(88, 23);
            this._closeButton.TabIndex = 9;
            this._closeButton.Text = "Close";
            this._closeButton.UseVisualStyleBackColor = true;
            this._closeButton.Click += new System.EventHandler(this._closeButton_Click);
            // 
            // FindReplaceDialog
            // 
            this.AcceptButton = this._findNextButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._closeButton;
            this.ClientSize = new System.Drawing.Size(498, 132);
            this.Controls.Add(this._closeButton);
            this.Controls.Add(this._replaceAllButton);
            this.Controls.Add(this._replaceButton);
            this.Controls.Add(this._findNextButton);
            this.Controls.Add(this._wholeWordCheckBox);
            this.Controls.Add(this._matchCaseCheckBox);
            this.Controls.Add(this._replaceTextBox);
            this.Controls.Add(this._replaceLabel);
            this.Controls.Add(this._findTextBox);
            this.Controls.Add(this._findLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FindReplaceDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Find and Replace";
            this.Shown += new System.EventHandler(this.FindReplaceDialog_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _findLabel;
        private System.Windows.Forms.TextBox _findTextBox;
        private System.Windows.Forms.Label _replaceLabel;
        private System.Windows.Forms.TextBox _replaceTextBox;
        private System.Windows.Forms.CheckBox _matchCaseCheckBox;
        private System.Windows.Forms.CheckBox _wholeWordCheckBox;
        private System.Windows.Forms.Button _findNextButton;
        private System.Windows.Forms.Button _replaceButton;
        private System.Windows.Forms.Button _replaceAllButton;
        private System.Windows.Forms.Button _closeButton;
    }
}
