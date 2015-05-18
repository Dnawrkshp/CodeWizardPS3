using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

/*
 * Icon of wizards hat: http://www.clipartpal.com/clipart_pd/holiday/halloween/witcheshat_10027.html
 */

namespace CodeWizardPS3
{
    public partial class InsBox : Form
    {

        public int[] ItemData = new int[0];

        public InsBox()
        {
            InitializeComponent();
        }

        
        private void InsBox_Load(object sender, EventArgs e)
        {
            int x = 0;
            
            //Instructions
            while (ASMDeclarations.ASMDef[x].name != null && x < ASMDeclarations.ASMDef.Length)
            {
                if (x != 0 && ASMDeclarations.ASMDef[x].name != ASMDeclarations.ASMDef[x - 1].name)
                {
                    Array.Resize(ref ItemData, ItemData.Length + 1);
                    ItemData[ItemData.Length - 1] = x;
                    ListIns.Items.Add(ASMDeclarations.ASMDef[x].name);
                }
                x++;
            }
            //TextIns.Text = ASMDeclarations.ASMDef[0].help;
            //ListIns.SelectedIndex = 0;

            //Commands
            ListCom.Items.Add("address");
            ListCom.Items.Add("hook");
            ListCom.Items.Add("hookl");
            ListCom.Items.Add("setreg");
            ListCom.Items.Add("hexcode");
            ListCom.Items.Add("import");
            ListCom.Items.Add("float");
            ListCom.Items.Add("string");

            //Registers
            ListReg.Items.Add("r0");
            ListReg.Items.Add("r1");
            ListReg.Items.Add("r2");
            ListReg.Items.Add("r3");
            ListReg.Items.Add("r4 - r10");
            ListReg.Items.Add("r11 - r12");
            ListReg.Items.Add("r13 - r31");
            ListReg.Items.Add("f0 - f13");
            ListReg.Items.Add("f14 - f31");
            ListReg.Items.Add("cr0 - cr7");
            ListReg.Items.Add("XER");
            ListReg.Items.Add("LR");
            ListReg.Items.Add("CTR");

            //Terms
            ListTerm.Items.Add("rA");
            ListTerm.Items.Add("rB");
            ListTerm.Items.Add("rD");
            ListTerm.Items.Add("BF");
            ListTerm.Items.Add("IMM");
            ListTerm.Items.Add("Signed");
            ListTerm.Items.Add("Unsigned");
        }

        private void ListIns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ListIns.SelectedIndex < 0)
                return;

            //Deselect item in other list boxes
            ListCom.SelectedIndex = -1;
            ListReg.SelectedIndex = -1;
            ListTerm.SelectedIndex = -1;

            //Set text and color it
            TextIns.Text = ASMDeclarations.ASMDef[ItemData[ListIns.SelectedIndex]].help;
            ColorTextIns();
        }

        private void ListCom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ListCom.SelectedIndex < 0)
                return;

            //Deselect item in other list boxes
            ListIns.SelectedIndex = -1;
            ListReg.SelectedIndex = -1;
            ListTerm.SelectedIndex = -1;

            TextIns.Text = ASMDeclarations.helpCom[ListCom.SelectedIndex];
            ColorTextIns();
        }

        private void ListReg_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ListReg.SelectedIndex < 0)
                return;

            //Deselect item in other list boxes
            ListIns.SelectedIndex = -1;
            ListCom.SelectedIndex = -1;
            ListTerm.SelectedIndex = -1;

            TextIns.Text = ASMDeclarations.helpReg[ListReg.SelectedIndex];
            ColorTextIns();
        }

        private void ListTerm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ListTerm.SelectedIndex < 0)
                return;

            //Deselect item in other list boxes
            ListIns.SelectedIndex = -1;
            ListReg.SelectedIndex = -1;
            ListCom.SelectedIndex = -1;

            TextIns.Text = ASMDeclarations.helpTerm[ListTerm.SelectedIndex];
            ColorTextIns();
        }

        private void ColorTextIns()
        {
            string[] cat = { "Description:", "Operation:", "Syntax:", "Example:", "NOTE:", "Output:", "Purpose:", "Usage:" };
            Color[] catCol = { Color.Red, Color.Yellow, Color.Turquoise, Color.Orange, Color.LightSteelBlue, Color.Thistle, Color.Red, Color.LightSteelBlue, };

            //Make everything GreenYellow;
            TextIns.SelectionStart = 0;
            TextIns.SelectionLength = TextIns.Text.Length;
            TextIns.SelectionColor = Color.GreenYellow;

            //Color categories
            int x = 0, y = 0;
            for (x = 0; x < cat.Length; x++) {
                y = TextIns.Find(cat[x]);
                if (y >= 0 && y < TextIns.Text.Length)
                {
                    TextIns.SelectionStart = y;
                    TextIns.SelectionLength = cat[x].Length;
                    TextIns.SelectionColor = catCol[x];
                    TextIns.SelectionLength = 0;
                }
            }

            //Color first line
            y = TextIns.Find("\r");
            if (y >= 0)
            {
                TextIns.SelectionStart = 0;
                TextIns.SelectionLength = y;
                TextIns.SelectionColor = Color.HotPink;
                TextIns.SelectionLength = 0;
            }

            TextIns.SelectionStart = 0;
            TextIns.SelectionLength = 0;
            TextIns.SelectionColor = Color.GreenYellow;
        }

    }
}
