using SaintCoinach.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FFXIV_Crafting_Sim.Converters
{
    public static class BitmapSourceExtentions
    {
        public static BitmapSource GetBitmapSource(this ImageFile file)
        {
            var argb = SaintCoinach.Imaging.ImageConverter.GetA8R8G8B8(file);
            return BitmapSource.Create(
                                       file.Width, file.Height,
                96, 96,
                PixelFormats.Bgra32, null,
                argb, file.Width * 4);
        }

        public static byte[] GetBytes(this BitmapSource source)
        {
            var stream = new MemoryStream();
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                var encoder = new PngBitmapEncoder();
                var frame = BitmapFrame.Create(source);
                encoder.Frames.Add(frame);
                
                encoder.Save(stream);
            });
            return stream.ToArray();
        }

        public static BitmapSource FromBytes(byte[] data)
        {
            return BitmapFrame.Create(new MemoryStream(data));
        }
    }
}
