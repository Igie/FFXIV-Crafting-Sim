using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions.Buffs
{
    public abstract class CraftingBuff
    {
        public virtual string Name { 
            get
            {
                throw new NotImplementedException();
            }
        }
        public int Stack { get; set; }
        public bool NeedsRemove { get; set; } = false;

        public virtual void Step(CraftingSim sim)
        {
        }

        public virtual void Remove(CraftingSim sim)
        {
            if (sim.CraftingBuffs.Contains(this))
                sim.CraftingBuffs.Remove(this);
        }


    }
}
