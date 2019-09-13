using MiniBlinkPinvoke;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WoWLauncher
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            BlinkBrowserPInvoke.ResourceAssemblys.Add("WoWLauncher", System.Reflection.Assembly.GetExecutingAssembly());
            Application.Run(new Main());
            ////Application.Run(new TabForm());
        }
    }
}
