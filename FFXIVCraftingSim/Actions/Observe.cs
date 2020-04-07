using FFXIVCraftingSim.Actions.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions
{
    public class Observe : CraftingAction
    {
        public override int Id => 20;

        public override string Name => "Observe";
        public override bool IsBuff => true;
        public override bool IncreasesProgress => false;
        public override bool IncreasesQuality => false;
        protected override int DurabilityCost => 0;
        protected override int CPCost => 7;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => true;

        public override void AddBuff(CraftingSim sim)
        {
            if (sim.ObserveBuff == null)
            {
                sim.ObserveBuff = new ObserveBuff();
                sim.CraftingBuffs.Add(sim.ObserveBuff);
            }
            sim.ObserveBuff.Stack = 1;
        }
    }
}
