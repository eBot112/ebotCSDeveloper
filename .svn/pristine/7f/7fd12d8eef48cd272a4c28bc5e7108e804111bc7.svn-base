using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using eBotLib.Capture;

namespace eBotLib
{
    /// <summary>
    /// デバイスキャプチャ 制御コントロール
    /// </summary>
    public partial class DevelopmentControl : UserControl, IDisposable
    {
        /// <summary>スナップショットの保存先</summary>
        private static readonly string SNAPSHOT_DIRECTORY = "snapshots";
        /// <summary>テンプレートの初期フォルダ</summary>
        private static readonly string TEMPLATE_INIT_DIRECTORY = "templates";

        /// <summary>選択されたテンプレート画像</summary>
        private Bitmap selectedTemplate = null;
        /// <summary>最新のスナップショット画像</summary>
        private Bitmap snapshotImage = null;
        /// <summary>テンプレートのファイル</summary>
        private string selectedTemplateFile = null;
        /// <summary>ウインドウハンドル検索用タイマ</summary>
        private Timer updateWindowSearchTimer;
        /// <summary>選択矩形定義開始フラグ</summary>
        private bool isSelectRectExecuting = false;
        /// <summary>選択位置</summary>
        private Point selectPoint = new Point();
        /// <summary>選択矩形</summary>
        private Rectangle selectRect = new Rectangle();
        /// <summary>認識矩形</summary>
        private Rectangle searchRect = new Rectangle();
        /// <summary>Tittan One API</summary>
        private TitanWrapper.Wrapper titan;
        /// <summary>Titan入力状態</summary>
        private bool titanInputIsDown = false;
        /// <summary>OCR</summary>
        private eBotLib.OCR.TesseractWrap ocr;

        [System.Runtime.InteropServices.DllImport("winmm.dll", SetLastError = true)]
        private static extern UInt32 timeBeginPeriod(UInt32 uMilliseconds);
        [System.Runtime.InteropServices.DllImport("winmm.dll", SetLastError = true)]
        private static extern uint timeEndPeriod(UInt32 uMilliseconds);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DevelopmentControl()
        {
            InitializeComponent();

            this.Disposed += (sender, args) =>
            {
                timeEndPeriod(1);
            };
        }

        private void DevelopmentConsole_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            timeBeginPeriod(1);


            if (!System.IO.Directory.Exists(SNAPSHOT_DIRECTORY))
            {
                System.IO.Directory.CreateDirectory(SNAPSHOT_DIRECTORY);
            }
            if (!System.IO.Directory.Exists(TEMPLATE_INIT_DIRECTORY))
            {
                System.IO.Directory.CreateDirectory(TEMPLATE_INIT_DIRECTORY);
            }

            this.previewDirectCapturePictureBox.SnapShotCompleteHandler += PreviewDirectCapturePictureBox_SnapShotCompleteHandler;

            // デバイス一覧の取得
            this.deviceSelectComboBox.Items.Clear();
            foreach (string s in DirectCapturePictureBox.GetCaptureDevices())
            {
                this.deviceSelectComboBox.Items.Add(s);
            }
            if (0 != this.deviceSelectComboBox.Items.Count)
            {
                this.deviceSelectComboBox.SelectedIndex = 0;
            }

            this.templateNameLabel.Text = "";
            this.templateSizeLabel.Text = "";
            this.imageSearchAreaLabel.Text = "SearchArea(Image): " + searchRect + "[px]";

            // ウインドウハンドル検索用タイマ設定
            updateWindowSearchTimer = new Timer();
            updateWindowSearchTimer.Interval = 30;
            updateWindowSearchTimer.Tick += new EventHandler(updateWindowSearchTimer_Tick);
            updateWindowSearchTimer.Start();
        }

        /// <summary>
        /// デバイス初期化ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void initDeviceButton_Click(object sender, EventArgs e)
        {
            this.analysisPreview.Out("Start initialize device.");
            this.previewDirectCapturePictureBox.InitDevice(this.deviceSelectComboBox.SelectedIndex);
            this.analysisPreview.Out("Complete initialize device.");
        }


