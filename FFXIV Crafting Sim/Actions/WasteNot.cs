using FFXIV_Crafting_Sim.Actions.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV_Crafting_Sim.Actions
{
    public class WasteNot : CraftingAction
    {
        public override int Id => 5;

        public override string Name => "Waste Not";
        public override bool IncreasesProgress => false;
        public override bool IncreasesQuality => false;
        protected override int DurabilityCost => 0;
        public override int CPCost => 56;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => true;

        public override void AddBuff(CraftingSim sim)
        {
            if (sim.WasteNotBuff == null)
            {
                sim.WasteNotBuff = new WasteNotBuff();
                sim.CraftingBuffs.Add(sim.WasteNotBuff);
            }
            sim.WasteNotBuff.Stack = 4;
        }
    }
}
