using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBotLib.Vision
{
    /// <summary>
    /// OpenCVSharp4によるパターンマッチングクラス
    /// </summary>
    class Matching
    {
        /// <summary>初期判定閾値</summary>
        public const double DEFAULT_MATCH_THRESHOLD = 0.700;

        /// <summary>
        /// 画像認識処理
        /// </summary>
        /// <param name="searchMat">サーチ対象画像</param>
        /// <param name="templateMat">テンプレート画像</param>
        /// <param name="threshold">判定閾値</param>
        /// <param name="resultRect">認識矩形</param>
        /// <param name="resultValue">判定値</param>
        /// <returns>成否</returns>
        private static bool matching(Mat searchMat, Mat templateMat, double threshold, out Rectangle resultRect, out double resultValue)
        {
            resultRect = new Rectangle();

            using (Mat result = new Mat())
            {
                //画像認識
                Cv2.MatchTemplate(searchMat, templateMat, result, TemplateMatchModes.CCoeffNormed);

                // 類似度が最大/最小となる画素の位置を調べる
                OpenCvSharp.Point minloc, maxloc;
                double minval, maxval;
                Cv2.MinMaxLoc(result, out minval, out maxval, out minloc, out maxloc);
                resultValue = maxval;
                resultRect.X = maxloc.X;
                resultRect.Y = maxloc.Y;
                resultRect.Width = templateMat.Width;
                resultRect.Height = templateMat.Height;

                // しきい値で判断
                if (maxval >= threshold)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ImageをMatに変換して画像認識処理に渡す
        /// </summary>
        /// <param name="searchFullImage">サーチ対象画像</param>
        /// <param name="templateImage">テンプレート画像</param>
        /// <param name="threshold">判定閾値</param>
        /// <param name="resultRect">判定矩形</param>
        /// <param name="searchRect">認識矩形</param>
        /// <param name="resultValue">判定値</param>
        /// <returns>成否</returns>
        public static bool ImageMatching(Bitmap searchFullImage, Bitmap templateImage, double threshold, Rectangle searchRect, out Rectangle resultRect, out double resultValue)
        {
            bool result;

            using (Bitmap searchImage = searchFullImage.Clone(searchRect, searchFullImage.PixelFormat))
            using (Mat searchMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(searchImage))
            using (Mat templateMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(templateImage))
            {
                result = matching(searchMat, templateMat, threshold, out resultRect, out resultValue);
                resultRect = new Rectangle(resultRect.X + searchRect.X, resultRect.Y + searchRect.Y, resultRect.Width, resultRect.Height);
                return result;
            }
        }
    }
}
