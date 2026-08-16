using System;
using System.Drawing;
using System.Windows.Forms;

namespace War3Trainer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // 1. 开启高 DPI 自适应，避免高分屏显示模糊
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 2. 启动主窗体
            Application.Run(new MainForm());
        }
    }
}