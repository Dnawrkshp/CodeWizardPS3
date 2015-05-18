using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using FastColoredTextBoxNS;

namespace CodeWizardPS3
{
    class ASMDeclarations
    {

        /*
         * http://demono.ru/Utilities/onlinePpcDecoder.aspx
         */

        #region Declarations

        /* Error Output */
        static int currentLineNumber = 0;
        static bool isDeclaration = false;
        static string currentFileName = "";
        public static List<ErrorOutput> debugErrors = new List<ErrorOutput>();

        public struct ErrorOutput
        {
            public string fullPath;
            public string name;

            public int line;
            public int charIndex;
        };

        /* Instruction types */
        public const int typeNAN = 0;           /* GP - all registers */
        public const int typeBNC = 1;           /* Branch */
        public const int typeNOP = 2;           /* No parameters */
        public const int typeOFI = 3;           /* Offset immediate "0x1234(r3)" */
        public const int typeIMM = 4;           /* Immediate */
        public const int typeSPR = 5;           /* Special register */
        public const int typeBNCMP = 6;         /* Branch Compare */
        public const int typeCND = 7;           /* Instruction that uses a conditional register (not a branch) */
        public const int typeIMM5 = 8;          /* Immediate */
        public const int typeFNAN = 9;          /* Float - all registers */
        public const int typeFOFI = 10;         /* Float - Offset Immediate */
        public const int typeFCND = 11;         /* Float - Compare */

        /* Register color */
        public struct RegCol
        {
            public string reg;                  /* Name of register */
            public Brush col;                   /* Color / Brush of register */
            public string title;                /* Title string for the tooltip */
        }

        /* PowerPC Instruction struct */
        public struct PPCInstr
        {
            public string name;                 /* Name of instruction */
            public uint opCode;                 /* Instruction's defining bit sequence */
            public int[] opShift;               /* Left and right shifts to make the opCode 0 (Disassembly) */
            public int[] shifts;                /* Register bit shifts */
            public int[] order;                 /* Register order */
            public int type;                    /* Type of instruction */
            public string help;                 /* Help string for the InsBox */
            public string title;                /* Title string for the tooltip */
        };
            
        public static PPCInstr[] ASMDef = new PPCInstr[100];
        public static RegCol[] RegColArr = new RegCol[76];
        public static int[] InsPlaceArr = new int[26];
        public static string[] helpCom = new string[8];
        public static string[] helpReg = new string[14];
        public static string[] helpTerm = new string[78];

        /* Assembly */
        public struct ASMLabel
        {
            public uint address;           /* Address of label */
            public string name;            /* Name of label */
        }
        public static string debugString = "";
        public static uint CompAddress = 0;
        public static uint FirstCompAddr = 0;
        public static uint CompHook = 0;
        public static ASMLabel[] labels = null;
        public static int HookMode = 0;
        public static int ValState = 0;
            public static int vstHex = 0;
            public static int vstDec = 1;
            public static int vstLab = 2;
        public static string importDef;
        static List<Imports> importPaths = new List<Imports>();
        static int impOffset = 0;

        public struct Imports
        {
            public string importString;
            public string realPath;
        };

        /* Disassembly */
        static uint DisASMAddr = 0;
        static ASMLabel[] DisASMLabel = new ASMLabel[0];
        static bool addr = true;
        public static int ASMDisMode = 0;

        #endregion

        #region Remove Comments

        private static bool isInMCom = false, isInString = false;
        static string RemoveComments(string str)
        {
            int y = 0, x = 0;

            isInMCom = false;
            isInString = false;

            for (x = 0; x < (str.Length - 1); x++)
            {
                //Remove line
                if (str[x] == '/' && str[x + 1] == '/' && !isInString && !isInMCom)
                {
                    str = RemoveLine(x, str);
                }
                //Remove multiline
                else if (((str[x] == '/' && str[x + 1] == '*') || isInMCom) && !isInString)
                {
                    isInMCom = true;
                    str = RemoveLineOrEnd(x, str);
                }
                //String
                else if (str[x] == '\"')
                {
                    isInString = !isInString;
                }
            }

            return str;
        }

        static string RemoveLineOrEnd(int ind, string str)
        {
            int y = 0;
            for (y = ind; y < str.Length; y++)
            {
                if (str[y] == '\n' || str[y] == '\r')
                    break;
                else if (str[y] == '*' && y < (str.Length - 1) && str[y + 1] == '/')
                {
                    y += 2;
                    isInMCom = false;
                    break;
                }
            }

            return str.Remove(ind, (y - ind));
        }

        static string RemoveLine(int ind, string str)
        {
            int y = 0;
            for (y = ind; y < str.Length; y++)
            {
                if (str[y] == '\n' || str[y] == '\r')
                    break;
            }

            return str.Remove(ind, (y - ind));
        }

        #endregion

        #region Assembly and Disassembly

        /* Debug output */
        static void AddDebug(string str, int charIndex)
        {
            if (currentFileName != "")
            {
                string name = new FileInfo(currentFileName).Name.Replace(".cwp3", "") + (isDeclaration ? "'s Declaration" : "");
                string line = currentLineNumber.ToString() + ", " + charIndex.ToString();

                ErrorOutput eo = new ErrorOutput();
                eo.charIndex = charIndex;
                eo.name = name;
                eo.line = currentLineNumber - 1;
                eo.fullPath = currentFileName;
                debugErrors.Add(eo);

                debugString += "(" + name + ": " + line + ") " + str.Trim(Environment.NewLine.ToCharArray()) + Environment.NewLine;
            }
        }

        /* ---------- Assemble ---------- */
        public static string ASMAssemble(FastColoredTextBox ASMBox, FastColoredTextBox DefBox, int outputType, string GlobalFileName)
        {
            string CodeBoxRet = "";
            int importOff = 0;
            if (ASMBox.Text.IndexOf("\r\n") >= 0)
                importOff = 1;

            importPaths = new List<Imports>();
            debugErrors = new List<ErrorOutput>();
            importLoaded = new bool[0];
            impOffset = 0;
            CompAddress = 0;
            FirstCompAddr = 0;
            HookMode = 0;

            if (GlobalFileName != "")
                currentFileName = (new FileInfo(GlobalFileName)).FullName;
            else
                currentFileName = "[new]";
            if (DefBox != null)
                importDef = DefBox.Text + "\n";

            if (ASMBox.Text == "")
                return "";

            debugString = "";
            string fileStr = "";
            if (outputType == 2)
            {
                if (GlobalFileName == "")
                    CodeBoxRet = "byte[] NAME = { ";
                else
                {
                    //Get file name
                    int len = GlobalFileName.LastIndexOf('\\');
                    fileStr = Main.sRight(GlobalFileName, GlobalFileName.Length - len - 1);
                    //Remove the extension
                    len = fileStr.IndexOf('.');
                    fileStr = Main.sLeft(fileStr, len);
                    //Replace any spaces with '_'
                    fileStr = fileStr.Replace(" ", "_");
                    CodeBoxRet = "byte[] " + fileStr + " = { ";
                }
            }
            else
                CodeBoxRet = "";

            /* 
             * Remove comments
             * Reason I call it so often is because the process following needs to have it removed in order to work
             * Like labels can be defined in a comment and we don't want the parser to interpret that as a valid label
             */
            var re = @"(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')|//.*|/\*(?s:.*?)\*/";

            /* This is a lot faster than using Split */
            RichTextBox rtb = new RichTextBox();
            string rtbText = "";
            rtb.Text = RemoveComments(ASMBox.Text); //Regex.Replace(ASMBox.Text + Environment.NewLine, re, "$1");
            foreach (string line in rtb.Lines)
                rtbText += ParseASMText(line) + Environment.NewLine;
            rtb.Text = rtbText; 
            rtb.Find("a");

            /* Append any imported files */
            rtb.Text = GetImports(rtb.Text, GlobalFileName);
            rtb.Text = RemoveComments(rtb.Text); //Regex.Replace(rtb.Text, re, "$1");
            rtbText = "";
            foreach (string line in rtb.Lines)
                rtbText += ParseASMText(line) + Environment.NewLine;
            rtb.Text = rtbText;

            /* Parse any custom pseudo instructions */
            string rtbLower = rtb.Text.ToLower();
            bool cpiReset = false;
            for (int cpi = 0; cpi < Main.customPIns.Length; cpi++)
            {
                string postFix = " ";
                int cpiOff = rtbLower.IndexOf(Main.customPIns[cpi].name.ToLower() + postFix, 0);
                if (Main.customPIns[cpi].regs.Length <= 0 && cpiOff < 0)
                {
                    postFix = "\n";
                    cpiOff = rtbLower.IndexOf(Main.customPIns[cpi].name.ToLower() + postFix, 0);
                    if (cpiOff < 0)
                    {
                        postFix = "\r";
                        cpiOff = rtbLower.IndexOf(Main.customPIns[cpi].name.ToLower() + postFix, 0);
                    }
                }

                    //rtb.Find(Main.customPIns[cpi].name + " ", 0, RichTextBoxFinds.None);
                if (cpiOff != 0)
                {
                    cpiOff = rtbLower.IndexOf("\r" + Main.customPIns[cpi].name.ToLower() + postFix, 0);
                    if (cpiOff < 0)
                        cpiOff = rtbLower.IndexOf("\n" + Main.customPIns[cpi].name.ToLower() + postFix, 0);
                }
                        //rtb.Find("\r" + Main.customPIns[cpi].name + " ", 0, RichTextBoxFinds.None);

                while (cpiOff >= 0 || cpiReset)
                {
                    if (cpiReset)
                    {
                        cpi = 0;
                        cpiOff = rtbLower.IndexOf(Main.customPIns[cpi].name.ToLower() + " ", 0);
                            //rtb.Find(Main.customPIns[cpi].name + " ", 0, RichTextBoxFinds.None);
                        if (cpiOff != 0)
                            cpiOff = rtbLower.IndexOf(Main.customPIns[cpi].name.ToLower() + " ", 0);
                                //rtb.Find("\r" + Main.customPIns[cpi].name + " ", 0, RichTextBoxFinds.None);
                        if (cpiOff < 0)
                        {
                            cpiReset = false;
                            goto nextCPI;
                        }
                    }
                    cpiReset = true;

                    int rtbFind = rtb.Find("\r", cpiOff + 1, RichTextBoxFinds.None);
                    string[] line;
                    if (cpiOff != 0)
                        line = Main.sMid(rtb.Text, cpiOff + importOff, rtbFind - cpiOff).Split(' ');
                    else
                        line = Main.sMid(rtb.Text, cpiOff, rtbFind - cpiOff).Split(' ');
                    string[] args = new string[Main.customPIns[cpi].regs.Length];
                    if ((line.Length - 1) != args.Length && args.Length != 1)
                    {
                        //debugString += "Invalid number of arguments for \"" + rtb.Lines[rtb.GetLineFromCharIndex(cpiOff + cpiInc)] + "\"";
                        cpiReset = false;
                        goto skipCPI;
                    }

                    //if (args.Length != 0)
                    //{
                        for (int argsCnt = 0; argsCnt < args.Length; argsCnt++)
                            args[argsCnt] = line[argsCnt + 1].Replace(",", "").Replace("\n", "");

                        string newline = Main.customPIns[cpi].asm;
                        for (int nlCnt = 0; nlCnt < Main.customPIns[cpi].regs.Length; nlCnt++)
                            newline = newline.Replace(Main.customPIns[cpi].regs[nlCnt], args[nlCnt]);
                        
                        if (cpiOff == 0)
                            rtb.Text = rtb.Text.Replace(Main.sMid(rtb.Text, cpiOff, rtbFind - cpiOff), newline + "\n");
                        else
                            rtb.Text = rtb.Text.Replace(Main.sMid(rtb.Text, cpiOff + importOff, rtbFind - cpiOff), newline + "\n");
                    //}

                skipCPI: ;
                    cpiOff = (rtbLower = rtb.Text.ToLower()).IndexOf("\r" + Main.customPIns[cpi].name.ToLower() + " ", cpiOff + 1);
                        //rtb.Find("\r" + Main.customPIns[cpi].name + " ", cpiOff + 1, RichTextBoxFinds.None);
                }
            nextCPI: ;
            }
            //Remove imported comments...
            rtb.Text = RemoveComments(rtb.Text); //Regex.Replace(rtb.Text, re, "$1");

            //Add all imports
            rtb.Text += "\n/*\n" + importDef + "\n*/\n";

            labels = ProcessLabels(rtb.Lines);
            if (labels == null)
            {
                //Main.DebugMenu(debugString);
                return "";
            }

            //Remove importDef
            rtb.Text = RemoveComments(rtb.Text); //Regex.Replace(rtb.Text, re, "$1");
            string[] ASMArray = rtb.Lines;
            string retStr = "";
            currentLineNumber = 0;

            isDeclaration = false;
            for (int cnt = 0; cnt < ASMArray.Length; cnt++)
            {
                /* Skip blanks */
                currentLineNumber++;
                while (cnt < ASMArray.Length && ASMArray[cnt] == "")
                {
                    cnt++;
                    currentLineNumber++;
                }
                if (cnt >= ASMArray.Length)
                    break;

                if (ASMArray[cnt].IndexOf("import") >= 0)
                {
                    string impPath = ASMArray[cnt].Replace("import ", "");
                    Imports i = importPaths.Where(ip => ip.importString == impPath).FirstOrDefault();
                    if (i.realPath != null)
                    {
                        currentFileName = i.realPath;
                        currentLineNumber = -2;
                    }
                }

                retStr = ASMToHex(ParseASMText(ASMArray[cnt]));
                if (retStr != null && retStr != "")
                {
                    string outStr = "";
                    string[] outCode;
                    switch (outputType)
                    {
                        case 0: //NetCheat PS3
                            CodeBoxRet += retStr;
                            break;
                        case 1: //Hex String Array
                            outCode = retStr.Split(' ');
                            if (outCode[0].Length <= 1)
                                break;
                            for (int x = 0; x < outCode.Length; x += 2)
                            {
                                outStr = Main.sLeft(outCode[x], 8);
                                if (outStr.Length == 8)
                                {
                                    outStr = outStr.Insert(2, " ");
                                    outStr = outStr.Insert(5, " ");
                                    outStr = outStr.Insert(8, " ");
                                    outStr = outStr.Insert(11, " ");

                                    CodeBoxRet += outStr;
                                }
                            }
                            break;
                        case 2: //Byte Array
                            outCode = retStr.Split(' ');
                            if (outCode.Length > 0 && outCode[0].Length <= 3)
                                break;

                            for (int x = 0; x < outCode[0].Length; x += 8)
                            {
                                outStr = Main.sMid(outCode[0], x, 8);
                                outStr = Main.sLeft(outStr, 8);
                                outStr = outStr.Insert(2, ", 0x");
                                outStr = outStr.Insert(8, ", 0x");
                                outStr = outStr.Insert(14, ", 0x");
                                outStr = outStr.Insert(20, ", ");

                                CodeBoxRet += "0x" + outStr;
                            }
                            break;
                    }
                    CompAddress += 4;
                }
            }

            if (outputType == 2)
            {
                CodeBoxRet = CodeBoxRet.Remove(CodeBoxRet.Length - 2);
                CodeBoxRet += " };";
            }

            if (importDef != "" && importDef != "\n" && importDef != "\r" && importDef != "\r\n")
            {
                currentLineNumber = 0;
                isDeclaration = true;
                currentFileName = new FileInfo(GlobalFileName).FullName;
                int defParseCnt = 0;
                bool addLines = false;
                string[] lines = RemoveComments(importDef).Split('\n'); //Regex.Replace(importDef, re, "$1").Split('\n');
                foreach (string line in lines)
                {
                    string hex = null;

                    currentLineNumber++;
                    if (line.IndexOf("--!--") >= 0)
                    {
                        currentFileName = line.Replace("--!--", "").Replace("\r", "").Trim(' ');
                        currentLineNumber = 0;
                    }
                    else if (line != "")
                        hex = ASMToHex(line);
                    if (hex != null && hex != "")
                    {
                        if (!addLines && (hex[1] == ' ' || outputType != 0))
                        {
                            CodeBoxRet += Environment.NewLine + Environment.NewLine;
                            addLines = true;
                        }

                        switch (outputType)
                        {
                            case 0: //NetCheat
                                CodeBoxRet += hex;
                                break;
                            case 1: //Hex String Array
                                //CodeBoxRet += Environment.NewLine;
                                hex = Main.sRight(hex, 8);
                                if (hex.Length == 8)
                                    CodeBoxRet +=   hex[0].ToString() + hex[1].ToString() + " " +
                                                    hex[2].ToString() + hex[3].ToString() + " " +
                                                    hex[4].ToString() + hex[5].ToString() + " " +
                                                    hex[6].ToString() + hex[7].ToString() + " ";
                                break;
                            case 2: //Byte Array
                                hex = Main.sRight(hex, 8);
                                if (hex.IndexOf(' ') < 0)
                                {
                                    if (defParseCnt == 0)
                                        CodeBoxRet += "byte[] " + fileStr + "_decl = { 0x";
                                    else
                                        CodeBoxRet += ", 0x";
                                    CodeBoxRet += hex[0].ToString() + hex[1].ToString() + ", 0x" +
                                                    hex[2].ToString() + hex[3].ToString() + ", 0x" +
                                                    hex[4].ToString() + hex[5].ToString() + ", 0x" +
                                                    hex[6].ToString() + hex[7].ToString();
                                    defParseCnt++;
                                }
                                break;
                        }
                        CompAddress += 4;
                    }
                }
                if (outputType == 2)
                    CodeBoxRet += " };";
            }

            if (HookMode != 0)
            {
                string hook = "b";
                if (HookMode == 1)
                    hook = "bl";

                CompAddress = CompHook;
                string hex = Main.sRight(ASMToHex(hook + " 0x" + (FirstCompAddr - 4).ToString("X8")), 8);
                //CodeBoxRet += Environment.NewLine;
                switch (outputType)
                {
                    case 0: //NetCheat
                        CodeBoxRet += Environment.NewLine + "0 " + CompHook.ToString("X8") + " " + hex;
                        break;
                    case 1: //Hex String Array
                        CodeBoxRet += Environment.NewLine + Environment.NewLine + 
                                        hex[0] + hex[1] + " " +
                                        hex[2] + hex[3] + " " +
                                        hex[4] + hex[5] + " " +
                                        hex[6] + hex[7] + " ";
                        break;
                    case 2: //Byte Array
                        CodeBoxRet += Environment.NewLine + "byte[] " + fileStr + "_hook = { 0x" +
                                        hex[0].ToString() + hex[1].ToString() + ", 0x" +
                                        hex[2].ToString() + hex[3].ToString() + ", 0x" +
                                        hex[4].ToString() + hex[5].ToString() + ", 0x" +
                                        hex[6].ToString() + hex[7].ToString() + " };";
                        break;
                }
                CodeBoxRet += Environment.NewLine;
            }

            debugString = debugString.Trim(Environment.NewLine.ToCharArray());
            return CodeBoxRet.Trim(Environment.NewLine.ToCharArray()) + Environment.NewLine;
        }

