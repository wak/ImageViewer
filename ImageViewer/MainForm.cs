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
        const int PICTURE_BORDER_SIZE = 0;
        const long OVERWRAP_WAIT_MSEC = 200; // 200ms

        long CHANGE_IMAGE_WAIT_MSEC = 0;

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

        private bool autoFitWindowMode = false;

        private bool overwrapWait;
        private DateTime overwrapedTime;

        private DateTime lastImageChangedTime;

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
            turnOffOverwrapWait();
            currentDrawLocation.X = currentDrawLocation.Y = 0;
        }

        private bool isZip()
        {
            return imageList is ZipImageList;
        }

        private void updateImageList(string path)
        {
            turnOffOverwrapWait();

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
                if (System.IO.Path.GetExtension(path).ToLower().EndsWith(".zip"))
                {
                    ZipImageList zipImageList = new ZipImageList(path);

                    imageList = zipImageList;
                    currentImageListIndex = 0;
                    imageLoader = zipImageList.getImageLoader();
                    return;
                }
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

            if (imageLoader is ZipImageLoader)
                imageLoader = new ImageLoader();

            imageList = new ImageList(directory);
            currentDirectoryPath = directory;
            currentImagePath = filepath;
            currentImageListIndex = Math.Max(imageList.findIndex(filepath), 0);

            changeImage();
            updateDirectoryWatcher();
        }

        private void renameImageFilename()
        {
            if (isZip())
                return;

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
            if (isZip())
                return;
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

            if (autoFitWindowMode)
            {
                autoFitWindow();
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

            if (autoFitWindowMode)
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

            if (overwrapWait)
            {
                SolidBrush b = new SolidBrush(Color.FromArgb(190, 210, 210, 210));
                e.Graphics.FillRectangle(b, 0, 0, pictureBox.Width, pictureBox.Height);
                b.Dispose();

                using (Font font2 = new Font("Meiryo UI", 20, FontStyle.Bold, GraphicsUnit.Point))
                {
                    Rectangle rect2 = new Rectangle(0, 0, pictureBox.Width, pictureBox.Height);

                    TextFormatFlags flags = TextFormatFlags.HorizontalCenter |
                        TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;

                    TextRenderer.DrawText(e.Graphics, "(overwrapped)", font2, rect2, Color.Blue, flags);
                    e.Graphics.DrawRectangle(Pens.Transparent, rect2);
                }
            }
        }

        private void updateWindowTitle()
        {
            string newTitle = "";
            string flags = "";

            if (isRangeOperating)
                newTitle += "【選択】";

            if (imageList.Count > 0)
            {
                newTitle += string.Format(
                    " [{0," + imageList.Count.ToString("#").Length + "}/{1}]",
                    currentImageListIndex + 1,
                    imageList.Count
                );

                if (currentImage == null)
                    newTitle += " !!LOAD ERROR!!";

                if (isFixedZoomRatio)
                    flags += "Z";

                if (isFixedDrawLocation)
                    flags += "F";

                if (isZip())
                    newTitle += "[ZIP]";

                if (flags.Length > 0)
                    newTitle += "[" + flags + "]";

                if (CHANGE_IMAGE_WAIT_MSEC > 0)
                    newTitle += string.Format("[{0}ms]", CHANGE_IMAGE_WAIT_MSEC);

                newTitle += string.Format(" {0:0.00}x: {1})",
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

        private void turnOnOverwrapWait()
        {
            overwrapedTime = DateTime.Now;
            overwrapWait = true;
        }

        private bool turnOffOverwrapWait()
        {
            double elapsedMiliSeconds = (DateTime.Now - overwrapedTime).TotalMilliseconds;

            if (!overwrapWait)
                return true;

            if (elapsedMiliSeconds > OVERWRAP_WAIT_MSEC)
            {
                overwrapWait = false;
                return true;
            }

            return false;
        }

        private void changeImageListIndex(int index)
        {
            lastImageChangedTime = DateTime.Now;
            currentImageListIndex = index;
        }

        private bool isChangeImageWaitElapsed()
        {
            if (CHANGE_IMAGE_WAIT_MSEC <= 0)
                return true;

            double elapsedMiliSeconds = (DateTime.Now - lastImageChangedTime).TotalMilliseconds;

            if (elapsedMiliSeconds > CHANGE_IMAGE_WAIT_MSEC)
                return true;
            else
                return false;
        }

        private void showNextImage()
        {
            prepareToChangeImage();

            if (currentImageListIndex + 1 >= imageList.Count)
            {
                if (!overwrapWait)
                {
                    turnOnOverwrapWait();
                }
                else
                {
                    if (turnOffOverwrapWait())
                        changeImageListIndex(0);
                }
            }
            else
            {
                if (!overwrapWait)
                {
                    changeImageListIndex(currentImageListIndex + 1);
                }
                turnOffOverwrapWait();
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
                    turnOnOverwrapWait();
                }
                else
                {
                    if (turnOffOverwrapWait())
                        changeImageListIndex(imageList.Count - 1);
                }
            }
            else
            {
                if (!overwrapWait)
                {
                    changeImageListIndex(currentImageListIndex - 1);
                }
                turnOffOverwrapWait();
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
                this.pictureBox.BorderStyle = BorderStyle.None;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
                this.pictureBox.BorderStyle = BorderStyle.None;
            }
        }

        private void toggleWindowMaximized()
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
                this.pictureBox.BorderStyle = BorderStyle.None;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                this.pictureBox.BorderStyle = BorderStyle.None;
                refreshWindow(); // AutoFitMode時にうまく中央表示できないため。
            }
        }

        private void toggleFormBorderStyleNone()
        {
            if (this.FormBorderStyle == FormBorderStyle.Sizable)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.pictureBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.pictureBox.BorderStyle = BorderStyle.None;
            }
        }

        private void toggleAutoFitWindowMode()
        {
            autoFitWindowMode = !autoFitWindowMode;

            if (autoFitWindowMode)
            {
                autoFitWindow();
            }
            refreshWindow();
        }

        private void normalizeWindow()
        {
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Normal;
        }

        private void autoFitWindow()
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
                    if (!isChangeImageWaitElapsed())
                        break;
                    showNextImage();
                    break;

                case '1':
                case 'k':
                    if (!isChangeImageWaitElapsed())
                        break;
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
                    if (overwrapWait)
                    {
                        turnOffOverwrapWait();
                        refreshWindow();
                        break;
                    }
                    if (anyMouseMode())
                        resetMouseMode();
                    else
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
                    autoFitWindowMode = false;
                    autoFitWindow();
                    centerWindow();
                    break;

                case 'c':
                    centerWindow();
                    break;

                case 'A':
                    showFirstImage();
                    break;

                case 's':
                    if (CHANGE_IMAGE_WAIT_MSEC > 0)
                        CHANGE_IMAGE_WAIT_MSEC = 0;
                    else
                        CHANGE_IMAGE_WAIT_MSEC = 250;
                    refreshWindow();
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

                case '/':
                    enableWatchDirectoryMode();
                    this.FormBorderStyle = FormBorderStyle.None;

                    System.Windows.Forms.Screen s = System.Windows.Forms.Screen.FromControl(this);
                    this.Height = (int)(s.Bounds.Height / 8);
                    this.Width = (int)(s.Bounds.Width / 15);
                    this.TopMost = true;
                    this.pictureBox.BorderStyle = BorderStyle.FixedSingle;
                    mwEnterMovingWindowMode();
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

                case (char)Keys.Enter:
                    resetMouseMode();
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

        private void resetMouseMode()
        {
            if (cwmChangingWindowSize)
                cwmExitChangingWindowMode();

            if (wmiMovingImage)
                mwiExitMovingImageMode();

            if (mwMovingWindow)
                mwExitMovingWindowMode();
        }

        private bool anyMouseMode()
        {
            return cwmChangingWindowSize || wmiMovingImage || mwMovingWindow;
        }

        #region ウインドウサイズ変更
        private Boolean cwmChangingWindowSize = false;
        private BackgroundWorker resizeWindowBGWorker;
        private ManualResetEvent cwmResetEvent = new ManualResetEvent(false);

        private void cwmEnterChangingWindowMode()
        {
            resetMouseMode();

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
            cwmResetEvent.WaitOne();

            cwmChangingWindowSize = false;
            this.Capture = false;
            this.Cursor = Cursors.Default;
        }

        private delegate void DelegateChangeWindowSize();
        private void ResizeWindowBGWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!resizeWindowBGWorker.CancellationPending)
            {
                this.Invoke(new DelegateChangeWindowSize(this.cwmChangeWindowSize));
                Thread.Sleep(100); // 100ms
            }
            cwmResetEvent.Set();
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
        #endregion

        #region 画像表示位置移動
        private Boolean wmiMovingImage = false;
        private Point wmiMovingImagePreviousPoint;

        private void mwiEnterMovingImageMode(Point p)
        {
            resetMouseMode();

            wmiMovingImage = true;
            wmiMovingImagePreviousPoint = p;
            this.Cursor = Cursors.SizeAll;
        }

        private void mwiExitMovingImageMode()
        {
            wmiMovingImage = false;
            this.Cursor = Cursors.Default;
        }

        private void mwiMoveImage(MouseEventArgs e)
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
        #endregion

        #region ウィンドウ移動
        private Boolean mwMovingWindow = false;
        private void mwEnterMovingWindowMode()
        {
            resetMouseMode();

            mwMovingWindow = true;
            this.Cursor = Cursors.Cross;
            this.pictureBox.Capture = true;

            Cursor.Position = new Point(
                this.Location.X + this.Width / 2,
                this.Location.Y + this.Height / 2
                );

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
                mwiMoveImage(e);
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
            turnOffOverwrapWait();

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
                    turnOffOverwrapWait();
                    break;

                case MouseButtons.XButton1: // backward
                    toggleWatchDirectoryMode();
                    break;

                case MouseButtons.XButton2: // forward
                    zoomNative();
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
            toolStripMenuItem_AutoFitWindow.Enabled = enabled;
            toolStripMenuItem_CopyDirectoryPathToClipboard.Enabled = enabled;
            toolStripMenuItem_CopyFilePathToClipboard.Enabled = enabled;
            toolStripMenuItem_OpenInExplorer.Enabled = enabled;
            toolStripMenuItem_ToggleAutoFitMode.Enabled = enabled;
            toolStripMenuItem_ToggleAutoFitMode.Checked = autoFitWindowMode;
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

        private void ToolStripMenuItem_AutoFitWindow_Click(object sender, EventArgs e)
        {
            autoFitWindow();
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
            if (isZip())
                return;

            int fromIndex = Math.Min(index1, currentImageListIndex);
            int count = Math.Abs(index2 - index1) + 1;

            ImageList range = imageList.GetRange(fromIndex, count);
            MoveForm mf = new MoveForm(range);

            mf.ShowDialog();
            mf.Dispose();
            reloadDirectory();
        }

        private void ToolStripMenuItem_ToggleAutoFitWindowMode_Click(object sender, EventArgs e)
        {
            toggleAutoFitWindowMode();
        }

        private void ToolStripMenuItem_ToggleTopMost_Click(object sender, EventArgs e)
        {
            toggleTopMost();
        }

        private void ToolStripMenuItem_MoveWindow_Click(object sender, EventArgs e)
        {
            mwEnterMovingWindowMode();
        }

        private void ToolStripMenuItem_ChangeWindowSize_Click(object sender, EventArgs e)
        {
            cwmEnterChangingWindowMode();
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
            toggleWatchDirectoryMode();
        }

        private void toggleWatchDirectoryMode()
        {
            if (watchDirectoryMode)
            {
                disableWatchDirectoryMode();
            }
            else
            {
                enableWatchDirectoryMode();
            }
        }

        private void enableWatchDirectoryMode()
        {
            toolStripMenuItem_watchDirectory.Checked = true;
            watchDirectoryMode = true;
            this.Icon = FormIcon.greenIcon();
            updateDirectoryWatcher();
        }

        private void disableWatchDirectoryMode()
        {
            toolStripMenuItem_watchDirectory.Checked = false;
            watchDirectoryMode = false;
            this.Icon = FormIcon.appIcon();
            updateDirectoryWatcher();
        }

        #endregion
    }
}