        /// <summary>
        /// スナップショット要求ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void reqSnapshotButton_Click(object sender, EventArgs e)
        {
            this.analysisPreview.Out("Request snapshot.");
            this.previewDirectCapturePictureBox.RequestCaptureSnapshot();
        }

        /// <summary>
        /// スナップショット保存ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveSnapshotButton_Click(object sender, EventArgs e)
        {
            string fname = DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".bmp";
            this.analysisPreview.ImageSave(SNAPSHOT_DIRECTORY + "\\" + fname);
            this.analysisPreview.Out("Save snapshot: " + fname);
        }

        /// <summary>
        /// スナップショット保存フォルダをエクスプローラで開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openSnapshotDirectoryButton_Click(object sender, EventArgs e)
        {
            this.analysisPreview.Out("Open snapshot directory: " + System.IO.Directory.GetCurrentDirectory() + "\\" + SNAPSHOT_DIRECTORY);
            System.Diagnostics.Process.Start(SNAPSHOT_DIRECTORY);
        }

        /// <summary>
        /// セレクトエリアの画像保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveSelectAreaButton_Click(object sender, EventArgs e)
        {
            string fname = DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".bmp";
            this.analysisPreview.ImageSave(TEMPLATE_INIT_DIRECTORY + "\\" + fname, selectRect);
            this.analysisPreview.Out("Save select area image: " + fname);
        }

        /// <summary>
        /// テンプレート選択ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectTemplateButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            string templateImageName;
            string templateImageSize;

            string initDir = System.IO.Directory.GetCurrentDirectory() + "\\" + TEMPLATE_INIT_DIRECTORY;

            ofd.InitialDirectory = initDir;
            ofd.Filter = "BMPファイル(*.bmp;*.BMP)|*.bmp;*.BMP|すべてのファイル(*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                this.selectedTemplateFile = ofd.FileName;
                this.selectedTemplate = new Bitmap(this.selectedTemplateFile, true);
                this.templatePictureBox.Image = this.selectedTemplate;

                templateImageName = System.IO.Path.GetFileName(this.selectedTemplateFile);
                templateImageSize = "(S:" + this.templatePictureBox.Image.Width.ToString("0000") + "," +
                    this.templatePictureBox.Image.Height.ToString("0000") + ")[px]";
                this.templateNameLabel.Text = templateImageName;
                this.templateSizeLabel.Text = templateImageSize;

