using eBotLib.Capture;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBotLib.Scenario
{
    /// <summary>
    /// OCR閾値モード
    /// </summary>
    public enum OCR_THRESHOLD_MODE
    {
        /// <summary>どれか1文字でも閾値より小さく検出された場合はNGとする。</summary>
        Minimum,
        /// <summary>信頼度の平均値が閾値をしたまった場合にNGとする。</summary>
        Average
    }

    /// <summary>
    /// OCR判定モード
    /// </summary>
    public enum OCR_JUDGE_MODE
    {
        /// <summary>指定した文字列が含まれればOKとする。</summary>
        Contain,
        /// <summary>指定した文字列と検出した文字列が一致している場合のみOKとする。</summary>
        Exact
    }

    /// <summary>
    /// シナリオ完了イベント引数
    /// </summary>
    public class ScenarioCompleteEventArgs
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ScenarioCompleteEventArgs()
        {
        }
    }

    /// <summary>
    /// テンプレートマッチング用パラメータ
    /// </summary>
    public class TemplateMatchingParam
    {
        /// <summary>テンプレート画像</summary>
        public Bitmap TemplateImage;
        /// <summary>判定閾値</summary>
        public double Threshold;
        /// <summary>判定矩形</summary>
        public Rectangle SearchRect;
    }

    /// <summary>
    /// OCR用パラメータ
    /// </summary>
    public class OCRMatchingParam
    {
        /// <summary>OCRエンジン</summary>
        public OCR.TesseractWrap ocr;

        /// <summary>対象文字列</summary>
        public string Text;
        /// <summary>判定閾値</summary>
        public float Threshold;
        /// <summary>認識矩形</summary>
        public Rectangle SearchRect;
        /// <summary>閾値モード</summary>
        public OCR_THRESHOLD_MODE ThresholdMode;
        /// <summary>判定モード</summary>
        public OCR_JUDGE_MODE JudgeMode;
        /// <summary>空文字処理</summary>
        /// <remarks>true:無視する, false:考慮する</remarks>
        public bool IsBlanksIgnore;
    }

    /// <summary>
    /// シナリオテンプレート
    /// </summary>
    abstract public class Template
    {
        /// <summary>シナリオ完了デリゲート</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        public delegate void ScenarioCompleteEventHandler<T>(T args);
        /// <summary>シナリオ完了通知イベント</summary>
        public event ScenarioCompleteEventHandler<ScenarioCompleteEventArgs> ScenarioCompleteHandler;

        /// <summary>画像転送待ちカウントダウン</summary>
        private readonly System.Threading.CountdownEvent countdownImageReady = new System.Threading.CountdownEvent(1);
        /// <summary>シナリオ稼働カウントダウン</summary>
        private readonly System.Threading.CountdownEvent countdownScenarioReady = new System.Threading.CountdownEvent(1);
        /// <summary>スナップショットのバンク数</summary>
        private const int SNAPSHOT_BANK_NUM = 10;

        /// <summary>スナップショットの保存先</summary>
        private const string SNAPSHOT_DIRECTORY = "snapshot";

        /// <summary>キャプチャ</summary>
        private Capture.DirectCapturePictureBox capture;

        /// <summary>スナップショット画像</summary>
        private Bitmap snapShotImage;
        /// <summary>更新周期[msec]</summary>
        private int interval;
        /// <summary>周期毎にスナップショットを自動取得するか</summary>
        private bool isAutomaticSnapshot;

        /// <summary>スナップショット排他制御用オブジェクト</summary>
        private object lockSnapshotObject;

        /// <summary>コントローラ</summary>
        protected TitanWrapper.Wrapper titan;
        /// <summary>プレビュー画面</summary>
        protected AnalysisPreviewControl analysisPreview;
        /// <summary>テンプレート画像の保存フォルダ</summary>
        /// <remarks>"%dll参照場所%\templates"を推奨</remarks>
        abstract protected string TemplateDirectory { get; }
        /// <summary>ツールチップに表示するメッセージ</summary>
        abstract public string HelpTipMessage { get; }
        /// <summary>テンプレートマッチング用パラメータ</summary>
        protected Dictionary<string, TemplateMatchingParam> TemplateMatchingParams { get; }

        /// <summary>OCRパラメータ</summary>
        protected Dictionary<string, OCRMatchingParam> OcrParams { get; }

        /// <summary>実行済みフラグ</summary>
        private bool isStarted = false;
        /// <summary>実行済みフラグ</summary>
        public bool IsStarted { get { return isStarted; } }
        /// <summary>稼働停止要求</summary>
        private bool requestAbort = false;
        /// <summary>中断フラグ</summary>
        public bool IsPause { get { return (0 != this.countdownScenarioReady.CurrentCount); } }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="capture">キャプチャ</param>
        /// <param name="titan">コントローラ</param>
        /// <param name="analysisPreview">プレビュー画面</param>
        /// <param name="interval">更新周期[msec]</param>
        /// <param name="isAutomaticSnapshot">周期毎にスナップショットを自動取得するか</param>
        public Template(Capture.DirectCapturePictureBox capture, TitanWrapper.Wrapper titan, AnalysisPreviewControl analysisPreview, int interval = 100, bool isAutomaticSnapshot = true)
        {
            this.capture = capture;
            this.titan = titan;
            this.analysisPreview = analysisPreview;
            this.interval = interval;
            this.isAutomaticSnapshot = isAutomaticSnapshot;

            this.lockSnapshotObject = new object();

            this.capture.SnapShotCompleteHandler += snapShotCompleteHandler;

            this.TemplateMatchingParams = new Dictionary<string, TemplateMatchingParam>();
            this.OcrParams = new Dictionary<string, OCRMatchingParam>();
            this.MakeMatchingParams();
        }

        /// <summary>
        /// シナリオの実行
        /// </summary>
        public void Start()
        {
            Task.Run(() =>
            {
                this.countdownScenarioReady.Signal();
                this.isStarted = true;

                double left;
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                while (true)
                {
                    // 稼働許可待ち
                    this.countdownScenarioReady.Wait();

                    // スナップショットの要求
                    if (this.isAutomaticSnapshot)
                    {
                        this.UpdateSnapshot();
                    }

                    // シナリオ実行
                    if (this.requestAbort || !Tick())
                    {
                        break;
                    }

                    left = (double)interval - sw.Elapsed.TotalMilliseconds;
                    // 切り捨てて1msec以上残っている場合はSleep
                    if ((int)left >= 1)
                    {
                        System.Threading.Thread.Sleep((int)left - 1);
                    }
                    // 残り時間はDispatch
                    while (0.0 < (double)interval - sw.Elapsed.TotalMilliseconds)
                    {
                        System.Threading.Thread.Sleep(0);
                    }

                    sw.Restart();
                }

                // シナリオ完了通知
                this.isStarted = false;
                this.requestAbort = false;
                this.countdownScenarioReady.Reset();
                var args = new ScenarioCompleteEventArgs();
                if (null != ScenarioCompleteHandler)
                {
                    ScenarioCompleteHandler(args);
                }
            });
        }

        /// <summary>
        /// 稼働停止要求
        /// </summary>
        public void RequestAbort()
        {
            this.requestAbort = true;
            if (0 != this.countdownScenarioReady.CurrentCount)
            {
                this.countdownScenarioReady.Signal();
            }
        }

        /// <summary>
        /// シナリオの稼働要求
        /// </summary>
        public void PauseResume()
        {
            if (0 == this.countdownScenarioReady.CurrentCount)
            {
                this.countdownScenarioReady.Reset();
            }
            else
            {
                this.countdownScenarioReady.Signal();
            }
        }

        /// <summary>
        /// マッチング用パラメータの作成
        /// </summary>
        protected abstract void MakeMatchingParams();

        /// <summary>
        /// テンプレートマッチング用パラメータの登録
        /// </summary>
        /// <param name="parameterName">パラメータの名称</param>
        /// <param name="filePath">テンプレート画像のパス</param>
        /// <param name="threshold">判定Q値の閾値</param>
        /// <param name="x">認識範囲のx座標</param>
        /// <param name="y">認識範囲のy座標</param>
        /// <param name="width">認識範囲の幅</param>
        /// <param name="height">認識範囲の高さ</param>
        protected void SetTemplateMatchingParam(string parameterName, string fileName, double threshold, int x, int y, int width, int height)
        {
            this.TemplateMatchingParams.Add(parameterName, new TemplateMatchingParam()
            {
                TemplateImage = new System.Drawing.Bitmap(this.TemplateDirectory + "\\" + fileName),
                Threshold = threshold,
                SearchRect = new System.Drawing.Rectangle(x, y, width, height)
            });
        }

        /// <summary>
        /// OCR用パラメータの登録
        /// </summary>
        /// <param name="parameterName">パラメータの名称</param>
        /// <param name="text">対象文字列</param>
        /// <param name="x">認識矩形 X座標</param>
        /// <param name="y">認識矩形 Y座標</param>
        /// <param name="width">認識矩形 幅</param>
        /// <param name="height">認識矩形 高さ</param>
        /// <param name="langPack">言語パック</param>
        /// <param name="segMode">セグメントモード</param>
        /// <param name="whiteList">ホワイトリスト</param>
        /// <param name="isBlanksIgnore">空文字処理</param>
        /// <param name="threshold">判定閾値</param>
        /// <param name="thresholdMode">閾値モード</param>
        /// <param name="judgeMode">判定モード</param>
        protected void SetOCRMatchingParam(string parameterName, string text, int x, int y, int width, int height, 
            string langPack = "eng", int segMode = 7, string whiteList = @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-&#*.,'\:;/\=!?",
            bool isBlanksIgnore = true, float threshold = 40.0f, OCR_THRESHOLD_MODE thresholdMode = OCR_THRESHOLD_MODE.Minimum,
            OCR_JUDGE_MODE judgeMode = OCR_JUDGE_MODE.Contain)
        {
            OCR.TesseractWrap ocr = new OCR.TesseractWrap();
            ocr.LanguagePackSelect = langPack;
            ocr.SegMode = (Tesseract.PageSegMode)segMode;   // 初期値は7(Single Line)
            ocr.WhiteList = whiteList;
            ocr.EngineUpdate();

            this.OcrParams.Add(parameterName, new OCRMatchingParam()
            {
                ocr = ocr,
                Text = text,
                Threshold = threshold,
                SearchRect = new Rectangle(x, y, width, height),
                ThresholdMode = thresholdMode,
                JudgeMode = judgeMode,
                IsBlanksIgnore = isBlanksIgnore
            });
        }

        /// <summary>
        /// シナリオ更新
        /// </summary>
        /// <returns>継続フラグ</returns>
        protected abstract bool Tick();

        /// <summary>
        /// テンプレートマッチングの実行
        /// </summary>
        /// <param name="matchingKey">テンプレートマッチング用パラメータのキー</param>
        /// <param name="times">認識回数(連続で認識成功した場合trueとする)</param>
        /// <param name="interval">認識間隔[msec]</param>
        /// <returns></returns>
        protected bool TemplateMatching(string matchingKey, int times = 1, int interval = 500)
        {
            bool result = true;
            double resultValue;
            Rectangle resultRect;

            for (int cnt = 0; cnt < times && result; cnt++)
            {
                if (0 != cnt)
                {
                    this.UpdateSnapshot();

                    System.Threading.Thread.Sleep(interval);
                }
                try
                {
                    TemplateMatchingParam param = this.TemplateMatchingParams[matchingKey];
                    lock (this.lockSnapshotObject)
                    {
                        result = Vision.Matching.ImageMatching(this.snapShotImage,
                            param.TemplateImage, param.Threshold, param.SearchRect, out resultRect, out resultValue);
                    }
                    this.analysisPreview.RecognitionOut(result, resultValue, param.Threshold, resultRect, param.SearchRect);
                }
                catch (KeyNotFoundException)
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// OCRによるマッチングの実行
        /// </summary>
        /// <param name="matchingKey">OCR用パラメータのキー</param>
        /// <param name="text">認識テキスト</param>
        /// <param name="recOnly">読み取りモード(マッチング無し)</param>
        /// <param name="times">認識回数(連続で認識成功した場合trueとする)</param>
        /// <param name="interval">認識間隔[msec]</param>
        /// <returns>マッチングの成否</returns>
        protected bool OCRMatching(string matchingKey, out string text, bool recOnly = false, int times = 1, int interval = 500)
        {
            bool result = true;
            float confidenceMin = 100.0f;
            float confidenceAve = 0.0f;
            int targetSymbolIdx;

            text = "";

            for (int cnt = 0; cnt < times && result; cnt++)
            {
                if (0 != cnt)
                {
                    this.UpdateSnapshot();

                    System.Threading.Thread.Sleep(interval);
                }
                try
                {
                    OCRMatchingParam param = this.OcrParams[matchingKey];
                    OCR.TesseractWrap.Result ocrResult;
                    lock (this.lockSnapshotObject)
                    {
                        ocrResult = param.ocr.Execute(this.snapShotImage, param.SearchRect);
                    }
                    this.analysisPreview.OCROut(ocrResult, param.SearchRect, param.Threshold);

                    do
                    {
                        // 空白文字の削除
                        if (param.IsBlanksIgnore)
                        {
                            text = ocrResult.Text.Replace(" ", "").Replace("　", "").Replace("\n", ""); // 半角・全角スペース、改行を削除
                        }
                        else
                        {
                            text = ocrResult.Text;
                        }

                        if (!recOnly)
                        {
                            // 文字列チェック
                            if (OCR_JUDGE_MODE.Exact == param.JudgeMode)
                            {
                                result = text.Equals(param.Text);
                                targetSymbolIdx = 0;
                            }
                            else
                            {
                                targetSymbolIdx = text.IndexOf(param.Text);
                                result = (0 <= targetSymbolIdx);
                            }
                            if (!result) break;

                            // 閾値チェック
                            for (int i = targetSymbolIdx; i < ocrResult.symbols.Length; i++)
                            {
                                if (confidenceMin > ocrResult.symbols[i].confidence) confidenceMin = ocrResult.symbols[i].confidence;
                                confidenceAve += ocrResult.symbols[i].confidence;
                            }
                            confidenceAve /= param.Text.Length;
                            if (OCR_THRESHOLD_MODE.Minimum == param.ThresholdMode)
                            {
                                result = (param.Threshold <= confidenceMin);
                            }
                            else
                            {
                                result = (param.Threshold <= confidenceAve);
                            }
                            if (!result) break;
                        }
                    } while (false);

                }
                catch (KeyNotFoundException)
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// コントローラ ボタンクリック操作
        /// </summary>
        /// <param name="key">入力対象のキー</param>
        /// <param name="pushTime">ボタン押し込み時間[msec]</param>
        /// <param name="delay">ボタン解放後ディレイ[msec]</param>
        /// <param name="isAxis">アナログ軸</param>
        /// <param name="isAxis">+方向</param>
        protected void ButtonClick(int key, int pushTime, int delay, bool isAxis = false, bool plus = true)
        {
            int down;

            if (!isAxis)
            {
                down = (int)TitanWrapper.TitanOne.KEY_STATE.DOWN;
            }
            else if (plus)
            {
                down = (int)TitanWrapper.TitanOne.KEY_STATE.AXIS_MAX;
            }
            else
            {
                down = (int)TitanWrapper.TitanOne.KEY_STATE.AXIS_MIN;
            }

            this.titan.InputButton(key, down);
            System.Threading.Thread.Sleep(pushTime);
            this.titan.InputButton(key, (int)TitanWrapper.TitanOne.KEY_STATE.UP);
            System.Threading.Thread.Sleep(delay);
        }

        /// <summary>
        /// スナップショットの取得
        /// </summary>
        private void requestCaptureSnapshot()
        {
            this.countdownImageReady.Reset();
            this.capture.Invoke(new Action(() =>
            {
                this.capture.RequestCaptureSnapshot();
            }));
        }

        /// <summary>
        /// スナップショット取得完了待ち
        /// </summary>
        private void waitCaptureSnapshot()
        {
            this.countdownImageReady.Wait();
        }

        /// <summary>
        /// スナップショットを更新する
        /// </summary>
        protected void UpdateSnapshot()
        {
            requestCaptureSnapshot();
            waitCaptureSnapshot();
        }

        /// <summary>
        /// 解析描画のクリア
        /// </summary>
        protected void ClearAnalysis()
        {
            this.analysisPreview.ClearDraw();
        }

        /// <summary>
        /// スナップショットの保存
        /// </summary>
        protected void SaveSnapShot()
        {
            if (!System.IO.Directory.Exists(SNAPSHOT_DIRECTORY))
            {
                System.IO.Directory.CreateDirectory(SNAPSHOT_DIRECTORY);
            }
            lock (this.lockSnapshotObject)
            {
                this.snapShotImage.Save(SNAPSHOT_DIRECTORY + "\\" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".bmp");
            }
        }

        /// <summary>
        /// スナップショット取得イベント
        /// </summary>
        /// <param name="args"></param>
        private void snapShotCompleteHandler(SnapShotCompleteEventArgs args)
        {
            if (this.analysisPreview.InvokeRequired)
            {
                this.analysisPreview.BeginInvoke(new Action(() => snapShotCompleteHandler(args)));
                return;
            }

            lock (this.lockSnapshotObject)
            {
                Bitmap tempImage = this.snapShotImage;
                this.snapShotImage = args.Bitmap;
                this.analysisPreview.DrawImage(new Bitmap(this.snapShotImage));
                if (null != tempImage)
                {
                    tempImage.Dispose();
                }
            }

            if (0 != this.countdownImageReady.CurrentCount)
            {
                this.countdownImageReady.Signal();
            }
        }
    }
}
