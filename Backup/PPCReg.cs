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
    public partial class PPCReg : UserControl
    {
        TextBox[] regs = new TextBox[4];
        int bitOrder = 0;
        int bits = 0;

        public PPCReg()
        {
            InitializeComponent();
        }

        [Description("Name of register"), Category("Data")]
        public string RegName
        {
            get { return NameLab.Text; }
            set { NameLab.Text = value; }
        }

        [Description("Order of bits"), Category("Data")]
        public int BitOrder
        {
            get { return bitOrder; }
            set
            {
                if (value >= 0 && value <= 1) {
                    SetBitOrder(bitOrder, value);
                    bitOrder = value;
                }
            }
        }

        [Description("Number of bits"), Category("Data")]
        public int Bits
        {
            get { return bits; }
            set
            {
                if (value >= 1 && value <= 127)
                    bits = value;

                SetTBoxes();
            }
        }

        public void SetTBoxes()
        {
            int numOfBoxes = bits / 32;

            switch (numOfBoxes)
            {
                case 0: //1
                    regs[0].Visible = true;
                    regs[1].Visible = false;
                    regs[2].Visible = false;
                    regs[3].Visible = false;

                    if (bitOrder == 0)
                    {
                        RangeLab2.Left = regs[1].Left;

                        RangeLab1.Left = 65;
                    }
                    else
                    {
                        //RangeLab1.Left = regs[1].Left + regs[1].Width - RangeLab1.Width;
                        RangeLab1.Left = regs[0].Left - RangeLab1.Width;

                        RangeLab2.Left = 337;
                    }
                    break;
                case 1: //2
                    regs[0].Visible = true;
                    regs[1].Visible = true;
                    regs[2].Visible = false;
                    regs[3].Visible = false;

                    if (bitOrder == 0)
                    {
                        RangeLab2.Left = regs[2].Left;

                        RangeLab1.Left = 65;
                    }
                    else
                    {
                        RangeLab1.Left = regs[1].Left - 28;

                        RangeLab2.Left = 337;
                    }
                    break;
                case 2: //3
                    regs[0].Visible = true;
                    regs[1].Visible = true;
                    regs[2].Visible = true;
                    regs[3].Visible = false;

                    if (bitOrder == 0)
                    {
                        RangeLab2.Left = regs[3].Left;

                        RangeLab1.Left = 65;
                    }
                    else
                    {
                        RangeLab1.Left = regs[2].Left - 28;

                        RangeLab2.Left = 337;
                    }
                    break;
                case 3: //4
                    regs[0].Visible = true;
                    regs[1].Visible = true;
                    regs[2].Visible = true;
                    regs[3].Visible = true;

                    if (bitOrder == 0)
                    {
                        RangeLab2.Left = 337;

                        RangeLab1.Left = 65;
                    }
                    else
                    {
                        RangeLab1.Left = regs[3].Left - 21 - 14;

                        RangeLab2.Left = 337;
                    }
                    break ;
            }

            if (bitOrder == 0)
            {
                RangeLab1.Text = "0";
                RangeLab2.Text = (numOfBoxes + 1).ToString(); //(((numOfBoxes + 1) * 32) - 1).ToString();
            }
            else
            {
                RangeLab2.Text = "0";
                RangeLab1.Text = (numOfBoxes + 1).ToString(); //(((numOfBoxes + 1) * 32) - 1).ToString();
            }

        }

        public void SetBitOrder(int oldOrder, int newOrder)
        {
            if (oldOrder == newOrder)
                return;

            if (oldOrder != -1)
            {
                string rang1 = RangeLab1.Text;
                string rang2 = RangeLab2.Text;
                RangeLab1.Text = rang2;
                RangeLab2.Text = rang1;
            }

            if (newOrder == 0)
            {
                regs[0] = textBox1;
                regs[1] = textBox2;
                regs[2] = textBox3;
                regs[3] = textBox4;

                /*
                //In the case that you can swap before this is initialized
                regs[0].Text = textBox4.Text;
                regs[1].Text = textBox3.Text;
                regs[2].Text = textBox2.Text;
                regs[3].Text = textBox1.Text;
                */
            }
            else
            {
                regs[0] = textBox4;
                regs[1] = textBox3;
                regs[2] = textBox2;
                regs[3] = textBox1;

                /*
                //In the case that you can swap before this is initialized
                regs[0].Text = textBox1.Text;
                regs[1].Text = textBox2.Text;
                regs[2].Text = textBox3.Text;
                regs[3].Text = textBox4.Text;
                */
            }
        }

        public void SetReg(int box, string value)
        {
            if (value.Length != 8)
                value.PadLeft(8, '0');
            regs[box].Text = value;
        }

        public string GetReg(int box)
        {
            if (box < 0 || box > 3)
                return null;
            return regs[box].Text;
        }

        public void SetForeColor(int box, int size, Color forecolor) {
            if (forecolor == null)
                return;
            if ((box + size) >= regs.Length)
                size = regs.Length - box - 1;
            for (int x = box; x < (size + box); x++)
                regs[x].ForeColor = forecolor;
        }

        private void PPCReg_Load(object sender, EventArgs e)
        {
            SetBitOrder(-1, bitOrder);
        }

    }
}