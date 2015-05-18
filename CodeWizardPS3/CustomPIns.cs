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
    public partial class CustomPIns : Form
    {
        bool textChanged = false;

        public CustomPIns()
        {
            InitializeComponent();
        }

        public static Main.pInstruction[] LoadAllPCI(string directory)
        {
            string[] files = System.IO.Directory.GetFiles(directory, "*.cwpi");
            if (files == null)
                return new Main.pInstruction[0];

            Main.pInstruction[] ret = new Main.pInstruction[files.Length];

            for (int x = 0; x < files.Length; x++)
            {
                string[] text = FileIO.OpenFile(files[x]).Split('\n');
                ret[x].name = text[0];
                ret[x].format = text[1];
                for (int z = 2; z < text.Length; z++)
                    ret[x].asm += text[z] + "\n";
                int y = ret[x].asm.Length - 1;
                while (y >= 0 && ret[x].asm[y] == '\n')
                    y--;
                ret[x].asm = ret[x].asm.Remove(y+1);


                ret[x].regs = ParseFormat(text[1]);
            }
            return ret;
        }

        private static string[] ParseFormat(string format)
        {
            if (format == "")
                return new string[0];
            string[] ret = format.Split(',');
            for (int x = 0; x < ret.Length; x++)
                ret[x] = ret[x].Replace(" ", "");
            return ret;
        }

        private void savePCI_Click(object sender, EventArgs e)
        {
            for (int x = 0; x < listCPI.Items.Count; x++)
            {
                Main.customPIns[x].name = Main.customPIns[x].name.Replace(" ", "_");
                string file = Main.DirOf(Application.ExecutablePath) + "\\Pseudo Instructions\\" + Main.customPIns[x].name + ".cwpi";
                string text = Main.customPIns[x].name + "\n";
                text += Main.customPIns[x].format + "\n";
                text += Main.customPIns[x].asm;
                FileIO.SaveFile(file, text);

                Main.customPIns[x].regs = ParseFormat(Main.customPIns[x].format);
                Main.UpdateAutoCompleteBox(null);

                //Pseudo Instructions
                foreach (Main.pInstruction pIns in Main.customPIns)
                {
                    if (Main.pInsRegex == "")
                        Main.pInsRegex = Main.insRegex.Replace("%s", pIns.name);
                    else
                        Main.pInsRegex += "|" + Main.insRegex.Replace("%s", pIns.name);
                }

            }
        }

        private void listCPI_DoubleClick(object sender, EventArgs e)
        {
            listCPI.Items.Add("newItem" + (listCPI.Items.Count).ToString());

            Array.Resize(ref Main.customPIns, Main.customPIns.Length + 1);

            Main.customPIns[listCPI.Items.Count - 1].name = "newItem" + (listCPI.Items.Count).ToString();
            Main.customPIns[listCPI.Items.Count - 1].format = "";
            Main.customPIns[listCPI.Items.Count - 1].asm = "";
            
            listCPI.SelectedIndex++;
        }

        private void listCPI_SelectedIndexChanged(object sender, EventArgs e)
        {
            int x = listCPI.SelectedIndex;
            if (x < 0 || textChanged)
                return;

            namePCI.Text = Main.customPIns[x].name;
            formatPCI.Text = Main.customPIns[x].format;
            asmPCI.Text = Main.customPIns[x].asm;
        }

        private void listCPI_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                int oldSel = listCPI.SelectedIndex;

                if (System.IO.File.Exists(Main.DirOf(Application.ExecutablePath) + "\\Pseudo Instructions\\" + Main.customPIns[oldSel].name + ".cwpi"))
                    System.IO.File.Delete(Main.DirOf(Application.ExecutablePath) + "\\Pseudo Instructions\\" + Main.customPIns[oldSel].name + ".cwpi");

                for (int x = listCPI.SelectedIndex; x < (listCPI.Items.Count-1); x++)
                    Main.customPIns[x] = Main.customPIns[x + 1];

                Array.Resize(ref Main.customPIns, Main.customPIns.Length - 1);
                listCPI.Items.RemoveAt(listCPI.SelectedIndex);
                if (oldSel < listCPI.Items.Count)
                    listCPI.SelectedIndex = oldSel;
                else
                    listCPI.SelectedIndex = oldSel - 1;
            }
        }

        private void exitPCI_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }

        private void namePCI_TextChanged(object sender, EventArgs e)
        {
            int x = listCPI.SelectedIndex;
            if (x < 0)
                return;

            textChanged = true;
            listCPI.Items[x] = namePCI.Text;
            textChanged = false;
            Main.customPIns[x].name = namePCI.Text;
        }

        private void formatPCI_TextChanged(object sender, EventArgs e)
        {
            int x = listCPI.SelectedIndex;
            if (x < 0)
                return;

            Main.customPIns[x].format = formatPCI.Text;
        }

        private void CustomPIns_Load(object sender, EventArgs e)
        {
            Main.customPIns = CustomPIns.LoadAllPCI(Main.DirOf(Application.ExecutablePath) + "\\Pseudo Instructions");

            for (int x = 0; x < Main.customPIns.Length; x++)
            {
                listCPI.Items.Add(Main.customPIns[x].name);

                if (listCPI.SelectedIndex < 0)
                    listCPI.SelectedIndex = 0;
            }
        }

        private void asmPCI_TextChanged(object sender, EventArgs e)
        {
            int x = listCPI.SelectedIndex;
            if (x < 0)
                return;

            Main.customPIns[x].asm = asmPCI.Text;
        }

        private void openDir_Click(object sender, EventArgs e)
        {
            string path = Application.StartupPath + "\\Pseudo Instructions";
            if (System.IO.Directory.Exists(path))
                System.Diagnostics.Process.Start(path);
        }
    }
}
