using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HifiFirmwareGenerator
{
    class ExtractFile
    {
        private string mPath;

        public ExtractFile(string path)
        {
            mPath = path;
        }

        public void Parse(Firmware firmware)
        {
            StreamReader sr = new StreamReader(mPath, Encoding.Default);

            Console.WriteLine("Parsing file: {0}...", mPath);

            while (sr.EndOfStream == false)
            {
                string line = sr.ReadLine();

                // Parse line like: C:00000040 32
                char[] split = { ':', ' ' };
                string[] parts = line.Split(split);
                Char type = Convert.ToChar(parts[0]);
                UInt32 address = UInt32.Parse(parts[1], System.Globalization.NumberStyles.HexNumber);
                Byte value = Byte.Parse(parts[2], System.Globalization.NumberStyles.HexNumber);

                //Console.WriteLine("Parse a line: type={0}, address={1}, value={2}", type, address, value);
                //Console.ReadKey();

                if (type.Equals('C'))
                    firmware.AddCode(address, value);
                else
                    firmware.AddData(address, value);
            }
        }
    }
}
