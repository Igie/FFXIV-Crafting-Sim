﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions.Buffs
{
    public class GreatStridesBuff : CraftingBuff
    {
        public override string Name => "Great Strides";

        public override void Step(CraftingSim sim)
        {
            Stack--;
            if (Stack == 0)
                NeedsRemove = true;
        }

        public override void Remove(CraftingSim sim)
        {
            sim.GreatStridesBuff = null;
            base.Remove(sim);
        }
    }

    
}
