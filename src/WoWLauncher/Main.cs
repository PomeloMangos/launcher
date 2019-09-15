using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using System.IO;
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
            si.UseShellExecute = true;
            si.FileName = ConfigManager.Config.register;
            Process.Start(si);
        }

        [JSFunction]
        public void Launch(string name)
        {
            var realm = ConfigManager.Config.realm_list.Single(x => x.name == name);
            var content = $"SET realmlist \"{realm.address}\"";

            // Remove read-only flag
            if (File.GetAttributes(Path.Combine(ConfigManager.Config.game_path, "realmlist.wtf")).ToString().IndexOf("ReadOnly") != -1)
            {
                File.SetAttributes(Path.Combine(ConfigManager.Config.game_path, "realmlist.wtf"), FileAttributes.Normal);
            }

            File.WriteAllText(Path.Combine(ConfigManager.Config.game_path, "realmlist.wtf"), content);
            File.WriteAllText(Path.Combine(ConfigManager.Config.game_path, "data/zhtw/realmlist.WTF"), content);
            File.WriteAllText(Path.Combine(ConfigManager.Config.game_path, "data/zhcn/realmlist.WTF"), content);

            blink.InvokeJSW($"window.app.running=true;");
            BackgroundWorker bw_wow = new BackgroundWorker();
            bw_wow.DoWork += Bw_wow_DoWork;
            bw_wow.RunWorkerCompleted += Bw_wow_RunWorkerCompleted;
            bw_wow.RunWorkerAsync();
        }

        [JSFunction]
        public void Install(string path, bool addons)
        {
            ConfigManager.Config.game_path = path;
            ConfigManager.Config.install_addons = addons;
            ConfigManager.SaveConfig();
            UpdateClient();
        }

        [JSFunction]
        public void BrowseDirectory()
        {
            var dialog = new FolderBrowserDialog();
            dialog.Description = "请选择目录";
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    blink.InvokeJSW($"window.app.game_path='{dialog.SelectedPath.Replace("\\", "\\\\")}';");
                    FindWoWExe(dialog.SelectedPath);
                }
            }
        }

        [JSFunction]
        public void FindWoWExe(string path)
        {
            if (File.Exists(Path.Combine(path, "wow.exe")))
            {
                blink.InvokeJSW($"window.app.game_exists=true;");
            }
            else
            {
                blink.InvokeJSW($"window.app.game_exists=false;");
            }
        }

        public void UpdateClient()
        {
            blink.InvokeJSW($"window.app.view='update';");

            BackgroundWorker bw_update = new BackgroundWorker();
            bw_update.DoWork += Bw_update_DoWork;
            bw_update.RunWorkerCompleted += Bw_update_RunWorkerCompleted;
            bw_update.RunWorkerAsync();
        }

        private void Bw_wow_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            blink.InvokeJSW($"window.app.running=false;");
        }

        private void Bw_wow_DoWork(object sender, DoWorkEventArgs e)
        {
            var si = new ProcessStartInfo();
            si.FileName = "wow.exe";
            si.WorkingDirectory = ConfigManager.Config.game_path;
            var p = Process.Start(si);
            p.WaitForExit();
            p.Dispose();
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

            if (File.Exists(Path.Combine(ConfigManager.Config.game_path, "wow.exe")))
            {
                UpdateClient();
            }
            else
            {
                if (string.IsNullOrEmpty(ConfigManager.Config.game_path))
                {
                    blink.InvokeJSW($"window.app.game_path='{Path.Combine(Environment.CurrentDirectory, "game").Replace("\\", "\\\\")}';");
                }
                else
                {
                    blink.InvokeJSW($"window.app.game_path='{ConfigManager.Config.game_path.Replace("\\", "\\\\")}';");
                }
                blink.InvokeJSW($"window.app.view='install';");
                blink.InvokeJSW($"window.app.addons_option={(ConfigManager.Config.install_addons ? "true" : "false")};");
            }
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
