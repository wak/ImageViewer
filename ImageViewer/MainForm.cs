using System;
using System.Drawing;
using System.Windows.Forms;

namespace ImageViewer
{
    public partial class MainForm : Form
    {
        const int PICTURE_BORDER_SIZE = 10;

        private ImageLoader imageLoader = new ImageLoader();

        private ImageList imageList;
        private string currentDirectoryPath;
        private string currentImagePath;

        private Image currentImage;
        private int currentImageListIndex;

        private Rectangle currentRectangle;
        private double currentZoomRatio;
        private Point currentDrawLocation;

        private bool isFixedZoomRatio;
        private bool isFixedDrawLocation;

        private bool autoResizeWindowMode = false;

        private bool overwrapWait;

        private bool initialized = false;

        public MainForm(string path)
        {
            initializeInstanceVariables();

            InitializeComponent();
            this.MouseWheel += MainForm_MouseWheel;

            restoreWindowSize();
            updateImageList(path);

            initialized = true;
            changeImage();
        }

        private void initializeInstanceVariables()
        {
            imageList = new ImageList();
            currentDirectoryPath = null;
            currentImageListIndex = 0;
            currentImage = null;
            currentZoomRatio = 1.0;
            isFixedZoomRatio = false;
            currentImagePath = null;
            overwrapWait = false;
            currentDrawLocation.X = currentDrawLocation.Y = 0;
        }

