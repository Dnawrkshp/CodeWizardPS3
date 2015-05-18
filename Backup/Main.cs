using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace CodeWizardPS3
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();

            ASMBox.DragDrop += new DragEventHandler(ASMBox_DragDrop);
            ASMBox.AllowDrop = true;
            CodeBox.DragDrop += new DragEventHandler(CodeBox_DragDrop);
            CodeBox.AllowDrop = true;
        }

        /* Miscellaneous */
        public string GlobalFileName = "";

        /* Colorizer */
        public int ASMBoxLen = 0;
        public static bool StopColor = false;
        public bool ColorIns = true;
        public RichTextBox colorRTB = new RichTextBox();

        /* Settings */
        public static string settFile = "";
        public static string pathDir = "";
        public static int outputType = 0;
        public static int bitOrder = 0;
        public static uint minMemRange = 0x00000000;
        public static uint maxMemRange = 0x01BB0000;
        public static uint startStackPtr = 0x01BB0000;
        public static bool colEnabled = true;
        public static bool colIns = true;
        public static bool colReg = true;
        public static bool colCom = true;
        public static bool colCommand = true;
        public static bool colLab = true;

        /* Undo */
        public struct undoStr
        {
            public string Text;
            public int Start;
        };
        public undoStr[] undoArray = new undoStr[50];
        public int undoArrCnt = 0;

        /* Custom Pseudo Instructions */
        public struct pInstruction
        {
            public string name;
            public string[] regs;
            public string format;
            public string asm;
        };
        public static pInstruction[] customPIns = new pInstruction[0]; 

        /* ---------- Miscellaneous ---------- */
        /* Returns the directory of the path */
        public static string DirOf(string path)
        {
            int dirLen = path.LastIndexOf('\\');
            if (dirLen < 0)
                dirLen = path.LastIndexOf('/');

            return sLeft(path, dirLen);
        }

        /* Shows debug menu and prints Text */
        public void DebugMenu(string Text)
        {
            DebugPopup a = new DebugPopup();
            a.debugText = Text;
            a.MainWidth = this.Width;
            a.MainLeft = this.Left;
            a.MainHeight = this.Height;
            a.MainTop = this.Top;
            a.TopMost = true;
            a.Visible = false;
            a.Show();
        }

        /* Converts a byte array into a string */
        public static string ByteAToString(byte[] a)
        {
            int x = 0;
            string ret = "";
            for (x = 0; x < a.Length; x++)
                ret = (char)a[x] + ret;
            return ret;
        }

        /* Converts a string into a byte array */
        public static byte[] StringToByteArray(string str)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            return encoding.GetBytes(str);
        }

        //Equivalent to VB6's Left function (grabs length many left most characters in text)
        public static string sLeft(string text, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", length, "length must be > 0");
            else if (length == 0 || text.Length == 0)
                return "";
            else if (text.Length < length)
                return text;
            else
                return text.Substring(0, length);
        }

        //Equivalent to VB6's Right function (grabs length many right most characters in text)
        public static string sRight(string text, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", length, "length must be > 0");
            else if (length == 0 || text.Length == 0)
                return "";
            else if (text.Length <= length)
                return text;
            else
                return text.Substring(text.Length - length, length);
        }

        //Equivalent to VB6's Mid function
        public static string sMid(string text, int index, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", length, "length must be > 0");
            else if (length == 0 || text.Length == 0)
                return "";
            else if (text.Length < (length + index))
                return text;
            else
                return text.Substring(index, length);
        }

        /* -------- Colorizer -------- */
        /* Color line by going through each substring */
        public void ColorLine(int lineNum)
        {
            if (colorRTB == null)
                return;

            string prefix = "\r";
            if (lineNum == 0)
                prefix = "";

            if (lineNum < 0 || lineNum >= colorRTB.Lines.Length)
                return;
            int line = colorRTB.GetFirstCharIndexFromLine(lineNum);
            int end = colorRTB.GetFirstCharIndexFromLine(colorRTB.GetLineFromCharIndex(line)) + colorRTB.Lines[lineNum].Length;
            int x = 0;

            if (line == end || line < 0 || end < 0)
                return;

            ColorIns = false;
            if (colCom && colEnabled) //Color Single-line Comment
            {
                if (ColorASM("//", line, end, -1, Color.GreenYellow, 1))
                    return;

                int tempFind = colorRTB.Find("/*", line, RichTextBoxFinds.None);
                int tempFind2 = 0;
                if (tempFind >= 0 && tempFind <= end)
                {
                    int oldSel = colorRTB.SelectionStart;
                    colorRTB.Enabled = false;

                    tempFind2 = colorRTB.Find("*/", tempFind + 2, RichTextBoxFinds.None);
                    if (tempFind2 < 0)
                        tempFind2 = tempFind;
                    colorRTB.Select(tempFind, tempFind2 - tempFind + 2);
                    colorRTB.SelectionColor = Color.GreenYellow;
                    colorRTB.Select(tempFind2 + 2, 1);
                    colorRTB.SelectionColor = colorRTB.ForeColor;

                    colorRTB.Enabled = true;
                    colorRTB.Focus();
                    colorRTB.SelectionStart = oldSel;
                    return;
                }
                tempFind2 = colorRTB.Find("*/", line, RichTextBoxFinds.None);
                if (tempFind < 0 && tempFind2 >= 0)
                    return;
            }

            ColorIns = true;
            if (colCommand && colEnabled)
            {
                if (ColorASM("hookl", line, end, 0, Color.Red, 1))
                    return;
                if (ColorASM("hook", line, end, 0, Color.Red, 1))
                    return;
                if (ColorASM("address", line, end, 0, Color.Red, 1))
                    return;
                if (ColorASM("hexcode", line, end, 0, Color.Red, 1))
                    return;
                if (ColorASM("import", line, end, 0, Color.Red, 1))
                    return;
                if (ColorASM("float", line, end, 0, Color.Red, 1))
                    return;
                if (ColorASM("string", line, end, 0, Color.Red, 1))
                    return;
            }
            if (colLab && colEnabled)
            {
                if (ColorLabels(line, end, Color.LightSteelBlue) == 1)
                    return;
            }

            if (colReg && colEnabled)
                ColorRegs(line, end);

            /* Commands with register arguments */
            if (colCommand && colEnabled)
            {
                if (ColorASM("setreg", line, end, 0, Color.Red, 1))
                    return;
                /*
                if (ColorASM("lwi", line, end, 0, Color.Red, 1))
                    return;
                */

                //Pseudo Instructions
                for (int cpiCnt = 0; cpiCnt < customPIns.Length; cpiCnt++)
                {
                    if (colorRTB.Find(prefix + customPIns[cpiCnt].name + " ", 0) >= 0)
                        if (ColorINS(prefix + customPIns[cpiCnt].name + " ", line - prefix.Length, end, 0, Color.DarkRed, 0))
                            return;
                }
            }

            if (colIns && colEnabled)
            {
                x = ASMDeclarations.GetInsStart(colorRTB.Lines[lineNum][0]);
                while (ASMDeclarations.ASMDef[x].name != null)
                {
                    switch (ASMDeclarations.ASMDef[x].type)
                    {
                        case ASMDeclarations.typeBNCMP:
                            if (ColorINS(prefix + ASMDeclarations.ASMDef[x].name + " ", line - prefix.Length, end, 0, Color.Snow, 0))
                                return;
                            break;
                        case ASMDeclarations.typeBNC:
                            if (ColorINS(prefix + ASMDeclarations.ASMDef[x].name + " ", line - prefix.Length, end, 0, Color.Blue, 0))
                                return;
                            break;
                        case ASMDeclarations.typeFNAN:
                        case ASMDeclarations.typeNAN:
                            if (ColorINS(prefix + ASMDeclarations.ASMDef[x].name + " ", line - prefix.Length, end, 0, Color.Orange, 0))
                                return;
                            break;
                        case ASMDeclarations.typeFOFI:
                        case ASMDeclarations.typeOFI:
                            if (ColorINS(prefix + ASMDeclarations.ASMDef[x].name + " ", line - prefix.Length, end, 0, Color.Purple, 0))
                                return;
                            break;
                        case ASMDeclarations.typeNOP:
                            if (ColorINS(prefix + ASMDeclarations.ASMDef[x].name, line - prefix.Length, end, 0, Color.Yellow, 0))
                                return;
                            break;
                        case ASMDeclarations.typeIMM:
                            if (ColorINS(prefix + ASMDeclarations.ASMDef[x].name + " ", line - prefix.Length, end, 0, Color.LightPink, 0))
                                return;
                            break;
                        case ASMDeclarations.typeSPR:
                            if (ColorINS(prefix + ASMDeclarations.ASMDef[x].name + " ", line - prefix.Length, end, 0, Color.Gainsboro, 0))
                                return;
                            break;
                        case ASMDeclarations.typeCND:
                            if (ColorINS(prefix + ASMDeclarations.ASMDef[x].name + " ", line - prefix.Length, end, 0, Color.Violet, 0))
                                return;
                            break;
                        case ASMDeclarations.typeIMM5:
                            if (ColorINS(prefix + ASMDeclarations.ASMDef[x].name + " ", line - prefix.Length, end, 0, Color.LightPink, 0))
                                return;
                            break;
                    }
                    x++;
                }
            }

            if (colEnabled)
            {
                //Otherwise color green
                colorRTB.Select(line, end - line);
                colorRTB.SelectionColor = Color.GreenYellow;
                colorRTB.Select(end, 1);
                colorRTB.SelectionColor = colorRTB.ForeColor;
            }
            else
            {
                //Otherwise color white
                colorRTB.Select(line, end - line);
                colorRTB.SelectionColor = Color.White;
                colorRTB.Select(end, 1);
                colorRTB.SelectionColor = colorRTB.ForeColor;
            }
        }

        /* Color regs in colorRTB */
        public void ColorRegs(int line, int end)
        {
            return;

            int oldStart = 0;
            ColorIns = false;
            for (int x = 0; x < ASMDeclarations.RegColArr.Length; x++)
            {
                if (ASMDeclarations.RegColArr[x].reg == null)
                    break;

                oldStart = colorRTB.Find(ASMDeclarations.RegColArr[x].reg, line, RichTextBoxFinds.None);
                while (oldStart >= line && oldStart <= end)
                {
                    ColorASM(ASMDeclarations.RegColArr[x].reg, line, end, oldStart, ASMDeclarations.RegColArr[x].col, 1);
                    oldStart = colorRTB.Find(ASMDeclarations.RegColArr[x].reg, oldStart+1, RichTextBoxFinds.None);
                }
            }
            ColorIns = true;
        }

        /* Color instruction in colorRTB */
        public bool ColorASM(string word, int line, int end, int off, Color col, int clearLen)
        {
            if (word == null)
                return false;
            int len = word.Length;
            if (off < 0)
                len = end - line;
            if (off <= 0)
                off = colorRTB.Find(word, line, RichTextBoxFinds.None);
            if (off >= line && off <= end)
            {
                if (ColorIns && off > 0 && (colorRTB.Text[off-1] != '\n' && colorRTB.Text[off-1] != '\r'))
                    return false;
                else if (ColorIns == false && (off + len) < colorRTB.Text.Length && 
                        colorRTB.Text[off + len] != ' ' && colorRTB.Text[off + len] != ',' &&
                        colorRTB.Text[off + len] != '(' && colorRTB.Text[off + len] != ')' &&
                        colorRTB.Text[off + len] != '\n' && colorRTB.Text[off + len] != '\r')
                    return false;

                if (off > 0 && (colorRTB.Text[off - 1] == '$' || colorRTB.Text[off - 1] == '%'))
                {
                    off--;
                    len++;
                }
                colorRTB.Select(off, len);
                colorRTB.SelectionColor = col;
                colorRTB.Select(off + len, clearLen);
                colorRTB.SelectionColor = colorRTB.ForeColor;
                return true;
            }
            return false;
        }

        /* Color instruction in colorRTB */
        public bool ColorINS(string word, int line, int end, int off, Color col, int clearLen)
        {
            return false;

            if (word == null)
                return false;
            if (line < 0)
            {
                line = 0;
                word = word.Replace("\r", "");
            }

            int len = word.Length;
            if (off < 0)
                len = end - line + 2;
            if (off <= 0)
                off = colorRTB.Find(word, line, RichTextBoxFinds.None);
            if (off >= line && off < end)
            {
                if (line > 0)
                    off++;
                if (line != 0)
                    len--;
                colorRTB.Select(off, len);
                colorRTB.SelectionColor = col;
                colorRTB.Select(off + len, clearLen);
                colorRTB.SelectionColor = colorRTB.ForeColor;
                return true;
            }
            return false;
        }

        public int ColorLabels(int line, int end, Color col)
        {
            return 0;

            string lineStr = colorRTB.Lines[colorRTB.GetLineFromCharIndex(line)];
            int off = colorRTB.Find(":", line, RichTextBoxFinds.None);

            if (off < 0 || off > end)
                return 0;

            //Label is declared
            if ((off-line) == (lineStr.Length-1))
            {
                colorRTB.Select(line, end - line);
                colorRTB.SelectionColor = col;
                colorRTB.Select(end, 1);
                colorRTB.SelectionColor = colorRTB.ForeColor;
                return 1;
            }
            else //Label is referred
            {
                int len = colorRTB.Find("\r", off, RichTextBoxFinds.None);
                len -= off;
                if (len < 0)
                    len = line - end;
                if (len < 0)
                    return 0;
                colorRTB.Select(off-1, len+1);
                colorRTB.SelectionColor = col;
                colorRTB.Select(off+len+1, 0);
                colorRTB.SelectionColor = colorRTB.ForeColor;
                return 2;
            }
        }

        /* Color lines surrounding cursor */
        public void ColorSurroundingLines()
        {
            return;

            if (colorRTB == null)
                return;

            int oldSel = colorRTB.SelectionStart;
            int lineNum = colorRTB.GetLineFromCharIndex(oldSel);
            //int oldScroll = colorRTB.Car

            //HideSelection will hide the selection color when the RTB is not in focus
            colorRTB.HideSelection = true;
            AssembleASM.Focus();

            ColorLine(lineNum - 1);
            ColorLine(lineNum);
            ColorLine(lineNum + 1);

            colorRTB.Focus();
            colorRTB.SelectionStart = oldSel;
            colorRTB.SelectionLength = 0;
        }

        /* Color selected line */
        public void ColorSelLine(int offset)
        {
            return;

            if (colorRTB == null)
                return;

            int oldSel = colorRTB.SelectionStart;
            int lineNum = colorRTB.GetLineFromCharIndex(oldSel);

            //HideSelection will hide the selection color when the RTB is not in focus
            colorRTB.HideSelection = true;
            CodeBox.Focus();

            ColorLine(lineNum + offset);

            colorRTB.Focus();
            colorRTB.SelectionStart = oldSel;
            colorRTB.SelectionLength = 0;
        }

        /* Color whole RTB */
        public void ASMColorBox()
        {
            ASMBox.Enabled = false;
            codeDefBox.Enabled = false;

            colorRTB = ASMBox;

            for (int z = 0; z < 2; z++)
            {
                StopColor = true;
                if (z == 1)
                    colorRTB = codeDefBox;

                int oldSel = colorRTB.SelectionStart;

                colorRTB.SelectionStart = 0;
                colorRTB.SelectionLength = colorRTB.Text.Length;
                colorRTB.SelectionColor = Color.White;
                colorRTB.SelectionLength = 0;

                if (colEnabled)
                {
                    int x = 0, size = colorRTB.Lines.Length;
                    for (x = 0; x < size; x++)
                        ColorLine(x);
                }

                colorRTB.SelectionStart = oldSel;
                colorRTB.SelectionLength = 0;

                if (z == 0)
                    ASMBox = colorRTB;
                else if (z == 1)
                    codeDefBox = colorRTB;
            }

            ASMBox.Enabled = true;
            codeDefBox.Enabled = true;

            StopColor = false;
        }

        /* Form related things */
        private void AssembleASM_Click(object sender, EventArgs e)
        {
            CodeBox.Text = ASMDeclarations.ASMAssemble(ASMBox, codeDefBox, outputType, GlobalFileName);
            if (ASMDeclarations.debugString != "")
                DebugMenu(ASMDeclarations.debugString);

            return;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            ASMDeclarations.DeclareHelpStr();
            ASMDeclarations.DeclareInstructions();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                GlobalFileName = args[1];
                LoadCWP3RTB();

                codeDefBox.SelectionStart = 0;
                codeDefBox.SelectionLength = codeDefBox.Text.Length;
                codeDefBox.SelectionFont = codeDefBox.Font;
                codeDefBox.SelectionLength = 0;

                ASMBox.SelectionStart = 0;
                ASMBox.SelectionLength = ASMBox.Text.Length;
                ASMBox.SelectionFont = ASMBox.Font;
                ASMBox.SelectionLength = 0;
            }

            //Set minimum size
            this.MinimumSize = new System.Drawing.Size(654, 564);

            //Set settings file
            int off = Application.ExecutablePath.LastIndexOf("\\");
            settFile = sLeft(Application.ExecutablePath, off+1);
            pathDir = settFile;
            settFile += "cwps3.ini";

            //Load settings
            string a = FileIO.OpenFile(settFile);
            if (a != "" && a != null)
            {
                string[] b = a.Split('\n');
                try
                {
                    outputType = Convert.ToInt32(b[0]);
                    bitOrder = Convert.ToInt32(b[1]);
                    minMemRange = Convert.ToUInt32(b[2], 16);
                    maxMemRange = Convert.ToUInt32(b[3], 16);
                    colEnabled = Convert.ToBoolean(b[4]);
                    colIns = Convert.ToBoolean(b[5]);
                    colReg = Convert.ToBoolean(b[6]);
                    colCom = Convert.ToBoolean(b[7]);
                    colCommand = Convert.ToBoolean(b[8]);
                    colLab = Convert.ToBoolean(b[9]);
                }
                catch { }
            }

            if (System.IO.Directory.Exists(Main.DirOf(Application.ExecutablePath) + "\\Pseudo Instructions") == false)
                System.IO.Directory.CreateDirectory(Main.DirOf(Application.ExecutablePath) + "\\Pseudo Instructions");

            customPIns = CustomPIns.LoadAllPCI(Main.DirOf(Application.ExecutablePath) + "\\Pseudo Instructions");

            ASMBox.Focus();
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            ASMBox.Height = this.Height - 150 - 90 - 15;
            ASMBox.Width = this.Width - 50 - CodeBox.Width;
            ASMBox.Location = new Point(12, 12 + 15);

            CodeBox.Location = new Point(this.Width - CodeBox.Width - 30, 12 + 15);
            CodeBox.Height = this.Height - 150 - 15;

            AssembleASM.Location = new Point(CodeBox.Location.X, this.Height - 155 + AssembleASM.Height);
            DisassembleASM.Location = new Point(CodeBox.Location.X, this.Height - 155 + (AssembleASM.Height * 2));
            CopyCode.Location = new Point(CodeBox.Location.X, this.Height - 155 + (AssembleASM.Height * 3));

            OpenCWP3.Location = new Point(CodeBox.Location.X + AssembleASM.Width, this.Height - 155 + AssembleASM.Height);
            SaveCWP3.Location = new Point(CodeBox.Location.X + AssembleASM.Width, this.Height - 155 + (AssembleASM.Height * 2));
            SaveAsCWP3.Location = new Point(CodeBox.Location.X + AssembleASM.Width, this.Height - 155 + (AssembleASM.Height * 3));

            codeDefBox.Height = this.Height - ASMBox.Height - ASMBox.Location.Y - 60;
            codeDefBox.Width = this.Width - 50 - CodeBox.Width;
            codeDefBox.Location = new Point(12, this.Height - 155 - 90 + AssembleASM.Height);
        }

        private void CopyCode_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            string a = CodeBox.Text;
            
            Clipboard.SetText(CodeBox.Text);
        }

        private void SaveAsCWP3_Click(object sender, EventArgs e)
        {
            SaveFileDialog fd = new SaveFileDialog();
            fd.Filter = "CodeWizard PS3 Files (*.cwp3)|*.cwp3|All files (*.*)|*.*";
            fd.RestoreDirectory = true;

            if (fd.ShowDialog() == DialogResult.OK)
            {
                //FileIO.SaveFile(fd.FileName, ASMBox.Text);
                GlobalFileName = fd.FileName;
                FileIO.SaveCWP3(new RichTextBox[] { ASMBox, codeDefBox }, GlobalFileName);
            }
        }

        private void SaveCWP3_Click(object sender, EventArgs e)
        {
            if (GlobalFileName == "" || GlobalFileName == null)
                SaveAsCWP3_Click(null, null);
            else
                FileIO.SaveCWP3(new RichTextBox[] { ASMBox, codeDefBox }, GlobalFileName);
        }

        private void OpenCWP3_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "CodeWizard PS3 files (*.cwp3)|*.cwp3|All files (*.*)|*.*";
            fd.RestoreDirectory = true;

            if (fd.ShowDialog() == DialogResult.OK)
            {
                GlobalFileName = fd.FileName;
                LoadCWP3RTB();

                codeDefBox.SelectionStart = 0;
                codeDefBox.SelectionLength = codeDefBox.Text.Length;
                codeDefBox.SelectionFont = codeDefBox.Font;
                codeDefBox.SelectionLength = 0;

                ASMBox.SelectionStart = 0;
                ASMBox.SelectionLength = ASMBox.Text.Length;
                ASMBox.SelectionFont = ASMBox.Font;
                ASMBox.SelectionLength = 0;
            }
        }

        private void LoadCWP3RTB()
        {
            ASMBox.Text = "";
            codeDefBox.Text = "// Declarations\n";

            string[] tempStr = FileIO.LoadCWP3(GlobalFileName);
            if (tempStr.Length >= 1)
                ASMBox.LoadFile(tempStr[0]);
            if (tempStr.Length >= 2)
                codeDefBox.LoadFile(tempStr[1]);

            foreach (string str in tempStr)
                System.IO.File.Delete(str);
        }

        void ASMBox_DragDrop(object sender, DragEventArgs e)
        {
            object filename = e.Data.GetData("FileDrop");
            if (filename != null)
            {
                var list = filename as string[];

                if (list != null && list[0] != null)
                {
                    GlobalFileName = (string)list[0];
                    LoadCWP3RTB();

                    ASMBox.SelectionStart = 0;
                    ASMBox.SelectionLength = ASMBox.Text.Length;
                    ASMBox.SelectionFont = ASMBox.Font;
                    ASMBox.SelectionLength = 0;

                    codeDefBox.SelectionStart = 0;
                    codeDefBox.SelectionLength = codeDefBox.Text.Length;
                    codeDefBox.SelectionFont = codeDefBox.Font;
                    codeDefBox.SelectionLength = 0;
                }

            }
        }

        void CodeBox_DragDrop(object sender, DragEventArgs e)
        {
            object filename = e.Data.GetData("FileDrop");
            if (filename != null)
            {
                var list = filename as string[];

                if (list != null && list[0] != null)
                {
                    CodeBox.Clear();
                    string a = FileIO.OpenFile((string)list[0]);
                    CodeBox.Text = a;
                    GlobalFileName = (string)list[0];
                }

            }
        }

        private void ASMBox_TextChanged(object sender, EventArgs e)
        {
            if (!StopColor)
            {
                //colorRTB = ASMBox;
                //ColorSurroundingLines();
                //SaveASM();
                //ASMBox = colorRTB;
                //colorRTB = null;
            }
        }

        private void ASMBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.L && e.Control && e.Shift == false)
                ASMColorBox();
            else if (e.KeyCode == Keys.V && e.Control && e.Shift == false)
                return;
            
            e.SuppressKeyPress = true;
        }

        private void CodeBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 65 && e.Control)
            {
                CodeBox.SelectionStart = 0;
                CodeBox.SelectionLength = CodeBox.Text.Length;
            }
            e.SuppressKeyPress = true;
        }
        private void ASMBox_KeyDown(object sender, KeyEventArgs e)
        {
            RichTextBox rtb = ASMBox;
            if (e.KeyData == Keys.Left && rtb.SelectionStart == 0)
                e.SuppressKeyPress = true;
            if (e.KeyData == Keys.Right && rtb.SelectionStart == rtb.Text.Length)
                e.SuppressKeyPress = true;
            if (e.KeyData == Keys.Up && rtb.GetLineFromCharIndex(rtb.SelectionStart) == 0)
                e.SuppressKeyPress = true;
            if (e.KeyData == Keys.Down && rtb.GetLineFromCharIndex(rtb.SelectionStart) == rtb.GetLineFromCharIndex(rtb.Text.Length))
                e.SuppressKeyPress = true;
            if (e.KeyData == Keys.Back && rtb.SelectionStart == 0 && rtb.SelectionLength == 0)
                e.SuppressKeyPress = true;
            if (e.Control && e.KeyCode == Keys.Z)
            {
                UndoASM();
                e.SuppressKeyPress = true;
            }
        }

        public void UndoASM()
        {
            if (undoArrCnt <= 0)
                return;

            undoArrCnt--;
            ASMBox.Text = undoArray[undoArrCnt].Text;
            ASMBox.SelectionStart = undoArray[undoArrCnt].Start;
        }

        public void SaveASM()
        {
            if (undoArray[undoArrCnt].Text == ASMBox.Text)
                return;

            undoArrCnt++;
            if (undoArrCnt >= undoArray.Length)
            {
                //Shift everything down
                undoArrCnt = undoArray.Length-1;
                for (int x = 0; x < (undoArray.Length - 1); x++)
                {
                    undoArray[x].Text = undoArray[x+1].Text;
                    undoArray[x].Start = undoArray[x+1].Start;
                }
            }
            //Copy new string in
            undoArray[undoArrCnt].Text = ASMBox.Text;
            undoArray[undoArrCnt].Start = ASMBox.SelectionStart;
        }

        private void CodeBox_TextChanged(object sender, EventArgs e)
        {
            if (CodeBox.Lines.Length > (CodeBox.Height / CodeBox.Font.GetHeight()))
                CodeBox.ScrollBars = ScrollBars.Vertical;
            else
                CodeBox.ScrollBars = ScrollBars.None;
        }

        private void DisassembleASM_Click(object sender, EventArgs e)
        {
            if (ASMBox.Text != "")
            {
                DialogResult a = MessageBox.Show(null, "If you continue, all the assembly will be erased!", "", MessageBoxButtons.OKCancel);
                if (DialogResult.Cancel == a)
                    return;
            }

            ASMBox.Text = ASMDeclarations.ASMDisassemble(CodeBox.Text);

            ASMColorBox();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenCWP3_Click(null, null);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveCWP3_Click(null, null);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAsCWP3_Click(null, null);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GlobalFileName = "";
            ASMBox.Text = "";
        }

        private void assembleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AssembleASM_Click(null, null);
        }

        private void instructionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InsBox a = new InsBox();
            a.Visible = true;
            a.Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox a = new AboutBox();
            a.Visible = true;
            a.Show();
        }

        private void disassembleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisassembleASM_Click(null, null);
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsForm a = new OptionsForm();
            a.Visible = true;
            a.Show();
        }

        private void helpToolStripMenuItemBox_Click(object sender, EventArgs e)
        {
            string filepath = pathDir + "Help.txt";
            if (System.IO.File.Exists(filepath) == false)
                MessageBox.Show("Error: " + filepath + " doesn't exist!");
            else
                System.Diagnostics.Process.Start(filepath);
        }

        private void conversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConversionForm a = new ConversionForm();
            a.Visible = true;
            a.Show();
        }

        private void cToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void simulateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int oldOutput = outputType;
            outputType = 0;
            //Compile code
            AssembleASM_Click(null, null);
            string[] hexArr = CodeBox.Text.Split('\r');
            ASMEmu.MemCode[] codesArg = new ASMEmu.MemCode[hexArr.Length-1];
            for (int x = 0; x < (hexArr.Length-1); x++)
            {
                string[] code = hexArr[x].Replace("\n", "").Split(' ');
                codesArg[x].addr = uint.Parse(code[1], System.Globalization.NumberStyles.HexNumber);
                codesArg[x].val = uint.Parse(code[2], System.Globalization.NumberStyles.HexNumber);
            }

            outputType = oldOutput;

            ASMEmu a = new ASMEmu();
            a.codes = codesArg;
            a.Visible = true;
            a.Show();
        }

        private void pinstructionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CustomPIns a = new CustomPIns();

            a.Visible = true;
            a.Show();
        }

        private void codeDefBox_TextChanged(object sender, EventArgs e)
        {
            if (!StopColor)
            {
                //colorRTB = codeDefBox;
                //ColorSurroundingLines();
                //SaveASM();
                //codeDefBox = colorRTB;
                colorRTB = null;
            }
        }

    }
}
