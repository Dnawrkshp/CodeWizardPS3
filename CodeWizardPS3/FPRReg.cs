using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CodeWizardPS3
{
    public partial class FPRReg : UserControl
    {
        public FPRReg()
        {
            InitializeComponent();
        }

        [Description("Name of register"), Category("Data")]
        public string RegName
        {
            get { return NameLab.Text; }
            set { NameLab.Text = value; }
        }

        public void SetReg(string value)
        {
            textBox1.Text = value;
        }

        public string GetReg()
        {
            return textBox1.Text;
        }

        public void SetForeColor(Color forecolor) {
            if (forecolor == null)
                return;
            textBox1.ForeColor = forecolor;
        }

        private void PPCReg_Load(object sender, EventArgs e)
        {

        }

    }
}