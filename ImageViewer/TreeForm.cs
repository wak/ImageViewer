﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;

namespace ImageViewer
{
    public partial class TreeForm : Form
    {
        public delegate void ItemSelectedEventHandler(ImageFile selectedFile);
        public delegate void ItemArrangedEventHandler();
        public delegate void RepositoryChangedEventHandler();
        public event ItemSelectedEventHandler itemSelected;

        private bool notifyingOtherForm = false;
        private bool hooking = false;
        private bool processingOtherFormNotification = false;

        public event ItemArrangedEventHandler itemArranged;
        public event RepositoryChangedEventHandler repositoryChanged;

        private ImageTree imageTree { get { return imageRepository.tree; } }
        private ImageRepository imageRepository;
        private string baseDirectory;

        private ImageFile selectedFile = null;
        private ImageTree viewingTree = null;
        private List<ImageFile> viewingFiles = new List<ImageFile>();

        public TreeForm(string _baseDirectory, ImageRepository repository, ImageFile defaultSelected)
        {
            InitializeComponent();

            baseDirectory = _baseDirectory;
            imageRepository = repository;
            selectedFile = defaultSelected;
        }

        private void TreeForm_Shown(object sender, EventArgs e)
        {
            runNonPreemptive(() =>
            {
                setupTreeView();
                updateWidgetStatus();
            });
        }

        public delegate void RunNonPreemptiveDelegate();
        private void runNonPreemptive(RunNonPreemptiveDelegate func)
        {
            /*
             * 無限イベントハンドラ呼び出し防止。
             */
            var before = hooking;

            hooking = true;
            func.Invoke();
            hooking = before;
        }

        public void reload()
        {
            clearListView();

            imageRepository.reload();
            setupTreeView();
        }

        #region ツリー
        private void setupTreeView()
        {
            // imageTree.dump();
            treeView.Nodes.Clear();

            setupTreeViewNodes(treeView.Nodes, imageTree);
            treeView.ExpandAll();
            treeView.Select();
            treeView.Focus();
        }

        private void setupTreeViewNodes(TreeNodeCollection myNodes, ImageTree imageNode)
        {
            string nodeName = (imageNode.isRoot() ? "(root)" : imageNode.name);
            if (!imageNode.isRoot() && imageNode.files.Count > 0 && !imageNode.files[0].IsImage())
                nodeName += " (*)";

            TreeNode myNode = new TreeNode(nodeName);
            myNode.Tag = imageNode;
            myNodes.Add(myNode);

            if (imageNode.contains(selectedFile))
                treeView.SelectedNode = myNode;

            foreach (var inode in imageNode.nodes)
            {
                setupTreeViewNodes(myNode.Nodes, inode);
            }
        }
        #endregion

        #region リスト
        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ImageTree node = (ImageTree)e.Node.Tag;

            debug("treeView AfterSelect: " + node.name);

            setupListView(node);
        }

        private void setupListView(ImageTree node)
        {
            runNonPreemptive(() =>
            {
                debug("updateItemViewFiles");
                setupListViewItems(node);
                setupListViewSelected();
            });
        }

        private void setupListViewItems(ImageTree node)
        {
            if (!shouldUpdateListView(node))
                return;

            debug("_updateItemViewFiles");

            viewingTree = node;
            viewingFiles = new List<ImageFile>();

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
        }

        private void setupListViewSelected()
        {
            debug("updateItemViewSelected");

            listView.SelectedItems.Clear();

            foreach (ListViewItem i in listView.Items)
            {
                var file = (ImageFile)i.Tag;

                if (file.Equals(selectedFile))
                {
                    i.Selected = true;
                    listView.EnsureVisible(listView.SelectedIndices[0]);
                    return;
                }
            }

            // ツリーをクリックした場合に、そのツリーの先頭の画像を表示する。
            if (listView.SelectedItems.Count == 0 && listView.Items.Count > 0)
            {
                listView.Items[0].Selected = true;
            }
        }

        private bool shouldUpdateListView(ImageTree node)
        {
            return viewingTree != node;
            // return !viewingFiles.SequenceEqual(node.files);
        }

        public void changeSelectedItem(ImageFile newSelected)
        {
            // 本体Formからのみ呼び出される。

            if (notifyingOtherForm)
                return;

            debug("changeSelectedItem");

            processingOtherFormNotification = true;
            changeSelectedItemSafe(newSelected, treeView.Nodes);
            processingOtherFormNotification = false;
        }

        private bool changeSelectedItemSafe(ImageFile newSelected, TreeNodeCollection searchNodes)
        {
            debug("changeSelectedItemSafe");

            foreach (TreeNode t in searchNodes)
            {
                var tree = (ImageTree)t.Tag;

                if (tree.files.Contains(newSelected))
                {
                    runNonPreemptive(() => treeView.SelectedNode = t);
                    selectedFile = newSelected;
                    setupListView(tree);

                    return true;
                }

                if (changeSelectedItemSafe(newSelected, t.Nodes))
                    return true;
            }

            return false;
        }

