using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using FastColoredTextBoxNS;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;
using System.Diagnostics;
using Ionic.Zip;

namespace CodeWizardPS3
{
    public partial class Main : Form
    {

        #region Declarations

        string cwVersion = "1.2.6";

        /* Errors */
        ListBox errorsLBox = new ListBox();

        /* Autocomplete */
        //public static AutocompleteMenu popupAutoCom;
        public static List<AutocompleteItem> autoComWords = new List<AutocompleteItem>();

        /* Input Box Argument Structure */
        public struct IBArg
        {
            public string label;
            public string defStr;
            public string retStr;
        };

        /* Miscellaneous */
        public string GlobalFileName = "";

        /* Settings */
        public static string settFile = "";
        public static string pathDir = "";
        public static int outputType = 0;
        public static int bitOrder = 0;
        public static uint minMemRange = 0x00000000;
        public static uint maxMemRange = 0x02000000;
        public static uint startStackPtr = 0x02000000;
        public static bool themeLight = false;
        public static Color[] themeColors = new Color[] { Color.Black, Color.White };

        /* Custom Pseudo Instructions */
        public struct pInstruction
        {
            public string name;
            public string[] regs;
            public string format;
            public string asm;
        };
        public static pInstruction[] customPIns = new pInstruction[0];

        /* TabPage */
        public struct tabStripPage
        {
            public TabPage tab;                     /* Tab page */
            public FastColoredTextBox ASMBox;       /* Holds main assembly */
            public FastColoredTextBox codeDefBox;   /* Holds declarations */
            public string GlobalFileName;           /* Path to file */
            public AutocompleteMenu autoComBox;     /* Auto Complete Box */
        };
        public static List<tabStripPage> asmTabs = new List<tabStripPage>();
        public static int tabCtrlSIndex = 0;

        //FastColoredTextBox asmTabs[asmTabControl.SelectedIndex].ASMBox = new FastColoredTextBox();
        //FastColoredTextBox asmTabs[asmTabControl.SelectedIndex].codeDefBox = new FastColoredTextBox();

        /* Highlighting */
        string wordRegex = @"(%s^* )";
        public static string insRegex = "^\\s+((%s^* )|(%s$)|(%s[\r\n]))|^((%s^* )|(%s$)|(%s[\r\n]))";
        string regRegex = @"(%s)";

        TextStyle pInsStyle = new TextStyle(Brushes.Teal, null, FontStyle.Regular);
        public static string pInsRegex = "(%s^* )|(%s$)|(%s[\r\n])";

        TextStyle[] regStyles = new TextStyle[0];
        string[] regStyleRegex = new string[0];

        TextStyle[] insStyles = new TextStyle[0];
        string[] insStyleRegex = new string[0];

        TextStyle commentStyle = new TextStyle(Brushes.LimeGreen, null, FontStyle.Regular);
        //string commStyleRegex = @"(//.*)|(/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/)";
        string commStyleRegex = @"(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')|//.*|/\*(?s:.*?)\*/";

        TextStyle commandStyle = new TextStyle(Brushes.BurlyWood, null, FontStyle.Regular);
        string cmdStyleRegex = @"(address^* )|(hook^* )|(hookl^* )|(import^* )|(hexcode^* )|(setreg^* )|(float^* )|(string^* )";

        /* Instruction Type Brushes */
        public static Brush[] typeBrush =
        {
            Brushes.Yellow,
            Brushes.LightBlue,
            Brushes.Yellow,
            Brushes.OrangeRed,
            Brushes.MediumVioletRed,
            Brushes.SandyBrown,
            Brushes.SkyBlue,
            Brushes.Sienna,
            Brushes.Magenta,
            Brushes.LightSalmon, //typeFNAN, typeFOFI, typeFCND
        };

        /* Register Type Brushes */
        public static Brush[] regBrush =
        {
            Brushes.Lavender,
            Brushes.Indigo,
            Brushes.Honeydew
        };

        #endregion

        #region Syntax Highlighting

        private void ASMBox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            FastColoredTextBox fctb = ((FastColoredTextBox)sender);

            ((FastColoredTextBox)sender).ChangedLineColor = Color.Indigo;
            /* Comments */
            ((FastColoredTextBox)sender).Range.ClearStyle(commentStyle);
            ((FastColoredTextBox)sender).Range.SetStyle(commentStyle, commStyleRegex, RegexOptions.Multiline);

            /* Commands */
            e.ChangedRange.ClearStyle(commandStyle);
            e.ChangedRange.SetStyle(commandStyle, cmdStyleRegex);

            /* Pseudo Instructions */
            e.ChangedRange.ClearStyle(pInsStyle);
            e.ChangedRange.SetStyle(pInsStyle, pInsRegex, RegexOptions.Multiline);

            /* Instructions */
            for (int x = (insStyles.Length - 1); x >= 0; x--)
            {
                e.ChangedRange.ClearStyle(insStyles[x]);
                e.ChangedRange.SetStyle(insStyles[x], insStyleRegex[x], RegexOptions.Multiline);
            }

            /* Registers */
            for (int x = (regStyles.Length - 1); x >= 0; x--)
            {
                e.ChangedRange.ClearStyle(regStyles[x]);
                e.ChangedRange.SetStyle(regStyles[x], regStyleRegex[x]);
            }

            //If a label and it doesn't exist in the list update the autoComWords with the new label
            string writtenLabel = ((FastColoredTextBox)sender).GetLineText(e.ChangedRange.Start.iLine);
            if (isLabel(writtenLabel))
            {
                var re = @"(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')|//.*|/\*(?s:.*?)\*/";
                string[] arg1 = new string[] { Regex.Replace(asmTabs[asmTabControl.SelectedIndex].ASMBox.Text, re, "$1"), Regex.Replace(asmTabs[asmTabControl.SelectedIndex].codeDefBox.Text, re, "$1") };
                UpdateAutoCompleteBox(arg1);
                //asmTabs[asmTabControl.SelectedIndex].ASMBox.
            }

            //If adding an import then reassess imports
            if (writtenLabel.StartsWith("import "))
            {
                try
                {
                    string[] impWords = writtenLabel.Split(' ');
                    if (impWords.Length > 1 && System.IO.File.Exists(DirOf(GlobalFileName) + "\\" + new System.IO.FileInfo(impWords[1]).Name))
                    {
                        var re = @"(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')|//.*|/\*(?s:.*?)\*/";
                        string file = ASMDeclarations.GetImports(Regex.Replace(asmTabs[asmTabControl.SelectedIndex].ASMBox.Text, re, "$1") + "\n" + Regex.Replace(asmTabs[asmTabControl.SelectedIndex].codeDefBox.Text, re, "$1"), GlobalFileName);
                        UpdateAutoCompleteBox(new string[] { Regex.Replace(file, re, "$1") });
                    }
                }
                catch { }
            }