        private void updateImageList(string path)
        {
            if (path == null)
            {
                currentDirectoryPath = null;
                currentImagePath = null;
                currentImageListIndex = 0;
                currentImage = null;
                return;
            }

            string filepath, directory;
            path = System.IO.Path.GetFullPath(path);

            if (System.IO.File.Exists(path))
            {
                filepath = path;
                directory = System.IO.Path.GetDirectoryName(path);
            }
            else if (System.IO.Directory.Exists(path))
            {
                if (currentImagePath != null && currentImagePath.Contains(path))
                    filepath = currentImagePath;
                else
                    filepath = null;
                directory = path;
            }
            else
            {
                MessageBox.Show(
                    "不明なパスが指定されました。\n\n" + path,
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            imageList = new ImageList(directory);
            currentDirectoryPath = directory;
            currentImagePath = filepath;
            currentImageListIndex = Math.Max(imageList.findIndex(filepath), 0);

            changeImage();
            updateDirectoryWatcher();
        }

        private void reloadImageList()
        {
            string viewingPath = currentImagePath;
        }

        private void renameImageFilename()
        {
            RenameForm form = new RenameForm(currentImagePath);
            string newFilename = null;

            if (form.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            newFilename = form.result;
            form.Dispose();

            string dirname = System.IO.Path.GetDirectoryName(currentImagePath);
            string newFilepath = System.IO.Path.Combine(dirname, newFilename);
            try
            {
                System.IO.File.Move(currentImagePath, newFilepath);
            } catch (Exception e)
            {
                MessageBox.Show("名前変更に失敗しました。\n\n" + e.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            updateImageList(newFilepath);
            refreshWindow();
        }

        private void copyToClipboard()
        {
            if (currentImage != null)
                Clipboard.SetImage(currentImage);
        }

        #region Window描画

        private void changeImage()
        {
            if (imageList.Count == 0)
            {
                currentImage = null;
                refreshWindow();
                return;
            }

            currentImagePath = imageList[currentImageListIndex];
            currentImage = imageLoader.loadImage(currentImagePath);

            if (autoResizeWindowMode)
            {
                autoResizeWindow();
            }

            refreshWindow();
        }

        private void prepareToChangeImage()
        {
            currentImage = null;
        }

        private void refreshWindow()
        {
            if (currentImage == null)
            {
                pictureBox.Invalidate();
                updateWindowTitle();
                return;
            }

            if (!isFixedZoomRatio)
                calcDefaultZoomRatio();
            if (!isFixedDrawLocation)
                calcDefaultDrawLocation();

            if (autoResizeWindowMode)
            {
                centerWindow();
                calcDefaultZoomRatio();
                calcDefaultDrawLocation();
            }

            currentRectangle = new Rectangle(0, 0,
                (int)Math.Round(currentImage.Width * currentZoomRatio),
                (int)Math.Round(currentImage.Height * currentZoomRatio));

            currentRectangle.X = currentDrawLocation.X;
            currentRectangle.Y = currentDrawLocation.Y;

            updateWindowTitle();
            pictureBox.Invalidate();
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (currentImage == null)
            {
                e.Graphics.Clear(Color.WhiteSmoke);
            }
            else
            {
                e.Graphics.DrawImage(currentImage, currentRectangle);
            }
        }

        private void updateWindowTitle()
        {
            string newTitle = "ImageViewer";

            if (imageList.Count > 0)
            {
                if (currentImage == null)
                    newTitle += " !!LOAD ERROR!!";

                if (overwrapWait)
                {
                    pictureBox.BackColor = Color.FromArgb(115, 199, 255);
                    newTitle += " (!!OVERWRAPPED!!)";
                } else
                {
                    pictureBox.BackColor = SystemColors.Control;
                }

                if (isFixedZoomRatio)
                    newTitle += "[FZ]";

                if (isFixedDrawLocation)
                    newTitle += "[FL]";

                newTitle += string.Format(" ({0}/{1}, {2:0.00}x, {3})",
                    currentImageListIndex + 1,
                    imageList.Count,
                    currentZoomRatio, currentImagePath);
            }
            this.Text = newTitle;
        }

        #endregion

        #region 画像描画ビュー変更

        private double calcZoomRatio(int outerHeight, int outerWidth, int imageHeight, int imageWidth)
        {
            double ratio;

            ratio = Math.Min((double)(outerWidth - PICTURE_BORDER_SIZE * 2) / imageWidth,
                             (double)(outerHeight - PICTURE_BORDER_SIZE * 2) / imageHeight);

            return Math.Min(ratio, 1.0);
        }

        private void calcDefaultZoomRatio()
        {
            isFixedZoomRatio = false;

            currentZoomRatio = calcZoomRatio(pictureBox.Height, pictureBox.Width, currentImage.Height, currentImage.Width);
        }

        private void calcDefaultDrawLocation()
        {
            isFixedDrawLocation = false;

            currentDrawLocation.X = (pictureBox.Width - (int)(currentImage.Width * currentZoomRatio)) / 2;
            currentDrawLocation.Y = (pictureBox.Height - (int)(currentImage.Height * currentZoomRatio)) / 2;
        }

        private void showNextImage()
        {
            prepareToChangeImage();

            if (currentImageListIndex + 1  >= imageList.Count)
            {
                if (!overwrapWait)
                {
                    overwrapWait = true;
                }
                else
                {
                    overwrapWait = false;
                    currentImageListIndex = 0;
                }
            }
            else
            {
                overwrapWait = false;
                currentImageListIndex += 1;
            }

            changeImage();
        }

        private void showPreviousImage()
        {
            prepareToChangeImage();

            if (currentImageListIndex - 1 < 0)
            {
                if (!overwrapWait)
                {
                    overwrapWait = true;
                }
                else
                {
                    overwrapWait = false;
                    currentImageListIndex = imageList.Count - 1;
                }
            }
            else
            {
                overwrapWait = false;
                currentImageListIndex -= 1;
            }

            changeImage();
        }

        private void showLastImage()
        {
            prepareToChangeImage();

            currentImageListIndex = imageList.Count - 1;
            changeImage();
        }

        private void showFirstImage()
        {
            prepareToChangeImage();
            currentImageListIndex = 0;
            changeImage();
        }

        private void zoomReset()
        {
            currentZoomRatio = 1.0;
            isFixedZoomRatio = false;
            refreshWindow();
        }

        private void zoomIn()
        {
            currentZoomRatio += 0.1;
            isFixedZoomRatio = true;
            refreshWindow();
        }

        private void zoomOut()
        {
            currentZoomRatio -= 0.1;
            isFixedZoomRatio = true;
            refreshWindow();
        }

        #endregion

        #region Window調整

        private void toggleFullscreen()
        {
            if (this.FormBorderStyle == FormBorderStyle.Sizable)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
            } else
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void toggleWindowMaximized()
        {
            if (this.WindowState == FormWindowState.Normal)
                this.WindowState = FormWindowState.Maximized;
            else
            {
                this.WindowState = FormWindowState.Normal;
                refreshWindow(); // AutoResizeMode時にうまく中央表示できないため。
            }
        }

        private void toggleFormBorderStyleNone()
        {
            if (this.FormBorderStyle == FormBorderStyle.Sizable)
                this.FormBorderStyle = FormBorderStyle.None;
            else
                this.FormBorderStyle = FormBorderStyle.Sizable;
        }

        private void toggleAutoResizeWindowMode()
        {
            autoResizeWindowMode = !autoResizeWindowMode;

            if (autoResizeWindowMode)
            {
                normalizeWindow();
                autoResizeWindow();
            }
            refreshWindow();
        }

        private void normalizeWindow()
        {
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Normal;
        }

        private void autoResizeWindow()
        {
            if (currentImage == null)
                return;

            System.Windows.Forms.Screen s = System.Windows.Forms.Screen.FromControl(this);
            int maxHeight = (int) (s.Bounds.Height * 0.9);
            int maxWidth = (int) (s.Bounds.Width * 0.9);

            int heightDiff = this.Height - this.pictureBox.Height;
            int widthDiff = this.Width - this.pictureBox.Width;
            int height, width;

            height = Math.Min(currentImage.Height + heightDiff + PICTURE_BORDER_SIZE * 2, maxHeight) + 10;
            width = Math.Min(currentImage.Width + widthDiff + PICTURE_BORDER_SIZE * 2, maxWidth) + 10;

            double ratio = calcZoomRatio(height, width, currentImage.Height, currentImage.Width);

            int neededHeight = (int)((this.currentImage.Height * ratio) + PICTURE_BORDER_SIZE * 2 + heightDiff);
            if (height > neededHeight + 1)
                height = neededHeight;

            int neededWidth = (int)((this.currentImage.Width * ratio) + PICTURE_BORDER_SIZE * 2 + widthDiff);
            if (width > neededWidth + 1)
                width = neededWidth;

            this.Height = height;
            this.Width = width;
            
            this.Location = new Point(
                Math.Min(this.Location.X, s.WorkingArea.X + s.WorkingArea.Width - this.Width),
                Math.Min(this.Location.Y, s.WorkingArea.Y + s.WorkingArea.Height - this.Height));
        }

        private void centerWindow()
        {
            if (currentImage == null)
                return;

            System.Windows.Forms.Screen s = System.Windows.Forms.Screen.FromControl(this);

            this.Location = new Point(
                Math.Max(s.WorkingArea.X, s.WorkingArea.X + (s.WorkingArea.Width - this.Width) / 2),
                Math.Max(s.WorkingArea.Y, s.WorkingArea.Y + (s.WorkingArea.Height - this.Height) / 2));
        }

        #endregion

        #region キーボード操作

        private void MainForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                case Keys.Right:
                    showNextImage();
                    break;

                case Keys.Up:
                case Keys.Left:
                    showPreviousImage();
                    break;

                case Keys.F5:
                    toggleFullscreen();
                    break;
            }
        }

        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case '2':
                case 'j':
                case 'l':
                    showNextImage();
                    break;

                case '1':
                case 'k':
                case 'h':
                    showPreviousImage();
                    break;

                case '-':
                    zoomOut();
                    break;

                case '+':
                    zoomIn();
                    break;

                case '0':
                case '=':
                    zoomReset();
                    break;

                case 'q':
                case 'w':
                    Application.Exit();
                    break;

                case 'f':
                    toggleWindowMaximized();
                    break;

                case 'm':
                    toggleFormBorderStyleNone();
                    //cwmEnterChangingWindowMode();
                    break;

                case 'g':
                    toggleFullscreen();
                    break;

                case 'R':
                    updateImageList(currentDirectoryPath);
                    refreshWindow();
                    break;

                case 'r':
                    renameImageFilename();
                    break;

                case 'a':
                    autoResizeWindowMode = false;
                    normalizeWindow();
                    autoResizeWindow();
                    centerWindow();
                    break;

                case 'A':
                    toggleAutoResizeWindowMode();
                    break;

                case 'c':
                    centerWindow();
                    break;

                case 'z':
                    showFirstImage();
                    break;
            }
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private static bool isControlKeyEnabled()
        {
            return (Control.ModifierKeys & Keys.Control) == Keys.Control;
        }

        #endregion

        #region マウス操作

        private Boolean wmiMovingImage = false;
        private Point wmiMovingImagePreviousPoint;

        private Boolean cwmChangingWindowSize = false;

        private void cwmEnterChangingWindowMode()
        {
            cwmChangingWindowSize = true;
            this.Cursor = Cursors.SizeAll;
        }

        private void cwmExitChangingWindowMode()
        {
            cwmChangingWindowSize = false;
            this.Capture = false;
            this.Cursor = Cursors.Default;
        }

        private void mwiEnterMovingImageMode(Point p)
        {
            wmiMovingImage = true;
            wmiMovingImagePreviousPoint = p;
            this.Cursor = Cursors.SizeAll;
        }

        private void mwiExitMovingImageMode()
        {
            wmiMovingImage = false;
            this.Cursor = Cursors.Default;
        }

        private void MainForm_MouseWheel(object sender, MouseEventArgs e)
        {
            if (isControlKeyEnabled())
            {
                if (e.Delta > 0)
                    zoomIn();
                else
                    zoomOut();
            }
            else
            {
                if (e.Delta > 0)
                    showPreviousImage();
                else
                    showNextImage();
            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (cwmChangingWindowSize)
            {
                // 微妙にマウス位置がずれて実装が難しい。
                // 高DPIの問題？クライアント座標・スクリーン座標変換の問題？フォーム影の問題？
                return;
            }
            else if (wmiMovingImage)
            {
                int x = e.Location.X - wmiMovingImagePreviousPoint.X;
                int y = e.Location.Y - wmiMovingImagePreviousPoint.Y;

                currentDrawLocation.X += x;
                currentDrawLocation.Y += y;

                wmiMovingImagePreviousPoint = e.Location;
                if (x != 0 || y != 0)
                    isFixedDrawLocation = true;

                refreshWindow();
            }
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    mwiEnterMovingImageMode(e.Location);
                    break;

                case MouseButtons.Middle:
                    resetCustomView();
                    refreshWindow();
                    break;
            }
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (wmiMovingImage)
                        mwiExitMovingImageMode();
                    else if (cwmChangingWindowSize)
                        cwmExitChangingWindowMode();

                    break;
            }
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (initialized)
                refreshWindow();
        }

        private void resetCustomView()
        {
            calcDefaultZoomRatio();
            calcDefaultDrawLocation();
        }

        #endregion

        #region ドラッグアンドドロップ

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] fileName = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
            updateImageList(fileName[0]);
            changeImage();
        }

