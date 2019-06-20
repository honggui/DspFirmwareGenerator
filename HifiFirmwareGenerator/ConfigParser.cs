using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HifiFirmwareGenerator
{
    class ConfigParser
    {
        public static Firmware parse(string configFile)
        {
            Firmware firmware = null;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(configFile);
            string fwNodePath = ConfigXml.NODE_ROOT + "/" + ConfigXml.NODE_FIRMWARE;
            XmlNodeList nodeList = xmlDoc.SelectNodes(fwNodePath);

            // Just support one firmware in ConfigXml
            firmware = Firmware.CreateFromXml(nodeList.Item(0));

            return firmware;
        }
    }
}
