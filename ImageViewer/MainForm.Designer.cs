﻿namespace ImageViewer
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_OpenInExplorer = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_CopyToClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_AutoResizeWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_CopyFilePathToClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_CopyDirectoryPathToClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_ReloadDirectory = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_ResetCustomView = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_RunSnippingTool = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_ExitApplication = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.ContextMenuStrip = this.contextMenuStrip1;
            this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox.Location = new System.Drawing.Point(0, 0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(800, 450);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            this.pictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox_Paint);
            this.pictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseDown);
            this.pictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseMove);
            this.pictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseUp);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_OpenInExplorer,
            this.toolStripSeparator1,
            this.toolStripMenuItem_CopyToClipboard,
            this.toolStripMenuItem_AutoResizeWindow,
            this.toolStripSeparator4,
            this.toolStripMenuItem_CopyFilePathToClipboard,
            this.toolStripMenuItem_CopyDirectoryPathToClipboard,
            this.toolStripSeparator2,
            this.toolStripMenuItem_ReloadDirectory,
            this.toolStripMenuItem_ResetCustomView,
            this.toolStripMenuItem_RunSnippingTool,
            this.toolStripSeparator3,
            this.toolStripMenuItem_ExitApplication});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(269, 244);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // toolStripMenuItem_OpenInExplorer
            // 
            this.toolStripMenuItem_OpenInExplorer.Name = "toolStripMenuItem_OpenInExplorer";
            this.toolStripMenuItem_OpenInExplorer.Size = new System.Drawing.Size(268, 24);
            this.toolStripMenuItem_OpenInExplorer.Text = "エクスプローラーで開く";
            this.toolStripMenuItem_OpenInExplorer.Click += new System.EventHandler(this.ToolStripMenuItem_OpenInExplorer_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(265, 6);
            // 
            // toolStripMenuItem_CopyToClipboard
            // 
            this.toolStripMenuItem_CopyToClipboard.Name = "toolStripMenuItem_CopyToClipboard";
            this.toolStripMenuItem_CopyToClipboard.Size = new System.Drawing.Size(268, 24);
            this.toolStripMenuItem_CopyToClipboard.Text = "クリップボードにコピー";
            this.toolStripMenuItem_CopyToClipboard.Click += new System.EventHandler(this.ToolStripMenuItem_CopyToClipboard_Click);
            // 
            // toolStripMenuItem_AutoResizeWindow
            // 
            this.toolStripMenuItem_AutoResizeWindow.Name = "toolStripMenuItem_AutoResizeWindow";
            this.toolStripMenuItem_AutoResizeWindow.Size = new System.Drawing.Size(268, 24);
            this.toolStripMenuItem_AutoResizeWindow.Text = "ウィンドウ幅を調整して中央表示";
            this.toolStripMenuItem_AutoResizeWindow.Click += new System.EventHandler(this.ToolStripMenuItem_AutoResizeWindow_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(265, 6);
            // 
            // toolStripMenuItem_CopyFilePathToClipboard
            // 
            this.toolStripMenuItem_CopyFilePathToClipboard.Name = "toolStripMenuItem_CopyFilePathToClipboard";
            this.toolStripMenuItem_CopyFilePathToClipboard.Size = new System.Drawing.Size(268, 24);
            this.toolStripMenuItem_CopyFilePathToClipboard.Text = "ファイルパスをコピー";
            this.toolStripMenuItem_CopyFilePathToClipboard.Click += new System.EventHandler(this.ToolStripMenuItem_CopyFilePathToClipboard_Click);
            // 
            // toolStripMenuItem_CopyDirectoryPathToClipboard
            // 
            this.toolStripMenuItem_CopyDirectoryPathToClipboard.Name = "toolStripMenuItem_CopyDirectoryPathToClipboard";
            this.toolStripMenuItem_CopyDirectoryPathToClipboard.Size = new System.Drawing.Size(268, 24);
            this.toolStripMenuItem_CopyDirectoryPathToClipboard.Text = "フォルダパスをコピー";
            this.toolStripMenuItem_CopyDirectoryPathToClipboard.Click += new System.EventHandler(this.ToolStripMenuItem_CopyDirectoryPathToClipboard_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(265, 6);
            // 
            // toolStripMenuItem_ReloadDirectory
            // 
            this.toolStripMenuItem_ReloadDirectory.Name = "toolStripMenuItem_ReloadDirectory";
            this.toolStripMenuItem_ReloadDirectory.Size = new System.Drawing.Size(268, 24);
            this.toolStripMenuItem_ReloadDirectory.Text = "フォルダの内容を再読込み";
            this.toolStripMenuItem_ReloadDirectory.Click += new System.EventHandler(this.ToolStripMenuItem_ReloadDirectory_Click);
            // 
            // toolStripMenuItem_ResetCustomView
            // 
            this.toolStripMenuItem_ResetCustomView.Name = "toolStripMenuItem_ResetCustomView";
            this.toolStripMenuItem_ResetCustomView.Size = new System.Drawing.Size(268, 24);
            this.toolStripMenuItem_ResetCustomView.Text = "カスタムビューを解除";
            this.toolStripMenuItem_ResetCustomView.Click += new System.EventHandler(this.ToolStripMenuItem_ResetCustomView_Click);
            // 
            // toolStripMenuItem_RunSnippingTool
            // 
            this.toolStripMenuItem_RunSnippingTool.Name = "toolStripMenuItem_RunSnippingTool";
            this.toolStripMenuItem_RunSnippingTool.Size = new System.Drawing.Size(268, 24);
            this.toolStripMenuItem_RunSnippingTool.Text = "Snipping Toolを起動";
            this.toolStripMenuItem_RunSnippingTool.Click += new System.EventHandler(this.ToolStripMenuItem_RunSnippingTool_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(265, 6);
            // 
            // toolStripMenuItem_ExitApplication
            // 
            this.toolStripMenuItem_ExitApplication.Name = "toolStripMenuItem_ExitApplication";
            this.toolStripMenuItem_ExitApplication.Size = new System.Drawing.Size(268, 24);
            this.toolStripMenuItem_ExitApplication.Text = "終了";
            this.toolStripMenuItem_ExitApplication.Click += new System.EventHandler(this.ToolStripMenuItem_ExitApplication_Click);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.pictureBox);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(100, 100);
            this.Name = "MainForm";
            this.Text = "ImageViewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MainForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseDown);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.MainForm_PreviewKeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_CopyFilePathToClipboard;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_OpenInExplorer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_CopyDirectoryPathToClipboard;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ReloadDirectory;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_RunSnippingTool;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ExitApplication;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ResetCustomView;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_AutoResizeWindow;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_CopyToClipboard;
    }
}

