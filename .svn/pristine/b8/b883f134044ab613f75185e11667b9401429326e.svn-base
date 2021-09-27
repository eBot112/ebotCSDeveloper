using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eBotLib;
using eBotLib.Scenario;

namespace previewmode
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
                return "Transfer the preview to the analysis screen.";
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
            AnalysisPreviewControl analysisPreview = null) : base(capture, titan, analysisPreview, 1)
        {
        }

        /// <summary>
        /// テンプレートマッチング用パラメータの作成
        /// </summary>
        protected override void MakeMatchingParams()
        {
        }

        /// <summary>
        /// シナリオ更新
        /// </summary>
        /// <remarks>INTERVAL周期でスナップショットを撮像しTickが実行される。</remarks>
        /// <returns>継続フラグ</returns>
        protected override bool Tick()
        {
            return true;
        }
    }
}
