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
            this.toolStripMenuItem_watchDirectory = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_rotateRight = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_rotateLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
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
            this.toolStripMenuItem_SetRatio100 = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.ContextMenuStrip = this.contextMenuStrip1;
            this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox.Location = new System.Drawing.Point(0, 0);
            this.pictureBox.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(600, 360);
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
            this.toolStripMenuItem_watchDirectory,
            this.toolStripSeparator5,
            this.toolStripMenuItem_rotateRight,
            this.toolStripMenuItem_rotateLeft,
            this.toolStripSeparator6,
            this.toolStripMenuItem_OpenInExplorer,
            this.toolStripSeparator1,
            this.toolStripMenuItem_CopyToClipboard,
            this.toolStripMenuItem_AutoResizeWindow,
            this.toolStripMenuItem_SetRatio100,
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
            this.contextMenuStrip1.Size = new System.Drawing.Size(227, 348);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // toolStripMenuItem_watchDirectory
            // 
            this.toolStripMenuItem_watchDirectory.Name = "toolStripMenuItem_watchDirectory";
            this.toolStripMenuItem_watchDirectory.Size = new System.Drawing.Size(226, 22);
            this.toolStripMenuItem_watchDirectory.Text = "フォルダを監視";
            this.toolStripMenuItem_watchDirectory.Click += new System.EventHandler(this.ToolStripMenuItem_watchDirectory_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(223, 6);
            // 
            // toolStripMenuItem_rotateRight
            // 
            this.toolStripMenuItem_rotateRight.Name = "toolStripMenuItem_rotateRight";
            this.toolStripMenuItem_rotateRight.Size = new System.Drawing.Size(226, 22);
            this.toolStripMenuItem_rotateRight.Text = "右に回転";
            this.toolStripMenuItem_rotateRight.Click += new System.EventHandler(this.ToolStripMenuItem_rotateRight_Click);
            // 
            // toolStripMenuItem_rotateLeft
            // 
            this.toolStripMenuItem_rotateLeft.Name = "toolStripMenuItem_rotateLeft";
            this.toolStripMenuItem_rotateLeft.Size = new System.Drawing.Size(226, 22);
            this.toolStripMenuItem_rotateLeft.Text = "左に回転";
            this.toolStripMenuItem_rotateLeft.Click += new System.EventHandler(this.ToolStripMenuItem_rotateLeft_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(223, 6);
            // 
            // toolStripMenuItem_OpenInExplorer
            // 
            this.toolStripMenuItem_OpenInExplorer.Name = "toolStripMenuItem_OpenInExplorer";
            this.toolStripMenuItem_OpenInExplorer.Size = new System.Drawing.Size(226, 22);
            this.toolStripMenuItem_OpenInExplorer.Text = "エクスプローラーで開く";
            this.toolStripMenuItem_OpenInExplorer.Click += new System.EventHandler(this.ToolStripMenuItem_OpenInExplorer_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(223, 6);
            // 
            // toolStripMenuItem_CopyToClipboard
            // 
            this.toolStripMenuItem_CopyToClipboard.Name = "toolStripMenuItem_CopyToClipboard";
            this.toolStripMenuItem_CopyToClipboard.Size = new System.Drawing.Size(226, 22);
            this.toolStripMenuItem_CopyToClipboard.Text = "クリップボードにコピー";
            this.toolStripMenuItem_CopyToClipboard.Click += new System.EventHandler(this.ToolStripMenuItem_CopyToClipboard_Click);
            // 
            // toolStripMenuItem_AutoResizeWindow
            // 
            this.toolStripMenuItem_AutoResizeWindow.Name = "toolStripMenuItem_AutoResizeWindow";
            this.toolStripMenuItem_AutoResizeWindow.Size = new System.Drawing.Size(226, 22);
            this.toolStripMenuItem_AutoResizeWindow.Text = "ウィンドウ幅を調整して中央表示";
            this.toolStripMenuItem_AutoResizeWindow.Click += new System.EventHandler(this.ToolStripMenuItem_AutoResizeWindow_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(223, 6);
            // 
            // toolStripMenuItem_CopyFilePathToClipboard
            // 
            this.toolStripMenuItem_CopyFilePathToClipboard.Name = "toolStripMenuItem_CopyFilePathToClipboard";
            this.toolStripMenuItem_CopyFilePathToClipboard.Size = new System.Drawing.Size(226, 22);
            this.toolStripMenuItem_CopyFilePathToClipboard.Text = "ファイルパスをコピー";
            this.toolStripMenuItem_CopyFilePathToClipboard.Click += new System.EventHandler(this.ToolStripMenuItem_CopyFilePathToClipboard_Click);
            // 
            // toolStripMenuItem_CopyDirectoryPathToClipboard
            // 
            this.toolStripMenuItem_CopyDirectoryPathToClipboard.Name = "toolStripMenuItem_CopyDirectoryPathToClipboard";
            this.toolStripMenuItem_CopyDirectoryPathToClipboard.Size = new System.Drawing.Size(226, 22);
            this.toolStripMenuItem_CopyDirectoryPathToClipboard.Text = "フォルダパスをコピー";
            this.toolStripMenuItem_CopyDirectoryPathToClipboard.Click += new System.EventHandler(this.ToolStripMenuItem_CopyDirectoryPathToClipboard_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(223, 6);
            // 
            // toolStripMenuItem_ReloadDirectory
            // 
            this.toolStripMenuItem_ReloadDirectory.Name = "toolStripMenuItem_ReloadDirectory";
            this.toolStripMenuItem_ReloadDirectory.Size = new System.Drawing.Size(226, 22);
            this.toolStripMenuItem_ReloadDirectory.Text = "フォルダの内容を再読込み";
            this.toolStripMenuItem_ReloadDirectory.Click += new System.EventHandler(this.ToolStripMenuItem_ReloadDirectory_Click);
            // 
            // toolStripMenuItem_ResetCustomView
            // 
            this.toolStripMenuItem_ResetCustomView.Name = "toolStripMenuItem_ResetCustomView";
            this.toolStripMenuItem_ResetCustomView.Size = new System.Drawing.Size(226, 22);
            this.toolStripMenuItem_ResetCustomView.Text = "カスタムビューを解除";
            this.toolStripMenuItem_ResetCustomView.Click += new System.EventHandler(this.ToolStripMenuItem_ResetCustomView_Click);
            // 
            // toolStripMenuItem_RunSnippingTool
            // 
            this.toolStripMenuItem_RunSnippingTool.Name = "toolStripMenuItem_RunSnippingTool";
            this.toolStripMenuItem_RunSnippingTool.Size = new System.Drawing.Size(226, 22);
            this.toolStripMenuItem_RunSnippingTool.Text = "Snipping Toolを起動";
            this.toolStripMenuItem_RunSnippingTool.Click += new System.EventHandler(this.ToolStripMenuItem_RunSnippingTool_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(223, 6);
            // 
            // toolStripMenuItem_ExitApplication
            // 
            this.toolStripMenuItem_ExitApplication.Name = "toolStripMenuItem_ExitApplication";
            this.toolStripMenuItem_ExitApplication.Size = new System.Drawing.Size(226, 22);
            this.toolStripMenuItem_ExitApplication.Text = "終了";
            this.toolStripMenuItem_ExitApplication.Click += new System.EventHandler(this.ToolStripMenuItem_ExitApplication_Click);
            // 
            // toolStripMenuItem_SetRatio100
            // 
            this.toolStripMenuItem_SetRatio100.Name = "toolStripMenuItem_SetRatio100";
            this.toolStripMenuItem_SetRatio100.Size = new System.Drawing.Size(226, 22);
            this.toolStripMenuItem_SetRatio100.Text = "拡大率を100%に設定";
            this.toolStripMenuItem_SetRatio100.Click += new System.EventHandler(this.ToolStripMenuItem_SetRatio100_Click);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(600, 360);
            this.Controls.Add(this.pictureBox);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(79, 88);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
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
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_watchDirectory;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_rotateRight;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_rotateLeft;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_SetRatio100;
    }
}

