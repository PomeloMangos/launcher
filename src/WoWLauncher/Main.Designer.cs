﻿namespace WoWLauncher
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.blink = new MiniBlinkPinvoke.BlinkBrowser();
            this.SuspendLayout();
            // 
            // blink
            // 
            this.blink._ZoomFactor = 0F;
            this.blink.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.blink.HTML = "";
            this.blink.Location = new System.Drawing.Point(0, 0);
            this.blink.Margin = new System.Windows.Forms.Padding(6);
            this.blink.Name = "blink";
            this.blink.Size = new System.Drawing.Size(580, 760);
            this.blink.TabIndex = 5;
            this.blink.Text = " ";
            this.blink.Url = "";
            this.blink.ZoomFactor = 0F;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(580, 760);
            this.ControlBox = false;
            this.Controls.Add(this.blink);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Main";
            this.Text = "魔兽世界：燃烧的远征";
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private MiniBlinkPinvoke.BlinkBrowser blink;
    }
}