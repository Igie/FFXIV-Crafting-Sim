using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV_Crafting_Sim.Actions
{
    public class DelicateSynthesis : CraftingAction
    {
        public override int Id => 19;

        public override string Name => "Delicate Synthesis";
        public override bool IncreasesProgress => true;
        public override bool IncreasesQuality => true;
        protected override int DurabilityCost => 10;
        public override int CPCost => 32;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => false;
        public override double GetEfficiency(CraftingSim sim)
        {
            return 1d;
        }
    }
}
