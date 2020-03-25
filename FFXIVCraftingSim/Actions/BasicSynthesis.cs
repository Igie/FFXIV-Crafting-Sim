using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions
{
    public class BasicSynthesis : CraftingAction
    {
        public override int Id => 1;

        public override string Name => "Basic Synthesis";
        public override bool IncreasesProgress => true;
        public override bool IncreasesQuality => false;
        protected override int DurabilityCost => 10;
        public override int CPCost => 0;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => false;
        public override double GetEfficiency(CraftingSim sim)
        {
            if (sim.Level >= 31)
                return 1.2d;
            return 1d;
        }
    }
}
