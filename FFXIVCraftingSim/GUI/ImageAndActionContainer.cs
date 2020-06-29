using FFXIVCraftingSimLib.Actions;
using System.Windows.Media.Imaging;

namespace FFXIVCraftingSim.GUI
{
    public class ImageAndActionContainer
    {
        public CraftingAction CraftingAction { get; private set; }
        public BitmapSource BitmapSource { get; private set; }

        public ImageAndActionContainer(CraftingSimEx sim, CraftingAction craftingAction)
        {
            CraftingAction = craftingAction;
            BitmapSource = G.Actions[CraftingAction.Name].Images[sim.CurrentRecipe.ClassJob];
        }
                
    }
}
