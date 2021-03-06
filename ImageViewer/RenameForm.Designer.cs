﻿namespace ImageViewer
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
            this.labelFileName = new System.Windows.Forms.Label();
            this.numericUpDownLevel = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLevel)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(341, 174);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(2);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(65, 32);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "変更";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // textBox_filename
            // 
            this.textBox_filename.Location = new System.Drawing.Point(86, 11);
            this.textBox_filename.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_filename.Name = "textBox_filename";
            this.textBox_filename.Size = new System.Drawing.Size(320, 23);
            this.textBox_filename.TabIndex = 2;
            this.textBox_filename.TextChanged += new System.EventHandler(this.textBox_filename_TextChanged);
            this.textBox_filename.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_filename_KeyPress);
            // 
            // textBox_comment
            // 
            this.textBox_comment.AcceptsTab = true;
            this.textBox_comment.Location = new System.Drawing.Point(86, 50);
            this.textBox_comment.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.Size = new System.Drawing.Size(320, 23);
            this.textBox_comment.TabIndex = 3;
            this.textBox_comment.TextChanged += new System.EventHandler(this.TextBox_comment_TextChanged);
            this.textBox_comment.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_comment_KeyDown);
            this.textBox_comment.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_filename_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "ファイル名";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "コメント";
            // 
            // labelNewBreadcrumbs
            // 
            this.labelNewBreadcrumbs.AutoSize = true;
            this.labelNewBreadcrumbs.Location = new System.Drawing.Point(144, 90);
            this.labelNewBreadcrumbs.Name = "labelNewBreadcrumbs";
            this.labelNewBreadcrumbs.Size = new System.Drawing.Size(76, 15);
            this.labelNewBreadcrumbs.TabIndex = 6;
            this.labelNewBreadcrumbs.Text = "breadcrumbs";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 90);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 15);
            this.label3.TabIndex = 7;
            this.label3.Text = "階層";
            // 
            // labelFileName
            // 
            this.labelFileName.AutoSize = true;
            this.labelFileName.Location = new System.Drawing.Point(17, 139);
            this.labelFileName.Name = "labelFileName";
            this.labelFileName.Size = new System.Drawing.Size(52, 15);
            this.labelFileName.TabIndex = 9;
            this.labelFileName.Text = "filename";
            // 
            // numericUpDownLevel
            // 
            this.numericUpDownLevel.Location = new System.Drawing.Point(86, 88);
            this.numericUpDownLevel.Name = "numericUpDownLevel";
            this.numericUpDownLevel.Size = new System.Drawing.Size(52, 23);
            this.numericUpDownLevel.TabIndex = 10;
            this.numericUpDownLevel.ValueChanged += new System.EventHandler(this.numericUpDownLevel_ValueChanged);
            this.numericUpDownLevel.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_filename_KeyPress);
            // 
            // RenameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 215);
            this.Controls.Add(this.numericUpDownLevel);
            this.Controls.Add(this.labelFileName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.labelNewBreadcrumbs);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_comment);
            this.Controls.Add(this.textBox_filename);
            this.Controls.Add(this.buttonOK);
            this.Font = new System.Drawing.Font("Yu Gothic UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RenameForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ファイル名の変更";
            this.Shown += new System.EventHandler(this.RenameForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLevel)).EndInit();
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
        private System.Windows.Forms.Label labelFileName;
        private System.Windows.Forms.NumericUpDown numericUpDownLevel;
    }
}