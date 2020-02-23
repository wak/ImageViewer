using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ImageViewer
{
    public partial class RenameForm : Form
    {
        const string REGEX = @"(?<filename>.*?)\s+(?<separator>-+)\s*(?<comment>.*)(?<extension>\..*?)";

        public string result;
        private ImageTree imageTree;
        private string originalAbsPath;
        private readonly List<char> INVALID_CHARACTERS = new List<char>(new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' });

        public RenameForm(string oldFilePath, ImageTree imageTree)
        {
            InitializeComponent();

            this.result = null;
            this.imageTree = imageTree;
            this.originalAbsPath = oldFilePath;
            this.textBox_filename.Text = System.IO.Path.GetFileName(oldFilePath);
            this.textBox_filename.SelectionStart = System.IO.Path.GetFileName(oldFilePath).Length - System.IO.Path.GetExtension(oldFilePath).Length;

            updateCommentFromFilename();
            // this.textBox_filename.SelectionStart = 0;
            // this.textBox_filename.SelectionLength = System.IO.Path.GetFileName(oldFilePath).Length - System.IO.Path.GetExtension(oldFilePath).Length;
        }

        private void RenameForm_Shown(object sender, EventArgs e)
        {
            this.textBox_comment.Focus();
        }

        private void textBox_filename_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case (char)Keys.Enter:
                    e.Handled = true;
                    desideFilename();
                    break;

                case (char)Keys.Escape:
                    e.Handled = true;
                    this.Close();
                    break;
            }
        }

        private void textBox_comment_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    e.Handled = true;
                    updateFilenameFromComment(-1);
                    break;

                case Keys.Down:
                    e.Handled = true;
                    updateFilenameFromComment(1);
                    break;
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

                if (beforeSelectionStart - nrIgnored >= 0)
                    this.textBox_filename.SelectionStart = beforeSelectionStart - nrIgnored;
            }
        }

        private void updateCommentFromFilename()
        {
            ImageFile f = new ImageFile(this.textBox_filename.Text);

            this.textBox_comment.Text = f.comment;
            updateBreadcrumbs(f.commentLevel);
        }

        private void updateFilenameFromComment(int commentLevelAdd = 0)
        {
            ImageFile f = new ImageFile(this.textBox_filename.Text);

            string filenameBase;
            filenameBase = System.IO.Path.GetFileNameWithoutExtension(f.filenameWithoutComment);

            int commentLevel;
            if (this.textBox_comment.Text.Length > 0)
            {
                commentLevel = f.commentLevel + commentLevelAdd;
                if (commentLevel <= 0)
                    commentLevel = 1;
            }
            else
            {
                commentLevel = 0;
            }

            string filenameSep = "";
            if (commentLevel > 0)
                filenameSep = " " + new string('-', commentLevel) + " ";

            string newfilename = null;

            newfilename =
                string.Format("{0}{1}{2}{3}",
                    filenameBase,
                    filenameSep,
                    this.textBox_comment.Text,
                    System.IO.Path.GetExtension(this.textBox_filename.Text)
                );

            this.textBox_filename.Text = newfilename;
            fixFilename();

            updateBreadcrumbs(commentLevel);
        }

        private void updateBreadcrumbs(int level)
        {
            var tree = imageTree.findTreeByAbsPath(originalAbsPath);
            List<string> parts = tree.breadcrumbs("(root)");
            if (tree.files[0].absPath == originalAbsPath)
                parts.RemoveAt(parts.Count - 1);

            this.labelCurrentBreadcrumbs.Text = string.Join(" > ", parts);
            if (level <= 1)
            {
                this.labelNewBreadcrumbs.Text = "(root)";
                return;
            }

            this.labelNewBreadcrumbs.Text = string.Join(" > ", parts.GetRange(0, Math.Min(parts.Count, level)));
        }

        private void textBox_filename_TextChanged(object sender, EventArgs e)
        {
            if (this.ActiveControl == textBox_filename)
            {
                fixFilename();
                updateCommentFromFilename();
            }
        }

        private void TextBox_comment_TextChanged(object sender, EventArgs e)
        {
            if (this.ActiveControl == textBox_comment)
            {
                updateFilenameFromComment();
            }
        }
    }
}