            //clear folding markers
            e.ChangedRange.ClearFoldingMarkers();

            //e.ChangedRange.SetFoldingMarkers("{", "}");//allow to collapse brackets block
            e.ChangedRange.SetFoldingMarkers(@"#region\b", @"#endregion\b"); //allow to collapse #region blocks
            //e.ChangedRange.SetFoldingMarkers(@"#region\b", @"#endregion\b");//allow to collapse #region blocks
            e.ChangedRange.SetFoldingMarkers(@"/\*", @"\*/");//allow to collapse comment block
        }

        private void HighlightEverything()
        {
            FastColoredTextBox[] color = new FastColoredTextBox[] { asmTabs[asmTabControl.SelectedIndex].ASMBox, asmTabs[asmTabControl.SelectedIndex].codeDefBox };

            for (int z = 0; z < color.Length; z++)
            {
                /* Comments */
                color[z].Range.ClearStyle(commentStyle);
                color[z].Range.SetStyle(commentStyle, commStyleRegex);

                /* Numbers */
                color[z].Range.ClearStyle(commandStyle);
                color[z].Range.SetStyle(commandStyle, cmdStyleRegex);

                /* Pseudo Instructions */
                color[z].Range.ClearStyle(pInsStyle);
                color[z].Range.SetStyle(pInsStyle, pInsRegex, RegexOptions.Multiline);

                /* Instructions */
                for (int x = (insStyles.Length - 1); x >= 0; x--)
                {
                    color[z].VisibleRange.ClearStyle(insStyles[x]);
                    color[z].VisibleRange.SetStyle(insStyles[x], insStyleRegex[x], RegexOptions.Multiline);
                }

                /* Registers */
                for (int x = (regStyles.Length - 1); x >= 0; x--)
                {
                    color[z].Range.ClearStyle(regStyles[x]);
                    color[z].Range.SetStyle(regStyles[x], regStyleRegex[x]);
                }
            }


        }

        #endregion

        #region Miscellaneous

        /* ---------- Miscellaneous ---------- */

        private void UpdateThemeColor(object sender, EventArgs e)
        {
            BackColor = themeLight ? Color.White : Color.Black;
            ForeColor = themeLight ? Color.Black : Color.White;

            HandleColorControls(this.Controls);
        }

        public void HandleColorControls(Control.ControlCollection plgCtrl)
        {
            foreach (Control ctrl in plgCtrl)
            {
                if (ctrl is GroupBox || ctrl is Panel || ctrl is TabControl || ctrl is TabPage ||
                    ctrl is UserControl || ctrl is ListBox || ctrl is ListView)
                    HandleColorControls(ctrl.Controls);

                ctrl.BackColor = this.BackColor;
                ctrl.ForeColor = this.ForeColor;
            }
        }

        public FastColoredTextBox InitializeFastTB(int mode)
        {
            FastColoredTextBox ret = new FastColoredTextBox();

            if (mode == 0)
            {
                //asmTabs[asmTabControl.SelectedIndex].ASMBox
                ret.Size = ASMBox_RTB.Size;
                ret.Font = ASMBox_RTB.Font;
                ret.BackColor = ASMBox_RTB.BackColor;
                ret.ForeColor = ASMBox_RTB.ForeColor;
                ret.BorderStyle = BorderStyle.Fixed3D;

                /* ----- Setup auto complete box ----- */
                //UpdateAutoCompleteBox(ret, null);
                //UpdateLabels(new string[1] { "" });
            }
            else if (mode == 1)
            {
                //asmTabs[asmTabControl.SelectedIndex].codeDefBox
                ret.Size = codeDefBox_RTB.Size;
                ret.Font = codeDefBox_RTB.Font;
                ret.BackColor = codeDefBox_RTB.BackColor;
                ret.ForeColor = codeDefBox_RTB.ForeColor;
                ret.BorderStyle = BorderStyle.Fixed3D;
            }

            ret.AutoIndent = false;
            ret.LineNumberColor = ForeColor;
            ret.IndentBackColor = BackColor;
            ret.CurrentLineColor = Color.DarkGray;
            ret.ShowFoldingLines = true;

            ret.DragDrop += new DragEventHandler(ASMBox_DragDrop);
            ret.AllowDrop = true;
            ret.TextChanged += new EventHandler<TextChangedEventArgs>(ASMBox_TextChanged);
            ret.KeyUp += new KeyEventHandler(ASMBox_KeyUp);

            return ret;
        }

        /* Returns true if it is a label */
        public static bool isLabel(string name)
        {
            if (name == "" || name == null)
                return false;
            if (name.Split(' ')[0].EndsWith(":") && name != ":")
                return true;

            return false;
        }

        /* Initializes the command, instruction, and register tooltip strings */
        public static void InitToolTipACBox()
        {
            //Construct words
            autoComWords.Clear();
            AutocompleteItem item = new AutocompleteItem();

            //Instructions
            string oldOp = "";
            foreach (ASMDeclarations.PPCInstr op in ASMDeclarations.ASMDef)
            {
                if (op.name != null && op.name != "")
                {
                    item = new AutocompleteItem(op.name, -1, op.name, "Instruction: " + op.title, op.help);

                    if (oldOp != op.name)
                    {
                        autoComWords.Add(item);
                        oldOp = op.name;
                    }
                }
            }

            //Registers
            foreach (ASMDeclarations.RegCol reg in ASMDeclarations.RegColArr)
                if (reg.reg != "" && reg.reg != null)
                {
                    item = new AutocompleteItem(reg.reg, -1, reg.reg, "Register: " + reg.title + " (" + reg.reg + ")", ASMDeclarations.helpReg[ASMDeclarations.GetRegHelpIndex(reg.reg)]);
                    autoComWords.Add(item);
                }
            //Commands
            string[] commands = new string[] { "address", "hook", "hookl", "setreg", "hexcode", "import", "float", "string" };
            string[] titleCom = new string[] { "Set Address", "Set Hook Address", "Set Hook Address With Link", "Set Register",
                                                "Hexadecimal/Decimal Code", "Import CWP3 File", "Float To Hex", "String To Hex" };
            for (int comInt = 0; comInt < commands.Length; comInt++)
            {
                item = new AutocompleteItem(commands[comInt], -1, commands[comInt], "Command: " + titleCom[comInt], ASMDeclarations.helpCom[comInt]);
                autoComWords.Add(item);
            }
        }

