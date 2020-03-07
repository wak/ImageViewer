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

        private ImageRepository imageRepository;
        private ImageFile currentImageFile;
        private Image currentImage
        {
            get
            {
                if (currentImageFile == null)
                    return null;
                else
                    return currentImageFile.LoadImage();
            }
        }
        private int currentImageListIndex;

        private string currentDirectoryPath
        {
            get
            {
                if (currentImageFile == null)
                    return imageRepository.repoPath;
                else
                    return imageRepository.repoPath;
            }
        }

        private PaintBoard paintBoard = new PaintBoard();

        private Rectangle currentRectangle;
        private float currentZoomRatio;
        private Point currentDrawLocation;

        private bool IsBreadcrumbsEnabled = true;

        private bool IsIncludeSubDirectory = false;

        private bool isFixedZoomRatio;
        private bool isFixedDrawLocation;

        private bool autoFitWindowMode = false;

        private bool overwrapWait;
        private DateTime overwrapedTime;

        private DateTime lastImageChangedTime;

        private bool initialized = false;

        private bool isRangeOperating = false;
        private int isRangeOperate_StartPosition = 0;

        private TreeForm treeForm = null;

        public MainForm(string path)
        {
            initializeInstanceVariables();

            InitializeComponent();
            this.Icon = FormIcon.appIcon();

            this.MouseWheel += MainForm_MouseWheel;

            restoreWindowSize();
            if (path != null)
                changePathFromUI(path);

            initialized = true;
            changeImage();
        }

        private void showImageTree()
        {
            if (treeForm != null)
            {
                treeForm.Focus();
                return;
            }
            treeForm = new TreeForm(this, currentDirectoryPath, imageRepository, currentImageFile);
            treeForm.itemSelected += ((selectedFile) =>
            {
                overwrapWait = false;
                changeImage(selectedFile);
            });
            treeForm.itemArranged += (() => reloadDirectory());
            treeForm.FormClosed += ((a, b) => treeForm = null);
            treeForm.Show(this);
        }

        private void TreeForm_itemSelected(ImageFile selectedFile)
        {
            changeImage(selectedFile);
        }

        private void initializeInstanceVariables()
        {
            imageRepository = new ImageRepository();
            currentImageListIndex = 0;
            currentZoomRatio = 1.0f;
            isFixedZoomRatio = false;
            currentImageFile = null;
            turnOffOverwrapWait();
            currentDrawLocation.X = currentDrawLocation.Y = 0;
        }

        private void updateImageList(string path)
        {
            turnOffOverwrapWait();

            if (path == null)
            {
                currentImageFile = null;
                currentImageListIndex = 0;
                if (treeForm != null)
                    treeForm.Close();
                return;
            }

            //if (imageRepository != null && imageRepository.repoPath == path)
            //{
            //    imageRepository.Recursive = IsIncludeSubDirectory;
            //    imageRepository.reload();
            //    return;
            //}

            string filepath;
            string directory;
            path = System.IO.Path.GetFullPath(path);

            if (System.IO.File.Exists(path))
            {
                if (System.IO.Path.GetExtension(path).ToLower().EndsWith(".zip"))
                {
                    imageRepository = ImageRepositoryFactory.openRepository(path, IsIncludeSubDirectory);
                    currentImageListIndex = 0;

                    if (treeForm != null)
                        treeForm.changeRepository(imageRepository);
                    return;
                }
                filepath = path;
                directory = System.IO.Path.GetDirectoryName(path);
            }
            else if (System.IO.Directory.Exists(path))
            {
                if (currentImageFile != null && currentImageFile.AbsPath.Contains(path))
                    filepath = currentImageFile.AbsPath;
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

            imageRepository = ImageRepositoryFactory.openRepository(directory, IsIncludeSubDirectory);
            currentImageFile = imageRepository.FindImage(filepath);
            currentImageListIndex = Math.Max(imageRepository.findIndex(filepath), 0);

            if (treeForm != null)
                treeForm.changeRepository(imageRepository);

            changeImage();
            updateDirectoryWatcher();
        }

        private void changePathFromUI(string newPath)
        {
            if (IsIncludeSubDirectory)
            {
                DialogResult r = MessageBox.Show(
                    "サブディレクトリの検索が有効になっています。\r\n" +
                    "サブフォルダも検索対象にしますか？",
                    "確認",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                IsIncludeSubDirectory = (r == DialogResult.Yes);
            }

            updateImageList(newPath);

            if (imageRepository.Count > 0)
                return;
            if (IsIncludeSubDirectory)
                return;

            DialogResult result = MessageBox.Show(
                "画像が見つかりませんでした。\r\n" +
                "サブフォルダも検索対象にしますか？",
                "確認",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
                return;

            IsIncludeSubDirectory = true;
            reloadDirectory();
        }

        public void renameImageFilename()
        {
            if (imageRepository.IsReadonly())
                return;

            if (imageRepository.Count == 0)
                return;

            RenameForm form = new RenameForm(currentImageFile.AbsPath, imageRepository.tree);

            if (form.ShowDialog() == DialogResult.OK)
            {
                string newFilepath = form.result;
                form.Dispose();

                updateImageList(newFilepath);
                updateTreeForm();
                refreshWindow();
            }
        }

        private void updateTreeForm()
        {
            if (treeForm != null)
                treeForm.reload();
        }

        private void reloadDirectory()
        {
            if (imageRepository.IsReadonly())
                return;
            updateImageList(currentDirectoryPath);
        }

        private void copyToClipboard()
        {
            if (currentImage != null)
                Clipboard.SetImage(currentImage);
        }

        public void deleteImage()
        {
            if (currentImageFile == null)
                return;

            DialogResult result = MessageBox.Show("Delete " + currentImageFile.AbsPath + " ?", "Delete File", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                FileSystem.DeleteFile(currentImageFile.AbsPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);

                int save = currentImageListIndex;
                reloadDirectory();
                if (save >= imageRepository.Count)
                    save -= 1;
                currentImageListIndex = Math.Max(save, 0);
                changeImage();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void askOpen()
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();

                ofd.InitialDirectory = currentDirectoryPath;
                ofd.Filter = "対応ファイル|*.bmp;*.jpg;*.jpeg;*.png;*.zip";
                ofd.Title = "開く";

                //ダイアログを表示する
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    updateImageList(ofd.FileName);
                    changeImage();
                }
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        #region Window描画

        private void changeImage(ImageFile imageFile = null)
        {
            if (imageRepository.Count == 0)
            {
                currentImageFile = null;
                refreshWindow();
                return;
            }

            if (imageFile != null)
            {
                int newIndex = imageRepository.findIndex(imageFile.AbsPath);
                if (newIndex >= 0)
                    currentImageListIndex = newIndex;
            }

            currentImageFile = imageRepository[currentImageListIndex];

            if (treeForm != null)
                treeForm.changeSelectedItem(currentImageFile);

            if (autoFitWindowMode)
            {
                autoFitWindow();
            }

            refreshWindow();
        }

        private void prepareToChangeImage()
        {
            currentImageFile = null;
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
            if (currentImageFile == null)
            {
                e.Graphics.Clear(Color.WhiteSmoke);
            }
            else if (currentImageFile.IsImage() && currentImage != null)
            {
                e.Graphics.DrawImage(currentImage, currentRectangle);
                paintBoard.draw(e.Graphics, currentDrawLocation, (float)currentZoomRatio);

                if (rcmRangeCopyModeSelecting)
                {
                    Pen p = new Pen(Color.Black, 1);
                    e.Graphics.DrawRectangle(p, rcmX(), rcmY(), rcmWidth(), rcmHeight());
                    p.Dispose();
                }
            }
            else if (currentImageFile.HasComment())
            {
                using (Font font2 = new Font("Meiryo UI", 30, FontStyle.Bold, GraphicsUnit.Point))
                {
                    Rectangle rect2 = new Rectangle(0, 0, pictureBox.Width, pictureBox.Height);

                    TextFormatFlags flags = TextFormatFlags.HorizontalCenter |
                        TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;

                    TextRenderer.DrawText(e.Graphics, currentImageFile.Comment, font2, rect2, Color.Gray, flags);
                    e.Graphics.DrawRectangle(Pens.Transparent, rect2);
                }
            }

            if (currentImageFile != null && IsBreadcrumbsEnabled)
            {
                using (Font font2 = new Font("Meiryo UI", 8, FontStyle.Bold, GraphicsUnit.Point))
                {
                    string breadcrumbs = makeBreadcrumbs();
                    int height = font2.Height + 10;
                    int width = (int)e.Graphics.MeasureString(breadcrumbs, font2).Width + 10;

                    SolidBrush b = new SolidBrush(Color.FromArgb(190, 255, 255, 255));
                    e.Graphics.FillRectangle(b, 0, 0, width, height);
                    b.Dispose();


                    Rectangle rect2 = new Rectangle(5, 0, width, height);

                    TextFormatFlags flags = TextFormatFlags.VerticalCenter;

                    TextRenderer.DrawText(e.Graphics, breadcrumbs, font2, rect2, Color.Crimson, flags);
                    e.Graphics.DrawRectangle(Pens.Transparent, rect2);
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

        private string makeBreadcrumbs()
        {
            var blist = imageRepository.tree.findTreeByAbsPath(currentImageFile.AbsPath).breadcrumbs("/");
            string breadcrumbs = string.Join(" > ", blist);
            return breadcrumbs;
        }

        private void updateWindowTitle()
        {
            string newTitle = "";
            string flags = "";

            if (isRangeOperating)
                newTitle += "【選択】";

            if (imageRepository.Count > 0)
            {
                newTitle += string.Format(
                    " [{0," + imageRepository.Count.ToString("#").Length + "}/{1}]",
                    currentImageListIndex + 1,
                    imageRepository.Count
                );

                if (currentImageFile != null && currentImageFile.IsImage() && currentImage == null)
                    newTitle += " !!LOAD ERROR!!";

                if (isFixedZoomRatio)
                    flags += "Z";

                if (isFixedDrawLocation)
                    flags += "F";

                if (imageRepository.IsReadonly())
                    newTitle += "[ZIP]";

                if (flags.Length > 0)
                    newTitle += "[" + flags + "]";

                if (CHANGE_IMAGE_WAIT_MSEC > 0)
                    newTitle += string.Format("[{0}ms]", CHANGE_IMAGE_WAIT_MSEC);

                newTitle += string.Format(" {0:0.00}x: {1})",
                    currentZoomRatio, 
                    (currentImageFile == null) ? "" : currentImageFile.Filename);
            }
            this.Text = newTitle;
        }

        #endregion

        #region 画像描画ビュー変更

        private void resetToDefaultView()
        {
            resetCustomView();
            refreshWindow();
            turnOffOverwrapWait();
            paintBoard.clear();
        }

        private float calcZoomRatio(int outerHeight, int outerWidth, int imageHeight, int imageWidth)
        {
            float ratio;

            ratio = Math.Min((float)(outerWidth - PICTURE_BORDER_SIZE * 2) / imageWidth,
                             (float)(outerHeight - PICTURE_BORDER_SIZE * 2) / imageHeight);

            return Math.Min(ratio, 1.0f);
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
            if (!overwrapWait)
                return true;

            double elapsedMiliSeconds = (DateTime.Now - overwrapedTime).TotalMilliseconds;
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

        public void showNextImage()
        {
            prepareToChangeImage();

            if (currentImageListIndex + 1 >= imageRepository.Count)
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

        public void showPreviousImage()
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
                        changeImageListIndex(imageRepository.Count - 1);
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

            currentImageListIndex = imageRepository.Count - 1;
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
            currentZoomRatio = 1.0f;
            isFixedZoomRatio = false;
            refreshWindow();
        }

        private void toggleZoomNative()
        {
            if (currentZoomRatio == 1.0f)
                zoomReset();
            else
                zoomNative();
        }

        private void zoomIn()
        {
            currentZoomRatio += 0.1f;
            isFixedZoomRatio = true;
            refreshWindow();
        }

        private void zoomOut()
        {
            currentZoomRatio = Math.Max(0.0f, currentZoomRatio - 0.1f);
            isFixedZoomRatio = true;
            refreshWindow();
        }

        private void zoomNative()
        {
            currentZoomRatio = 1.0f;
            isFixedZoomRatio = true;
            refreshWindow();
        }

        private void rotateRight90()
        {
            if (currentImage != null)
                currentImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
            refreshWindow();
        }

        private void rotateLeft90()
        {
            if (currentImage != null)
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
                    if (treeForm != null)
                    {
                        treeForm.Close();
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

                case 'F':
                    askOpen();
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
                    showImageTree();
                    break;

                case 'e':
                    launchExplorer();
                    break;

                case (char)Keys.Space:
                    if (!isRangeOperating)
                        ToolStripMenuItem_RangeOpe_FromHere_Click(null, null);
                    else
                        ToolStripMenuItem_RangeOpe_MoveTo_Click(null, null);
                    refreshWindow();
                    break;

                case (char)Keys.Enter:
                    if (anyMouseMode())
                        resetMouseMode();
                    else
                        toggleZoomNative();
                    break;

                case (char)Keys.Tab:
                    askOpen();
                    break;

                case (char)Keys.Back:
                    resetToDefaultView();
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

            if (pmDrawLineMode)
                pmExitDrawLineMode();
        }

        private bool anyMouseMode()
        {
            return cwmChangingWindowSize || wmiMovingImage || mwMovingWindow;
        }

        private Point pointSub(Point l, Point r)
        {
            return new Point(l.X - r.X, l.Y - r.Y);
        }

        private Point pointAdd(Point l, Point r)
        {
            return new Point(l.X + r.X, l.Y + r.Y);
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
        private Point mvMoveDelta;
        private void mwEnterMovingWindowMode(bool center = true)
        {
            this.Cursor = Cursors.NoMove2D;
            resetMouseMode();

            mwMovingWindow = true;
            this.pictureBox.Capture = true;

            Point centerPoint = new Point(
                    this.Location.X + this.Width / 2,
                    this.Location.Y + this.Height / 2
                    );
            if (center)
            {
                Cursor.Position = centerPoint;
                mvMoveDelta = new Point(0, 0);
            }
            else
            {
                mvMoveDelta = pointSub(centerPoint, middleClickScreenPosition);
            }

            mwMoveWindowToCursorPosition();
        }

        private void mwMoveWindowToCursorPosition()
        {
            // フォーム外でコマンドから移動モードを開始した場合、
            // Waitカーソルになっているため、再設定する。
            this.Cursor = Cursors.NoMove2D;

            this.Location = pointAdd(
                new Point(
                    Cursor.Position.X - this.Width / 2,
                    Cursor.Position.Y - this.Height / 2
                ),
                mvMoveDelta
            );
        }

        private void mwExitMovingWindowMode()
        {
            mwMovingWindow = false;
            this.Cursor = Cursors.Default;
            this.pictureBox.Capture = false;
        }
        #endregion

        #region お絵かき
        bool pmDrawLineMode = false;

        private void pmEnterDrawLineMode()
        {
            pmDrawLineMode = true;
            paintBoard.newLine();
        }

        private void pmAddLinePoint(Point p)
        {
            PointF newP = new PointF((p.X - currentDrawLocation.X) / currentZoomRatio, (p.Y - currentDrawLocation.Y) / currentZoomRatio);
            paintBoard.addPoint(newP);
            refreshWindow();
        }

        private void pmExitDrawLineMode()
        {
            pmDrawLineMode = false;
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
            else if (pmDrawLineMode)
            {
                pmAddLinePoint(e.Location);
            }
        }

        private Point rightClickPosition;
        private Point middleClickScreenPosition;

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

                case MouseButtons.Right:
                    rightClickPosition = e.Location;
                    Cursor.Current = Cursors.Hand;
                    pmEnterDrawLineMode();
                    break;

                case MouseButtons.Middle:
                    this.Cursor = Cursors.NoMove2D;
                    middleClickScreenPosition = pictureBox.PointToScreen(e.Location);
                    mwEnterMovingWindowMode(false);
                    break;

                case MouseButtons.XButton1: // backward
                    toggleWatchDirectoryMode();
                    break;

                case MouseButtons.XButton2: // forward
                    toggleZoomNative();
                    break;
            }
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.Default;

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

                case MouseButtons.Right:
                    pmExitDrawLineMode();
                    if (rightClickPosition == e.Location)
                        contextMenuStrip1.Show(pictureBox, e.Location);
                    break;

                case MouseButtons.Middle:
                    if (mwMovingWindow)
                        mwExitMovingWindowMode();

                    if (middleClickScreenPosition != pictureBox.PointToScreen(e.Location))
                        break;

                    if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                    {
                        zoomNative();
                    }
                    else
                    {
                        resetToDefaultView();
                    }

                    break;
            }
        }

        private void PictureBox_DoubleClick(object sender, EventArgs e)
        {
            paintBoard.clear();
            refreshWindow();
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
            changePathFromUI(fileName[0]);
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

            toolStripMenuItem_showBreadcrumbs.Checked = IsBreadcrumbsEnabled;
            toolStripMenuItem_includeSubDirectories.Checked = IsIncludeSubDirectory;
            toolStripMenuItem_toggleVirtualTreeView.Checked = (imageRepository == null || imageRepository.IsVirtualRepository);
        }

        private void ToolStripMenuItem_OpenInExplorer_Click(object sender, EventArgs e)
        {
            launchExplorer();
        }

        private void launchExplorer()
        {
            if (currentImageFile == null)
                return;

            System.Diagnostics.Process process = System.Diagnostics.Process.Start(
                "EXPLORER.EXE", String.Format(@"/select,""{0}""", currentImageFile.AbsPath)
            );
        }

        private void ToolStripMenuItem_CopyFilePathToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(currentImageFile.AbsPath);
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
            if (imageRepository.Count == 0)
                return;
            isRangeOperating = true;
            isRangeOperate_StartPosition = currentImageListIndex;
        }

        private void ToolStripMenuItem_RangeOpe_MoveTo_Click(object sender, EventArgs e)
        {
            if (imageRepository.Count == 0)
                return;
            isRangeOperating = false;
            moveItems(isRangeOperate_StartPosition, currentImageListIndex);
        }

        private void ToolStripMenuItem_MoveAll_Click(object sender, EventArgs e)
        {
            moveItems(0, imageRepository.Count - 1);
        }

        private void ToolStripMenuItem_MoveAllFromHere_Click(object sender, EventArgs e)
        {
            moveItems(currentImageListIndex, imageRepository.Count - 1);
        }

        private void toolStripMenuItem_showBreadcrumbs_Click(object sender, EventArgs e)
        {
            IsBreadcrumbsEnabled = !IsBreadcrumbsEnabled;
            refreshWindow();
        }

        private void moveItems(int index1, int index2)
        {
            if (imageRepository.IsReadonly())
                return;
            if (imageRepository.Count == 0)
                return;

            int fromIndex = Math.Min(index1, currentImageListIndex);
            int count = Math.Abs(index2 - index1) + 1;

            ImageRepository range = imageRepository.GetRange(fromIndex, count);
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

        private void ToolStripMenuItem_openFile_Click(object sender, EventArgs e)
        {
            askOpen();
        }

        private void toolStripMenuItem_openTreeView_Click(object sender, EventArgs e)
        {
            showImageTree();
        }

        private void toolStripMenuItem_includeSubDirectories_Click(object sender, EventArgs e)
        {
            IsIncludeSubDirectory = !IsIncludeSubDirectory;
            reloadDirectory();
        }

        private void toolStripMenuItem_toggleVirtualTreeView_Click(object sender, EventArgs e)
        {
            if (imageRepository != null)
                imageRepository.IsVirtualRepository = !imageRepository.IsVirtualRepository;

            refreshWindow();
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
                    //if (imageDirectoryWatcher.Path == currentImageFile.AbsPath)
                    //    return;
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
            currentImageListIndex = imageRepository.lastUpdatedFileIndex;
            changeImage();
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
