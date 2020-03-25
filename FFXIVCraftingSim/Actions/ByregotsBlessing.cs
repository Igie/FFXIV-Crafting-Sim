using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions
{
    public class ByregotsBlessing : CraftingAction
    {
        public override int Id => 11;

        public override string Name => "Byregot's Blessing";
        public override bool IncreasesProgress => false;
        public override bool IncreasesQuality => true;
        protected override int DurabilityCost => 10;
        public override int CPCost => 24;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => false;

        public override CraftingActionResult CheckInner(CraftingSim sim)
        {
            if (sim.InnerQuietBuff == null)
                return CraftingActionResult.NeedsBuff;
            return CraftingActionResult.Success;
        }

        public override double GetEfficiency(CraftingSim sim)
        {
            return 1d + 0.2 * (sim.InnerQuietBuff.Stack - 1);
        }

        public override void IncreaseQuality(CraftingSim sim)
        {
            sim.CurrentQuality += sim.GetQualityIncrease(GetEfficiency(sim));
            sim.InnerQuietBuff.NeedsRemove = true;

            if (sim.GreatStridesBuff != null)
                sim.GreatStridesBuff.NeedsRemove = true;
        }
    }
}
