using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;
using Microsoft.VisualBasic.FileIO;

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

        private bool isRangeOperating = false;
        private int isRangeOperate_StartPosition = 0;

        public MainForm(string path)
        {
            initializeInstanceVariables();

            InitializeComponent();
            this.Icon = FormIcon.appIcon();

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
            }
            catch (Exception e)
            {
                MessageBox.Show("名前変更に失敗しました。\n\n" + e.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            updateImageList(newFilepath);
            refreshWindow();
        }

        private void reloadDirectory()
        {
            updateImageList(currentDirectoryPath);
        }

        private void copyToClipboard()
        {
            if (currentImage != null)
                Clipboard.SetImage(currentImage);
        }

        private void deleteImage()
        {
            if (currentImage == null)
                return;

            DialogResult result = MessageBox.Show("Delete " + currentImagePath + " ?", "Delete File", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            try
            {
                //
                // ゴミ箱に移動できない場合に「完全に削除するか」を問うためには、
                // AllDialogs を指定する必要がある。
                //
                // ゴミ箱の設定にて「削除の確認メッセージを表示する」にチェックが入っている場合に
                // 「ゴミ箱に移動するか」も問われるため、上の削除確認MessageBoxと重複してしまう。
                //
                FileSystem.DeleteFile(currentImagePath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);

                int save = currentImageListIndex;
                reloadDirectory();
                if (save >= imageList.Count)
                    save -= 1;
                currentImageListIndex = Math.Max(save, 0);
                changeImage();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

                if (rcmRangeCopyModeSelecting)
                {
                    Pen p = new Pen(Color.Black, 1);
                    e.Graphics.DrawRectangle(p, rcmX(), rcmY(), rcmWidth(), rcmHeight());
                    p.Dispose();
                }
            }
        }

        private void updateWindowTitle()
        {
            string newTitle = "";

            if (isRangeOperating)
                newTitle += "【選択】";

            if (imageList.Count > 0)
            {
                if (currentImage == null)
                    newTitle += " !!LOAD ERROR!!";

                if (overwrapWait)
                {
                    pictureBox.BackColor = Color.FromArgb(115, 199, 255);
                    newTitle += " (!!OVERWRAPPED!!)";
                }
                else
                {
                    pictureBox.BackColor = SystemColors.Control;
                }

                if (isFixedZoomRatio)
                    newTitle += "[FZ]";

                if (isFixedDrawLocation)
                    newTitle += "[FL]";

                newTitle += string.Format(
                    " [{0," + imageList.Count.ToString("#").Length + "}/{1}]",
                    currentImageListIndex + 1,
                    imageList.Count
                );

                newTitle += string.Format(" {0:0.00}x, {1})",
                    currentZoomRatio,
                    System.IO.Path.GetFileName(currentImagePath));
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

            if (currentImageListIndex + 1 >= imageList.Count)
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

        private void zoomNative()
        {
            currentZoomRatio = 1.0;
            isFixedZoomRatio = true;
            refreshWindow();
        }

        private void rotateRight90()
        {
            currentImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
            refreshWindow();
        }

        private void rotateLeft90()
        {
            currentImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
            refreshWindow();
        }

        private void toggleTopMost()
        {
            this.TopMost = !this.TopMost;
        }

        #endregion

        #region 範囲コピー

        private Boolean rcmRangeCopyMode = false;
        private Boolean rcmRangeCopyModeSelecting = false;
        private Point rcmStartPoint;
        private Point rcmCurrentPoint;

        private void copyRectangle()
        {
            using (Bitmap bmp = new Bitmap(pictureBox.Width, pictureBox.Height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(currentImage, currentRectangle);
                    Clipboard.SetDataObject(bmp, true);

                    Rectangle rect = new Rectangle(rcmX(), rcmY(), rcmWidth(), rcmHeight());
                    Bitmap dest = bmp.Clone(rect, bmp.PixelFormat);

                    Clipboard.SetImage(dest);
                }
            }
        }

        private void rcmEnterRangeCopyMode()
        {
            rcmRangeCopyMode = true;
            rcmRangeCopyModeSelecting = false;
            this.Cursor = Cursors.Cross;
        }

        private void rcmStartSelectingRange(Point startPoint)
        {
            rcmStartPoint = startPoint;
            rcmRangeCopyModeSelecting = true;
        }

        private void rcmExitRangeCopyMode(Point endPoint)
        {
            rcmRangeCopyMode = false;
            rcmRangeCopyModeSelecting = false;
            this.Cursor = Cursors.Default;
            copyRectangle();

            refreshWindow();
        }

        private int rcmX()
        {
            return Math.Min(rcmStartPoint.X, rcmCurrentPoint.X);
        }

        private int rcmY()
        {
            return Math.Min(rcmStartPoint.Y, rcmCurrentPoint.Y);
        }

        private int rcmWidth()
        {
            return Math.Abs(rcmStartPoint.X - rcmCurrentPoint.X);
        }

        private int rcmHeight()
        {
            return Math.Abs(rcmStartPoint.Y - rcmCurrentPoint.Y);
        }

        #endregion

        #region Window調整

        private void toggleFullscreen()
        {
            if (this.FormBorderStyle == FormBorderStyle.Sizable)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
            }
            else
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
            int maxHeight = (int)(s.Bounds.Height * 0.9);
            int maxWidth = (int)(s.Bounds.Width * 0.9);

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

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Control | Keys.C:
                    copyToClipboard();
                    break;

                case Keys.Control | Keys.Shift | Keys.C:
                    rcmEnterRangeCopyMode();
                    break;
            }
        }

        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case '2':
                case 'j':
                    showNextImage();
                    break;

                case '1':
                case 'k':
                    showPreviousImage();
                    break;

                case 'l':
                    rotateRight90();
                    break;

                case 'h':
                    rotateLeft90();
                    break;

                case '-':
                    zoomOut();
                    break;

                case '+':
                    zoomIn();
                    break;

                case '0':
                    zoomReset();
                    break;

                case '=':
                    zoomNative();
                    break;

                case 'q':
                case 'w':
                case (char)Keys.Escape:
                    Application.Exit();
                    break;

                case 'f':
                    toggleWindowMaximized();
                    break;

                case 'm':
                    toggleFormBorderStyleNone();
                    break;

                case 'g':
                    toggleFullscreen();
                    break;

                case 'R':
                    reloadDirectory();
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

                case 'c':
                    centerWindow();
                    break;

                case 'A':
                    showFirstImage();
                    break;

                case 'z':
                    showLastImage();
                    break;

                case 'd':
                    deleteImage();
                    break;

                case ',':
                    mwEnterMovingWindowMode();
                    break;

                case '.':
                    cwmEnterChangingWindowMode();
                    break;

                case 't':
                    toggleTopMost();
                    break;

                case (char)Keys.Space:
                    if (!isRangeOperating)
                        ToolStripMenuItem_RangeOpe_FromHere_Click(null, null);
                    else
                        ToolStripMenuItem_RangeOpe_MoveTo_Click(null, null);
                    refreshWindow();
                    break;

                case (char)Keys.Tab:
                    rcmEnterRangeCopyMode();
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

        #region ウインドウサイズ変更
        private Boolean cwmChangingWindowSize = false;

        private void cwmEnterChangingWindowMode()
        {
            cwmChangingWindowSize = true;
            this.Cursor = Cursors.SizeAll;
            this.Capture = true; // フォームの縁のクリックを取得するため。
            Cursor.Position = new Point(this.Location.X + windowSize().X, this.Location.Y + windowSize().Y);

            // フォーム外のマウス操作を取得するため。
            // Win32APIを呼び出したくないため、スレッドで対応する。
            resizeWindowBGWorker = new BackgroundWorker();
            resizeWindowBGWorker.DoWork += ResizeWindowBGWorker_DoWork;
            resizeWindowBGWorker.WorkerSupportsCancellation = true;
            resizeWindowBGWorker.RunWorkerAsync();
        }

        private void cwmExitChangingWindowMode()
        {
            resizeWindowBGWorker.CancelAsync();

            cwmChangingWindowSize = false;
            this.Capture = false;
            this.Cursor = Cursors.Default;
        }

        private delegate void DelegateChangeWindowSize();
        private void ResizeWindowBGWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (resizeWindowBGWorker.CancellationPending)
                    break;

                Thread.Sleep(100); // 100ms
                this.Invoke(new DelegateChangeWindowSize(this.cwmChangeWindowSize));
            }
        }

        private Point windowSize()
        {
            Rectangle a = this.DesktopBounds;
            Rectangle b = this.RectangleToScreen(this.DisplayRectangle);

            Rectangle windowRect = new Rectangle(b.X, a.Y, b.Width, b.Y - a.Y + b.Height);

            return new Point(windowRect.Width, windowRect.Height);
        }

        private void cwmChangeWindowSize()
        {
            Point ws = windowSize();
            
            // フォーム内をクリックするように、少し大きめにする。
            this.Width = Cursor.Position.X - this.Location.X + (this.Width - ws.X) / 2 + 2;
            this.Height = Cursor.Position.Y - this.Location.Y + (this.Height - ws.Y) / 2 + 2;
        }

        private BackgroundWorker resizeWindowBGWorker;
        #endregion

        #region 画像表示位置移動
        private Boolean wmiMovingImage = false;
        private Point wmiMovingImagePreviousPoint;

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
        #endregion

        #region ウィンドウ移動
        private Boolean mwMovingWindow = false;
        private void mwEnterMovingWindowMode()
        {
            mwMovingWindow = true;
            this.Cursor = Cursors.Cross;
            this.pictureBox.Capture = true;

            mwMoveWindowToCursorPosition();
        }

        private void mwMoveWindowToCursorPosition()
        {
            this.Location = new Point(
                Cursor.Position.X - this.Width / 2,
                Cursor.Position.Y - this.Height / 2
                );
        }

        private void mwExitMovingWindowMode()
        {
            mwMovingWindow = false;
            this.Cursor = Cursors.Default;
            this.pictureBox.Capture = false;
        }
        #endregion

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
                cwmChangeWindowSize();
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
            else if (rcmRangeCopyMode)
            {
                rcmCurrentPoint = e.Location;
                refreshWindow();
            }
            else if (mwMovingWindow)
            {
                mwMoveWindowToCursorPosition();
            }
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (cwmChangingWindowSize)
                        cwmExitChangingWindowMode();
                    else if (rcmRangeCopyMode)
                        rcmStartSelectingRange(e.Location);
                    else if (mwMovingWindow)
                        mwExitMovingWindowMode();
                    else
                        mwiEnterMovingImageMode(e.Location);
                    break;

                case MouseButtons.Middle:
                    if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                    {
                        zoomNative();
                    }
                    else
                    {
                        resetCustomView();
                        refreshWindow();
                    }
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
                    else if (rcmRangeCopyMode)
                        rcmExitRangeCopyMode(e.Location);

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
            if (currentImage == null)
                return;

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
            string[] fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
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
            toolStripMenuItem_ToggleAutoResizeMode.Enabled = enabled;
            toolStripMenuItem_ToggleAutoResizeMode.Checked = autoResizeWindowMode;
            toolStripMenuItem_rotateRight.Enabled = enabled;
            toolStripMenuItem_rotateLeft.Enabled = enabled;
            toolStripMenuItem_SetRatio100.Enabled = enabled;

            if (isRangeOperating)
            {
                toolStripMenuItem_RangeOpe_FromHere.Enabled = false;
                toolStripMenuItem_RangeOpe_MoveTo.Enabled = enabled;
            }
            else
            {
                toolStripMenuItem_RangeOpe_FromHere.Enabled = enabled;
                toolStripMenuItem_RangeOpe_MoveTo.Enabled = false;
            }

            toolStripMenuItem_MoveAllFromHere.Enabled = enabled;
            toolStripMenuItem_MoveAll.Enabled = enabled;

            toolStripMenuItem_ToggleTopMost.Checked = this.TopMost;
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
            reloadDirectory();
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
            }
            catch (Exception)
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

        private void ToolStripMenuItem_CopyRectangleToClipboard_Click(object sender, EventArgs e)
        {
            rcmEnterRangeCopyMode();
        }

        private void ToolStripMenuItem_rotateRight_Click(object sender, EventArgs e)
        {
            rotateRight90();
        }

        private void ToolStripMenuItem_rotateLeft_Click(object sender, EventArgs e)
        {
            rotateLeft90();
        }

        private void ToolStripMenuItem_SetRatio100_Click(object sender, EventArgs e)
        {
            zoomNative();
        }

        private void ToolStripMenuItem_Color_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;

            this.Icon = FormIcon.getIcon((string)menu.Tag);
        }

        private void ToolStripMenuItem_RangeOpe_FromHere_Click(object sender, EventArgs e)
        {
            if (currentImage == null)
                return;
            isRangeOperating = true;
            isRangeOperate_StartPosition = currentImageListIndex;
        }

        private void ToolStripMenuItem_RangeOpe_MoveTo_Click(object sender, EventArgs e)
        {
            isRangeOperating = false;
            moveItems(isRangeOperate_StartPosition, currentImageListIndex);
        }

        private void ToolStripMenuItem_MoveAll_Click(object sender, EventArgs e)
        {
            moveItems(0, imageList.Count - 1);
        }

        private void ToolStripMenuItem_MoveAllFromHere_Click(object sender, EventArgs e)
        {
            moveItems(currentImageListIndex, imageList.Count - 1);
        }

        private void moveItems(int index1, int index2)
        {
            int fromIndex = Math.Min(index1, currentImageListIndex);
            int count = Math.Abs(index2 - index1) + 1;

            ImageList range = imageList.GetRange(fromIndex, count);
            MoveForm mf = new MoveForm(range);

            mf.ShowDialog();
            mf.Dispose();
            reloadDirectory();
        }

        private void ToolStripMenuItem_ToggleAutoResizeMode_Click(object sender, EventArgs e)
        {
            toggleAutoResizeWindowMode();
        }

        private void ToolStripMenuItem_ToggleTopMost_Click(object sender, EventArgs e)
        {
            toggleTopMost();
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
            }
            catch (Exception)
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
            if (currentDirectoryPath == null)
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
            reloadDirectory();
            showLastImage();
        }

        private void ToolStripMenuItem_watchDirectory_Click(object sender, EventArgs e)
        {
            if (watchDirectoryMode)
            {
                toolStripMenuItem_watchDirectory.Checked = false;
                watchDirectoryMode = false;
                this.Icon = FormIcon.appIcon();
            }
            else
            {
                toolStripMenuItem_watchDirectory.Checked = true;
                watchDirectoryMode = true;
                this.Icon = FormIcon.greenIcon();
            }
            updateDirectoryWatcher();
        }

        #endregion
    }
}
