using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV_Crafting_Sim.Actions.Buffs
{
    public abstract class CraftingBuff
    {
        public abstract string GetName();
        public abstract int GetCurrentStack();
    }
}
