using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions
{
    public class PreparatoryTouch : CraftingAction
    {
        public override ushort Id => 17;

        public override string Name => "Preparatory Touch";
        public override bool IsBuff => false;
        public override bool IncreasesProgress => false;
        public override bool IncreasesQuality => true;
        protected override int DurabilityCost => 20;
        protected override int CPCost => 40;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => false;

        public override double GetEfficiency(CraftingSim sim)
        {
            return 2d;
        }

        public override void IncreaseQuality(CraftingSim sim)
        {
            sim.CurrentQuality += sim.GetQualityIncrease(GetEfficiency(sim));
            if (sim.InnerQuietBuff != null)
            {
                sim.InnerQuietBuff.Stack += 2;
                if (sim.InnerQuietBuff.Stack > 11)
                    sim.InnerQuietBuff.Stack = 11;
            }

            if (sim.GreatStridesBuff != null)
                sim.GreatStridesBuff.NeedsRemove = true;
        }
    }
}