                this.analysisPreview.Out("Template selected: " + templateImageName + templateImageSize);
            }
        }

        /// <summary>
        /// マッチング開始ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void executeMatcingButton_Click(object sender, EventArgs e)
        {
            if (null == selectedTemplate) {
                this.analysisPreview.Out("Matching Cancel: Template image not found.");
                return;
            }
            if (null == this.snapshotImage)
            {
                this.analysisPreview.Out("Matching Cancel: Snapshot image not found.");
                return;
            }
            if (0 == searchRect.Width || 0 == searchRect.Height)
            {
                this.analysisPreview.Out("Matching Cancel: Search area is not set.");
                return;
            }
            if (selectedTemplate.Width > searchRect.Width || selectedTemplate.Height > searchRect.Height)
            {
                this.analysisPreview.Out("Matching Cancel: The search area is smaller than the template.");
                return;
            }

            bool result;
            double resultValue;
            Rectangle resultRect;
            double matingThreshold = Vision.Matching.DEFAULT_MATCH_THRESHOLD;

            this.analysisPreview.Out("Execute matching: " + System.IO.Path.GetFileName(this.selectedTemplateFile));

            result = Vision.Matching.ImageMatching(this.snapshotImage, this.selectedTemplate, matingThreshold,
                searchRect, out resultRect, out resultValue);

            this.analysisPreview.RecognitionOut(result, resultValue, matingThreshold, resultRect, searchRect);

            this.analysisPreview.Out("Complete matching.");
        }

        /// <summary>
        /// サーチエリア決定ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setSearchAreaButton_Click(object sender, EventArgs e)
        {
            searchRect = selectRect;
            this.imageSearchAreaLabel.Text = "SearchArea(Image): " + searchRect + "[px]";
        }

        /// <summary>
        /// サーチエリア クリップボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setSearchAreaClipboardButton_Click(object sender, EventArgs e)
        {
            if (null == searchRect) return;
            Clipboard.SetText(searchRect.X + ", " + searchRect.Y + ", " + searchRect.Width + ", " + searchRect.Height);
            this.analysisPreview.Out("Set Clipboard Search Area: \"" + searchRect.ToString() + "\"");
        }

        /// <summary>
        /// スナップショット取得イベント
        /// </summary>
        /// <param name="args"></param>
        private void PreviewDirectCapturePictureBox_SnapShotCompleteHandler(SnapShotCompleteEventArgs args)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => PreviewDirectCapturePictureBox_SnapShotCompleteHandler(args)));
                return;
            }

            Bitmap tempImage = this.snapshotImage;
            this.snapshotImage = args.Bitmap;
            this.analysisPreview.DrawImage(snapshotImage);
            if (null != tempImage)
            {
                tempImage.Dispose();
            }
            this.analysisPreview.Out("Complete snapshot update.");
        }

        /// <summary>
        /// Titan初期化ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void titanInitButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (null == titan)
                {
                    titan = new TitanWrapper.Wrapper();
                }

                titan.Init();

                this.analysisPreview.Out("Titan initialization completed: " + 
                    titan.oneApi.CurrentOutputType + "(" + (int)titan.oneApi.CurrentOutputType + ")");

                this.titanKeySelectComboBox.Items.Clear();
                foreach (string map in this.titan.oneApi.GetKeyMap(this.titan.oneApi.CurrentOutputType))
                {
                    this.titanKeySelectComboBox.Items.Add(map);
                }
                if (0 != this.titanKeySelectComboBox.Items.Count)
                {
                    this.titanKeySelectComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                this.analysisPreview.Out("Titan initialization error occured: " + ex.ToString());
            }
        }

        /// <summary>
        /// OCR 初期化ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ocrInitButton_Click(object sender, EventArgs e)
        {
            ocr = new OCR.TesseractWrap();

            // OCR セグメントモード一覧取得
            this.ocrPageSegModeComboBox.DataSource = Enum.GetValues(typeof(Tesseract.PageSegMode));
            this.ocrPageSegModeComboBox.SelectedIndex = this.ocrPageSegModeComboBox.FindStringExact("Auto");
            string[] langpacks = System.IO.Directory.GetFiles(ocr.LanguageFilePath, "*.traineddata");
            for (int i = 0; i < langpacks.Length; i++)
            {
                langpacks[i] = System.IO.Path.GetFileNameWithoutExtension(langpacks[i]);
            }
            this.ocrLanguagePackComboBox.DataSource = langpacks;
            this.ocrWhiteListTextBox.Text = ocr.WhiteList;
            this.ocrConfidenceTextBox.Text = "40.0";
        }

        /// <summary>
        /// OCR 認識実行ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ocrRecButton_Click(object sender, EventArgs e)
        {
            OCR.TesseractWrap.Result result;
            float threshold;

            if (null == ocr || null == snapshotImage || null == searchRect) return;

            ocr.SegMode = (Tesseract.PageSegMode)this.ocrPageSegModeComboBox.SelectedIndex;
            ocr.LanguagePackSelect = this.ocrLanguagePackComboBox.SelectedItem.ToString();
            ocr.WhiteList = this.ocrWhiteListTextBox.Text;
            float.TryParse(this.ocrConfidenceTextBox.Text, out threshold);
            ocr.EngineUpdate();

            result = ocr.Execute(snapshotImage, searchRect);
            this.analysisPreview.Out("OCR: " + result.Text);
            this.analysisPreview.OCROut(result, searchRect, threshold);
        }

        /// <summary>
        /// OCR情報コピーボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ocrClipboardButton_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(searchRect.X + ", " + searchRect.Y + ", " + searchRect.Width + ", " + searchRect.Height +
                "," + "\"" + this.ocrLanguagePackComboBox.SelectedItem.ToString() + "\"" + "," + this.ocrPageSegModeComboBox.SelectedIndex +
                "," + "\"" + this.ocrWhiteListTextBox.Text + "\"");
            this.analysisPreview.Out("Set Clipboard OCR");
        }

        /// <summary>
        /// Titan 入力ボタン マウスダウン
        /// </summary>
        private void titanInputButton_MouseDown(object sender, MouseEventArgs e)
        {
            this.titan.InputButton(this.titanKeySelectComboBox.SelectedIndex, (int)TitanWrapper.TitanOne.KEY_STATE.DOWN);
            this.titanInputIsDown = true;
        }

        /// <summary>
        /// Titan 入力ボタン マウスアップ
        /// </summary>
        private void titanInputButton_MouseUp(object sender, MouseEventArgs e)
        {
            this.titan.InputButton(this.titanKeySelectComboBox.SelectedIndex, (int)TitanWrapper.TitanOne.KEY_STATE.UP);
            this.titanInputIsDown = false;
        }


        /// <summary>
        /// ウインドウハンドル検索用タイマ チック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateWindowSearchTimer_Tick(object sender, EventArgs e)
        {
            PointF analysisCursor = analysisPreview.PointToClient(Cursor.Position);
            PointF imageCursor = analysisPreview.GetImageLocation(analysisCursor);

            // 範囲選択
            if (isSelectRectExecuting)
            {
                int left = (int)Math.Round(Math.Min(selectPoint.X, imageCursor.X));
                int right = (int)Math.Round(Math.Max(selectPoint.X, imageCursor.X));
                int top = (int)Math.Round(Math.Min(selectPoint.Y, imageCursor.Y));
                int bottom = (int)Math.Round(Math.Max(selectPoint.Y, imageCursor.Y));

                selectRect = new Rectangle(left, top, right - left, bottom - top);
                analysisPreview.RectOut(selectRect);
            }

            // 選択矩形　開始・終了
            if (!isSelectRectExecuting && (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left)
            {
                bool inside = analysisPreview.ClientRectangle.Contains((int)Math.Round(analysisCursor.X), (int)Math.Round(analysisCursor.Y));
                if (inside)
                {
                    selectPoint = new Point((int)Math.Round(imageCursor.X), (int)Math.Round(imageCursor.Y));
                    selectRect = new Rectangle(selectPoint.X, selectPoint.Y, 0, 0);
                    isSelectRectExecuting = true;
                }
            }
            else if (isSelectRectExecuting && (Control.MouseButtons & MouseButtons.Left) != MouseButtons.Left)
            {
                isSelectRectExecuting = false;
            }

            if ((Control.MouseButtons & MouseButtons.Left) != MouseButtons.Left &&
                (Control.MouseButtons & MouseButtons.Right) != MouseButtons.Right)
            {
                // ボタン範囲外でマウスアップした場合の保護
                if (this.titanInputIsDown)
                {
                    this.titan.ClearButton((int)TitanWrapper.TitanOne.KEY_STATE.UP);
                }
            }

            if (null != this.titan)
            {
                this.titanSelectedDeviceLabel.Text = "Device: " +
                    this.titan.oneApi.CurrentOutputType + "(" + (int)this.titan.oneApi.CurrentOutputType + ")" +
                    "/" + ((this.titan.IsConnected()) ? "Connected" : "Disconnected");
            }

            this.imageCursorLabel.Text = "Cursor(Image): " + imageCursor + "[px]";
            this.imageSelectAreaLabel.Text = "SelectArea(Image): " + selectRect + "[px]";
        }
    }
}
