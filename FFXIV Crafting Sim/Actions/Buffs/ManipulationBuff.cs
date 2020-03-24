using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV_Crafting_Sim.Actions.Buffs
{
    public class ManipulationBuff : CraftingBuff
    {
        public override string Name => "Manipulation";

        public override void Step(CraftingSim sim)
        {
            sim.CurrentDurability += 5;
            if (sim.CurrentDurability > sim.CurrentRecipe.Durability)
                sim.CurrentDurability = sim.CurrentRecipe.Durability;
            Stack--;
            if (Stack == 0)
                NeedsRemove = true;
        }

        public override void Remove(CraftingSim sim)
        {
            sim.ManipulationBuff = null;
            base.Remove(sim);
        }
    }

    
}
