using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using System.Collections;

namespace HifiFirmwareGenerator
{
    class Program
    {
        public static string VERSION = "V0.1.0";
        public static string PROCESS_DIRECTORY = "process";
        public static string OUTPUT_DIRECTORY = "output";

        static void Prepare()
        {
            string currPath = System.IO.Directory.GetCurrentDirectory();

            /* Create directory "process" */
            string subPath = currPath + "\\" + PROCESS_DIRECTORY;

            if (System.IO.Directory.Exists(subPath))
            {
                foreach (FileInfo file in (new DirectoryInfo(subPath)).GetFiles())
                {
                    file.Attributes = FileAttributes.Normal;
                    file.Delete();
                }
            }
            else
            {
                System.IO.Directory.CreateDirectory(subPath);
            }
       
            /* Create directory "output" */
            subPath = currPath + "\\" + OUTPUT_DIRECTORY;
            if (!System.IO.Directory.Exists(subPath))
            {
                System.IO.Directory.CreateDirectory(subPath);
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Ceva Firmware Generator {0} [for Rockchip platforms]", VERSION);
            Console.WriteLine("Copyright (C) 2019 Rockchip Electronics Co., Ltd.");
            Console.WriteLine("-------------------------------------------------------");

            Prepare();

            FirmwareGenerator generator = new FirmwareGenerator(ConfigParser.parse("FwConfig.xml"), OUTPUT_DIRECTORY, "rkdsp.bin");
            generator.Generate();
        }
    }
}
