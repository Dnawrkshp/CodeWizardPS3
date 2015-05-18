using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;

namespace CodeWizardPS3
{
    class ASMDeclarations
    {
        
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
        public const int typeFOFI = 10;          /* Float - Offset Immediate */

        /* Register color */
        public struct RegCol
        {
            public string reg;                  /* Name of register */
            public Color col;                   /* Color of register */
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
        };
            
        public static PPCInstr[] ASMDef = new PPCInstr[100];
        public static RegCol[] RegColArr = new RegCol[75];
        public static int[] InsPlaceArr = new int[26];
        public static string[] helpStr = new string[100];
        public static string[] helpCom = new string[8];
        public static string[] helpReg = new string[13];
        public static string[] helpTerm = new string[7];

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
        static string[] importPaths = new string[0];
        static int impOffset = 0;

        /* Disassembly */
        static uint DisASMAddr = 0;
        static ASMLabel[] DisASMLabel = new ASMLabel[0];
        static bool addr = true;
        public static int ASMDisMode = 0;

        /* ---------- Assemble ---------- */
        public static string ASMAssemble(RichTextBox ASMBox, RichTextBox DefBox, int outputType, string GlobalFileName)
        {
            string CodeBoxRet = "";
            importPaths = new string[0];
            importLoaded = new bool[0];
            impOffset = 0;
            CompAddress = 0;
            FirstCompAddr = 0;
            HookMode = 0;
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

            /* Remove comments */
            var re = @"(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')|//.*|/\*(?s:.*?)\*/";

            /* This is a lot faster than using Split */
            RichTextBox rtb = new RichTextBox();
            rtb.Text = Regex.Replace(ASMBox.Text, re, "$1");
            /* Append any imported files */
            rtb = GetImports(rtb, GlobalFileName);
            rtb.Text = Regex.Replace(rtb.Text, re, "$1");
            
            /* Parse any custom pseudo instructions */
            bool cpiReset = false;
            for (int cpi = 0; cpi < Main.customPIns.Length; cpi++)
            {
                int cpiOff = rtb.Text.ToLower().IndexOf(Main.customPIns[cpi].name.ToLower() + " ", 0);
                    //rtb.Find(Main.customPIns[cpi].name + " ", 0, RichTextBoxFinds.None);
                if (cpiOff != 0)
                {
                    cpiOff = rtb.Text.ToLower().IndexOf("\r" + Main.customPIns[cpi].name.ToLower() + " ", 0);
                    if (cpiOff < 0)
                        cpiOff = rtb.Text.ToLower().IndexOf("\n" + Main.customPIns[cpi].name.ToLower() + " ", 0);
                }
                        //rtb.Find("\r" + Main.customPIns[cpi].name + " ", 0, RichTextBoxFinds.None);

                while (cpiOff >= 0 || cpiReset)
                {
                    if (cpiReset)
                    {
                        cpi = 0;
                        cpiOff = rtb.Text.ToLower().IndexOf(Main.customPIns[cpi].name.ToLower() + " ", 0);
                            //rtb.Find(Main.customPIns[cpi].name + " ", 0, RichTextBoxFinds.None);
                        if (cpiOff != 0)
                            cpiOff = rtb.Text.ToLower().IndexOf(Main.customPIns[cpi].name.ToLower() + " ", 0);
                                //rtb.Find("\r" + Main.customPIns[cpi].name + " ", 0, RichTextBoxFinds.None);
                        if (cpiOff < 0)
                        {
                            cpiReset = false;
                            goto nextCPI;
                        }
                    }
                    cpiReset = true;

                    int rtbFind = rtb.Find("\r", cpiOff + 1, RichTextBoxFinds.None);
                    string[] line = Main.sMid(rtb.Text, cpiOff + 1, rtbFind - cpiOff).Split(' ');
                    string[] args = new string[Main.customPIns[cpi].regs.Length];
                    if ((line.Length - 1) != args.Length)
                    {
                        //debugString += "Invalid number of arguments for \"" + rtb.Lines[rtb.GetLineFromCharIndex(cpiOff + cpiInc)] + "\"";
                        cpiReset = false;
                        goto skipCPI;
                    }

                    for (int argsCnt = 0; argsCnt < args.Length; argsCnt++)
                        args[argsCnt] = line[argsCnt+1].Replace(",", "").Replace("\n", "");

                    string newline = Main.customPIns[cpi].asm;
                    for (int nlCnt = 0; nlCnt < Main.customPIns[cpi].regs.Length; nlCnt++)
                        newline = newline.Replace(Main.customPIns[cpi].regs[nlCnt], args[nlCnt]);

                    rtb.Text = rtb.Text.Replace(Main.sMid(rtb.Text, cpiOff + 1, rtbFind - cpiOff), newline + "\n");

                skipCPI: ;
                    cpiOff = rtb.Text.ToLower().IndexOf("\r" + Main.customPIns[cpi].name.ToLower() + " ", cpiOff + 1);
                        //rtb.Find("\r" + Main.customPIns[cpi].name + " ", cpiOff + 1, RichTextBoxFinds.None);
                }
            nextCPI: ;
            }
            //Remove imported comments...
            rtb.Text = Regex.Replace(rtb.Text, re, "$1");

            //Add all imports
            rtb.Text += "\n/*\n" + importDef + "\n*/\n";

            labels = ProcessLabels(rtb.Lines);
            if (labels == null)
            {
                //Main.DebugMenu(debugString);
                return "";
            }
            //Remove importDef
            rtb.Text = Regex.Replace(rtb.Text, re, "$1");
            string[] ASMArray = rtb.Lines;
            string retStr = "";

            for (int cnt = 0; cnt < ASMArray.Length; cnt++)
            {
                /* Skip blanks */
                while (cnt < ASMArray.Length && ASMArray[cnt] == "")
                    cnt++;
                if (cnt >= ASMArray.Length)
                    break;

                retStr = ASMToHex(ASMArray[cnt]);
                if (retStr != null && retStr != "")
                {
                    string outStr = "";
                    string[] outCode;
                    switch (outputType)
                    {
                        case 0: //NetCheat PS3
                            CodeBoxRet += "2 " + retStr + Environment.NewLine;
                            break;
                        case 1: //Hex String Array
                            outCode = retStr.Split(' ');
                            for (int x = 1; x < outCode.Length; x += 2)
                            {
                                outStr = Main.sLeft(outCode[x], 8);
                                outStr = outStr.Insert(2, " ");
                                outStr = outStr.Insert(5, " ");
                                outStr = outStr.Insert(8, " ");
                                outStr = outStr.Insert(11, " ");

                                CodeBoxRet += outStr;
                            }
                            break;
                        case 2: //Byte Array
                            outCode = retStr.Split(' ');
                            for (int x = 1; x < outCode.Length; x += 2)
                            {
                                outStr = Main.sLeft(outCode[x], 8);
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

            if (importDef != "")
            {
                int defParseCnt = 0;
                string[] lines = Regex.Replace(importDef, re, "$1").Split('\n');
                foreach (string line in lines)
                {
                    string hex = null;
                    if (line != "")
                        hex = ASMToHex(line);
                    if (hex != null)
                    {
                        switch (outputType)
                        {
                            case 0: //NetCheat
                                CodeBoxRet += "2 " + hex + Environment.NewLine;
                                break;
                            case 1: //Hex String Array
                                hex = Main.sRight(hex, 8);
                                CodeBoxRet += hex[0] + hex[1] + " " +
                                                hex[2] + hex[3] + " " +
                                                hex[4] + hex[5] + " " +
                                                hex[6] + hex[7] + " ";
                                break;
                            case 2: //Byte Array
                                hex = Main.sRight(hex, 8);
                                if (defParseCnt == 0)
                                    CodeBoxRet += "byte[] " + fileStr + "_decl = { 0x";
                                else
                                    CodeBoxRet += ", 0x";
                                CodeBoxRet += hex[0].ToString() + hex[1].ToString() + ", 0x" +
                                                hex[2].ToString() + hex[3].ToString() + ", 0x" +
                                                hex[4].ToString() + hex[5].ToString() + ", 0x" +
                                                hex[6].ToString() + hex[7].ToString();
                                break;
                        }
                        defParseCnt++;
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
                string hex = Main.sRight(ASMToHex(hook + " 0x" + FirstCompAddr.ToString("X8")), 8);
                switch (outputType)
                {
                    case 0: //NetCheat
                        CodeBoxRet += "2 " + CompHook.ToString("X8") + " " + hex;
                        break;
                    case 1: //Hex String Array
                        CodeBoxRet += Environment.NewLine + hex[0] + hex[1] + " " +
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

            return CodeBoxRet;
        }

        /* Gets the text from all the imported cwp3 files */
        public static RichTextBox GetImports(RichTextBox rtb, string filePath)
        {
            if (filePath == "")
                return rtb;
            RichTextBox[] retRTBArr = ReadImport(rtb, new FileInfo(filePath).Directory.FullName);
            bool contLoop = true;
            while (contLoop)
            {
                int skip = 0;
                contLoop = false;
                impOffset = retRTBArr.Length;
                retRTBArr = ParseImports(retRTBArr, skip);
                for (int x = impOffset; x < retRTBArr.Length; x++)
                    if (retRTBArr[x].Find("import", 0, RichTextBoxFinds.WholeWord) >= 0)
                        contLoop = true;
                skip = impOffset + 1;
            }
            
            //Append all the imported files together
            for (int x = 0; x < retRTBArr.Length; x++)
                rtb.Text += "\n" + retRTBArr[x].Text;

            rtb.Text += "\n";
            return rtb;
        }

        /* Parses each import in a RichTextBox and returns the results in a RichTextBox array */
        public static RichTextBox[] ParseImports(RichTextBox[] rtb, int skip)
        {
            RichTextBox[] tempRTBArr = (RichTextBox[])rtb.Clone();
            for (int z = skip; z < rtb.Length; z++)
            {
                int offVal = 0;
                while (offVal >= 0)
                {
                    offVal = rtb[z].Find("import", offVal + 1, RichTextBoxFinds.WholeWord);
                    if (offVal >= 0)
                    {
                        int off = tempRTBArr.Length;
                        RichTextBox[] tempRTB = ReadImport(rtb[z], (string)rtb[z].Tag);
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
        public static RichTextBox[] ReadImport(RichTextBox rtb, string GlobalFileName)
        {
            RichTextBox[] retRTB = new RichTextBox[0];

            if (GlobalFileName == null)
                return null;

            //Calculate each import
            int offImport = 0;

            while (offImport >= 0)
            {
                offImport = rtb.Text.ToLower().IndexOf("import", offImport + 1);
                if (offImport >= 0)
                {
                    int rtbFind = rtb.Find("\r", offImport, RichTextBoxFinds.None);
                    if (rtbFind < 0)
                        rtbFind = rtb.Text.Length;

                    string impStr = Main.sMid(rtb.Text, offImport, rtbFind - offImport).Split(' ')[1].Replace("\"", "");
                    if (impStr.IndexOf(':') < 0)
                        impStr = GlobalFileName + "\\" + impStr;
                    impStr = new FileInfo(impStr).FullName;
                    if (!isLoaded(impStr, importPaths.Length))
                    {
                        Array.Resize(ref importPaths, importPaths.Length + 1);
                        importPaths[importPaths.Length - 1] = impStr;
                    }
                }
            }

            RichTextBox rtfToPlain = new RichTextBox();

            //Import each file
            for (int x = impOffset; x < importPaths.Length; x++)
            {
                //importStr = importStr.Split(' ')[1].Replace("\"", "");
                //Parse file string
                if (importPaths[x].IndexOf(':') >= 0 && !isLoaded(importPaths[x], x)) //File is whole path
                {
                    if (System.IO.File.Exists(importPaths[x]))
                    {
                        Array.Resize(ref retRTB, retRTB.Length + 1);
                        retRTB[retRTB.Length - 1] = new RichTextBox();
                        string[] arr = FileIO.LoadCWP3(importPaths[x]);
                        if (arr.Length >= 1)
                            retRTB[retRTB.Length - 1].LoadFile(arr[0]);
                        if (arr.Length >= 2)
                        {
                            rtfToPlain.LoadFile(arr[1]);
                            importDef += rtfToPlain.Text + "\n";
                        }
                        importLoaded[x] = true;
                        retRTB[retRTB.Length - 1].Tag = new FileInfo(importPaths[x]).Directory.FullName;
                    }
                    else
                        debugString += "Imported file " + importPaths[x] + " doesn't exist!" + Environment.NewLine;
                }
                else if (GlobalFileName != "" && !isLoaded(importPaths[x], x)) //File is appended to GlobalFileName
                {
                    string impStr = new FileInfo(GlobalFileName + "\\" + importPaths[x]).FullName;
                    if (System.IO.File.Exists(impStr) && !isLoaded(impStr, x))
                    {
                        Array.Resize(ref retRTB, retRTB.Length + 1);
                        retRTB[retRTB.Length - 1] = new RichTextBox();
                        string[] arr = FileIO.LoadCWP3(impStr);
                        if (arr.Length >= 1)
                            retRTB[retRTB.Length - 1].LoadFile(arr[0]);
                        if (arr.Length >= 2)
                        {
                            rtfToPlain.LoadFile(arr[1]);
                            importDef += rtfToPlain.Text + "\n";
                        }
                        importLoaded[x] = true;
                        retRTB[retRTB.Length - 1].Tag = new FileInfo(impStr).Directory.FullName;
                    }
                    else if (System.IO.File.Exists(impStr))
                        debugString += "Imported file " + impStr + " doesn't exist!" + Environment.NewLine;
                }
                else if (GlobalFileName == "")
                    debugString += "You must save the file before importing other files!" + Environment.NewLine;

                offImport = rtb.Text.ToLower().IndexOf("import", offImport + 1);
            }

            return retRTB;
        }

        /* Tells whether the imported string has been loaded yet */
        public static bool isLoaded(string file, int ind)
        {
            if (importPaths.Length <= 0)
                return false;

            Array.Resize(ref importLoaded, importPaths.Length + 1);
            if (importLoaded[ind])
                return true;

            ind--;
            for (int x = ind; x >= 0; x--)
            {
                if (new FileInfo(importPaths[x]).FullName == new FileInfo(file).FullName)
                    return true;
            }

            return false;
        }

        /* Assembles ASM and returns hex string */
        public static string ASMToHex(string argASM)
        {

            string[] asmSplit = argASM.Trim().Split(' ');
            uint retVal = 0;

            if (asmSplit[0] == "" || asmSplit[0] == null)
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
                return null;
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
                    debugString += Environment.NewLine + "Too few arguments for setreg: \"" + argASM + "\"";
                    return null;
                }

                //Parse in case of label
                long val = ParseVal(asmSplit[2], 1);
                string upper = Main.sLeft(val.ToString("X8"), 4);
                string lower = Main.sRight(val.ToString("X8"), 4);

                string argPass = "lis " + asmSplit[1] + " 0x" + upper;
                string retStr = ASMToHex(argPass) + Environment.NewLine;
                argPass = "ori " + asmSplit[1] + " " + asmSplit[1] + " 0x" + lower;
                CompAddress += 4;
                retStr += "2 " + ASMToHex(argPass);
                return retStr;
            }
            else if (asmSplit[0].ToLower() == "hexcode")
            {
                return CompAddress.ToString("X8") + " " + ParseVal(asmSplit[1], 1).ToString("X8");
            }
            else if (asmSplit[0].ToLower() == "import")
                return null;
            else if (asmSplit[0].ToLower() == "float")
            {
                byte[] flt = BitConverter.GetBytes(float.Parse(asmSplit[1]));
                return CompAddress.ToString("X8") + " " + BitConverter.ToUInt32(flt, 0).ToString("X8");
            }
            else if (asmSplit[0].ToLower() == "string")
            {
                string fullStr = asmSplit[1];
                for (int fS = 2; fS < asmSplit.Length; fS++)
                    fullStr += " " + asmSplit[fS];
                byte[] flt = Main.StringToByteArray(fullStr);


                string retStr = "", prefix = "";
                int strCnt = 0;
                for (strCnt = 0; strCnt < flt.Length; strCnt++)
                {
                    if ((strCnt % 4) == 0)
                    {
                        retStr += prefix + CompAddress.ToString("X8") + " " + flt[strCnt].ToString("X2");
                        CompAddress += 4;
                    }
                    else
                        retStr += flt[strCnt].ToString("X2");

                    prefix = Environment.NewLine + "2 ";
                }
                strCnt = 4 - (flt.Length % 4);
                while (strCnt > 0 && strCnt < 4)
                {
                    strCnt--;
                    retStr += "00";
                }

                CompAddress -= 4;
                return retStr;
            }


            for (int x = ASMDeclarations.GetInsStart(asmSplit[0][0]); x < ASMDeclarations.ASMDef.Length; x++)
            {
                if (asmSplit[0] == ASMDeclarations.ASMDef[x].name && IsCorrectSize(ASMDeclarations.ASMDef[x].order, asmSplit.Length - 1, ASMDeclarations.ASMDef[x].type))
                {
                    if (ASMDeclarations.ASMDef[x].order == null)
                        return CompAddress.ToString("X8") + " " + ASMDeclarations.ASMDef[x].opCode.ToString("X8");

                    int[] regs = ParseRegs(asmSplit, ASMDeclarations.ASMDef[x].order.Length);
                    if (regs == null)
                    {
                        debugString += Environment.NewLine + "Error: Missing argument(s) in \"" + argASM + "\"";
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
                            int offset = (int)((regs[ASMDeclarations.ASMDef[x].order[y]] - CompAddress) / 4) << ASMDeclarations.ASMDef[x].shifts[y];
                            retVal |= (uint)(((UInt32)offset << 6) >> 6);
                            break;
                        case ASMDeclarations.typeOFI:
                        case ASMDeclarations.typeFOFI:
                        case ASMDeclarations.typeCND:
                        case ASMDeclarations.typeIMM:
                            for (y = 0; y < (ASMDeclarations.ASMDef[x].shifts.Length - 1); y++)
                                retVal |= (uint)(regs[ASMDeclarations.ASMDef[x].order[y]] << ASMDeclarations.ASMDef[x].shifts[y]);

                            retVal |= (UInt16)(regs[ASMDeclarations.ASMDef[x].order[y]] << ASMDeclarations.ASMDef[x].shifts[y]);
                            break;
                        case ASMDeclarations.typeBNCMP:
                            for (y = 0; y < (ASMDeclarations.ASMDef[x].shifts.Length - 1); y++)
                                retVal |= (uint)(regs[ASMDeclarations.ASMDef[x].order[y]] << ASMDeclarations.ASMDef[x].shifts[y]);

                            uint subVal = (ValState == vstLab) ? CompAddress : 0;
                            int cmpOff = (int)((regs[ASMDeclarations.ASMDef[x].order[y]] - subVal) / 4) << ASMDeclarations.ASMDef[x].shifts[y];
                            retVal |= (UInt16)cmpOff;
                            break;
                        case ASMDeclarations.typeIMM5:
                            for (y = 0; y < (ASMDeclarations.ASMDef[x].shifts.Length - 1); y++)
                                retVal |= (uint)(regs[ASMDeclarations.ASMDef[x].order[y]] << ASMDeclarations.ASMDef[x].shifts[y]);

                            retVal |= (UInt16)(regs[ASMDeclarations.ASMDef[x].order[y]] << ASMDeclarations.ASMDef[x].shifts[y]);

                            if (ASMDeclarations.ASMDef[x].name == "slwi")
                                retVal |= (UInt16)((31 - regs[ASMDeclarations.ASMDef[x].order[y]]) << 1);
                            else if (ASMDeclarations.ASMDef[x].name == "srwi")
                            {
                                retVal |= (UInt16)((32 - regs[ASMDeclarations.ASMDef[x].order[y]]) << 11);
                                retVal |= 31 << 1;
                            }

                            break;
                    }
                    break;
                }
            }
            if (retVal == 0)
            {
                if (asmSplit[0].ToLower()[asmSplit[0].Length - 1] != ':')
                    debugString += Environment.NewLine + "\"" + argASM + "\" is either missing argument(s) or is not valid";
                return null;
            }

            return CompAddress.ToString("X8") + " " + retVal.ToString("X8");
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

        /* Parses string into value */
        public static long ParseVal(string str, int mode)
        {
            bool neg = false;
            ValState = 0;

            if (str == "" || str == null)
                return 0;
            uint ret = 0;

            str = str.Replace(",", "");

            if (str.IndexOf(':') >= 0) //Label
            {
                ValState = vstLab;
                str = str.Replace(":", "");
                //Find label
                for (int x = 0; x < labels.Length; x++)
                {
                    if (str == labels[x].name)
                        return labels[x].address;
                }
                //MessageBox.Show("Error: Label (" + str + ":) is referred but does not exist");
                debugString += Environment.NewLine + "Error: Label (" + str + ":) is referred but does not exist";
                return 0;
            }

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
                    debugString += Environment.NewLine + "Error: Hexadecimal value: 0x" + str.ToUpper() + " is not a valid hexadecimal value";
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
                    debugString += Environment.NewLine + "Error: Decimal value: " + str.ToUpper() + " is not a valid decimal value";
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
                string regStr = str[x + 1].ToLower().Replace(",", "");
                //Label
                if (regStr.IndexOf(':') >= 0)
                {
                    retArray[x] = (int)ParseVal(str[x + 1], 1);
                }
                //Immediate Offset
                else if (regStr.IndexOf('(') >= 0)
                {
                    retArray[x] = ParseOffImm(regStr);
                    retArray[x + 1] = ParseRegsImm;
                }
                //Special register
                else if (regStr.IndexOf("xer") >= 0 ||
                    regStr.IndexOf("lr") >= 0 ||
                    regStr.IndexOf("ctr") >= 0)
                    retArray[x] = SPRegToDec(regStr);
                //Register
                else if (regStr.IndexOf('r') >= 0 || (regStr.IndexOf('f') == 0 && regStr.Length <= 3))
                    retArray[x] = RegToDec(regStr);
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
                    debugString += "Register " + oldReg + " is not a valid register!";
                    return 0;
                }
                return ret;
            }
            catch
            {
                debugString += "Register " + oldReg + " is not a valid register!";
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
                string[] strSplit = strArray[x].Split(' ');
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
                else if (index > 0 && strArray[x][index - 1] != ' ' && strArray[x][index - 1] != ',')
                {
                    Array.Resize(ref ret, ret.Length + 1);
                    ret[ret.Length - 1].address = tempAddr;
                    ret[ret.Length - 1].name = strArray[x].Replace(":", "");

                    //Check if label already exists
                    for (int y = 0; y < ret.Length; y++)
                    {
                        if (ret[y].name == ret[ret.Length - 1].name && y != (ret.Length - 1))
                        {
                            //MessageBox.Show("Error: Label (" + ret[ret.Length - 1].name + ") duplication");
                            debugString += Environment.NewLine + "Error: Label (" + ret[ret.Length - 1].name + ") duplication";
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

        /* Checks if string is an instruction */
        public static bool IsInstruction(string asm)
        {
            if (asm == "")
                return false;
            for (int x = 0; x < ASMDeclarations.ASMDef.Length; x++)
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

            for (x = 0; x < hexArr.Length; x++)
            {
                if (hexArr[x] != "")
                {
                    if (hexArr[x].Length <= 8 && hexArr[x] != "\n")
                        addr = false;
                    uint uintHex = 0;
                    hexArr[x] = hexArr[x].Replace(" ", "");
                    hexArr[x] = hexArr[x].Replace("\n", "");
                    string hexVal = Main.sRight(hexArr[x], 8);
                    string hexAddr = "";
                    if (addr) 
                        hexAddr = Main.sMid(hexArr[x], 1, 8);

                    //Check if a valid hex value
                    try
                    {
                        uintHex = uint.Parse(hexVal, System.Globalization.NumberStyles.HexNumber);
                    }
                    catch
                    {
                        goto asmDisNextX;
                    }

                    if (addr)
                    {
                        if (DisASMAddr != uint.Parse(hexAddr, System.Globalization.NumberStyles.HexNumber))
                        {
                            DisASMAddr = uint.Parse(hexAddr, System.Globalization.NumberStyles.HexNumber);
                            ret += "address 0x" + hexAddr + "\n" + "\n";
                        }
                    }

                    int InsIndex = GetInsFromHex(uintHex);
                    if (addr)
                        DisASMAddr += 4;
                    if (InsIndex < 0)
                        ret += "hexcode 0x" + hexVal + "\n";
                    else
                    {
                        string asmReg = DisASMReg(InsIndex, uintHex);
                        ret += ASMDef[InsIndex].name + " " + asmReg + "\n";
                    }
                }

            asmDisNextX: ;
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

                if (addr == false)
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

            switch (ASMDef[index].type) {
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
                        regs[ASMDef[index].order[x]] = "0x" + (labelAddr+4).ToString("X8");
                    break;
                case typeNOP:
                    return "";
                case typeOFI:
                case typeFOFI:
                    regs = new string[ASMDef[index].shifts.Length-1];


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
                    int BNCMPoff = 4 * int.Parse(GetStrFromBit(2, 15, val));
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
                    int cndReg = int.Parse(GetStrFromBit(ASMDef[index+1].shifts[0], 3, val));
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
                    regs = new string[ASMDef[index].shifts.Length];

                    for (x = 0; x < ASMDef[index].shifts.Length - 1; x++)
                        regs[ASMDef[index].order[x]] = preReg + GetStrFromBit(ASMDef[index].shifts[x], 5, val);

                    regs[ASMDef[index].order[x]] = GetStrFromBit(ASMDef[index].shifts[x], 5, val);
                    break;
        }

            for (x = 0; x < regs.Length-1; x++)
                ret += regs[x] + ", ";
            ret += regs[x];
            return ret;
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
        public static void DeclareInstructions()
        {
            ulong x = 0;

            /* Define registers */
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Firebrick;
            x++; //1
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.BurlyWood;
            x++; //2
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Chocolate;
            x++; //3
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Violet;
            x++; //4
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Turquoise;
            x++; //5
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Turquoise;
            x++; //6
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Turquoise;
            x++; //7
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Turquoise;
            x++; //8
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Turquoise;
            x++; //9
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Turquoise;
            x++; //10
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Turquoise;
            x++; //11
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Wheat;
            x++; //12
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Wheat;
            x++; //13
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Brown;
            x++; //14
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Crimson;
            x++; //15
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Crimson;
            x++; //16
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Crimson;
            x++; //17
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Crimson;
            x++; //18
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Crimson;
            x++; //19
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Crimson;
            x++; //20
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Crimson;
            x++; //21
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Crimson;
            x++; //22
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Crimson;
            x++; //23
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Crimson;
            x++; //24
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Crimson;
            x++; //25
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Crimson;
            x++; //26
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Crimson;
            x++; //27
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Crimson;
            x++; //28
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Crimson;
            x++; //29
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Crimson;
            x++; //30
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Crimson;
            x++; //31
            RegColArr[x].reg = "r" + x.ToString();
            RegColArr[x].col = Color.Crimson;
            x++; //cr0
            RegColArr[x].reg = "cr0";
            RegColArr[x].col = Color.LightSeaGreen;
            x++; //cr1
            RegColArr[x].reg = "cr1";
            RegColArr[x].col = Color.LightSeaGreen;
            x++; //cr2
            RegColArr[x].reg = "cr2";
            RegColArr[x].col = Color.LightSeaGreen;
            x++; //cr3
            RegColArr[x].reg = "cr3";
            RegColArr[x].col = Color.LightSeaGreen;
            x++; //cr4
            RegColArr[x].reg = "cr4";
            RegColArr[x].col = Color.LightSeaGreen;
            x++; //cr5
            RegColArr[x].reg = "cr5";
            RegColArr[x].col = Color.LightSeaGreen;
            x++; //cr6
            RegColArr[x].reg = "cr6";
            RegColArr[x].col = Color.LightSeaGreen;
            x++; //cr7
            RegColArr[x].reg = "cr7";
            RegColArr[x].col = Color.LightSeaGreen;
            x++;
            RegColArr[x].reg = "xer";
            RegColArr[x].col = Color.Cornsilk;
            x++;
            RegColArr[x].reg = "lr";
            RegColArr[x].col = Color.Cornsilk;
            x++;
            RegColArr[x].reg = "ctr";
            RegColArr[x].col = Color.Cornsilk;
            x++;
            int floatCnt = 0;
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr1
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr2
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr3
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr4
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr5
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr6
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr7
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr8
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr9
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr10
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr11
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr12
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr13
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr14
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr15
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr16
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr17
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr18
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr19
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr20
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr21
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr22
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr23
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr24
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr25
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr26
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr27
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr28
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr29
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr30
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
            x++; floatCnt++; //fr31
            RegColArr[x].reg = "f" + floatCnt.ToString();
            RegColArr[x].col = Color.Tan;
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

            InsPlaceArr[(char)'A' - (char)'A'] = (int)x;
            /* ---------- addis ---------- */
            ASMDef[x].name = "addis";
            ASMDef[x].opCode = 0x3C000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 21; ASMDef[x].shifts[1] = 16; ASMDef[x].shifts[2] = 0;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeIMM;
            ASMDef[x].help = helpStr[x];
            x++;

            /* addi */
            ASMDef[x].name = "addi";
            ASMDef[x].opCode = 0x38000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 21; ASMDef[x].shifts[1] = 16; ASMDef[x].shifts[2] = 0;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeIMM;
            ASMDef[x].help = helpStr[x];
            x++;

            /* add */
            ASMDef[x].name = "add";
            ASMDef[x].opCode = 0x7C000214;
            ASMDef[x].opShift = new int[] { 6, 10 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 21; ASMDef[x].shifts[1] = 16; ASMDef[x].shifts[2] = 11;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = helpStr[x];
            x++;

            InsPlaceArr[(char)'B' - (char)'A'] = (int)x;
            /* ---------- blr ---------- */
            ASMDef[x].name = "blr";
            ASMDef[x].opCode = 0x4E800020;
            ASMDef[x].opShift = new int[] { 9, 6 };
            ASMDef[x].type = typeNOP;
            ASMDef[x].help = helpStr[x];
            x++;

            /* bl */
            ASMDef[x].name = "bl";
            ASMDef[x].opCode = 0x48000001;
            ASMDef[x].opShift = new int[] { 6, 1 };
            ASMDef[x].shifts = new int[1];
            ASMDef[x].shifts[0] = 2;
            ASMDef[x].order = new int[1];
            ASMDef[x].order[0] = 0;
            ASMDef[x].type = typeBNC;
            ASMDef[x].help = helpStr[x];
            x++;

            /* beq */
            //cr0 is default bf
            ASMDef[x].name = "beq";
            ASMDef[x].opCode = 0x41820000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[1];
            ASMDef[x].shifts[0] = 2;
            ASMDef[x].order = new int[1];
            ASMDef[x].order[0] = 0;
            ASMDef[x].type = typeBNCMP;
            ASMDef[x].help = helpStr[x];
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
            ASMDef[x].help = helpStr[x];
            x++;

            /* bne */
            //cr0 is default bf
            ASMDef[x].name = "bne";
            ASMDef[x].opCode = 0x40820000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[1];
            ASMDef[x].shifts[0] = 2;
            ASMDef[x].order = new int[1];
            ASMDef[x].order[0] = 0;
            ASMDef[x].type = typeBNCMP;
            ASMDef[x].help = helpStr[x];
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
            ASMDef[x].help = helpStr[x];
            x++;

            /* ble */
            //cr0 is default bf
            ASMDef[x].name = "ble";
            ASMDef[x].opCode = 0x40810000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[1];
            ASMDef[x].shifts[0] = 2;
            ASMDef[x].order = new int[1];
            ASMDef[x].order[0] = 0;
            ASMDef[x].type = typeBNCMP;
            ASMDef[x].help = helpStr[x];
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
            ASMDef[x].help = helpStr[x];
            x++;

            /* bge */
            //cr0 is default bf
            ASMDef[x].name = "bge";
            ASMDef[x].opCode = 0x40800000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[1];
            ASMDef[x].shifts[0] = 2;
            ASMDef[x].order = new int[1];
            ASMDef[x].order[0] = 0;
            ASMDef[x].type = typeBNCMP;
            ASMDef[x].help = helpStr[x];
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
            ASMDef[x].help = helpStr[x];
            x++;

            /* bgt */
            //cr0 is default bf
            ASMDef[x].name = "bgt";
            ASMDef[x].opCode = 0x41810000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[1];
            ASMDef[x].shifts[0] = 2;
            ASMDef[x].order = new int[1];
            ASMDef[x].order[0] = 0;
            ASMDef[x].type = typeBNCMP;
            ASMDef[x].help = helpStr[x];
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
            ASMDef[x].help = helpStr[x];
            x++;

            /* blt */
            //cr0 is default bf
            ASMDef[x].name = "blt";
            ASMDef[x].opCode = 0x41800000;
            ASMDef[x].opShift = new int[] { 11, 0 };
            ASMDef[x].shifts = new int[1];
            ASMDef[x].shifts[0] = 2;
            ASMDef[x].order = new int[1];
            ASMDef[x].order[0] = 0;
            ASMDef[x].type = typeBNCMP;
            ASMDef[x].help = helpStr[x];
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
            ASMDef[x].help = helpStr[x];
            x++;

            /* b */
            ASMDef[x].name = "b";
            ASMDef[x].opCode = 0x48000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[1];
            ASMDef[x].shifts[0] = 2;
            ASMDef[x].order = new int[1];
            ASMDef[x].order[0] = 0;
            ASMDef[x].type = typeBNC;
            ASMDef[x].help = helpStr[x];
            x++;

            InsPlaceArr[(char)'C' - (char)'A'] = (int)x;
            /* ---------- cmpwi ---------- */
            //cr0 is default bf
            ASMDef[x].name = "cmpwi";
            ASMDef[x].opCode = 0x2C000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 16;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeCND;
            ASMDef[x].help = helpStr[x];
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
            ASMDef[x].help = helpStr[x];
            x++;

            //cr0 is default bf
            ASMDef[x].name = "cmplwi";
            ASMDef[x].opCode = 0x28000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 16;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeCND;
            ASMDef[x].help = helpStr[x];
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
            ASMDef[x].help = helpStr[x];
            x++;

            //cr0 is default bf
            ASMDef[x].name = "cmpw";
            ASMDef[x].opCode = 0x7C000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 16; ASMDef[x].shifts[1] = 11;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeCND;
            ASMDef[x].help = helpStr[x];
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
            ASMDef[x].help = helpStr[x];
            x++;

            //cr0 is default bf
            ASMDef[x].name = "cmplw";
            ASMDef[x].opCode = 0x7C000040;
            ASMDef[x].opShift = new int[] { 6, 7 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 16; ASMDef[x].shifts[1] = 11;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeCND;
            ASMDef[x].help = helpStr[x];
            ASMDef[x].help = helpStr[x];
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
            ASMDef[x].help = helpStr[x];
            x++;

            InsPlaceArr[(char)'F' - (char)'A'] = (int)x;
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
            ASMDef[x].help = helpStr[x];
            x++;

            ASMDef[x].name = "fdiv";
            ASMDef[x].opCode = 0xFC000024;
            ASMDef[x].opShift = new int[] { 6, 6 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 11;
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = helpStr[x];
            x++;

            ASMDef[x].name = "fmr";
            ASMDef[x].opCode = 0xFC000090;
            ASMDef[x].opShift = new int[] { 6, 6 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 11;
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = helpStr[x];
            x++;

            ASMDef[x].name = "fmul";
            ASMDef[x].opCode = 0xFC000032;
            ASMDef[x].opShift = new int[] { 6, 6 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 6;
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = helpStr[x];
            x++;

            ASMDef[x].name = "fsub";
            ASMDef[x].opCode = 0xFC000028;
            ASMDef[x].opShift = new int[] { 6, 6 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 16;
            ASMDef[x].shifts[2] = 11;
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = helpStr[x];
            x++;

            //63 FRT /// FRB /// 22 Rc
            ASMDef[x].name = "fsqrt";
            ASMDef[x].opCode = 0xFC00002C;
            ASMDef[x].opShift = new int[] { 6, 6 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].shifts[1] = 11;
            ASMDef[x].type = typeFNAN;
            ASMDef[x].help = helpStr[x];
            x++;


            InsPlaceArr[(char)'L' - (char)'A'] = (int)x;
            /* ---------- lbz ---------- */
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
            ASMDef[x].help = helpStr[x];
            x++;

            /* ld */
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
            ASMDef[x].help = helpStr[x];
            x++;

            /* lfs */
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
            ASMDef[x].help = helpStr[x];
            x++;

            /* ---------- lhz ---------- */
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
            ASMDef[x].help = helpStr[x];
            x++;

            /* lis */
            ASMDef[x].name = "lis";
            ASMDef[x].opCode = 0x3C000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeIMM;
            ASMDef[x].help = helpStr[x];
            x++;

            /* li */
            ASMDef[x].name = "li";
            ASMDef[x].opCode = 0x38000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 21;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeIMM;
            ASMDef[x].help = helpStr[x];
            x++;

            /* lwz */
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
            ASMDef[x].help = helpStr[x];
            x++;

            InsPlaceArr[(char)'M' - (char)'A'] = (int)x;
            /* ---------- mfspr ---------- */
            ASMDef[x].name = "mfspr";
            ASMDef[x].opCode = 0x7C0002A6;
            ASMDef[x].opShift = new int[] { 6, 14 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 16; ASMDef[x].shifts[1] = 21;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 1; ASMDef[x].order[1] = 0;
            ASMDef[x].type = typeSPR;
            ASMDef[x].help = helpStr[x];
            x++;

            /* mtspr */
            ASMDef[x].name = "mtspr";
            ASMDef[x].opCode = 0x7C0003A6;
            ASMDef[x].opShift = new int[] { 6, 14 };
            ASMDef[x].shifts = new int[2];
            ASMDef[x].shifts[0] = 16; ASMDef[x].shifts[1] = 21;
            ASMDef[x].order = new int[2];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1;
            ASMDef[x].type = typeSPR;
            ASMDef[x].help = helpStr[x];
            x++;

            /* mullw */
            ASMDef[x].name = "mullw";
            ASMDef[x].opCode = 0x7C0001D6;
            ASMDef[x].opShift = new int[] { 6, 10 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 21; ASMDef[x].shifts[1] = 16; ASMDef[x].shifts[2] = 11;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = helpStr[x];
            x++;

            /* mulli */
            ASMDef[x].name = "mulli";
            ASMDef[x].opCode = 0x1C000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 21; ASMDef[x].shifts[1] = 16;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 0; ASMDef[x].order[1] = 1; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeIMM;
            ASMDef[x].help = helpStr[x];
            x++;

            InsPlaceArr[(char)'N' - (char)'A'] = (int)x;
            /* ---------- nop ---------- */
            ASMDef[x].name = "nop";
            ASMDef[x].opCode = 0x60000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].type = typeNOP;
            ASMDef[x].help = helpStr[x];
            x++;

            InsPlaceArr[(char)'O' - (char)'A'] = (int)x;
            /* ---------- ori ---------- */
            ASMDef[x].name = "ori";
            ASMDef[x].opCode = 0x60000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[3];
            ASMDef[x].shifts[0] = 21; ASMDef[x].shifts[1] = 16;
            ASMDef[x].order = new int[3];
            ASMDef[x].order[0] = 1; ASMDef[x].order[1] = 0; ASMDef[x].order[2] = 2;
            ASMDef[x].type = typeIMM;
            ASMDef[x].help = helpStr[x];
            x++;

            InsPlaceArr[(char)'S' - (char)'A'] = (int)x;
            /* ---------- sc ---------- */


            /* slwi */
            ASMDef[x].name = "slwi";
            ASMDef[x].opCode = 0x54000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[] { 21, 16, 11 };
            ASMDef[x].order = new int[] { 1, 0, 2 };
            ASMDef[x].type = typeIMM5;
            ASMDef[x].help = helpStr[x];
            x++;

            /* srwi */
            ASMDef[x].name = "srwi";
            ASMDef[x].opCode = 0x54000000;
            ASMDef[x].opShift = new int[] { 6, 0 };
            ASMDef[x].shifts = new int[] { 21, 16, 6 };
            ASMDef[x].order = new int[] { 1, 0, 2 };
            ASMDef[x].type = typeIMM5;
            ASMDef[x].help = helpStr[x];
            x++;

            /* stb */
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
            ASMDef[x].help = helpStr[x];
            x++;

            /* std */
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
            ASMDef[x].help = helpStr[x];
            x++;

            /* stdu */
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
            ASMDef[x].help = helpStr[x];
            x++;

            /* stfs */
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
            ASMDef[x].help = helpStr[x];
            x++;

            /* sth */
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
            ASMDef[x].help = helpStr[x];
            x++;

            /* stw */
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
            ASMDef[x].help = helpStr[x];
            x++;

            /* stwu */
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
            ASMDef[x].help = helpStr[x];
            x++;

            /* subf */
            ASMDef[x].name = "subf";
            ASMDef[x].opCode = 0x7C000050;
            ASMDef[x].opShift = new int[] { 6, 9 };
            ASMDef[x].shifts = new int[3] { 21, 16, 11 };
            ASMDef[x].order = new int[3] { 0, 1, 2 };
            ASMDef[x].type = typeNAN;
            ASMDef[x].help = helpStr[x];
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
            

            //addis
            helpStr[x] = "addis - Add Immediate Shifted" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Adds the register rA with the 16-bit signed value IMM and stores it into the lower part of register rD" + "\n";
            helpStr[x] += "Operation:" + "\n"; 
            helpStr[x] += "rD = (rA + IMM) << 16" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "addis rD, rA, 0xIMM" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "addis r4, r3, 0x1FF0" + "\n";
            x++;
            //addi
            helpStr[x] = "addi - Add Immediate" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Adds the register rA with the 16-bit signed value IMM and stores it into the upper part of register rD" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "rD = rA + IMM" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "addi rD, rA, 0xIMM" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "addi r4, r3, 0x1FF0" + "\n";
            x++;
            //add
            helpStr[x] = "add - Add" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Adds the register rA with the register rB and stores it into the register rD" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "rD = rA + rB" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "add rD, rA, rB" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "add r3, r3, r4" + "\n";
            x++;
            //blr
            helpStr[x] = "blr - Branch To Link Register" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Branches to the address held in the special register LR" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "PC = LR" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "blr" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "blr" + "\n";
            x++;
            //bl
            helpStr[x] = "bl - Branch Then Link" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Branches to the unsigned value IMM and sets special register LR to the address after it" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "LR = CURRENT_ADDR + 4\n" + "PC = IMM" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "bl IMM" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "bl 0x0024A7FC" + "\n";
            x++;
            //beq - 1
            helpStr[x] = "beq - Branch If Equal" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Branches to the value (IMM + CURRENT_ADDR) if CR is set equal" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "if (CR == 1) then PC = CURRENT_ADDR + IMM" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "beq CR, IMM" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "beq cr2, 0x5C" + "\n";
            helpStr[x] += "NOTE:" + "\n";
            helpStr[x] += "If there is no CR specified, then it will be defaulted to cr0" + "\n";
            x++;
            //beq - 2
            x++;
            //bne - 1
            helpStr[x] = "bne - Branch If Not Equal" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Branches to the value (IMM + CURRENT_ADDR) if CR is not set equal" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "if (CR != 1) then PC = 0xCURRENT_ADDR + IMM (DEC)" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "bne CR, IMM" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "bne cr2, 0x54" + "\n";
            helpStr[x] += "NOTE:" + "\n";
            helpStr[x] += "If there is no CR specified, then it will be defaulted to cr0" + "\n";
            x++;
            //bne - 2
            x++;
            //ble - 1
            helpStr[x] = "ble - Branch If Less Than Or Equal" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Branches to the value (IMM + CURRENT_ADDR) if CR is set less than or equal" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "if (CR == 1 || CR == 4) then PC = 0xCURRENT_ADDR + 0xIMM" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "ble CR, IMM" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "ble cr2, 0x5C" + "\n";
            helpStr[x] += "NOTE:" + "\n";
            helpStr[x] += "If there is no CR specified, then it will be defaulted to cr0" + "\n";
            x++;
            //ble - 2
            x++;
            //bge - 1
            helpStr[x] = "bge - Branch If Greater Than or Equal" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Branches to the value (IMM + CURRENT_ADDR) if CR is set greater than or equal" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "if (CR == 1 || CR == 2) then PC = 0xCURRENT_ADDR + 0xIMM" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "bge CR, IMM" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "bge cr2, 0x5C" + "\n";
            helpStr[x] += "NOTE:" + "\n";
            helpStr[x] += "If there is no CR specified, then it will be defaulted to cr0" + "\n";
            x++;
            //bge - 2
            x++;
            //bgt - 1
            helpStr[x] = "bgt - Branch If Greater Than" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Branches to the value (IMM + CURRENT_ADDR) if CR is set greater than" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "if (CR == 2) then PC = 0xCURRENT_ADDR + 0xIMM" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "bgt CR, IMM" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "bgt cr2, 0x5C" + "\n";
            helpStr[x] += "NOTE:" + "\n";
            helpStr[x] += "If there is no CR specified, then it will be defaulted to cr0" + "\n";
            x++;
            //bgt - 2
            x++;
            //blt - 1
            helpStr[x] = "blt - Branch If Less Than" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Branches to the value (IMM + CURRENT_ADDR) if CR is set less than" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "if (CR == 4) then PC = 0xCURRENT_ADDR + 0xIMM" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "blt CR, IMM" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "blt cr2, 0x5C" + "\n";
            helpStr[x] += "NOTE:" + "\n";
            helpStr[x] += "If there is no CR specified, then it will be defaulted to cr0" + "\n";
            x++;
            //blt - 2
            x++;
            //b
            helpStr[x] = "b - Branch Unconditionally" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Branches to the unsigned value IMM" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "PC = IMM" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "b IMM" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "b 0x0024A7FC" + "\n";
            x++;
            //cmpwi - 1
            helpStr[x] = "cmpwi - Compare Word Immediate" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Compares (signed 32 bits) rA with the signed value IMM and stores result in BF" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "if (rA == IMM) then BF = 1" + "\n" + "if (rA > IMM) then BF = 2" + "\n" + "if (rA < IMM) then BF = 4" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "cmpwi rA, 0xIMM" + "\n" + "cmpwi cr1, rA, 0xIMM" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "cmpwi cr2, r4, 0xA7FC" + "\n";
            helpStr[x] += "NOTE:" + "\n";
            helpStr[x] += "If there is no BF specified, then it will be defaulted to cr0" + "\n";
            x++;
            //cmpwi - 2
            x++;
            //cmplwi - 1
            helpStr[x] = "cmplwi - Compare Logical Word Immediate" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Compares (unsigned 32 bits) rA with the unsigned value IMM and stores result in BF" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "if (rA == IMM) then BF = 1" + "\n" + "if (rA > IMM) then BF = 2" + "\n" + "if (rA < IMM) then BF = 4" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "cmplwi rA, 0xIMM" + "\n" + "cmplwi cr1, rA, 0xIMM" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "cmplwi cr2, r4, 0xA7FC" + "\n";
            helpStr[x] += "NOTE:" + "\n";
            helpStr[x] += "If there is no BF specified, then it will be defaulted to cr0" + "\n";
            x++;
            //cmplwi - 2
            x++;
            //cmpw - 1
            helpStr[x] = "cmpw - Compare Word" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Compares (signed 32 bits) the rA with rB and stores result in BF" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "if (rA == rB) then BF = 1" + "\n" + "if (rA > rB) then BF = 2" + "\n" + "if (rA < rB) then BF = 4" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "cmpw rA, rB" + "\n" + "cmpw cr1, rA, rB" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "cmpw cr2, r4, r3" + "\n";
            helpStr[x] += "NOTE:" + "\n";
            helpStr[x] += "If there is no BF specified, then it will be defaulted to cr0" + "\n";
            x++;
            //cmpw - 2
            x++;
            //cmplw - 1
            helpStr[x] = "cmplw - Compare Logical Word" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Compares (unsigned 32 bits) the rA with rB and stores result in BF" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "if (rA == rB) then BF = 1" + "\n" + "if (rA > rB) then BF = 2" + "\n" + "if (rA < rB) then BF = 4" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "cmplw rA, rB" + "\n" + "cmplw cr1, rA, rB" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "cmplw cr2, r4, r3" + "\n";
            helpStr[x] += "NOTE:" + "\n";
            helpStr[x] += "If there is no BF specified, then it will be defaulted to cr0" + "\n";
            x++;
            //cmplw - 2
            x++;
            //fadd
            helpStr[x] = "fadd - Floating-Point Add" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Adds frA with frB and stores the result into frD" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "frD =  (frA + frB)" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "fadd frD, frA, frB" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "fadd f1, f2, f3" + "\n";
            x++;
            //fdiv
            helpStr[x] = "fdiv - Floating-Point Divide" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Divides frA by frB and stores the result into frD" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "frD =  (frA / frB)" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "fdiv frD, frA, frB" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "fdiv f1, f2, f3" + "\n";
            x++;
            //fmr
            helpStr[x] = "fmr - Floating-Point Move Register" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Moves the contents of frA to frD" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "frD =  frA" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "fmr frD, frA" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "fmr f1, f2" + "\n";
            x++;
            //fmul
            helpStr[x] = "fmul - Floating-Point Multiply" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Multiplies frA with frB and stores the result into frD" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "frD =  (frA + frB)" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "fmul frD, frA, frB" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "fmul f1, f2, f3" + "\n";
            x++;
            //fsub
            helpStr[x] = "fsub - Floating-Point Subtract" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Subtracts frB from frA and stores the result into frD" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "frD =  (frA - frB)" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "fsub frD, frA, frB" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "fsub f1, f2, f3" + "\n";
            x++;
            //fsqrt
            helpStr[x] = "fsqrt - Floating-Point Square Root" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Takes the square root of frA and stores the result in frD" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "frD = sqrt(frA)" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "fsqrt frD, frA" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "fsqrt f1, f2" + "\n";
            x++;
            //lbz
            helpStr[x] = "lbz - Load Byte And Zero" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Loads the byte from the address (IMM + rA) and stores it into lower 8 bits of rD. The other 24 bits are set to 0" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "rD = (char)MEM(IMM + rA)" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "lbz rD, IMM(rA)" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "lbz r3, 0x4058(r4)" + "\n";
            x++;
            //ld
            helpStr[x] = "ld - Load Double" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Loads the double-word from the address (IMM + rA) and stores it into the lower 64 bits of rD" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "rD = (long)MEM(IMM + rA)" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "ld rD, IMM(rA)" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "ld r14, 0x0020(r1)" + "\n";
            x++;
            //lfs
            helpStr[x] = "lfs - Load Floating-Point Single" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Loads the single from (IMM + rB) and converts it to double and stores it in frA" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "frA = (double)MEM(IMM + rB)" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "stfs frA, IMM(rB)" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "stfs f1, 0x0054(r5)" + "\n";
            x++;
            //lhz
            helpStr[x] = "lhz - Load Half And Zero" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Loads the halfword from the address (IMM + rA) and stores it into the lower 16 bits of rD\nThe remaining bits are cleared" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "rD = (short)MEM(IMM + rA)" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "lhz rD, IMM(rA)" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "lhz r14, 0x0020(r1)" + "\n";
            x++;
            //lis
            helpStr[x] = "lis - Load Immediate Shifted" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Sets the lower part of the register rD to the 16-bit signed value IMM" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "rD = (IMM << 16)" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "lis rD, IMM" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "lis r3, 0x1234" + "\n";
            helpStr[x] += "NOTE:" + "\n";
            helpStr[x] += "Result is similar to addis" + "\n";
            x++;
            //li
            helpStr[x] = "li - Load Immediate" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Sets the upper part of the register rD to the 16-bit signed value IMM" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "rD = IMM" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "li rD, IMM" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "li r3, 0x1234" + "\n";
            helpStr[x] += "NOTE:" + "\n";
            helpStr[x] += "Result is similar to addi" + "\n";
            x++;
            //lwz
            helpStr[x] = "lwz - Load Word And Zero" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Loads the word from the address (IMM + rA) and stores it into rD" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "rD = (int)MEM(IMM + rA)" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "lwz rD, IMM(rA)" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "lwz r3, 0x4058(r4)" + "\n";
            x++;
            //mfspr
            helpStr[x] = "mfspr - Move From Special Purpose Register" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "The contents of SPR are placed into rD" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "rD = SPR" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "mfspr rD, SPR" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "mfspr r0, LR" + "\n";
            x++;
            //mtspr
            helpStr[x] = "mtspr - Move To Special Purpose Register" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "The contents of rA are placed into SPR" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "SPR = rA" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "mtspr SPR, rA" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "mtspr LR, r0" + "\n";
            x++;
            //mullw
            helpStr[x] = "mullw - Multiply Low Word" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Multiplies the 32 bits of rA and rB and stores the resulting 64 bits into rD" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "rD = (long)(rA * rB)" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "mullw rD, rA, rB" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "mullw r3, r3, r3" + "\n";
            x++;
            //mulli
            helpStr[x] = "mulli - Multiply Low Immediate" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Multiplies the 64 bits of rA and IMM and stores the resulting 64 bits into rD" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "rD = (long)(rA * IMM)" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "mulli rD, rA, IMM" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "mulli r3, r3, 3" + "\n";
            x++;
            //nop
            helpStr[x] = "nop / noop - No Operation" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Does nothing (disassembled as ori, r0, r0, 0)" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "r0 = r0 | 0" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "nop" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "nop" + "\n";
            x++;
            //ori
            helpStr[x] = "ori - Or Immediate" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Logical Ors rA with IMM and stores result in rD" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "rD = rA | IMM" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "ori rD, rA, IMM" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "ori r3, r3, 0x1300" + "\n";
            x++;
            //slwi
            helpStr[x] = "slwi - Shift Left Word Immediate" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Shifts the word (32 bits) rB left by IMM bits and stores the result in rA" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "rA = (int)(rB << IMM)" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "slwi rA, rB, IMM" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "slwi r3, r27, 2" + "\n";
            x++;
            //srwi
            helpStr[x] = "srwi - Shift Right Word Immediate" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Shifts the word (32 bits) rB right by IMM bits and stores the result in rA" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "rA = (int)(rB >> IMM)" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "srwi rA, rB, IMM" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "srwi r3, r4, 2" + "\n";
            x++;
            //stb
            helpStr[x] = "stb - Store Byte" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Stores the byte rA at the address (IMM + rB)" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "MEM(IMM + rB) = (char)rA" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "stb rA, IMM(rB)" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "stb r4, 0x0054(r5)" + "\n";
            x++;
            //std
            helpStr[x] = "std - Store Double" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Stores the doubleword (64 bits) rA at the address (IMM + rB)" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "MEM(IMM + rB) = (long)rA" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "std rA, IMM(rB)" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "std r4, 0x0054(r5)" + "\n";
            x++;
            //stdu
            helpStr[x] = "stdu - Store Double With Update" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Stores the doubleword (64 bits) rA at the address (IMM + rB). rB is then set to (IMM + rB)" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "MEM(IMM + rB) = (long)rA" + "\n" + "rB += IMM" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "stdu rA, IMM(rB)" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "stdu r4, 0x0054(r5)" + "\n";
            x++;
            //stfs
            helpStr[x] = "stfs - Store Floating-Point Single" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Converts the float in frA to a single and stores it at (IMM + rB)" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "MEM(IMM + rB) = (Single)frA" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "stfs frA, IMM(rB)" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "stfs f1, 0x0054(r5)" + "\n";
            x++;
            //sth
            helpStr[x] = "sth - Store Half" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Stores the lower 16 bits of rD at the address (IMM + rA)" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "MEM(IMM + rA) = (short)rD" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "sth rD, IMM(rA)" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "sth r14, 0x0020(r1)" + "\n";
            x++;
            //stw
            helpStr[x] = "stw - Store Word" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Stores the word (32 bits) rA at the address (IMM + rB)" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "MEM(IMM + rB) = (int)rA" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "stw rA, IMM(rB)" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "stw r4, 0x0054(r5)" + "\n";
            x++;
            //stwu
            helpStr[x] = "stwu - Store Word With Update" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Stores the word (32 bits) rA at the address (IMM + rB). rB is then set to (IMM + rB)" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "MEM(IMM + rB) = (int)rA" + "\n" + "rB += IMM" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "stwu rA, IMM(rB)" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "stwu r4, 0x0054(r5)" + "\n";
            x++;
            //subf
            helpStr[x] = "subf - Subtract From" + "\n";
            helpStr[x] += "Description:" + "\n";
            helpStr[x] += "Stores (~rA + rB + 1) into rD" + "\n";
            helpStr[x] += "Operation:" + "\n";
            helpStr[x] += "rD = rB - rA or rD = ~rA + rB + 1" + "\n";
            helpStr[x] += "Syntax:" + "\n";
            helpStr[x] += "subf rD, rA, rB" + "\n";
            helpStr[x] += "Example:" + "\n";
            helpStr[x] += "subf r4, r4, r3" + "\n";
            x++;

            
            /* Command Help String */
            x = 0;

            //address
            helpCom[x] = "address - Set Address" + nl;
            helpCom[x] += "Description:" + nl;
            helpCom[x] += "Set the current address of the subroutine" + nl;
            helpCom[x] += "NOTE:" + nl;
            helpCom[x] += "The address will automatically be incremented by 4 on each instruction" + nl;
            x++;
            //hook
            helpCom[x] = "hook - Set Hook Address" + nl;
            helpCom[x] += "Description:" + nl;
            helpCom[x] += "Set the current hook address of the subroutine (b). At the end of the compiled subroutine there will be a line with the address of the hook and a b to the address" + nl;
            helpCom[x] += "NOTE:" + nl;
            helpCom[x] += "There can only be one hook address at this point" + nl;
            x++;
            //hookl
            helpCom[x] = "hookl - Set Hook Address With Link" + nl;
            helpCom[x] += "Description:" + nl;
            helpCom[x] += "Set the current hook address of the subroutine (bl). At the end of the compiled subroutine there will be a line with the address of the hook and a bl to the address" + nl;
            helpCom[x] += "NOTE:" + nl;
            helpCom[x] += "There can only be one hook address at this point" + nl;
            x++;
            //setreg
            helpCom[x] = "setreg - Set Register" + nl;
            helpCom[x] += "Description:" + nl;
            helpCom[x] += "Sets rD to the 32 bit unsigned IMM" + nl;
            helpCom[x] += "Operation:" + nl;
            helpCom[x] += "rD = IMM" + nl;
            helpCom[x] += "Syntax:" + nl;
            helpCom[x] += "setreg rD, IMM" + nl;
            helpCom[x] += "Example:" + nl;
            helpCom[x] += "setreg r3, 0x12345678" + nl;
            helpCom[x] += "Output:" + nl;
            helpCom[x] += "lis r3, 0x1234" + nl + "ori r3, r3, 0x5678" + nl;
            x++;
            //hexcode
            helpCom[x] = "hexcode - Hexadecimal/Decimal Code" + nl;
            helpCom[x] += "Description:" + nl;
            helpCom[x] += "Inserts the value specified into the assembled code" + nl;
            helpCom[x] += "NOTE:" + nl;
            helpCom[x] += "Useful for unsupported instructions" + nl;
            helpCom[x] += "Example:" + nl;
            helpCom[x] += "address 0x00000000" + nl + "hexcode 0x12345678" + nl;
            helpCom[x] += "Output:" + nl;
            helpCom[x] += "2 00000000 12345678" + nl;
            x++;
            //import
            helpCom[x] = "import - Import CWP3 File" + nl;
            helpCom[x] += "Description:" + nl;
            helpCom[x] += "Inserts the CWP3 file(s) specified into the assembly at compile time" + nl;
            helpCom[x] += "If a full path is not specified, the path will be that of the current cwp3 file" + nl;
            helpCom[x] += "Example:" + nl;
            helpCom[x] += "import Math.cwp3" + nl + "import C:\\CodeWizard PS3\\Math.cwp3" + nl;
            x++;
            //float
            helpCom[x] = "float - Float To Hex" + nl;
            helpCom[x] += "Description:" + nl;
            helpCom[x] += "Inserts the float value specified into the assembled code as hex" + nl;
            helpCom[x] += "Example:" + nl;
            helpCom[x] += "float 3.14159" + nl;
            helpCom[x] += "Output:" + nl;
            helpCom[x] += "2 00000000 40490FD0" + nl;
            x++;
            //string
            helpCom[x] = "string - String To Hex" + nl;
            helpCom[x] += "Description:" + nl;
            helpCom[x] += "Inserts the string specified into the assembled code as hex" + nl;
            helpCom[x] += "Example:" + nl;
            helpCom[x] += "string ABCDE F" + nl;
            helpCom[x] += "Output:" + nl;
            helpCom[x] += "2 00000000 41424344" + nl;
            helpCom[x] += "2 00000004 45204600" + nl;
            x++;


            /* Register Help String */
            x = 0;

            //r0
            helpReg[x] = "r0 - Register Zero" + nl;
            helpReg[x] += "More information on PPC registers: http://www.csd.uwo.ca/~mburrel/stuff/ppc-asm.html" + nl;
            helpReg[x] += "Purpose:" + nl;
            helpReg[x] += "Commonly used to hold the old link register when building the stack frame" + nl;
            helpReg[x] += "Usage:" + nl;
            helpReg[x] += "It is best not to modify this register unless dealing with the stack" + nl;
            x++;
            //r1
            helpReg[x] = "r1 - Stack Pointer" + nl;
            helpReg[x] += "More information on PPC registers: http://www.csd.uwo.ca/~mburrel/stuff/ppc-asm.html" + nl;
            helpReg[x] += "Purpose:" + nl;
            helpReg[x] += "Points to the beginning of the stack" + nl;
            helpReg[x] += "Usage:" + nl;
            helpReg[x] += "It is best not to modify this register unless dealing with the stack" + nl;
            x++;
            //r2
            helpReg[x] = "r2 - Table of Contents Pointer" + nl;
            helpReg[x] += "More information on Register 2: http://physinfo.ulb.ac.be/divers_html/powerpc_programming_info/intro_to_ppc/ppc4_runtime4.html" + nl;
            helpReg[x] += "Purpose:" + nl;
            helpReg[x] += " a collection of pointers that the code uses to locate its static data (including global variables), pointers to external functions, and imported globals from other fragments" + nl;
            helpReg[x] += "Usage:" + nl;
            helpReg[x] += "Do not modify this register" + nl;
            x++;
            //r3
            helpReg[x] = "r3 - Argument 1 and/or Return Value" + nl;
            helpReg[x] += "More information on PPC registers: http://www.csd.uwo.ca/~mburrel/stuff/ppc-asm.html" + nl;
            helpReg[x] += "Purpose:" + nl;
            helpReg[x] += "Commonly used as the return value of a function, and also the first argument" + nl;
            helpReg[x] += "Usage:" + nl;
            helpReg[x] += "In most cases this can be modified, however, using r4 - r10 is more reliable" + nl;
            x++;
            //r4 - r10
            helpReg[x] = "(r4 - r10) - Arguments 2 - 8" + nl;
            helpReg[x] += "More information on PPC registers: http://www.csd.uwo.ca/~mburrel/stuff/ppc-asm.html" + nl;
            helpReg[x] += "Purpose:" + nl;
            helpReg[x] += "Commonly used as function arguments 2 through 8" + nl;
            helpReg[x] += "Usage:" + nl;
            helpReg[x] += "Free to be used" + nl;
            x++;
            //r11 - r12
            helpReg[x] = "(r11 - r12) - Unknown" + nl;
            helpReg[x] += "More information on Register 11: http://dslab.lzu.edu.cn:8080/members/siro/docs/ppc/ppc_inline_programming.pdf" + nl;
            helpReg[x] += "Purpose:" + nl;
            helpReg[x] += "None" + nl;
            helpReg[x] += "Usage:" + nl;
            helpReg[x] += "Free to be used" + nl;
            x++;
            //r13 - r31
            helpReg[x] = "(r13 - r31) - Global Registers" + nl;
            helpReg[x] += "More information on PPC registers: http://www.csd.uwo.ca/~mburrel/stuff/ppc-asm.html" + nl;
            helpReg[x] += "Purpose:" + nl;
            helpReg[x] += "None" + nl;
            helpReg[x] += "Usage:" + nl;
            helpReg[x] += "Free to be used, BUT, they must be preserved in the stack" + nl;
            x++;
            //f0 - f13
            helpReg[x] = "(f0 - f13) - Floating Point Registers (volatile)" + nl;
            helpReg[x] += "Purpose:" + nl;
            helpReg[x] += "None" + nl;
            helpReg[x] += "Usage:" + nl;
            helpReg[x] += "Free to be used" + nl;
            x++;
            //f14 - f31
            helpReg[x] = "(f14 - f31) - Floating Point Registers (nonvolatile)" + nl;
            helpReg[x] += "Purpose:" + nl;
            helpReg[x] += "None" + nl;
            helpReg[x] += "Usage:" + nl;
            helpReg[x] += "Free to be used, BUT, they must be preserved in the stack" + nl;
            x++;
            //cr0 - cr7
            helpReg[x] = "(cr0 - cr7) - Condition Registers" + nl;
            helpReg[x] += "Purpose:" + nl;
            helpReg[x] += "Hold results of comparisons" + nl;
            helpReg[x] += "Usage:" + nl;
            helpReg[x] += "Free to be used, BUT, they must be preserved in the stack" + nl;
            helpReg[x] += "See the instructions cmpwi, cmplwi, cmpw, and cmplw for usage" + nl;
            x++;
            //XER
            helpReg[x] = "XER" + nl;
            helpReg[x] += "Purpose:" + nl;
            helpReg[x] += "Overflows and extra stuff" + nl;
            helpReg[x] += "For more info search for the section \"XER Register (XER)\" at http://www.cebix.net/downloads/bebox/pem32b.pdf" + nl;
            helpReg[x] += "Usage:" + nl;
            helpReg[x] += "Do not modify this special register" + nl;
            x++;
            //LR
            helpReg[x] = "LR - Link Register" + nl;
            helpReg[x] += "Purpose:" + nl;
            helpReg[x] += "Holds return address when a linking branch is called" + nl;
            helpReg[x] += "Usage:" + nl;
            helpReg[x] += "It is best not to modify this register unless dealing with the stack" + nl;
            x++;
            //CTR
            helpReg[x] = "CTR - Count Register" + nl;
            helpReg[x] += "Purpose:" + nl;
            helpReg[x] += "Holds a loop count that can be decremented during execution of branch instructions" + nl;
            helpReg[x] += "For more info search for the section \"Count Register (CTR)\" at http://www.cebix.net/downloads/bebox/pem32b.pdf" + nl;
            helpReg[x] += "Usage:" + nl;
            helpReg[x] += "Only modify in situations that this register can be used in" + nl;
            x++;

            /* Term Help String */
            x = 0;

            //rA
            helpTerm[x] = "rA - Register A" + nl;
            helpTerm[x] += "Description:" + nl;
            helpTerm[x] += "First non-destination register in an assembly code" + nl;
            x++;
            //rB
            helpTerm[x] = "rB - Register B" + nl;
            helpTerm[x] += "Description:" + nl;
            helpTerm[x] += "Second non-destination register in an assembly code" + nl;
            x++;
            //rD
            helpTerm[x] = "rD - Destination Register" + nl;
            helpTerm[x] += "Description:" + nl;
            helpTerm[x] += "First destination register in an assembly code" + nl;
            x++;
            //BF
            helpTerm[x] = "BF - Conditional Register" + nl;
            helpTerm[x] += "Description:" + nl;
            helpTerm[x] += "Conditional register used in the following instructions:" + nl;
            helpTerm[x] += "beq, bne, ble, blt, bgt, bge, cmpwi, cmplwi, cmpw, cmplw" + nl;
            x++;
            //IMM
            helpTerm[x] = "IMM - Immediate Value" + nl;
            helpTerm[x] += "Description:" + nl;
            helpTerm[x] += "A constant value with a bit length defined by the instruction" + nl;
            x++;
            //Signed
            helpTerm[x] = "Signed" + nl;
            helpTerm[x] += "Description:" + nl;
            helpTerm[x] += "Defines that the value can be negative" + nl;
            x++;
            //Signed
            helpTerm[x] = "Unsigned" + nl;
            helpTerm[x] += "Description:" + nl;
            helpTerm[x] += "Defines that the value cannot be negative" + nl;
            helpTerm[x] += "An unsigned 32-bit value of 0xFFFFFFFF is equal to 4294967295";
            helpTerm[x] += " while a signed 32-bit value of 0xFFFFFFFF is equal to -1" + nl;
            x++;
        }
    }
}
