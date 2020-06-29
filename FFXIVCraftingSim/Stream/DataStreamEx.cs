using FFXIVCraftingSim.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FFXIVCraftingSim.Stream
{
    public class DataStreamEx : DataStream
    {
        public DataStreamEx(byte[] data = null)
            : base(data) { }

        public void WriteBitmapSource(BitmapSource value)
        {
            if (value == null)
            {
                WriteInt(0);
                return;
            }
            byte[] data = value.GetBytes();
            WriteInt(data.Length);
            WriteBytes(data);
        }

        public BitmapSource ReadBitmapSource()
        {
            int size = ReadInt();
            if (size == 0) return null;
            return BitmapSourceExtentions.FromBytes(ReadBytes(size));
        }
    }
}
