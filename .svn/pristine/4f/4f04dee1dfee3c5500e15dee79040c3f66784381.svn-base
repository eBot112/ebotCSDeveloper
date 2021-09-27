using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace eBotDevelopmentConsole
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            System.Diagnostics.FileVersionInfo ver =
                System.Diagnostics.FileVersionInfo.GetVersionInfo(
                    System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.Text = "eBot Development Console - build:" + ver.FileVersion + " / lib:" + eBotLib.Manager.GetLibFileVersion();
        }
    }
}
