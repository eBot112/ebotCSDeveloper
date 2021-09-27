﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace eBotLib
{
    public partial class AnalysisPreviewControl : UserControl
    {
        /// <summary>
        /// 認識結果
        /// </summary>
        private class RecognitionResult
        {
            /// <summary>認識結果</summary>
            public bool result;
            /// <summary>判定値</summary>
            public double score;
            /// <summary>判定閾値</summary>
            public double threshold;
            /// <summary>判定矩形</summary>
            public Rectangle resultRect;
            /// <summary>認識矩形</summary>
            public Rectangle searchRect;
        }

        /// <summary>
        /// OCR結果
        /// </summary>
        private class OCRResult
        {
            /// <summary>認識結果</summary>
            public OCR.TesseractWrap.Result result;
            /// <summary>認識矩形</summary>
            public Rectangle searchRect;
            /// <summary>認識閾値</summary>
            public float threshold;
        }

        /// <summary>更新周期の初期値</summary>
        private const int DEFAULT_REFRESH_RATE = 16;

        /// <summary>ログの上限</summary>
        private const int LOG_MAX = 10;
        /// <summary>認識ログの上限</summary>
        private const int REC_RESULT_MAX = 10;
        /// <summary>OCRログの上限</summary>
        private const int OCR_RESULT_MAX = 10;
        /// <summary>ログ表示の開始位置</summary>
        private static readonly PointF LOG_START_POINT = new PointF(4.0f, 4.0f);
        /// <summary>ログ表示の行間</summary>
        private static readonly float LOG_LINE_INTERVAL = 14.0f;
        /// <summary>ログフォント</summary>
        private static readonly Font logFont = new Font("SimSun", 9.75f);
        /// <summary>ログ文字ブラシ</summary>
        private static readonly Brush logBrush = Brushes.Lime;
        /// <summary>認識矩形用ペン(Success)</summary>
        private static readonly Pen successRecResultPen = new Pen(Brushes.Lime)
        {
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dot
        };
        /// <summary>認識矩形用ペン(Faile)</summary>
        private static readonly Pen faileRecResultPen = new Pen(Brushes.Fuchsia)
        {
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dot
        };
        /// <summary>認識矩形用ペン(SearchArea)</summary>
        private static readonly Pen searchRecResultPen = new Pen(Brushes.Cyan)
        {
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dot
        };
        /// <summary>OCR矩形用ペン(SearchArea)</summary>
        private static readonly Pen searchOCRResultPen = new Pen(Brushes.DarkOrange)
        {
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dot
        };
        /// <summary>OCR認識結果用ペン</summary>
        private static readonly Pen ocrFindPen = new Pen(Brushes.DarkOrange);
        /// <summary>選択矩形用ペン</summary>
        private static readonly Pen selectRectPen = new Pen(Brushes.Blue)
        {
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dot
        };
        /// <summary>認識ログフォント</summary>
        private static readonly Font recResultFont = new Font("SimSun", 9.75f);
        /// <summary>OCRログフォント</summary>
        private static readonly Font ocrResultFont = new Font("SimSun", 9.75f);

        /// <summary>トレースログ</summary>
        private List<string> logs;
        /// <summary>認識ログ</summary>
        private List<RecognitionResult> recResults;
        /// <summary>認識ログ</summary>
        private List<OCRResult> ocrResults;
        /// <summary>選択矩形</summary>
        private Rectangle selectRect = new Rectangle(0, 0, 0, 0);

        /// <summary>描画の排他制御オブジェクト</summary>
        private object lockDrawObject;
        /// <summary>スナップショット画像</summary>
        private Image snapshotImage;

        /// <summary>プレビュー更新タイマー</summary>
        private System.Windows.Forms.Timer refreshTimer;
        
        /// <summary>リフレッシュの予約</summary>
        private bool reserveRefresh;
        /// <summary>リフレッシュレート[msec]</summary>
        private double refreshRate;
        /// <summary>リフレッシュタイミング制御用ストップウォッチ</summary>
        System.Diagnostics.Stopwatch refreshStopWatch;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="refreshRate">リフレッシュレート[msec]</param>
        public AnalysisPreviewControl(double refreshRate = DEFAULT_REFRESH_RATE)
        {
            InitializeComponent();
            init(refreshRate);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AnalysisPreviewControl()
        {
            InitializeComponent();
            init();
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="refreshRate">リフレッシュレート[msec]</param>
        private void init(double refreshRate = DEFAULT_REFRESH_RATE)
        {
            logs = new List<string>();
            recResults = new List<RecognitionResult>();
            ocrResults = new List<OCRResult>();
            lockDrawObject = new object();

            this.refreshRate = refreshRate;
            this.reserveRefresh = false;

            this.DoubleBuffered = true;

            this.refreshStopWatch = System.Diagnostics.Stopwatch.StartNew();

            this.refreshTimer = new Timer();
            this.refreshTimer.Interval = 1;
            this.refreshTimer.Tick += new EventHandler(refreshTimer_Tick);
            this.refreshTimer.Start();
        }

        /// <summary>
        /// トレース情報の出力
        /// </summary>
        /// <param name="log">トレースログ</param>
        public void Out(string log)
        {
            lock (lockDrawObject)
            {
                logs.Add(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + ": " + log);
                while (logs.Count > LOG_MAX)
                {
                    logs.RemoveAt(0);
                }
            }
            reserveRefresh = true;
        }


        /// <summary>
        /// 認識結果の出力
        /// </summary>
        /// <param name="result">認識結果</param>
        /// <param name="score">判定値</param>
        /// <param name="threshold">判定閾値</param>
        /// <param name="resultRect">判定矩形</param>
        /// <param name="searchRect">認識矩形</param>
        public void RecognitionOut(bool result, double score, double threshold, Rectangle resultRect, Rectangle searchRect)
        {
            lock (lockDrawObject)
            {
                RecognitionResult rec = new RecognitionResult()
                {
                    result = result,
                    score = score,
                    threshold = threshold,
                    resultRect = resultRect,
                    searchRect = searchRect
                };
                recResults.Add(rec);
                while (recResults.Count > REC_RESULT_MAX)
                {
                    recResults.RemoveAt(0);
                }
            }
            reserveRefresh = true;
        }


        /// <summary>
        /// OCR結果の出力
        /// </summary>
        /// <param name="result">認識結果</param>
        /// <param name="searchRect">認識矩形</param>
        /// <param name="threshold">認識閾値</param>
        public void OCROut(OCR.TesseractWrap.Result result, Rectangle searchRect, float threshold)
        {
            lock (lockDrawObject)
            {
                OCRResult ocr = new OCRResult()
                {
                    result = result,
                    searchRect = searchRect,
                    threshold = threshold
                };
                ocrResults.Add(ocr);
                while (ocrResults.Count > REC_RESULT_MAX)
                {
                    ocrResults.RemoveAt(0);
                }
            }
            reserveRefresh = true;
        }

        /// <summary>
        /// 選択矩形の出力
        /// </summary>
        /// <param name="rect">選択矩形</param>
        public void RectOut(Rectangle rect)
        {
            lock (lockDrawObject)
            {
                selectRect = rect;
            }
            reserveRefresh = true;
        }

        /// <summary>
        /// snapshot画像の保存
        /// </summary>
        /// <param name="path">画像の保存先</param>
        public void ImageSave(string path)
        {
            if (null == this.snapshotImage) return;

            lock (lockDrawObject)
            {
                ((Bitmap)this.snapshotImage).Save(path);
            }
        }

        /// <summary>
        /// snapshot画像の保存
        /// </summary>
        /// <param name="path">画像の保存先</param>
        /// <param name="srcRect">保存矩形</param>
        public void ImageSave(string path, Rectangle srcRect)
        {
            if (null == this.snapshotImage) return;
            if (0 >= srcRect.Width || 0 >= srcRect.Height) return;

            lock (lockDrawObject)
            {
                Bitmap bmp = new Bitmap(srcRect.Width, srcRect.Height, this.snapshotImage.PixelFormat);
                Graphics g = Graphics.FromImage(bmp);
                Rectangle dstRect = new Rectangle(0, 0, srcRect.Width, srcRect.Height);
                g.DrawImage(this.snapshotImage, dstRect, srcRect, GraphicsUnit.Pixel);
                g.Dispose();

                bmp.Save(path);
            }
        }


        /// <summary>
        /// ペイントイベントで描画
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            lock (lockDrawObject)
            {
                try
                {
                    this.SuspendLayout();
                    // スナップショット画像の描画
                    if (null != this.snapshotImage)
                    {
                        PointF lt = GetPictureLocation(new PointF(0f, 0f));
                        PointF rb = GetPictureLocation(new PointF(this.snapshotImage.Width, this.snapshotImage.Height));
                        e.Graphics.DrawImage(this.snapshotImage, lt.X, lt.Y, rb.X - lt.X, rb.Y - lt.Y);
                    }
                    // ログの描画
                    foreach (var log in logs.Select((v, i) => new { val = v, idx = i }))
                    {
                        e.Graphics.DrawString(log.val, logFont, logBrush, new PointF(LOG_START_POINT.X, LOG_START_POINT.Y + LOG_LINE_INTERVAL * log.idx)); ;
                    }
                    // 認識結果の描画
                    foreach (RecognitionResult recRet in recResults)
                    {
                        Pen findPen;
                        Rectangle findRect = GetPictureRectangle(recRet.resultRect);
                        Rectangle searchRect = GetPictureRectangle(recRet.searchRect);

                        if (recRet.result)
                        {
                            findPen = successRecResultPen;
                        }
                        else
                        {
                            findPen = faileRecResultPen;
                        }
                        e.Graphics.DrawRectangle(searchRecResultPen, searchRect);
                        e.Graphics.DrawRectangle(findPen, findRect);
                        e.Graphics.DrawString("[" + recRet.score.ToString("0.000") + "/" + recRet.threshold.ToString("0.000") + "]",
                            recResultFont, findPen.Brush, new Point(findRect.X, findRect.Y + findRect.Height + 2));
                    }
                    // OCR結果の描画
                    foreach (OCRResult ocrRet in ocrResults)
                    {
                        Rectangle searchRect = GetPictureRectangle(ocrRet.searchRect);
                        e.Graphics.DrawRectangle(searchOCRResultPen, searchRect);
                        e.Graphics.DrawString(ocrRet.result.Text, ocrResultFont, ocrFindPen.Brush, new Point(searchRect.X, searchRect.Y + searchRect.Height + 2));
                        foreach (OCR.TesseractWrap.Symbol symbol in ocrRet.result.symbols)
                        {
                            double confidence;
                            int score;

                            confidence = symbol.confidence - ocrRet.threshold;
                            if (0.0 > confidence) confidence = 0.0;
                            score = (int)Math.Round(confidence / (100.0f - ocrRet.threshold) * 255.0);

                            Pen p = new Pen(Color.FromArgb(255 - score, score, 0))
                            {
                                DashStyle = System.Drawing.Drawing2D.DashStyle.Dot
                            };
                            e.Graphics.DrawRectangle(p, GetPictureRectangle(symbol.rect));
                        }
                    }
                    // 選択矩形の描画
                    if (0 != selectRect.Width && 0 != selectRect.Height)
                    {
                        e.Graphics.DrawRectangle(selectRectPen, GetPictureRectangle(selectRect));
                    }
                }
                finally
                {
                    this.ResumeLayout();
                }
            }
        }

        /// <summary>
        /// リサイズ時はリフレッシュ要求
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            reserveRefresh = true;
            base.OnResize(e);
        }

        /// <summary>
        /// 画像描画
        /// </summary>
        /// <param name="bitmap"></param>
        public void DrawImage(Bitmap bitmap)
        {
            lock (lockDrawObject)
            {
                Image tmp = this.snapshotImage;
                this.snapshotImage = bitmap;
                if (null != tmp) tmp.Dispose();
            }
            reserveRefresh = true;
        }

        /// <summary>
        /// 解析描画のクリア
        /// </summary>
        public void ClearDraw()
        {
            lock (lockDrawObject)
            {
                this.logs.Clear();
                this.recResults.Clear();
                this.ocrResults.Clear();
            }
        }

        /// <summary>
        /// 画像の表示倍率を算出
        /// </summary>
        /// <returns>表示倍率</returns>
        public double GetImageRatio()
        {
            return Math.Min((double)this.ClientSize.Width / (double)this.snapshotImage.Width, (double)this.ClientSize.Height / (double)this.snapshotImage.Height);
        }

        /// <summary>
        /// ピクチャボックス座標系でのターゲット画像位置を算出
        /// </summary>
        /// <returns>表示倍率</returns>
        public PointF GetImageOffset()
        {
            PointF imageOffset = new PointF();
            double imageRatio = GetImageRatio();

            imageOffset.X = ((float)this.ClientSize.Width - (float)Math.Round((double)this.snapshotImage.Width * imageRatio)) / 2.0f;
            imageOffset.Y = ((float)this.ClientSize.Height - (float)Math.Round((double)this.snapshotImage.Height * imageRatio)) / 2.0f;

            return imageOffset;
        }

        /// <summary>
        /// 画像矩形からピクチャボックス矩形を取得
        /// </summary>
        /// <param name="location">画像矩形</param>
        /// <returns>ピピクチャボックス矩形</returns>
        public Rectangle GetPictureRectangle(Rectangle rect)
        {
            PointF ltf = GetPictureLocation(new PointF(rect.Left, rect.Top));
            PointF rbf = GetPictureLocation(new PointF(rect.Right, rect.Bottom));
            int left = (int)Math.Round(ltf.X);
            int right = (int)Math.Round(rbf.X);
            int top = (int)Math.Round(ltf.Y);
            int bottom = (int)Math.Round(rbf.Y);

            return new Rectangle(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// 画像座標からピクチャボックス座標を取得
        /// </summary>
        /// <param name="location">画像座標</param>
        /// <returns>ピクチャボックス座標</returns>
        public PointF GetPictureLocation(PointF location)
        {
            if (null == this.snapshotImage) return location;

            PointF pictureLocation = new PointF();
            PointF imageOffset = GetImageOffset();
            double imageRatio = GetImageRatio();

            // ターゲット画像座標系からピクチャボックス座標系に射影
            pictureLocation.X = (float)(location.X * imageRatio) + imageOffset.X;
            pictureLocation.Y = (float)(location.Y * imageRatio) + imageOffset.Y;
            // ピクチャボックス外部は境界値
            if (pictureLocation.X > this.ClientSize.Width) pictureLocation.X = this.ClientSize.Width;
            if (pictureLocation.X < 0.0f) pictureLocation.X = 0.0f;
            if (pictureLocation.Y > this.ClientSize.Height) pictureLocation.Y = this.ClientSize.Height;
            if (pictureLocation.Y < 0.0f) pictureLocation.Y = 0.0f;

            return pictureLocation;
        }

        /// <summary>
        /// ピクチャボックス座標から画像座標を取得
        /// </summary>
        /// <param name="location">ピクチャボックス座標</param>
        /// <returns>画像座標</returns>
        public PointF GetImageLocation(PointF location)
        {
            if (null == this.snapshotImage) return location;

            PointF imageLocation = new PointF();
            PointF imageOffset = GetImageOffset();
            double imageRatio = GetImageRatio();

            // ピクチャボックス画像座標系からターゲット画像座標系に射影
            imageLocation.X = (location.X - imageOffset.X) / (float)imageRatio;
            imageLocation.Y = (location.Y - imageOffset.Y) / (float)imageRatio;

            // ターゲット画像外部は境界値
            if (imageLocation.X > this.snapshotImage.Width) imageLocation.X = this.snapshotImage.Width;
            if (imageLocation.X < 0.0f) imageLocation.X = 0.0f;
            if (imageLocation.Y > this.snapshotImage.Height) imageLocation.Y = this.snapshotImage.Height;
            if (imageLocation.Y < 0.0f) imageLocation.Y = 0.0f;

            return imageLocation;
        }

        /// <summary>
        /// リフレッシュタイマ チック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            if (reserveRefresh && this.refreshRate < refreshStopWatch.Elapsed.TotalMilliseconds)
            {
                this.refreshStopWatch.Restart();
                this.Refresh();
                this.reserveRefresh = false;
            }
        }
    }
}
