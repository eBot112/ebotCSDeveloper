using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBotLib
{
    /// <summary>ライブラリ管理クラス</summary>
    public static class Manager
    {
        /// <summary>
        /// ライブラリのファイルバージョンを取得
        /// </summary>
        /// <returns>ファイルバージョン</returns>
        public static string GetLibFileVersion()
        {
            System.Diagnostics.FileVersionInfo ver =
                System.Diagnostics.FileVersionInfo.GetVersionInfo(
                    System.Reflection.Assembly.GetExecutingAssembly().Location);

            return ver.FileVersion;
        }
    }
}
