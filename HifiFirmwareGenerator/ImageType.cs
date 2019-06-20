using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HifiFirmwareGenerator
{
    public class ImageType
    {
        public static int PERMANENT = 1;
        public static int LOADABLE  = 2;
        public static string PERMANENT_STRING = "Permanent";
        public static string LOADABLE_STRING = "Loadable";

        private string mName;
        private int mValue;

        public ImageType(string name, int value)
        {
            mName = name;
            mValue = value;
        }

        public ImageType(string name)
        {
            if (name.Equals(PERMANENT_STRING))
                mValue = PERMANENT;
            else if (name.Equals(LOADABLE_STRING))
                mValue = LOADABLE;

            mName = name;
        }

        public string GetName()
        {
            return mName;
        }

        public int GetValue()
        {
            return mValue;
        }
    }
}
