﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV_Crafting_Sim.Actions.Buffs
{
    public class MuscleMemoryBuff : CraftingBuff
    {
        public override string Name => "Muscle Memory";

        public override void Step(CraftingSim sim)
        {
            Stack--;
            if (Stack == 0)
                NeedsRemove = true;
        }

        public override void Remove(CraftingSim sim)
        {
            sim.MuscleMemoryBuff = null;
            base.Remove(sim);
        }
    }

    
}
