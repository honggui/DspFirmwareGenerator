using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HifiFirmwareGenerator
{
    struct SecInfo
    {
        public UInt32 id;
        public string name;
        public UInt32 size;
        public UInt32 addr;
    }

    public enum SectionType
    {
        CodeInt,
        CodeExt,
        DataInt,
        DataExt
    };

    public class Section
    {
        public static int BIT_ALIGN = 32;
        public static int BYTE_ALIGN = BIT_ALIGN / 8;
        public static UInt32 INVALID_ADDRESS = 0xffffffff;
        public static UInt32 FAKE_ADDRESS_MASK = 0xfffff;
        public static UInt32 FAKE_ADDRESS_START = 0xc0000000;

        SectionType mType;
        private int mValid;
        private Byte[] mData;
        private String mName;
        private UInt32 mId;

        private UInt32 mSize;
        private UInt32 mStartAddress;
        private UInt32 mEndAddress;

        private UInt32 mValueStart;
        private UInt32 mValueEnd;

        Section()
        {

        }

        public Section(SectionType type)
        {
            mValueStart = mValueEnd = mStartAddress = mEndAddress = INVALID_ADDRESS;
            mType = type;
            mValid = 0;
        }

        /* SectionType no work now */
        public Section(UInt32 id, string name, UInt32 size, UInt32 address)
        {
            mId   = id;
            mName = name;
            mSize = size;
            mStartAddress = address;
            mEndAddress = address + size;
            mValueStart = mValueEnd = INVALID_ADDRESS;
            mValid = 0;
        }

        public int Save(FileStream file, int offset)
        {
            int fileOffset = offset;
            UInt32 valueCount = GetSize();

            file.Seek(fileOffset, SeekOrigin.Begin);
            Console.WriteLine("    Writing section: id={0}, size={1}, address=0x{2:X}", mId, valueCount, mStartAddress);

            // 4 bytes id
            Byte[] id = BitConverter.GetBytes(mId);
            file.Write(id, 0, id.Count());
            fileOffset += id.Count();

            // 4 bytes size

            Byte[] size = BitConverter.GetBytes(valueCount);
            file.Write(size, 0, size.Count());
            fileOffset += size.Count();

            // 4bytes load address
            Byte[] address = BitConverter.GetBytes(mStartAddress);
            file.Write(address, 0, address.Count());
            fileOffset += address.Count();

            string secPath = "process/" + mName;
            Byte[] data = File.ReadAllBytes(secPath);
            if (data.Count() > 0)
            {
                file.Write(data, 0, data.Count());
                fileOffset += data.Count();
            }
            Console.WriteLine("path: {0} count:{1} size :{2}, offset: {3}", secPath, data.Count(), fileOffset - offset, fileOffset - data.Count());
            return fileOffset - offset;
        }

        public UInt32 GetSize()
        {
            return mSize;
        }

        public UInt32 GetValueCount()
        {
            UInt32 align = (UInt32)BYTE_ALIGN;

            if (mValid == 0)
                return 0;

            if (mValueEnd == INVALID_ADDRESS)
                return 0;
            else
                return align * ((((mValueEnd + 1) - mStartAddress) + align - 1) / align);
        }

        public void AddValue(UInt32 address, Byte value)
        {
            if (value == 0)
                return;

            if (address >= FAKE_ADDRESS_START)
                address = mStartAddress + (address & FAKE_ADDRESS_MASK);

            if (mValueStart == 0xffffffff)
                mValueStart = mValueEnd = address;

            if (address < mValueStart)
                mValueStart = address;
            if (address > mValueEnd)
                mValueEnd = address;

            int index = (int)(address - mStartAddress);
            mData.SetValue(value, index);
        }

        public void SetRange(UInt32 start, UInt32 end)
        {
            if (start >= end)
            {
                Console.WriteLine("Invalid section range");
                return;
            }

            mStartAddress = start;
            mEndAddress = end;

            mData = new Byte[end - start];
            mValid = 1;
        }

        public bool InRange(UInt32 address)
        {
            if (address >= mStartAddress && address <= mEndAddress)
                return true;
            else
                return false;
        }
    }
}
