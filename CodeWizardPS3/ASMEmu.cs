﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace CodeWizardPS3
{
    public partial class ASMEmu : Form
    {
        PPCReg[] regBoxes;
        FPRReg[] fprBoxes;
        public MemCode[] codes;
        string memoryBin = "";
        bool RefreshMem = false;
        uint PCounter = 0;

        //comparison integers
        int cmpEQ = 1;
        int cmpGT = 2;
        int cmpLT = 4;

        public struct MemCode
        {
            public uint addr;
            public uint val;
        }

        public ASMEmu()
        {
            InitializeComponent();
        }

        /* Sets the memory at addr to val of length len */
        public void SetMemory(uint addr, uint val, int len)
        {
            if (addr < Main.minMemRange || addr >= Main.maxMemRange)
            {
                debugBox.Text += "Address \"" + addr.ToString("X8") + "\" is outside the set memory range!" + Environment.NewLine;
                return;
            }

            uint oldAddr = addr;
            addr -= addr % (uint)len;
            if (addr != oldAddr)
                debugBox.Text += "Address \"" + oldAddr.ToString("X8") + "\" is not " + len.ToString() + " byte aligned!" + Environment.NewLine;

            FileIO.SetMemory(memoryBin, addr, val, len, Main.minMemRange);
        }

        /* Gets the memory at addr of length len */
        public uint GetMemory(uint addr, int len)
        {
            if (addr < Main.minMemRange || addr >= Main.maxMemRange)
            {
                debugBox.Text += "Address \"" + addr.ToString("X8") + "\" is outside the set memory range!"  + Environment.NewLine;
                return 0;
            }

            uint oldAddr = addr;
            addr -= addr % (uint)len;
            if (addr != oldAddr)
                debugBox.Text += "Address " + oldAddr.ToString("X8") + " is not " + len.ToString() + " byte aligned!" + Environment.NewLine;

            uint[] a = FileIO.GrabMemory(memoryBin, addr, len, Main.minMemRange);
            return a[0];
        }

        /* Sets the (32 * (index + 1)) bits of register reg to value */
        public void SetRegister(string reg, string value, int index)
        {
            reg = reg.ToLower();
            switch (reg)
            {
                case "cr":
                    regBoxes[32].SetReg(index, value);
                    regBoxes[32].SetForeColor(index, 1, Color.Red);
                    return;
                case "xer":
                    regBoxes[33].SetReg(index, value);
                    regBoxes[33].SetForeColor(index, 1, Color.Red);
                    return;
                case "lr":
                    regBoxes[34].SetReg(index, value);
                    regBoxes[34].SetForeColor(index, 1, Color.Red);
                    return;
                case "ctr":
                    regBoxes[35].SetReg(index, value);
                    regBoxes[35].SetForeColor(index, 1, Color.Red);
                    return;
                case "fpscr":
                    regBoxes[36].SetReg(index, value);
                    regBoxes[36].SetForeColor(index, 1, Color.Red);
                    return;
            }

            //Floating Point Registers
            if (reg[0] == 'f')
            {
                int val = int.Parse(reg.Replace("f", ""));
                fprBoxes[val].SetReg(value);
                fprBoxes[val].SetForeColor(Color.Red);
            }
            else if (reg[0] == 'r') //General Purpose Registers
            {
                int val = int.Parse(reg.Replace("r", ""));
                regBoxes[val].SetReg(index, value);
                regBoxes[val].SetForeColor(index, 1, Color.Red);
            }
        }

        /* Gets the (32 * (index + 1)) bits of register reg*/
        public string GetRegister(string reg,int index)
        {
            reg = reg.ToLower();
            switch (reg)
            {
                case "cr":
                    return regBoxes[32].GetReg(index);
                case "xer":
                    return regBoxes[33].GetReg(index);
                case "lr":
                    return regBoxes[34].GetReg(index);
                case "ctr":
                    return regBoxes[35].GetReg(index);
                case "fpscr":
                    return regBoxes[36].GetReg(index);
            }

            //Floating Point Registers
            if (reg[0] == 'f')
            {
                int val = int.Parse(reg.Replace("f", ""));
                return fprBoxes[val].GetReg();
            }
            else //General Purpose Registers
            {
                int val = int.Parse(reg.Replace("r", ""));
                return regBoxes[val].GetReg(index);
            }
        }

        /* Takes the op/instruction and the register (as an array) and runs it
         * Error Codes:
         * -1: Disassembly error, number of registers doesn't match
         * -2: Unsupported instruction
         * -3: Error occured in emulation
         */
        public int RunASM(string op, string[] regs)
        {
            //Set all registers forecolor to white
            for (int x = 0; x < regBoxes.Length; x++)
                regBoxes[x].SetForeColor(0, 3, Color.White);
            for (int x = 0; x < fprBoxes.Length; x++)
                fprBoxes[x].SetForeColor(Color.White);

            if (op == "illegal" || op == "nop")
            {
                StepOver_Click(null, null);
                return 0;
            }
            if (op == "hexcode")
            {
                StepOver_Click(null, null);
                return -2;
            }

            //Finds the function that handles the emulation using reflection and calls it
            object[] args = { regs };
            MethodInfo mi = this.GetType().GetMethod("EMU_" + op.ToLower());
            if (mi != null)
                return (int)mi.Invoke(this, args);

            //Op not supported
            return -2;
        }

        /* Update the memory boxes based on the Program Counter */
        void UpdateMemBox()
        {
            if (PCounter < Main.minMemRange)
            {
                debugBox.Text += "Address " + PCounter.ToString("X8") + " is not within the set memory!" + Environment.NewLine;
                PCounter = Main.minMemRange;
            }
            else if (PCounter > Main.maxMemRange)
            {
                debugBox.Text += "Address " + PCounter.ToString("X8") + " is not within the set memory!" + Environment.NewLine;
                PCounter = Main.maxMemRange;
            }

            DrawMemoryBox(PCounter, 44 * 4);
            firstIndChange = true;
            ASMBox.SelectedIndex = 0;
            StringBox.SelectedIndex = 0;
            ValueBox.SelectedIndex = 0;
            AddressBox.SelectedIndex = 0;
            firstIndChange = false;
        }

        /* 
         * Updates the memory boxes based on PC 
         * The SelectedIndex is set to index
         */
        void UpdateMemBoxCustom(uint PC, int index)
        {
            DrawMemoryBox(PC, 44 * 4);
            if (ASMBox.SelectedIndex != index)
                ASMBox.SelectedIndex = index;
            if (AddressBox.SelectedIndex != index)
                AddressBox.SelectedIndex = index;
            if (ValueBox.SelectedIndex != index)
                ValueBox.SelectedIndex = index;
            if (StringBox.SelectedIndex != index)
                StringBox.SelectedIndex = index;
        }

        /* Set the order of each 32 bits of the registers */
        public void SetBitOrder()
        {
            for (int x = 0; x < regBoxes.Length; x++)
                regBoxes[x].BitOrder = Main.bitOrder;
        }

        /* Fills the memory boxes with the address, value, and disassembly */
        public void DrawMemoryBox(uint address, int length)
        {
            if (address >= Main.maxMemRange)
                return;

            uint[] values = FileIO.GrabMemory(memoryBin, address, length, Main.minMemRange);

            //Add items to lists
            AddressBox.Items.Clear();
            ValueBox.Items.Clear();
            StringBox.Items.Clear();
            ASMBox.Items.Clear();

            ASMDeclarations.ASMDisMode = 1;
            for (int x = 0; x < values.Length; x++)
            {
                AddressBox.Items.Add(address.ToString("X8"));
                ValueBox.Items.Add(values[x].ToString("X8"));
                StringBox.Items.Add(Main.ByteAToString(BitConverter.GetBytes(values[x])));
                string asm = ASMDeclarations.ASMDisassemble("0 " + address.ToString("X8") + " " + values[x].ToString("X8"));
                if (asm.IndexOf("address") >= 0)
                    ASMBox.Items.Add(asm.Replace("\n", "").Substring(18).Replace("hexcode 0x00000000", "illegal"));
                else
                    ASMBox.Items.Add(asm.Replace("\n", "").Replace("hexcode 0x00000000", "illegal"));
                address += 4;
            }
            ASMDeclarations.ASMDisMode = 0;

            AddressBox.SelectedIndex = 0;
            ValueBox.SelectedIndex = 0;
            ASMBox.SelectedIndex = 0;
        }

        /* Parses string into value */
        public long ParseVal(string str, int mode)
        {
            bool neg = false;

            if (str == "" || str == null)
                return 0;
            uint ret = 0;

            str = str.Replace(",", "");

            str = str.ToLower();
            if (str.IndexOf('x') >= 0 || str.IndexOf('$') >= 0)
            {
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
                    return 0;
                }
            }
            else
            {
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
                    return 0;
                }
            }
        }

        /* ---------- Emulation functions ---------- */
        /*Calculate Offset Immediate Value */
        public uint CalculateOffImm(string offimm)
        {
            string[] split = offimm.Split('(');
            int regVal = (int)ParseVal("0x" + GetRegister(split[1].Replace(")", ""), 0), 1);
            int offVal = (Int16)ParseVal(split[0], 0);
            return (uint)(regVal + offVal);
        }

        /* Compare Signed Doubleword Values */
        public void CompareSDValues(int crNum, long reg1, long val1)
        {
            int mask = 0xF << (crNum * 4);
            int cr = int.Parse(GetRegister("cr", 0), System.Globalization.NumberStyles.HexNumber);
            cr &= ~mask;

            int crVal = 0;
            if (reg1 == val1)
                crVal = cmpEQ;
            else if (reg1 > val1)
                crVal = cmpGT;
            else
                crVal = cmpLT;

            cr |= crVal << (crNum * 4);
            SetRegister("cr", cr.ToString("X8"), 0);
        }

        /* Compare Signed Values */
        public void CompareSValues(int crNum, int reg1, int val1)
        {
            int mask = 0xF << (crNum * 4);
            int cr = int.Parse(GetRegister("cr", 0), System.Globalization.NumberStyles.HexNumber);
            cr &= ~mask;

            int crVal = 0;
            if (reg1 == val1)
                crVal = cmpEQ;
            else if (reg1 > val1)
                crVal = cmpGT;
            else
                crVal = cmpLT;

            cr |= crVal << (crNum * 4);
            SetRegister("cr", cr.ToString("X8"), 0);
        }

        /* Compare Float Values */
        public void CompareFValues(int crNum, Double reg1, Double reg2)
        {
            int mask = 0xF << (crNum * 4);
            int cr = int.Parse(GetRegister("cr", 0), System.Globalization.NumberStyles.HexNumber);
            cr &= ~mask;

            mask = 0xF << 16;
            int fpcc = int.Parse(GetRegister("fpscr", 0), System.Globalization.NumberStyles.HexNumber);
            fpcc &= ~mask;

            int crVal = 0;

            if (double.IsNaN(reg1) || double.IsNaN(reg2))
                crVal = 1;
            else if (reg1 == reg2)
                crVal = cmpEQ << 1;
            else if (reg1 > reg2)
                crVal = cmpGT << 1;
            else
                crVal = cmpLT << 1;

            cr |= crVal << (crNum * 4);
            fpcc |= crVal << 16;
            SetRegister("cr", cr.ToString("X8"), 0);
            SetRegister("fpscr", fpcc.ToString("X8"), 0);
        }

        /* Compare Unsigned Values */
        public void CompareUValues(int crNum, uint reg1, uint val1)
        {
            int mask = 0xF << (crNum * 4);
            int cr = int.Parse(GetRegister("cr", 0), System.Globalization.NumberStyles.HexNumber);
            cr &= ~mask;

            int crVal = 0;
            if (reg1 == val1)
                crVal = cmpEQ;
            else if (reg1 > val1)
                crVal = cmpGT;
            else
                crVal = cmpLT;

            cr |= crVal << (crNum * 4);
            SetRegister("cr", cr.ToString("X8"), 0);
        }

        /* Convert hex to Single (float) */
        public Single HexToSingle(string hex)
        {
            byte[] bytes = BitConverter.GetBytes(uint.Parse(hex, System.Globalization.NumberStyles.HexNumber));
            //if (BitConverter.IsLittleEndian)
                //bytes = bytes.Reverse().ToArray();
            return (Single)BitConverter.ToSingle(bytes, 0);
        }

        /* Convert hex to Double */
        public Double HexToDouble(string hex)
        {
            byte[] bytes = BitConverter.GetBytes(ulong.Parse(hex, System.Globalization.NumberStyles.HexNumber));
            //if (BitConverter.IsLittleEndian)
            //bytes = bytes.Reverse().ToArray();
            return (Double)BitConverter.ToDouble(bytes, 0);
        }

        /* Convert Single (float) to hex */
        public string SingleToHex(Single floatVal)
        {
            byte[] flt = BitConverter.GetBytes(Single.Parse(floatVal.ToString()));
            return BitConverter.ToUInt32(flt, 0).ToString("X8");
        }

        /* Convert Double to hex */
        public string DoubleToHex(Double floatVal)
        {
            byte[] flt = BitConverter.GetBytes(Double.Parse(floatVal.ToString()));
            return BitConverter.ToUInt64(flt, 0).ToString("X16");
        }

        public int EMU_addis(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            uint res = (uint)((int)ParseVal("0x" + GetRegister(regs[1], 0), 1) + (ParseVal(regs[2], 0) << 16));
            SetRegister(regs[0], res.ToString("X8"), 0);
            if ((int)res < 0)
                SetRegister(regs[0], "FFFFFFFF", 1);
            else
                SetRegister(regs[0], "00000000", 1);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_addic(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            uint res = (uint)((int)ParseVal("0x" + GetRegister(regs[1], 0), 1) + (Int16)ParseVal(regs[2], 0));
            if ((int)res < 0)
                SetRegister(regs[0], "FFFFFFFF", 1);
            else
                SetRegister(regs[0], "00000000", 1);
            SetRegister(regs[0], res.ToString("X8"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_addi(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            uint res = (uint)((int)ParseVal("0x" + GetRegister(regs[1], 0), 1) + (Int16)ParseVal(regs[2], 0));
            if ((int)res < 0)
                SetRegister(regs[0], "FFFFFFFF", 1);
            else
                SetRegister(regs[0], "00000000", 1);
            SetRegister(regs[0], res.ToString("X8"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_add(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            uint val = (uint)(Convert.ToInt32(GetRegister(regs[1], 0), 16) + Convert.ToInt32(GetRegister(regs[2], 0), 16));
            if ((int)val < 0)
                SetRegister(regs[0], "FFFFFFFF", 1);
            else
                SetRegister(regs[0], "00000000", 1);
            SetRegister(regs[0], val.ToString("X8"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_and(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            ulong regVal = Convert.ToUInt64(GetRegister(regs[1], 1) + GetRegister(regs[1], 0), 16);
            uint immVal = (uint)ParseVal(regs[2], 0);
            string resVal = (regVal & immVal).ToString("X16");
            SetRegister(regs[0], Main.sLeft(resVal, 8), 1);
            SetRegister(regs[0], Main.sRight(resVal, 8), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_andi(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            ulong regVal = Convert.ToUInt64(GetRegister(regs[1], 1) + GetRegister(regs[1], 0), 16);
            ulong immVal = Convert.ToUInt64(GetRegister(regs[2], 1) + GetRegister(regs[2], 0), 16);
            string resVal = (regVal & immVal).ToString("X16");
            SetRegister(regs[0], Main.sLeft(resVal, 8), 1);
            SetRegister(regs[0], Main.sRight(resVal, 8), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_andis(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            ulong regVal = Convert.ToUInt64(GetRegister(regs[1], 1) + GetRegister(regs[1], 0), 16);
            uint immVal = (uint)ParseVal(regs[2], 0);
            string resVal = (regVal & (immVal << 16)).ToString("X16");
            SetRegister(regs[0], Main.sLeft(resVal, 8), 1);
            SetRegister(regs[0], Main.sRight(resVal, 8), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_b(string[] regs)
        {
            if (regs.Length != 1)
                return -1;

            uint addr = (uint)ParseVal(regs[0], 1);
            PCounter = addr - 4;

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_beq(string[] regs)
        {
            if (regs.Length != 1 && regs.Length != 2)
                return -1;

            int crNum = 0;
            if (regs.Length == 2)
                crNum = int.Parse(regs[0].Replace("cr", ""));

            uint addrTrue = (uint)((int)PCounter + (int)ParseVal(regs[regs.Length - 1], 0));
            uint cr = Convert.ToUInt32(GetRegister("cr", 0), 16);
            int sLeft = 28-(crNum*4);
            cr = (cr << sLeft) >> sLeft;
            cr = (cr >> (crNum * 4));

            if (cr == cmpEQ)
                PCounter = addrTrue - 4;

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_bge(string[] regs)
        {
            if (regs.Length != 1 && regs.Length != 2)
                return -1;

            int crNum = 0;
            if (regs.Length == 2)
                crNum = int.Parse(regs[0].Replace("cr", ""));

            uint addrTrue = (uint)((int)PCounter + (int)ParseVal(regs[regs.Length - 1], 0));
            uint cr = Convert.ToUInt32(GetRegister("cr", 0), 16);
            int sLeft = 28 - (crNum * 4);
            cr = (cr << sLeft) >> sLeft;
            cr = (cr >> (crNum * 4));

            if (cr == cmpEQ || cr == cmpGT)
                PCounter = addrTrue - 4;

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_bgt(string[] regs)
        {
            if (regs.Length != 1 && regs.Length != 2)
                return -1;

            int crNum = 0;
            if (regs.Length == 2)
                crNum = int.Parse(regs[0].Replace("cr", ""));

            uint addrTrue = (uint)((int)PCounter + (int)ParseVal(regs[regs.Length - 1], 0));
            uint cr = Convert.ToUInt32(GetRegister("cr", 0), 16);
            int sLeft = 28 - (crNum * 4);
            cr = (cr << sLeft) >> sLeft;
            cr = (cr >> (crNum * 4));

            if (cr == cmpGT)
                PCounter = addrTrue - 4;

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_bl(string[] regs)
        {
            if (regs.Length != 1)
                return -1;

            uint addr = (uint)ParseVal(regs[0], 1);
            SetRegister("lr", (PCounter + 4).ToString("X8"), 0);
            PCounter = addr - 4;

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_ble(string[] regs)
        {
            if (regs.Length != 1 && regs.Length != 2)
                return -1;

            int crNum = 0;
            if (regs.Length == 2)
                crNum = int.Parse(regs[0].Replace("cr", ""));

            uint addrTrue = (uint)((int)PCounter + (int)ParseVal(regs[regs.Length - 1], 0));
            uint cr = Convert.ToUInt32(GetRegister("cr", 0), 16);
            int sLeft = 28 - (crNum * 4);
            cr = (cr << sLeft) >> sLeft;
            cr = (cr >> (crNum * 4));

            if (cr == cmpEQ || cr == cmpLT)
                PCounter = addrTrue - 4;

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_blt(string[] regs)
        {
            if (regs.Length != 1 && regs.Length != 2)
                return -1;

            int crNum = 0;
            if (regs.Length == 2)
                crNum = int.Parse(regs[0].Replace("cr", ""));

            uint addrTrue = (uint)((int)PCounter + (int)ParseVal(regs[regs.Length - 1], 0));
            uint cr = Convert.ToUInt32(GetRegister("cr", 0), 16);
            int sLeft = 28 - (crNum * 4);
            cr = (cr << sLeft) >> sLeft;
            cr = (cr >> (crNum * 4));

            if (cr == cmpLT)
                PCounter = addrTrue - 4;

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_blr(string[] regs)
        {
            PCounter = Convert.ToUInt32(GetRegister("lr", 0), 16) - 4;

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_bne(string[] regs)
        {
            if (regs.Length != 1 && regs.Length != 2)
                return -1;

            int crNum = 0;
            if (regs.Length == 2)
                crNum = int.Parse(regs[0].Replace("cr", ""));

            uint addrTrue = (uint)((int)PCounter + (int)ParseVal(regs[regs.Length - 1], 0));
            uint cr = Convert.ToUInt32(GetRegister("cr", 0), 16);
            int sLeft = 28 - (crNum * 4);
            cr = (cr << sLeft) >> sLeft;
            cr = (cr >> (crNum * 4));

            if (cr != cmpEQ)
                PCounter = addrTrue - 4;

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_cmpwi(string[] regs)
        {
            if (regs.Length != 2 && regs.Length != 3)
                return -1;

            int crNum = 0;
            if (regs.Length == 3)
                crNum = int.Parse(regs[0].Replace("cr", ""));

            int immVal = (int)ParseVal(regs[regs.Length - 1], 0);
            int regVal = Convert.ToInt32(GetRegister(regs[regs.Length - 2], 0), 16);

            CompareSValues(crNum, regVal, immVal);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_cmplwi(string[] regs)
        {
            if (regs.Length != 2 && regs.Length != 3)
                return -1;

            int crNum = 0;
            if (regs.Length == 3)
                crNum = int.Parse(regs[0].Replace("cr", ""));

            uint immVal = (uint)ParseVal(regs[regs.Length - 1], 0);
            uint regVal = Convert.ToUInt32(GetRegister(regs[regs.Length - 2], 0), 16);

            CompareUValues(crNum, regVal, immVal);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_cmpw(string[] regs)
        {
            if (regs.Length != 2 && regs.Length != 3)
                return -1;

            int crNum = 0;
            if (regs.Length == 3)
                crNum = int.Parse(regs[0].Replace("cr", ""));

            int regVal2 = Convert.ToInt32(GetRegister(regs[regs.Length - 1], 0), 16);
            int regVal = Convert.ToInt32(GetRegister(regs[regs.Length - 2], 0), 16);

            CompareSValues(crNum, regVal, regVal2);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_cmpd(string[] regs)
        {
            if (regs.Length != 2 && regs.Length != 3)
                return -1;

            int crNum = 0;
            if (regs.Length == 3)
                crNum = int.Parse(regs[0].Replace("cr", ""));

            long regVal2 = Convert.ToInt64(GetRegister(regs[regs.Length - 1], 0), 16);
            long regVal = Convert.ToInt64(GetRegister(regs[regs.Length - 2], 0), 16);

            CompareSDValues(crNum, regVal, regVal2);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_cmplw(string[] regs)
        {
            if (regs.Length != 2 && regs.Length != 3)
                return -1;

            int crNum = 0;
            if (regs.Length == 3)
                crNum = int.Parse(regs[0].Replace("cr", ""));

            uint regVal2 = Convert.ToUInt32(GetRegister(regs[regs.Length - 1], 0), 16);
            uint regVal = Convert.ToUInt32(GetRegister(regs[regs.Length - 2], 0), 16);

            CompareUValues(crNum, regVal, regVal2);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_divw(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            long numerator = Convert.ToInt64(GetRegister(regs[1], 1) + GetRegister(regs[1], 0), 16);
            long denominator = Convert.ToInt64(GetRegister(regs[2], 1) + GetRegister(regs[2], 0), 16);
            string val = Main.sRight((numerator / denominator).ToString("X8"), 8);
            SetRegister(regs[0], val, 0);
            SetRegister(regs[0], "00000000", 1);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_fadd(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            Double res = Convert.ToDouble(GetRegister(regs[1], 0)) + Convert.ToDouble(GetRegister(regs[2], 0));
            SetRegister(regs[0], res.ToString("G"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_fadds(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            Single res = Convert.ToSingle(GetRegister(regs[1], 0)) + Convert.ToSingle(GetRegister(regs[2], 0));
            SetRegister(regs[0], res.ToString("G"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_fcfid(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            Double res = (Double)Convert.ToInt64(DoubleToHex(Convert.ToDouble(GetRegister(regs[1], 0))), 16);
            SetRegister(regs[0], res.ToString("G"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_fcmpu(string[] regs)
        {
            if (regs.Length != 2 && regs.Length != 3)
                return -1;

            int crNum = 0;
            if (regs.Length == 3)
                crNum = int.Parse(regs[0].Replace("cr", ""));

            Double reg1 = Convert.ToDouble(GetRegister(regs[regs.Length - 2], 0));
            Double reg2 = Convert.ToDouble(GetRegister(regs[regs.Length - 1], 0));

            CompareFValues(crNum, reg1, reg2);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_fdiv(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            Double res = Convert.ToDouble(GetRegister(regs[1], 0)) / Convert.ToDouble(GetRegister(regs[2], 0));
            SetRegister(regs[0], res.ToString("G"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_fdivs(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            Single res = Convert.ToSingle(GetRegister(regs[1], 0)) / Convert.ToSingle(GetRegister(regs[2], 0));
            SetRegister(regs[0], res.ToString("G"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_fmadd(string[] regs)
        {
            if (regs.Length != 4)
                return -1;

            Double res = Convert.ToDouble(GetRegister(regs[1], 0)) * Convert.ToDouble(GetRegister(regs[2], 0));
            res += Convert.ToDouble(GetRegister(regs[3], 0));
            SetRegister(regs[0], res.ToString("G"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_fmadds(string[] regs)
        {
            if (regs.Length != 4)
                return -1;

            Single res = Convert.ToSingle(GetRegister(regs[1], 0)) * Convert.ToSingle(GetRegister(regs[2], 0));
            res += Convert.ToSingle(GetRegister(regs[3], 0));
            SetRegister(regs[0], res.ToString("G"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_fmr(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            SetRegister(regs[0], GetRegister(regs[1], 0), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_fmuls(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            Single res = Convert.ToSingle(GetRegister(regs[1], 0)) * Convert.ToSingle(GetRegister(regs[2], 0));
            SetRegister(regs[0], res.ToString("G"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_fmul(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            Double res = Convert.ToDouble(GetRegister(regs[1], 0)) * Convert.ToDouble(GetRegister(regs[2], 0));
            SetRegister(regs[0], res.ToString("G"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_frsp(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            Single res = (Single)Convert.ToDouble(GetRegister(regs[1], 0));
            SetRegister(regs[0], res.ToString("G"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_fsqrt(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            Double res = Math.Sqrt(Convert.ToDouble(GetRegister(regs[1], 0)));
            SetRegister(regs[0], res.ToString("G"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_fsubs(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            Single res = Convert.ToSingle(GetRegister(regs[1], 0)) - Convert.ToSingle(GetRegister(regs[2], 0));
            SetRegister(regs[0], res.ToString("G"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_fsub(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            Double res = Convert.ToDouble(GetRegister(regs[1], 0)) - Convert.ToDouble(GetRegister(regs[2], 0));
            SetRegister(regs[0], res.ToString("G"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_lbz(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            uint addr = CalculateOffImm(regs[1]);
            uint byteVal = GetMemory(addr, 1);
            SetRegister(regs[0], byteVal.ToString("X8"), 0);
            SetRegister(regs[0], "00000000", 1);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_lbzx(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            uint addr = Convert.ToUInt32(GetRegister(regs[1], 0), 16) + Convert.ToUInt32(GetRegister(regs[2], 0), 16);
            SetRegister(regs[0], GetMemory(addr, 1).ToString("X8"), 0);
            SetRegister(regs[0], "00000000", 1);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_ld(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            uint addr = CalculateOffImm(regs[1]);
            uint wordVal1 = GetMemory(addr, 4);
            uint wordVal2 = GetMemory(addr + 4, 4);
            SetRegister(regs[0], wordVal1.ToString("X8"), 0);
            SetRegister(regs[0], wordVal2.ToString("X8"), 1);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_ldx(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            uint addr = Convert.ToUInt32(GetRegister(regs[1], 0), 16) + Convert.ToUInt32(GetRegister(regs[2], 0), 16);
            uint wordVal1 = GetMemory(addr, 4);
            uint wordVal2 = GetMemory(addr + 4, 4);
            SetRegister(regs[0], wordVal1.ToString("X8"), 0);
            SetRegister(regs[0], wordVal2.ToString("X8"), 1);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_lfs(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            uint addr = CalculateOffImm(regs[1]);
            uint wordVal = GetMemory(addr, 4);
            SetRegister(regs[0], HexToSingle(wordVal.ToString("X8")).ToString("G"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_lfd(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            uint addr = CalculateOffImm(regs[1]);
            uint wordVal = GetMemory(addr, 4);
            uint wordVal2 = GetMemory(addr + 4, 4);
            double res = HexToDouble(wordVal.ToString("X8") + wordVal2.ToString("X8"));
            SetRegister(regs[0], res.ToString("G"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_lhz(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            uint addr = CalculateOffImm(regs[1]);
            uint byteVal = GetMemory(addr, 2);
            SetRegister(regs[0], "0000" + byteVal.ToString("X4"), 0);
            SetRegister(regs[0], "00000000", 1);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_lhzx(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            uint addr = Convert.ToUInt32(GetRegister(regs[1], 0), 16) + Convert.ToUInt32(GetRegister(regs[2], 0), 16);
            SetRegister(regs[0], GetMemory(addr, 2).ToString("X8"), 0);
            SetRegister(regs[0], "00000000", 1);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_lis(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            string value = (ParseVal(regs[1], 0) << 16).ToString("X8");
            SetRegister(regs[0], value, 0);
            if (value[0] >= '8')
                SetRegister(regs[0], "FFFFFFFF", 1);
            else
                SetRegister(regs[0], "00000000", 1);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_li(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            string value = ParseVal(regs[1], 0).ToString("X4");
            if (value[0] >= '8')
            {
                SetRegister(regs[0], "FFFF" + value, 0);
                SetRegister(regs[0], "FFFFFFFF", 1);
            }
            else
            {
                SetRegister(regs[0], "0000" + value, 0);
                SetRegister(regs[0], "00000000", 1);
            }

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_lwzx(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            uint addr = Convert.ToUInt32(GetRegister(regs[1], 0), 16) + Convert.ToUInt32(GetRegister(regs[2], 0), 16);
            SetRegister(regs[0], GetMemory(addr, 4).ToString("X8"), 0);
            SetRegister(regs[0], "00000000", 1);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_lwz(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            uint addr = CalculateOffImm(regs[1]);
            SetRegister(regs[0], GetMemory(addr, 4).ToString("X8"), 0);
            SetRegister(regs[0], "00000000", 1);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_mfspr(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            SetRegister(regs[0], GetRegister(regs[1], 0), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_mtspr(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            SetRegister(regs[0], GetRegister(regs[1], 0), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_mullw(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            int regVal = Convert.ToInt32(GetRegister(regs[1], 0), 16);
            int regVal2 = Convert.ToInt32(GetRegister(regs[2], 0), 16);
            string mulVal = (regVal * regVal2).ToString("X16");
            SetRegister(regs[0], Main.sRight(mulVal, 8), 0);
            SetRegister(regs[0], Main.sLeft(mulVal, 8), 1);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_mulli(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            long regVal = Convert.ToInt64(GetRegister(regs[1], 1) + GetRegister(regs[1], 0), 16);
            long immVal = (long)ParseVal(regs[2], 1);
            string mulVal = (regVal * immVal).ToString("X16");
            SetRegister(regs[0], Main.sRight(mulVal, 8), 0);
            SetRegister(regs[0], Main.sLeft(mulVal, 8), 1);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_ori(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            ulong regVal = Convert.ToUInt64(GetRegister(regs[1], 1) + GetRegister(regs[1], 0), 16);
            uint immVal = (uint)ParseVal(regs[2], 0);
            string resVal = (regVal | immVal).ToString("X16");
            SetRegister(regs[0], Main.sLeft(resVal, 8), 1);
            SetRegister(regs[0], Main.sRight(resVal, 8), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_or(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            ulong regVal = Convert.ToUInt64(GetRegister(regs[1], 1) + GetRegister(regs[1], 0), 16);
            ulong immVal = Convert.ToUInt64(GetRegister(regs[2], 1) + GetRegister(regs[2], 0), 16);
            string resVal = (regVal | immVal).ToString("X16");
            SetRegister(regs[0], Main.sLeft(resVal, 8), 1);
            SetRegister(regs[0], Main.sRight(resVal, 8), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_oris(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            ulong regVal = Convert.ToUInt64(GetRegister(regs[1], 1) + GetRegister(regs[1], 0), 16);
            uint immVal = (uint)ParseVal(regs[2], 0);
            string resVal = (regVal | (immVal << 16)).ToString("X16");
            SetRegister(regs[0], Main.sLeft(resVal, 8), 1);
            SetRegister(regs[0], Main.sRight(resVal, 8), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_sldi(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            ulong regVal = Convert.ToUInt64(GetRegister(regs[1], 1) + GetRegister(regs[1], 0), 16);
            int immVal = (int)ParseVal(regs[2], 0);
            string resVal = (regVal << immVal).ToString("X16");
            SetRegister(regs[0], Main.sLeft(resVal, 8), 1);
            SetRegister(regs[0], Main.sRight(resVal, 8), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_srdi(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            ulong regVal = Convert.ToUInt64(GetRegister(regs[1], 1) + GetRegister(regs[1], 0), 16);
            int immVal = (int)ParseVal(regs[2], 0);
            string resVal = (regVal >> immVal).ToString("X16");
            SetRegister(regs[0], Main.sLeft(resVal, 8), 1);
            SetRegister(regs[0], Main.sRight(resVal, 8), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_slwi(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            int regVal = Convert.ToInt32(GetRegister(regs[1], 0), 16);
            int immVal = (int)ParseVal(regs[2], 0);
            SetRegister(regs[0], ((Int32)(regVal << immVal)).ToString("X8"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_srwi(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            int regVal = Convert.ToInt32(GetRegister(regs[1], 0), 16);
            int immVal = (int)ParseVal(regs[2], 0);
            SetRegister(regs[0], ((Int32)(regVal >> immVal)).ToString("X8"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_slw(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            int regVal = Convert.ToInt32(GetRegister(regs[1], 0), 16);
            int immVal = Convert.ToInt32(GetRegister(regs[2], 0), 16);
            SetRegister(regs[0], ((Int32)(regVal << immVal)).ToString("X8"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_srw(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            int regVal = Convert.ToInt32(GetRegister(regs[1], 0), 16);
            int immVal = Convert.ToInt32(GetRegister(regs[2], 0), 16);
            SetRegister(regs[0], ((Int32)(regVal >> immVal)).ToString("X8"), 0);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_stb(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            uint storeAddr = CalculateOffImm(regs[1]);
            SetMemory(storeAddr, (uint)ParseVal("0x" + GetRegister(regs[0], 0), 1), 1);

            RefreshMem = true;
            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_stbx(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            uint addr = Convert.ToUInt32(GetRegister(regs[1], 0), 16) + Convert.ToUInt32(GetRegister(regs[2], 0), 16);
            SetMemory(addr, Convert.ToUInt16(Main.sRight(GetRegister(regs[0], 0), 2), 16), 1);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_stdx(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            uint addr = Convert.ToUInt32(GetRegister(regs[1], 0), 16) + Convert.ToUInt32(GetRegister(regs[2], 0), 16);
            SetMemory(addr, Convert.ToUInt32(GetRegister(regs[0], 1), 16), 4);
            SetMemory(addr + 4, Convert.ToUInt32(GetRegister(regs[0], 0), 16), 4);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_std(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            uint storeAddr = CalculateOffImm(regs[1]);
            SetMemory(storeAddr, (uint)ParseVal("0x" + GetRegister(regs[0], 1), 1), 4);
            SetMemory(storeAddr + 4, (uint)ParseVal("0x" + GetRegister(regs[0], 0), 1), 4);

            RefreshMem = true;
            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_stdu(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            uint storeAddr = CalculateOffImm(regs[1]);
            SetMemory(storeAddr, (uint)ParseVal("0x" + GetRegister(regs[0], 1), 1), 4);
            SetMemory(storeAddr + 4, (uint)ParseVal("0x" + GetRegister(regs[0], 0), 1), 4);

            //Update
            SetRegister(regs[1].Split('(')[1].Replace(")", ""), storeAddr.ToString("X8"), 0);

            RefreshMem = true;
            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_sth(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            uint storeAddr = CalculateOffImm(regs[1]);
            SetMemory(storeAddr, (uint)ParseVal("0x" + GetRegister(regs[0], 0), 0), 2);

            RefreshMem = true;
            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_sthx(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            uint addr = Convert.ToUInt32(GetRegister(regs[1], 0), 16) + Convert.ToUInt32(GetRegister(regs[2], 0), 16);
            SetMemory(addr, Convert.ToUInt16(Main.sRight(GetRegister(regs[0], 0), 4), 16), 2);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_stfs(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            uint addr = CalculateOffImm(regs[1]);
            string val = SingleToHex(Single.Parse(GetRegister(regs[0], 0)));
            SetMemory(addr, uint.Parse(
                                    val,
                                System.Globalization.NumberStyles.HexNumber),
                            4);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_stfd(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            uint addr = CalculateOffImm(regs[1]);
            string val = DoubleToHex(Double.Parse(GetRegister(regs[0], 0)));
            SetMemory(addr, Convert.ToUInt32(Main.sLeft(val, 8), 16), 4);
            SetMemory(addr + 4, Convert.ToUInt32(Main.sRight(val, 8), 16), 4);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_stwx(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            uint addr = Convert.ToUInt32(GetRegister(regs[1], 0), 16) + Convert.ToUInt32(GetRegister(regs[2], 0), 16);
            SetMemory(addr, Convert.ToUInt32(GetRegister(regs[0], 0), 16), 4);

            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_stw(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            uint storeAddr = CalculateOffImm(regs[1]);
            SetMemory(storeAddr, (uint)ParseVal("0x" + GetRegister(regs[0], 0), 1), 4);

            RefreshMem = true;
            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_stwu(string[] regs)
        {
            if (regs.Length != 2)
                return -1;

            uint storeAddr = CalculateOffImm(regs[1]);
            SetMemory(storeAddr, (uint)ParseVal("0x" + GetRegister(regs[0], 0), 1), 4);

            //Update
            SetRegister(regs[1].Split('(')[1].Replace(")", ""), storeAddr.ToString("X8"), 0);

            RefreshMem = true;
            StepOver_Click(null, null);
            return 0;
        }

        public int EMU_subf(string[] regs)
        {
            if (regs.Length != 3)
                return -1;

            string val = (~Convert.ToInt32(GetRegister(regs[1], 0), 16) + Convert.ToInt32(GetRegister(regs[2], 0), 16) + 1).ToString("X8");
            SetRegister(regs[0], val, 0);

            StepOver_Click(null, null);
            return 0;
        }

        /* ---------- Form events ---------- */
        private void GPregLab_Click(object sender, EventArgs e)
        {
            int x = 0;
            for (x = 0; x < regBoxes.Length; x++)
            {
                if (x < 32)
                    regBoxes[x].Visible = true;
                else
                    regBoxes[x].Visible = false;  
            }
            for (x = 0; x < fprBoxes.Length; x++)
                fprBoxes[x].Visible = false;

            GPregLab.BorderStyle = BorderStyle.Fixed3D;
            FPregLab.BorderStyle = BorderStyle.None;
            SPregLab.BorderStyle = BorderStyle.None;
        }

        private void FPregLab_Click(object sender, EventArgs e)
        {
            int x = 0;
            for (x = 0; x < regBoxes.Length; x++)
                regBoxes[x].Visible = false;
            for (x = 0; x < fprBoxes.Length; x++)
                fprBoxes[x].Visible = true;

            GPregLab.BorderStyle = BorderStyle.None;
            FPregLab.BorderStyle = BorderStyle.Fixed3D;
            SPregLab.BorderStyle = BorderStyle.None;
        }

        private void SPregLab_Click(object sender, EventArgs e)
        {
            int x = 0;
            for (x = 0; x < regBoxes.Length; x++)
            {
                if (x < 32 || x > 36)
                    regBoxes[x].Visible = false;
                else
                    regBoxes[x].Visible = true;
            }
            for (x = 0; x < fprBoxes.Length; x++)
                fprBoxes[x].Visible = false;

            GPregLab.BorderStyle = BorderStyle.None;
            FPregLab.BorderStyle = BorderStyle.None;
            SPregLab.BorderStyle = BorderStyle.Fixed3D;
        }

        private void ASMEmu_Shown(object sender, EventArgs e)
        {
            int pointX = 5;
            int pointY = 15;
            int bits = 63;
            int y = 0;
            bool visibility = true;
            regBoxes = new PPCReg[32 + 1 + 4];
            fprBoxes = new FPRReg[32];

            for (int x = 0; x < regBoxes.Length; x++)
            {
                PPCReg tempReg = new PPCReg();
                tempReg.RegName = ASMDeclarations.RegColArr[y].reg;

                if (y == 32)
                {
                    //Add button at the bottom of the GroupBox that clears the registers
                    Button clearRegs = new Button();
                    clearRegs.Text = "Clear Registers";
                    clearRegs.Name = "clearRegs";
                    clearRegs.AutoSize = true;
                    clearRegs.FlatStyle = FlatStyle.Popup;
                    clearRegs.Visible = true;
                    clearRegs.Click += new EventHandler(clearRegs_Click);
                    RegGroup.Controls.Add(clearRegs);
                    clearRegs.Location = new Point((RegGroup.Width / 2) - (clearRegs.Width / 2), pointY + (clearRegs.Height / 2));

                    pointX = 5;
                    pointY = 15;
                    bits = 31;
                    visibility = false;
                    tempReg.RegName = "cr";
                    y = 39;
                }

                RegGroup.Controls.Add(tempReg);

                tempReg.Location = new Point(pointX, pointY);
                tempReg.Visible = visibility;
                tempReg.BitOrder = Main.bitOrder;
                tempReg.Bits = bits;
                pointY += tempReg.Height - 5;

                regBoxes[x] = tempReg;
                y++;
            }

            pointX = 5;
            pointY = 15;
            for (int x = 0; x < fprBoxes.Length; x++)
            {
                FPRReg tempReg = new FPRReg();
                tempReg.RegName = ASMDeclarations.RegColArr[y].reg;

                RegGroup.Controls.Add(tempReg);

                tempReg.Location = new Point(pointX, pointY);
                tempReg.Visible = false;
                pointY += tempReg.Height - 5;

                fprBoxes[x] = tempReg;
                y++;
            }



            SetBitOrder();

            //Create dummy file
            string appDir = Main.DirOf(Application.ExecutablePath);
            memoryBin = appDir + "\\tempMemory.bin";
            System.IO.File.WriteAllBytes(memoryBin, new byte[Main.maxMemRange - Main.minMemRange]);
            FileIO.InsertCode(memoryBin, codes, Main.minMemRange);
            if (codes.Length > 0)
                PCounter = codes[0].addr;
            DrawMemoryBox(PCounter, 44 * 4);

            //Set register r1 to default stack value
            SetRegister("r1", Main.startStackPtr.ToString("X8"), 0);
        }

        private void ASMEmu_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (System.IO.File.Exists(memoryBin))
                System.IO.File.Delete(memoryBin);
        }

        private void ASMEmu_Load(object sender, EventArgs e)
        {
            //Use ASMEmu_Shown instead
        }

        private void ASMEmu_Resize(object sender, EventArgs e)
        {
            if (this.WindowState.ToString() == "Maximized")
                this.MaximizeBox = false;
            else
                this.MaximizeBox = true;

            //RegGroup
            RegGroup.Height = this.Height - 70;
            GPregLab.Top = 10 - (GPregLab.Height / 2);
            SPregLab.Top = 10 - (SPregLab.Height / 2);
            FPregLab.Top = 10 - (FPregLab.Height / 2);
            RegGroup.Top = 10;

            //ControlsBox
            ControlsBox.Height = this.Height - 70;
            ControlsBox.Left = this.Width - 210;
            ControlsBox.Top = 10;
            StepBack.Top = ControlsBox.Height - StepBack.Height - 5;
            StepOver.Top = StepBack.Top - StepOver.Height - 5;
            StepInto.Top = StepOver.Top - StepInto.Height - 5;
            GotoAddr.Top = StepInto.Top - GotoAddr.Height - 5;
            GotoAddrBox.Top = GotoAddr.Top;
            RunCycles.Top = GotoAddrBox.Top - RunCycles.Height - 5;
            PBcycles.Top = RunCycles.Top;
            CyclesBox.Top = RunCycles.Top;
            importBINELF.Top = RunCycles.Top - importBINELF.Height - 5;
            debugBox.Height = ControlsBox.Height - (ControlsBox.Height - importBINELF.Top) - 25;

            //MemoryBox
            MemoryBox.Height = this.Height - 70;
            MemoryBox.Left = RegGroup.Left + RegGroup.Width + 5;
            MemoryBox.Width = ControlsBox.Left - MemoryBox.Left - 5;
            MemoryBox.Top = 10;
            ASMBox.Width = MemoryBox.Width - ASMBox.Left - 6;
            ASMBox.Height = MemoryBox.Height - 10;
            ValueBox.Height = MemoryBox.Height - 10;
            AddressBox.Height = MemoryBox.Height - 10;
            StringBox.Height = MemoryBox.Height - 10;
        }

        bool firstIndChange = false;
        private void AddressBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (firstIndChange == false)
            {
                firstIndChange = true;
                ValueBox.SelectedIndex = AddressBox.SelectedIndex;
                StringBox.SelectedIndex = AddressBox.SelectedIndex;
                ASMBox.SelectedIndex = AddressBox.SelectedIndex;
                PCounter = Convert.ToUInt32(AddressBox.Items[AddressBox.SelectedIndex].ToString(), 16);
                firstIndChange = false;
            }
        }

        private void ValueBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (firstIndChange == false)
            {
                firstIndChange = true;
                AddressBox.SelectedIndex = ValueBox.SelectedIndex;
                StringBox.SelectedIndex = ValueBox.SelectedIndex;
                ASMBox.SelectedIndex = ValueBox.SelectedIndex;
                PCounter = Convert.ToUInt32(AddressBox.Items[ValueBox.SelectedIndex].ToString(), 16);
                firstIndChange = false;
            }
        }

        private void StringBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (firstIndChange == false)
            {
                firstIndChange = true;
                AddressBox.SelectedIndex = StringBox.SelectedIndex;
                ValueBox.SelectedIndex = StringBox.SelectedIndex;
                ASMBox.SelectedIndex = StringBox.SelectedIndex;
                PCounter = Convert.ToUInt32(AddressBox.Items[StringBox.SelectedIndex].ToString(), 16);
                firstIndChange = false;
            }
        }

        private void ASMBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (firstIndChange == false)
            {
                firstIndChange = true;
                ValueBox.SelectedIndex = ASMBox.SelectedIndex;
                AddressBox.SelectedIndex = ASMBox.SelectedIndex;
                StringBox.SelectedIndex = ASMBox.SelectedIndex;
                PCounter = Convert.ToUInt32(AddressBox.Items[ASMBox.SelectedIndex].ToString(), 16);
                firstIndChange = false;
            }
        }

        private void StepOver_Click(object sender, EventArgs e)
        {
            uint min = Convert.ToUInt32(AddressBox.Items[0].ToString(), 16);
            uint max = Convert.ToUInt32(AddressBox.Items[43].ToString(), 16);

            if (RefreshMem)
                UpdateMemBoxCustom(min, ASMBox.SelectedIndex);
            RefreshMem = false;

            PCounter += 4;
            if (PCounter < Main.minMemRange || PCounter > Main.maxMemRange)
                return;

            if (PCounter > max || PCounter < min)
                UpdateMemBox();
            else if (ASMBox.SelectedIndex != 43)
                ASMBox.SelectedIndex = (int)((PCounter - min) / 4);
            else
                UpdateMemBox();
        }

        private void StepBack_Click(object sender, EventArgs e)
        {
            uint min = Convert.ToUInt32(AddressBox.Items[0].ToString(), 16);
            uint max = Convert.ToUInt32(AddressBox.Items[43].ToString(), 16);

            if (RefreshMem)
                UpdateMemBoxCustom(min, ASMBox.SelectedIndex);
            RefreshMem = false;

            PCounter -= 4;
            if (PCounter < Main.minMemRange || PCounter > Main.maxMemRange)
                return;

            if (PCounter > max || PCounter < min || (ASMBox.SelectedIndex == 0 && min > Main.minMemRange))
                UpdateMemBoxCustom(min - (44 * 4), 43);
            else if (ASMBox.SelectedIndex != 0)
                ASMBox.SelectedIndex = (int)((PCounter - min) / 4);
            else
                UpdateMemBox();
        }

        private void StepInto_Click(object sender, EventArgs e)
        {
            string[] asm = ASMBox.Items[ASMBox.SelectedIndex].ToString().Split(' ');
            string op = asm[0];
            string[] regs = new string[asm.Length - 1];
            for (int x = 1; x < asm.Length; x++)
                regs[x-1] = asm[x].Replace(",", "").Replace("$", "").Replace("%", "");

            if (RunASM(op, regs) < 0)
                debugBox.Text += "Instruction " + op + " not supported!" + Environment.NewLine;
        }

        private void GotoAddr_Click(object sender, EventArgs e)
        {
            uint addr = 0;
            try
            {
                addr = uint.Parse(GotoAddrBox.Text, System.Globalization.NumberStyles.HexNumber);
            }
            catch
            {
                MessageBox.Show("GoTo Address is not a valid hexadecimal value!");
                return;
            }

            PCounter = addr;
            UpdateMemBox();
        }

        Single ratio = 0;
        private void RunCycles_Click(object sender, EventArgs e)
        {
            int cycles = (int)ParseVal(CyclesBox.Text, 1);
            PBcycles.Visible = true;
            for (int x = 0; x < cycles; x++)
            {
                ratio = (Single)x / (Single)cycles;
                PBcycles.Refresh();
                StepInto_Click(null, null);
                Application.DoEvents();
            }
            PBcycles.Visible = false;
        }

        /* Draws the progress bar */
        private void PBcycles_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawLine(
                new Pen(Color.White, 25f),
                new Point(0, PBcycles.Size.Height / 2),
                new Point((int)(PBcycles.Size.Width * ratio), PBcycles.Size.Height / 2));
        }

        private void AddressBox_KeyDown(object sender, KeyEventArgs e)
        {
            int index = AddressBox.SelectedIndex;
            e.SuppressKeyPress = false;
            uint addr = uint.Parse(AddressBox.Items[index].ToString(), System.Globalization.NumberStyles.HexNumber);
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.F10)
            {
                if (index == 43)
                {
                    if (addr < Main.maxMemRange)
                        StepOver_Click(null, null);
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (index == 0)
                {
                    if (addr > Main.minMemRange)
                        StepBack_Click(null, null);
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.F11)
                StepInto_Click(null, null);
        }

        private void ValueBox_KeyDown(object sender, KeyEventArgs e)
        {
            int index = ValueBox.SelectedIndex;
            e.SuppressKeyPress = false;
            uint addr = uint.Parse(AddressBox.Items[index].ToString(), System.Globalization.NumberStyles.HexNumber);
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.F10)
            {
                if (index == 43)
                {
                    if (addr < Main.maxMemRange)
                        StepOver_Click(null, null);
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (index == 0)
                {
                    if (addr > Main.minMemRange)
                        StepBack_Click(null, null);
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.F11)
                StepInto_Click(null, null);
        }

        private void ASMBox_KeyDown(object sender, KeyEventArgs e)
        {
            int index = ASMBox.SelectedIndex;
            e.SuppressKeyPress = false;
            uint addr = uint.Parse(AddressBox.Items[index].ToString(), System.Globalization.NumberStyles.HexNumber);
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.F10)
            {
                if (index == 43)
                {
                    if (addr < Main.maxMemRange)
                        StepOver_Click(null, null);
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (index == 0)
                {
                    if (addr > Main.minMemRange)
                        StepBack_Click(null, null);
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.F11)
                StepInto_Click(null, null);
        }

        private void clearRegs_Click(object sender, EventArgs e)
        {
            for (int x = 0; x < regBoxes.Length; x++)
            {
                regBoxes[x].SetReg(0, "00000000");
                regBoxes[x].SetReg(1, "00000000");
                regBoxes[x].SetReg(2, "00000000");
                regBoxes[x].SetReg(3, "00000000");
                regBoxes[x].SetForeColor(0, 3, Color.White);
            }

            for (int x = 0; x < fprBoxes.Length; x++)
            {
                fprBoxes[x].SetReg("0.0");
                fprBoxes[x].SetForeColor(Color.White);
            }
        }

        private void StringBox_KeyDown(object sender, KeyEventArgs e)
        {
            int index = StringBox.SelectedIndex;
            e.SuppressKeyPress = false;
            uint addr = uint.Parse(AddressBox.Items[index].ToString(), System.Globalization.NumberStyles.HexNumber);
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.F10)
            {
                if (index == 43)
                {
                    if (addr < Main.maxMemRange)
                        StepOver_Click(null, null);
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (index == 0)
                {
                    if (addr > Main.minMemRange)
                        StepBack_Click(null, null);
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.F11)
                StepInto_Click(null, null);
        }

        private void importBINELF_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(ofd.FileName);
                if (fi.Extension.ToLower() == ".elf")
                {
                    LoadFile_ELF(fi.FullName);
                }
                else if (fi.Extension.ToLower() == ".bin")
                {
                    LoadFile_BIN(fi.FullName);
                }
                else
                {
                    MessageBox.Show("I need a .bin or a .elf to load!\nIf it the extension isn't either then please change it appropriately");
                }
            }

            FileIO.InsertCode(memoryBin, codes, Main.minMemRange);
        }

        void LoadFile_ELF(string filename)
        {
            uint offset = 0x00010000;
            uint buffer = FileIO.GrabMemory(filename, 0x2C, 4, 0)[0];

            if (buffer < Main.maxMemRange)
            {
                byte[] elf = new byte[buffer - offset];
                System.IO.FileStream fs = System.IO.File.OpenRead(filename);
                fs.Read(elf, 0, (int)buffer - (int)offset);
                fs.Close();

                using (System.IO.Stream stream = System.IO.File.Open(memoryBin, System.IO.FileMode.Open))
                {
                    stream.Position = offset;
                    stream.Write(elf, 0, elf.Length);
                }
            }
            else
            {
                MessageBox.Show("ELF File loads past max memory range (" + buffer.ToString("X8") + ")!\nPlease increase the max range before trying to import it again.");
                return;
            }

        }

        void LoadFile_BIN(string filename)
        {
            byte[] bin = System.IO.File.ReadAllBytes(filename);
            int offset = 0;

            Main.IBArg[] a = new Main.IBArg[1];
            a[0].label = "Enter a memory offset to start loading from:";
            a[0].defStr = "00000000";

            a = Main.CallIBox(a, this);
            if (a != null && a[0].retStr != "")
            {
                offset = Convert.ToInt32(a[0].retStr, 16);
                if ((bin.Length + offset) < Main.maxMemRange)
                {
                    using (System.IO.Stream stream = System.IO.File.Open(memoryBin, System.IO.FileMode.Open))
                    {
                        stream.Position = offset;
                        stream.Write(bin, 0, bin.Length);
                    }
                }
                else
                {
                    MessageBox.Show("BIN File loads past max memory range (" + (bin.Length + offset).ToString("X8") + ")!\nPlease increase the max range before trying to import it again.");
                    return;
                }
            }
                
        }

    }
}
