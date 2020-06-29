using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSimLib.Actions
{
    public class MastersMend : CraftingAction
    {

        public override ushort Id => 3;

        public override string Name => "Master's Mend";
        public override int Level => 7;
        public override bool IsBuff => false;
        public override bool IncreasesProgress => false;
        public override bool IncreasesQuality => false;
        protected override int DurabilityCost => -30;
        protected override int CPCost => 88;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => false;
    }
}
