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
    public partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }

        private void FontLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.urbanfonts.com/fonts/MonaKo.htm");
        }

        private void SourceLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Dnawrkshp");
        }
    }
}
