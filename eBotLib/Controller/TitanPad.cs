using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace eBotLib.Controller
{
    /// <summary>
    /// Titan用パッド
    /// </summary>
    public partial class TitanPad : Form
    {
        /// <summary>
        /// キー種別
        /// </summary>
        private enum KEY_TYPE
        {
            button,
            axisNegative,
            axisPassive
        }

        /// <summary>Titanラッパ</summary>
        private TitanWrapper.Wrapper titan;
        /// <summary>状態更新タイマ</summary>
        private Timer updateTimer;
        /// <summary>入力中かどうか</summary>
        private bool isKeyDown;

        /// <summary>
        /// コンストラクタ(デザイナ用)
        /// </summary>
        public TitanPad()
        {
            InitializeComponent();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="titan">Titanラッパ</param>
        public TitanPad(TitanWrapper.Wrapper titan)
        {
            InitializeComponent();

            this.titan = titan;

            updateTimer = new Timer();
            updateTimer.Interval = 30;
            updateTimer.Tick += new EventHandler(updateTimer_Tick);
            updateTimer.Start();
        }

        /// <summary>
        /// コントローラ初期化ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void initControllerButton_Click(object sender, EventArgs e)
        {
            titan = new TitanWrapper.Wrapper();
            titan.Init();
        }

        /// <summary>
        /// コントローラ操作ボタン　マウスダウン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void controllerButton_MouseDown(object sender, MouseEventArgs e)
        {
            int map = -1, type = -1;

            if (getButtonMapType(sender, ref map, ref type))
            {
                this.titan.InputButton(map, 
                    (type == (int)KEY_TYPE.button || type == (int)KEY_TYPE.axisPassive) ? 
                    (int)TitanWrapper.TitanOne.KEY_STATE.DOWN : (int)TitanWrapper.TitanOne.KEY_STATE.AXIS_MIN);
                this.isKeyDown = true;
            }
        }

        /// <summary>
        /// コントローラ操作ボタン　マウスダウン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void controllerButton_MouseUp(object sender, MouseEventArgs e)
        {
            int map = -1, type = -1;

            if (getButtonMapType(sender, ref map, ref type))
            {
                this.titan.InputButton(map, (int)TitanWrapper.TitanOne.KEY_STATE.UP);
                this.isKeyDown = false;
            }
        }

        /// <summary>
        /// 状態更新タイマ チック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateTimer_Tick(object sender, EventArgs e)
        {
            if ((Control.MouseButtons & MouseButtons.Left) != MouseButtons.Left &&
                (Control.MouseButtons & MouseButtons.Right) != MouseButtons.Right)
            {
                // ボタン範囲外でマウスアップした場合の保護
                if (this.isKeyDown)
                {
                    this.titan.ClearButton((int)TitanWrapper.TitanOne.KEY_STATE.UP);
                    this.isKeyDown = false;
                }
            }
        }

    /// <summary>
    /// ボタン情報の取得
    /// </summary>
    /// <param name="button"></param>
    /// <param name="map"></param>
    /// <param name="type"></param>
    /// <returns>情報有無</returns>
    private bool getButtonMapType(object button, ref int map, ref int type)
        {
            bool ret = false;

            if (button == this.homeButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.HOME; type = (int)KEY_TYPE.button; ret = true; }
            if (button == this.captureButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.CAPTURE; type = (int)KEY_TYPE.button; ret = true; }
            if (button == this.plusButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.PLUS; type = (int)KEY_TYPE.button; ret = true; }
            if (button == this.minusButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.MINUS; type = (int)KEY_TYPE.button; ret = true; }

            if (button == this.l1Button) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.L; type = (int)KEY_TYPE.button; ret = true; }
            if (button == this.l2Button) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.ZL; type = (int)KEY_TYPE.button; ret = true; }
            if (button == this.alcButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.SL; type = (int)KEY_TYPE.button; ret = true; }
            if (button == this.r1Button) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.R; type = (int)KEY_TYPE.button; ret = true; }
            if (button == this.r2Button) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.ZR; type = (int)KEY_TYPE.button; ret = true; }
            if (button == this.arcButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.SR; type = (int)KEY_TYPE.button; ret = true; }

            if (button == this.cllButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.LEFT; type = (int)KEY_TYPE.button; ret = true; }
            if (button == this.cluButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.UP; type = (int)KEY_TYPE.button; ret = true; }
            if (button == this.clrButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RIGHT; type = (int)KEY_TYPE.button; ret = true; }
            if (button == this.cldButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.DOWN; type = (int)KEY_TYPE.button; ret = true; }

            if (button == this.crlButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.Y; type = (int)KEY_TYPE.button; ret = true; }
            if (button == this.cruButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.X; type = (int)KEY_TYPE.button; ret = true; }
            if (button == this.crrButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.A; type = (int)KEY_TYPE.button; ret = true; }
            if (button == this.crdButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.B; type = (int)KEY_TYPE.button; ret = true; }

            if (button == this.alxnButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.LX; type = (int)KEY_TYPE.axisNegative; ret = true; }
            if (button == this.alypButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.LY; type = (int)KEY_TYPE.axisPassive; ret = true; }
            if (button == this.alxpButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.LX; type = (int)KEY_TYPE.axisPassive; ret = true; }
            if (button == this.alynButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.LY; type = (int)KEY_TYPE.axisNegative; ret = true; }

            if (button == this.arxnButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RX; type = (int)KEY_TYPE.axisNegative; ret = true; }
            if (button == this.arypButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RY; type = (int)KEY_TYPE.axisPassive; ret = true; }
            if (button == this.arxpButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RX; type = (int)KEY_TYPE.axisPassive; ret = true; }
            if (button == this.arynButton) { map = (int)TitanWrapper.TitanOne.KEY_MAP_SWITCH.RY; type = (int)KEY_TYPE.axisNegative; ret = true; }

            return ret;
        }
    }
}