        /* Updates the auto complete popup with the correct words and size for the selected ASMBox */
        public static void UpdateAutoCompleteBox(string[] data)
        {
            List<AutocompleteItem> autoComLabels = _UpdateAutoCompleteBox(data);
            int ind = tabCtrlSIndex;
            if (ind < 0 || ind >= asmTabs.Count)
                ind = asmTabs.Count - 1;

            if (tabCtrlSIndex >= 0)
            {
                //asmTabs[tabCtrlSIndex].autoComBox = new AutocompleteMenu(asmTabs[tabCtrlSIndex].ASMBox);
                asmTabs[ind].autoComBox.ShowItemToolTips = true;
                asmTabs[ind].autoComBox.ToolTipDuration = 0x7FFFFFFF;
                asmTabs[ind].autoComBox.ToolTip.BackColor = themeColors[0];
                asmTabs[ind].autoComBox.ToolTip.ForeColor = themeColors[1];
                //popupAutoCom.ImageList = descImageList;
                asmTabs[ind].autoComBox.BackColor = themeColors[0];
                asmTabs[ind].autoComBox.ForeColor = themeColors[1];
                asmTabs[ind].autoComBox.Font = new Font(FontFamily.GenericMonospace, 9.75f);
                asmTabs[ind].autoComBox.SelectedColor = Color.Red;
                asmTabs[ind].autoComBox.Items.MaximumSize = new System.Drawing.Size(200, 300);
                asmTabs[ind].autoComBox.Items.Width = 200;
                asmTabs[ind].autoComBox.MinFragmentLength = 0;
                asmTabs[ind].autoComBox.AppearInterval = 0x7FFFFFFF;
                asmTabs[ind].autoComBox.Items.SetAutocompleteItems(autoComLabels);
            }
        }

        private static List<AutocompleteItem> _UpdateAutoCompleteBox(string[] data)
        {
            List<AutocompleteItem> autoComLabels = new List<AutocompleteItem>();
            List<string> autoComLabelsStr = new List<string>();

            string text = "";
            if (data != null)
                foreach (string str in data)
                    text += str + "\r\n";
            else if (asmTabs.Count > 0 && tabCtrlSIndex >= 0)
                text = asmTabs[tabCtrlSIndex].ASMBox.Text + "\r\n" + asmTabs[tabCtrlSIndex].codeDefBox.Text;

            if (text == "")
                return null;

            //Find all instances of a label
            // (/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/)|(//.*)
            string regexPattern = @"[\r\n].*:";
            MatchCollection ret = Regex.Matches("\r\n" + text, regexPattern);
            foreach (Match match in ret)
            {
                if (match.Value.IndexOf(' ') < 0 && match.Value != ":")
                {
                    string val = match.Value.Split(' ')[0].Replace(":", "").Replace("\n", "");
                    if (!autoComLabelsStr.Any(val.Contains))
                    {
                        AutocompleteItem item = new AutocompleteItem(val, -1, val, "Label: " + val, sMid(text, match.Index, 100) + "...");
                        autoComLabels.Add(item);
                        autoComLabelsStr.Add(val);
                    }
                }
            }

            //Pseudo instructions
            foreach (pInstruction pIns in customPIns)
                if (pIns.name != "" && pIns.name != null)
                {
                    AutocompleteItem item = new AutocompleteItem(pIns.name, -1, pIns.name, "Pseudo Instruction: " + pIns.name, "Format: " + pIns.format);
                    autoComLabels.Add(item);
                }

            autoComLabels.AddRange(autoComWords);
            return autoComLabels;
        }

        public static List<AutocompleteItem> SortACIList(List<AutocompleteItem> list)
        {
            return (List<AutocompleteItem>)list.OrderBy(AutocompleteItem => AutocompleteItem.MenuText).ToList(); ;
        }

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

        #endregion

        #region CodeWizard Updater

