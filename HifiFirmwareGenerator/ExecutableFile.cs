using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HifiFirmwareGenerator
{
    class ExecutableFile
    {
        public static string NAME = "RKDSP";
        private string mPath;
        private string mCoreName;
        private string mToolsPath;
        private string mExtrFile= null;
        private UInt32 mExtrAddr = 0;
        UInt32 mSecId = 0;

        private UInt32 srcItcmStart = 0;
        private UInt32 srcItcmEnd = 0;
        private UInt32 dstItcmStart = 0;
        private UInt32 srcDtcmStart = 0;
        private UInt32 srcDtcmEnd = 0;
        private UInt32 dstDtcmStart = 0;
        private UInt32 extDataStart = 0;
        private UInt32 extDataEnd = 0;

        public ExecutableFile(string path, string core, string toolspath, string extrFile, UInt32 extrAddr)
        {
            mPath = path;
            mCoreName = core;
            mToolsPath = toolspath;
            mExtrFile = extrFile;
            mExtrAddr = extrAddr;
            if (System.IO.File.Exists(mPath) == false)
            {
                Console.WriteLine("Cannot find the executable file specified in ConfigFile");
                throw new System.IO.FileNotFoundException();
            }
        }

        public void SetTcmAddr(UInt32 srcItcmStart, UInt32 srcItcmEnd, UInt32 dstItcmStart, 
                                UInt32 srcDtcmStart, UInt32 srcDtcmEnd, UInt32 dstDtcmStart,
                                UInt32 extDataStart, UInt32 extDataEnd)
        {
            this.srcItcmStart = srcItcmStart;
            this.srcItcmEnd = srcItcmEnd;
            this.dstItcmStart = dstItcmStart;
            this.srcDtcmStart = srcDtcmStart;
            this.srcDtcmEnd = srcDtcmEnd;
            this.dstDtcmStart = dstDtcmStart;
            this.extDataStart = extDataStart;
            this.extDataEnd = extDataEnd;
        }

        private void Prepare()
        {
            /* Check necessary tools */
            //if (System.IO.File.Exists(mToolsPath + "/xt-objcopy.exe") == false)
            if (System.IO.File.Exists(mToolsPath + "/xt-objcopy") == false)
            {
                //Console.WriteLine("xt-objcopy.exe is necessary, please set right XtensaTools path");
                Console.WriteLine("xt-objcopy is necessary, please set right XtensaTools path");
                throw new System.IO.FileNotFoundException();
            }

            /* Check necessary tools */
            //if (System.IO.File.Exists(mToolsPath + "/xt-size.exe") == false)
            if (System.IO.File.Exists(mToolsPath + "/xt-size") == false)
            {
                //Console.WriteLine("xt-size.exe is necessary, please set right XtensaTools path");
                Console.WriteLine("xt-size is necessary, please set right XtensaTools path");
                throw new System.IO.FileNotFoundException();
            }
        }

        public ArrayList Parser()
        {
            this.Prepare();
            System.Diagnostics.Process exep = new System.Diagnostics.Process();
            //exep.StartInfo.FileName = string.Format("{0}/xt-size.exe", mToolsPath);
            exep.StartInfo.FileName = string.Format("{0}/xt-size", mToolsPath);
            exep.StartInfo.Arguments = string.Format("-A {0}", mPath);
            Console.WriteLine("Execute: {0} {1}", exep.StartInfo.FileName, exep.StartInfo.Arguments);
            exep.StartInfo.CreateNoWindow = true;
            exep.StartInfo.UseShellExecute = false;
            exep.StartInfo.RedirectStandardOutput = true;
            exep.Start();
            string output = exep.StandardOutput.ReadToEnd();
            exep.WaitForExit();
            exep.Close();
            Console.WriteLine("output: {0}", output);

            ArrayList secInfo = ParserOutput(output);
            ExtractSection(secInfo);
            ExternalFile(secInfo);
            return secInfo;
        }

        private void ExtractSection(ArrayList secInfo)
        {
            for (int i = 0; i < secInfo.Count; i++)
            {
                SecInfo info = (SecInfo)secInfo[i];
                System.Diagnostics.Process exep = new System.Diagnostics.Process();
                //exep.StartInfo.FileName = string.Format("{0}/xt-objcopy.exe", mToolsPath);
                exep.StartInfo.FileName = string.Format("{0}/xt-objcopy", mToolsPath);
                exep.StartInfo.Arguments = string.Format("-O binary -j {0} {1} {2} --xtensa-core={3}",
                                           info.name, mPath, "process/" + info.name, mCoreName);
                Console.WriteLine("Execute: {0} {1}", exep.StartInfo.FileName, exep.StartInfo.Arguments);
                exep.StartInfo.CreateNoWindow = true;
                exep.StartInfo.UseShellExecute = false;
                exep.Start();
                exep.WaitForExit();
                exep.Close();
                if (System.IO.File.Exists(@"process/" + info.name) == false)
                {
                    Console.WriteLine("General {0} fail", info.name);
                    throw new System.IO.FileNotFoundException();
                }

                if (info.addr != this.extDataStart || !info.name.Equals(".srom.rodata"))
                    continue;

                //TODO support more section
                /* external data partition */
                if (this.extDataStart != 0 && this.extDataEnd != 0)
                {
                    //System.IO.File.Copy("process\\" + info.name, "output\\" + "ext_rkdsp.bin", true);
                    System.IO.File.Copy("process/" + info.name, "output/" + "ext_rkdsp.bin", true);
                    Console.WriteLine("Get rodata section to ext_rkdsp.bin");
                    secInfo.RemoveAt(i);
                }
            }
        }

        private ArrayList ParserOutput(string output)
        {
            ArrayList secArray = new ArrayList();
            //string[] strArray = output.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] strArray = output.Split(new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string str in strArray)
            {
                // Remove bss section, because the bss will be init in reset vector.
                if (str.Contains("Total") || str.Contains("section") || str.Contains(":") || str.Contains("0x0") || str.Contains("bss"))
                    continue;
                string[] eleArray = str.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (!eleArray[0].StartsWith("."))
                {
                    Console.WriteLine("Section Parser fail");
                    throw new System.IO.FileNotFoundException();
                }
                SecInfo secInfo;
                secInfo.name = eleArray[0];
                secInfo.size = Convert.ToUInt32(eleArray[1]);
                secInfo.addr = Convert.ToUInt32(eleArray[2]);
                secInfo.addr = processTcmAddr(secInfo.addr);
                if (secInfo.size == 0 || secInfo.addr == 0)
                    continue;

                secInfo.id = mSecId;
                mSecId++;
                Console.WriteLine("id : {0} name: {1}, size: {2}, addr: {3}", secInfo.id, secInfo.name, secInfo.size, secInfo.addr);
                secArray.Add(secInfo);
            }
            return secArray;
        }

        private void ExternalFile(ArrayList secArray)
        {
            string currPath = System.IO.Directory.GetCurrentDirectory();

            if (string.IsNullOrEmpty(mExtrFile) || mExtrAddr == 0)
                return;

            //string ExtrPath = currPath + "\\" + mExtrFile;
            string ExtrPath = currPath + "/" + mExtrFile;
            if (System.IO.File.Exists(ExtrPath) == false)
                return;

            SecInfo secInfo;
            secInfo.name = System.IO.Path.GetFileName(mExtrFile);
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(ExtrPath);
            secInfo.size = (UInt32)fileInfo.Length;
            secInfo.addr = mExtrAddr;
            secInfo.id = mSecId;
            mSecId++;
            //System.IO.File.Copy(ExtrPath, "process\\" + secInfo.name, true);
            System.IO.File.Copy(ExtrPath, "process/" + secInfo.name, true);
            Console.WriteLine("external file id : {0} name: {1}, size: {2}, addr: {3}", secInfo.id, secInfo.name, secInfo.size, secInfo.addr);
            secArray.Add(secInfo);
        }

        private UInt32 processTcmAddr(UInt32 srcAddr)
        {
            UInt32 tempAddr = srcAddr;
            if (srcItcmStart == 0 && srcDtcmStart == 0)
                return tempAddr;

            if (srcItcmStart != 0 && srcItcmStart <= srcAddr && srcItcmEnd >= srcAddr)
            {
                tempAddr = (dstItcmStart - srcItcmStart) + srcAddr;
                return tempAddr;
            }

            if (srcDtcmStart != 0 && srcDtcmStart <= srcAddr && srcDtcmEnd >= srcAddr)
            {
                tempAddr = (dstDtcmStart - srcDtcmStart) + srcAddr;
                return tempAddr;
            }
            return tempAddr;
        }
    }
}
