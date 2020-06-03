﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions.Buffs
{
    public class InnerQuietBuff : CraftingBuff
    {
        public override string Name => "Inner Quiet";

        public override void Remove(CraftingSim sim)
        {
            sim.InnerQuietBuff = null;
            base.Remove(sim);
        }

        public override CraftingBuff Clone()
        {
            return new InnerQuietBuff { Stack = Stack, NeedsRemove = NeedsRemove };
        }
    }
}
