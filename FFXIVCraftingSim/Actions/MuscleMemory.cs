using FFXIVCraftingSim.Actions.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions
{
    public class MuscleMemory : CraftingAction
    {
        public override int Id => 12;

        public override string Name => "Muscle Memory";
        public override bool IncreasesProgress => true;
        public override bool IncreasesQuality => false;
        protected override int DurabilityCost => 10;
        public override int CPCost => 6;
        public override bool AsFirstActionOnly => true;
        public override bool AddsBuff => true;
        public override double GetEfficiency(CraftingSim sim)
        {
            return 3d;
        }

        public override void AddBuff(CraftingSim sim)
        {
            sim.MuscleMemoryBuff = new MuscleMemoryBuff();
            sim.CraftingBuffs.Add(sim.MuscleMemoryBuff);
            sim.MuscleMemoryBuff.Stack = 5;
        }
    }
}
