using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSimLib.Actions
{
    public class BasicSynthesis : CraftingAction
    {
        public override ushort Id => 1;

        public override string Name => "Basic Synthesis";
        public override int Level => 1;
        public override bool IsBuff => false;
        public override bool IncreasesProgress => true;
        public override bool IncreasesQuality => false;
        protected override int DurabilityCost => 10;
        protected override int CPCost => 0;
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
