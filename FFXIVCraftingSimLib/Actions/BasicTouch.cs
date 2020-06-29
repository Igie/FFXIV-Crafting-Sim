using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSimLib.Actions
{
    public class BasicTouch : CraftingAction
    {
        public override ushort Id => 2;

        public override string Name => "Basic Touch";
        public override int Level => 5;
        public override bool IsBuff => false;
        public override bool IncreasesProgress => false;
        public override bool IncreasesQuality => true;
        protected override int DurabilityCost => 10;
        protected override int CPCost => 18;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => false;
        public override double GetEfficiency(CraftingSim sim)
        {
            return 1d;
        }
    }
}
