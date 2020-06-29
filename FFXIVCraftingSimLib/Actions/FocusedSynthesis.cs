using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSimLib.Actions
{
    public class FocusedSynthesis : CraftingAction
    {
        public override ushort Id => 21;

        public override string Name => "Focused Synthesis";
        public override int Level => 67;
        public override bool IsBuff => false;
        public override bool IncreasesProgress => true;
        public override bool IncreasesQuality => false;
        protected override int DurabilityCost => 10;
        protected override int CPCost => 5;
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
            return 2d;
        }
    }
}
