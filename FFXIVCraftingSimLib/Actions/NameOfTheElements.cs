using FFXIVCraftingSimLib.Actions.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSimLib.Actions
{
    public class NameOfTheElements : CraftingAction
    {
        public override ushort Id => 23;

        public override string Name => "Name of the Elements";
        public override int Level => 37;
        public override bool IsBuff => true;
        public override bool IncreasesProgress => false;
        public override bool IncreasesQuality => false;
        protected override int DurabilityCost => 0;
        protected override int CPCost => 30;
        public override bool AsFirstActionOnly => false;
        public override bool AddsBuff => true;

        public override CraftingActionResult CheckInner(CraftingSim sim)
        {
            if (sim.NameOfTheElementsUsed)
                return CraftingActionResult.BuffUsedUp;
            return CraftingActionResult.Success;
        }

        public override void AddBuff(CraftingSim sim)
        {
            if (sim.NameOfTheElementsBuff == null)
            {
                sim.NameOfTheElementsUsed = true;
                sim.NameOfTheElementsBuff = new NameOfTheElementsBuff();
                sim.CraftingBuffs.Add(sim.NameOfTheElementsBuff);
            }
            sim.NameOfTheElementsBuff.Stack = 3;
        }
    }
}
