using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HifiFirmwareGenerator
{
    class ConfigXml
    {
        public static string NODE_ROOT = "HifiFirmwareGen";
        public static string NODE_FIRMWARE = "Firmware";
        public static string NODE_IMAGE = "Image";

        public static string ATTR_ID = "Id";
        public static string ATTR_NAME = "Name";
        public static string ATTR_TYPE = "Type";
        public static string ATTR_VERSION = "Version";
        public static string ATTR_CORENAME = "CoreName";
        public static string ATTR_TOOLSPATH= "ToolsPath";
        public static string ATTR_COMPATIBLE = "Compatible";
        public static string ATTR_EXEFILE = "ExecutableFile";
        public static string ATTR_EXTRFILE = "ExternalFile";
        public static string ATTR_EXTRADDR = "ExternalAddr";
        public static string ATTR_MAX_CSIZE = "MaxCodeSize";
        public static string ATTR_MAX_DSIZE = "MaxDataSize";
        public static string ATTR_LOADABLE_CSTART = "CodeLoadableStart";
        public static string ATTR_LOADABLE_CSIZE = "CodeLoadableSize";
        public static string ATTR_LOADABLE_DSTART = "DataLoadableStart";
        public static string ATTR_LOADABLE_DSIZE = "DataLoadableSize";
        public static string ATTR_OVERLAY_START = "OverlayStart";
        public static string ATTR_OVERLAY_END = "OverlayEnd";
        public static string ATTR_EXTCMEM_START = "ExternalCodeMemStart";
        public static string ATTR_EXTCMEM_END = "ExternalCodeMemEnd";
        public static string ATTR_EXTDMEM_START = "ExternalDataMemStart";
        public static string ATTR_EXTDMEM_END = "ExternalDataMemEnd";

        public static string VALUE_PERMANENT = "Permanent";
        public static string VALUE_LOADABLE = "Loadable";
    }

}
