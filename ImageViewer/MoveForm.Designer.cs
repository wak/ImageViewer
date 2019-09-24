namespace ImageViewer
{
    partial class MoveForm
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
            this.button_Close = new System.Windows.Forms.Button();
            this.button_Move = new System.Windows.Forms.Button();
            this.listView = new System.Windows.Forms.ListView();
            this.columnHeader_Result = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_Path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_Close
            // 
            this.button_Close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Close.Location = new System.Drawing.Point(503, 244);
            this.button_Close.Name = "button_Close";
            this.button_Close.Size = new System.Drawing.Size(63, 23);
            this.button_Close.TabIndex = 1;
            this.button_Close.Text = "閉じる";
            this.button_Close.UseVisualStyleBackColor = true;
            this.button_Close.Click += new System.EventHandler(this.Button_Close_Click);
            // 
            // button_Move
            // 
            this.button_Move.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Move.Location = new System.Drawing.Point(421, 244);
            this.button_Move.Name = "button_Move";
            this.button_Move.Size = new System.Drawing.Size(63, 23);
            this.button_Move.TabIndex = 2;
            this.button_Move.Text = "移動";
            this.button_Move.UseVisualStyleBackColor = true;
            this.button_Move.Click += new System.EventHandler(this.Button_Move_Click);
            // 
            // listView
            // 
            this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_Result,
            this.columnHeader_Path});
            this.listView.Font = new System.Drawing.Font("ＭＳ ゴシック", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.listView.FullRowSelect = true;
            this.listView.GridLines = true;
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(0, 0);
            this.listView.MultiSelect = false;
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(580, 228);
            this.listView.TabIndex = 3;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_Result
            // 
            this.columnHeader_Result.Text = "Result";
            // 
            // columnHeader_Path
            // 
            this.columnHeader_Path.Text = "Path";
            this.columnHeader_Path.Width = 200;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 249);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "label1";
            // 
            // MoveForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(578, 276);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listView);
            this.Controls.Add(this.button_Move);
            this.Controls.Add(this.button_Close);
            this.MinimumSize = new System.Drawing.Size(100, 100);
            this.Name = "MoveForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "画像の移動";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MoveForm_FormClosing);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MoveForm_KeyPress);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button_Close;
        private System.Windows.Forms.Button button_Move;
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader columnHeader_Result;
        private System.Windows.Forms.ColumnHeader columnHeader_Path;
        private System.Windows.Forms.Label label1;
    }
}