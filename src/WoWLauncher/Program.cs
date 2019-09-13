using MiniBlinkPinvoke;
using System;
using System.Windows.Forms;

namespace WoWLauncher
{
    public static class Program
    {
        public static Main MainWindow { get; private set; }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            BlinkBrowserPInvoke.ResourceAssemblys.Add("WoWLauncher", System.Reflection.Assembly.GetExecutingAssembly());
            MainWindow = new Main();
            Application.Run(MainWindow);
        }
    }
}