        /* Gets the text from all the imported cwp3 files */
        public static string GetImports(string rtb, string filePath)
        {
            importPaths = new List<Imports>();
            TextBox argTB = new TextBox();
            argTB.Text = rtb;

            if (filePath == "")
                return rtb;
            TextBox[] retRTBArr = ReadImport(argTB, new FileInfo(filePath).Directory.FullName);
            bool contLoop = true;
            while (contLoop)
            {
                int skip = 0;
                contLoop = false;
                impOffset = retRTBArr.Length;
                retRTBArr = ParseImports(retRTBArr, skip);
                for (int x = impOffset; x < retRTBArr.Length; x++)
                    //if (retRTBArr[x].Find("import", 0, RichTextBoxFinds.WholeWord) >= 0)
                    //    contLoop = true;
                    if (Regex.IsMatch(retRTBArr[x].Text, "import"))
                        contLoop = true;
                skip = impOffset + 1;
            }
            
            //Append all the imported files together
            for (int x = 0; x < retRTBArr.Length; x++)
            {
                argTB.Text += "\n" + retRTBArr[x].Text;
            }

            argTB.Text += "\n";
            return argTB.Text;
        }

        /* Parses each import in a RichTextBox and returns the results in a RichTextBox array */
        public static TextBox[] ParseImports(TextBox[] rtb, int skip)
        {
            TextBox[] tempRTBArr = (TextBox[])rtb.Clone();
            for (int z = skip; z < rtb.Length; z++)
            {
                int offVal = 0;
                while (offVal >= 0)
                {
                    //offVal = rtb[z].Find("import", offVal + 1, RichTextBoxFinds.WholeWord);
                    //offVal = Regex.Match(rtb[z].Text, @"\bimport\b").Index;
                    offVal = rtb[z].Text.IndexOf("import ", offVal + 1);
                    if (offVal >= 0)
                    {
                        int off = tempRTBArr.Length;
                        TextBox[] tempRTB = ReadImport(rtb[z], (string)rtb[z].Tag);
                        if (tempRTB != null && tempRTB.Length != 0)
                        {
                            Array.Resize(ref tempRTBArr, tempRTBArr.Length + tempRTB.Length);
                            Array.Copy(tempRTB, 0, tempRTBArr, tempRTBArr.Length - tempRTB.Length, tempRTB.Length);
                        }
                    }
                }
            }
            return tempRTBArr;
        }

        /* Gets the text from a specific cwp3 file */
        static bool[] importLoaded = new bool[0];
        public static TextBox[] ReadImport(TextBox rtb, string GlobalFileName)
        {
            string lowerText = rtb.Text.ToLower();
            TextBox[] retRTB = new TextBox[0];

            if (GlobalFileName == null)
                return null;

            //Calculate each import
            int offImport = lowerText.IndexOf("import");

            while (offImport >= 0)
            {
                if (offImport >= 0 && offImport < rtb.Text.Length)
                {
                    //int rtbFind = rtb.Find("\r", offImport, RichTextBoxFinds.None);
                    int rtbFind = lowerText.IndexOf("\r", offImport);
                    if (rtbFind < 0)
                        rtbFind = lowerText.IndexOf("\n", offImport);
                    if (rtbFind < 0)
                        rtbFind = rtb.Text.Length;

                    string impStr = "";
                    string[] tempimpStr = Main.sMid(rtb.Text, offImport, rtbFind - offImport).Split(' ');
                    for (int x = 1; x < tempimpStr.Length; x++)
                        impStr += tempimpStr[x] + " ";
                    impStr = impStr.Trim(' ');
                    if (impStr.StartsWith("Map"))
                    {

                    }
                    string newImpStr = "";
                    if (impStr.IndexOf(':') < 0)
                        newImpStr = GlobalFileName + "\\" + impStr;
                    newImpStr = new FileInfo(newImpStr).FullName;
                    if (!isLoaded(newImpStr, importPaths.Count))
                    {
                        Imports i = new Imports();
                        i.importString = impStr;
                        i.realPath = newImpStr;
                        importPaths.Add(i);
                    }

                    offImport = lowerText.IndexOf("import", offImport + 1);
                }
            }

            //TextBox rtfToPlain = new TextBox();

            //Import each file
            for (int x = impOffset; x < importPaths.Count; x++)
            {
                //importStr = importStr.Split(' ')[1].Replace("\"", "");
                //Parse file string
                if (importPaths[x].realPath.IndexOf(':') >= 0 && !isLoaded(importPaths[x].realPath, x)) //File is whole path
                {
                    if (System.IO.File.Exists(importPaths[x].realPath))
                    {
                        Array.Resize(ref retRTB, retRTB.Length + 1);
                        retRTB[retRTB.Length - 1] = new TextBox();
                        string[] arr = FileIO.LoadCWP3(importPaths[x].realPath);
                        if (arr.Length >= 1)
                            retRTB[retRTB.Length - 1].Text = FileIO.OpenFile(arr[0]);
                        if (arr.Length >= 2)
                        {
                            //rtfToPlain.LoadFile(arr[1]);
                            importDef += "--!-- " + new FileInfo(importPaths[x].realPath).FullName + " --!--\n" + FileIO.OpenFile(arr[1]) + "\n";
                        }
                        importLoaded[x] = true;
                        retRTB[retRTB.Length - 1].Tag = new FileInfo(importPaths[x].realPath).Directory.FullName;
                    }
                    else
                    {
                        string deb = "Imported file " + importPaths[x].realPath + " doesn't exist!" + Environment.NewLine;
                        AddDebug(deb, 0);
                    }
                }
                else if (GlobalFileName != "" && !isLoaded(importPaths[x].realPath, x)) //File is appended to GlobalFileName
                {
                    string impStr = new FileInfo(GlobalFileName + "\\" + importPaths[x]).FullName;
                    if (System.IO.File.Exists(impStr) && !isLoaded(impStr, x))
                    {
                        Array.Resize(ref retRTB, retRTB.Length + 1);
                        retRTB[retRTB.Length - 1] = new TextBox();
                        string[] arr = FileIO.LoadCWP3(impStr);
                        if (arr.Length >= 1)
                            retRTB[retRTB.Length - 1].Text = FileIO.OpenFile(arr[0]);
                        if (arr.Length >= 2)
                        {
                            importDef += FileIO.OpenFile(arr[1]) + "\n";
                        }
                        importLoaded[x] = true;
                        retRTB[retRTB.Length - 1].Tag = new FileInfo(impStr).Directory.FullName;
                    }
                    else if (System.IO.File.Exists(impStr))
                    {
                        string deb = "Imported file " + impStr + " doesn't exist!" + Environment.NewLine;
                        AddDebug(deb, 0);
                    }
                }
                else if (GlobalFileName == "")
                {
                    AddDebug("You must save the file before importing other files!" + Environment.NewLine, 0);
                }
            }

            return retRTB;
        }

        /* Tells whether the imported string has been loaded yet */
        public static bool isLoaded(string file, int ind)
        {
            if (importPaths.Count <= 0)
                return false;

            Array.Resize(ref importLoaded, importPaths.Count + 1);
            if (importLoaded[ind])
                return true;

            ind--;
            for (int x = ind; x >= 0; x--)
            {
                if (new FileInfo(importPaths[x].realPath).FullName == new FileInfo(file).FullName)
                    return true;
            }

            return false;
        }

        /* Assembles ASM and returns hex string */
        public static string ASMToHex(string argASM)
        {

            string[] asmSplit = argASM.Trim().Split(' ');
            uint retVal = 0;

            if (asmSplit[0] == "" || asmSplit[0] == null || argASM.StartsWith("#"))
                return null;

            if (asmSplit[0].ToLower() == "address")
            {
                if (CompAddress == 0)
                {
                    FirstCompAddr = (uint)ParseVal(asmSplit[1], 1);
                    CompAddress = FirstCompAddr;
                }
                else
                {
                    CompAddress = (uint)ParseVal(asmSplit[1], 1);
                }
                return Environment.NewLine + "0 " + CompAddress.ToString("X8") + " ";
            }
            else if (asmSplit[0].ToLower() == "hook")
            {
                CompHook = (uint)ParseVal(asmSplit[1], 1);
                HookMode = 2;
                return null;
            }
            else if (asmSplit[0].ToLower() == "hookl")
            {
                CompHook = (uint)ParseVal(asmSplit[1], 1);
                HookMode = 1;
                return null;
            }
            else if (asmSplit[0].ToLower() == "setreg")
            {
                //Invalid arguments
                if (asmSplit.Length != 3)
                {
                    AddDebug("Too few arguments for setreg: \"" + argASM + "\"", 7);
                    return null;
                }

                //Parse in case of label
                long val = ParseVal(asmSplit[2], 1);
                string upper = Main.sLeft(val.ToString("X8"), 4);
                string lower = Main.sRight(val.ToString("X8"), 4);

                string argPass = "lis " + asmSplit[1] + " 0x" + upper;
                string retStr = ASMToHex(argPass) + ASMToHex("ori " + asmSplit[1] + " " + asmSplit[1] + " 0x" + lower);
                CompAddress += 4;
                return retStr;
            }
            else if (asmSplit[0].ToLower() == "hexcode")
            {
                return ParseVal(asmSplit[1], 1).ToString("X8");
            }
            else if (asmSplit[0].ToLower() == "import")
                return null;
            else if (asmSplit[0].ToLower() == "float")
            {
                byte[] flt = BitConverter.GetBytes(float.Parse(asmSplit[1]));
                return BitConverter.ToUInt32(flt, 0).ToString("X8");
            }
            else if (asmSplit[0].ToLower() == "string")
            {
                string fullStr = asmSplit[1];
                for (int fS = 2; fS < asmSplit.Length; fS++)
                    fullStr += " " + asmSplit[fS];
                byte[] flt = Main.StringToByteArray(fullStr);


                string retStr = "";
                foreach (byte b in flt)
                    retStr += b.ToString("X2");
                retStr = retStr.PadRight(retStr.Length + ((4 - (retStr.Length % 4)) * 2), '0');
                CompAddress -= (uint)(retStr.Length / 2);
                return retStr;
            }


            for (int x = ASMDeclarations.GetInsStart(asmSplit[0][0]); x < ASMDeclarations.ASMDef.Length; x++)
            {
                if (asmSplit[0] == ASMDeclarations.ASMDef[x].name && IsCorrectSize(ASMDeclarations.ASMDef[x].order, asmSplit.Length - 1, ASMDeclarations.ASMDef[x].type))
                {
                    if (ASMDeclarations.ASMDef[x].order == null)
                        return ASMDeclarations.ASMDef[x].opCode.ToString("X8");

                    int[] regs = ParseRegs(asmSplit, ASMDeclarations.ASMDef[x].order.Length);
                    if (regs == null)
                    {
                        AddDebug("Error: Missing argument(s) in \"" + argASM + "\"", ASMDeclarations.ASMDef[x].name.Length + 1);
                        return null;
                    }

                    int y = 0;
                    retVal = ASMDeclarations.ASMDef[x].opCode;
                    switch (ASMDeclarations.ASMDef[x].type)
                    {
                        case ASMDeclarations.typeNAN:
                        case ASMDeclarations.typeFNAN:
                        case ASMDeclarations.typeSPR:
                            for (y = 0; y < ASMDeclarations.ASMDef[x].shifts.Length; y++)
                                retVal |= (uint)(regs[ASMDeclarations.ASMDef[x].order[y]] << ASMDeclarations.ASMDef[x].shifts[y]);
                            break;
                        case ASMDeclarations.typeBNC:
                            for (y = 0; y < (ASMDeclarations.ASMDef[x].shifts.Length - 1); y++)
                                retVal |= (uint)(regs[ASMDeclarations.ASMDef[x].order[y]] << ASMDeclarations.ASMDef[x].shifts[y]);
                            int offset = (int)((regs[ASMDeclarations.ASMDef[x].order[y]] - CompAddress + 4) / 4) << ASMDeclarations.ASMDef[x].shifts[y];
                            retVal |= (uint)(((UInt32)offset << 6) >> 6);
                            break;
                        case ASMDeclarations.typeOFI:
                        case ASMDeclarations.typeFOFI:
                        case ASMDeclarations.typeCND:
                        case ASMDeclarations.typeFCND:
                        case ASMDeclarations.typeIMM:
                            for (y = 0; y < (ASMDeclarations.ASMDef[x].shifts.Length - 1); y++)
                                retVal |= (uint)(regs[ASMDeclarations.ASMDef[x].order[y]] << ASMDeclarations.ASMDef[x].shifts[y]);

                            retVal |= (UInt16)(regs[ASMDeclarations.ASMDef[x].order[y]] << ASMDeclarations.ASMDef[x].shifts[y]);
                            break;
                        case ASMDeclarations.typeBNCMP:
                            for (y = 0; y < (ASMDeclarations.ASMDef[x].shifts.Length - 1); y++)
                                retVal |= (uint)(regs[ASMDeclarations.ASMDef[x].order[y]] << ASMDeclarations.ASMDef[x].shifts[y]);

                            uint subVal = (ValState == vstLab) ? (CompAddress-4) : 0;
                            int cmpOff = (int)((regs[ASMDeclarations.ASMDef[x].order[y]] - subVal) / 4) << ASMDeclarations.ASMDef[x].shifts[y];
                            retVal |= (UInt16)cmpOff;
                            break;
                        case ASMDeclarations.typeIMM5:
                            if (ASMDeclarations.ASMDef[x].name.ToLower() == "srdi" && regs[ASMDeclarations.ASMDef[x].order[2]] < 32)
                            {
                                AddDebug("srdi cannot have a shift less than 32!" + Environment.NewLine, 0);
                            }
                            retVal |= AssembleRotate(ASMDeclarations.ASMDef[x].name, regs, x, retVal);
                            break;
                    }
                    break;
                }
            }
            if (retVal == 0)
            {
                if (asmSplit[0].ToLower()[asmSplit[0].Length - 1] != ':')
                {
                    AddDebug("\"" + argASM + "\" is either missing argument(s) or is not valid", 0);
                }
                return null;
            }

            return retVal.ToString("X8");
        }

