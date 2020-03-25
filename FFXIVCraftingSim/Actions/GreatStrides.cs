using FFXIVCraftingSim.Actions.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions
{
    public class GreatStrides : CraftingAction
    {
        public override int Id => 8;

        public override string Name => "Great Strides";
        public override bool IncreasesProgress => false;
        public override bool IncreasesQuality => false;
        protected override int DurabilityCost => 0;
        public override int CPCost => 32;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => true;

        public override void AddBuff(CraftingSim sim)
        {
            if (sim.GreatStridesBuff == null)
            {
                sim.GreatStridesBuff = new GreatStridesBuff();
                sim.CraftingBuffs.Add(sim.GreatStridesBuff);
            }
            sim.GreatStridesBuff.Stack = 3;
        }
    }
}
