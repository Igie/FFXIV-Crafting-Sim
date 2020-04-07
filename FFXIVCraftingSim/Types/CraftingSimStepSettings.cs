using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Types
{

    public enum RecipeCondition
    {
        Normal,         // default
        Good,           // 1.5 efficiency to quality
        Excellent,      // 4.0 efficiency to quality
        Poor,            // 0.5 efficiency to quality
        Centered,       // 25% to action success rate
        Sturdy,         // 50% reduced durability loss
        Pliant          // 50% reduced cp cost
    }

    public class CraftingSimStepSettings
    {
        public int CurrentStep { get; set; }

        public RecipeCondition RecipeCondition { get; set; }

        public CraftingSimStepSettings()
        {
            RecipeCondition = RecipeCondition.Normal;
        }
    }
}
