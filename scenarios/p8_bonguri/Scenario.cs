using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eBotLib;
using eBotLib.Scenario;

namespace p8_bonguri
{
    /// <summary>
    /// シナリオクラス
    /// </summary>
    public class Scenario: Template
    {
        enum SEQ_SCENARIO
        {
            TIME_SELECT,
            SHAKE_TREE,
            RESET,
        }

        SEQ_SCENARIO seq = SEQ_SCENARIO.RESET;

        /// <summary>
        /// ヘルプメッセージ
        /// </summary>
        public override string HelpTipMessage
        {
            get
            {
                return "ぼんぐりおよび木の実を回収するボット\n\nときわたり有効\n集中の森の木の前で実行する。";
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
            SetTemplateMatchingParam("powersw", "powersw.bmp", 0.95, 1385, 761, 120, 117);
            SetTemplateMatchingParam("asoberu", "asoberu.bmp", 0.95, 578, 382, 408, 81);
        }

        /// <summary>
        /// シナリオ更新
        /// </summary>
        /// <remarks>INTERVAL周期でスナップショットを撮像しTickが実行される。</remarks>
        /// <returns>継続フラグ</returns>
        protected override bool Tick()
        {
            switch (seq)
            {
                case SEQ_SCENARIO.TIME_SELECT:
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 100, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RIGHT, 100, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RIGHT, 100, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RIGHT, 100, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RIGHT, 100, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RIGHT, 100, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 3000);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 4000, 500);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 1000);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 200, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 200, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 200, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 200, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 200, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 200, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 200, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 2000);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 100, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 100, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 2000);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RIGHT, 100, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RIGHT, 100, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.UP, 100, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RIGHT, 100, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RIGHT, 100, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RIGHT, 100, 200);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 2000);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.HOME, 100, 2000);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.HOME, 100, 3000);

                    seq = SEQ_SCENARIO.SHAKE_TREE;
                    break;

                case SEQ_SCENARIO.SHAKE_TREE:
                    if (TemplateMatching("asoberu"))
                    {
                        break;
                    }
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 1000);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 1000);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 1000);
                    for (int i = 0; i < 20; i++)
                    {
                        this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.B, 100, 500);
                    }

                    seq = SEQ_SCENARIO.RESET;
                    break;

                case SEQ_SCENARIO.RESET:
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.HOME, 100, 3000);
                    if (TemplateMatching("powersw"))
                    {
                        // パワースイッチが見えてない、メニューでない場合は再度HOME
                        this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.HOME, 100, 3000);
                    }

                    seq = SEQ_SCENARIO.TIME_SELECT;
                    break;
            }

            return true;
        }
    }
}
