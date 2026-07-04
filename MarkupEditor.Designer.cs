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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MarkupEditor));
            this._menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.fileNew = new System.Windows.Forms.ToolStripMenuItem();
            this.fileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.fileRecentSep = new System.Windows.Forms.ToolStripSeparator();
            this.fileRecentDocuments = new System.Windows.Forms.ToolStripMenuItem();
            this.fileSep1 = new System.Windows.Forms.ToolStripSeparator();
            this.fileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.fileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.fileSep2 = new System.Windows.Forms.ToolStripSeparator();
            this.fileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.editMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.editUndo = new System.Windows.Forms.ToolStripMenuItem();
            this.editRedo = new System.Windows.Forms.ToolStripMenuItem();
            this.editSep1 = new System.Windows.Forms.ToolStripSeparator();
            this.editCut = new System.Windows.Forms.ToolStripMenuItem();
            this.editCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.editPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.editSepFind = new System.Windows.Forms.ToolStripSeparator();
            this.editFind = new System.Windows.Forms.ToolStripMenuItem();
            this.editReplace = new System.Windows.Forms.ToolStripMenuItem();
            this.editSep2 = new System.Windows.Forms.ToolStripSeparator();
            this.editSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsRender = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsSep = new System.Windows.Forms.ToolStripSeparator();
            this.settingsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this._livePreviewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._wordWrapMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsSep = new System.Windows.Forms.ToolStripSeparator();
            this.fontIncrease = new System.Windows.Forms.ToolStripMenuItem();
            this.fontDecrease = new System.Windows.Forms.ToolStripMenuItem();
            this.fontReset = new System.Windows.Forms.ToolStripMenuItem();
            this.helpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.helpSyntax = new System.Windows.Forms.ToolStripMenuItem();
            this.helpSep = new System.Windows.Forms.ToolStripSeparator();
            this.helpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this._mainSplit = new System.Windows.Forms.SplitContainer();
            this._editorTextBox = new System.Windows.Forms.TextBox();
            this._previewBrowser = new System.Windows.Forms.WebBrowser();
            this._statusStrip = new System.Windows.Forms.StatusStrip();
            this._fileStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._modifiedStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._previewTimer = new System.Windows.Forms.Timer(this.components);
            this._menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._mainSplit)).BeginInit();
            this._mainSplit.Panel1.SuspendLayout();
            this._mainSplit.Panel2.SuspendLayout();
            this._mainSplit.SuspendLayout();
            this._statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _menuStrip
            // 
            this._menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenu,
            this.editMenu,
            this.toolsMenu,
            this.helpMenu});
            this._menuStrip.Location = new System.Drawing.Point(0, 0);
            this._menuStrip.Name = "_menuStrip";
            this._menuStrip.Size = new System.Drawing.Size(884, 24);
            this._menuStrip.TabIndex = 0;
            this._menuStrip.Text = "_menuStrip";
            // 
            // fileMenu
            // 
            this.fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileNew,
            this.fileOpen,
            this.fileRecentSep,
            this.fileRecentDocuments,
            this.fileSep1,
            this.fileSave,
            this.fileSaveAs,
            this.fileSep2,
            this.fileExit});
            this.fileMenu.Name = "fileMenu";
            this.fileMenu.Size = new System.Drawing.Size(37, 20);
            this.fileMenu.Text = "&File";
            // 
            // fileNew
            // 
            this.fileNew.Name = "fileNew";
            this.fileNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.fileNew.Size = new System.Drawing.Size(195, 22);
            this.fileNew.Text = "&New";
            this.fileNew.Click += new System.EventHandler(this.fileNew_Click);
            // 
            // fileOpen
            // 
            this.fileOpen.Name = "fileOpen";
            this.fileOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.fileOpen.Size = new System.Drawing.Size(195, 22);
            this.fileOpen.Text = "&Open...";
            this.fileOpen.Click += new System.EventHandler(this.fileOpen_Click);
            // 
            // fileRecentSep
            // 
            this.fileRecentSep.Name = "fileRecentSep";
            this.fileRecentSep.Size = new System.Drawing.Size(192, 6);
            // 
            // fileRecentDocuments
            // 
            this.fileRecentDocuments.Name = "fileRecentDocuments";
            this.fileRecentDocuments.Size = new System.Drawing.Size(195, 22);
            this.fileRecentDocuments.Text = "Recent &Documents";
            // 
            // fileSep1
            // 
            this.fileSep1.Name = "fileSep1";
            this.fileSep1.Size = new System.Drawing.Size(192, 6);
            // 
            // fileSave
            // 
            this.fileSave.Name = "fileSave";
            this.fileSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.fileSave.Size = new System.Drawing.Size(195, 22);
            this.fileSave.Text = "&Save";
            this.fileSave.Click += new System.EventHandler(this.fileSave_Click);
            // 
            // fileSaveAs
            // 
            this.fileSaveAs.Name = "fileSaveAs";
            this.fileSaveAs.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.fileSaveAs.Size = new System.Drawing.Size(195, 22);
            this.fileSaveAs.Text = "Save &As...";
            this.fileSaveAs.Click += new System.EventHandler(this.fileSaveAs_Click);
            // 
            // fileSep2
            // 
            this.fileSep2.Name = "fileSep2";
            this.fileSep2.Size = new System.Drawing.Size(192, 6);
            // 
            // fileExit
            // 
            this.fileExit.Name = "fileExit";
            this.fileExit.Size = new System.Drawing.Size(195, 22);
            this.fileExit.Text = "E&xit";
            this.fileExit.Click += new System.EventHandler(this.fileExit_Click);
            // 
            // editMenu
            // 
            this.editMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editUndo,
            this.editRedo,
            this.editSep1,
            this.editCut,
            this.editCopy,
            this.editPaste,
            this.editSepFind,
            this.editFind,
            this.editReplace,
            this.editSep2,
            this.editSelectAll});
            this.editMenu.Name = "editMenu";
            this.editMenu.Size = new System.Drawing.Size(39, 20);
            this.editMenu.Text = "&Edit";
            // 
            // editUndo
            // 
            this.editUndo.Name = "editUndo";
            this.editUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.editUndo.Size = new System.Drawing.Size(167, 22);
            this.editUndo.Text = "&Undo";
            this.editUndo.Click += new System.EventHandler(this.editUndo_Click);
            // 
            // editRedo
            // 
            this.editRedo.Name = "editRedo";
            this.editRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.editRedo.Size = new System.Drawing.Size(167, 22);
            this.editRedo.Text = "&Redo";
            this.editRedo.Click += new System.EventHandler(this.editRedo_Click);
            // 
            // editSep1
            // 
            this.editSep1.Name = "editSep1";
            this.editSep1.Size = new System.Drawing.Size(164, 6);
            // 
            // editCut
            // 
            this.editCut.Name = "editCut";
            this.editCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.editCut.Size = new System.Drawing.Size(167, 22);
            this.editCut.Text = "Cu&t";
            this.editCut.Click += new System.EventHandler(this.editCut_Click);
            // 
            // editCopy
            // 
            this.editCopy.Name = "editCopy";
            this.editCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.editCopy.Size = new System.Drawing.Size(167, 22);
            this.editCopy.Text = "&Copy";
            this.editCopy.Click += new System.EventHandler(this.editCopy_Click);
            // 
            // editPaste
            // 
            this.editPaste.Name = "editPaste";
            this.editPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.editPaste.Size = new System.Drawing.Size(167, 22);
            this.editPaste.Text = "&Paste";
            this.editPaste.Click += new System.EventHandler(this.editPaste_Click);
            // 
            // editSepFind
            // 
            this.editSepFind.Name = "editSepFind";
            this.editSepFind.Size = new System.Drawing.Size(164, 6);
            // 
            // editFind
            // 
            this.editFind.Name = "editFind";
            this.editFind.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.editFind.Size = new System.Drawing.Size(167, 22);
            this.editFind.Text = "&Find...";
            this.editFind.Click += new System.EventHandler(this.editFind_Click);
            // 
            // editReplace
            // 
            this.editReplace.Name = "editReplace";
            this.editReplace.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.editReplace.Size = new System.Drawing.Size(167, 22);
            this.editReplace.Text = "&Replace...";
            this.editReplace.Click += new System.EventHandler(this.editReplace_Click);
            // 
            // editSep2
            // 
            this.editSep2.Name = "editSep2";
            this.editSep2.Size = new System.Drawing.Size(164, 6);
            // 
            // editSelectAll
            // 
            this.editSelectAll.Name = "editSelectAll";
            this.editSelectAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.editSelectAll.Size = new System.Drawing.Size(167, 22);
            this.editSelectAll.Text = "Select &All";
            this.editSelectAll.Click += new System.EventHandler(this.editSelectAll_Click);
            // 
            // toolsMenu
            // 
            this.toolsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsRender,
            this.toolsSep,
            this.settingsMenu});
            this.toolsMenu.Name = "toolsMenu";
            this.toolsMenu.Size = new System.Drawing.Size(47, 20);
            this.toolsMenu.Text = "&Tools";
            // 
            // toolsRender
            // 
            this.toolsRender.Name = "toolsRender";
            this.toolsRender.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.toolsRender.Size = new System.Drawing.Size(180, 22);
            this.toolsRender.Text = "&Render Preview";
            this.toolsRender.Click += new System.EventHandler(this.toolsRender_Click);
            // 
            // toolsSep
            // 
            this.toolsSep.Name = "toolsSep";
            this.toolsSep.Size = new System.Drawing.Size(177, 6);
            // 
            // settingsMenu
            // 
            this.settingsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._livePreviewMenuItem,
            this._wordWrapMenuItem,
            this.settingsSep,
            this.fontIncrease,
            this.fontDecrease,
            this.fontReset});
            this.settingsMenu.Name = "settingsMenu";
            this.settingsMenu.Size = new System.Drawing.Size(180, 22);
            this.settingsMenu.Text = "&Settings";
            // 
            // _livePreviewMenuItem
            // 
            this._livePreviewMenuItem.Checked = true;
            this._livePreviewMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this._livePreviewMenuItem.Name = "_livePreviewMenuItem";
            this._livePreviewMenuItem.Size = new System.Drawing.Size(264, 22);
            this._livePreviewMenuItem.Text = "&Live Preview";
            this._livePreviewMenuItem.Click += new System.EventHandler(this._livePreviewMenuItem_Click);
            // 
            // _wordWrapMenuItem
            // 
            this._wordWrapMenuItem.Name = "_wordWrapMenuItem";
            this._wordWrapMenuItem.Size = new System.Drawing.Size(264, 22);
            this._wordWrapMenuItem.Text = "&Word Wrap";
            this._wordWrapMenuItem.Click += new System.EventHandler(this._wordWrapMenuItem_Click);
            // 
            // settingsSep
            // 
            this.settingsSep.Name = "settingsSep";
            this.settingsSep.Size = new System.Drawing.Size(261, 6);
            // 
            // fontIncrease
            // 
            this.fontIncrease.Name = "fontIncrease";
            this.fontIncrease.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Oemplus)));
            this.fontIncrease.Size = new System.Drawing.Size(264, 22);
            this.fontIncrease.Text = "Increase Font Size";
            this.fontIncrease.Click += new System.EventHandler(this.fontIncrease_Click);
            // 
            // fontDecrease
            // 
            this.fontDecrease.Name = "fontDecrease";
            this.fontDecrease.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.OemMinus)));
            this.fontDecrease.Size = new System.Drawing.Size(264, 22);
            this.fontDecrease.Text = "Decrease Font Size";
            this.fontDecrease.Click += new System.EventHandler(this.fontDecrease_Click);
            // 
            // fontReset
            // 
            this.fontReset.Name = "fontReset";
            this.fontReset.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D0)));
            this.fontReset.Size = new System.Drawing.Size(264, 22);
            this.fontReset.Text = "Reset Font Size";
            this.fontReset.Click += new System.EventHandler(this.fontReset_Click);
            // 
            // helpMenu
            // 
            this.helpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpSyntax,
            this.helpSep,
            this.helpAbout});
            this.helpMenu.Name = "helpMenu";
            this.helpMenu.Size = new System.Drawing.Size(44, 20);
            this.helpMenu.Text = "&Help";
            // 
            // helpSyntax
            // 
            this.helpSyntax.Name = "helpSyntax";
            this.helpSyntax.Size = new System.Drawing.Size(173, 22);
            this.helpSyntax.Text = "Supported &Markup";
            this.helpSyntax.Click += new System.EventHandler(this.helpSyntax_Click);
            // 
            // helpSep
            // 
            this.helpSep.Name = "helpSep";
            this.helpSep.Size = new System.Drawing.Size(170, 6);
            // 
            // helpAbout
            // 
            this.helpAbout.Name = "helpAbout";
            this.helpAbout.Size = new System.Drawing.Size(173, 22);
            this.helpAbout.Text = "&About";
            this.helpAbout.Click += new System.EventHandler(this.helpAbout_Click);
            // 
            // _mainSplit
            // 
            this._mainSplit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._mainSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this._mainSplit.Location = new System.Drawing.Point(0, 24);
            this._mainSplit.Name = "_mainSplit";
            // 
            // _mainSplit.Panel1
            // 
            this._mainSplit.Panel1.Controls.Add(this._editorTextBox);
            // 
            // _mainSplit.Panel2
            // 
            this._mainSplit.Panel2.Controls.Add(this._previewBrowser);
            this._mainSplit.Size = new System.Drawing.Size(884, 537);
            this._mainSplit.SplitterDistance = 442;
            this._mainSplit.TabIndex = 1;
            // 
            // _editorTextBox
            // 
            this._editorTextBox.AcceptsTab = true;
            this._editorTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._editorTextBox.Font = new System.Drawing.Font("Consolas", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._editorTextBox.HideSelection = false;
            this._editorTextBox.Location = new System.Drawing.Point(0, 0);
            this._editorTextBox.Multiline = true;
            this._editorTextBox.Name = "_editorTextBox";
            this._editorTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._editorTextBox.Size = new System.Drawing.Size(440, 535);
            this._editorTextBox.TabIndex = 0;
            this._editorTextBox.WordWrap = false;
            // 
            // _previewBrowser
            // 
            this._previewBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this._previewBrowser.Location = new System.Drawing.Point(0, 0);
            this._previewBrowser.Name = "_previewBrowser";
            this._previewBrowser.ScriptErrorsSuppressed = true;
            this._previewBrowser.Size = new System.Drawing.Size(436, 535);
            this._previewBrowser.TabIndex = 0;
            // 
            // _statusStrip
            // 
            this._statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fileStatusLabel,
            this._modifiedStatusLabel});
            this._statusStrip.Location = new System.Drawing.Point(0, 539);
            this._statusStrip.Name = "_statusStrip";
            this._statusStrip.Size = new System.Drawing.Size(884, 22);
            this._statusStrip.TabIndex = 3;
            this._statusStrip.Text = "_statusStrip";
            // 
            // _fileStatusLabel
            // 
            this._fileStatusLabel.Name = "_fileStatusLabel";
            this._fileStatusLabel.Size = new System.Drawing.Size(110, 17);
            this._fileStatusLabel.Text = "Unsaved document";
            // 
            // _modifiedStatusLabel
            // 
            this._modifiedStatusLabel.Name = "_modifiedStatusLabel";
            this._modifiedStatusLabel.Size = new System.Drawing.Size(38, 17);
            this._modifiedStatusLabel.Text = "Saved";
            // 
            // _previewTimer
            // 
            this._previewTimer.Interval = 300;
            // 
            // MarkupEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 561);
            this.Controls.Add(this._statusStrip);
            this.Controls.Add(this._mainSplit);
            this.Controls.Add(this._menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this._menuStrip;
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.Name = "MarkupEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Markup Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MarkupEditor_FormClosing);
            this._menuStrip.ResumeLayout(false);
            this._menuStrip.PerformLayout();
            this._mainSplit.Panel1.ResumeLayout(false);
            this._mainSplit.Panel1.PerformLayout();
            this._mainSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._mainSplit)).EndInit();
            this._mainSplit.ResumeLayout(false);
            this._statusStrip.ResumeLayout(false);
            this._statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip _menuStrip;
        private System.Windows.Forms.SplitContainer _mainSplit;
        private System.Windows.Forms.TextBox _editorTextBox;
        private System.Windows.Forms.WebBrowser _previewBrowser;
        private System.Windows.Forms.StatusStrip _statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel _fileStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel _modifiedStatusLabel;
        private System.Windows.Forms.Timer _previewTimer;
        private System.Windows.Forms.ToolStripMenuItem _livePreviewMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _wordWrapMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileMenu;
        private System.Windows.Forms.ToolStripMenuItem fileNew;
        private System.Windows.Forms.ToolStripMenuItem fileOpen;
        private System.Windows.Forms.ToolStripSeparator fileRecentSep;
        private System.Windows.Forms.ToolStripMenuItem fileRecentDocuments;
        private System.Windows.Forms.ToolStripSeparator fileSep1;
        private System.Windows.Forms.ToolStripMenuItem fileSave;
        private System.Windows.Forms.ToolStripMenuItem fileSaveAs;
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
        private System.Windows.Forms.ToolStripMenuItem settingsMenu;
        private System.Windows.Forms.ToolStripMenuItem helpMenu;
        private System.Windows.Forms.ToolStripMenuItem helpSyntax;
        private System.Windows.Forms.ToolStripSeparator helpSep;
        private System.Windows.Forms.ToolStripMenuItem helpAbout;
        private System.Windows.Forms.ToolStripSeparator settingsSep;
        private System.Windows.Forms.ToolStripMenuItem fontIncrease;
        private System.Windows.Forms.ToolStripMenuItem fontDecrease;
        private System.Windows.Forms.ToolStripMenuItem fontReset;
    }
}

