using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ImageViewer
{
    public partial class RenameForm : Form
    {
        public string result;
        private readonly List<char> INVALID_CHARACTERS = new List<char>(new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' });

        public RenameForm(string oldFilePath)
        {
            InitializeComponent();

            this.result = null;
            this.textBox_filename.Text = System.IO.Path.GetFileName(oldFilePath);
            this.textBox_filename.SelectionStart = System.IO.Path.GetFileName(oldFilePath).Length - System.IO.Path.GetExtension(oldFilePath).Length;
            // this.textBox_filename.SelectionStart = 0;
            // this.textBox_filename.SelectionLength = System.IO.Path.GetFileName(oldFilePath).Length - System.IO.Path.GetExtension(oldFilePath).Length;
        }

        private void RenameForm_Shown(object sender, EventArgs e)
        {
            this.textBox_filename.Focus();
        }

        private void textBox_filename_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Enter)
            {
                e.Handled = true;
                desideFilename();
            }
            if (e.KeyChar == (char) Keys.Escape)
            {
                e.Handled = true;
                this.Close();
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            desideFilename();
        }

        private void desideFilename()
        {
            this.result = this.textBox_filename.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void fixFilename()
        {
            string newFilename = "";
            int nrIgnored = 0;
            int beforeSelectionStart = this.textBox_filename.SelectionStart;

            foreach (char c in this.textBox_filename.Text)
            {
                if (INVALID_CHARACTERS.Contains(c))
                {
                    nrIgnored += 1;
                    continue;
                }
                newFilename += c;
            }

            if (nrIgnored > 0)
            {
                this.textBox_filename.Text = newFilename;
                this.textBox_filename.SelectionStart = beforeSelectionStart - nrIgnored;
            }
        }

        private void textBox_filename_TextChanged(object sender, EventArgs e)
        {
            fixFilename();
        }
    }
}