        /* For all shift instructions, assembles them */
        public static uint AssembleRotate(string op, int[] regs, int asmDefIndex, uint retVal)
        {
            int y = 0;

            for (y = 0; y < (ASMDeclarations.ASMDef[asmDefIndex].shifts.Length - 1); y++)
                retVal |= (uint)(regs[ASMDeclarations.ASMDef[asmDefIndex].order[y]] << ASMDeclarations.ASMDef[asmDefIndex].shifts[y]);
            retVal |= (UInt16)(regs[ASMDeclarations.ASMDef[asmDefIndex].order[y]] << ASMDeclarations.ASMDef[asmDefIndex].shifts[y]);

            int regv = regs[ASMDeclarations.ASMDef[asmDefIndex].order[y]];

            switch (op)
            {
                case "slwi":
                    retVal |= (UInt16)((31 - regs[ASMDeclarations.ASMDef[asmDefIndex].order[y]]) << 1);
                    break;
                case "srwi":
                    retVal |= (UInt16)((32 - regs[ASMDeclarations.ASMDef[asmDefIndex].order[y]]) << 11);
                    retVal |= 31 << 1;
                    break;
                case "sldi":
                    //retVal |= (UInt16)((60 - regv) << 6);

                    retVal = AssembleRLDICR((ushort)regs[ASMDeclarations.ASMDef[asmDefIndex].order[0]], (ushort)regs[ASMDeclarations.ASMDef[asmDefIndex].order[1]], (uint)regs[ASMDeclarations.ASMDef[asmDefIndex].order[2]]);
                    break;
                case "srdi":
                    //retVal |= (UInt16)((60 - regv) << 6);
                    //retVal |= 63 << 1;
                    retVal = AssembleRLDICL((ushort)regs[ASMDeclarations.ASMDef[asmDefIndex].order[0]], (ushort)regs[ASMDeclarations.ASMDef[asmDefIndex].order[1]], (uint)regs[ASMDeclarations.ASMDef[asmDefIndex].order[2]]);
                    break;
            }

            return retVal;
        }

        static uint AssembleRLDICR(UInt16 rA, UInt16 rS, uint n)
        {
            uint ret = 0x78000004;

            ret |= (uint)rS << 16;
            ret |= (uint)rA << 21;

            uint newN = 31 - n;
            if (n > 31)
            {
                ret |= 2;
                newN = 63 - n;
            }
            else
            {
                ret |= 1 << 5;
            }

            ret |= (ushort)(newN << 6);
            ret |= (ushort)(n << 11);

            return ret;
        }

        static uint AssembleRLDICL(UInt16 rA, UInt16 rS, uint n)
        {
            uint ret = 0x78000000;

            //if (n < 32)
            //    debugString += "srdi cannot have a shift less than 32!" + Environment.NewLine;

            ret |= (uint)rS << 16;
            ret |= (uint)rA << 21;

            uint newN = n - 32;
            if (n > 32)
            {
                n = 64 - n;
                ret |= (ushort)(newN << 6);
                ret |= (ushort)(n << 11);
                ret |= 0x20;
            }
            else
            {
                ret |= (ushort)n;
                ret |= 2;
            }

            return ret;
        }

        /* Checks if the instruction is the correct size */
        public static bool IsCorrectSize(int[] order, int regNum, int type)
        {
            if (order == null && regNum == 0)
                return true;
            else if (order == null)
                return false;

            switch (type)
            {
                case ASMDeclarations.typeOFI:
                case ASMDeclarations.typeFOFI:
                    if ((regNum + 1) == order.Length)
                        return true;
                    break;
                default:
                    if (regNum == order.Length)
                        return true;
                    break;
            }
            return false;
        }

        /* 
         * Parses the assembly line into something that can be assembled
         * Pretty much removes excess spaces and tabs
         */
        static string ParseASMText(string line)
        {
            string newLine = line.Trim(new char[] { ' ', '\t' });
            string[] subStrs = newLine.Replace('\t', ' ').Split(' ');

            string ret = "";
            foreach (string str in subStrs)
                if (str != "")
                    ret += str + " ";

            return ret.Trim(' ');
        }

        /* Parses string into value */
        public static long ParseVal(string str, int mode)
        {
            bool neg = false;
            ValState = 0;

            if (str == "" || str == null)
                return 0;
            uint ret = 0;

            str = str.Replace(",", "");

            ASMLabel label = isLabel(str);
            if (label.name != null) //Label
            {
                ValState = vstLab;
                return label.address;
            }

            string oldStr = str;
            str = str.ToLower();
            if (str.IndexOf('x') >= 0 || str.IndexOf('$') >= 0)
            {
                ValState = vstHex;
                str = str.Replace("0x", "");
                str = str.Replace("$", "");
                neg = str.StartsWith("-");
                if (neg)
                    str = str.Replace("-", "");

                try
                {
                    if (mode == 0)
                        ret = (UInt16)long.Parse(str, System.Globalization.NumberStyles.HexNumber);
                    else
                        ret = (UInt32)long.Parse(str, System.Globalization.NumberStyles.HexNumber);
                    if (neg)
                        return -ret;
                    return (long)ret;
                }
                catch
                {
                    //MessageBox.Show("Error: Hexadecimal value: 0x" + str.ToUpper() + " is not a valid hexadecimal value");
                    //debugString += Environment.NewLine + "Error: Hexadecimal value: 0x" + str.ToUpper() + " is not a valid hexadecimal value";
                    AddDebug("Error: Hexadecimal value: 0x" + str.ToUpper() + " is not a valid hexadecimal value", 0);
                    
                    return 0;
                }
            }
            else
            {
                ValState = vstDec;
                try
                {
                    neg = str.StartsWith("-");
                    if (neg)
                        str = str.Replace("-", "");

                    if (mode == 0)
                        ret = (UInt16)long.Parse(str);
                    else
                        ret = (UInt32)long.Parse(str);

                    if (neg)
                        return -ret;
                    return (long)ret;
                }
                catch
                {
                    //MessageBox.Show("Error: Decimal value: " + str.ToUpper() + " is not a valid decimal value");
                    //debugString += Environment.NewLine + "Error: Label " + oldStr + " hasn't been declared (or invalid decimal value)";
                    AddDebug("Error: Label " + oldStr + " hasn't been declared (or invalid decimal value)", 0);
                    return 0;
                }
            }
        }

        /* Parses registers from ASM line */
        public static int ParseRegsImm;
        public static int[] ParseRegs(string[] str, int size)
        {

            if (str.Length < 2)
                return null;

            int[] retArray = new int[size];

            for (int x = 0; x < (str.Length - 1); x++)
            {
                string regStr = str[x + 1].Replace(",", "");
                string regStrLower = regStr.ToLower();
                //Label
                if (isLabel(regStr).name != null)
                {
                    retArray[x] = (int)ParseVal(str[x + 1], 1);
                }
                //Immediate Offset
                else if (regStr.IndexOf('(') >= 0)
                {
                    retArray[x] = ParseOffImm(regStrLower);
                    retArray[x + 1] = ParseRegsImm;
                }
                //Special register
                else if (regStrLower.IndexOf("xer") >= 0 ||
                    regStrLower.IndexOf("lr") >= 0 ||
                    regStrLower.IndexOf("ctr") >= 0)
                    retArray[x] = SPRegToDec(regStrLower);
                //Register
                else if (regStrLower.IndexOf('r') >= 0 || (regStrLower.IndexOf('f') == 0 && regStrLower.Length <= 3))
                    retArray[x] = RegToDec(regStrLower);
                //Immediate
                else
                    retArray[x] = (int)ParseVal(regStr, 1);
            }
            return retArray;
        }

        /* Parses the immediate and register */
        public static int ParseOffImm(string str)
        {
            string[] strSplit = str.Split('(');
            ParseRegsImm = (int)ParseVal(strSplit[0], 0);

            string ret = strSplit[1].Replace(")", "");
            return RegToDec(ret);
        }

        /* Converts the string representation of the register to the decimal equivalent */
        public static int RegToDec(string reg)
        {
            string oldReg = reg;

            if (reg[0] == ':') //Label
                return 0;

            reg = reg.ToLower();
            reg = reg.Replace("c", "");
            reg = reg.Replace("r", "");
            reg = reg.Replace(",", "");
            reg = reg.Replace("%", "");
            reg = reg.Replace("f", "");
            try
            {
                int ret = int.Parse(reg.Replace("$", ""));
                if (ret < 0 || ret > 31)
                {
                    //debugString += "Register " + oldReg + " is not a valid register!";
                    AddDebug("Register " + oldReg + " is not a valid register!", 0);
                    return 0;
                }
                return ret;
            }
            catch
            {
                //debugString += "Register " + oldReg + " is not a valid register!";
                AddDebug("Register " + oldReg + " is not a valid register!", 0);
                return 0;
            }
        }

        public static int SPRegToDec(string reg)
        {
            switch (reg.ToLower())
            {
                case "xer":
                    return 1;
                case "lr":
                    return 8;
                case "ctr":
                    return 9;
            }
            return 0;
        }

        public static ASMLabel[] ProcessLabels(string[] strArray)
        {
            int x = 0;
            uint tempAddr = 0;

            ASMLabel[] ret = new ASMLabel[0];
            for (x = 0; x < strArray.Length; x++)
            {
                string[] strSplit = ParseASMText(strArray[x]).Split(' ');
                int index = strArray[x].IndexOf(':');

                //Address
                if (strSplit[0].ToLower() == "address")
                    tempAddr = (uint)ParseVal(strSplit[1], 1);
                else if (strSplit[0].ToLower() == "hexcode")
                    tempAddr += 4;
                else if (strSplit[0].ToLower() == "setreg")
                    tempAddr += 8;
                else if (strSplit[0].ToLower() == "float")
                    tempAddr += 4;
                else if (strSplit[0].ToLower() == "string")
                {
                    string fullStr = strSplit[1];
                    for (int fS = 2; fS < strSplit.Length; fS++)
                        fullStr += " " + strSplit[fS];

                    int strAdd = 4 - (fullStr.Length % 4);
                    if (strAdd != 4)
                        tempAddr += (uint)(strAdd + fullStr.Length);
                    else
                        tempAddr += (uint)fullStr.Length;
                }
                //Label
                else if (index > 0 && strArray[x][index - 1] != ' ' && strArray[x][index - 1] != ',' && strArray[x].IndexOf("--!--") < 0)
                {
                    //string[] splitStr = strArray[x].Split(' ');
                    if (strSplit.Length > 1 && strSplit[0].IndexOf(":") < 0)
                    {
                        //debugString += Environment.NewLine + "Error: Label at (" + strArray[x] + ") accidently declared instead of called";
                        AddDebug("Error: Label at (" + strArray[x] + ") accidently declared instead of called", 0);
                        return null;
                    }
                    Array.Resize(ref ret, ret.Length + 1);
                    ret[ret.Length - 1].address = tempAddr;
                    ret[ret.Length - 1].name = strSplit[0].Replace(":", "");

                    //Check if label already exists
                    for (int y = 0; y < ret.Length; y++)
                    {
                        if (ret[y].name == ret[ret.Length - 1].name && y != (ret.Length - 1))
                        {
                            //MessageBox.Show("Error: Label (" + ret[ret.Length - 1].name + ") duplication");
                            //debugString += Environment.NewLine + "Error: Label (" + ret[ret.Length - 1].name + ") duplication";
                            AddDebug("Error: Label (" + ret[ret.Length - 1].name + ") duplication", 0);
                            return null;
                        }
                    }
                }
                //Instruction
                else if (IsInstruction(strSplit[0]))
                    tempAddr += 4;
            }
            return ret;
        }

        /* Checks if string is a label */
        public static ASMLabel isLabel(string label)
        {
            if (labels == null)
                return new ASMLabel();

            label = label.Replace(":", "").ToLower();
            foreach (ASMLabel lab in labels)
                if (label == lab.name.ToLower())
                    return lab;

            return new ASMLabel();
        }

        /* Checks if string is an instruction */
        public static bool IsInstruction(string asm)
        {
            if (asm == "")
                return false;
            for (int x = ASMDeclarations.GetInsStart((int)asm[0]); x < ASMDeclarations.ASMDef.Length; x++)
            {
                if (ASMDeclarations.ASMDef[x].name == null)
                    return false;
                if (asm == ASMDeclarations.ASMDef[x].name)
                    return true;
            }
            return false;
        }

