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
    public partial class ConversionForm : Form
    {
        public ConversionForm()
        {
            InitializeComponent();
        }

        bool isClicked = false;

        private string ConvertFormat(int input, int output, string inputStr, string extra)
        {
            if (input == output)
                return inputStr;

            string ret = "";

            if (inputStr == null || inputStr == "")
                return "";

            switch (input)
            {
                case 0: //NetCheat PS3
                    if (output == 1)
                    {
                        string[] inputArr = inputStr.Split('\r');
                        for (int x = 0; x < inputArr.Length; x++)
                        {
                            if (inputArr[x] != "")
                            {
                                string outStr = Main.sRight(inputArr[x], 8);
                                ret += InsertSeq(outStr, 2, 3, "", " ", " ");
                            }
                        }
                        return ret.Remove(ret.Length - 1);
                    }
                    else
                    {
                        string[] inputArr = inputStr.Split('\r');
                        ret = "byte[] " + extra + " = { ";
                        for (int x = 0; x < inputArr.Length; x++)
                        {
                            string outStr = Main.sRight(inputArr[x], 8);
                            ret += InsertSeq(outStr, 4, 6, "0x", ", 0x", ", ");
                        }
                        return ret.Remove(ret.Length - 2) + " };";
                    }
                case 1: //Hex String Array
                    if (output == 0)
                    {
                        uint addr = uint.Parse(extra, System.Globalization.NumberStyles.HexNumber);
                        string[] inputArr = inputStr.Split(' ');
                        for (int x = 0; x < inputArr.Length; x += 4)
                        {
                            string outStr = "";
                            int stop = x + 4;
                            while (x < inputArr.Length && x < stop)
                            {
                                outStr += inputArr[x];
                                x++;
                            }
                            if (x != stop)
                                outStr = outStr.PadRight(8, '0');
                            ret += "2 " + addr.ToString("X8") + " " + outStr + Environment.NewLine;
                            addr += 4;
                        }
                        return ret;
                    }
                    else
                    {
                        string[] inputArr = inputStr.Split(' ');
                        ret = "byte[] " + extra + " = { ";
                        for (int x = 0; x < inputArr.Length; x++)
                            ret += "0x" + inputArr[x] + ", ";
                        return ret.Remove(ret.Length - 2) + " };";
                    }
                case 2: //Byte Array
                    if (output == 0)
                    {
                        uint addr = uint.Parse(extra, System.Globalization.NumberStyles.HexNumber);
                        string[] inputArr = inputStr.Replace("0x", "").Replace(",", "").Replace(" };", "").Split(' ');
                        int start = 0;
                        while (inputArr[start].Length != 2 && start < inputArr.Length || inputArr[start+1] == "=")
                            start++;
                        for (int x = start; x < inputArr.Length; x += 4)
                        {
                            //if ((x + 3) >= 
                            string outStr = inputArr[x] + inputArr[x + 1] + inputArr[x + 2] + inputArr[x + 3];
                            ret += "2 " + addr.ToString("X8") + " " + outStr + Environment.NewLine;
                            addr += 4;
                        }
                        return ret;
                    }
                    else
                    {
                        string[] inputArr = inputStr.Split(',');
                        ret = Main.sRight(inputArr[0], 2) + " ";
                        for (int x = 1; x < (inputArr.Length-1); x++)
                            ret += Main.sRight(inputArr[x], 2) + " ";
                        return ret + Main.sMid(inputArr[inputArr.Length-1], 3, 2);
                    }
            }
            return null;
        }

        public static string InsertSeq(string insStr, int startOff, int inc, string start, string seq, string end)
        {
            string ret = insStr.Insert(0, start);
            for (int x = startOff; x < ret.Length; x += inc)
            {
                ret = ret.Insert(x, seq);
            }
            return ret.Insert(ret.Length, end);
        }

        private void ConversionForm_Load(object sender, EventArgs e)
        {
            InputCBox.SelectedIndex = 0;
            OutputCBox.SelectedIndex = 0;
        }

        private void stateNormal_MouseDown(object sender, MouseEventArgs e)
        {
            stateMouseDown.Location = stateNormal.Location;
            stateMouseDown.Visible = true;
            stateNormal.Visible = false;

            if (isClicked == false)
            {
                isClicked = true;
                if (OutputCBox.SelectedIndex == 0)
                {
                    try
                    {
                        int.Parse(extraTBox.Text, System.Globalization.NumberStyles.HexNumber);
                    }
                    catch
                    {
                        MessageBox.Show("Error: Start Address not valid!");
                        stateNormal_MouseUp(null, null);
                        return;
                    }
                }
                else if (OutputCBox.SelectedIndex == 2)
                {
                    if (extraTBox.Text == "")
                    {
                        MessageBox.Show("Error: Name not valid!");
                        stateNormal_MouseUp(null, null);
                        return;
                    }
                    extraTBox.Text = extraTBox.Text.Replace(" ", "_");
                }
                OutputTBox.Text = ConvertFormat(InputCBox.SelectedIndex, OutputCBox.SelectedIndex, InputTBox.Text, extraTBox.Text);
            }
        }

        private void stateNormal_MouseUp(object sender, MouseEventArgs e)
        {
            stateNormal.Visible = true;
            stateMouseDown.Visible = false;
            isClicked = false;
        }

        private void OutputCBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (OutputCBox.SelectedIndex)
            {
                case 0:
                    this.Size = new Size(395, 395);
                    if (extraTBox.Text == "")
                        extraTBox.Text = "00000000";
                    label3.Text = "Start Address";
                    extraTBox.Visible = true;
                    label3.Visible = true;
                    break;
                case 1:
                    this.Size = new Size(395, 350);
                    extraTBox.Visible = false;
                    label3.Visible = false;
                    break;
                case 2:
                    this.Size = new Size(395, 395);
                    if (extraTBox.Text == "")
                        extraTBox.Text = "NAME";
                    label3.Text = "Name of Array";
                    extraTBox.Visible = true;
                    label3.Visible = true;
                    break;
            }
        }
    }
}
