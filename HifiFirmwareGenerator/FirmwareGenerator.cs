using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HifiFirmwareGenerator
{
    class FirmwareGenerator
    {
        private Firmware mFirmware;
        private string mPath;
        private string mName;

        public FirmwareGenerator(Firmware firmware, string path, string name)
        {
            mPath = path;
            mName = name;
            mFirmware = firmware;
        }

        public bool Generate()
        {
            // Save firmware to file
            string path = string.Format("{0}/{1}", mPath, mName);
            System.IO.File.Delete(path);
            FileStream f = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            mFirmware.Save(f);
            f.Close();

            return true;
        }
    }
}
