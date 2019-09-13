using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using MiniBlinkPinvoke;

namespace WoWLauncher
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            blink.GlobalObjectJs = this;
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
            blink.Url = "mb://resource/index.html";
            blink.ZoomFactor = CurrentAutoScaleDimensions.Width / 144F;
        }

        [JSFunctin]
        public void MainMinimize()
        {
            this.WindowState = FormWindowState.Minimized;
        }

        [JSFunctin]
        public void MainClose()
        {
            this.Close();
        }

        private void Main_Move(object sender, EventArgs e)
        {
            blink.ZoomFactor = CurrentAutoScaleDimensions.Width / 144F;
        }
    }
}
