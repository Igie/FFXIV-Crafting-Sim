﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSimLib.Actions
{
    public class StandardTouch : CraftingAction
    {
        public override ushort Id => 7;

        public override string Name => "Standard Touch";
        public override int Level => 18;
        public override bool IsBuff => false;
        public override bool IncreasesProgress => false;
        public override bool IncreasesQuality => true;
        protected override int DurabilityCost => 10;
        protected override int CPCost => 32;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => false;
        public override double GetEfficiency(CraftingSim sim)
        {
            return 1.25d;
        }
    }
}
