using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV_Crafting_Sim.Stream
{
    public class DataStream : MemoryStream
    {
        public DataStream(byte[] data = null)
            :base()
        {
            if (data != null)
            {
                WriteBytes(data);
                Position = 0;
            }
        }

        public void WriteBytes(byte[] value)
        {
            Write(value, 0, value.Length);
        }

        public byte[] ReadBytes(int count)
        {
            byte[] result = new byte[count];
            Read(result, 0, count);
            return result;
        }

        public void WriteShort(short value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public short ReadShort()
        {
            return BitConverter.ToInt16(ReadBytes(2), 0);
        }

        public void WriteUShort(ushort value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public ushort ReadUShort()
        {
            return BitConverter.ToUInt16(ReadBytes(2), 0);
        }

        public void WriteInt(int value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public int ReadInt()
        {
            return BitConverter.ToInt32(ReadBytes(4), 0);
        }

        public void WriteUInt(ushort value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public uint ReadUInt()
        {
            return BitConverter.ToUInt32(ReadBytes(4), 0);
        }

        public byte[] GetBytes()
        {
            Position = 0;
            return ReadBytes((int)Length);
        }
    }
}
