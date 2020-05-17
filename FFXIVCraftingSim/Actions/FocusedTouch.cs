using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions
{
    public class FocusedTouch : CraftingAction
    {
        public override ushort Id => 22;

        public override string Name => "Focused Touch";
        public override bool IsBuff => false;
        public override bool IncreasesProgress => false;
        public override bool IncreasesQuality => true;
        protected override int DurabilityCost => 10;
        protected override int CPCost => 18;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => false;

        public override CraftingActionResult CheckInner(CraftingSim sim)
        {
            if (sim.ObserveBuff == null)
                return CraftingActionResult.NeedsBuff;
            return CraftingActionResult.Success;
        }

        public override double GetEfficiency(CraftingSim sim)
        {
            return 1.5d;
        }
    }
}
