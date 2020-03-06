namespace ImageViewer
{
    partial class TreeForm
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
            this.treeView = new System.Windows.Forms.TreeView();
            this.listView = new System.Windows.Forms.ListView();
            this.listMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ItemuMenu_ChangeName = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonArrange = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.buttonReload = new System.Windows.Forms.Button();
            this.treeMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ToolStripMenuItem_rename = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_addIV = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_removeIV = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_upLevel = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_downLevel = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_upTreeLevel = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_downTreeLevel = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonReLevel = new System.Windows.Forms.Button();
            this.buttonToggleTreeViewMode = new System.Windows.Forms.Button();
            this.listMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.treeMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(420, 347);
            this.treeView.TabIndex = 0;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            this.treeView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView_MouseDown);
            // 
            // listView
            // 
            this.listView.ContextMenuStrip = this.listMenu;
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(0, 0);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(262, 347);
            this.listView.TabIndex = 1;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listView_ItemSelectionChanged);
            // 
            // listMenu
            // 
            this.listMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ItemuMenu_ChangeName});
            this.listMenu.Name = "listMenu";
            this.listMenu.Size = new System.Drawing.Size(132, 26);
            // 
            // ItemuMenu_ChangeName
            // 
            this.ItemuMenu_ChangeName.Name = "ItemuMenu_ChangeName";
            this.ItemuMenu_ChangeName.Size = new System.Drawing.Size(131, 22);
            this.ItemuMenu_ChangeName.Text = "名前を変更";
            this.ItemuMenu_ChangeName.Click += new System.EventHandler(this.ItemuMenu_ChangeName_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.Location = new System.Drawing.Point(592, 365);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(82, 30);
            this.buttonClose.TabIndex = 3;
            this.buttonClose.Text = "閉じる";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonArrange
            // 
            this.buttonArrange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonArrange.Location = new System.Drawing.Point(316, 365);
            this.buttonArrange.Name = "buttonArrange";
            this.buttonArrange.Size = new System.Drawing.Size(82, 30);
            this.buttonArrange.TabIndex = 4;
            this.buttonArrange.Text = "整理";
            this.buttonArrange.UseVisualStyleBackColor = true;
            this.buttonArrange.Click += new System.EventHandler(this.buttonArrange_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listView);
            this.splitContainer1.Size = new System.Drawing.Size(686, 347);
            this.splitContainer1.SplitterDistance = 420;
            this.splitContainer1.TabIndex = 5;
            // 
            // buttonReload
            // 
            this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonReload.Location = new System.Drawing.Point(12, 365);
            this.buttonReload.Name = "buttonReload";
            this.buttonReload.Size = new System.Drawing.Size(82, 30);
            this.buttonReload.TabIndex = 6;
            this.buttonReload.Text = "リロード";
            this.buttonReload.UseVisualStyleBackColor = true;
            this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
            // 
            // treeMenu
            // 
            this.treeMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_rename,
            this.toolStripSeparator6,
            this.ToolStripMenuItem_addIV,
            this.ToolStripMenuItem_removeIV,
            this.toolStripSeparator1,
            this.ToolStripMenuItem_upLevel,
            this.ToolStripMenuItem_downLevel,
            this.toolStripSeparator7,
            this.ToolStripMenuItem_upTreeLevel,
            this.ToolStripMenuItem_downTreeLevel});
            this.treeMenu.Name = "treeMenu";
            this.treeMenu.Size = new System.Drawing.Size(193, 176);
            // 
            // ToolStripMenuItem_rename
            // 
            this.ToolStripMenuItem_rename.Name = "ToolStripMenuItem_rename";
            this.ToolStripMenuItem_rename.Size = new System.Drawing.Size(192, 22);
            this.ToolStripMenuItem_rename.Text = "名前を変更";
            this.ToolStripMenuItem_rename.Click += new System.EventHandler(this.ToolStripMenuItem_rename_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(189, 6);
            // 
            // ToolStripMenuItem_addIV
            // 
            this.ToolStripMenuItem_addIV.Name = "ToolStripMenuItem_addIV";
            this.ToolStripMenuItem_addIV.Size = new System.Drawing.Size(192, 22);
            this.ToolStripMenuItem_addIV.Text = "階層を追加";
            this.ToolStripMenuItem_addIV.Click += new System.EventHandler(this.ToolStripMenuItem_addIV_Click);
            // 
            // ToolStripMenuItem_removeIV
            // 
            this.ToolStripMenuItem_removeIV.Name = "ToolStripMenuItem_removeIV";
            this.ToolStripMenuItem_removeIV.Size = new System.Drawing.Size(192, 22);
            this.ToolStripMenuItem_removeIV.Text = "階層を削除（.ivのみ）";
            this.ToolStripMenuItem_removeIV.Click += new System.EventHandler(this.ToolStripMenuItem_removeIV_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(189, 6);
            // 
            // ToolStripMenuItem_upLevel
            // 
            this.ToolStripMenuItem_upLevel.Name = "ToolStripMenuItem_upLevel";
            this.ToolStripMenuItem_upLevel.Size = new System.Drawing.Size(192, 22);
            this.ToolStripMenuItem_upLevel.Tag = "up";
            this.ToolStripMenuItem_upLevel.Text = "階層を上げる";
            this.ToolStripMenuItem_upLevel.Click += new System.EventHandler(this.ToolStripMenuItem_upLevel_Click);
            // 
            // ToolStripMenuItem_downLevel
            // 
            this.ToolStripMenuItem_downLevel.Name = "ToolStripMenuItem_downLevel";
            this.ToolStripMenuItem_downLevel.Size = new System.Drawing.Size(192, 22);
            this.ToolStripMenuItem_downLevel.Tag = "down";
            this.ToolStripMenuItem_downLevel.Text = "階層を下げる";
            this.ToolStripMenuItem_downLevel.Click += new System.EventHandler(this.ToolStripMenuItem_upLevel_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(189, 6);
            // 
            // ToolStripMenuItem_upTreeLevel
            // 
            this.ToolStripMenuItem_upTreeLevel.Name = "ToolStripMenuItem_upTreeLevel";
            this.ToolStripMenuItem_upTreeLevel.Size = new System.Drawing.Size(192, 22);
            this.ToolStripMenuItem_upTreeLevel.Tag = "treeUp";
            this.ToolStripMenuItem_upTreeLevel.Text = "サブツリーの階層を上げる";
            this.ToolStripMenuItem_upTreeLevel.Click += new System.EventHandler(this.ToolStripMenuItem_upLevel_Click);
            // 
            // ToolStripMenuItem_downTreeLevel
            // 
            this.ToolStripMenuItem_downTreeLevel.Name = "ToolStripMenuItem_downTreeLevel";
            this.ToolStripMenuItem_downTreeLevel.Size = new System.Drawing.Size(192, 22);
            this.ToolStripMenuItem_downTreeLevel.Tag = "treeDown";
            this.ToolStripMenuItem_downTreeLevel.Text = "サブツリーの階層を下げる";
            this.ToolStripMenuItem_downTreeLevel.Click += new System.EventHandler(this.ToolStripMenuItem_upLevel_Click);
            // 
            // buttonReLevel
            // 
            this.buttonReLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonReLevel.Location = new System.Drawing.Point(404, 365);
            this.buttonReLevel.Name = "buttonReLevel";
            this.buttonReLevel.Size = new System.Drawing.Size(118, 30);
            this.buttonReLevel.TabIndex = 8;
            this.buttonReLevel.Text = "ファイル名を正規化";
            this.buttonReLevel.UseVisualStyleBackColor = true;
            this.buttonReLevel.Click += new System.EventHandler(this.buttonReLevel_Click);
            // 
            // buttonToggleTreeViewMode
            // 
            this.buttonToggleTreeViewMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonToggleTreeViewMode.Location = new System.Drawing.Point(109, 365);
            this.buttonToggleTreeViewMode.Name = "buttonToggleTreeViewMode";
            this.buttonToggleTreeViewMode.Size = new System.Drawing.Size(160, 30);
            this.buttonToggleTreeViewMode.TabIndex = 9;
            this.buttonToggleTreeViewMode.Text = "ツリー表示切替";
            this.buttonToggleTreeViewMode.UseVisualStyleBackColor = true;
            this.buttonToggleTreeViewMode.Click += new System.EventHandler(this.buttonToggleTreeViewMode_Click);
            // 
            // TreeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(686, 401);
            this.Controls.Add(this.buttonToggleTreeViewMode);
            this.Controls.Add(this.buttonReLevel);
            this.Controls.Add(this.buttonReload);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.buttonArrange);
            this.Controls.Add(this.buttonClose);
            this.KeyPreview = true;
            this.Name = "TreeForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "仮想ツリー管理";
            this.Load += new System.EventHandler(this.TreeForm_Load);
            this.Shown += new System.EventHandler(this.TreeForm_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TreeForm_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TreeForm_KeyPress);
            this.listMenu.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.treeMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonArrange;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button buttonReload;
        private System.Windows.Forms.ContextMenuStrip treeMenu;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_rename;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_downLevel;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_upLevel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_upTreeLevel;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_downTreeLevel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.Button buttonReLevel;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_addIV;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_removeIV;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Button buttonToggleTreeViewMode;
        private System.Windows.Forms.ContextMenuStrip listMenu;
        private System.Windows.Forms.ToolStripMenuItem ItemuMenu_ChangeName;
    }
}
