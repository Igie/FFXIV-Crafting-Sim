using FFXIV_Crafting_Sim.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FFXIV_Crafting_Sim.GUI
{
    public class CraftingActionContainer
    {
        public BitmapSource Source { get; set; }
        public CraftingAction Action { get; set; }

        public CraftingActionContainer(BitmapSource source, CraftingAction action)
        {
            Source = source;
            Action = action;
        }
    }
}
