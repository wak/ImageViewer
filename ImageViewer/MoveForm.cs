using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;

namespace ImageViewer
{
    public partial class MoveForm : Form
    {
        private ImageList imageList = null;
        private string srcDirectory = null;
        private string targetDirectory = null;

        private BackgroundWorker worker = null;
        private int currentTargetIndex = 0;
        private bool isMoveCompleted = false;

        private List<bool> results = new List<bool>();

        public MoveForm(ImageList imageList)
        {
            InitializeComponent();

            this.KeyPreview = true;

            this.srcDirectory = System.IO.Path.GetDirectoryName(imageList[0]);
            this.imageList = imageList;

            listView.Items.Clear();
            for (int i = 0; i < imageList.Count; i++)
            {
                this.listView.Items.Add(new ListViewItem(new string[] { "", imageList[i] }));
            }
            columnHeader_Path.Width = -1;

            label1.Text = imageList.Count.ToString("#") + " items.";
        }

        private void Button_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Button_Move_Click(object sender, EventArgs e)
        {
            targetDirectory = askDestination();
            if (targetDirectory != null)
                moveItems();
        }

        private string askDestination()
        {
            try
            {
                FolderSelectDialog dlg = new FolderSelectDialog();
                dlg.Path = srcDirectory;
                dlg.Title = "Where to move?";
                if (dlg.ShowDialog() == DialogResult.OK)
                    return dlg.Path;
                else
                    return null;
            }
            catch (Exception)
            {
                // do nothing
            }

            //======================

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Where to move?";

            fbd.SelectedPath = srcDirectory;
            fbd.ShowNewFolderButton = true;

            if (fbd.ShowDialog(this) == DialogResult.OK)
                return fbd.SelectedPath;
            else
                return null;
        }

        private void moveItems()
        {
            currentTargetIndex = 0;
            this.Enabled = false;

            worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync();
        }

        private bool predicateBool(bool item)
        {
            return item == false;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Enabled = true;
            this.button_Move.Enabled = false;
            isMoveCompleted = true;

            updateResult();

            int failed = results.FindAll(predicateBool).Count;
            if (failed > 0)
            {
                MessageBox.Show(
                    string.Format("Move {0} items failed.", failed),
                    "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show(
                    string.Format("Move all {0} items succeed.", imageList.Count),
                    "Complete!", MessageBoxButtons.OK, MessageBoxIcon.None);

                this.Close();
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.listView.Items[currentTargetIndex].Selected = true;
            updateResult();
        }

        private void updateResult()
        {
            for (int i = 0; i < results.Count; i++)
            {
                bool succeed = results[i];

                this.listView.Items[i].SubItems[0].Text = (succeed ? "OK" : "FAILED");
                if (!succeed)
                    this.listView.Items[i].SubItems[0].BackColor = Color.FromArgb(0xff, 0, 0);
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < imageList.Count; i++)
            {
                Console.WriteLine("do " + i.ToString());
                currentTargetIndex = i;

                string srcFilepath = imageList[i];
                string srcFilename = System.IO.Path.GetFileName(srcFilepath);
                string dstFilepath = System.IO.Path.Combine(targetDirectory, srcFilename);

                try
                {
                    System.IO.File.Move(srcFilepath, dstFilepath);
                    results.Add(true);
                }
                catch (Exception)
                {
                    results.Add(false);
                }

                worker.ReportProgress((int)((float)i / (float)imageList.Count * 100));
            }
        }

        private void MoveForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;

            switch (e.KeyChar)
            {
                case (char)Keys.Escape:
                    this.Close();
                    break;

                case (char)Keys.Space:
                case (char)Keys.Enter:
                    if (isMoveCompleted)
                    {
                        this.Close();
                    }
                    else
                    {
                        button_Move.PerformClick();
                    }
                    break;
            }
        }

        private void MoveForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (worker != null)
                worker.Dispose();
        }
    }
}
