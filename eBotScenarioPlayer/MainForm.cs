using eBotLib.Capture;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace eBotScenarioPlayer
{
    public partial class MainForm : Form
    {
        [System.Runtime.InteropServices.DllImport("winmm.dll", SetLastError = true)]
        private static extern UInt32 timeBeginPeriod(UInt32 uMilliseconds);
        [System.Runtime.InteropServices.DllImport("winmm.dll", SetLastError = true)]
        private static extern uint timeEndPeriod(UInt32 uMilliseconds);

        /// <summary>ワイプ画面のサイズ</summary>
        private static readonly Size WIPE_SIZE = new Size(320, 180);

        /// <summary>設定</summary>
        private Config config;
        /// <summary>コントローラ</summary>
        private TitanWrapper.Wrapper titan;
        /// <summary>パッド</summary>
        private eBotLib.Controller.TitanPad titanPad = null;
        /// <summary>状態更新用タイマ</summary>
        private Timer statusUpdateTimer;
        /// <summary>シナリオ</summary>
        private dynamic scenario;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            this.statusUpdateTimer = new Timer();
            this.statusUpdateTimer.Interval = 30;
            this.statusUpdateTimer.Tick += new EventHandler(this.statusUpdateTimer_Tick);
            this.statusUpdateTimer.Start();

            this.Disposed += (sender, args) =>
            {
                timeEndPeriod(1);
            };
        }

        /// <summary>
        /// ロード処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            System.Diagnostics.FileVersionInfo ver =
                System.Diagnostics.FileVersionInfo.GetVersionInfo(
                    System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.Text = "eBot Scenario Player - build:" + ver.FileVersion + " / lib:" + eBotLib.Manager.GetLibFileVersion();

            timeBeginPeriod(1);

            // シナリオフォルダ作成
            if (!System.IO.Directory.Exists(Config.SCENARIOS_DIRECTORY))
            {
                System.IO.Directory.CreateDirectory(Config.SCENARIOS_DIRECTORY);
            }

            // 設定読み込み
            config = Config.Load();

            // キャプチャデバイス初期化
            this.deviceSelectComboBox.DataSource = DirectCapturePictureBox.GetCaptureDevices();
            if (0 != this.deviceSelectComboBox.Items.Count)
            {
                this.deviceSelectComboBox.SelectedIndex = config.SelectedCaptureDeviceIndex;
            }
            this.deviceSelectComboBox.SelectedIndexChanged += new System.EventHandler(this.deviceSelectComboBox_SelectedIndexChanged);

            // シナリオ検索
            int scenarioIdx;
            this.scenarioSelectComboBox.DataSource = System.IO.Directory.GetFiles(Config.SCENARIOS_DIRECTORY, "*.dll", System.IO.SearchOption.AllDirectories);
            scenarioIdx = this.scenarioSelectComboBox.FindStringExact(config.SelectedScenario);
            if (0 <= scenarioIdx)
            {
                this.scenarioSelectComboBox.SelectedIndex = scenarioIdx;
            }
            else
            {
                this.scenarioSelectComboBox.SelectedIndex = 0;
                config.SelectedScenario = this.scenarioSelectComboBox.Items[this.scenarioSelectComboBox.SelectedIndex].ToString();
            }
            this.scenarioSelectComboBox.SelectedIndexChanged += new System.EventHandler(this.scenarioSelectComboBox_SelectedIndexChanged);

            viewStyleUpdate();

            initAllDevices();
            initScenario();
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            config.Save();
        }

        /// <summary>
        /// キャプチャデバイス選択コンボボックス選択変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deviceSelectComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // デバイスの初期化
            config.SelectedCaptureDeviceIndex = deviceSelectComboBox.SelectedIndex;
            initCaptureDevice();
        }

        /// <summary>
        /// シナリオ選択コンボボックス選択変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scenarioSelectComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // シナリオの初期化
            config.SelectedScenario = scenarioSelectComboBox.Items[scenarioSelectComboBox.SelectedIndex].ToString();
            initScenario();
        }

        /// <summary>
        /// ビューのスタイルを設定
        /// </summary>
        private void viewStyleUpdate()
        {
            dynamic mainView, wipeView;

            mainView = analysisPreview;
            wipeView = previewDirectCapturePictureBox;

            mainView.Visible = true;
            if ((int)Config.VIEW_STYLE.AnalysisOnly == config.SelectedViewStyle)
            {
                wipeView.Visible = false;
            }
            else
            {
                wipeView.Visible = true;
            }

            mainView.Parent = this.viewStylePanel;
            wipeView.Parent = mainView;

            mainView.BorderStyle = BorderStyle.None;
            wipeView.BorderStyle = BorderStyle.FixedSingle;

            mainView.Location = new Point(0, 0);
            mainView.Size = this.viewStylePanel.Size;

            wipeView.Location = new Point(
                mainView.Width - wipeView.Width - 20,
                mainView.Height - wipeView.Height - 20);
            wipeView.Size = WIPE_SIZE;

            mainView.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right);
            wipeView.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
        }

        /// <summary>
        /// キャプチャデバイスの初期化
        /// </summary>
        private void initCaptureDevice()
        {
            this.analysisPreview.Out("Initialize capture device.");
            this.previewDirectCapturePictureBox.InitDevice(config.SelectedCaptureDeviceIndex);
        }

        /// <summary>
        /// シナリオの初期化
        /// </summary>
        private void initScenario()
        {
            if (null == config.SelectedScenario) return;

            this.analysisPreview.Out("Initialize scenario.");
            var asm = System.Reflection.Assembly.LoadFrom(config.SelectedScenario);
            var module = asm.GetModule(System.IO.Path.GetFileName(config.SelectedScenario));
            var person = module.GetType(System.IO.Path.GetFileNameWithoutExtension(config.SelectedScenario) + ".Scenario");
            scenario = Activator.CreateInstance(person, previewDirectCapturePictureBox, titan, analysisPreview);

            this.helpToolTip.SetToolTip(this.scenarioHelpPanel, scenario.HelpTipMessage);
        }

        /// <summary>
        /// デバイスの初期化
        /// </summary>
        private void initAllDevices()
        {
            // キャプチャデバイスの初期化
            initCaptureDevice();

            // コントローラの初期化
            this.analysisPreview.Out("Initialize controller device.");
            titan = new TitanWrapper.Wrapper();
            titan.Init();

            // シナリオの初期化
            initScenario();

            this.analysisPreview.Out("Initialize complete.");
        }

        /// <summary>
        /// シナリオの稼働/停止切り替えボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scenarioStartPauseButton_Click(object sender, EventArgs e)
        {
            if (!this.scenario.IsStarted)
            {
                initScenario();
                this.scenario.Start();
                this.analysisPreview.Out("Start scenario.");
            } else
            {
                this.scenario.PauseResume();
                if (this.scenario.IsPause)
                {
                    this.analysisPreview.Out("Pause scenario.");
                }
                else
                {
                    this.analysisPreview.Out("Resume scenario.");
                }

            }
        }

        /// <summary>
        /// シナリオの中止ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scenarioAbortButton_Click(object sender, EventArgs e)
        {
            this.scenario.RequestAbort();
            this.analysisPreview.Out("Abort scenario.");
        }

        /// <summary>
        /// デバイス初期化ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void initDevicesButton_Click(object sender, EventArgs e)
        {
            initAllDevices();
        }

        /// <summary>
        /// ビュースタイル変更ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void changeViewStyleButton_Click(object sender, EventArgs e)
        {
            config.SelectedViewStyle++;
            config.SelectedViewStyle %= Enum.GetNames(typeof(Config.VIEW_STYLE)).Length;
            viewStyleUpdate();
        }

        /// <summary>
        /// コントローラパッドの表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showControllerPadButton_Click(object sender, EventArgs e)
        {
            if ( null == titanPad || !titanPad.Visible)
            {
                titanPad = new eBotLib.Controller.TitanPad(this.titan);
                titanPad.Show();
            }
        }

        /// <summary>
        /// ステータス更新タイマ チック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void statusUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (this.scenario.IsStarted)
            {
                if (this.scenario.IsPause)
                {
                    this.scenarioStartPauseButton.BackgroundImage = Properties.Resources.play;
                }
                else
                {
                    this.scenarioStartPauseButton.BackgroundImage = Properties.Resources.pause;
                }
                this.deviceSelectComboBox.Enabled = false;
                this.scenarioSelectComboBox.Enabled = false;
                this.initDevicesButton.Enabled = false;
                this.scenarioAbortButton.Enabled = true;
            }
            else
            {
                this.scenarioStartPauseButton.BackgroundImage = Properties.Resources.play;
                this.deviceSelectComboBox.Enabled = true;
                this.scenarioSelectComboBox.Enabled = true;
                this.initDevicesButton.Enabled = true;
                this.scenarioAbortButton.Enabled = false;
            }

            this.showControllerPadButton.Enabled = (null == titanPad || !titanPad.Visible);
        }
    }
}