        private void clearListView()
        {
            viewingTree = null;
            listView.Clear();
        }
        #endregion

        private void change()
        {
            updateWidgetStatus();

            reload();

            if (repositoryChanged != null)
                repositoryChanged();
        }

        private void debug(string message)
        {
            Console.WriteLine(message);
        }

        private void safeNotifyItemSelected(ImageFile file)
        {
            if (notifyingOtherForm || processingOtherFormNotification)
                return;

            debug("notify item changed: " + file.Filename);

            var before = notifyingOtherForm;
            notifyingOtherForm = true;

            if (itemSelected != null)
                runNonPreemptive(() => itemSelected(file));

            notifyingOtherForm = before;
        }

        private void listView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.Item == null)
                return; // 選択をクリアしたとき

            if (processingOtherFormNotification)
                return;

            debug("listView ItemSelectionChanged: " + ((ImageFile)e.Item.Tag).Filename);

            safeNotifyItemSelected((ImageFile)(e.Item.Tag));
            selectedFile = (ImageFile)e.Item.Tag;
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

        public void changeRepository(ImageRepository imageRepository)
        {
            this.imageRepository = imageRepository;
            setupTreeView();
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

        private bool hasSelectedTree()
        {
            if (treeView.SelectedNode == null)
                return false;

            var tree = (ImageTree)treeView.SelectedNode.Tag;

            if (tree.files.Count == 0)
                return false;

            return true;
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

        private void updateWidgetStatus()
        {
            bool enabled = (imageRepository.IsVirtualRepository && !imageRepository.IsReadonly());
            buttonArrange.Visible = enabled;
            buttonReLevel.Visible = enabled;

            if (imageRepository.IsVirtualRepository)
            {
                buttonToggleTreeViewMode.Text = "実フォルダツリー管理に切替";
                Text = "仮想ツリー管理";
            }
            else
            {
                buttonToggleTreeViewMode.Text = "仮想ツリー管理に切替";
                Text = "実フォルダツリー管理";
            }
        }

        private void updateTreeMenuStatus(ImageTree selected)
        {
            if (!imageRepository.IsVirtualRepository || imageRepository.IsReadonly())
            {
                ToolStripMenuItem_rename.Enabled = false;
                ToolStripMenuItem_upLevel.Enabled = false;
                ToolStripMenuItem_upTreeLevel.Enabled = false;
                ToolStripMenuItem_downLevel.Enabled = false;
                ToolStripMenuItem_downTreeLevel.Enabled = false;
                ToolStripMenuItem_addIV.Enabled = false;
                ToolStripMenuItem_removeIV.Enabled = false;
                return;
            }

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

            ToolStripMenuItem_removeIV.Enabled = (!selected.Empty() && !selected.files[0].IsImage());
        }

        #region ボタン

        private void buttonClose_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void buttonReload_Click(object sender, EventArgs e)
        {
            reload();
        }

        private void buttonReLevel_Click(object sender, EventArgs e)
        {
            imageTree.fixLevel();
            change();
        }

        private void buttonToggleTreeViewMode_Click(object sender, EventArgs e)
        {
            imageRepository.IsVirtualRepository = !imageRepository.IsVirtualRepository;
            change();
        }

        #endregion

        #region ツリーの右クリックメニュー

        private void ToolStripMenuItem_rename_Click(object sender, EventArgs e)
        {
            if (!hasSelectedTree())
                return;

            var tree = (ImageTree)treeView.SelectedNode.Tag;
            var renameForm = new RenameForm(tree.files[0].AbsPath, tree);
            if (renameForm.ShowDialog() == DialogResult.OK)
                change();
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

        private void ToolStripMenuItem_addIV_Click(object sender, EventArgs e)
        {
            // @todo 選択されているツリーに一つもファイルがないと作成できない。

            if (!hasSelectedTree())
                return;

            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            var tree = (ImageTree)treeView.SelectedNode.Tag;

            var newPath = Path.Combine(
                tree.files[0].DirPath,
                makePreviousFilename(tree.files[0]));

            FSUtility.Touch(newPath);
            imageRepository.reload();
            var renameForm = new RenameForm(newPath, imageTree);
            renameForm.setCommentLevel(tree.treeLevel);
            if (renameForm.ShowDialog() != DialogResult.OK)
                File.Delete(newPath);

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

            foreach (var i in imageTree.imageRepository)
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

                foreach (var image in imageTree.imageRepository)
                    if (image.FilenameWithoutComment == newName)
                        uniq = false;

                if (uniq)
                    break;
            }

            // 10個も作ることはきっとない。
            return newName;
        }

        #endregion

        #region リストの右クリックメニュー

        private void ItemuMenu_ChangeName_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count != 1)
                return;

            var renameForm = new RenameForm(((ImageFile)listView.SelectedItems[0].Tag).AbsPath, imageTree);
            if (renameForm.ShowDialog() == DialogResult.OK)
                change();
        }

        #endregion
    }
}
