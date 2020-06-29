﻿using FFXIVCraftingSimLib.Actions.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSimLib.Actions
{
    public class Veneration : CraftingAction
    {
        public override ushort Id => 6;

        public override string Name => "Veneration";
        public override int Level => 15;
        public override bool IsBuff => true;
        public override bool IncreasesProgress => false;
        public override bool IncreasesQuality => false;
        protected override int DurabilityCost => 0;
        protected override int CPCost => 18;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => true;

        public override void AddBuff(CraftingSim sim)
        {
            if (sim.VenerationBuff == null)
            {
                sim.VenerationBuff = new VenerationBuff();
                sim.CraftingBuffs.Add(sim.VenerationBuff);
            }
            sim.VenerationBuff.Stack = 4;
        }
    }
}
