using FFXIV_Crafting_Sim.Actions.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV_Crafting_Sim.Actions
{
    public enum CraftingActionResult
    {
        Success,
        CraftCompleted,
        NeedsBuff,
        NotEnoughDurability,
        NotEnoughCP,
        FirstActionOnly
    }

    public class CraftingAction
    {
        static CraftingAction()
        {
            CraftingActions = new Dictionary<int, CraftingAction>();
            CraftingActions.Add(1, new BasicSynthesis());
            CraftingActions.Add(2, new BasicTouch());
        }

        public static Dictionary<int, CraftingAction> CraftingActions { get; private set; }
        public virtual int Id
        {
            get { throw new NotImplementedException(); }
        }

        public virtual string Name
        {
            get { throw new NotImplementedException(); }
        }

        public virtual bool IncreasesProgress
        {
            get { throw new NotImplementedException(); }
        }

        public virtual bool IncreasesQuality
        {
            get { throw new NotImplementedException(); }
        }
       
        protected virtual int DurabilityCost
        {
            get { throw new NotImplementedException(); }
        }

        public virtual int CPCost
        {
            get { throw new NotImplementedException(); }
        }

        public virtual bool AsFirstActionOnly
        {
            get { throw new NotImplementedException(); }
        }

        public virtual bool AddsBuff
        {
            get { throw new NotImplementedException(); }
        }

        public CraftingActionResult Check(CraftingSim sim, int index)
        {
            if (AsFirstActionOnly && index > 0)
                return CraftingActionResult.FirstActionOnly;
            if (sim.CurrentProgress >= sim.CurrentRecipe.MaxProgress)
                return CraftingActionResult.CraftCompleted;
            if (CPCost > sim.CurrentCP)
                return CraftingActionResult.NotEnoughCP;
            if (sim.CurrentDurability <= 0)
                return CraftingActionResult.NotEnoughDurability;

            return CheckInner(sim);
        }

        public virtual CraftingActionResult CheckInner(CraftingSim sim)
        {
            return CraftingActionResult.Success;
        }

        public virtual double GetEfficiency(CraftingSim sim)
        {
            throw new NotImplementedException();
        }

        public int GetDurabilityCost(CraftingSim sim)
        {
            if (DurabilityCost <= 0)
                return DurabilityCost;
            else return DurabilityCost;

            //waste not check
        }

        public virtual void IncreaseProgress(CraftingSim sim)
        {
            if (IncreasesProgress)
                sim.CurrentProgress += sim.GetProgressIncrease(GetEfficiency(sim));
        }

        public virtual void IncreaseQuality(CraftingSim sim)
        {
            if (IncreasesQuality)
            {
                sim.CurrentQuality += sim.GetQualityIncrease(GetEfficiency(sim));
                //inner quiet?
            }
        }

        public virtual CraftingBuff GetBuff()
        {
            throw new NotImplementedException();
        }
    }
}
