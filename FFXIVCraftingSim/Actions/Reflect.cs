using FFXIVCraftingSim.Actions.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions
{
    public class Reflect : CraftingAction
    {
        public override int Id => 16;

        public override string Name => "Reflect";
        public override bool IsBuff => false;
        public override bool IncreasesProgress => false;
        public override bool IncreasesQuality => true;
        protected override int DurabilityCost => 10;
        protected override int CPCost => 24;
        public override bool AsFirstActionOnly => true;
        public override bool AddsBuff => true;

        public override double GetEfficiency(CraftingSim sim)
        {
            return 1d;
        }
        public override void AddBuff(CraftingSim sim)
        {
            sim.InnerQuietBuff = new InnerQuietBuff();
            sim.CraftingBuffs.Add(sim.InnerQuietBuff);
            sim.InnerQuietBuff.Stack = 3;
        }
    }
}
