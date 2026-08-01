namespace MarkupEditor
{
    internal sealed partial class MarkupEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MarkupEditor));
            _menuStrip = new System.Windows.Forms.MenuStrip();
            fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            fileNew = new System.Windows.Forms.ToolStripMenuItem();
            fileOpen = new System.Windows.Forms.ToolStripMenuItem();
            fileReload = new System.Windows.Forms.ToolStripMenuItem();
            fileRecentSep = new System.Windows.Forms.ToolStripSeparator();
            fileRecentDocuments = new System.Windows.Forms.ToolStripMenuItem();
            fileSep1 = new System.Windows.Forms.ToolStripSeparator();
            fileSave = new System.Windows.Forms.ToolStripMenuItem();
            fileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            filePrint = new System.Windows.Forms.ToolStripMenuItem();
            fileExportMenu = new System.Windows.Forms.ToolStripMenuItem();
            fileExportPdf = new System.Windows.Forms.ToolStripMenuItem();
            fileExportTex = new System.Windows.Forms.ToolStripMenuItem();
            fileExportHtml = new System.Windows.Forms.ToolStripMenuItem();
            fileSep2 = new System.Windows.Forms.ToolStripSeparator();
            fileExit = new System.Windows.Forms.ToolStripMenuItem();
            editMenu = new System.Windows.Forms.ToolStripMenuItem();
            editUndo = new System.Windows.Forms.ToolStripMenuItem();
            editRedo = new System.Windows.Forms.ToolStripMenuItem();
            editSep1 = new System.Windows.Forms.ToolStripSeparator();
            editCut = new System.Windows.Forms.ToolStripMenuItem();
            editCopy = new System.Windows.Forms.ToolStripMenuItem();
            editPaste = new System.Windows.Forms.ToolStripMenuItem();
            editSepFind = new System.Windows.Forms.ToolStripSeparator();
            editFind = new System.Windows.Forms.ToolStripMenuItem();
            editReplace = new System.Windows.Forms.ToolStripMenuItem();
            editSep2 = new System.Windows.Forms.ToolStripSeparator();
            editSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            toolsMenu = new System.Windows.Forms.ToolStripMenuItem();
            toolsRender = new System.Windows.Forms.ToolStripMenuItem();
            toolsSep = new System.Windows.Forms.ToolStripSeparator();
            toolsSettings = new System.Windows.Forms.ToolStripMenuItem();
            helpMenu = new System.Windows.Forms.ToolStripMenuItem();
            helpSyntax = new System.Windows.Forms.ToolStripMenuItem();
            helpSep = new System.Windows.Forms.ToolStripSeparator();
            helpAbout = new System.Windows.Forms.ToolStripMenuItem();
            _mainSplit = new System.Windows.Forms.SplitContainer();
            _editorTextBox = new System.Windows.Forms.TextBox();
            _previewBrowser = new Microsoft.Web.WebView2.WinForms.WebView2();
            _statusStrip = new System.Windows.Forms.StatusStrip();
            _fileStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            _modifiedStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            _previewTimer = new System.Windows.Forms.Timer(components);
            _lineNumberPanel = new LineNumberPanel(_editorTextBox);
            _menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_mainSplit).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_previewBrowser).BeginInit();
            _mainSplit.Panel1.SuspendLayout();
            _mainSplit.Panel2.SuspendLayout();
            _mainSplit.SuspendLayout();
            _statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // _menuStrip
            // 
            _menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileMenu, editMenu, toolsMenu, helpMenu });
            _menuStrip.Location = new System.Drawing.Point(0, 0);
            _menuStrip.Name = "_menuStrip";
            _menuStrip.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            _menuStrip.Size = new System.Drawing.Size(1031, 24);
            _menuStrip.TabIndex = 0;
            _menuStrip.Text = "_menuStrip";
            // 
            // fileMenu
            // 
            fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { fileNew, fileOpen, fileReload, fileRecentSep, fileRecentDocuments, fileSep1, fileSave, fileSaveAs, filePrint, fileExportMenu, fileSep2, fileExit });
            fileMenu.Name = "fileMenu";
            fileMenu.Size = new System.Drawing.Size(37, 20);
            fileMenu.Text = "&File";
            // 
            // fileNew
            // 
            fileNew.Name = "fileNew";
            fileNew.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N;
            fileNew.Size = new System.Drawing.Size(195, 22);
            fileNew.Text = "&New";
            fileNew.Click += fileNew_Click;
            // 
            // fileOpen
            // 
            fileOpen.Name = "fileOpen";
            fileOpen.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
            fileOpen.Size = new System.Drawing.Size(195, 22);
            fileOpen.Text = "&Open...";
            fileOpen.Click += fileOpen_Click;
            // 
            // fileReload
            // 
            fileReload.Name = "fileReload";
            fileReload.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R;
            fileReload.Size = new System.Drawing.Size(195, 22);
            fileReload.Text = "&Reload";
            fileReload.Click += fileReload_Click;
            // 
            // fileRecentSep
            // 
            fileRecentSep.Name = "fileRecentSep";
            fileRecentSep.Size = new System.Drawing.Size(192, 6);
            // 
            // fileRecentDocuments
            // 
            fileRecentDocuments.Name = "fileRecentDocuments";
            fileRecentDocuments.Size = new System.Drawing.Size(195, 22);
            fileRecentDocuments.Text = "Recent &Documents";
            // 
            // fileSep1
            // 
            fileSep1.Name = "fileSep1";
            fileSep1.Size = new System.Drawing.Size(192, 6);
            // 
            // fileSave
            // 
            fileSave.Name = "fileSave";
            fileSave.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            fileSave.Size = new System.Drawing.Size(195, 22);
            fileSave.Text = "&Save";
            fileSave.Click += fileSave_Click;
            // 
            // fileSaveAs
            // 
            fileSaveAs.Name = "fileSaveAs";
            fileSaveAs.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.S;
            fileSaveAs.Size = new System.Drawing.Size(195, 22);
            fileSaveAs.Text = "Save &As...";
            fileSaveAs.Click += fileSaveAs_Click;
            // 
            // filePrint
            // 
            filePrint.Name = "filePrint";
            filePrint.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P;
            filePrint.Size = new System.Drawing.Size(195, 22);
            filePrint.Text = "&Print...";
            filePrint.Click += filePrint_Click;
            // 
            // fileExportMenu
            // 
            fileExportMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { fileExportPdf, fileExportTex, fileExportHtml });
            fileExportMenu.Name = "fileExportMenu";
            fileExportMenu.Size = new System.Drawing.Size(195, 22);
            fileExportMenu.Text = "&Export";
            // 
            // fileExportPdf
            // 
            fileExportPdf.Name = "fileExportPdf";
            fileExportPdf.Size = new System.Drawing.Size(195, 22);
            fileExportPdf.Text = "Export as &PDF...";
            fileExportPdf.Click += fileExportPdf_Click;
            // 
            // fileExportTex
            // 
            fileExportTex.Name = "fileExportTex";
            fileExportTex.Size = new System.Drawing.Size(195, 22);
            fileExportTex.Text = "Export as &TeX...";
            fileExportTex.Click += fileExportTex_Click;
            // 
            // fileExportHtml
            // 
            fileExportHtml.Name = "fileExportHtml";
            fileExportHtml.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.E;
            fileExportHtml.Size = new System.Drawing.Size(195, 22);
            fileExportHtml.Text = "Export as &HTML...";
            fileExportHtml.Click += fileExportHtml_Click;
            // 
            // fileSep2
            // 
            fileSep2.Name = "fileSep2";
            fileSep2.Size = new System.Drawing.Size(192, 6);
            // 
            // fileExit
            // 
            fileExit.Name = "fileExit";
            fileExit.Size = new System.Drawing.Size(195, 22);
            fileExit.Text = "E&xit";
            fileExit.Click += fileExit_Click;
            // 
            // editMenu
            // 
            editMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { editUndo, editRedo, editSep1, editCut, editCopy, editPaste, editSepFind, editFind, editReplace, editSep2, editSelectAll });
            editMenu.Name = "editMenu";
            editMenu.Size = new System.Drawing.Size(39, 20);
            editMenu.Text = "&Edit";
            // 
            // editUndo
            // 
            editUndo.Name = "editUndo";
            editUndo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z;
            editUndo.Size = new System.Drawing.Size(167, 22);
            editUndo.Text = "&Undo";
            editUndo.Click += editUndo_Click;
            // 
            // editRedo
            // 
            editRedo.Name = "editRedo";
            editRedo.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y;
            editRedo.Size = new System.Drawing.Size(167, 22);
            editRedo.Text = "&Redo";
            editRedo.Click += editRedo_Click;
            // 
            // editSep1
            // 
            editSep1.Name = "editSep1";
            editSep1.Size = new System.Drawing.Size(164, 6);
            // 
            // editCut
            // 
            editCut.Name = "editCut";
            editCut.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;
            editCut.Size = new System.Drawing.Size(167, 22);
            editCut.Text = "Cu&t";
            editCut.Click += editCut_Click;
            // 
            // editCopy
            // 
            editCopy.Name = "editCopy";
            editCopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            editCopy.Size = new System.Drawing.Size(167, 22);
            editCopy.Text = "&Copy";
            editCopy.Click += editCopy_Click;
            // 
            // editPaste
            // 
            editPaste.Name = "editPaste";
            editPaste.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
            editPaste.Size = new System.Drawing.Size(167, 22);
            editPaste.Text = "&Paste";
            editPaste.Click += editPaste_Click;
            // 
            // editSepFind
            // 
            editSepFind.Name = "editSepFind";
            editSepFind.Size = new System.Drawing.Size(164, 6);
            // 
            // editFind
            // 
            editFind.Name = "editFind";
            editFind.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F;
            editFind.Size = new System.Drawing.Size(167, 22);
            editFind.Text = "&Find...";
            editFind.Click += editFind_Click;
            // 
            // editReplace
            // 
            editReplace.Name = "editReplace";
            editReplace.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H;
            editReplace.Size = new System.Drawing.Size(167, 22);
            editReplace.Text = "&Replace...";
            editReplace.Click += editReplace_Click;
            // 
            // editSep2
            // 
            editSep2.Name = "editSep2";
            editSep2.Size = new System.Drawing.Size(164, 6);
            // 
            // editSelectAll
            // 
            editSelectAll.Name = "editSelectAll";
            editSelectAll.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A;
            editSelectAll.Size = new System.Drawing.Size(167, 22);
            editSelectAll.Text = "Select &All";
            editSelectAll.Click += editSelectAll_Click;
            // 
            // toolsMenu
            // 
            toolsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toolsRender, toolsSep, toolsSettings });
            toolsMenu.Name = "toolsMenu";
            toolsMenu.Size = new System.Drawing.Size(47, 20);
            toolsMenu.Text = "&Tools";
            // 
            // toolsRender
            // 
            toolsRender.Name = "toolsRender";
            toolsRender.ShortcutKeys = System.Windows.Forms.Keys.F5;
            toolsRender.Size = new System.Drawing.Size(174, 22);
            toolsRender.Text = "&Render Preview";
            toolsRender.Click += toolsRender_Click;
            // 
            // toolsSep
            // 
            toolsSep.Name = "toolsSep";
            toolsSep.Size = new System.Drawing.Size(171, 6);
            // 
            // toolsSettings
            // 
            toolsSettings.Name = "toolsSettings";
            toolsSettings.Size = new System.Drawing.Size(174, 22);
            toolsSettings.Text = "&Settings...";
            toolsSettings.Click += toolsSettings_Click;
            // 
            // helpMenu
            // 
            helpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { helpSyntax, helpSep, helpAbout });
            helpMenu.Name = "helpMenu";
            helpMenu.Size = new System.Drawing.Size(44, 20);
            helpMenu.Text = "&Help";
            // 
            // helpSyntax
            // 
            helpSyntax.Name = "helpSyntax";
            helpSyntax.Size = new System.Drawing.Size(173, 22);
            helpSyntax.Text = "Supported &Markup";
            helpSyntax.Click += helpSyntax_Click;
            // 
            // helpSep
            // 
            helpSep.Name = "helpSep";
            helpSep.Size = new System.Drawing.Size(170, 6);
            // 
            // helpAbout
            // 
            helpAbout.Name = "helpAbout";
            helpAbout.Size = new System.Drawing.Size(173, 22);
            helpAbout.Text = "&About";
            helpAbout.Click += helpAbout_Click;
            // 
            // _mainSplit
            // 
            _mainSplit.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _mainSplit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _mainSplit.Location = new System.Drawing.Point(0, 28);
            _mainSplit.Margin = new System.Windows.Forms.Padding(0);
            _mainSplit.Name = "_mainSplit";
            // 
            // _mainSplit.Panel1
            // 
            _mainSplit.Panel1.Controls.Add(_editorTextBox);
            _mainSplit.Panel1.Controls.Add(_lineNumberPanel);
            // 
            // _mainSplit.Panel2
            // 
            _mainSplit.Panel2.Controls.Add(_previewBrowser);
            _mainSplit.Size = new System.Drawing.Size(1031, 597);
            _mainSplit.SplitterDistance = 514;
            _mainSplit.SplitterWidth = 5;
            _mainSplit.TabIndex = 1;
            // 
            // _editorTextBox
            // 
            _editorTextBox.AcceptsTab = true;
            _editorTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            _editorTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            _editorTextBox.Font = new System.Drawing.Font("Consolas", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            _editorTextBox.HideSelection = false;
            _editorTextBox.Location = new System.Drawing.Point(0, 0);
            _editorTextBox.Margin = new System.Windows.Forms.Padding(0);
            _editorTextBox.Multiline = true;
            _editorTextBox.Name = "_editorTextBox";
            _editorTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            _editorTextBox.Size = new System.Drawing.Size(512, 595);
            _editorTextBox.TabIndex = 0;
            _editorTextBox.WordWrap = false;
            // 
            // _previewBrowser
            // 
            _previewBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            _previewBrowser.Location = new System.Drawing.Point(0, 0);
            _previewBrowser.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            _previewBrowser.Name = "_previewBrowser";
            _previewBrowser.Size = new System.Drawing.Size(510, 595);
            _previewBrowser.TabIndex = 0;
            // 
            // _statusStrip
            // 
            _statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _fileStatusLabel, _modifiedStatusLabel });
            _statusStrip.Location = new System.Drawing.Point(0, 625);
            _statusStrip.Name = "_statusStrip";
            _statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            _statusStrip.Size = new System.Drawing.Size(1031, 22);
            _statusStrip.TabIndex = 3;
            _statusStrip.Text = "_statusStrip";
            // 
            // _fileStatusLabel
            // 
            _fileStatusLabel.Name = "_fileStatusLabel";
            _fileStatusLabel.Size = new System.Drawing.Size(110, 17);
            _fileStatusLabel.Text = "Unsaved document";
            // 
            // _modifiedStatusLabel
            // 
            _modifiedStatusLabel.Name = "_modifiedStatusLabel";
            _modifiedStatusLabel.Size = new System.Drawing.Size(38, 17);
            _modifiedStatusLabel.Text = "Saved";
            // 
            // _previewTimer
            // 
            _previewTimer.Interval = 300;
            // 
            // MarkupEditor
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1031, 647);
            Controls.Add(_statusStrip);
            Controls.Add(_menuStrip);
            Controls.Add(_mainSplit);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = _menuStrip;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MinimumSize = new System.Drawing.Size(1047, 686);
            Name = "MarkupEditor";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Markup Editor";
            FormClosing += MarkupEditor_FormClosing;
            _menuStrip.ResumeLayout(false);
            _menuStrip.PerformLayout();
            _mainSplit.Panel1.ResumeLayout(false);
            _mainSplit.Panel1.PerformLayout();
            _mainSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_previewBrowser).EndInit();
            ((System.ComponentModel.ISupportInitialize)_mainSplit).EndInit();
            _mainSplit.ResumeLayout(false);
            _statusStrip.ResumeLayout(false);
            _statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip _menuStrip;
        private System.Windows.Forms.SplitContainer _mainSplit;
        private System.Windows.Forms.TextBox _editorTextBox;
        private Microsoft.Web.WebView2.WinForms.WebView2 _previewBrowser;
        private LineNumberPanel _lineNumberPanel;
        private System.Windows.Forms.StatusStrip _statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel _fileStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel _modifiedStatusLabel;
        private System.Windows.Forms.Timer _previewTimer;
        private System.Windows.Forms.ToolStripMenuItem fileMenu;
        private System.Windows.Forms.ToolStripMenuItem fileNew;
        private System.Windows.Forms.ToolStripMenuItem fileOpen;
        private System.Windows.Forms.ToolStripMenuItem fileReload;
        private System.Windows.Forms.ToolStripSeparator fileRecentSep;
        private System.Windows.Forms.ToolStripMenuItem fileRecentDocuments;
        private System.Windows.Forms.ToolStripSeparator fileSep1;
        private System.Windows.Forms.ToolStripMenuItem fileSave;
        private System.Windows.Forms.ToolStripMenuItem fileSaveAs;
        private System.Windows.Forms.ToolStripMenuItem filePrint;
        private System.Windows.Forms.ToolStripMenuItem fileExportMenu;
        private System.Windows.Forms.ToolStripMenuItem fileExportPdf;
        private System.Windows.Forms.ToolStripMenuItem fileExportTex;
        private System.Windows.Forms.ToolStripMenuItem fileExportHtml;
        private System.Windows.Forms.ToolStripSeparator fileSep2;
        private System.Windows.Forms.ToolStripMenuItem fileExit;
        private System.Windows.Forms.ToolStripMenuItem editMenu;
        private System.Windows.Forms.ToolStripMenuItem editUndo;
        private System.Windows.Forms.ToolStripMenuItem editRedo;
        private System.Windows.Forms.ToolStripSeparator editSep1;
        private System.Windows.Forms.ToolStripMenuItem editCut;
        private System.Windows.Forms.ToolStripMenuItem editCopy;
        private System.Windows.Forms.ToolStripMenuItem editPaste;
        private System.Windows.Forms.ToolStripSeparator editSepFind;
        private System.Windows.Forms.ToolStripMenuItem editFind;
        private System.Windows.Forms.ToolStripMenuItem editReplace;
        private System.Windows.Forms.ToolStripSeparator editSep2;
        private System.Windows.Forms.ToolStripMenuItem editSelectAll;
        private System.Windows.Forms.ToolStripMenuItem toolsMenu;
        private System.Windows.Forms.ToolStripMenuItem toolsRender;
        private System.Windows.Forms.ToolStripSeparator toolsSep;
        private System.Windows.Forms.ToolStripMenuItem toolsSettings;
        private System.Windows.Forms.ToolStripMenuItem helpMenu;
        private System.Windows.Forms.ToolStripMenuItem helpSyntax;
        private System.Windows.Forms.ToolStripSeparator helpSep;
        private System.Windows.Forms.ToolStripMenuItem helpAbout;
    }
}

