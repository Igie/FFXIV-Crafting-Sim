using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions
{
    public class BrandOfTheElements : CraftingAction
    {
        public override ushort Id => 24;

        public override string Name => "Brand of the Elements";
        public override bool IsBuff => false;
        public override bool IncreasesProgress => true;
        public override bool IncreasesQuality => false;
        protected override int DurabilityCost => 10;
        protected override int CPCost => 6;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => false;
        public override double GetEfficiency(CraftingSim sim)
        {
            if (sim.NameOfTheElementsBuff != null)
            {
                return 1d + (2 * Math.Ceiling((1 - (double)sim.CurrentProgress / sim.CurrentRecipe.MaxProgress) * 100)) / 100;
            }

            return 1;
        }
    }
}
