using FFXIVCraftingSim.Actions.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions
{
    public class Manipulation : CraftingAction
    {
        public override ushort Id => 14;

        public override string Name => "Manipulation";
        public override int Level => 65;
        public override bool IsBuff => true;
        public override bool IncreasesProgress => false;
        public override bool IncreasesQuality => false;
        protected override int DurabilityCost => 0;
        protected override int CPCost => 96;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => true;

        public override void AddBuff(CraftingSim sim)
        {
            if (sim.ManipulationBuff == null)
            {
                sim.ManipulationBuff = new ManipulationBuff();
                sim.CraftingBuffs.Add(sim.ManipulationBuff);
            }
            sim.ManipulationBuff.Stack = 8;
        }
    }
}
