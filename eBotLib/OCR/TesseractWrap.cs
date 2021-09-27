using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBotLib.OCR
{
    /// <summary>
    /// Tesseractラッパ
    /// </summary>
    public class TesseractWrap
    {
        /// <summary>文字解析情報</summary>
        public class Symbol
        {
            /// <summary>文字</summary>
            public string symbol;
            /// <summary>検出矩形</summary>
            public System.Drawing.Rectangle rect;
            /// <summary>信頼度</summary>
            public float confidence;
        }

        /// <summary>
        /// 解析結果
        /// </summary>
        public class Result
        {
            /// <summary>解析文字列</summary>
            public string Text;
            /// <summary>文字解析情報配列</summary>
            public Symbol[] symbols;
        }

        /// <summary>設定ファイルの保存場所</summary>
        private const string CONFIG_FILE_PATH = @"ocr.xml";
        /// <summary>言語パックの保存先（初期値）</summary>
        private const string INIT_LANGUAGE_FILE_PATH = @"OCR\language\best";
        /// <summary>言語パックの種別（初期値）</summary>
        private const string INIT_LANGUAGE_SELECT = "eng";
        /// <summary>セグメントモード（初期値）</summary>
        private const Tesseract.PageSegMode INIT_TESSERACT_SEG_MODE = Tesseract.PageSegMode.Auto;
        /// <summary>ホワイトリスト（初期値）</summary>
        private const string INIT_TESSERACT_WHITE_LIST_WORDS = @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-&#*.,'\:;/\=!?";

        /// <summary>言語パックの保存先</summary>
        public string LanguageFilePath = INIT_LANGUAGE_FILE_PATH;
        /// <summary>言語パックの種別</summary>
        public string LanguagePackSelect = INIT_LANGUAGE_SELECT;
        /// <summary>セグメントモード</summary>
        public Tesseract.PageSegMode SegMode = INIT_TESSERACT_SEG_MODE;
        /// <summary>ホワイトリスト</summary>
        public string WhiteList = INIT_TESSERACT_WHITE_LIST_WORDS;

        /// <summary>OCRエンジン</summary>
        private Tesseract.TesseractEngine engine = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TesseractWrap()
        {
        }

        /// <summary>
        /// エンジン更新
        /// </summary>
        public void EngineUpdate()
        {
            this.engine = new Tesseract.TesseractEngine(this.LanguageFilePath, this.LanguagePackSelect);
            this.engine.SetVariable("tessedit_char_whitelist", WhiteList);
        }

        /// <summary>
        /// OCR実行
        /// </summary>
        /// <param name="src">解析対象のビットマップファイル</param>
        /// <param name="targetRect">解析対象矩形</param>
        /// <returns>解析結果</returns>
        public Result Execute(Bitmap src, System.Drawing.Rectangle targetRect)
        {
            Tesseract.Page page = null;
            Tesseract.Rect rect;
            Tesseract.Rect bounds;
            Result result = null;
            Tesseract.ResultIterator iterator;
            List<Symbol> symbolList;

            if (null == this.engine) return null;

            rect = new Tesseract.Rect(targetRect.X, targetRect.Y, targetRect.Width, targetRect.Height);

            Tesseract.Pix img = Tesseract.PixConverter.ToPix(src);
            page = this.engine.Process(img, rect, this.SegMode);

            result = new Result();
            symbolList = new List<Symbol>();

            result.Text = page.GetText();
            iterator = page.GetIterator();
            do
            {
                Symbol symbol = new Symbol();

                symbol.symbol = iterator.GetText(Tesseract.PageIteratorLevel.Symbol);
                if (!iterator.TryGetBoundingBox(Tesseract.PageIteratorLevel.Symbol, out bounds)) continue;
                symbol.rect = new Rectangle(bounds.X1, bounds.Y1, bounds.Width, bounds.Height);
                symbol.confidence = iterator.GetConfidence(Tesseract.PageIteratorLevel.Symbol);

                symbolList.Add(symbol);
            } while (iterator.Next(Tesseract.PageIteratorLevel.Symbol));
            result.symbols = symbolList.ToArray();
            page.Dispose();

            return result;
        }
    }
}
