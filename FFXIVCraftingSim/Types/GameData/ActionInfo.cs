using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FFXIVCraftingSim.Types.GameData
{
    public class ActionInfo
    {
        public string Name { get; set; }
        public Dictionary<ClassJobInfo, BitmapSource> Images { get; set; }
        public int Level { get; set; }

        public ActionInfo()
        {
            Images = new Dictionary<ClassJobInfo, BitmapSource>();
        }
    }
}
