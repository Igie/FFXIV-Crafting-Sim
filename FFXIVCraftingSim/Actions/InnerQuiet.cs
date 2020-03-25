using FFXIVCraftingSim.Actions.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions
{
    public class InnerQuiet : CraftingAction
    {

        public override int Id => 4;

        public override string Name => "Inner Quiet";
        public override bool IsBuff => true;
        public override bool IncreasesProgress => false;
        public override bool IncreasesQuality => false;
        protected override int DurabilityCost => 0;
        public override int CPCost => 18;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => true;

        public override CraftingActionResult CheckInner(CraftingSim sim)
        {
            if (sim.InnerQuietBuff != null)
                return CraftingActionResult.NeedsNoBuff;
            return CraftingActionResult.Success;
        }

        public override void AddBuff(CraftingSim sim)
        {
            sim.InnerQuietBuff = new InnerQuietBuff();
            sim.CraftingBuffs.Add(sim.InnerQuietBuff);
            sim.InnerQuietBuff.Stack = 1;
        }
    }
}
