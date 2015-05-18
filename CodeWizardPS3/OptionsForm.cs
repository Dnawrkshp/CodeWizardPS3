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
    public partial class OptionsForm : Form
    {
        public static event EventHandler UpdateTheme;

        public OptionsForm()
        {
            InitializeComponent();
        }

        private void OkayButt_Click(object sender, EventArgs e)
        {
            string saveStr = Main.outputType.ToString() + "\n";
            saveStr += Main.bitOrder.ToString() + "\n";
            saveStr += MinMemRange.Text + "\n";
            saveStr += MaxMemRange.Text + "\n";
            saveStr += stackPointer.Text + "\n";
            //saveStr += themeDark.Checked ? "0\n" : "1\n";
            FileIO.SaveFile(Main.settFile, saveStr);

            Main.minMemRange = Convert.ToUInt32(MinMemRange.Text, 16);
            Main.maxMemRange = Convert.ToUInt32(MaxMemRange.Text, 16);
            Main.startStackPtr = Convert.ToUInt32(stackPointer.Text, 16);
            /*
            bool themeL = themeDark.Checked ? false : true;;
            if (Main.themeLight != themeL)
            {
                Main.themeLight = themeL;
                UpdateTheme(this, new EventArgs());
            }
            */

            this.Dispose();
            this.Close();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Main.outputType = 0;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Main.outputType = 1;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            Main.outputType = 2;
        }

        private void CancButt_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }

        private void OptionsForm_Load(object sender, EventArgs e)
        {
            //Load output type
            switch (Main.outputType)
            {
                case 0: //NetCheat PS3
                    rad1NC.Checked = true;
                    rad1HSA.Checked = false;
                    rad1BA.Checked = false;
                    break;
                case 1: //Hex String Array
                    rad1NC.Checked = false;
                    rad1HSA.Checked = true;
                    rad1BA.Checked = false;
                    break;
                case 2: //Byte Array
                    rad1NC.Checked = false;
                    rad1HSA.Checked = false;
                    rad1BA.Checked = true;
                    break;
            }

            //Register Bit Order
            switch (Main.bitOrder)
            {
                case 0: //Least To Greatest
                    rboLTG.Checked = true;
                    rboGTL.Checked = false;
                    break;
                case 1: //Greatest To Least
                    rboLTG.Checked = false;
                    rboGTL.Checked = true;
                    break;
            }

            //Theme
            switch (Main.themeLight)
            {
                case false:
                    themeDark.Checked = true;
                    break;
                case true:
                    themeLight.Checked = true;
                    break;
            }

            MinMemRange.Text = Main.minMemRange.ToString("X8");
            MaxMemRange.Text = Main.maxMemRange.ToString("X8");
            stackPointer.Text = Main.startStackPtr.ToString("X8");
        }

        private void rboLTG_CheckedChanged(object sender, EventArgs e)
        {
            Main.bitOrder = 0;
        }

        private void rboGTL_CheckedChanged(object sender, EventArgs e)
        {
            Main.bitOrder = 1;
        }
    }
}
