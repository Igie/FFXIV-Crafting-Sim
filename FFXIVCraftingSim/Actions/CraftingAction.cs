﻿using FFXIVCraftingSim.Actions.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Actions
{
    public enum CraftingActionResult
    {
        Success,
        CraftCompleted,
        NeedsBuff,
        NeedsNoBuff,
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
            CraftingActions.Add(3, new MastersMend());
            CraftingActions.Add(4, new InnerQuiet());
            CraftingActions.Add(5, new WasteNot());
            CraftingActions.Add(6, new Veneration());
            CraftingActions.Add(7, new StandardTouch());
            CraftingActions.Add(8, new GreatStrides());
            CraftingActions.Add(9, new Innovation());
            CraftingActions.Add(10, new WasteNotII());
            CraftingActions.Add(11, new ByregotsBlessing());
            CraftingActions.Add(12, new MuscleMemory());
            CraftingActions.Add(13, new CarefulSynthesis());
            CraftingActions.Add(14, new Manipulation());
            CraftingActions.Add(15, new PrudentTouch());
            CraftingActions.Add(16, new Reflect());
            CraftingActions.Add(17, new PreparatoryTouch());
            CraftingActions.Add(18, new Groundwork());
            CraftingActions.Add(19, new DelicateSynthesis());
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
            else return sim.WasteNotBuff == null ? DurabilityCost : DurabilityCost / 2;

            //waste not check
        }

        public virtual void IncreaseProgress(CraftingSim sim)
        {
            if (IncreasesProgress)
            {  
                sim.CurrentProgress += sim.GetProgressIncrease(GetEfficiency(sim));
                if (sim.MuscleMemoryBuff != null)
                    sim.MuscleMemoryBuff.NeedsRemove = true;
            }
        }

        public virtual void IncreaseQuality(CraftingSim sim)
        {
            if (IncreasesQuality)
            {
                sim.CurrentQuality += sim.GetQualityIncrease(GetEfficiency(sim));
                if (sim.InnerQuietBuff != null)
                {
                    sim.InnerQuietBuff.Stack++;
                    if (sim.InnerQuietBuff.Stack > 11)
                        sim.InnerQuietBuff.Stack = 11;
                }

                if (sim.GreatStridesBuff != null)
                    sim.GreatStridesBuff.NeedsRemove = true;
            }
        }

        public virtual void AddBuff(CraftingSim sim)
        {
            throw new NotImplementedException();
        }
    }
}
