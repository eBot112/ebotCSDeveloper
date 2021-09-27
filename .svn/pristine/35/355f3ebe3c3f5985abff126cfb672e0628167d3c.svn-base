using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eBotLib;
using eBotLib.Scenario;

namespace p8_gantetsu_cram
{
    /// <summary>
    /// シナリオクラス
    /// </summary>
    public class Scenario: Template
    {
        int goodsCount = 0;

        /// <summary>
        /// ヘルプメッセージ
        /// </summary>
        public override string HelpTipMessage
        {
            get
            {
                return "手持ちのぼんぐりをすべてウッウロボに投入する。\nウッウロボの手前で開始する。";
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
            SetTemplateMatchingParam("item_bag", "item_bag.bmp", 0.95, 14, 7, 147, 149);
            SetTemplateMatchingParam("goods", "goods.bmp", 0.95, 982, 41, 288, 134);
            SetTemplateMatchingParam("bonguri", "bonguri.bmp", 0.95, 938, 209, 430, 602);
            
        }

        /// <summary>
        /// シナリオ更新
        /// </summary>
        /// <remarks>INTERVAL周期でスナップショットを撮像しTickが実行される。</remarks>
        /// <returns>継続フラグ</returns>
        protected override bool Tick()
        {
            bool isContinue = true;
            bool ret = true;

            if (TemplateMatching("item_bag"))
            {
                if (!TemplateMatching("goods"))
                {
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RIGHT, 100, 500);
                    isContinue = false;
                }
                else
                {
                    if (!TemplateMatching("bonguri"))
                    {
                        this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 100, 500);
                        goodsCount++;
                        if (goodsCount > 100)
                        {
                            ret = false;
                        }
                        isContinue = false;
                    }
                    else
                    {
                        goodsCount = 0;
                    }
                }
            }
            if(isContinue)
            {
                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 1000);
            }

            return ret;
        }
    }
}
