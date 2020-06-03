using FFXIVCraftingSim.Actions.Buffs;
using FFXIVCraftingSim.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        BuffUsedUp,
        NotEnoughDurability,
        NotEnoughCP,
        FirstActionOnly
    }

    [Flags]
    public enum CraftingActionType
    {
        None = 0,
        IncreasesProgress = 1,
        IncreasesQuality = 2,
        IsBuff = 4
    }


    public class CraftingAction
    {
        static CraftingAction()
        {

            CraftingActions = new Dictionary<int, CraftingAction>();
            CraftingActions.Add(1, BasicSynthesis);
            CraftingActions.Add(2, BasicTouch);
            CraftingActions.Add(3, MastersMend);
            CraftingActions.Add(4, InnerQuiet);
            CraftingActions.Add(5, WasteNot);
            CraftingActions.Add(6, Veneration);
            CraftingActions.Add(7, StandardTouch);
            CraftingActions.Add(8, GreatStrides);
            CraftingActions.Add(9, Innovation);
            CraftingActions.Add(10, WasteNotII);
            CraftingActions.Add(11, ByregotsBlessing);
            CraftingActions.Add(12, MuscleMemory);
            CraftingActions.Add(13, CarefulSynthesis);
            CraftingActions.Add(14, Manipulation);
            CraftingActions.Add(15, PrudentTouch);
            CraftingActions.Add(16, Reflect);
            CraftingActions.Add(17, PreparatoryTouch);
            CraftingActions.Add(18, Groundwork);
            CraftingActions.Add(19, DelicateSynthesis);
            CraftingActions.Add(20, Observe);
            CraftingActions.Add(21, FocusedSynthesis);
            CraftingActions.Add(22, FocusedTouch);
            CraftingActions.Add(23, NameOfTheElements);
            CraftingActions.Add(24, BrandOfTheElements);

            for (int i = 1; i <= CraftingActions.Count; i++)
            {
                if (CraftingActions[i].Id != i)
                    Debugger.Break();
            }

        }

        public static Dictionary<int, CraftingAction> CraftingActions { get; private set; }

        public static BasicSynthesis BasicSynthesis { get; set; } = new BasicSynthesis();
        public static BasicTouch BasicTouch { get; private set; } = new BasicTouch();
        public static MastersMend MastersMend { get; private set; } = new MastersMend();
        public static InnerQuiet InnerQuiet { get; private set; } = new InnerQuiet();
        public static WasteNot WasteNot { get; private set; } = new WasteNot();
        public static Veneration Veneration { get; private set; } = new Veneration();
        public static StandardTouch StandardTouch { get; private set; } = new StandardTouch();
        public static GreatStrides GreatStrides { get; private set; } = new GreatStrides();
        public static Innovation Innovation { get; private set; } = new Innovation();
        public static WasteNotII WasteNotII { get; private set; } = new WasteNotII();
        public static ByregotsBlessing ByregotsBlessing { get; private set; } = new ByregotsBlessing();
        public static MuscleMemory MuscleMemory { get; private set; } = new MuscleMemory();
        public static CarefulSynthesis CarefulSynthesis { get; private set; } = new CarefulSynthesis();
        public static Manipulation Manipulation { get; private set; } = new Manipulation();
        public static PrudentTouch PrudentTouch { get; private set; } = new PrudentTouch();
        public static Reflect Reflect { get; private set; } = new Reflect();
        public static PreparatoryTouch PreparatoryTouch { get; private set; } = new PreparatoryTouch();
        public static Groundwork Groundwork { get; private set; } = new Groundwork();
        public static DelicateSynthesis DelicateSynthesis { get; private set; } = new DelicateSynthesis();
        public static Observe Observe { get; private set; } = new Observe();
        public static FocusedSynthesis FocusedSynthesis { get; private set; } = new FocusedSynthesis();
        public static FocusedTouch FocusedTouch { get; private set; } = new FocusedTouch();
        public static NameOfTheElements NameOfTheElements { get; private set; } = new NameOfTheElements();
        public static BrandOfTheElements BrandOfTheElements { get; private set; } = new BrandOfTheElements();

        public CraftingActionType CraftingActionType
        {
            get
            {
                CraftingActionType flags = CraftingActionType.None;
                if (IncreasesProgress)
                    flags |= CraftingActionType.IncreasesProgress;
                if (IncreasesQuality)
                    flags |= CraftingActionType.IncreasesQuality;
                if (IsBuff)
                    flags |= CraftingActionType.IsBuff;
                return flags;
            }
        }

        public virtual ushort Id
        {
            get { throw new NotImplementedException(); }
        }

        public virtual string Name
        {
            get { throw new NotImplementedException(); }
        }

        public virtual int Level
        {
            get { throw new NotImplementedException(); }

        }

        public virtual bool IsBuff
        {
            get
            {
                { throw new NotImplementedException(); }
            }
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

        protected virtual int CPCost
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
            int durabilityCost = DurabilityCost;
            if (sim.GetStepSettings().RecipeCondition == RecipeCondition.Sturdy)
                durabilityCost /= 2;
            if (sim.WasteNotBuff != null)
                durabilityCost = (int)Math.Ceiling(durabilityCost / 2d);
            return durabilityCost;

            //waste not check
        }

        public int GetCPCost(CraftingSim sim)
        {
            int cpCost = CPCost;
            if (sim.GetStepSettings().RecipeCondition == RecipeCondition.Pliant)
                cpCost = (int)Math.Ceiling(cpCost / 2d);
            return cpCost;
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
