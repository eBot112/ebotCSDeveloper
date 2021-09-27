using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eBotLib;
using eBotLib.Scenario;

namespace p8_battle_tower_wheel
{
    /// <summary>
    /// シナリオクラス
    /// </summary>
    public class Scenario: Template
    {
        /// <summary>
        /// ヘルプメッセージ
        /// </summary>
        public override string HelpTipMessage
        {
            get
            {
                return "This is a battle tower scenario for the 8th gen.\n" +
                    "    a) Select the team to use in advance.\n" +
                    "    b) Select a three-member team.\n" +
                    "    c) There is only one move to make them learn.\n" +
                    "    d) Please play the scenario in front of the reception desk.";
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
            SetTemplateMatchingParam("connect", "connect.bmp", 0.95, 0, 958, 115, 122);
            SetTemplateMatchingParam("bp_team_select", "bp_team_select.bmp", 0.95, 143, 38, 229, 116);
            SetTemplateMatchingParam("tsuyosa", "tsuyosa.bmp", 0.95, 622, 94, 1264, 888);
            SetTemplateMatchingParam("status", "status.bmp", 0.95, 120, 0, 492, 120);
            SetTemplateMatchingParam("tatakaenai", "tatakaenai.bmp", 0.95, 823, 0, 315, 67);
            SetTemplateMatchingParam("tatakaeru", "tatakaeru.bmp", 0.95, 823, 0, 274, 82);
        }

        /// <summary>
        /// シナリオ更新
        /// </summary>
        /// <remarks>INTERVAL周期でスナップショットを撮像しTickが実行される。</remarks>
        /// <returns>継続フラグ</returns>
        protected override bool Tick()
        {
            if (TemplateMatching("tsuyosa"))
            {
                this.analysisPreview.Out("Pattern Found: tsuyosa");

                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.B, 100, 1500);
            }
            else if (TemplateMatching("status"))
            {
                this.analysisPreview.Out("Pattern Found: status");

                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.B, 100, 1500);
                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.B, 100, 3000);
            }
            else if (TemplateMatching("tatakaenai", 4))
            {
                this.analysisPreview.Out("Pattern Found: tatakaenai");

                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 100, 500);
                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 500);
                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 3000);
            }
            else if (TemplateMatching("tatakaeru", 4))
            {
                this.analysisPreview.Out("Pattern Found: tatakaeru");

                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 500);
                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 3000);
            }
            else if (TemplateMatching("bp_team_select", 4))
            {
                this.analysisPreview.Out("Pattern Found: bp_team_select");

                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 500);    // 選択
                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 500);    // 参加
                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 100, 500); // 2体目
                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 500);    // 選択
                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 500);    // 参加
                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 100, 500); // 3体目
                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 500);    // 選択
                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 1000);   // 参加
                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 5000);   // 決定
            }
            else
            {
                this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 1000);
            }

            return true;
        }
    }
}
