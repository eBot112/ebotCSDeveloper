using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eBotLib;
using eBotLib.Scenario;

namespace p8_fixsymbol_shiny
{
    /// <summary>
    /// シナリオクラス
    /// </summary>
    public class Scenario: Template
    {
        /// <summary>
        /// シナリオシーケンス
        /// </summary>
        private enum SCENARIO_SEQUENCE
        {
            RESET,
            WAIT_START,
            WATCH
        }

        const long SHINY_TATAKAU_THRESHOLD = 16000; // msec
        const long SHINY_TATAKAU_TIMEOUT = 20000;

        /// <summary>シナリオ現在位置</summary>
        SCENARIO_SEQUENCE sequence = SCENARIO_SEQUENCE.RESET;
        System.Diagnostics.Stopwatch shinyDetectStopwatch = new System.Diagnostics.Stopwatch();

        /// <summary>
        /// ヘルプメッセージ
        /// </summary>
        public override string HelpTipMessage
        {
            get
            {
                return "固定シンボル色厳選\nシンボルの手前で開始。\n現在レジ専用";
            }
        }
        
        /// <summary>
        /// テンプレート画像の保存フォルダ
        /// </summary>
        protected override string TemplateDirectory
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\templates";
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="capture">ビデオキャプチャ</param>
        /// <param name="titan">コントローラ</param>
        /// <param name="analysisPreview">プレビュー</param>
        public Scenario(eBotLib.Capture.DirectCapturePictureBox capture, TitanWrapper.Wrapper titan = null,
            AnalysisPreviewControl analysisPreview = null) : base(capture, titan, analysisPreview, 100)
        {
        }

        /// <summary>
        /// テンプレートマッチング用パラメータの作成
        /// </summary>
        protected override void MakeMatchingParams()
        {
            SetTemplateMatchingParam("connect", "connect.bmp", 0.95, 0, 975, 91, 105);
            SetTemplateMatchingParam("asoberu", "asoberu.bmp", 0.95, 578, 382, 408, 81);
            SetTemplateMatchingParam("tatakau", "tatakau.bmp", 0.95, 1569, 620, 195, 71);
        }

        /// <summary>
        /// シナリオ更新
        /// </summary>
        /// <remarks>INTERVAL周期でスナップショットを撮像しTickが実行される。</remarks>
        /// <returns>継続フラグ</returns>
        protected override bool Tick()
        {
            bool ret = true;

            switch (sequence)
            {
                case SCENARIO_SEQUENCE.RESET:
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.HOME, 100, 2000);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.X, 100, 1000);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 5000);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 1000);

                    sequence = SCENARIO_SEQUENCE.WAIT_START;
                    break;
                case SCENARIO_SEQUENCE.WAIT_START:
                    if (TemplateMatching("asoberu", 4, 200))
                    {
                        System.Threading.Thread.Sleep(500);
                        break;
                    }
                    if (TemplateMatching("connect", 6, 200))
                    {
                        sequence = SCENARIO_SEQUENCE.WATCH;

                        shinyDetectStopwatch.Start();
                        break;
                    }
                    if (TemplateMatching("tatakau"))
                    {
                        sequence = SCENARIO_SEQUENCE.RESET;
                        this.analysisPreview.Out("goto: ERROR sequence abnormal -> RESET");
                        break;
                    }
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 500);

                    break;
                case SCENARIO_SEQUENCE.WATCH:
                    UpdateSnapshot();
                    if (TemplateMatching("tatakau"))
                    {
                        shinyDetectStopwatch.Stop();

                        long watch = shinyDetectStopwatch.ElapsedMilliseconds;
                        this.analysisPreview.Out("meas: " + watch + "[msec]");
                        SaveSnapShot();

                        if (SHINY_TATAKAU_THRESHOLD <= watch)
                        {
                            this.analysisPreview.Out("Complete");
                            ret = false;
                            break;
                        }

                        shinyDetectStopwatch.Reset();
                        sequence = SCENARIO_SEQUENCE.RESET;
                        break;
                    }
                    else
                    {
                        long watch = shinyDetectStopwatch.ElapsedMilliseconds;
                        if (SHINY_TATAKAU_TIMEOUT <= watch)
                        {
                            shinyDetectStopwatch.Stop();
                            shinyDetectStopwatch.Reset();

                            sequence = SCENARIO_SEQUENCE.RESET;
                            this.analysisPreview.Out("goto: ERROR watch timeout -> RESET");
                            break;
                        }
                        this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 100);
                    }
                    break;
            }
            return ret;
        }
    }
}