        /* ---------- Disassembly ---------- */
        public static string ASMDisassemble(string hexCode)
        {
            string ret = "";

            /* Remove comments and split lines */
            var re = @"(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')|//.*|/\*(?s:.*?)\*/";
            string[] hexArr = Regex.Replace(hexCode, re, "$1").Split('\r');
            int x = 0;
            addr = true;

            for (x = 0; x < hexArr.Length; x++)
            {
                if (hexArr[x] != "")
                {
                    if (hexArr[x].Length <= 8 || hexArr[x] == "\n" || hexArr[x].IndexOf(' ') < 0)
                        addr = false;
                    uint uintHex = 0;
                    hexArr[x] = hexArr[x].Replace("\n", "");
                    string[] hexWords = hexArr[x].Split(' ');
                    string hexVal = hexWords[hexWords.Length - 1];
                    if (hexVal != "")
                    {
                        string hexAddr = "";
                        if (addr)
                        {
                            hexAddr = Main.sMid(hexWords[1], 1, 8);

                            if (DisASMAddr != uint.Parse(hexAddr, System.Globalization.NumberStyles.HexNumber))
                            {
                                DisASMAddr = uint.Parse(hexAddr, System.Globalization.NumberStyles.HexNumber);
                                ret += "address 0x" + hexAddr + "\n" + "\n";
                            }
                        }

                        //Parse each word (32 bits) of the whole code
                        for (int lineParse = 0; lineParse < hexVal.Length; lineParse += 8)
                        {
                            string val = Main.sMid(hexVal, lineParse, 8);

                            try
                            {
                                uintHex = Convert.ToUInt32(val, 16);
                            }
                            catch (Exception e)
                            {
                                goto asmDisNextX;
                            }

                            int InsIndex = GetInsFromHex(uintHex);
                            if (addr)
                                DisASMAddr += 4;
                            if (InsIndex < 0)
                                ret += "hexcode 0x" + val + "\n";
                            else
                            {
                                string asmReg = DisASMReg(InsIndex, uintHex);
                                ret += ASMDef[InsIndex].name + " " + asmReg + "\n";
                            }

                        asmDisNextX: ;
                        }
                    }
                }
            }

            if (ASMDisMode == 0)
            {
                /* Parse through and insert labels */
                RichTextBox tempBox = new RichTextBox();
                tempBox.Text = ret;
                int cnt = 0;
                DisASMAddr = 0;
                bool[] labelInsert = new bool[DisASMLabel.Length];
                bool addrLast = false;

                for (cnt = 0; cnt < tempBox.Lines.Length; cnt++)
                {
                    if (tempBox.Lines[cnt] != "")
                    {
                        string[] tempStr = tempBox.Lines[cnt].Split(' ');
                        if (tempStr[0] == "address")
                        {
                            string tempA = tempStr[1].Replace("0x", "");
                            tempA = tempA.Replace("$", "");
                            tempA = tempA.Replace(" ", "");
                            DisASMAddr = uint.Parse(tempA, System.Globalization.NumberStyles.HexNumber);
                            addrLast = true;
                        }

                        for (x = 0; x < DisASMLabel.Length; x++)
                        {
                            if (DisASMAddr == DisASMLabel[x].address)
                            {
                                int lineStart = 0;
                                if (addrLast)
                                {
                                    for (int y = 0; y <= cnt; y++)
                                        lineStart += tempBox.Lines[y].Length + 1;
                                }
                                else
                                {
                                    for (int y = 0; y < cnt; y++)
                                        lineStart += tempBox.Lines[y].Length + 1;
                                }

                                tempBox.Text = tempBox.Text.Insert(lineStart, DisASMLabel[x].name + ":" + "\n");
                                cnt++;
                                labelInsert[x] = true;
                                break;
                            }
                        }

                        if (tempStr[0] != "hook" && tempStr[0] != "hookl")
                            DisASMAddr += 4;
                        if (tempStr[0] == "setreg")
                            DisASMAddr += 4;

                        addrLast = false;
                    }
                }


                ret = tempBox.Text;
                tempBox.Dispose();

                if (addr == false && !ret.StartsWith("address"))
                    ret = "address 0x00000000\n\n" + ret;
                ret = ret.Replace("blr", "blr\n"); //Add newline after a blr

                //Go through and add any labels not already added
                for (x = 0; x < labelInsert.Length; x++)
                {
                    //Insert label at the end
                    if (labelInsert[x] == false)
                    {
                        ret += "\n" + "address 0x" + DisASMLabel[x].address.ToString("X8") + "\n";
                        ret += DisASMLabel[x].name + ":\n";
                    }
                }
            }
            else
            {
                for (int z = 0; z < DisASMLabel.Length; z++)
                    ret = ret.Replace(":" + DisASMLabel[z].name, "0x" + DisASMLabel[z].address.ToString("X8"));
            }


            //Reset
            DisASMAddr = 0;
            DisASMLabel = new ASMLabel[0];
            addr = true;

            return ret;
        }

        public static int GetInsFromHex(uint val)
        {
            int x = 0;
            int skip = 0;

            while (ASMDef[x].name != null)
            {
                //uint newVal = ((val << ASMDef[x].opShift[0]) >> (ASMDef[x].opShift[0] + ASMDef[x].opShift[1])) << ASMDef[x].opShift[1];
                uint opVal = ((val << ASMDef[x].opShift[0]) >> (ASMDef[x].opShift[0] + ASMDef[x].opShift[1])) << ASMDef[x].opShift[1];
                uint newVal = opVal & ~ASMDef[x].opCode;
                uint subVal = val - ASMDef[x].opCode;
                if (subVal == newVal && ((val & ASMDef[x].opCode) == ASMDef[x].opCode))
                {
                    //Instruction specific fixes
                    if (ASMDef[x].name == "addi" && GetStrFromBit(16, 5, newVal) == "0")
                        skip = 1;
                    else if (ASMDef[x].name == "addis" && GetStrFromBit(16, 5, newVal) == "0")
                        skip = 1;
                    else if (ASMDef[x].name == "cmpw" && GetStrFromBit(0, 8, newVal) != "0")
                        skip = 1;
                    else if (ASMDef[x].name == "std" && GetStrFromBit(0, 1, newVal) != "0")
                        skip = 1;
                    else if (ASMDef[x].name == "slwi" && GetStrFromBit(1, 5, newVal) == "31")
                        skip = 1;
                    else if (ASMDef[x].type == typeNOP && val != ASMDef[x].opCode)
                        skip = 1;

                    if (skip == 0)
                        return x;

                    skip = 0;
                }

                x++;
            }
            return -1;
        }

        public static string DisASMReg(int index, uint val)
        {
            if (ASMDef[index].shifts == null)
                return "";

            int x = 0;
            string ret = "";
            string[] regs = null;
            string preReg = "r";
            if (ASMDeclarations.ASMDef[index].type == typeFNAN || ASMDeclarations.ASMDef[index].type == typeFOFI)
                preReg = "f";

            switch (ASMDef[index].type)
            {
                case typeNAN:
                case typeFNAN:
                    regs = new string[ASMDef[index].shifts.Length];

                    for (x = 0; x < ASMDef[index].shifts.Length; x++)
                        regs[ASMDef[index].order[x]] = preReg + GetStrFromBit(ASMDef[index].shifts[x], 5, val);
                    break;
                case typeBNC:
                    regs = new string[ASMDef[index].shifts.Length];

                    uint bVal = (uint.Parse(GetStrFromBit(2, 23, val)) << 2);
                    uint labelAddr = DisASMAddr;
                    if (bVal >= 0x1800000)
                        labelAddr += (uint)Convert.ToUInt32(bVal - 0x2000000);
                    else
                        labelAddr += (uint)Convert.ToUInt32(bVal);
                    labelAddr -= 4;
                    string labelName = "";
                    if (addr)
                    {
                        labelName = "loc_" + labelAddr.ToString("X");

                        //Check if label already exists
                        int exists = 0;
                        for (x = 0; x < DisASMLabel.Length; x++)
                        {
                            if (DisASMLabel[x].address == labelAddr)
                            {
                                exists = 1;
                                break;
                            }
                        }
                        if (exists == 0)
                        {
                            Array.Resize(ref DisASMLabel, DisASMLabel.Length + 1);
                            DisASMLabel[DisASMLabel.Length - 1].address = labelAddr;
                            DisASMLabel[DisASMLabel.Length - 1].name = labelName;
                        }
                    }

                    for (x = 0; x < ASMDef[index].shifts.Length - 1; x++)
                        regs[ASMDef[index].order[x]] = "cr" + GetStrFromBit(ASMDef[index].shifts[x], 3, val);

                    if (addr)
                        regs[ASMDef[index].order[x]] = ":" + labelName;
                    else
                        regs[ASMDef[index].order[x]] = "0x" + (labelAddr + 4).ToString("X8");
                    break;
                case typeNOP:
                    return "";
                case typeOFI:
                case typeFOFI:
                    regs = new string[ASMDef[index].shifts.Length - 1];


                    for (x = 0; x < ASMDef[index].shifts.Length - 1; x++)
                        regs[ASMDef[index].order[x]] = preReg + GetStrFromBit(ASMDef[index].shifts[x], 5, val);
                    //In the case of a float the second register should be a GP register
                    regs[ASMDef[index].order[x - 1]] = regs[ASMDef[index].order[x - 1]].Replace("f", "r");

                    if (ASMDef[index].name == "stdu")
                        val--;
                    regs[ASMDef[index].order[x - 1]] = "0x" + Main.sRight(val.ToString("X"), 4) + "(" + regs[ASMDef[index].order[x - 1]] + ")";

                    break;
                case typeIMM:
                    regs = new string[ASMDef[index].shifts.Length];

                    for (x = 0; x < ASMDef[index].shifts.Length - 1; x++)
                        regs[ASMDef[index].order[x]] = preReg + GetStrFromBit(ASMDef[index].shifts[x], 5, val);

                    regs[ASMDef[index].order[x]] = "0x" + Main.sRight(val.ToString("X"), 4);
                    break;
                case typeSPR:
                    regs = new string[ASMDef[index].shifts.Length];

                    regs[ASMDef[index].order[0]] = StrToSPR(GetStrFromBit(ASMDef[index].shifts[0], 4, val));

                    for (x = 1; x < ASMDef[index].shifts.Length; x++)
                        regs[ASMDef[index].order[x]] = preReg + GetStrFromBit(ASMDef[index].shifts[x], 5, val);
                    break;
                case typeBNCMP:
                    int BNCMPoff = 4 * int.Parse(GetStrFromBit(2, 14, val));
                    int cmpReg = int.Parse(GetStrFromBit(ASMDef[index + 1].shifts[0], 3, val));

                    if (cmpReg != 0)
                        index++;

                    regs = new string[ASMDef[index].shifts.Length];
                    for (x = 0; x < ASMDef[index].shifts.Length - 1; x++)
                        regs[ASMDef[index].order[x]] = "cr" + GetStrFromBit(ASMDef[index].shifts[x], 3, val);

                    if ((Int16)BNCMPoff < 0)
                        regs[ASMDef[index].order[x]] = "-0x" + (-(Int16)BNCMPoff).ToString("X");
                    else
                        regs[ASMDef[index].order[x]] = "0x" + BNCMPoff.ToString("X");
                    break;
                case typeCND:
                    int cndReg = int.Parse(GetStrFromBit(ASMDef[index + 1].shifts[0], 3, val));
                    if (cndReg != 0)
                        index++;

                    regs = new string[ASMDef[index].shifts.Length];

                    int y = 0;
                    if (cndReg != 0)
                    {
                        regs[ASMDef[index].order[0]] = "cr" + GetStrFromBit(ASMDef[index].shifts[0], 3, val);
                        y = 1;
                    }
                    //Immediate
                    if (ASMDef[index].name[ASMDef[index].name.Length - 1] == 'i')
                    {
                        for (x = y; x < ASMDef[index].shifts.Length - 1; x++)
                            regs[ASMDef[index].order[x]] = preReg + GetStrFromBit(ASMDef[index].shifts[x], 5, val);

                        regs[ASMDef[index].order[x]] = "0x" + Main.sRight(val.ToString("X"), 4);
                    }
                    else
                    {
                        for (x = y; x < ASMDef[index].shifts.Length; x++)
                            regs[ASMDef[index].order[x]] = "r" + GetStrFromBit(ASMDef[index].shifts[x], 5, val);
                    }
                    break;
                case typeIMM5:
                    regs = DisassembleRotate(index, preReg, val);
                    break;
                case typeFCND:
                    int fcndReg = int.Parse(GetStrFromBit(ASMDef[index + 1].shifts[0], 3, val));
                    if (fcndReg != 0)
                        index++;

                    regs = new string[ASMDef[index].shifts.Length];

                    int fy = 0;
                    if (fcndReg != 0)
                    {
                        regs[ASMDef[index].order[0]] = "cr" + GetStrFromBit(ASMDef[index].shifts[0], 3, val);
                        fy = 1;
                    }

                    for (x = fy; x < ASMDef[index].shifts.Length; x++)
                        regs[ASMDef[index].order[x]] = "f" + GetStrFromBit(ASMDef[index].shifts[x], 5, val);
                    break;
            }

            for (x = 0; x < regs.Length-1; x++)
                ret += regs[x] + ", ";
            ret += regs[x];
            return ret;
        }

        public static string[] DisassembleRotate(int index, string preReg, uint val)
        {
            string op = ASMDef[index].name;
            string[] regs = new string[ASMDef[index].shifts.Length];
            int x = 0;
            //ASMDef[index].order[x]
            for (x = 0; x < ASMDef[index].shifts.Length - 1; x++)
                regs[ASMDef[index].order[x]] = preReg + GetStrFromBit(ASMDef[index].shifts[x], 5, val);

            switch (op)
            {
                case "slwi":
                case "srwi":
                    regs[ASMDef[index].order[x]] = GetStrFromBit(ASMDef[index].shifts[x], 5, val);
                    break;
                case "sldi":
                    bool isLessThan32 = GetStrFromBit(1, 1, val) == "0";

                    if (isLessThan32)
                        regs[ASMDef[index].order[x]] = GetStrFromBit(ASMDef[index].shifts[x], 5, val);
                    else
                        regs[ASMDef[index].order[x]] = (int.Parse(GetStrFromBit(ASMDef[index].shifts[x], 5, val)) + 32).ToString();
                    break;
                case "srdi":
                    bool is32 = GetStrFromBit(1, 1, val) == "1";

                    if (is32)
                        regs[ASMDef[index].order[x]] = "32";
                    else
                        regs[ASMDef[index].order[x]] = (64 - int.Parse(GetStrFromBit(ASMDef[index].shifts[ASMDef[index].order[x]], 5, val))).ToString();
                    break;
            }

            return regs;
        }

        public static string GetStrFromBit(int bitOff, int len, uint val)
        {
            string valStr = Convert.ToString(val, 2);
            valStr = Reverse(valStr.PadLeft(32, '0'));
            string bitStr = Main.sMid(valStr, bitOff, len);
            return Convert.ToInt32(Reverse(bitStr), 2).ToString();
        }

        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static string StrToSPR(string val)
        {
            switch (val)
            {
                case "1":
                    return "XER";
                case "8":
                    return "LR";
                case "9":
                    return "CTR";
            }
            return "";
        }

        /* ---------- Declarations ---------- */
        public static int GetRegHelpIndex(string reg)
        {
            switch (reg.ToLower())
            {
                case "r0":
                    return 0;
                case "r1":
                    return 1;
                case "r2":
                    return 2;
                case "r3":
                    return 3;
                case "r4":
                case "r5":
                case "r6":
                case "r7":
                case "r8":
                case "r9":
                case "r10":
                    return 4;
                case "r11":
                case "r12":
                    return 5;
                case "r13":
                    return 6;
                case "r14":
                case "r15":
                case "r16":
                case "r17":
                case "r18":
                case "r19":
                case "r20":
                case "r21":
                case "r22":
                case "r23":
                case "r24":
                case "r25":
                case "r26":
                case "r27":
                case "r28":
                case "r29":
                case "r30":
                case "r31":
                    return 7;
                case "f0":
                case "f1":
                case "f2":
                case "f3":
                case "f4":
                case "f5":
                case "f6":
                case "f7":
                case "f8":
                case "f9":
                case "f10":
                case "f11":
                case "f12":
                case "f13":
                    return 8;
                case "f14":
                case "f15":
                case "f16":
                case "f17":
                case "f18":
                case "f19":
                case "f20":
                case "f21":
                case "f22":
                case "f23":
                case "f24":
                case "f25":
                case "f26":
                case "f27":
                case "f28":
                case "f29":
                case "f30":
                case "f31":
                    return 9;
                case "cr0":
                case "cr1":
                case "cr2":
                case "cr3":
                case "cr4":
                case "cr5":
                case "cr6":
                case "cr7":
                    return 10;
                case "xer":
                    return 11;
                case "lr":
                    return 12;
                case "ctr":
                    return 13;
            }


            return 0;
        }

        #endregion

        #region Register and Instruction Declarations

