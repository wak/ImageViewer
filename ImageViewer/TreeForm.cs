using System;
using System.IO;
using System.Windows.Forms;

namespace ImageViewer
{
    public partial class TreeForm : Form
    {
        public delegate void ItemSelectedEventHandler(ImageFile selectedFile);
        public delegate void ItemArrangedEventHandler();
        public event ItemSelectedEventHandler itemSelected;
        public event ItemArrangedEventHandler itemArranged;

        private ImageTree imageTree;
        private string baseDirectory;

        public TreeForm(string _baseDirectory, ImageTree tree)
        {
            InitializeComponent();

            baseDirectory = _baseDirectory;
            imageTree = tree;
            addNodes(treeView.Nodes, imageTree);

            treeView.ExpandAll();
            treeView.Focus();
        }

        private void addNodes(TreeNodeCollection myNodes, ImageTree imageNode)
        {
            string nodeName = (imageNode.isRoot() ? "(root)" : imageNode.name);
            TreeNode myNode = new TreeNode(nodeName);
            myNode.Tag = imageNode;
            myNodes.Add(myNode);

            foreach (var inode in imageNode.nodes)
            {
                addNodes(myNode.Nodes, inode);
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ImageTree node = (ImageTree)e.Node.Tag;

            listView.Clear();
            listView.Columns.Add("ファイル名");
            foreach (var n in node.files)
            {
                var item = new ListViewItem(n.filename);

                item.Tag = n;
                listView.Items.Add(item);
            }

            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listView.Columns[0].Width -= 5;
        }

        private void listView_DoubleClick(object sender, System.EventArgs e)
        {
            if (itemSelected != null)
                itemSelected((ImageFile)listView.SelectedItems[0].Tag);
        }

        private void TreeForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case (char)Keys.Enter:
                    e.Handled = true;
                    break;

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
                    string dstname = Path.Combine(dirname, f.filename);

                    Console.WriteLine("  move to " + dstname);

                    if (File.Exists(dstname) || !File.Exists(f.absPath))
                    {
                        succeed = false;
                    }
                    else
                    {
                        if (!dryRun)
                            File.Move(f.absPath, dstname);
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
    }
}
