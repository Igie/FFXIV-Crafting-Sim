using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSimLib.Actions
{
    public class CarefulSynthesis : CraftingAction
    {
        public override ushort Id => 13;

        public override string Name => "Careful Synthesis";
        public override int Level => 62;
        public override bool IsBuff => false;
        public override bool IncreasesProgress => true;
        public override bool IncreasesQuality => false;
        protected override int DurabilityCost => 10;
        protected override int CPCost => 7;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => false;
        public override double GetEfficiency(CraftingSim sim)
        {
            return 1.5d;
        }
    }
}
