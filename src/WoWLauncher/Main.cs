using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using MiniBlinkPinvoke;

namespace WoWLauncher
{
    public partial class Main : Form
    {
        delegate void InvokeJsDelegate(string js);
        private void InvokeJs(string js)
        {
            blink.InvokeJSW(js);
        }

        public Main()
        {
            InitializeComponent();
            blink.GlobalObjectJs = this;
            blink.Url = "mb://resource/index.html";
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            blink.ZoomFactor = CurrentAutoScaleDimensions.Width / 144F;
        }

        [JSFunction]
        public void MainMinimize()
        {
            this.WindowState = FormWindowState.Minimized;
        }

        [JSFunction]
        public void MainClose()
        {
            this.Close();
        }

        [JSFunction]
        public void RegisterAccount()
        {
            var si = new ProcessStartInfo();
            si.FileName = ConfigManager.Config.register;
            Process.Start(si);
        }

        private void Main_Move(object sender, EventArgs e)
        {
            blink.ZoomFactor = CurrentAutoScaleDimensions.Width / 144F;
        }

        private void Blink_DocumentReadyCallback()
        {
            BackgroundWorker bw_announce = new BackgroundWorker();
            bw_announce.DoWork += Bw_announce_DoWork;
            bw_announce.RunWorkerCompleted += Bw_announce_RunWorkerCompleted;
            bw_announce.RunWorkerAsync();

            BackgroundWorker bw_realm = new BackgroundWorker();
            bw_realm.DoWork += Bw_realm_DoWork;
            bw_realm.RunWorkerCompleted += Bw_realm_RunWorkerCompleted;
            bw_realm.RunWorkerAsync();

            BackgroundWorker bw_register = new BackgroundWorker();
            bw_register.DoWork += Bw_register_DoWork;
            bw_register.RunWorkerCompleted += Bw_register_RunWorkerCompleted;
            bw_register.RunWorkerAsync();

            blink.InvokeJSW($"window.app.view='update';");

            BackgroundWorker bw_update = new BackgroundWorker();
            bw_update.DoWork += Bw_update_DoWork;
            bw_update.RunWorkerCompleted += Bw_update_RunWorkerCompleted;
            bw_update.RunWorkerAsync();
        }

        private void Bw_update_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            blink.InvokeJSW($"window.app.view='announce';");
        }

        private void Bw_announce_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                ServerManager.GetAnnounce();
            }
            catch { }
        }

        private void Bw_announce_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            blink.InvokeJSW($"app.announce = '{ConfigManager.Config.announce}';");
        }

        private void Bw_realm_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                ServerManager.GetRealmList();
            }
            catch { }
        }

        private void Bw_realm_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            foreach (var x in ConfigManager.Config.realm_list)
            {
                blink.InvokeJSW($"window.app.realm.push({{name: '{x.name}', addr: '{x.address}'}});");
            }
            blink.InvokeJSW("window.app.autoSelectRealm();");
        }

        private void Bw_register_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                ServerManager.GetRegisterAddress();
            }
            catch { }
        }

        private void Bw_register_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            blink.InvokeJSW($"window.app.register_address = '{ConfigManager.Config.register}';");
        }


        private void Bw_update_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                InvokeJsDelegate d = new InvokeJsDelegate(InvokeJs);
                ServerManager.HandleUpdate((cur, total) =>
                {
                    blink.Invoke(d, $"window.app.setTotalProgress({cur}, {total});");
                    Application.DoEvents();
                },
                (fname, cur, total) =>
                {
                    blink.Invoke(d, $"window.app.setCurrentProgress('{fname}', {cur}, {total});");
                    Application.DoEvents();
                });
            }
            catch { }
        }
    }
}
