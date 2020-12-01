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
        private string fileExtension;
        private readonly List<char> INVALID_CHARACTERS = new List<char>(new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' });

        private int commentLevel = 0;

        public RenameForm(string oldFilePath, ImageTree imageTree)
        {
            InitializeComponent();

            var image = new ImageFile(oldFilePath);
            this.fileExtension = System.IO.Path.GetExtension(oldFilePath);
            this.result = null;
            this.imageTree = imageTree;
            this.originalAbsPath = oldFilePath;
            this.targetTree = imageTree.findTreeByAbsPath(originalAbsPath);
            this.textBox_filename.Text = System.IO.Path.GetFileNameWithoutExtension(image.FilenameWithoutComment);
            this.textBox_comment.Text = image.Comment;

            if (targetTree != null)
                setCommentLevel(targetTree.treeLevel);
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
                    setCommentLevel(commentLevel - 1);
                    break;

                case Keys.Down:
                    setCommentLevel(commentLevel + 1);
                    break;

                case Keys.J:
                    if (isControlKeyPressing())
                    {
                        setCommentLevel(commentLevel + 1);
                    }
                    break;

                case Keys.K:
                    if (isControlKeyPressing())
                    {
                        setCommentLevel(commentLevel - 1);
                    }
                    break;

                default:
                    return;
            }
            e.Handled = true;
        }

        private static bool isControlKeyPressing()
        {
            return (Control.ModifierKeys & Keys.Control) == Keys.Control;
        }

        private void setCommentLevel(int newLevel)
        {
            if (targetTree.treeLevel == 0)
                commentLevel = 1;
            else if (targetTree.files[0].AbsPath == originalAbsPath)
                commentLevel = Math.Min(Math.Max(1, newLevel), targetTree.treeLevel);
            else
                commentLevel = Math.Min(Math.Max(1, newLevel), targetTree.treeLevel + 1);

            numericUpDownLevel.Value = commentLevel;
            updateFilenameLabel();
            updateBreadcrumbs(commentLevel);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            desideFilename();
        }

        private void desideFilename()
        {
            string dirname = System.IO.Path.GetDirectoryName(originalAbsPath);
            string newFilepath = System.IO.Path.Combine(dirname, this.labelFileName.Text);

           if (FSUtility.Rename(originalAbsPath, newFilepath))
            {
                this.result = newFilepath;
                this.DialogResult = DialogResult.OK;
            }

            this.Close();
        }

        private void removeIgnoredCharacters(TextBox textbox)
        {
            string newFilename = "";
            int nrIgnored = 0;
            int beforeSelectionStart = textbox.SelectionStart;

            foreach (char c in textbox.Text)
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
                textbox.Text = newFilename;

                if (beforeSelectionStart - nrIgnored >= 0)
                    textbox.SelectionStart = beforeSelectionStart - nrIgnored;
            }
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
            removeIgnoredCharacters(this.textBox_filename);
            updateFilenameLabel();
        }

        private void TextBox_comment_TextChanged(object sender, EventArgs e)
        {
            removeIgnoredCharacters(this.textBox_comment);
            updateFilenameLabel();
            updateBreadcrumbs(commentLevel);
        }

        private void numericUpDownLevel_ValueChanged(object sender, EventArgs e)
        {
            setCommentLevel((int)numericUpDownLevel.Value);
        }

        private void updateFilenameLabel()
        {
            string filenameSep = "";
            if (commentLevel > 0 && textBox_comment.Text.Length > 0)
                filenameSep = " " + new string('-', commentLevel) + " ";

            labelFileName.Text = textBox_filename.Text + filenameSep + textBox_comment.Text + this.fileExtension;
        }
    }
}
