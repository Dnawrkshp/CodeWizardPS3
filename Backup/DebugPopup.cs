using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CodeWizardPS3
{
    public partial class DebugPopup : Form
    {
        public DebugPopup()
        {
            InitializeComponent();
        }

        public string debugText;
        public int MainWidth;
        public int MainLeft;
        public int MainHeight;
        public int MainTop;

        private void DebugBox_TextChanged(object sender, EventArgs e)
        {
            if (DebugBox.Lines.Length > (DebugBox.Height / DebugBox.Font.GetHeight()))
                DebugBox.ScrollBars = ScrollBars.Vertical;
            else
                DebugBox.ScrollBars = ScrollBars.None;
        }

        private void DebugPopup_Shown(object sender, EventArgs e)
        {
            if (debugText == "")
            {
                this.Dispose();
                this.Close();
            }

            this.Visible = false;

            int width = 0;
            //Calculate the longest string and then set the width of the form to it
            Graphics g = this.CreateGraphics();

            string[] strArr = debugText.Split('\r');
            int x = 0;
            for (x = 0; x < strArr.Length; x++)
            {
                int a = (int)g.MeasureString(strArr[x], this.Font).Width;
                if (a > width)
                    width = a;
            }
            int height = 0;
            x = 0;
            do
            {
                height = (int)g.MeasureString(strArr[x], this.Font).Height;
                x++;
            } while (x < strArr.Length && height <= 0);
            height *= strArr.Length/2;
            if (height > 166)
                height = 166;
            
            this.Height = height + OkayButt.Height + label1.Height + 40;

            g.Dispose();

            this.Width = width + 48;

            DebugBox.Width = this.Width - 24;
            DebugBox.Left = 12;
            DebugBox.Height = this.Height - OkayButt.Height - label1.Height - 24;
            DebugBox.Top = 18;
            OkayButt.Width = this.Width - 24;
            OkayButt.Left = 12;
            OkayButt.Top = DebugBox.Top + 10 + DebugBox.Height;
            label1.Width = this.Width - 24;
            label1.Left = 12;

            //Remove first newline
            if (debugText.StartsWith(Environment.NewLine))
                debugText = debugText.Substring(1, debugText.Length - 1);

            DebugBox.Text = debugText;

            //Put it in the center of the Main form
            this.Left = (MainWidth / 2) - (this.Width / 2) + MainLeft;
            this.Top = (MainHeight / 2) - (this.Height / 2) + MainTop;

            this.Visible = true;
        }

        private void DebugPopup_Load(object sender, EventArgs e)
        {

        }

        private void OkayButt_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }
    }
}
