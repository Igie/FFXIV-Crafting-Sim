using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions.Buffs
{
    public class ObserveBuff : CraftingBuff
    {
        public override string Name => "Observe";

        public override void Step(CraftingSim sim)
        {
            Stack--;
            if (Stack == 0)
                NeedsRemove = true;
        }

        public override void Remove(CraftingSim sim)
        {
            sim.ObserveBuff = null;
            base.Remove(sim);
        }

        public override CraftingBuff Clone()
        {
            return new ObserveBuff { Stack = Stack, NeedsRemove = NeedsRemove };
        }
    }  
}