        #endregion

        #region 右クリックメニュー

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool enabled = !(currentImage == null);

            toolStripMenuItem_ReloadDirectory.Enabled = true;
            toolStripMenuItem_CopyToClipboard.Enabled = enabled;
            toolStripMenuItem_AutoResizeWindow.Enabled = enabled;
            toolStripMenuItem_CopyDirectoryPathToClipboard.Enabled = enabled;
            toolStripMenuItem_CopyFilePathToClipboard.Enabled = enabled;
            toolStripMenuItem_OpenInExplorer.Enabled = enabled;
        }

        private void ToolStripMenuItem_OpenInExplorer_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(
                "EXPLORER.EXE", String.Format(@"/select,""{0}""", currentImagePath)
            );
        }

        private void ToolStripMenuItem_CopyFilePathToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(currentImagePath);
        }

        private void ToolStripMenuItem_CopyDirectoryPathToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(currentDirectoryPath);
        }

        private void ToolStripMenuItem_ReloadDirectory_Click(object sender, EventArgs e)
        {
            updateImageList(currentDirectoryPath);
            refreshWindow();
        }

        private void ToolStripMenuItem_RunSnippingTool_Click(object sender, EventArgs e)
        {
            //if (!Environment.Is64BitProcess)
            //    System.Diagnostics.Process.Start(@"C:\Windows\sysnative\SnippingTool.exe");
            //else
            //    System.Diagnostics.Process.Start(@"C:\Windows\system32\SnippingTool.exe");
            try
            {
                System.Diagnostics.Process.Start(@"C:\Windows\sysnative\SnippingTool.exe");
                return;
            } catch (Exception)
            {
                // nothing to do
            }

            try
            {
                System.Diagnostics.Process.Start(@"C:\Windows\system32\SnippingTool.exe");
                return;
            }
            catch (Exception)
            {
                // nothing to do
            }
        }

        private void ToolStripMenuItem_ExitApplication_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ToolStripMenuItem_ResetCustomView_Click(object sender, EventArgs e)
        {
            resetCustomView();
            refreshWindow();
        }

        private void ToolStripMenuItem_AutoResizeWindow_Click(object sender, EventArgs e)
        {
            autoResizeWindow();
            centerWindow();
        }

        private void ToolStripMenuItem_CopyToClipboard_Click(object sender, EventArgs e)
        {
            copyToClipboard();
        }

        #endregion

        #region ウインドウ幅保存・復元

        private void restoreWindowSize()
        {
            try
            {
                string width = System.Configuration.ConfigurationManager.AppSettings["WindowWidth"];
                string height = System.Configuration.ConfigurationManager.AppSettings["WindowHeight"];

                this.Height = int.Parse(height);
                this.Width = int.Parse(width);
            } catch (Exception)
            {
                // do nothing
            }
        }

        private void saveWindowSize()
        {
            try
            {
                System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
                if (WindowState == FormWindowState.Maximized)
                {
                    config.AppSettings.Settings["WindowWidth"].Value = this.RestoreBounds.Width.ToString();
                    config.AppSettings.Settings["WindowHeight"].Value = this.RestoreBounds.Height.ToString();
                }
                else
                {
                    config.AppSettings.Settings["WindowWidth"].Value = this.Width.ToString();
                    config.AppSettings.Settings["WindowHeight"].Value = this.Height.ToString();
                }

                config.Save();
            }
            catch (Exception)
            {
                // MessageBox.Show("ImageViewer.exe.configを作成してください。", "設定保存エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveWindowSize();
        }

        #endregion

        #region ディレクトリ監視

        System.IO.FileSystemWatcher imageDirectoryWatcher;
        bool watchDirectoryMode = false;

        private void updateDirectoryWatcher()
        {
            if (watchDirectoryMode)
            {
                if (imageDirectoryWatcher != null)
                {
                    if (imageDirectoryWatcher.Path == this.currentImagePath)
                        return;
                    imageDirectoryWatcher.EnableRaisingEvents = false;
                    imageDirectoryWatcher.Dispose();
                    imageDirectoryWatcher = null;
                }
                watchImageDirectory();
            }
            else
            {
                if (imageDirectoryWatcher != null)
                {
                    imageDirectoryWatcher.EnableRaisingEvents = false;
                    imageDirectoryWatcher.Dispose();
                    imageDirectoryWatcher = null;
                }
            }
        }

        private void watchImageDirectory()
        {
            if (currentImage == null)
            {
                return;
            }

            imageDirectoryWatcher = new System.IO.FileSystemWatcher();
            imageDirectoryWatcher.Path = this.currentDirectoryPath;
            imageDirectoryWatcher.NotifyFilter = System.IO.NotifyFilters.LastWrite;
            imageDirectoryWatcher.IncludeSubdirectories = false;
            imageDirectoryWatcher.SynchronizingObject = this;
            imageDirectoryWatcher.Changed += new System.IO.FileSystemEventHandler(watcher_Changed);
            imageDirectoryWatcher.EnableRaisingEvents = true;
        }

        private void watcher_Changed(System.Object source, System.IO.FileSystemEventArgs e)
        {
            updateImageList(currentDirectoryPath);
            showLastImage();
        }

        private void ToolStripMenuItem_watchDirectory_Click(object sender, EventArgs e)
        {
            if (watchDirectoryMode)
            {
                toolStripMenuItem_watchDirectory.Checked = false;
                watchDirectoryMode = false;
            }
            else
            {
                toolStripMenuItem_watchDirectory.Checked = true;
                watchDirectoryMode = true;
            }
            updateDirectoryWatcher();
        }

        #endregion
    }
}
