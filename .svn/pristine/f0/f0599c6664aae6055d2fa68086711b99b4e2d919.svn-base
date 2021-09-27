using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBotScenarioPlayer
{
    /// <summary>
    /// 設定クラス
    /// </summary>
    public class Config
    {
        /// <summary>ビュースタイル</summary>
        public enum VIEW_STYLE
        {
            WipePreview = 0,
            AnalysisOnly = 1,
        }

        /// <summary>シナリオ保存フォルダ</summary>
        public static readonly string SCENARIOS_DIRECTORY = "scenarios";

        /// <summary>設定ファイル名</summary>
        private static readonly string FILE_NAME = "config.xml";

        /// <summary>選択されているキャプチャデバイス</summary>
        public int SelectedCaptureDeviceIndex;
        /// <summary>選択されているシナリオ</summary>
        public string SelectedScenario;
        /// <summary>選択されているビュースタイル</summary>
        public int SelectedViewStyle;

        /// <summary>
        /// 初期化
        /// </summary>
        private void init()
        {
            SelectedCaptureDeviceIndex = 0;
            SelectedScenario = null;
            SelectedViewStyle = (int)VIEW_STYLE.WipePreview;
        }

        /// <summary>
        /// セーブ
        /// </summary>
        public void Save()
        {
            System.Xml.Serialization.XmlSerializer s = new System.Xml.Serialization.XmlSerializer(typeof(Config));
            System.IO.StreamWriter sw = new System.IO.StreamWriter(FILE_NAME, false, new System.Text.UTF8Encoding(false));
            s.Serialize(sw, this);
            sw.Close();
        }

        /// <summary>
        /// ロード
        /// </summary>
        /// <returns>読み込んだConfigクラスのインスタンス</returns>
        public static Config Load()
        {
            Config config = null;

            try
            {
                System.Xml.Serialization.XmlSerializer s = new System.Xml.Serialization.XmlSerializer(typeof(Config));
                System.IO.StreamReader sr = new System.IO.StreamReader(FILE_NAME, new System.Text.UTF8Encoding(false));
                config = (Config)s.Deserialize(sr);
                sr.Close();
            }
            catch (Exception)
            {
                config = new Config();
                config.init();
            }

            return config;
        }
    }
}
