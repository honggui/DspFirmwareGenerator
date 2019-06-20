using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HifiFirmwareGenerator
{
    class LoadableImage : Image
    {
        private UInt32 mOverlayStart;
        private UInt32 mOverlayEnd;

        public LoadableImage(int id, string name)
            : base(id, name, new ImageType(ImageType.LOADABLE_STRING))
        {

        }

        public static Image CreateFromXml(XmlNode node, int id, string name)
        {
            LoadableImage image = new LoadableImage(id, name);

            UInt32 overlayStart = (UInt32)Convert.ToInt32(node.SelectSingleNode(ConfigXml.ATTR_OVERLAY_START).InnerText.Trim(), 16);
            UInt32 overlayEnd = (UInt32)Convert.ToInt32(node.SelectSingleNode(ConfigXml.ATTR_OVERLAY_END).InnerText.Trim(), 16);

            image.SetOverlayStart(overlayStart);
            image.SetOverlayEnd(overlayEnd);
            return image;
        }

        public void SetOverlayStart(UInt32 start)
        {
            mOverlayStart = start;
        }

        public void SetOverlayEnd(UInt32 end)
        {
            mOverlayEnd = end;
        }

        public override bool InCodeRange(UInt32 address)
        {
            if (address >= mOverlayStart && address <= mOverlayEnd)
                return true;
            else
                return false;
        }

        public override bool InDataRange(UInt32 address)
        {
            if (address >= mOverlayStart && address <= mOverlayEnd)
                return true;
            else
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
