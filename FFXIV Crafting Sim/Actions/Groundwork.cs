using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV_Crafting_Sim.Actions
{
    public class Groundwork : CraftingAction
    {
        public override int Id => 18;

        public override string Name => "Groundwork";
        public override bool IncreasesProgress => true;
        public override bool IncreasesQuality => false;
        protected override int DurabilityCost => 20;
        public override int CPCost => 18;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => false;
        public override double GetEfficiency(CraftingSim sim)
        {
            if (sim.CurrentDurability < GetDurabilityCost(sim))
                return 1.5d;
            return 3d;
        }
    }
}
