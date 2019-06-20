using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HifiFirmwareGenerator
{
    class PermanentImage : Image
    {
        public PermanentImage(int id, string name, ArrayList secInfo)
            : base(id, name, new ImageType(ImageType.PERMANENT_STRING), secInfo)
        {
        }

        public static Image CreateFromXml(XmlNode node, int id, string name, ArrayList secInfo)
        {
            PermanentImage image = new PermanentImage(id, name, secInfo);
            return image;
        }

        public override bool InCodeRange(UInt32 address)
        {
            return false;
        }

        public override bool InDataRange(UInt32 address)
        {
            return false;
        }

        public override void AddCode(UInt32 address, Byte code)
        {
        }

        public override void AddData(UInt32 address, Byte data)
        {
        }
    }
}
