using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace CodeWizardPS3
{
    class FileIO
    {
        public static void SaveCWP3(RichTextBox[] rtb, string file)
        {
            using (StreamWriter sr = new StreamWriter(file))
            {
                foreach (RichTextBox tempRTB in rtb)
                    sr.Write(tempRTB.Rtf.ToString() + "\n[]\n");
            }
        }

        public static string[] LoadCWP3(string file)
        {
            string[] retStr = new string[0];
            if (File.Exists(file))
            {
                string fullText = File.ReadAllText(file);
                string[] rtbText = fullText.Split(new string[] { "\n[]\n" }, StringSplitOptions.RemoveEmptyEntries);
                //Save each rtb as a temp and load it into the retRTB array
                foreach (string str in rtbText)
                {
                    if (str != "")
                    {
                        //Save to temp file
                        string tempFileN = System.IO.Path.GetTempFileName();
                        File.WriteAllText(tempFileN, str);

                        int ind = retStr.Length;
                        Array.Resize(ref retStr, retStr.Length + 1);
                        retStr[ind] = tempFileN;
                    }
                }

            }
            return retStr;
        }

        public static void SaveFile(string file, string asm)
        {
            if (file == "" || file == null)
            {
                //System.Windows.Forms.MessageBox.Show("Error: File path invalid!");
                return;
            }

            if (Directory.Exists(file) == false)
                Directory.CreateDirectory(Main.DirOf(file));
            File.WriteAllText(file, asm);
        }

        public static string OpenFile(string file)
        {
            if (file == "" || file == null || File.Exists(file) == false)
            {
                //System.Windows.Forms.MessageBox.Show("Error: File path invalid!");
                return null;
            }
            return File.ReadAllText(file);
        }

        public static void InsertCode(string filename, ASMEmu.MemCode[] codes, uint offset)
        {
            if (codes == null)
                return;

            for (int x = 0; x < codes.Length; x++)
            {
                int pos = (int)(codes[x].addr - offset);
                byte[] data = BitConverter.GetBytes(codes[x].val);
                Array.Reverse(data);
                using (Stream stream = File.Open(filename, FileMode.Open))
                {
                    stream.Position = pos;
                    stream.Write(data, 0, data.Length);
                }
            }
        }

        public static void SetMemory(string filename, uint start, uint val, int length, uint offset)
        {
            int pos = (int)(start - offset);
            byte[] data = BitConverter.GetBytes(val);
            Array.Reverse(data, 0, length);
            using (Stream stream = File.Open(filename, FileMode.Open))
            {
                stream.Position = pos;
                stream.Write(data, 0, length);
            }
        }

        public static uint[] GrabMemory(string filename, uint start, int length, uint offset)
        {
            start -= offset;
            if (length <= 0 || File.Exists(filename) == false)
                return null;

            int len = 4;
            if (length < 4)
                len = length;

            byte[] data = new byte[length];
            using (Stream stream = File.Open(filename, FileMode.Open))
            {
                stream.Position = start;
                stream.Read(data, 0, length);
            }

            uint[] ret;
            if ((length / 4) <= 0)
                ret = new uint[1];
            else
                ret = new uint[(length / 4)];

            int y = 0;
            for (int x = 0; x < data.Length; x += 4)
            {
                byte[] convertData = new byte[len];
                Array.Copy(data, x, convertData, 0, len);
                Array.Reverse(convertData);
                if (len == 3 || len == 4)
                    ret[y] = BitConverter.ToUInt32(convertData, 0);
                else if (len == 2)
                    ret[y] = BitConverter.ToUInt16(convertData, 0);
                else if (len == 1)
                    ret[y] = (uint)convertData[0];
                y++;
            }

            return ret;
        }

    }
}
