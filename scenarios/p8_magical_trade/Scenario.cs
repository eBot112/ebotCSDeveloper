using eBotLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p8_magical_trade
{
    public class Scenario : eBotLib.Scenario.Template
    {
        private const int TRADE_COUNT_MAX = 300;
        private const int BOX_LINE_MAX = 5;
        private const int BOX_CLMN_MAX = 6;

        private int tradeCount = 0;
        private bool isSearchNow = false;

        /// <summary>
        /// ヘルプメッセージ
        /// </summary>
        public override string HelpTipMessage
        {
            get
            {
                return "This is a scenario in which magical exchange is executed a specified number of times.(8th gen)\n" +
                    "    a) Connect to the internet in advance.\n" +
                    "    b) Trades in order from the upper left of the currently selected box.";
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
            SetTemplateMatchingParam("connect", "connect.bmp", 0.95, 5, 976, 87, 102);
            SetTemplateMatchingParam("searchnow", "searchnow.bmp", 0.95, 141, 986, 221, 87);
            SetTemplateMatchingParam("traded", "traded.bmp", 0.95, 149, 994, 206, 67);
        }


        /// <summary>
        /// シナリオ更新
        /// </summary>
        /// <remarks>INTERVAL周期でスナップショットを撮像しTickが実行される。</remarks>
        /// <returns>継続フラグ</returns>
        protected override bool Tick()
        {
            bool ret = true;

            do
            {
                if (TemplateMatching("traded"))
                {
                    isSearchNow = false;

                    this.analysisPreview.Out("traded");

                    System.Threading.Thread.Sleep(2000);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.Y, 100, 5000);
                }
                else if (TemplateMatching("searchnow"))
                {
                    if (!isSearchNow)
                    {
                        isSearchNow = true;
                        this.analysisPreview.Out("search now...");
                    }
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 1000);
                }
                else if (TemplateMatching("connect", 4))
                {
                    isSearchNow = false;

                    this.analysisPreview.Out("connect");

                    if (tradeCount >= TRADE_COUNT_MAX)
                    {
                        this.analysisPreview.Out("trade completed: " + tradeCount);
                        ret = false;
                        break;
                    }

                    int line = tradeCount % BOX_CLMN_MAX;
                    int clmn = tradeCount % (BOX_LINE_MAX * BOX_CLMN_MAX) / BOX_CLMN_MAX;

                    this.analysisPreview.Out("trade start: " + (tradeCount + 1) + "[" + line + "," + clmn + "]");

                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.Y, 100, 3000);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 100, 500);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 5000);

                    if (0 != tradeCount && 0 == tradeCount % (BOX_LINE_MAX * BOX_CLMN_MAX))
                    {
                        this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.R, 100, 500);
                    }
                    for (int c = 0; c < clmn; c++)
                    {
                        this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 100, 500);
                    }
                    for (int l = 0; l < line; l++)
                    {
                        this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RIGHT, 100, 500);
                    }
                    for (int i = 0; i < 10; i++)
                    {
                        this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 500);
                    }
                    for (int i = 0; i < 10; i++)
                    {
                        this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.B, 100, 500);
                    }

                    tradeCount++;
                }
                else
                {
                    isSearchNow = false;

                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 1000);
                }
            } while (false);

            return ret;
        }
    }
}