        public void RunUpdateChecker(bool allowForce)
        {
            string[] updateStr = CheckForUpdate();
            string newVer = updateStr[0].Split(' ')[0].Replace("\r", "").Replace("\n", "");
            string updateVer = updateStr[0].Replace("\r", "").Replace("\n", "");
            bool update = int.Parse(newVer.Replace(".", "")) > int.Parse(cwVersion.Split(' ')[0].Replace(".", ""));
            string title = update ?
                "CodeWizard PS3 Version " + updateVer + " is available for download.\nWould you like to update and restart CodeWizard?" :
                "CodeWizard is up-to-date! Would you like to Force Update?";
            string updateArg = "";
            if (updateStr.Length > 1)
                updateArg = String.Join(Environment.NewLine, updateStr);
            else
                updateArg = "";

            //string title = update ? "Update Available" : "Force Update?";

            bool allow = false;
            if (allowForce || update)
            {
                updateForm mBox = new updateForm();
                mBox.Title = title;
                mBox.UpdateStr = updateArg;
                mBox.ForeColor = ForeColor;
                mBox.BackColor = BackColor;
                mBox.Show();

                while (mBox.Return < 0)
                    Application.DoEvents();
                allow = (mBox.Return == 0) ? false : true;
                mBox.Close();
            }

            if (allow)
            {

                Form loadingFrm = new Form();
                loadingFrm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
                loadingFrm.ControlBox = false;
                loadingFrm.Size = new System.Drawing.Size(200, 75);
                Label lbl = new Label();
                lbl.Text = "Updating CodeWizard PS3...";
                lbl.AutoSize = false;
                lbl.Size = loadingFrm.Size;
                lbl.Location = new Point(0, 0);
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                lbl.Font = new System.Drawing.Font(this.Font.FontFamily, 15.0f);
                loadingFrm.Controls.Add(lbl);
                loadingFrm.BackColor = BackColor;
                loadingFrm.ForeColor = ForeColor;
                loadingFrm.Show();
                loadingFrm.TopLevel = true;
                loadingFrm.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width / 2 - (loadingFrm.Width / 2),
                    Screen.PrimaryScreen.WorkingArea.Height / 2 - (loadingFrm.Height / 2));
                Application.DoEvents();

                UpdateCWPS3();
            }
        }

        public string[] CheckForUpdate()
        {
            string webpath = "http://www.cod-orc.com/Dnawrkshp/CodeWizardPS3Update.txt";

            try
            {
                string store = Path.GetTempFileName();
                WebClient Client = new WebClient();
                Client.DownloadFile(webpath, store);

                string[] ver = File.ReadAllLines(store);
                File.Delete(store);

                return ver;
            }
            catch (Exception)
            {
                return new string[] { "0" };
            }
        }

        public void UpdateCWPS3()
        {
            string webpath = "http://www.cod-orc.com/Dnawrkshp/cwps3UpdateDir.zip";
            //FileInfo ncFI = new FileInfo(Application.ExecutablePath);
            string store = Application.StartupPath + "\\" + "cwps3UpdateDir.zip";

            WebClient Client = new WebClient();
            Client.DownloadFile(webpath, store);

            //Decompress rar
            DecompressFile(store, Application.StartupPath + "\\cwps3UpdateDir\\");
            File.Delete(store);

            //If there is a new updater use that and delete it from the extracted directory
            if (File.Exists(Application.StartupPath + "\\cwps3UpdateDir\\CWPS3Updater.exe"))
            {
                File.Copy(Application.StartupPath + "\\cwps3UpdateDir\\CWPS3Updater.exe",
                    Application.StartupPath + "\\CWPS3Updater.exe", true);
                File.Delete(Application.StartupPath + "\\cwps3UpdateDir\\CWPS3Updater.exe");
            }

            System.Threading.Thread.Sleep(1000);
            Process.Start("CWPS3Updater.exe", Process.GetCurrentProcess().Id.ToString() +
                " \"" + Application.StartupPath + "\\cwps3UpdateDir\"" +
                " \"" + Application.StartupPath + "\"" +
                " \"" + Application.ExecutablePath + "\"");
            Close();
        }

        public static void DecompressFile(string file, string directory)
        {
            using (ZipFile archive = ZipFile.Read(file))
            {
                foreach (ZipEntry entry in archive.Entries)
                {
                    try
                    {
                        if (entry.UncompressedSize > 0)
                        {
                            string mergedPath = Path.Combine(directory, entry.FileName);
                            FileInfo fi = new FileInfo(directory);
                            if (!Directory.Exists(fi.Directory.FullName))
                                Directory.CreateDirectory(fi.Directory.FullName);
                            entry.Extract(directory, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show("Exception: \n" + error.Message);
                    }
                }
            }
        }

        #endregion

        /* For running as admin (registry), from http://victorhurdugaci.com/using-uac-with-c-part-1/ */
        private void RunElevated(string fileName)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.Verb = "runas";
            processInfo.FileName = fileName;
            string args = "";
            for (int x = 1; x < Environment.GetCommandLineArgs().Length; x++)
            {
                args += "\"" + Environment.GetCommandLineArgs()[x] + "\" ";
            }
            processInfo.Arguments = args.Trim(' ');

            try
            {
                Process.Start(processInfo);
            }
            catch (Win32Exception)
            {

            }
        }

        public Main()
        {
            InitializeComponent();

            CodeBox.DragDrop += new DragEventHandler(CodeBox_DragDrop);
            CodeBox.AllowDrop = true;

            FormClosing += new FormClosingEventHandler(Main_FormClosing);

            OptionsForm.UpdateTheme += new EventHandler(UpdateThemeColor);
        }

        /* Form related things */
        private void AssembleASM_Click(object sender, EventArgs e)
        {
            errorsLBox.Items.Clear();
            CodeBox.Text = ASMDeclarations.ASMAssemble(asmTabs[asmTabControl.SelectedIndex].ASMBox, asmTabs[asmTabControl.SelectedIndex].codeDefBox, outputType, GlobalFileName);
            if (ASMDeclarations.debugString != "")
            {
                //DebugMenu(ASMDeclarations.debugString);
                string[] lines = ASMDeclarations.debugString.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    errorsLBox.Items.Add(line);
                }
            }

            Main_Resize(null, null);
            return;
        }

        private void ErrorsLBox_DblClick(object sender, EventArgs e)
        {
            if (errorsLBox.SelectedIndex >= 0 && errorsLBox.SelectedIndex < ASMDeclarations.debugErrors.Count)
            {
                ASMDeclarations.ErrorOutput err = ASMDeclarations.debugErrors[errorsLBox.SelectedIndex];
                tabStripPage t = new tabStripPage();

                if (err.fullPath != "[new]")
                {
                    GlobalFileName = err.fullPath;
                    OpenGFNTab();

                    t = asmTabs.Where(x => new FileInfo(x.GlobalFileName).FullName == err.fullPath).FirstOrDefault();
                }
                else
                {
                    int tCnt = 0;
                    foreach (tabStripPage tsp in asmTabs)
                    {
                        if (tsp.tab.Text == err.fullPath)
                        {
                            t = tsp;
                            tCnt++;
                        }
                    }

                    if (tCnt > 1)
                    {
                        DialogResult dr = MessageBox.Show("More than one tab called \"" + err.fullPath + "\".\nPlease select the one that contains the error and click Yes.", "Is the correct tab selected?", MessageBoxButtons.YesNo);
                        if (dr == System.Windows.Forms.DialogResult.No)
                            return;
                        else
                            t = asmTabs[asmTabControl.SelectedIndex];
                    }
                    else
                        asmTabControl.SelectedTab = t.tab;
                }
                if (t.tab != null)
                {
                    int ind = 0;
                    List<int> regions = new List<int>();
                    if (err.name.IndexOf("'s Declaration") >= 0 && t.codeDefBox.Text != "")
                    {
                        for (int x = 0; x < err.line; x++)
                        {
                            string text = t.codeDefBox.GetLine(x).Text;
                            ind += text.Length;
                            while (ind < t.codeDefBox.Text.Length && (t.codeDefBox.Text[ind] == '\r' || t.codeDefBox.Text[ind] == '\n'))
                                ind++;

                            if (text.IndexOf("#region") >= 0)
                                regions.Add(x);
                            else if (text.IndexOf("#endregion") >= 0 && regions.Count > 0)
                                regions.RemoveAt(regions.Count - 1);
                        }

                        foreach (int reg in regions)
                            t.codeDefBox.ExpandBlock(reg);
                        t.codeDefBox.Select();
                        t.codeDefBox.Navigate(err.line);
                        t.codeDefBox.SelectionStart = ind + err.charIndex;
                        t.codeDefBox.SelectionLength = t.codeDefBox.GetLineLength(err.line) - err.charIndex;
                    }
                    else if (t.ASMBox.Text != "")
                    {
                        for (int x = 0; x < (err.line); x++)
                        {
                            string text = t.ASMBox.GetLine(x).Text;
                            ind += text.Length;
                            while (ind < t.ASMBox.Text.Length && (t.ASMBox.Text[ind] == '\r' || t.ASMBox.Text[ind] == '\n'))
                                ind++;

                            if (text.IndexOf("#region") >= 0)
                                regions.Add(x);
                            else if (text.IndexOf("#endregion") >= 0 && regions.Count > 0)
                                regions.RemoveAt(regions.Count - 1);
                        }

                        foreach (int reg in regions)
                            t.ASMBox.ExpandBlock(reg);
                        t.ASMBox.Select();
                        t.ASMBox.Navigate(err.line);
                        t.ASMBox.SelectionStart = ind + err.charIndex;
                        t.ASMBox.SelectionLength = t.ASMBox.GetLineLength(err.line) - err.charIndex;
                    }
                }
            }
        }

        private void ErrorsLBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Text = "CodeWizard PS3 " + cwVersion + " BETA by Dnawrkshp  - " + errorsLBox.Items[errorsLBox.SelectedIndex].ToString();
        }

        private void ErrorsLBox_LostFocus(object sender, EventArgs e)
        {
            Text = "CodeWizard PS3 " + cwVersion + " BETA by Dnawrkshp";
        }

        private void Main_Load(object sender, EventArgs e)
        {
            RunUpdateChecker(false);
            
            Text = "CodeWizard PS3 " + cwVersion + " BETA by Dnawrkshp";

            if (Registry.ClassesRoot.OpenSubKey("CodeWizard PS3 Source") == null)
            {
                WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
                if (!hasAdministrativeRight)
                {
                    RunElevated(Application.ExecutablePath);
                    this.Close();
                }

                Registry
                    .ClassesRoot
                    .CreateSubKey(".cwp3")
                    .SetValue("", "CodeWizard PS3 Source", RegistryValueKind.String);
                Registry
                    .ClassesRoot
                    .CreateSubKey(@"CodeWizard PS3 Source\shell\open\command")
                    .SetValue("", Application.ExecutablePath + " \"%1\"", RegistryValueKind.String);
                Registry
                    .ClassesRoot
                    .CreateSubKey(@"CodeWizard PS3 Source\DefaultIcon")
                    .SetValue("", Application.ExecutablePath + ",0", RegistryValueKind.String);
            }
            
            ASMDeclarations.DeclareHelpStr();
            ASMDeclarations.DeclareInstructions();
            InitToolTipACBox();

            /* ----- Setup Styles ----- */

            //Registers
            Array.Resize(ref regStyles, 3);
            Array.Resize(ref regStyleRegex, 3);
            int regCnt = 0;
            foreach (ASMDeclarations.RegCol reg in ASMDeclarations.RegColArr)
            {
                switch (reg.reg[0])
                {
                    case 'r': //GPR
                        regCnt = 0;
                        break;
                    case 'x':
                    case 'l':
                    case 'c': //CTR and SPR
                        regCnt = 1;
                        break;
                    case 'f': //FPR
                        regCnt = 2;
                        break;
                }

                if (regStyles[regCnt] == null)
                {
                    regStyles[regCnt] = new TextStyle(reg.col, null, FontStyle.Regular);
                    regStyleRegex[regCnt] = regRegex.Replace("%s", reg.reg);
                }
                else
                    regStyleRegex[regCnt] = regRegex.Replace("%s", reg.reg) + "|" + regStyleRegex[regCnt];
            }

            //Instructions
            Array.Resize(ref insStyles, typeBrush.Length);
            Array.Resize(ref insStyleRegex, typeBrush.Length);

            List<string> ops = new List<string>();

            foreach (ASMDeclarations.PPCInstr op in ASMDeclarations.ASMDef)
            {
                if (op.name != null)
                    ops.Add(op.name);
            }
            ops.Sort((aSort, bSort) => aSort.Length.CompareTo(bSort.Length));
            foreach (string name in ops)
            {
                ASMDeclarations.PPCInstr op = ASMDeclarations.ASMDef.Where<ASMDeclarations.PPCInstr>(x => x.name == name).FirstOrDefault();
                int ind = op.type;
                if (ind >= typeBrush.Length)
                    ind = typeBrush.Length - 1;
                if (op.name != null && op.name != "")
                {
                    if (insStyles[ind] == null)
                        insStyles[ind] = new TextStyle(typeBrush[ind], null, FontStyle.Regular);

                    if (insStyleRegex[ind] == "" || insStyleRegex[ind] == null)
                        insStyleRegex[ind] = insRegex.Replace("%s", op.name);
                    else
                        insStyleRegex[ind] += "|" + insRegex.Replace("%s", op.name);
                }
            }

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                for (int argCnt = 1; argCnt < args.Length; argCnt++)
                {
                    GlobalFileName = args[argCnt];
                    OpenGFNTab();
                    HighlightEverything();
                    UpdateAutoCompleteBox(new string[] { asmTabs[asmTabControl.SelectedIndex].ASMBox.Text, asmTabs[asmTabControl.SelectedIndex].codeDefBox.Text });
                }
            }

            //Set minimum size
            this.MinimumSize = new System.Drawing.Size(654, 564);

            //Set settings file
            settFile += Application.StartupPath + "\\cwps3.ini";

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
                    startStackPtr = Convert.ToUInt32(b[4], 16);
                    //themeLight = (Convert.ToUInt32(b[4]) == 0) ? false : true;
                    //BackColor = themeLight ? Color.White : Color.Black;
                    //ForeColor = themeLight ? Color.Black : Color.White;
                    //HandleColorControls(this.Controls);
                }
                catch { }
            }

            if (System.IO.Directory.Exists(Main.DirOf(Application.ExecutablePath) + "\\Pseudo Instructions") == false)
                System.IO.Directory.CreateDirectory(Main.DirOf(Application.ExecutablePath) + "\\Pseudo Instructions");

            customPIns = CustomPIns.LoadAllPCI(Main.DirOf(Application.ExecutablePath) + "\\Pseudo Instructions");

            //Setup errorsLBox
            errorsLBox.ForeColor = ForeColor;
            errorsLBox.BackColor = BackColor;
            errorsLBox.DoubleClick += new EventHandler(ErrorsLBox_DblClick);
            errorsLBox.SelectedIndexChanged += new EventHandler(ErrorsLBox_SelectedIndexChanged);
            errorsLBox.LostFocus += new EventHandler(ErrorsLBox_LostFocus);
            Controls.Add(errorsLBox);

            //Pseudo Instructions
            foreach (pInstruction pIns in customPIns)
            {
                if (pInsRegex == "")
                    pInsRegex = insRegex.Replace("%s", pIns.name);
                else
                    pInsRegex += "|" + insRegex.Replace("%s", pIns.name);
            }

            if (System.IO.File.Exists(Application.StartupPath + "\\oldtabs"))
            {
                string[] tabs = System.IO.File.ReadAllLines(Application.StartupPath + "\\oldtabs");
                int tabSel = 0;
                foreach (string line in tabs)
                {
                    if (line != null && line != "")
                    {
                        if (line[0] == '\t')
                            tabSel = Convert.ToInt32(line.Replace("\t", ""));
                        else
                        {
                            GlobalFileName = line;
                            OpenGFNTab();
                            Main_Resize(null, null);
                        }
                    }
                }
                if (tabSel < asmTabControl.TabPages.Count)
                    asmTabControl.SelectedIndex = tabSel;
            }

            if (asmTabs.Count == 0)
            {
                makeNewTab("[new]"); 
                Main_Resize(null, null);
            }

            asmTabs[asmTabControl.SelectedIndex].ASMBox.Focus();
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            int leftSizeOff = 0;
            if (errorsLBox.Items.Count > 0)
            {
                leftSizeOff = 100;
                errorsLBox.Visible = true;
            }
            else
                errorsLBox.Visible = false;

            if (asmTabControl.SelectedIndex >= 0 && asmTabControl.SelectedIndex < asmTabs.Count)
            {
                asmTabControl.Height = this.Height - 75;
                asmTabControl.Width = this.Width - 50 - CodeBox.Width;
                asmTabControl.Location = new Point(10, 25);

                asmTabs[asmTabControl.SelectedIndex].ASMBox.Height = asmTabControl.Height - 175;
                asmTabs[asmTabControl.SelectedIndex].ASMBox.Width = asmTabControl.Width - 5;
                asmTabs[asmTabControl.SelectedIndex].ASMBox.Location = new Point(0, 0);

                CodeBox.Location = new Point(this.Width - CodeBox.Width - 30, 5 + 15);
                CodeBox.Height = this.Height - 150 - 15 - leftSizeOff;

                AssembleASM.Location = new Point(CodeBox.Location.X, this.Height - 155 - leftSizeOff + AssembleASM.Height);
                DisassembleASM.Location = new Point(CodeBox.Location.X, this.Height - 155 - leftSizeOff + (AssembleASM.Height * 2));
                CopyCode.Location = new Point(CodeBox.Location.X, this.Height - 155 - leftSizeOff + (AssembleASM.Height * 3));

                OpenCWP3.Location = new Point(CodeBox.Location.X + AssembleASM.Width, this.Height - 155 - leftSizeOff + AssembleASM.Height);
                SaveCWP3.Location = new Point(CodeBox.Location.X + AssembleASM.Width, this.Height - 155 - leftSizeOff + (AssembleASM.Height * 2));
                SaveAsCWP3.Location = new Point(CodeBox.Location.X + AssembleASM.Width, this.Height - 155 - leftSizeOff + (AssembleASM.Height * 3));

                label1.Size = new System.Drawing.Size(CodeBox.Width, 15);
                label1.Location = new Point(CodeBox.Location.X, this.Height - 155 + AssembleASM.Height - 10);

                errorsLBox.Size = new System.Drawing.Size(CodeBox.Width, leftSizeOff - 30);
                errorsLBox.Location = new Point(CodeBox.Location.X, this.Height - 155 + AssembleASM.Height + 10);

                asmTabs[asmTabControl.SelectedIndex].codeDefBox.Height = asmTabControl.TabPages[0].Height - asmTabs[asmTabControl.SelectedIndex].ASMBox.Height - asmTabs[asmTabControl.SelectedIndex].ASMBox.Location.Y;
                asmTabs[asmTabControl.SelectedIndex].codeDefBox.Width = asmTabControl.Width - 5;
                asmTabs[asmTabControl.SelectedIndex].codeDefBox.Location = new Point(0, asmTabControl.TabPages[0].Height - asmTabs[asmTabControl.SelectedIndex].codeDefBox.Height);

                ASMBox_RTB.Location = asmTabs[asmTabControl.SelectedIndex].ASMBox.Location;
                ASMBox_RTB.Size = asmTabs[asmTabControl.SelectedIndex].ASMBox.Size;
                codeDefBox_RTB.Location = asmTabs[asmTabControl.SelectedIndex].codeDefBox.Location;
                codeDefBox_RTB.Size = asmTabs[asmTabControl.SelectedIndex].codeDefBox.Size;

                ASMBox_RTB.Visible = false;
                codeDefBox_RTB.Visible = false;
            }
        }

        private void CopyCode_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            string a = CodeBox.Text;
            //a = "";
            //foreach (ASMDeclarations.PPCInstr aD in ASMDeclarations.ASMDef) {
            //    a += aD.name + Environment.NewLine;
            //}
            
            //Clipboard.SetText(a);
            Clipboard.SetDataObject(new DataObject(DataFormats.Text, a));
        }

        private void SaveAsCWP3_Click(object sender, EventArgs e)
        {
            SaveFileDialog fd = new SaveFileDialog();
            fd.Filter = "CodeWizard PS3 Files (*.cwp3)|*.cwp3|All files (*.*)|*.*";
            fd.RestoreDirectory = true;

            if (fd.ShowDialog() == DialogResult.OK)
            {
                //FileIO.SaveFile(fd.FileName, asmTabs[asmTabControl.SelectedIndex].ASMBox.Text);
                GlobalFileName = fd.FileName;
                FileIO.SaveCWP3(new FastColoredTextBox[] { asmTabs[asmTabControl.SelectedIndex].ASMBox, asmTabs[asmTabControl.SelectedIndex].codeDefBox }, GlobalFileName);
                asmTabs[asmTabControl.SelectedIndex].tab.Text = new System.IO.FileInfo(GlobalFileName).Name;
                //tabStripPage item = new tabStripPage();
                //item.ASMBox = asmTabs[asmTabControl.SelectedIndex].ASMBox;
                //item.autoComBox = asm
                tabStripPage item = asmTabs[asmTabControl.SelectedIndex];
                item.GlobalFileName = GlobalFileName;
                asmTabs[asmTabControl.SelectedIndex] = item;
            }
        }

        private void SaveCWP3_Click(object sender, EventArgs e)
        {
            if (GlobalFileName == "" || GlobalFileName == null)
                SaveAsCWP3_Click(null, null);
            else
            {
                FileIO.SaveCWP3(new FastColoredTextBox[] { asmTabs[asmTabControl.SelectedIndex].ASMBox, asmTabs[asmTabControl.SelectedIndex].codeDefBox }, GlobalFileName);
            }
        }

        private void OpenCWP3_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "CodeWizard PS3 files (*.cwp3)|*.cwp3|All files (*.*)|*.*";
            fd.RestoreDirectory = true;

            if (fd.ShowDialog() == DialogResult.OK)
            {
                GlobalFileName = fd.FileName;
                OpenGFNTab();
                Main_Resize(null, null);
            }
        }

        public void OpenGFNTab()
        {
            int newTab = 0;

            for (newTab = 0; newTab < asmTabControl.TabPages.Count; newTab++)
            {
                //[new] tab is blank
                if (asmTabControl.TabPages[newTab].Text == "[new]" &&
                    (asmTabs[newTab].ASMBox.Text == "" &&
                        (asmTabs[newTab].codeDefBox.Text == "" || asmTabs[newTab].codeDefBox.Text == "// Declarations")))
                    return;
                //File is already open
                if (new FileInfo(asmTabs[newTab].GlobalFileName).FullName == new FileInfo(GlobalFileName).FullName)
                {
                    asmTabControl.SelectedIndex = newTab;
                    return;
                }
            }

            if (newTab == asmTabControl.TabPages.Count || newTab < 0)
                newTab = asmTabs.Count;
            else
            {
                asmTabControl.TabPages.RemoveAt(newTab);
                asmTabs.RemoveAt(newTab);
            }

            tabStripPage tempTSP = new tabStripPage();
            tempTSP.ASMBox = InitializeFastTB(0);
            tempTSP.codeDefBox = InitializeFastTB(1);
            tempTSP.autoComBox = new AutocompleteMenu(tempTSP.ASMBox);
            tempTSP.tab = new TabPage(new System.IO.FileInfo(GlobalFileName).Name);
            tempTSP.tab.ForeColor = ForeColor;
            tempTSP.tab.BackColor = BackColor;
            tempTSP.tab.Controls.Add(tempTSP.ASMBox);
            tempTSP.tab.Controls.Add(tempTSP.codeDefBox);
            tempTSP.GlobalFileName = GlobalFileName;
            tempTSP.ASMBox.AutoIndent = true;
            tempTSP.codeDefBox.AutoIndent = true;
            asmTabs.Add(tempTSP);
            asmTabControl.TabPages.Add(asmTabs[newTab].tab);
            asmTabControl.SelectedIndex = newTab;

            tabsToolStripMenuItem.DropDownItems.Add(tempTSP.tab.Text);
            tabsToolStripMenuItem.DropDownItems[tabsToolStripMenuItem.DropDownItems.Count - 1].ForeColor = ForeColor;
            tabsToolStripMenuItem.DropDownItems[tabsToolStripMenuItem.DropDownItems.Count - 1].BackColor = BackColor;
            tabsToolStripMenuItem.DropDownItems[tabsToolStripMenuItem.DropDownItems.Count - 1].Click += new EventHandler(handleTabsMenuClick);

            LoadCWP3RTB();
            var re = @"(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')|//.*|/\*(?s:.*?)\*/";
            string file = ASMDeclarations.GetImports(Regex.Replace(asmTabs[asmTabControl.SelectedIndex].ASMBox.Text, re, "$1") + "\n" + Regex.Replace(asmTabs[asmTabControl.SelectedIndex].codeDefBox.Text, re, "$1"), GlobalFileName);
            UpdateAutoCompleteBox(new string[] { Regex.Replace(file, re, "$1") });

            asmTabs[asmTabControl.SelectedIndex].codeDefBox.SelectionStart = 0;
            asmTabs[asmTabControl.SelectedIndex].codeDefBox.SelectionLength = 0;
            asmTabs[asmTabControl.SelectedIndex].ASMBox.SelectionStart = 0;
            asmTabs[asmTabControl.SelectedIndex].ASMBox.SelectionLength = 0;
        }

        private void LoadCWP3RTB()
        {
            asmTabs[asmTabControl.SelectedIndex].ASMBox.Text = "";
            asmTabs[asmTabControl.SelectedIndex].codeDefBox.Text = "// Declarations\n";

            string[] tempStr = FileIO.LoadCWP3(GlobalFileName);
            if (tempStr.Length >= 1)
            {
                asmTabs[asmTabControl.SelectedIndex].ASMBox.Text = System.IO.File.ReadAllText(tempStr[0]);
                asmTabs[asmTabControl.SelectedIndex].ASMBox.SaveToFile(tempStr[0], Encoding.UTF8);
            }
            if (tempStr.Length >= 2)
            {
                asmTabs[asmTabControl.SelectedIndex].codeDefBox.Text = System.IO.File.ReadAllText(tempStr[1]);
                asmTabs[asmTabControl.SelectedIndex].codeDefBox.SaveToFile(tempStr[0], Encoding.UTF8);
            }

            foreach (string str in tempStr)
                System.IO.File.Delete(str);

            asmTabs[asmTabControl.SelectedIndex].ASMBox.CollapseAllFoldingBlocks();
            asmTabs[asmTabControl.SelectedIndex].codeDefBox.CollapseAllFoldingBlocks();
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

                    asmTabs[asmTabControl.SelectedIndex].codeDefBox.SelectionStart = 0;
                    asmTabs[asmTabControl.SelectedIndex].codeDefBox.SelectionLength = 0;

                    asmTabs[asmTabControl.SelectedIndex].ASMBox.SelectionStart = 0;
                    asmTabs[asmTabControl.SelectedIndex].ASMBox.SelectionLength = 0;
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

        private void ASMBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.L && e.Control && e.Shift == false)
                HighlightEverything();
            else if (e.KeyCode == Keys.S && e.Control && e.Shift == false)
                (sender as FastColoredTextBox).Refresh();
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

        private void CodeBox_TextChanged(object sender, EventArgs e)
        {
            if (CodeBox.Lines.Length > (CodeBox.Height / CodeBox.Font.GetHeight()))
                CodeBox.ScrollBars = ScrollBars.Vertical;
            else
                CodeBox.ScrollBars = ScrollBars.None;
        }

        private void DisassembleASM_Click(object sender, EventArgs e)
        {
            if (asmTabs[asmTabControl.SelectedIndex].ASMBox.Text != "")
            {
                DialogResult a = MessageBox.Show(null, "If you continue, all the assembly will be erased!", "", MessageBoxButtons.OKCancel);
                if (DialogResult.Cancel == a)
                    return;
            }

            string dis = ASMDeclarations.ASMDisassemble(CodeBox.Text);
            //string disP = ASMDeclarations.DisAsmPseudoOps(dis, customPIns);
            asmTabs[asmTabControl.SelectedIndex].ASMBox.Text = dis;
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
            asmTabControl.TabPages.RemoveAt(asmTabControl.SelectedIndex);
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
            //HandleColorControls(a.Controls);
            a.Visible = true;
            a.Show();
        }

        private void cToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void simulateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string oldText = CodeBox.Text;
            int oldOutput = outputType;
            outputType = 0;
            //Compile code
            AssembleASM_Click(null, null);
            string[] hexArr = CodeBox.Text.Split('\r');
            ASMEmu.MemCode[] codesArg = new ASMEmu.MemCode[0];
            int cnt = 0;
            for (int x = 0; x < (hexArr.Length-1); x++)
            {
                string[] code = hexArr[x].Replace("\n", "").Split(' ');
                if (code.Length >= 3)
                {
                    for (int y = 0; y < code[2].Length; y += 8)
                    {
                        Array.Resize(ref codesArg, codesArg.Length + 1);
                        codesArg[cnt].addr = uint.Parse(code[1], System.Globalization.NumberStyles.HexNumber) + (uint)(y / 2);
                        codesArg[cnt].val = uint.Parse(sMid(code[2], y, 8), System.Globalization.NumberStyles.HexNumber);
                        cnt++;
                    }
                }
                else if (code.Length >= 1)
                {
                    for (int y = 0; y < code[0].Length; y += 8)
                    {
                        Array.Resize(ref codesArg, codesArg.Length + 1);
                        codesArg[cnt].addr = (uint)(y / 2);
                        codesArg[cnt].val = uint.Parse(sMid(code[0], y, 8), System.Globalization.NumberStyles.HexNumber);
                        cnt++;
                    }
                }
            }
            Array.Resize(ref codesArg, cnt);

            outputType = oldOutput;
            CodeBox.Text = oldText;

            ASMEmu a = new ASMEmu();
            //HandleColorControls(a.Controls);
            a.codes = codesArg;
            a.Visible = true;
            a.Show();
        }

        private void pinstructionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CustomPIns a = new CustomPIns();
            //HandleColorControls(a.Controls);
            a.Visible = true;
            a.Show();
        }

        private void asmTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (asmTabControl.SelectedIndex >= 0)
            {
                if (asmTabControl.SelectedIndex < asmTabs.Count)
                {
                    GlobalFileName = asmTabs[asmTabControl.SelectedIndex].GlobalFileName;
                }
                else
                {
                    GlobalFileName = asmTabs[asmTabs.Count - 1].GlobalFileName;
                }

                //if (asmTabs[asmTabControl.SelectedIndex].ASMBox.Text != "" && asmTabs[asmTabControl.SelectedIndex].codeDefBox.Text != "")
                //    UpdateLabels(new string[] { asmTabs[asmTabControl.SelectedIndex].ASMBox.Text, asmTabs[asmTabControl.SelectedIndex].codeDefBox.Text });
                Main_Resize(null, null);
            }
            tabCtrlSIndex = asmTabControl.SelectedIndex;
        }

        private void asmTabControl_TabClosing(object sender, TabControlCancelEventArgs e)
        {
            int ind = asmTabControl.SelectedIndex;
            if (asmTabControl.SelectedIndex == 0 && asmTabControl.TabPages.Count > 1)
                asmTabControl.SelectedIndex++;
            else if (asmTabControl.SelectedIndex > 0 && asmTabControl.TabPages.Count > 1)
                asmTabControl.SelectedIndex--;

            if (asmTabs[ind].ASMBox.IsChanged || asmTabs[ind].codeDefBox.IsChanged)
            {
                DialogResult dr = MessageBox.Show("\"" + asmTabs[ind].tab.Text + "\" hasn't been saved yet.\nWould you like to save it?", "Save " + asmTabs[ind].tab.Text + "?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    if (asmTabs[ind].GlobalFileName == "")
                        SaveAsCWP3_Click(null, null);
                    else
                        SaveCWP3_Click(null, null);
                }
                else if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            tabsToolStripMenuItem.DropDownItems.RemoveAt(ind);
            asmTabs.RemoveAt(ind);

            //None left
            if (asmTabControl.TabPages.Count == 1)
            {
                //makeNewTab("[new]");
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            string prevOpen = "\t" + asmTabControl.SelectedIndex.ToString() + "\n";

            foreach (tabStripPage tBP in asmTabs)
            {
                if (tBP.ASMBox.IsChanged || tBP.codeDefBox.IsChanged)
                {
                    DialogResult dr = MessageBox.Show("\"" + tBP.tab.Text + "\" hasn't been saved yet.\nWould you like to save it?", "Save " + tBP.tab.Text + "?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        GlobalFileName = tBP.GlobalFileName;
                        asmTabControl.SelectedTab = tBP.tab;
                        if (GlobalFileName == "")
                            SaveAsCWP3_Click(null, null);
                        else
                            SaveCWP3_Click(null, null);
                    }
                    else if (dr == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                prevOpen += tBP.GlobalFileName + "\n";
            }

            FileIO.SaveFile(Application.StartupPath + "\\oldtabs", prevOpen);
        }

        private void handleTabsMenuClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            int ind = (item.OwnerItem as ToolStripMenuItem).DropDownItems.IndexOf(item);
            if (ind < asmTabControl.TabPages.Count)
                asmTabControl.SelectedIndex = ind;
        }

        private void addTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            makeNewTab("[new]");
        }

        private void removeAllTabsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            asmTabControl.TabPages.Clear();
            asmTabs.Clear();
            makeNewTab("[new]");
            Main_Resize(null, null);
        }

        private void makeNewTab(string text)
        {
            tabStripPage tempTSP = new tabStripPage();
            tempTSP.ASMBox = InitializeFastTB(0);
            tempTSP.codeDefBox = InitializeFastTB(1);
            tempTSP.autoComBox = new AutocompleteMenu(tempTSP.ASMBox);
            tempTSP.GlobalFileName = "";
            tempTSP.tab = new TabPage(text);
            tempTSP.tab.BackColor = BackColor;
            tempTSP.tab.ForeColor = ForeColor;
            tempTSP.ASMBox.AutoIndent = true;
            tempTSP.codeDefBox.AutoIndent = true;

            tempTSP.tab.Controls.Add(tempTSP.ASMBox);
            tempTSP.tab.Controls.Add(tempTSP.codeDefBox);
            asmTabs.Add(tempTSP);
            asmTabControl.TabPages.Add(asmTabs[asmTabs.Count - 1].tab);
            asmTabControl.SelectedIndex = -1;
            asmTabControl.SelectedTab = asmTabs[asmTabs.Count - 1].tab;

            int ind = asmTabControl.SelectedIndex;
            if (ind >= asmTabs.Count || ind < 0)
                ind = asmTabs.Count - 1;

            UpdateAutoCompleteBox(new string[] { asmTabs[ind].ASMBox.Text, asmTabs[ind].codeDefBox.Text });

            tabsToolStripMenuItem.DropDownItems.Add(text);
            tabsToolStripMenuItem.DropDownItems[tabsToolStripMenuItem.DropDownItems.Count - 1].ForeColor = ForeColor;
            tabsToolStripMenuItem.DropDownItems[tabsToolStripMenuItem.DropDownItems.Count - 1].BackColor = BackColor;
            tabsToolStripMenuItem.DropDownItems[tabsToolStripMenuItem.DropDownItems.Count - 1].Click += new EventHandler(handleTabsMenuClick);
        }

        private void asmTabControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.N && e.Control && !e.Shift)
                makeNewTab("[new]");
        }

        private void donateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=werewu45%40yahoo%2ecom&lc=US&item_name=Dnawrkshp%27s%20effort&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted");
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RunUpdateChecker(true);
        }

        /* Brings up the Input Box with the arguments of a */
        public static IBArg[] CallIBox(IBArg[] a, Control ctrl)
        {
            InputBox ib = new InputBox();

            ib.Arg = a;
            ib.fmHeight = ctrl.Height;
            ib.fmWidth = ctrl.Width;
            ib.fmLeft = ctrl.Left;
            ib.fmTop = ctrl.Top;
            ib.TopMost = true;
            ib.fmForeColor = ctrl.ForeColor;
            ib.fmBackColor = ctrl.BackColor;
            ib.Show();

            while (ib.ret == 0)
            {
                a = ib.Arg;
                Application.DoEvents();
            }
            a = ib.Arg;

            if (ib.ret == 1)
                return a;
            else if (ib.ret == 2)
                return null;

            return null;
        }

    }
}
