using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eBotLib;
using eBotLib.Scenario;

namespace p8_eternatus_selection
{
    /// <summary>
    /// シナリオクラス
    /// </summary>
    public class Scenario: Template
    {
        /// <summary>
        /// ステータスチェック
        /// </summary>
        private enum STATUS_CHECK
        {
            NO_LABEL,
            MISS,
            MATCH
        }

        /// <summary>
        /// シナリオシーケンス
        /// </summary>
        private enum SCENARIO_SEQUENCE
        {
            START,
            BATTLE_WAIT,
            SELECT_MENU_POKE,
            MOVE_STATUS,
            STATUS_CHECK,
            MOVE_ITEM,
            SELECT_MENU_BAG,
            SELECT_CANDY,
            RESET,
        }

        /// <summary></summary>
        const int RESET_SAME_SEQUENCE_COUNT = 400;
        /// <summary>同じシナリオを繰り返した回数</summary>
        int sameSequenceCount = 0;
        /// <summary>シナリオ現在位置</summary>
        SCENARIO_SEQUENCE sequence = SCENARIO_SEQUENCE.START;

        /// <summary>
        /// ヘルプメッセージ
        /// </summary>
        public override string HelpTipMessage
        {
            get
            {
                return "ムゲンダイナ厳選シナリオ\n" +
                    "1戦目スキップ、2戦目階段前からスタート\n" +
                    "手持ちは1体とし、ひとつめの技で倒せること\n" +
                    "メニュー画面左上が\"ポケモン\"であること";
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
            SetTemplateMatchingParam("stslbl_a_normal", "stslbl_a_normal.bmp", 0.95, 600, 255, 148, 60);
            SetTemplateMatchingParam("stslbl_a_down", "stslbl_a_down.bmp", 0.95, 600, 255, 148, 60);
            SetTemplateMatchingParam("stslbl_a_up", "stslbl_a_up.bmp", 0.95, 600, 255, 148, 60);
            SetTemplateMatchingParam("sta_location", "sta_location.bmp", 0.93, 89, 286, 823, 184);
            SetTemplateMatchingParam("connect", "connect.bmp", 0.95, 0, 975, 91, 105);
            SetTemplateMatchingParam("asoberu", "asoberu.bmp", 0.95, 578, 382, 408, 81);
            SetTemplateMatchingParam("menu_poke", "menu_poke.bmp", 0.95, 139, 120, 1649, 605);
            SetTemplateMatchingParam("menu_bag", "menu_bag.bmp", 0.95, 139, 120, 1649, 605);
            SetTemplateMatchingParam("candy", "candy.bmp", 0.95, 959, 212, 347, 599);
            SetTemplateMatchingParam("goods", "goods.bmp", 0.95, 1332, 50, 117, 87);

            SetOCRMatchingParam("stsval_a", "", 622, 310, 103, 45, "eng", 7, "0123456789");
            SetOCRMatchingParam("stsup_a", "+1", 1609, 315, 90, 68, "eng", 8, "+123456789");
        }

        /// <summary>
        /// ステータスチェック
        /// </summary>
        /// <param name="template"></param>
        /// <param name="param"></param>
        /// <returns>結果</returns>
        private STATUS_CHECK statusCheck(string template, string param)
        {
            string stsValA;
            STATUS_CHECK ret;

            if (TemplateMatching(template))
            {
                OCRMatching("stsval_a", out stsValA, true);
                if (!stsValA.Equals(param))
                {
                    analysisPreview.Out(template + ": NG(" + stsValA + ")");
                    ret = STATUS_CHECK.MISS;
                }
                else
                {
                    analysisPreview.Out(template + ": OK(" + stsValA + ")");
                    ret = STATUS_CHECK.MATCH;
                }
            } else
            {
                ret = STATUS_CHECK.NO_LABEL;
            }

            return ret;
        }  

        /// <summary>
        /// シナリオ更新
        /// </summary>
        /// <remarks>INTERVAL周期でスナップショットを撮像しTickが実行される。</remarks>
        /// <returns>継続フラグ</returns>
        protected override bool Tick()
        {
            bool menuFind;
            bool ret = true;
            SCENARIO_SEQUENCE cacheSequence = sequence;

            switch (sequence)
            {
                case SCENARIO_SEQUENCE.START:
                    if (TemplateMatching("sta_location", 4, 200))
                    {
                        this.analysisPreview.Out("Pattern Found: sta_location");
                        this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.LY, 5000, 500, true, false);
                        this.analysisPreview.Out("goto: BATTLE_WAIT");
                        sequence = SCENARIO_SEQUENCE.BATTLE_WAIT;
                    }
                    else
                    {
                        if (TemplateMatching("asoberu", 4, 200))
                        {
                            this.analysisPreview.Out("Pattern Found: asoberu");
                            System.Threading.Thread.Sleep(500);
                            break;
                        }
                        this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 0);
                    }
                    break;
                case SCENARIO_SEQUENCE.BATTLE_WAIT:
                    if (TemplateMatching("connect", 4))
                    {
                        this.analysisPreview.Out("goto: SELECT_MENU_POKE");
                        sequence = SCENARIO_SEQUENCE.SELECT_MENU_POKE;
                    }
                    else
                    {
                        this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 500);
                    }
                    break;
                case SCENARIO_SEQUENCE.SELECT_MENU_POKE:
                    menuFind = false;

                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.X, 100, 5000);
                    for (int i = 0; i < 2 && !menuFind; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            UpdateSnapshot();
                            if (TemplateMatching("menu_poke"))
                            {
                                menuFind = true;
                                break;
                            }
                            this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RIGHT, 100, 1000);
                        }
                        if (menuFind) break;
                        this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 100, 1000);
                    }
                    if (menuFind)
                    {
                        this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 3000);
                        this.analysisPreview.Out("goto: MOVE_STATUS");
                        sequence = SCENARIO_SEQUENCE.MOVE_STATUS;
                    }
                    else
                    {
                        this.analysisPreview.Out("goto: ERROR -> RESET");
                        sequence = SCENARIO_SEQUENCE.RESET;
                    }
                    break;
                case SCENARIO_SEQUENCE.MOVE_STATUS:
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 100, 500); // 手持ち2体目
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 1000);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 3000); // つよさをみる
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RIGHT, 100, 2000); // ステータス

                    this.analysisPreview.Out("goto: STATUS_CHECK");
                    sequence = SCENARIO_SEQUENCE.STATUS_CHECK;
                    break;
                case SCENARIO_SEQUENCE.STATUS_CHECK:
                    STATUS_CHECK status;

                    if (STATUS_CHECK.NO_LABEL != (status = statusCheck("stslbl_a_normal", "107")) ||
                        STATUS_CHECK.NO_LABEL != (status = statusCheck("stslbl_a_up", "117")) ||
                        STATUS_CHECK.NO_LABEL != (status = statusCheck("stslbl_a_down", "96")))
                    {
                        if (STATUS_CHECK.MATCH == status)
                        {
                            this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.B, 100, 5000); // 手持ちへ
                            this.analysisPreview.Out("goto: MOVE_ITEM");
                            sequence = SCENARIO_SEQUENCE.MOVE_ITEM;
                        }
                        else
                        {
                            this.analysisPreview.Out("goto: RESET");
                            sequence = SCENARIO_SEQUENCE.RESET;
                        }
                        SaveSnapShot();
                    }
                    break;
                case SCENARIO_SEQUENCE.MOVE_ITEM:
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 1000);
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 100, 500); // 入れ替える
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 100, 500); // かいふくする
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 100, 500); // もちもの
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 500); // バッグへ
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 3000); // バッグを開く

                    this.analysisPreview.Out("goto: SELECT_MENU_BAG");
                    sequence = SCENARIO_SEQUENCE.SELECT_MENU_BAG;
                    break;
                case SCENARIO_SEQUENCE.SELECT_MENU_BAG:
                    menuFind = false;

                    for (int i = 0; i < 12; i++)
                    {
                        UpdateSnapshot();
                        if (!TemplateMatching("goods", 1, 0))
                        {
                            this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RIGHT, 100, 500);
                        }
                        else
                        {
                            menuFind = true;
                            this.analysisPreview.Out("goto: SELECT_CANDY");
                            sequence = SCENARIO_SEQUENCE.SELECT_CANDY;
                            break;
                        }
                    }
                    if (!menuFind)
                    {
                        this.analysisPreview.Out("goto: ERROR -> RESET");
                        sequence = SCENARIO_SEQUENCE.RESET;
                    }
                    break;
                case SCENARIO_SEQUENCE.SELECT_CANDY:
                    menuFind = false;

                    for (int i = 0; i < 100; i++)
                    {
                        UpdateSnapshot();
                        if (!TemplateMatching("candy", 1, 0))
                        {
                            this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN, 100, 500);
                        }
                        else
                        {
                            menuFind = true;

                            this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 2000); // つかう
                            this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 2000);
                            this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 5000); // 経験値をもらった
                            UpdateSnapshot();
                            if (OCRMatching("stsup_a", out _, false))
                            {
                                this.analysisPreview.Out("Selection Complete. C0");
                                ret = false;
                            } else
                            {
                                this.analysisPreview.Out("goto: RESET");
                                sequence = SCENARIO_SEQUENCE.RESET;
                            }
                            break;
                        }
                    }
                    if (!menuFind)
                    {
                        this.analysisPreview.Out("goto: ERROR -> RESET");
                        sequence = SCENARIO_SEQUENCE.RESET;
                    }
                    break;
                case SCENARIO_SEQUENCE.RESET:
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.HOME, 100, 3000); // ホームへ
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.X, 100, 2000); // ホームへ
                    this.ButtonClick((int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A, 100, 5000);
                    ClearAnalysis();
                    this.analysisPreview.Out("goto: START");
                    sequence = SCENARIO_SEQUENCE.START;
                    break;
            }

            // 同一シーケンスを一定回数以上繰り返した場合はスタックと判断
            // リセットする。
            if (cacheSequence == sequence)
            {
                sameSequenceCount++;
                if (sameSequenceCount >= RESET_SAME_SEQUENCE_COUNT)
                {
                    this.analysisPreview.Out("goto: STACK DETECT -> RESET");
                    sequence = SCENARIO_SEQUENCE.RESET;
                }
            }
            else
            {
                sameSequenceCount = 0;
            }

            return ret;
        }
    }
}
