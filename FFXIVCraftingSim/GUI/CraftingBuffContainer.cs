using FFXIVCraftingSimLib.Actions.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FFXIVCraftingSim.GUI
{
    public class CraftingBuffContainer
    {
        public BitmapSource Source { get; set; }
        public CraftingBuff Buff { get; set; }

        public CraftingBuffContainer(BitmapSource source, CraftingBuff buff)
        {
            Source = source;
            Buff = buff;
        }
    }
}
