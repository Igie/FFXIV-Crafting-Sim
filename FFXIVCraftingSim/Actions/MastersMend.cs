using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions
{
    public class MastersMend : CraftingAction
    {

        public override int Id => 3;

        public override string Name => "Master's Mend";
        public override bool IsBuff => false;
        public override bool IncreasesProgress => false;
        public override bool IncreasesQuality => false;
        protected override int DurabilityCost => -30;
        public override int CPCost => 88;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => false;
    }
}
