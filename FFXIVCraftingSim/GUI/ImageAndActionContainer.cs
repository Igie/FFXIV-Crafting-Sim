using FFXIVCraftingSim.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FFXIVCraftingSim.GUI
{
    public class ImageAndActionContainer
    {
        public CraftingAction CraftingAction { get; private set; }
        public BitmapSource BitmapSource { get; private set; }

        public ImageAndActionContainer(CraftingSim sim, CraftingAction craftingAction)
        {
            CraftingAction = craftingAction;
            BitmapSource = G.Actions[CraftingAction.Name].Images[sim.CurrentRecipe.ClassJob];
        }
                
    }
}
