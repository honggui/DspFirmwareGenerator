using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HifiFirmwareGenerator
{
    class Firmware
    {
        public static int MAGIC_SIZE = 16;
        public static int VERSION_SIZE = 16;
        public static int RESERVE_SIZE = 60;
        public static int MAX_IMAGES = 16;

        public static string FIRMWARE_MAGIC = "#RKCPDSPFW#";

        private string mVersion;
        private string mCoreName;
        private string mToolsPath;
        private string mCompatible;
        private ArrayList mImages;
        private ExecutableFile mExecutableFile;
        private string mExternalFile;

        private UInt32 mMaxCodeSize;
        private UInt32 mMaxDataSize;

        private UInt32 mCodeLoadableStart;
        private UInt32 mCodeLoadableSize;
        private UInt32 mDataLoadableStart;
        private UInt32 mDataLoadableSize;

        private UInt32 mExternalCodeStart;
        private UInt32 mExternalCodeEnd;
        private UInt32 mExternalDataStart;
        private UInt32 mExternalDataEnd;

        public Firmware()
        {
            mImages = new ArrayList();
        }

        public static Firmware CreateFromXml(XmlNode fwNode)
        {
            Firmware firmware = new Firmware();

            // Parse firmware information in ConfigXml
            string version = fwNode.SelectSingleNode(ConfigXml.ATTR_VERSION).InnerText.Trim();
            string coreName = fwNode.SelectSingleNode(ConfigXml.ATTR_CORENAME).InnerText.Trim();
            string toolsPath = fwNode.SelectSingleNode(ConfigXml.ATTR_TOOLSPATH).InnerText.Trim();
            string compatiable = fwNode.SelectSingleNode(ConfigXml.ATTR_COMPATIBLE).InnerText.Trim();
            string exeFilePath = fwNode.SelectSingleNode(ConfigXml.ATTR_EXEFILE).InnerText.Trim();
            string extrFilePath = fwNode.SelectSingleNode(ConfigXml.ATTR_EXTRFILE).InnerText.Trim();
            UInt32 extrFileAddr = (UInt32)Convert.ToInt32(fwNode.SelectSingleNode(ConfigXml.ATTR_EXTRADDR).InnerText.Trim(), 16);
            UInt32 maxCodeSize = (UInt32)Convert.ToInt32(fwNode.SelectSingleNode(ConfigXml.ATTR_MAX_CSIZE).InnerText.Trim(), 16);
            UInt32 maxDataSize = (UInt32)Convert.ToInt32(fwNode.SelectSingleNode(ConfigXml.ATTR_MAX_DSIZE).InnerText.Trim(), 16);

            UInt32 srcItcmStart = (UInt32)Convert.ToInt32(fwNode.SelectSingleNode(ConfigXml.ATTR_SRC_ITCM_START).InnerText.Trim(), 16);
            UInt32 srcItcmEnd = (UInt32)Convert.ToInt32(fwNode.SelectSingleNode(ConfigXml.ATTR_SRC_ITCM_END).InnerText.Trim(), 16);
            UInt32 dstItcmStart = (UInt32)Convert.ToInt32(fwNode.SelectSingleNode(ConfigXml.ATTR_DST_ITCM_START).InnerText.Trim(), 16);
   
            UInt32 srcDtcmStart = (UInt32)Convert.ToInt32(fwNode.SelectSingleNode(ConfigXml.ATTR_SRC_DTCM_START).InnerText.Trim(), 16);
            UInt32 srcDtcmEnd = (UInt32)Convert.ToInt32(fwNode.SelectSingleNode(ConfigXml.ATTR_SRC_DTCM_END).InnerText.Trim(), 16);
            UInt32 dstDtcmStart = (UInt32)Convert.ToInt32(fwNode.SelectSingleNode(ConfigXml.ATTR_DST_DTCM_START).InnerText.Trim(), 16);
#if OVERLAYS
            UInt32 codeLoadableStart = (UInt32)Convert.ToInt32(fwNode.SelectSingleNode(ConfigXml.ATTR_LOADABLE_CSTART).InnerText.Trim(), 16);
            UInt32 codeLoadableSize = (UInt32)Convert.ToInt32(fwNode.SelectSingleNode(ConfigXml.ATTR_LOADABLE_CSIZE).InnerText.Trim(), 16);
            UInt32 dataLoadableStart = (UInt32)Convert.ToInt32(fwNode.SelectSingleNode(ConfigXml.ATTR_LOADABLE_DSTART).InnerText.Trim(), 16);
            UInt32 dataLoadableSize = (UInt32)Convert.ToInt32(fwNode.SelectSingleNode(ConfigXml.ATTR_LOADABLE_DSIZE).InnerText.Trim(), 16);
            UInt32 externalCodeStart = (UInt32)Convert.ToInt32(fwNode.SelectSingleNode(ConfigXml.ATTR_EXTCMEM_START).InnerText.Trim(), 16);
            UInt32 externalCodeEnd = (UInt32)Convert.ToInt32(fwNode.SelectSingleNode(ConfigXml.ATTR_EXTCMEM_END).InnerText.Trim(), 16);
            UInt32 externalDataStart = (UInt32)Convert.ToInt32(fwNode.SelectSingleNode(ConfigXml.ATTR_EXTDMEM_START).InnerText.Trim(), 16);
            UInt32 externalDataEnd = (UInt32)Convert.ToInt32(fwNode.SelectSingleNode(ConfigXml.ATTR_EXTDMEM_END).InnerText.Trim(), 16);
#endif
            firmware.SetVersion(version);
            firmware.SetCoreName(coreName);
            firmware.SetToolsPath(toolsPath);
            firmware.SetCompatible(compatiable);
            firmware.SetMaxCodeSize(maxCodeSize);
            firmware.SetMaxDataSize(maxDataSize);

            firmware.SetExecutableFile(new ExecutableFile(exeFilePath, coreName, toolsPath, extrFilePath, extrFileAddr));
            firmware.SetExternalFile(extrFilePath);
            firmware.GetExecutableFile().SetTcmAddr(srcItcmStart, srcItcmEnd, dstItcmStart, srcDtcmStart, srcDtcmEnd, dstDtcmStart);
            ArrayList secInfo = firmware.GetExecutableFile().Parser();
#if OVERLAYS
            firmware.SetCodeLoadableStart(codeLoadableStart);
            firmware.SetCodeLoadableSize(codeLoadableSize);
            firmware.SetDataLoadableStart(dataLoadableStart);
            firmware.SetDataLoadableSize(dataLoadableSize);
#endif

            XmlNodeList nodeList = fwNode.SelectNodes(ConfigXml.NODE_IMAGE);
            if (nodeList.Count < 1)
            {
                Console.WriteLine("At least one image in firmware node");
                return null;
            }

            foreach (XmlNode node in nodeList)
            {
                Image image = null;

                int id = int.Parse(node.SelectSingleNode(ConfigXml.ATTR_ID).InnerText.Trim());
                string name = node.SelectSingleNode(ConfigXml.ATTR_NAME).InnerText.Trim();
                string type = node.SelectSingleNode(ConfigXml.ATTR_TYPE).InnerText.Trim();

#if OVERLAYS
                if(type.Equals(ImageType.LOADABLE_STRING))
                {
                    image = LoadableImage.CreateFromXml(node, id, name);
                    image.SetInternalCodeRange(codeLoadableStart, codeLoadableStart + codeLoadableSize);
                    image.SetInternalDataRange(dataLoadableStart, dataLoadableStart + dataLoadableSize);
                }
                else if(type.Equals(ImageType.PERMANENT_STRING))
                {
                    image = PermanentImage.CreateFromXml(node, id, name);
                    image.SetInternalCodeRange(0, maxCodeSize);
                    image.SetInternalDataRange(0, maxDataSize);
                    image.SetExternalCodeRange(externalCodeStart, externalCodeEnd);
                    image.SetExternalDataRange(externalDataStart, externalDataEnd);
                }
#else
                image = PermanentImage.CreateFromXml(node, id, name, secInfo);
                image.SetInternalCodeRange(0, maxCodeSize);
                image.SetInternalDataRange(0, maxDataSize);
#endif
                firmware.AddImage(image);
            }

            return firmware;
        }

        public void Save(FileStream file)
        {
            int fileOffset = 0;

            Console.WriteLine("Writing firmware...");

            // Magic 16 bytes
            Byte[] magic = new Byte[MAGIC_SIZE];
            Encoding.ASCII.GetBytes(FIRMWARE_MAGIC).CopyTo(magic, 0);
            file.Write(magic, 0, magic.Count());
            fileOffset += magic.Count();

            // Version 16 bytes
            Byte[] version = new Byte[VERSION_SIZE];
            Encoding.ASCII.GetBytes(mVersion).CopyTo(version, 0);
            file.Write(version, 0, version.Count());
            fileOffset += version.Count();

            // Image count 4 bytes
            Byte[] imageCount = BitConverter.GetBytes(mImages.Count);
            file.Write(imageCount, 0, imageCount.Count());
            fileOffset += imageCount.Count();

            // Image size MAX_IMAGES * 4 bytes, write later
            int imageSizeOffset = fileOffset;
            int[] imageSize = new int[MAX_IMAGES];
            fileOffset += (MAX_IMAGES * sizeof(int));
            file.Seek(fileOffset, SeekOrigin.Begin);

            // Reserve 60 bytes
            Byte[] reserve = new Byte[RESERVE_SIZE];
            file.Write(reserve, 0, reserve.Count());
            fileOffset += reserve.Count();

            foreach (Image image in mImages)
            {
                int size = image.Save(file, fileOffset);
                fileOffset += size;
                imageSize[mImages.IndexOf(image)] = size;
            }

            // Seek to save image size
            file.Seek(imageSizeOffset, SeekOrigin.Begin);
            foreach (int size in imageSize)
            {
                Byte[] sizeBytes = BitConverter.GetBytes(size);
                file.Write(sizeBytes, 0, sizeBytes.Count());
            }

            Console.WriteLine("Firmware has been written");
        }

        public void AddImage(Image image)
        {
            mImages.Add(image);
        }

        public void AddCode(UInt32 address, Byte code)
        {
            foreach (Image image in mImages)
            {
                if (image.InCodeRange(address))
                {
                    image.AddCode(address, code);
                    return;
                }
            }

            Console.WriteLine("Cannot find a image can hold this code");
        }

        public void AddData(UInt32 address, Byte data)
        {
            foreach (Image image in mImages)
            {
                if (image.InDataRange(address))
                {
                    image.AddData(address, data);
                    return;
                }
            }

            Console.WriteLine("Cannot find a image can hold this data");
        }

        public void SetExecutableFile(ExecutableFile file)
        {
            mExecutableFile = file;
        }

        public void SetExternalFile(string extrfile)
        {
            mExternalFile = extrfile;
        }

        public void SetVersion(string version)
        {
            mVersion = version;
        }

        public void SetCoreName(string core)
        {
            mCoreName = core;
        }

        public void SetToolsPath(string toolspath)
        {
            mToolsPath = toolspath;
        }

        public void SetCompatible(string compatible)
        {
            mCompatible = compatible;
        }

        public void SetMaxCodeSize(UInt32 size)
        {
            mMaxCodeSize = size;
        }

        public void SetMaxDataSize(UInt32 size)
        {
            mMaxDataSize = size;
        }

        public void SetCodeLoadableStart(UInt32 start)
        {
            mCodeLoadableStart = start;
        }

        public void SetCodeLoadableSize(UInt32 size)
        {
            mCodeLoadableSize = size;
        }

        public void SetDataLoadableStart(UInt32 start)
        {
            mDataLoadableStart = start;
        }

        public void SetDataLoadableSize(UInt32 size)
        {
            mDataLoadableSize = size;
        }

        public void SetExternalCodeStart(UInt32 start)
        {
            mExternalCodeStart = start;
        }

        public void SetExternalCodeEnd(UInt32 end)
        {
            mExternalCodeEnd = end;
        }

        public void SetExternalDataStart(UInt32 start)
        {
            mExternalDataStart = start;
        }

        public void SetExternalDataEnd(UInt32 end)
        {
            mExternalDataEnd = end;
        }

        public ExecutableFile GetExecutableFile()
        {
            return mExecutableFile;
        }
    }
}
