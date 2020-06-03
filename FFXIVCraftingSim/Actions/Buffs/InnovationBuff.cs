using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions.Buffs
{
    public class InnovationBuff : CraftingBuff
    {
        public override string Name => "Innovation";

        public override void Step(CraftingSim sim)
        {
            Stack--;
            if (Stack == 0)
                NeedsRemove = true;
        }

        public override void Remove(CraftingSim sim)
        {
            sim.InnovationBuff = null;
            base.Remove(sim);
        }

        public override CraftingBuff Clone()
        {
            return new InnovationBuff { Stack = Stack, NeedsRemove = NeedsRemove };
        }
    }
}
