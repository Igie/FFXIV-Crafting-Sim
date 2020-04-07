using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions
{
    public class PrudentTouch : CraftingAction
    {
        public override int Id => 15;

        public override string Name => "Prudent Touch";
        public override bool IsBuff => false;
        public override bool IncreasesProgress => false;
        public override bool IncreasesQuality => true;
        protected override int DurabilityCost => 5;
        protected override int CPCost => 25;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => false;

        public override CraftingActionResult CheckInner(CraftingSim sim)
        {
            if (sim.WasteNotBuff != null)
                return CraftingActionResult.NeedsNoBuff;
            return CraftingActionResult.Success;
        }

        public override double GetEfficiency(CraftingSim sim)
        {
            return 1d;
        }
    }
}