        /*
         * ORDER MATTERS COMPLETELY
         * When you disassemble, you will probably have to reorder some instructions so that it disassembles properly
         */
        public static void DeclareInstructions()
        {
            ulong x = 0;

            /* Define registers */
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Firebrick;
            RegColArr[x].title = "Register Zero";
            x++; //1
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.BurlyWood;
            RegColArr[x].title = "Stack Pointer";
            x++; //2
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Chocolate;
            RegColArr[x].title = "Environment Pointer";
            x++; //3
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Violet;
            RegColArr[x].title = "Argument 1 and Return Value";
            x++; //4
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Turquoise;
            RegColArr[x].title = "Argument 2";
            x++; //5
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Turquoise;
            RegColArr[x].title = "Argument 3";
            x++; //6
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Turquoise;
            RegColArr[x].title = "Argument 4";
            x++; //7
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Turquoise;
            RegColArr[x].title = "Argument 5";
            x++; //8
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Turquoise;
            RegColArr[x].title = "Argument 6";
            x++; //9
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Turquoise;
            RegColArr[x].title = "Argument 7";
            x++; //10
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Turquoise;
            RegColArr[x].title = "Argument 8";
            x++; //11
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Wheat;
            RegColArr[x].title = "Unknown";
            x++; //12
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Wheat;
            RegColArr[x].title = "Unknown";
            x++; //13
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Brown;
            RegColArr[x].title = "Data Area Pointer?";
            x++; //14
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Crimson;
            RegColArr[x].title = "Saved Register";
            x++; //15
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Crimson;
            RegColArr[x].title = "Saved Register";
            x++; //16
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Crimson;
            RegColArr[x].title = "Saved Register";
            x++; //17
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Crimson;
            RegColArr[x].title = "Saved Register";
            x++; //18
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Crimson;
            RegColArr[x].title = "Saved Register";
            x++; //19
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Crimson;
            RegColArr[x].title = "Saved Register";
            x++; //20
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Crimson;
            RegColArr[x].title = "Saved Register";
            x++; //21
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Crimson;
            RegColArr[x].title = "Saved Register";
            x++; //22
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Crimson;
            RegColArr[x].title = "Saved Register";
            x++; //23
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Crimson;
            RegColArr[x].title = "Saved Register";
            x++; //24
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Crimson;
            RegColArr[x].title = "Saved Register";
            x++; //25
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Crimson;
            RegColArr[x].title = "Saved Register";
            x++; //26
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Crimson;
            RegColArr[x].title = "Saved Register";
            x++; //27
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Crimson;
            RegColArr[x].title = "Saved Register";
            x++; //28
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Crimson;
            RegColArr[x].title = "Saved Register";
            x++; //29
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Crimson;
            RegColArr[x].title = "Saved Register";
            x++; //30
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Crimson;
            RegColArr[x].title = "Saved Register";
            x++; //31
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Brushes.Crimson;
            RegColArr[x].title = "Saved Register";
            x++; //cr0
            RegColArr[x].reg = "cr0";
            RegColArr[x].col = Brushes.LightSeaGreen;
            RegColArr[x].title = "Compare Register 0";
            x++; //cr1
            RegColArr[x].reg = "cr1";
            RegColArr[x].col = Brushes.LightSeaGreen;
            RegColArr[x].title = "Compare Register 1";
            x++; //cr2
            RegColArr[x].reg = "cr2";
            RegColArr[x].col = Brushes.LightSeaGreen;
            RegColArr[x].title = "Compare Register 2";
            x++; //cr3
            RegColArr[x].reg = "cr3";
            RegColArr[x].col = Brushes.LightSeaGreen;
            RegColArr[x].title = "Compare Register 3";
            x++; //cr4
            RegColArr[x].reg = "cr4";
            RegColArr[x].col = Brushes.LightSeaGreen;
            RegColArr[x].title = "Compare Register 4";
            x++; //cr5
            RegColArr[x].reg = "cr5";
            RegColArr[x].col = Brushes.LightSeaGreen;
            RegColArr[x].title = "Compare Register 5";
            x++; //cr6
            RegColArr[x].reg = "cr6";
            RegColArr[x].col = Brushes.LightSeaGreen;
            RegColArr[x].title = "Compare Register 6";
            x++; //cr7
            RegColArr[x].reg = "cr7";
            RegColArr[x].col = Brushes.LightSeaGreen;
            RegColArr[x].title = "Compare Register 7";
            x++;
            RegColArr[x].reg = "xer";
            RegColArr[x].col = Brushes.Cornsilk;
            RegColArr[x].title = "Fixed-Point Exception Register";
            x++;
            RegColArr[x].reg = "lr";
            RegColArr[x].col = Brushes.Cornsilk;
            RegColArr[x].title = "Link Register";
            x++;
            RegColArr[x].reg = "ctr";
            RegColArr[x].col = Brushes.Cornsilk;
            RegColArr[x].title = "Count Register";
            x++;
            RegColArr[x].reg = "fpscr";
            RegColArr[x].col = Brushes.Cornsilk;
            RegColArr[x].title = "Floating-Point Status and Control Register";
            x++;
            int floatCnt = 0;
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 0";
            x++; floatCnt++; //fr1
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Float Argument 1 and Return Value";
            x++; floatCnt++; //fr2
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Float Argument 2";
            x++; floatCnt++; //fr3
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Float Argument 3";
            x++; floatCnt++; //fr4
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Float Argument 4";
            x++; floatCnt++; //fr5
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Float Argument 5";
            x++; floatCnt++; //fr6
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Float Argument 6";
            x++; floatCnt++; //fr7
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Float Argument 7";
            x++; floatCnt++; //fr8
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Float Argument 8";
            x++; floatCnt++; //fr9
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 9";
            x++; floatCnt++; //fr10
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 10";
            x++; floatCnt++; //fr11
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 11";
            x++; floatCnt++; //fr12
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 12";
            x++; floatCnt++; //fr13
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 13";
            x++; floatCnt++; //fr14
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 14";
            x++; floatCnt++; //fr15
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 15";
            x++; floatCnt++; //fr16
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 16";
            x++; floatCnt++; //fr17
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 17";
            x++; floatCnt++; //fr18
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 18";
            x++; floatCnt++; //fr19
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 19";
            x++; floatCnt++; //fr20
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 20";
            x++; floatCnt++; //fr21
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 21";
            x++; floatCnt++; //fr22
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 22";
            x++; floatCnt++; //fr23
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 23";
            x++; floatCnt++; //fr24
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 24";
            x++; floatCnt++; //fr25
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 25";
            x++; floatCnt++; //fr26
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 26";
            x++; floatCnt++; //fr27
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 27";
            x++; floatCnt++; //fr28
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 28";
            x++; floatCnt++; //fr29
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 29";
            x++; floatCnt++; //fr30
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 30";
            x++; floatCnt++; //fr31
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Brushes.Tan;
            RegColArr[x].title = "Floating Point Register 31";
            x++; floatCnt++;

            /* ---------- Instructions ---------- */
            /*
             * Refer to the "PPC_Vers202" pdf's at:
             * http://moss.csc.ncsu.edu/~mueller/cluster/ps3/SDK3.0/docs/arch/
             * 
             * Bear in mind that this isn't 100% alphabetical
             * Some have been moved within their first character to disassemble correctly
             */

            x = 0;
            string asmHelp = "", nl = Environment.NewLine;

            //Conflicts with cmpd
            asmHelp = "Stores (~rA + rB + 1) into rD" + nl;
            asmHelp += "subf rD, rA, rB :: rD = rB - rA or rD = ~rA + rB + 1" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "subf r4, r4, r3" + nl;
            ASMDef[x].name = "subf";
            ASMDef[x].opCode = 0x7C000050;
            ASMDef[x].opShift = new int[] { 9, 0 };
            ASMDef[x].shifts = new int[3] { 21, 16, 11 };
            ASMDef[x].order = new int[3] { 0, 1, 2 };
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Subtract From (subf)";
            x++;

            InsPlaceArr[(char)'A' - (char)'A'] = (int)x;
            /* ---------- addis ---------- */
            asmHelp = "Adds the register rA with the 16-bit signed value IMM that has been shifted left 16 bits and stores it into the lower part of register rD" + nl;
            asmHelp += "addis rD, rA, 0xIMM :: rD = (rA + IMM) << 16" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "addis r4, r3, 0x1FF0" + nl;
            ASMDef[x].name = "addis";
            ASMDef[x].opCode = 0x3C000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 21; ASMDef[x].shifts[1] = 16; ASMDef[x].shifts[2] = 0;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeIMM;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Add Immediate Shifted (addis)";
            x++;

            asmHelp = "Adds the register rA with the 16-bit signed value IMM and stores it into the lower part of register rD. It also does something with the XER register that I don't understand..." + nl;
            asmHelp += "addic rD, rA, 0xIMM :: rD = (rA + IMM)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "addic r4, r3, 0x1FF0" + nl;
            ASMDef[x].name = "addic";
            ASMDef[x].opCode = 0x30000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 21; ASMDef[x].shifts[1] = 16; ASMDef[x].shifts[2] = 0;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeIMM;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Add Immediate And Carry (addic)";
            x++;

            asmHelp = "Adds the register rA with the 16-bit signed value IMM and stores it into the upper part of register rD" + nl;
            asmHelp += "addi rD, rA, 0xIMM :: rD = rA + IMM" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "addi r4, r3, 0x1FF0" + nl;
            ASMDef[x].name = "addi";
            ASMDef[x].opCode = 0x38000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 21; ASMDef[x].shifts[1] = 16; ASMDef[x].shifts[2] = 0;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeIMM;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Add Immediate (addi)";
            x++;

            asmHelp = "Adds the register rA with the register rB and stores it into the register rD" + nl;
            asmHelp += "add rD, rA, rB :: rD = rA + rB" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "add r3, r3, r4" + nl;
            ASMDef[x].name = "add";
            ASMDef[x].opCode = 0x7C000214;
            ASMDef[x].opShift = new int[] { 6, 10 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 21; ASMDef[x].shifts[1] = 16; ASMDef[x].shifts[2] = 11;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Add (add)";
            x++;

            asmHelp = "Ands the register rA with the register rB and stores it into the register rD" + nl;
            asmHelp += "and rD, rA, rB :: rD = rA & rB" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "and r3, r3, r4" + nl;
            ASMDef[x].name = "and";
            ASMDef[x].opCode = 0x7C000038;
            ASMDef[x].opShift = new int[] { 6, 10 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 21; ASMDef[x].shifts[1] = 16; ASMDef[x].shifts[2] = 11;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 1; ASMDef[x].order[1] = 0; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Logical And (and)";
            x++;

            asmHelp = "Ands the register rA with IMM and stores it into the register rD" + nl;
            asmHelp += "andi rD, rA, IMM :: rD = rA & IMM" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "andi r3, r4, 0x1234" + nl;
            ASMDef[x].name = "andi";
            ASMDef[x].opCode = 0x70000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 21; ASMDef[x].shifts[1] = 16;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 1; ASMDef[x].order[1] = 0; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeIMM;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Logical And Immediate (andi)";
            x++;

            asmHelp = "Ands the register rA with IMM that has been shifted left 16 bits and stores it into the register rD" + nl;
            asmHelp += "andis rD, rA, IMM :: rD = (rA & IMM) << 16" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "andis r3, r4, 0x1234" + nl;
            ASMDef[x].name = "andis";
            ASMDef[x].opCode = 0x74000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 21; ASMDef[x].shifts[1] = 16;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 1; ASMDef[x].order[1] = 0; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeIMM;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Logical And Immediate Shifted (andis)";
            x++;

            InsPlaceArr[(char)'B' - (char)'A'] = (int)x;
            /* ---------- blr ---------- */
            asmHelp = "Branches to the address held in the special register LR" + nl;
            asmHelp += "blr :: PC = LR" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "blr" + nl;
            ASMDef[x].name = "blr";
            ASMDef[x].opCode = 0x4E800020;
            ASMDef[x].opShift = new int[] { 9, 6 };
            ASMDef[x].type = typeNOP;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Branch To Link Register (blr)";
            x++;

            asmHelp = "Branches to the unsigned value IMM and sets special register LR to the address after it" + nl;
            asmHelp += "bl IMM :: LR = CURRENT_ADDR + 4; PC = IMM" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "bl 0x0024A7FC" + nl;
            ASMDef[x].name = "bl";
            ASMDef[x].opCode = 0x48000001;
            ASMDef[x].opShift = new int[] { 6, 1 };
            ASMDef[x].shifts = new int[1];
            ASMDef[x].shifts[0] = 2;
            ASMDef[x].order = new int[1];
            ASMDef[x].order[0] = 0;
            ASMDef[x].type = typeBNC;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Branch And Link (bl)";
            x++;

            asmHelp = "Branches to the value (IMM + CURRENT_ADDR) if CR is set equal" + nl;
            asmHelp += "If there is no CR specified, then it will be defaulted to cr0" + nl;
            asmHelp += "beq CR, IMM :: if (CR == 1) then PC = CURRENT_ADDR + IMM" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "beq cr2, 0x5C" + nl;
            //cr0 is default bf
            ASMDef[x].name = "beq";
            ASMDef[x].opCode = 0x41820000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[1];
            ASMDef[x].shifts[0] = 2;
            ASMDef[x].order = new int[1];
            ASMDef[x].order[0] = 0;
            ASMDef[x].type = typeBNCMP;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Branch If Equal (beq)";
            x++;
            //bf is declared
            ASMDef[x].name = "beq";
            ASMDef[x].opCode = 0x41820000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 18; ASMDef[x].shifts[1] = 2;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeBNCMP;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Branch If Equal (beq)";
            x++;

            asmHelp = "Branches to the value (IMM + CURRENT_ADDR) if CR is not set equal" + nl;
            asmHelp += "If there is no CR specified, then it will be defaulted to cr0" + nl;
            asmHelp += "bne CR, IMM :: if (CR != 1) then PC = 0xCURRENT_ADDR + IMM (DEC)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "bne cr2, 0x54" + nl;
            //cr0 is default bf
            ASMDef[x].name = "bne";
            ASMDef[x].opCode = 0x40820000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[1];
            ASMDef[x].shifts[0] = 2;
            ASMDef[x].order = new int[1];
            ASMDef[x].order[0] = 0;
            ASMDef[x].type = typeBNCMP;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Branch If Not Equal (bne)";
            x++;
            //bf is declared
            ASMDef[x].name = "bne";
            ASMDef[x].opCode = 0x40820000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 18; ASMDef[x].shifts[1] = 2;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeBNCMP;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Branch If Not Equal (bne)";
            x++;

            asmHelp = "Branches to the value (IMM + CURRENT_ADDR) if CR is set less than or equal" + nl;
            asmHelp += "If there is no CR specified, then it will be defaulted to cr0" + nl;
            asmHelp += "ble CR, IMM :: if (CR == 1 || CR == 4) then PC = 0xCURRENT_ADDR + 0xIMM" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "ble cr2, 0x5C" + nl;
            //cr0 is default bf
            ASMDef[x].name = "ble";
            ASMDef[x].opCode = 0x40810000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[1];
            ASMDef[x].shifts[0] = 2;
            ASMDef[x].order = new int[1];
            ASMDef[x].order[0] = 0;
            ASMDef[x].type = typeBNCMP;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Branch If Less Than Or Equal (ble)";
            x++;
            //bf is declared
            ASMDef[x].name = "ble";
            ASMDef[x].opCode = 0x40810000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 18; ASMDef[x].shifts[1] = 2;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeBNCMP;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Branch If Less Than Or Equal (ble)";
            x++;

            asmHelp = "Branches to the value (IMM + CURRENT_ADDR) if CR is set greater than or equal" + nl;
            asmHelp += "If there is no CR specified, then it will be defaulted to cr0" + nl;
            asmHelp += "bge CR, IMM :: if (CR == 1 || CR == 2) then PC = 0xCURRENT_ADDR + 0xIMM" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "bge cr2, 0x5C" + nl;
            //cr0 is default bf
            ASMDef[x].name = "bge";
            ASMDef[x].opCode = 0x40800000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[1];
            ASMDef[x].shifts[0] = 2;
            ASMDef[x].order = new int[1];
            ASMDef[x].order[0] = 0;
            ASMDef[x].type = typeBNCMP;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Branch If Greater Than Or Equal (bge)";
            x++;
            //bf is declared
            ASMDef[x].name = "bge";
            ASMDef[x].opCode = 0x40800000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 18; ASMDef[x].shifts[1] = 2;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeBNCMP;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Branch If Greater Than Or Equal (bge)";
            x++;

            asmHelp = "Branches to the value (IMM + CURRENT_ADDR) if CR is set greater than" + nl;
            asmHelp += "If there is no CR specified, then it will be defaulted to cr0" + nl;
            asmHelp += "bgt CR, IMM :: if (CR == 2) then PC = 0xCURRENT_ADDR + 0xIMM" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "bgt cr2, 0x5C" + nl;
            //cr0 is default bf
            ASMDef[x].name = "bgt";
            ASMDef[x].opCode = 0x41810000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[1];
            ASMDef[x].shifts[0] = 2;
            ASMDef[x].order = new int[1];
            ASMDef[x].order[0] = 0;
            ASMDef[x].type = typeBNCMP;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Branch If Greater Than (bgt)";
            x++;
            //bf is declared
            ASMDef[x].name = "bgt";
            ASMDef[x].opCode = 0x41810000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 18; ASMDef[x].shifts[1] = 2;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeBNCMP;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Branch If Greater Than (bgt)";
            x++;

            asmHelp = "Branches to the value (IMM + CURRENT_ADDR) if CR is set less than" + nl;
            asmHelp += "If there is no CR specified, then it will be defaulted to cr0" + nl;
            asmHelp += "blt CR, IMM :: if (CR == 4) then PC = 0xCURRENT_ADDR + 0xIMM" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "blt cr2, 0x5C" + nl;
            //cr0 is default bf
            ASMDef[x].name = "blt";
            ASMDef[x].opCode = 0x41800000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[1];
            ASMDef[x].shifts[0] = 2;
            ASMDef[x].order = new int[1];
            ASMDef[x].order[0] = 0;
            ASMDef[x].type = typeBNCMP;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Branch If Less Than (blt)";
            x++;
            //bf is declared
            ASMDef[x].name = "blt";
            ASMDef[x].opCode = 0x41800000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 18; ASMDef[x].shifts[1] = 2;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeBNCMP;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Branch If Less Than (blt)";
            x++;

            asmHelp = "Branches to the unsigned value IMM" + nl;
            asmHelp += "b IMM :: PC = IMM" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "b 0x0024A7FC" + nl;
            ASMDef[x].name = "b";
            ASMDef[x].opCode = 0x48000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[1];
            ASMDef[x].shifts[0] = 2;
            ASMDef[x].order = new int[1];
            ASMDef[x].order[0] = 0;
            ASMDef[x].type = typeBNC;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Branch (b)";
            x++;

            InsPlaceArr[(char)'C' - (char)'A'] = (int)x;
            /* ---------- cmpwi ---------- */
            asmHelp = "Compares (signed 32 bits) rA with the signed value IMM and stores result in BF" + nl;
            asmHelp += "If there is no BF specified, then it will be defaulted to cr0" + nl;
            asmHelp += "cmpwi rA, 0xIMM; cmpwi cr1, rA, 0xIMM" + nl;
            asmHelp += "    if (rA == IMM) then BF = 1; if (rA > IMM) then BF = 2; if (rA < IMM) then BF = 4" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "cmpwi cr2, r4, 0xA7FC" + nl;
            //cr0 is default bf
            ASMDef[x].name = "cmpwi";
            ASMDef[x].opCode = 0x2C000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 16;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeCND;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Compare Word Immediate (cmpwi)";
            x++;
            //bf register declared
            ASMDef[x].name = "cmpwi";
            ASMDef[x].opCode = 0x2C000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 23; ASMDef[x].shifts[1] = 16;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeCND;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Compare Word Immediate (cmpwi)";
            x++;

            asmHelp = "Compares (unsigned 32 bits) rA with the unsigned value IMM and stores result in BF" + nl;
            asmHelp += "If there is no BF specified, then it will be defaulted to cr0" + nl;
            asmHelp += "cmplwi rA, 0xIMM; cmplwi cr1, rA, 0xIMM" + nl;
            asmHelp += "    if (rA == IMM) then BF = 1; if (rA > IMM) then BF = 2; if (rA < IMM) then BF = 4" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "cmplwi cr2, r4, 0xA7FC" + nl;
            //cr0 is default bf
            ASMDef[x].name = "cmplwi";
            ASMDef[x].opCode = 0x28000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 16;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeCND;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Compare Logical Word Immediate (cmplwi)";
            x++;
            //bf declared
            ASMDef[x].name = "cmplwi";
            ASMDef[x].opCode = 0x28000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 23; ASMDef[x].shifts[1] = 16;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeCND;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Compare Logical Word Immediate (cmplwi)";
            x++;

            asmHelp = "Compares (signed 64 bits) rA with rB and stores result in BF" + nl;
            asmHelp += "If there is no BF specified, then it will be defaulted to cr0" + nl;
            asmHelp += "cmpd rA, rB; cmpw cr1, rA, rB" + nl;
            asmHelp += "    if (rA == rB) then BF = 1; if (rA > rB) then BF = 2; if (rA < rB) then BF = 4" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "cmpd cr2, r4, r3" + nl;
            //cr0 is default bf
            ASMDef[x].name = "cmpd";
            ASMDef[x].opCode = 0x7C200000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 16; ASMDef[x].shifts[1] = 11;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeCND;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Compare Doubleword (cmpd)";
            x++;
            //bf declared
            ASMDef[x].name = "cmpd";
            ASMDef[x].opCode = 0x7C200000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 23; ASMDef[x].shifts[1] = 16; ASMDef[x].shifts[2] = 11;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeCND;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Compare Doubleword (cmpd)";
            x++;

            asmHelp = "Compares (signed 32 bits) rA with rB and stores result in BF" + nl;
            asmHelp += "If there is no BF specified, then it will be defaulted to cr0" + nl;
            asmHelp += "cmpw rA, rB; cmpw cr1, rA, rB" + nl;
            asmHelp += "    if (rA == rB) then BF = 1; if (rA > rB) then BF = 2; if (rA < rB) then BF = 4" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "cmpw cr2, r4, r3" + nl;
            //cr0 is default bf
            ASMDef[x].name = "cmpw";
            ASMDef[x].opCode = 0x7C000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 16; ASMDef[x].shifts[1] = 11;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeCND;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Compare Word (cmpw)";
            x++;
            //bf declared
            ASMDef[x].name = "cmpw";
            ASMDef[x].opCode = 0x7C000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 23; ASMDef[x].shifts[1] = 16; ASMDef[x].shifts[2] = 11;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeCND;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Compare Word (cmpw)";
            x++;

            asmHelp = "Compares (unsigned 32 bits) the rA with rB and stores result in BF" + nl;
            asmHelp += "If there is no BF specified, then it will be defaulted to cr0" + nl;
            asmHelp += "cmplw rA, rB; cmplw cr1, rA, rB" + nl;
            asmHelp += "if (rA == rB) then BF = 1; if (rA > rB) then BF = 2; if (rA < rB) then BF = 4" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "cmplw cr2, r4, r3" + nl;
            //cr0 is default bf
            ASMDef[x].name = "cmplw";
            ASMDef[x].opCode = 0x7C000040;
            ASMDef[x].opShift = new int[] { 6, 7 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 16; ASMDef[x].shifts[1] = 11;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeCND;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Compare Logical Word (cmplw)";
            x++;
            //bf declared
            ASMDef[x].name = "cmplw";
            ASMDef[x].opCode = 0x7C000040;
            ASMDef[x].opShift = new int[] { 6, 7 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 23; ASMDef[x].shifts[1] = 16; ASMDef[x].shifts[2] = 11;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeCND;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Compare Logical Word (cmplw)";
            x++;

            InsPlaceArr[(char)'D' - (char)'A'] = (int)x;
            /* ---------- divw ---------- */
            asmHelp = "The 64 bit contents of rA are divided by the 64 bit contents of rB and the resulting word is stored into rD" + nl;
            asmHelp += "divw rD, rA, rB :: rD = (int)((long)rA / (long)rB)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "divw r3, r4, r3" + nl;
            ASMDef[x].name = "divw";
            ASMDef[x].opCode = 0x7C0003D6;
            ASMDef[x].opShift = new int[] { 6, 9 };
            ASMDef[x].shifts = new int[3] { 21, 16, 11 };
            ASMDef[x].order = new int[3] { 0, 1, 2 };
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Divide Word (divw)";
            x++;

            InsPlaceArr[(char)'F' - (char)'A'] = (int)x;
            /* ---------- fadd ---------- */
            asmHelp = "Adds frA with frB and stores the result into frD" + nl;
            asmHelp += "fadd frD, frA, frB :: frD = (frA + frB)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "fadd f1, f2, f3" + nl;
            ASMDef[x].name = "fadd";
            ASMDef[x].opCode = 0xFC00002A;
            ASMDef[x].opShift = new int[] { 6, 6 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 11;
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Floating Point Double Add (fadd)";
            x++;

            asmHelp = "Adds frA with frB and stores the result into frD" + nl;
            asmHelp += "fadds frD, frA, frB :: frD = (frA + frB)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "fadds f1, f2, f3" + nl;
            ASMDef[x].name = "fadds";
            ASMDef[x].opCode = 0xEC00002A;
            ASMDef[x].opShift = new int[] { 6, 6 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 11;
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Floating Point Single Add (fadds)";
            x++;

            asmHelp = "The integer held in frA is converted to a double and stored in frD" + nl;
            asmHelp += "fcfid frD, frA :: frD = (Double)frA" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "fcfid f1, f2" + nl;
            ASMDef[x].name = "fcfid";
            ASMDef[x].opCode = 0xFC00069C;
            ASMDef[x].opShift = new int[] { 6, 10 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 11;
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Floating Point Convert to Double From Integer (fcfid)";
            x++;

            asmHelp = "Divides frA by frB and stores the result into frD" + nl;
            asmHelp += "fdivs frD, frA, frB :: frD = (frA / frB)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "fdivs f1, f2, f3" + nl;
            ASMDef[x].name = "fdivs";
            ASMDef[x].opCode = 0xEC000024;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 11;
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Floating Point Single Divide (fdivs)";
            x++;

            asmHelp = "frA is multiplied with frC, frB is added to the result and stored in frD" + nl;
            asmHelp += "fmadd frD, frA, frC, frB :: frD = (frA * frC) + frB" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "fmadd f1, f2, f5, f3" + nl;
            ASMDef[x].name = "fmadd";
            ASMDef[x].opCode = 0xFC00003A;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[] { 21, 16, 11, 6 };
            ASMDef[x].order = new int[] { 0, 1, 3, 2 };
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Floating Point Double Multiply Add (fmadd)";
            x++;

            asmHelp = "frA is multiplied with frC, frB is added to the result and stored in frD" + nl;
            asmHelp += "fmadds frD, frA, frC, frB :: frD = (frA * frC) + frB" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "fmadds f1, f2, f5, f3" + nl;
            ASMDef[x].name = "fmadds";
            ASMDef[x].opCode = 0xEC00003A;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[] { 21, 16, 11, 6 };
            ASMDef[x].order = new int[] { 0, 1, 3, 2 };
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Floating Point Single Multiply Add (fmadds)";
            x++;

            asmHelp = "Multiplies frA with frB and stores the result into frD" + nl;
            asmHelp += "fmul frD, frA, frB :: frD = (frA * frB)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "fmul f1, f2, f3" + nl;
            ASMDef[x].name = "fmul";
            ASMDef[x].opCode = 0xFC000032;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 6;
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Floating Point Double Multiply (fmul)";
            x++;

            asmHelp = "Moves the contents of frA to frD" + nl;
            asmHelp += "fmr frD, frA :: frD = frA" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "fmr f1, f2" + nl;
            ASMDef[x].name = "fmr";
            ASMDef[x].opCode = 0xFC000090;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 11;
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Move Floating Point Register (fmr)";
            x++;

            asmHelp = "Multiplies frA with frB and stores the result into frD" + nl;
            asmHelp += "fmuls frD, frA, frB :: frD = (frA * frB)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "fmuls f1, f2, f3" + nl;
            ASMDef[x].name = "fmuls";
            ASMDef[x].opCode = 0xEC000032;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 6;
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Floating Point Single Multiply (fmuls)";
            x++;

            asmHelp = "Rounds frA to Single and store the result in frD" + nl;
            asmHelp += "frsp frD, frA :: frD = (Single)frA" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "frsp f1, f2" + nl;
            ASMDef[x].name = "frsp";
            ASMDef[x].opCode = 0xFC000018;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 11;
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Floating Point Round to Single (frsp)";
            x++;

            asmHelp = "Takes the square root of frA and stores the result in frD" + nl;
            asmHelp += "fsqrt frD, frA :: frD = sqrt(frA)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "fsqrt f1, f2" + nl;
            ASMDef[x].name = "fsqrt";
            ASMDef[x].opCode = 0xFC00002C;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 11;
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Floating Point Square Root (fsqrt)";
            x++;

            asmHelp = "Subtracts frB from frA and stores the result into frD" + nl;
            asmHelp += "fsub frD, frA, frB :: frD = (frA - frB)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "fsub f1, f2, f3" + nl;
            ASMDef[x].name = "fsub";
            ASMDef[x].opCode = 0xFC000028;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 11;
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Floating Point Double Subtract (fsub)";
            x++;

            asmHelp = "Subtracts frB from frA and stores the result into frD" + nl;
            asmHelp += "fsubs frD, frA, frB :: frD = (frA - frB)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "fsubs f1, f2, f3" + nl;
            ASMDef[x].name = "fsubs";
            ASMDef[x].opCode = 0xEC000028;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 11;
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Floating Point Single Subtract (fsubs)";
            x++;

            asmHelp = "Divides frA by frB and stores the result into frD" + nl;
            asmHelp += "fdiv frD, frA, frB :: frD = (frA / frB)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "fdiv f1, f2, f3" + nl;
            ASMDef[x].name = "fdiv";
            ASMDef[x].opCode = 0xFC000024;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 11;
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Floating Point Double Divide (fdiv)";
            x++;


            asmHelp = "Compares frA with frB (unordered) and places the result in BF and FPCC" + nl;
            asmHelp += "If there is no CR specified, then it will be defaulted to cr0" + nl;
            asmHelp += "fcmpu frA, frB; cmpwi cr1, frA, frB" + nl;
            asmHelp += "    if (frA is NaN || frB is Nan) then BF = 1; if (frA == frB) then BF = 2; if (rA > IMM) then BF = 4; if (rA < IMM) then BF = 8" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "fcmpu cr2, f1, f2" + nl;
            //cr0 is default bf
            ASMDef[x].name = "fcmpu";
            ASMDef[x].opCode = 0xFC000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[] { 16, 11 };
            ASMDef[x].order = new int[] { 0, 1 };
            ASMDef[x].type = typeFCND;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Floating Point Compare Unordered (fcmpu)";
            x++;
            //bf is declared
            ASMDef[x].name = "fcmpu";
            ASMDef[x].opCode = 0xFC000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[] { 23, 16, 11 };
            ASMDef[x].order = new int[] { 0, 1, 2 };
            ASMDef[x].type = typeFCND;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Floating Point Compare Unordered (fcmpu)";
            x++;


            InsPlaceArr[(char)'L' - (char)'A'] = (int)x;
            /* ---------- lbz ---------- */
            asmHelp = "Loads the byte from the address (IMM + rA) and stores it into lower 8 bits of rD\nThe remaining bits are cleared" + nl;
            asmHelp += "lbz rD, IMM(rA) :: rD = (char)MEM(IMM + rA)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "lbz r3, 0x4058(r4)" + nl;
            ASMDef[x].name = "lbz";
            ASMDef[x].opCode = 0x88000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 0;
            ASMDef[x].type = typeOFI;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Load Byte And Zero (lbz)";
            x++;

            asmHelp = "Loads the byte from the address (rA + rB) and stores it into lower 8 bits of rD\nThe remaining bits are cleared" + nl;
            asmHelp += "lbzx rD, rA, rB :: rD = (char)MEM(rA + rB)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "lbzx r3, r4, r5" + nl;
            ASMDef[x].name = "lbzx";
            ASMDef[x].opCode = 0x7C0000AE;
            ASMDef[x].opShift = new int[] { 6, 10 };
            ASMDef[x].shifts = new int[3] { 21, 16, 11 };
            ASMDef[x].order = new int[3] { 0, 1, 2 };
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Load Byte And Zero Indexed (lbzx)";
            x++;

            asmHelp = "Loads the double-word from the address (rA + rB) and stores it into rD" + nl;
            asmHelp += "ldx rD, rA, rB :: rD = (long)MEM(rA + rB)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "ldx r14, r3, r4" + nl;
            ASMDef[x].name = "ldx";
            ASMDef[x].opCode = 0x7C00002A;
            ASMDef[x].opShift = new int[] { 6, 8 };
            ASMDef[x].shifts = new int[3] { 21, 16, 11 };
            ASMDef[x].order = new int[3] { 0, 1, 2 };
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Load Doubleword Indexed (ldx)";
            x++;

            asmHelp = "Loads the double-word from the address (IMM + rA) and stores it into rD" + nl;
            asmHelp += "ld rD, IMM(rA) :: rD = (long)MEM(IMM + rA)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "ld r14, 0x0020(r1)" + nl;
            ASMDef[x].name = "ld";
            ASMDef[x].opCode = 0xE8000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 0;
            ASMDef[x].type = typeOFI;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Load Doubleword (ld)";
            x++;

            asmHelp = "Loads the single from (IMM + rB) and converts it to double and stores it in frA" + nl;
            asmHelp += "lfs frA, IMM(rB) :: frA = (double)((single)MEM(IMM + rB))" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "lfs f1, 0x0054(r5)" + nl;
            ASMDef[x].name = "lfs";
            ASMDef[x].opCode = 0xC0000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 0;
            ASMDef[x].type = typeFOFI;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Load Floating Point Single (lfs)";
            x++;

            asmHelp = "Loads the double from (IMM + rB) and stores it in frA" + nl;
            asmHelp += "lfd frA, IMM(rB) :: rD = (double)MEM(IMM + rB)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "lfd f1, 0x0054(r5)" + nl;
            ASMDef[x].name = "lfd";
            ASMDef[x].opCode = 0xC8000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3] { 21, 16, 0 };
            ASMDef[x].order = new int[3] { 0, 1, 2 };
            ASMDef[x].type = typeFOFI;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Load Floating Point Doubleword (lfd)";
            x++;

            asmHelp = "Loads the halfword from the address (IMM + rA) and stores it into the lower 16 bits of rD\nThe remaining bits are cleared" + nl;
            asmHelp += "lhz rD, IMM(rA) :: rD = (short)MEM(IMM + rA)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "lhz r14, 0x0020(r1)" + nl;
            ASMDef[x].name = "lhz";
            ASMDef[x].opCode = 0xA0000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 0;
            ASMDef[x].type = typeOFI;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Load Halfword And Zero (lhz)";
            x++;

            asmHelp = "Loads the halfword from the address (rA + rB) and stores it into lower 16 bits of rD\nThe remaining bits are cleared" + nl;
            asmHelp += "lhzx rD, rA, rB :: rD = (short)MEM(rA + rB)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "lhzx r3, r4, r5" + nl;
            ASMDef[x].name = "lhzx";
            ASMDef[x].opCode = 0x7C00022E;
            ASMDef[x].opShift = new int[] { 6, 10 };
            ASMDef[x].shifts = new int[3] { 21, 16, 11 };
            ASMDef[x].order = new int[3] { 0, 1, 2 };
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Load Halfword And Zero Indexed (lhzx)";
            x++;

            asmHelp = "Sets the lower part of the register rD to the 16-bit signed value IMM" + nl;
            asmHelp += "Result is similar to addis" + nl;
            asmHelp += "lis rD, IMM :: rD = (IMM << 16)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "lis r3, 0x1234" + nl;
            ASMDef[x].name = "lis";
            ASMDef[x].opCode = 0x3C000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeIMM;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Load Immediate Shifted (lis)";
            x++;

            asmHelp = "Sets the upper part of the register rD to the 16-bit signed value IMM" + nl;
            asmHelp += "Result is similar to addi" + nl;
            asmHelp += "li rD, IMM :: rD = IMM" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "li r3, 0x1234" + nl;
            ASMDef[x].name = "li";
            ASMDef[x].opCode = 0x38000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeIMM;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Load Immediate (li)";
            x++;

            asmHelp = "Loads the word from the address (IMM + rA) and stores it into rD" + nl;
            asmHelp += "lwzx rD, rA, rB :: rD = (int)MEM(rA + rB)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "lwzx r3, r4, r3" + nl;
            ASMDef[x].name = "lwzx";
            ASMDef[x].opCode = 0x7C00002E;
            ASMDef[x].opShift = new int[] { 6, 10 };
            ASMDef[x].shifts = new int[3] { 21, 16, 11 };
            ASMDef[x].order = new int[3] { 0, 1, 2 };
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Load Word And Zero Indexed (lwzx)";
            x++;

            asmHelp = "Loads the word from the address (IMM + rA) and stores it into rD" + nl;
            asmHelp += "lwz rD, IMM(rA) :: rD = (int)MEM(IMM + rA)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "lwz r3, 0x4058(r4)" + nl;
            ASMDef[x].name = "lwz";
            ASMDef[x].opCode = 0x80000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 0;
            ASMDef[x].type = typeOFI;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Load Word And Zero (lwz)";
            x++;

            InsPlaceArr[(char)'M' - (char)'A'] = (int)x;
            /* ---------- mfspr ---------- */
            asmHelp = "The contents of SPR are placed into rD" + nl;
            asmHelp += "mfspr rD, SPR :: rD = SPR" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "mfspr r0, LR" + nl;
            ASMDef[x].name = "mfspr";
            ASMDef[x].opCode = 0x7C0002A6;
            ASMDef[x].opShift = new int[] { 6, 14 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 16; ASMDef[x].shifts[1] = 21;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 1; ASMDef[x].order[1] = 0;
            ASMDef[x].type = typeSPR;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Move From Special Purpose Register (mfspr)";
            x++;

            asmHelp = "The contents of rA are placed into SPR" + nl;
            asmHelp += "mtspr SPR, rA :: SPR = rA" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "mtspr LR, r0" + nl;
            ASMDef[x].name = "mtspr";
            ASMDef[x].opCode = 0x7C0003A6;
            ASMDef[x].opShift = new int[] { 6, 14 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 16; ASMDef[x].shifts[1] = 21;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeSPR;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Move To Special Purpose Register (mtspr)";
            x++;

            asmHelp = "Multiplies the 32 bits of rA and rB and stores the resulting 64 bits into rD" + nl;
            asmHelp += "mullw rD, rA, rB :: rD = (long)(rA * rB)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "mullw r3, r3, r3" + nl;
            ASMDef[x].name = "mullw";
            ASMDef[x].opCode = 0x7C0001D6;
            ASMDef[x].opShift = new int[] { 6, 10 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 21; ASMDef[x].shifts[1] = 16; ASMDef[x].shifts[2] = 11;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Multiply Low Word (mullw)";
            x++;

            asmHelp = "Multiplies the 64 bits of rA and IMM and stores the resulting 64 bits into rD" + nl;
            asmHelp += "mulli rD, rA, IMM :: rD = (long)(rA * IMM)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "mulli r3, r3, 3" + nl;
            ASMDef[x].name = "mulli";
            ASMDef[x].opCode = 0x1C000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 21; ASMDef[x].shifts[1] = 16;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeIMM;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Multiply Low Immediate (mulli)";
            x++;

            InsPlaceArr[(char)'N' - (char)'A'] = (int)x;
            /* ---------- nop ---------- */
            asmHelp = "Does nothing (disassembled as ori, r0, r0, 0)" + nl;
            asmHelp += "Operation:" + nl;
            asmHelp += "nop :: r0 = r0 | 0" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "nop" + nl;
            ASMDef[x].name = "nop";
            ASMDef[x].opCode = 0x60000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].type = typeNOP;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "No Operation (nop)";
            x++;

            InsPlaceArr[(char)'O' - (char)'A'] = (int)x;
            /* ---------- ori ---------- */
            asmHelp = "Logical or's rA with IMM and stores result in rD" + nl;
            asmHelp += "ori rD, rA, IMM :: rD = rA | IMM" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "ori r3, r3, 0x1300" + nl;
            ASMDef[x].name = "ori";
            ASMDef[x].opCode = 0x60000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 21; ASMDef[x].shifts[1] = 16;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 1; ASMDef[x].order[1] = 0; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeIMM;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Logical Or Immediate (ori)";
            x++;

            asmHelp = "Logical or's rA with rB and stores result in rD" + nl;
            asmHelp += "or rD, rA, rB :: rD = rA | rB" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "or r3, r3, r4" + nl;
            ASMDef[x].name = "or";
            ASMDef[x].opCode = 0x7C000378;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 21; ASMDef[x].shifts[1] = 16; ASMDef[x].shifts[2] = 11;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 1; ASMDef[x].order[1] = 0; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Logical Or (or)";
            x++;

            asmHelp = "Logical or's rA with IMM that has been shifted over 16 bits and stores result in rD" + nl;
            asmHelp += "oris rD, rA, rB :: rD = (rA | IMM) << 16" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "oris r3, r4, 0x1234" + nl;
            ASMDef[x].name = "oris";
            ASMDef[x].opCode = 0x64000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 21; ASMDef[x].shifts[1] = 16;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 1; ASMDef[x].order[1] = 0; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeIMM;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Logical Or Immediate Shifted (oris)";
            x++;

            //InsPlaceArr[(char)'S' - (char)'A'] = (int)x;
            /* ---------- sc ---------- */

            asmHelp = "Shifts the double-word (64 bits) rA left by IMM bits and stores the result in rD" + nl;
            asmHelp += "sldi rD, rA, rB :: rD = (long)(rA << IMM)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "sldi r3, r27, 2" + nl;
            ASMDef[x].name = "sldi";
            ASMDef[x].opCode = 0x78000004;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[] { 21, 16, 11 };
            ASMDef[x].order = new int[] { 1, 0, 2 };
            ASMDef[x].type = typeIMM5;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Shift Left Double-Word Immediate (sldi)";
            x++;

            asmHelp = "Shifts the double-word (64 bits) rA right by IMM bits and stores the result in rD. Then" + nl;
            asmHelp += "srdi rD, rA, rB :: rD = (long)(rA >> IMM)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "srdi r3, r27, 2" + nl;
            ASMDef[x].name = "srdi";
            ASMDef[x].opCode = 0x78000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[] { 21, 16, 11 };
            ASMDef[x].order = new int[] { 1, 0, 2 };
            ASMDef[x].type = typeIMM5;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Shift Right Double-Word Immediate (srdi)";
            x++;

            asmHelp = "Shifts the word (32 bits) rA left by IMM bits and stores the result in rD" + nl;
            asmHelp += "slwi rD, rA, rB :: rD = (int)(rA << IMM)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "slwi r3, r27, 2" + nl;
            ASMDef[x].name = "slwi";
            ASMDef[x].opCode = 0x54000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[] { 21, 16, 11 };
            ASMDef[x].order = new int[] { 1, 0, 2 };
            ASMDef[x].type = typeIMM5;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Shift Left Word Immediate (slwi)";
            x++;

            asmHelp = "Shifts the word (32 bits) rA right by IMM bits and stores the result in rD" + nl;
            asmHelp += "srwi rD, rA, rB :: rD = (int)(rA >> IMM)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "srwi r3, r4, 2" + nl;
            ASMDef[x].name = "srwi";
            ASMDef[x].opCode = 0x54000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[] { 21, 16, 6 };
            ASMDef[x].order = new int[] { 1, 0, 2 };
            ASMDef[x].type = typeIMM5;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Shift Right Word Immediate (srwi)";
            x++;

            asmHelp = "Shifts the word (32 bits) rA left by the last 5 bits of rB and stores the result in rD" + nl;
            asmHelp += "slw rD, rA, rB :: rD = (int)(rA << rB)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "slw r3, r4, r3" + nl;
            ASMDef[x].name = "slw";
            ASMDef[x].opCode = 0x7C000030;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[] { 21, 16, 11 };
            ASMDef[x].order = new int[] { 1, 0, 2 };
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Shift Left Word (slw)";
            x++;

            asmHelp = "Shifts the word (32 bits) rA right by the last 5 bits of rB and stores the result in rD" + nl;
            asmHelp += "srw rD, rA, rB :: rD = (int)(rA >> rB)" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "srw r3, r4, r3" + nl;
            ASMDef[x].name = "srw";
            ASMDef[x].opCode = 0x7C000430;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[] { 21, 16, 11 };
            ASMDef[x].order = new int[] { 1, 0, 2 };
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Shift Right Word (srw)";
            x++;

            asmHelp = "Stores the byte rS at the address (IMM + rB)" + nl;
            asmHelp += "stb rS, IMM(rB) :: MEM(IMM + rB) = (char)rS" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "stb r4, 0x0054(r5)" + nl;
            ASMDef[x].name = "stb";
            ASMDef[x].opCode = 0x98000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 0;
            ASMDef[x].type = typeOFI;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Store Byte (stb)";
            x++;

            asmHelp = "Stores the byte rS at the address (rA + rB)" + nl;
            asmHelp += "stbx rS, rA, rB :: MEM(rA + rB) = (char)rS" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "stbx r4, r5, r3" + nl;
            ASMDef[x].name = "stbx";
            ASMDef[x].opCode = 0x7C0001AE;
            ASMDef[x].opShift = new int[] { 6, 10 };
            ASMDef[x].shifts = new int[3] { 21, 16, 11 };
            ASMDef[x].order = new int[3] { 0, 1, 2 };
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Store Byte Indexed (stbx)";
            x++;

            asmHelp = "Stores the doubleword (64 bits) rS at the address (IMM + rB). rB is then set to (IMM + rB)" + nl;
            asmHelp += "stdu rS, IMM(rB) :: MEM(IMM + rB) = (long)rS; rB += IMM" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "stdu r4, 0x0054(r5)" + nl;
            ASMDef[x].name = "stdu";
            ASMDef[x].opCode = 0xF8000001;
            ASMDef[x].opShift = new int[] { 6, 1 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 0;
            ASMDef[x].type = typeOFI;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Store Doubleword And Update (stdu)";
            x++;

            asmHelp = "Stores the word (64 bits) rS at the address (rA + rB)" + nl;
            asmHelp += "stdx rS, rA, rB :: MEM(rA + rB) = (long)rS" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "stdx r3, r4, r3" + nl;
            ASMDef[x].name = "stdx";
            ASMDef[x].opCode = 0x7C00012A;
            ASMDef[x].opShift = new int[] { 6, 10 };
            ASMDef[x].shifts = new int[3] { 21, 16, 11 };
            ASMDef[x].order = new int[3] { 0, 1, 2 };
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Store Doubleword Indexed (stdx)";
            x++;

            asmHelp = "Stores the doubleword (64 bits) rS at the address (IMM + rB)" + nl;
            asmHelp += "std rS, IMM(rB) :: MEM(IMM + rB) = (long)rS" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "std r4, 0x0054(r5)" + nl;
            ASMDef[x].name = "std";
            ASMDef[x].opCode = 0xF8000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 0;
            ASMDef[x].type = typeOFI;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Store Doubleword (std)";
            x++;

            asmHelp = "Converts the float in frS to a single and stores it at (IMM + rB)" + nl;
            asmHelp += "stfs frS, IMM(rB) :: MEM(IMM + rB) = (Single)frS" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "stfs f1, 0x0054(r5)" + nl;
            ASMDef[x].name = "stfs";
            ASMDef[x].opCode = 0xD0000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 0;
            ASMDef[x].type = typeFOFI;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Store Floating Point Single (stfs)";
            x++;

            asmHelp = "Store the double from (IMM + rB) and stores it in frA" + nl;
            asmHelp += "stfd frA, IMM(rB) :: MEM(IMM + rB) = (double)frA" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "stfd f1, 0x0054(r5)" + nl;
            ASMDef[x].name = "stfd";
            ASMDef[x].opCode = 0xD8000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3] { 21, 16, 0 };
            ASMDef[x].order = new int[3] { 0, 1, 2 };
            ASMDef[x].type = typeFOFI;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Store Floating Point Doubleword (stfd)";
            x++;

            asmHelp = "Stores the lower 16 bits of rS at the address (IMM + rA)" + nl;
            asmHelp += "sth rS, IMM(rA) :: MEM(IMM + rA) = (short)rS" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "sth r14, 0x0020(r1)" + nl;
            ASMDef[x].name = "sth";
            ASMDef[x].opCode = 0xB0000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 0;
            ASMDef[x].type = typeOFI;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Store Halfword (sth)";
            x++;

            asmHelp = "Stores the lower 16 bits of rS at the address (rA + rB)" + nl;
            asmHelp += "sthx rS, rA, rB :: MEM(rA + rB) = (short)rS" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "sthx r14, r1, r4" + nl;
            ASMDef[x].name = "sthx";
            ASMDef[x].opCode = 0x7C00032E;
            ASMDef[x].opShift = new int[] { 6, 10 };
            ASMDef[x].shifts = new int[3] { 21, 16, 11 };
            ASMDef[x].order = new int[3] { 0, 1, 2 };
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Store Halfword Indexed (sthx)";
            x++;

            asmHelp = "Stores the word (32 bits) rS at the address (rA + rB)" + nl;
            asmHelp += "stwx rS, rA, rB :: MEM(rA + rB) = (int)rS" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "stwx r3, r4, r3" + nl;
            ASMDef[x].name = "stwx";
            ASMDef[x].opCode = 0x7C00012E;
            ASMDef[x].opShift = new int[] { 6, 10 };
            ASMDef[x].shifts = new int[3] { 21, 16, 11 };
            ASMDef[x].order = new int[3] { 0, 1, 2 };
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Store Word Indexed (stwx)";
            x++;

            asmHelp = "Stores the word (32 bits) rS at the address (IMM + rB)" + nl;
            asmHelp += "stw rS, IMM(rB) :: MEM(IMM + rB) = (int)rS" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "stw r4, 0x0054(r5)" + nl;
            ASMDef[x].name = "stw";
            ASMDef[x].opCode = 0x90000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 0;
            ASMDef[x].type = typeOFI;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Store Word (stw)";
            x++;

            asmHelp = "Stores the word (32 bits) rS at the address (IMM + rB). rB is then set to (IMM + rB)" + nl;
            asmHelp += "stwu rS, IMM(rB) :: MEM(IMM + rB) = (int)rS; rB += IMM" + nl;
            asmHelp += nl + "Example:" + nl;
            asmHelp += "stwu r4, 0x0054(r5)" + nl;
            ASMDef[x].name = "stwu";
            ASMDef[x].opCode = 0x94000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 0;
            ASMDef[x].type = typeOFI;
            ASMDef[x].help = asmHelp;
            ASMDef[x].title = "Store Word And Update (stwu)";
            x++;

        }

        public static int GetInsStart(int charOff)
        {
            charOff -= (char)'A';
            if (charOff > InsPlaceArr.Length)
                charOff -= 0x20;
            if (charOff < 0 || charOff >= InsPlaceArr.Length)
                return 0;
            return InsPlaceArr[charOff];
        }

        /* ---------- Help strings ---------- */

        /*
            Format {            
                Description:
                    Adds a register and a sign-extended immediate value and stores the 
                    result in a register
                Operation:
                    $t = $s + imm
                Syntax:
                    addi $t, $s, imm
            }
         */

        public static void DeclareHelpStr()
        {
            int x = 0;
            string nl = "\n"; /* newline */

            /* Command Help String */
            x = 0;

            //address
            helpCom[x] += "Set the current address of the subroutine" + nl;
            helpCom[x] += "The address will automatically be incremented by 4 on each instruction" + nl;
            helpCom[x] += nl + "address 0x003D44C8" + nl;
            x++;
            //hook
            helpCom[x] += "Set the current hook address of the subroutine (b). At the end of the compiled subroutine there will be a line with the address of the hook and a b to the address" + nl;
            helpCom[x] += "There can only be one hook address at this point" + nl;
            helpCom[x] += nl + "hook 0x0174DC68" + nl;
            x++;
            //hookl
            helpCom[x] += "Set the current hook address of the subroutine (bl). At the end of the compiled subroutine there will be a line with the address of the hook and a bl to the address" + nl;
            helpCom[x] += "There can only be one hook address at this point" + nl;
            helpCom[x] += nl + "hookl 0x0174DC68" + nl;
            x++;
            //setreg
            helpCom[x] += "Sets rD to the 32 bit unsigned IMM" + nl;
            helpCom[x] += "setreg rD, IMM :: rD = IMM" + nl;
            helpCom[x] += nl + "setreg r3, 0x12345678  ->" + nl;
            helpCom[x] += "\tlis r3, 0x1234" + nl + "\tori r3, r3, 0x5678" + nl;
            x++;
            //hexcode
            helpCom[x] += "Inserts the value specified into the assembled code" + nl;
            helpCom[x] += "Useful for unsupported instructions" + nl;
            helpCom[x] += nl + "hexcode 0x12345678  ->" + nl;
            helpCom[x] += "\t2 00000000 12345678" + nl;
            x++;
            //import
            helpCom[x] += "Inserts the CWP3 file(s) specified into the assembly at compile time" + nl;
            helpCom[x] += "If a full path is not specified, the path will be that of the current cwp3 file" + nl;
            helpCom[x] += nl + "import Math.cwp3" + nl + "import C:\\CodeWizard PS3\\Math.cwp3" + nl;
            x++;
            //float
            helpCom[x] += "Inserts the float value specified into the assembled code as hex" + nl;
            helpCom[x] += nl + "float 3.14159  ->" + nl;
            helpCom[x] += "\t2 00000000 40490FD0" + nl;
            x++;
            //string
            helpCom[x] += "Inserts the string specified into the assembled code as hex" + nl;
            helpCom[x] += nl + "string ABCDE F  ->" + nl;
            helpCom[x] += "\t2 00000000 41424344" + nl;
            helpCom[x] += "\t2 00000004 45204600" + nl;
            x++;


            /* Register Help String */
            x = 0;

            //r0
            helpReg[x] += "More information on PPC registers: http://www.csd.uwo.ca/~mburrel/stuff/ppc-asm.html" + nl;
            helpReg[x] += "Used to hold the old link register when building the stack" + nl;
            helpReg[x] += nl + "It is best not to modify this register unless dealing with the stack" + nl;
            x++;
            //r1
            helpReg[x] += "More information on PPC registers: http://www.csd.uwo.ca/~mburrel/stuff/ppc-asm.html" + nl;
            helpReg[x] += "Points to the beginning of the stack" + nl;
            helpReg[x] += nl + "Only modify when dealing with the stack" + nl;
            x++;
            //r2
            helpReg[x] += "More information on Register 2: http://physinfo.ulb.ac.be/divers_html/powerpc_programming_info/intro_to_ppc/ppc4_runtime4.html" + nl;
            helpReg[x] += "A collection of pointers that the code uses to locate its static data" + nl;
            helpReg[x] += nl + "Do not modify this register" + nl;
            x++;
            //r3
            helpReg[x] += "More information on PPC registers: http://www.csd.uwo.ca/~mburrel/stuff/ppc-asm.html" + nl;
            helpReg[x] += "Commonly used as the return value of a function, and also the first argument" + nl;
            helpReg[x] += nl + "Free to be used" + nl;
            x++;
            //r4 - r10
            helpReg[x] += "More information on PPC registers: http://www.csd.uwo.ca/~mburrel/stuff/ppc-asm.html" + nl;
            helpReg[x] += "Commonly used as function arguments 2 through 8" + nl;
            helpReg[x] += nl + "Free to be used" + nl;
            x++;
            //r11 - r12
            helpReg[x] += "More information on PPC registers: http://wiibrew.org/wiki/Assembler_Tutorial#Application_Binary_Interface_.28SVR4_ABI.29" + nl;
            helpReg[x] += "Used with general purpose operations" + nl;
            helpReg[x] += nl + "Free to be used" + nl;
            x++;
            //r13 - r31
            helpReg[x] += "More information on PPC registers: http://www.csd.uwo.ca/~mburrel/stuff/ppc-asm.html" + nl;
            helpReg[x] += "Used with general purpose operations" + nl;
            helpReg[x] += nl + "Free to be used, BUT, they must be preserved in the stack" + nl;
            x++;
            //f0 - f13
            helpReg[x] += "Used with floating point operations" + nl;
            helpReg[x] += nl + "Free to be used" + nl;
            x++;
            //f14 - f31
            helpReg[x] += "Used with floating point operations" + nl;
            helpReg[x] += nl + "Free to be used, BUT, they must be preserved in the stack" + nl;
            x++;
            //cr0 - cr7
            helpReg[x] += "Hold results of comparisons" + nl;
            helpReg[x] += nl + "Free to be used, BUT, they must be preserved in the stack" + nl;
            helpReg[x] += "See the instructions cmpwi, cmplwi, cmpw, and cmplw for usage" + nl;
            x++;
            //XER
            helpReg[x] += "Overflows and Exception stuff" + nl;
            helpReg[x] += "For more info search for the section \"XER Register (XER)\" at http://www.cebix.net/downloads/bebox/pem32b.pdf" + nl;
            helpReg[x] += "Do not modify this special register" + nl;
            x++;
            //LR
            helpReg[x] += "Holds return address when a linking branch is called" + nl;
            helpReg[x] += nl + "To be used exclusively for function calling" + nl;
            x++;
            //CTR
            helpReg[x] += "Holds a loop count that can be decremented during execution of branch instructions" + nl;
            helpReg[x] += nl + "For more info search for the section \"Count Register (CTR)\" at http://www.cebix.net/downloads/bebox/pem32b.pdf" + nl;
            x++;

            /* Term Help String */
            x = 0;

            //rA
            helpTerm[x] = "rA - Register A" + nl;
            helpTerm[x] += "First non-destination register in an assembly code" + nl;
            x++;
            //rB
            helpTerm[x] = "rB - Register B" + nl;
            helpTerm[x] += "Second non-destination register in an assembly code" + nl;
            x++;
            //rD
            helpTerm[x] = "rD - Destination Register" + nl;
            helpTerm[x] += "First destination register in an assembly code" + nl;
            x++;
            //rS
            helpTerm[x] = "rS - Source Register" + nl;
            helpTerm[x] += "First source register in an assembly code" + nl;
            x++;
            //BF
            helpTerm[x] = "BF - Conditional Register" + nl;
            helpTerm[x] += "Conditional register used in the following instructions:" + nl;
            helpTerm[x] += "beq, bne, ble, blt, bgt, bge, cmpwi, cmplwi, cmpw, cmplw" + nl;
            x++;
            //IMM
            helpTerm[x] = "IMM - Immediate Value" + nl;
            helpTerm[x] += "A constant value with a bit length defined by the instruction" + nl;
            x++;
            //Signed
            helpTerm[x] = "Signed" + nl;
            helpTerm[x] += "Defines that the value can be negative" + nl;
            x++;
            //Signed
            helpTerm[x] = "Unsigned" + nl;
            helpTerm[x] += "Defines that the value cannot be negative" + nl;
            helpTerm[x] += "An unsigned 32-bit value of 0xFFFFFFFF is equal to 4294967295";
            helpTerm[x] += " while a signed 32-bit value of 0xFFFFFFFF is equal to -1" + nl;
            x++;
        }

        #endregion

        public static string DisAsmPseudoOps(string code, Main.pInstruction[] pIns)
        {
            string[] lines = code.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            for (int x = 0; x < pIns.Length; x++)
            {
                string[] pLines = pIns[x].asm.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                List<int> inds = FindPInsIndex(lines, pIns[x]);

                foreach (int index in inds)
                {
                    //Add regs to hash
                    Hashtable hash = new Hashtable();
                    int cnt = 0;
                    foreach (string str in pIns[x].regs)
                    {
                        //find reg in asm
                        int off = 0;
                        for (off = 0; off < pLines.Length; off++)
                        {
                            if (pLines[off].IndexOf(str) >= 0)
                            {
                                string[] offArgs = pLines[off].Split(new char[] { ' ', '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);
                                for (cnt = 0; cnt < offArgs.Length; cnt++)
                                    if (offArgs[cnt] == str)
                                        break;
                                break;
                            }
                        }

                        if (off < pLines.Length && (off + index) < lines.Length)
                        {
                            string[] args = lines[off + index].Split(new char[] { ' ', '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);
                            hash.Add(str, args[cnt]);
                        }
                    }

                    //check if assembly matches
                    int lineSize = 0;
                    for (int lineCnt = 0; (lineCnt + index) < lines.Length; lineCnt++)
                    {
                        if (lineCnt >= pLines.Length)
                            break;
                        string pAsm = pLines[lineCnt];
                        foreach (string reg in pIns[x].regs)
                            pAsm = pAsm.Replace(reg, (string)hash[reg]);

                        if (ParsePAsm(pAsm) != lines[lineCnt + index])
                            break;

                        lineSize++;
                    }

                    lines[index] = pIns[x].name;
                    foreach (string reg in pIns[x].regs)
                        lines[index] += " " + (string)hash[reg] + ",";
                    lines[index] = lines[index].Trim(new char[] { ' ', ',' });

                    for (int linex = 1; linex < lineSize; linex++)
                        lines[index + linex] = "-";
                }
            }

            string ret = "";
            foreach (string line in lines)
            {
                if (line != "-")
                {
                    ret += line + Environment.NewLine;
                }
            }

            return ret;
        }

        static string ParsePAsm(string pAsm)
        {
            string[] args = pAsm.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int x = 0; x < args.Length; x++)
            {
                try
                {
                    string pre = "", post = "";

                    string[] immSplit = args[x].Split('(');
                    if (immSplit.Length > 1)
                        post = "(" + immSplit[1];
                    pre = immSplit[0];

                    int val = 0;
                    if (args[x].IndexOf("0x") >= 0)
                        val = int.Parse(pre, System.Globalization.NumberStyles.HexNumber);
                    else
                        val = int.Parse(pre);

                    args[x] = "0x" + val.ToString("X4") + post;
                }
                catch (Exception e)
                {
                    //Not valid
                }
            }

            string ret = args[0];
            for (int c = 1; c < args.Length; c++)
                ret += " " + args[c];

            return ret.Trim(' ');
        }

        static List<int> FindPInsIndex(string[] lines, Main.pInstruction pIns)
        {
            List<int> ret = new List<int>();
            string[] pLines = RemoveComments(pIns.asm).Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (pLines.Length > 0)
            {
                string[] firstLine = pLines[0].Split(' ');
                for (int x = 0; x < lines.Length; x++)
                {
                    if (lines[x].Split(' ')[0] == firstLine[0])
                    {
                        ret.Add(x);
                        for (int y = 1; y < pLines.Length; y++)
                        {
                            if (x + y >= lines.Length)
                            {
                                ret.RemoveAt(ret.Count - 1);
                                break;
                            }

                            if (lines[x + y].Split(' ')[0] != pLines[y].Split(' ')[0])
                            {
                                ret.RemoveAt(ret.Count - 1);
                                break;
                            }
                        }
                    }
                }
            }


            return ret;
        }

    }
}
