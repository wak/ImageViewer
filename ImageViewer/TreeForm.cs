using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace ImageViewer
{
    public partial class TreeForm : Form
    {
        public delegate void ItemSelectedEventHandler(ImageFile selectedFile);
        public delegate void ItemArrangedEventHandler();
        public delegate void RepositoryChangedEventHandler();
        public event ItemSelectedEventHandler itemSelected;
        public event ItemArrangedEventHandler itemArranged;
        public event RepositoryChangedEventHandler repositoryChanged;

        private ImageTree imageTree;
        private string baseDirectory;

        private List<ImageFile> viewingFiles = new List<ImageFile>();

        public TreeForm(string _baseDirectory, ImageTree tree)
        {
            InitializeComponent();

            baseDirectory = _baseDirectory;
            imageTree = tree;
            setupNodes();
        }

        private void setupNodes()
        {
            imageTree.dump();
            treeView.Nodes.Clear();

            setupNodes_(treeView.Nodes, imageTree);
            treeView.ExpandAll();
            treeView.Select();
            treeView.Focus();
        }

        private void setupNodes_(TreeNodeCollection myNodes, ImageTree imageNode)
        {
            string nodeName = (imageNode.isRoot() ? "(root)" : imageNode.name);
            if (imageNode.files.Count > 0 && !imageNode.files[0].IsImage())
                nodeName += " (*)";

            TreeNode myNode = new TreeNode(nodeName);
            myNode.Tag = imageNode;
            myNodes.Add(myNode);

            foreach (var oldview in viewingFiles)
                if (imageNode.contains(oldview))
                    treeView.SelectedNode = myNode;

            foreach (var inode in imageNode.nodes)
            {
                setupNodes_(myNode.Nodes, inode);
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            viewingFiles = new List<ImageFile>();

            ImageTree node = (ImageTree)e.Node.Tag;

            listView.Clear();
            listView.Columns.Add("ファイル名");
            foreach (var n in node.files)
            {
                var item = new ListViewItem(n.Filename);

                viewingFiles.Add(n);

                item.Tag = n;
                listView.Items.Add(item);
            }

            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listView.Columns[0].Width -= 5;

            if (listView.Items.Count > 0)
                listView.Items[0].Selected = true;
        }

        public void reload()
        {
            imageTree.reload();
            setupNodes();
        }

        private void change()
        {
            reload();

            if (repositoryChanged != null)
                repositoryChanged();
        }

        private void listView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (itemSelected != null)
                itemSelected((ImageFile)(e.Item.Tag));
        }

        private void TreeForm_Load(object sender, EventArgs e)
        {
            this.Location = new Point(
                this.Owner.Location.X + (this.Owner.Width - this.Width) / 2,
                this.Owner.Location.Y + (this.Owner.Height - this.Height) / 2);
        }

        private void TreeForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case (char)Keys.Enter:
                    e.Handled = true;
                    break;

                case 'r':
                    reload();
                    break;

                case 'w':
                case 'q':
                case (char)Keys.Escape:
                    e.Handled = true;
                    Close();
                    break;
            }
        }

        private void buttonClose_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void buttonReload_Click(object sender, EventArgs e)
        {
            reload();
        }

        #region フォルダ分け
        private void buttonArrange_Click(object sender, System.EventArgs e)
        {
            var result =
                MessageBox.Show(
                    "ツリー構造にフォルダ分けしますか？\r\n" +
                    "※※※この機能は実験段階です※※※",
                    "確認",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

            if (result != DialogResult.Yes)
                return;

            bool willFail = false;
            string additionalInfo = "";
            try
            {
                if (!arrangeImages(imageTree, true))
                    willFail = true;
            }
            catch (Exception err)
            {
                willFail = true;
                additionalInfo = "\r\n\r\n" + err.ToString();
            }

            if (willFail)
            {
                MessageBox.Show("" +
                    "検証の結果、失敗する可能性があるため中止しました。\r\n" +
                    "移動元ファイルが存在するか、移動先にファイルが存在していないかなど確認してください。" + additionalInfo,
                    "中止",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            try
            {
                if (arrangeImages(imageTree, false))
                    MessageBox.Show("完了しました。\r\nIV_ARRANGEフォルダに格納されています。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("いくつかのファイルの整理に失敗しました。", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (itemArranged != null)
                    itemArranged();
                Close();
            }
            catch (Exception err)
            {
                MessageBox.Show("途中で失敗しました。\r\n\r\n" + err.ToString(), "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool arrangeImages(ImageTree myNode, bool dryRun, int myIndex = 1, string myDir = null)
        {
            bool succeed = true;

            if (myNode.isRoot())
            {
                myDir = Path.Combine(Path.GetDirectoryName(baseDirectory), "IV_ARRANGE");
                if (!Directory.Exists(myDir))
                    Directory.CreateDirectory(myDir);
            }
            else
            {
                string dirname = Path.Combine(myDir, string.Format("{0:D2}. {1}", myIndex, myNode.name));
                Console.WriteLine("mkdir " + dirname);

                if (!Directory.Exists(dirname))
                {
                    if (!dryRun)
                        Directory.CreateDirectory(dirname);
                }

                foreach (var f in myNode.files)
                {
                    string dstname = Path.Combine(dirname, f.Filename);

                    Console.WriteLine("  move to " + dstname);

                    if (File.Exists(dstname) || !File.Exists(f.AbsPath))
                    {
                        succeed = false;
                    }
                    else
                    {
                        if (!dryRun)
                            File.Move(f.AbsPath, dstname);
                    }
                }

                myDir = dirname;
            }

            int childIndex = 1;
            foreach (var n in myNode.nodes)
            {
                if (!arrangeImages(n, dryRun, childIndex, myDir))
                    succeed = false;
                childIndex += 1;
            }

            return succeed;
        }
        #endregion

        private void ToolStripMenuItem_rename_Click(object sender, EventArgs e)
        {
            if (!hasSelectedTree())
                return;

            var tree = (ImageTree)treeView.SelectedNode.Tag;
            var renameForm = new RenameForm(tree.files[0].AbsPath, tree);
            if (renameForm.ShowDialog() == DialogResult.OK)
                change();
        }

        private bool hasSelectedTree()
        {
            if (treeView.SelectedNode == null)
                return false;

            var tree = (ImageTree)treeView.SelectedNode.Tag;

            if (tree.files.Count == 0)
                return false;

            return true;
        }

        private void ToolStripMenuItem_upLevel_Click(object sender, EventArgs e)
        {
            if (!hasSelectedTree())
                return;

            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            var tree = (ImageTree)treeView.SelectedNode.Tag;

            switch ((string)menu.Tag)
            {
                case "up":
                    tree.upLevel();
                    break;

                case "down":
                    tree.downLevel();
                    break;

                case "treeUp":
                    tree.upLevel(true);
                    break;

                case "treeDown":
                    tree.downLevel(true);
                    break;

                default:
                    throw new Exception("bug");
            }

            change();
        }

        private void treeView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var selectedNode = treeView.GetNodeAt(e.X, e.Y);
                treeView.SelectedNode = selectedNode;

                if (selectedNode != null)
                {
                    updateTreeMenuStatus((ImageTree)(treeView.SelectedNode.Tag));
                    treeMenu.Show(treeView, e.Location);
                }
            }
        }

        private void updateTreeMenuStatus(ImageTree selected)
        {
            ToolStripMenuItem_rename.Enabled = (selected.files.Count > 0);

            bool upEnabled = (selected.treeLevel > 1);
            ToolStripMenuItem_upLevel.Enabled = upEnabled;
            ToolStripMenuItem_upTreeLevel.Enabled = upEnabled;

            bool downEnabled = true;
            if (selected.isRoot())
                downEnabled = false;
            if (!selected.isRoot() && selected.parent.files.Count > 0 && selected.parent.nodes[0] == selected)
                downEnabled = false;

            ToolStripMenuItem_downLevel.Enabled = downEnabled;
            ToolStripMenuItem_downTreeLevel.Enabled = downEnabled;

            ToolStripMenuItem_removeIV.Enabled = !selected.files[0].IsImage();
        }

        private void buttonReLevel_Click(object sender, EventArgs e)
        {
            imageTree.fixLevel();
            change();
        }

        private void ToolStripMenuItem_addIV_Click(object sender, EventArgs e)
        {
            if (!hasSelectedTree())
                return;

            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            var tree = (ImageTree)treeView.SelectedNode.Tag;

            var newPath = Path.Combine(
                tree.files[0].DirPath,
                makePreviousFilename(tree.files[0]));

            FSUtility.Touch(newPath);
            imageTree.reload();
            var renameForm = new RenameForm(newPath, imageTree);
            renameForm.setCommentLevel(tree.treeLevel);
            renameForm.ShowDialog();

            change();
        }

        private void ToolStripMenuItem_removeIV_Click(object sender, EventArgs e)
        {
            if (!hasSelectedTree())
                return;

            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            var tree = (ImageTree)treeView.SelectedNode.Tag;

            if (tree.files[0].AbsPath.EndsWith(".iv"))
                File.Delete(tree.files[0].AbsPath);

            change();
        }

        private string makePreviousFilename(ImageFile target)
        {
            string prefix = "";
            ImageFile previous = null;

            foreach (var i in imageTree.imageList)
            {
                if (!i.IsImage())
                    continue;
                if (i.Equals(target))
                    break;
                previous = i;
            }

            if (previous != null)
                prefix = Path.GetFileNameWithoutExtension(previous.FilenameWithoutComment);

            string newName = null;

            for (int i = 1; i < 10; i++)
            {
                bool uniq = true;
                newName = string.Format("{0}[{1:d}].iv", prefix, i);

                foreach (var image in imageTree.imageList)
                    if (image.FilenameWithoutComment == newName)
                        uniq = false;

                if (uniq)
                    break;
            }

            // 10個も作ることはきっとない。
            return newName;
        }
    }
}
