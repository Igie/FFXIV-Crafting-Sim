using FFXIVCraftingSimLib.Actions.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSimLib.Actions
{
    public class Innovation : CraftingAction
    {
        public override ushort Id => 9;

        public override string Name => "Innovation";
        public override int Level => 26;
        public override bool IsBuff => true;
        public override bool IncreasesProgress => false;
        public override bool IncreasesQuality => false;
        protected override int DurabilityCost => 0;
        protected override int CPCost => 18;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => true;

        public override void AddBuff(CraftingSim sim)
        {
            if (sim.InnovationBuff == null)
            {
                sim.InnovationBuff = new InnovationBuff();
                sim.CraftingBuffs.Add(sim.InnovationBuff);
            }
            sim.InnovationBuff.Stack = 4;
        }
    }
}
