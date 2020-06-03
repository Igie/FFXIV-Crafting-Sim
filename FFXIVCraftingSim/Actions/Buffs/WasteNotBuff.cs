using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions.Buffs
{
    public class WasteNotBuff : CraftingBuff
    {
        public override string Name => "Waste Not";

        public override void Step(CraftingSim sim)
        {
            Stack--;
            if (Stack == 0)
                NeedsRemove = true;
        }

        public override void Remove(CraftingSim sim)
        {
            sim.WasteNotBuff = null;
            base.Remove(sim);
        }

        public override CraftingBuff Clone()
        {
            return new WasteNotBuff { Stack = Stack, NeedsRemove = NeedsRemove };
        }
    }

    
}
