using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ImageViewer
{
    public partial class RenameForm : Form
    {
        const string REGEX = @"(?<filename>.*?)\s+(?<separator>-+)\s*(?<comment>.*)(?<extension>\..*?)";

        public string result;
        private ImageTree imageTree, targetTree;
        private string originalAbsPath;
        private readonly List<char> INVALID_CHARACTERS = new List<char>(new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' });

        private int commentLevel = 0;

        public RenameForm(string oldFilePath, ImageTree imageTree)
        {
            InitializeComponent();

            this.result = null;
            this.imageTree = imageTree;
            this.originalAbsPath = oldFilePath;
            this.targetTree = imageTree.findTreeByAbsPath(originalAbsPath);
            this.textBox_filename.Text = System.IO.Path.GetFileName(oldFilePath);
            this.textBox_filename.SelectionStart = System.IO.Path.GetFileName(oldFilePath).Length - System.IO.Path.GetExtension(oldFilePath).Length;

            if (targetTree != null)
                setCommentLevel(targetTree.treeLevel);

            updateCommentFromFilename();
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
                    setCommentLevel(commentLevel - 1);
                    updateFilenameFromComment();
                    break;

                case Keys.Down:
                    e.Handled = true;
                    setCommentLevel(commentLevel + 1);
                    updateFilenameFromComment();
                    break;
            }
        }

        private void setCommentLevel(int newLevel)
        {
            if (targetTree.treeLevel == 0)
                commentLevel = 1;
            else if (targetTree.files[0].AbsPath == originalAbsPath)
                commentLevel = Math.Min(Math.Max(1, newLevel), targetTree.treeLevel);
            else
                commentLevel = Math.Min(Math.Max(1, newLevel), targetTree.treeLevel + 1);

            updateFilenameFromComment();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            desideFilename();
        }

        private void desideFilename()
        {
            ImageFile f = new ImageFile(this.textBox_filename.Text);
            string dirname = System.IO.Path.GetDirectoryName(originalAbsPath);
            string newFilepath = System.IO.Path.Combine(dirname, this.textBox_filename.Text);

            if (this.textBox_comment.Text == "")
            {
                newFilepath = System.IO.Path.Combine(dirname, f.FilenameWithoutComment);
            }

            if (FSUtility.Rename(originalAbsPath, newFilepath))
            {
                this.result = newFilepath;
                this.DialogResult = DialogResult.OK;
            }

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

            this.textBox_comment.Text = f.Comment;
            setCommentLevel(f.CommentLevel);
            updateBreadcrumbs(commentLevel);
        }

        private void updateFilenameFromComment()
        {
            ImageFile f = new ImageFile(this.textBox_filename.Text);

            string filenameBase;
            filenameBase = System.IO.Path.GetFileNameWithoutExtension(f.FilenameWithoutComment);


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
            List<string> parts = targetTree.breadcrumbs("(root)");

            if (targetTree.files[0].AbsPath == originalAbsPath)
                parts.RemoveAt(parts.Count - 1);

            if (level <= 1)
                this.labelNewBreadcrumbs.Text = "(root)" + " > " + textBox_comment.Text;
            else
                this.labelNewBreadcrumbs.Text = string.Join(" > ", parts.GetRange(0, Math.Min(parts.Count, level))) + " > " + textBox_comment.Text;
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
