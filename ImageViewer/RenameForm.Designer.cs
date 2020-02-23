namespace ImageViewer
{
    partial class RenameForm
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.textBox_filename = new System.Windows.Forms.TextBox();
            this.textBox_comment = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.labelNewBreadcrumbs = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelCurrentBreadcrumbs = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(315, 200);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(2);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(56, 26);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "変更";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // textBox_filename
            // 
            this.textBox_filename.Location = new System.Drawing.Point(36, 44);
            this.textBox_filename.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_filename.Name = "textBox_filename";
            this.textBox_filename.Size = new System.Drawing.Size(335, 19);
            this.textBox_filename.TabIndex = 2;
            this.textBox_filename.TextChanged += new System.EventHandler(this.textBox_filename_TextChanged);
            this.textBox_filename.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_filename_KeyPress);
            // 
            // textBox_comment
            // 
            this.textBox_comment.AcceptsTab = true;
            this.textBox_comment.Location = new System.Drawing.Point(36, 119);
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.Size = new System.Drawing.Size(335, 19);
            this.textBox_comment.TabIndex = 3;
            this.textBox_comment.TextChanged += new System.EventHandler(this.TextBox_comment_TextChanged);
            this.textBox_comment.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_comment_KeyDown);
            this.textBox_comment.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_filename_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "ファイル名";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 93);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "コメント";
            // 
            // labelNewBreadcrumbs
            // 
            this.labelNewBreadcrumbs.AutoSize = true;
            this.labelNewBreadcrumbs.Location = new System.Drawing.Point(88, 191);
            this.labelNewBreadcrumbs.Name = "labelNewBreadcrumbs";
            this.labelNewBreadcrumbs.Size = new System.Drawing.Size(70, 12);
            this.labelNewBreadcrumbs.TabIndex = 6;
            this.labelNewBreadcrumbs.Text = "breadcrumbs";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 191);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "変更階層：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 165);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "現在階層：";
            // 
            // labelCurrentBreadcrumbs
            // 
            this.labelCurrentBreadcrumbs.AutoSize = true;
            this.labelCurrentBreadcrumbs.Location = new System.Drawing.Point(88, 165);
            this.labelCurrentBreadcrumbs.Name = "labelCurrentBreadcrumbs";
            this.labelCurrentBreadcrumbs.Size = new System.Drawing.Size(70, 12);
            this.labelCurrentBreadcrumbs.TabIndex = 9;
            this.labelCurrentBreadcrumbs.Text = "breadcrumbs";
            // 
            // RenameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(413, 238);
            this.Controls.Add(this.labelCurrentBreadcrumbs);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.labelNewBreadcrumbs);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_comment);
            this.Controls.Add(this.textBox_filename);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RenameForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ファイル名の変更";
            this.Shown += new System.EventHandler(this.RenameForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TextBox textBox_filename;
        private System.Windows.Forms.TextBox textBox_comment;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelNewBreadcrumbs;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelCurrentBreadcrumbs;
    }
}