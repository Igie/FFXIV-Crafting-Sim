using FFXIVCraftingSimLib.Actions;
using FFXIVCraftingSimLib.Actions.Buffs;
using FFXIVCraftingSimLib.Solving;
using FFXIVCraftingSimLib.Types;
using FFXIVCraftingSimLib.Types.GameData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSimLib
{
    public class CraftingSim
    {
        public const int MaxActions = 40;

        protected int level;

        public virtual int Level
        {
            get
            {
                return level;
            }
            set
            {
                if (level == value) return;
                ActualLevel = Utils.GetPlayerLevel(value);
                level = value;
                if (CurrentRecipe != null)
                    LevelDifference = Utils.GetCraftingLevelDifference(ActualLevel - CurrentRecipe.Level);
                ExecuteActions();
            }
        }

        protected int _BaseCraftsmanship { get; set; }
        protected int _BaseControl { get; set; }
        protected int _BaseMaxCP { get; set; }


        public virtual int BaseCraftsmanship
        {
            get { return _BaseCraftsmanship; }
            set
            {
                if (_BaseCraftsmanship == value) return;
                _BaseCraftsmanship = value;
                ExecuteActions();
            }
        }

        public virtual int BaseControl
        {
            get { return _BaseControl; }
            set
            {
                if (_BaseControl == value) return;
                _BaseControl = value;
                ExecuteActions();
            }
        }

        public virtual int BaseMaxCP
        {
            get { return _BaseMaxCP; }
            set
            {
                if (_BaseMaxCP == value) return;
                _BaseMaxCP = value;
                ExecuteActions();
            }
        }

        public virtual int ActualLevel { get; protected set; }
        public virtual int Craftsmanship
        {
            get
            {
                return BaseCraftsmanship + CraftsmanshipBuff;
            }
            set
            {
                BaseCraftsmanship = value - CraftsmanshipBuff;
            }
        }
        public virtual int Control
        {

            get
            {
                return BaseControl + ControlBuff;
            }
            set
            {
                BaseControl = value - ControlBuff;
            }

        }
        public virtual double ActualControl { get; set; }
        public virtual int MaxCP
        {

            get
            {
                return BaseMaxCP + MaxCPBuff;
            }
            set
            {
                BaseMaxCP = value - MaxCPBuff;
            }

        }

        public virtual int CraftsmanshipBuff { get; set; }


        public virtual int ControlBuff { get; set; }


        public virtual int MaxCPBuff { get; set; }
        public virtual int Step { get; set; }

        public virtual int CurrentDurability { get; set; }


        public virtual int CurrentProgress { get; set; }

        public virtual int CurrentQuality { get; set; }

        public virtual int CurrentCP { get; set; }




        public CraftingSimStepSettings[] StepSettings { get; protected set; }

        protected RecipeInfo _CurrentRecipe { get; set; }

        public virtual RecipeInfo CurrentRecipe
        {
            get { return _CurrentRecipe; }
            protected set
            {
                if (_CurrentRecipe == value) return;
                _CurrentRecipe = value;
                ExecuteActions();

            }
        }

        public LevelDifferenceInfo LevelDifference { get; protected set; }


        public CraftingAction[] CraftingActions { get; protected set; }
        public List<CraftingBuff> CraftingBuffs { get; protected set; }
        public InnerQuietBuff InnerQuietBuff { get; set; }
        public WasteNotBuff WasteNotBuff { get; set; }
        public VenerationBuff VenerationBuff { get; set; }
        public GreatStridesBuff GreatStridesBuff { get; set; }
        public InnovationBuff InnovationBuff { get; set; }
        public MuscleMemoryBuff MuscleMemoryBuff { get; set; }
        public ManipulationBuff ManipulationBuff { get; set; }
        public ObserveBuff ObserveBuff { get; set; }
        public NameOfTheElementsBuff NameOfTheElementsBuff { get; set; }

        public bool NameOfTheElementsUsed { get; set; }

        protected int _CraftingActionsLength;
        public virtual int CraftingActionsLength
        {
            get
            {
                return _CraftingActionsLength;
            }
            protected set
            {
                _CraftingActionsLength = value;
                if (value > MaxActions)
                    Debugger.Break();
            }
        }

        public virtual int CraftingActionsTime
        {
            get
            {
                int time = 0;
                for (int i = 0; i < CraftingActionsLength; i++)
                    time += CraftingActions[i].IsBuff ? 2 : 3;
                return time;
            }
        }


        public ScoreDelegate ScoreFunction { get; set; }

        public event Action<CraftingSim> FinishedExecution = delegate { };
        public event Action<CraftingSim, int> FinishedStep = delegate { };

        public CraftingSim()
        {
            CraftingActions = new CraftingAction[MaxActions];
            CraftingActionsLength = 0;
            CraftingBuffs = new List<CraftingBuff>();
            StepSettings = new CraftingSimStepSettings[MaxActions];
            for (int i = 0; i < MaxActions; i++)
                StepSettings[i] = new CraftingSimStepSettings();
        }

        public bool Solved => CurrentProgress >= CurrentRecipe.MaxProgress && CurrentQuality >= CurrentRecipe.MaxQuality;

        public virtual CraftingSim Clone(bool copyActions = false)
        {
            CraftingSim result = new CraftingSim();
            result.SetRecipe(CurrentRecipe);
            CopyTo(result, copyActions);
            return result;
        }

        public virtual void CopyTo(CraftingSim sim, bool copyActions = false)
        {
            sim.Level = Level;
            sim.BaseCraftsmanship = BaseCraftsmanship;
            sim.BaseControl = BaseControl;
            sim.BaseMaxCP = BaseMaxCP;
            sim.CraftsmanshipBuff = CraftsmanshipBuff;
            sim.ControlBuff = ControlBuff;
            sim.MaxCPBuff = MaxCPBuff;
            sim.StepSettings = new CraftingSimStepSettings[MaxActions];

            for (int i = 0; i < MaxActions; i++)
                sim.StepSettings[i] = StepSettings[i].Clone();

            if (copyActions)
            {
                sim.RemoveActions();
                sim.AddActions(false, GetCraftingActions());

                sim.Step = Step;
                sim.CurrentDurability = CurrentDurability;
                sim.CurrentProgress = CurrentProgress;
                sim.CurrentQuality = CurrentQuality;
                sim.CurrentCP = CurrentCP;

                sim.InnerQuietBuff = InnerQuietBuff?.Clone() as InnerQuietBuff;
                if (sim.InnerQuietBuff != null) sim.CraftingBuffs.Add(sim.InnerQuietBuff);

                sim.WasteNotBuff = WasteNotBuff?.Clone() as WasteNotBuff;
                if (sim.WasteNotBuff != null) sim.CraftingBuffs.Add(sim.WasteNotBuff);

                sim.VenerationBuff = VenerationBuff?.Clone() as VenerationBuff;
                if (sim.VenerationBuff != null) sim.CraftingBuffs.Add(sim.VenerationBuff);

                sim.GreatStridesBuff = GreatStridesBuff?.Clone() as GreatStridesBuff;
                if (sim.GreatStridesBuff != null) sim.CraftingBuffs.Add(sim.GreatStridesBuff);

                sim.InnovationBuff = InnovationBuff?.Clone() as InnovationBuff;
                if (sim.InnovationBuff != null) sim.CraftingBuffs.Add(sim.InnovationBuff);

                sim.MuscleMemoryBuff = MuscleMemoryBuff?.Clone() as MuscleMemoryBuff;
                if (sim.MuscleMemoryBuff != null) sim.CraftingBuffs.Add(sim.MuscleMemoryBuff);

                sim.ManipulationBuff = ManipulationBuff?.Clone() as ManipulationBuff;
                if (sim.ManipulationBuff != null) sim.CraftingBuffs.Add(sim.ManipulationBuff);

                sim.ObserveBuff = ObserveBuff?.Clone() as ObserveBuff;
                if (sim.ObserveBuff != null) sim.CraftingBuffs.Add(sim.ObserveBuff);

                sim.NameOfTheElementsBuff = NameOfTheElementsBuff?.Clone() as NameOfTheElementsBuff;

                sim.NameOfTheElementsUsed = NameOfTheElementsUsed;
            }
        }

        public virtual void AddActions(bool execute, IEnumerable<CraftingAction> actions)
        {
            AddActions(execute, actions.ToArray());
        }

        public virtual void AddActions(bool execute = true, params CraftingAction[] actions)
        {
            if (CraftingActionsLength >= MaxActions)
                return;
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i].Level <= Level)
                {
                    CraftingActions[CraftingActionsLength] = actions[i];
                    CraftingActionsLength++;
                }
            }
            if (CraftingActionsLength > MaxActions)
                Debugger.Break();
            if (execute)
                ExecuteActions();
        }

        public virtual void RemoveActionAt(int index)
        {
            if (index >= CraftingActionsLength || index < 0)
                throw new IndexOutOfRangeException();

            for (int i = index; i < CraftingActionsLength - 1; i++)
            {
                CraftingActions[i] = CraftingActions[i + 1];
            }

            CraftingActions[CraftingActionsLength - 1] = null;
            CraftingActionsLength--;
            ExecuteActions();
        }

        public virtual void RemoveActions()
        {
            if (CraftingActionsLength > MaxActions)
                Debugger.Break();
            for (int i = 0; i < CraftingActionsLength; i++)
                CraftingActions[i] = null;
            CraftingActionsLength = 0;
            ExecuteActions();
        }

        public void RemoveRedundantActions()
        {
            for (int i = Step; i < CraftingActionsLength; i++)
                CraftingActions[i] = null;
            CraftingActionsLength = Step;
        }

        public CraftingAction[] GetCraftingActions()
        {
            CraftingAction[] result = new CraftingAction[CraftingActionsLength];
            Array.Copy(CraftingActions, result, CraftingActionsLength);
            return result;
        }

        public virtual void SetRecipe(RecipeInfo recipe)
        {
            if (CurrentRecipe == recipe)
                return;
            CurrentRecipe = recipe;
            LevelDifference = Utils.GetCraftingLevelDifference(ActualLevel - recipe.Level);
            ExecuteActions();
        }


        public virtual void ExecuteActions(Dictionary<ExtendedArray<ushort>, CraftingSim> states = null)
        {
            CurrentCP = MaxCP;
            CurrentProgress = 0;
            CurrentQuality = 0;

            CraftingBuffs.Clear();
            InnerQuietBuff = null;
            WasteNotBuff = null;
            VenerationBuff = null;
            GreatStridesBuff = null;
            InnovationBuff = null;
            MuscleMemoryBuff = null;
            ManipulationBuff = null;
            ObserveBuff = null;
            NameOfTheElementsBuff = null;
            NameOfTheElementsUsed = false;
            Step = 0;
            if (CurrentRecipe == null)
                return;
           
            //if (states == null)
            //    states = G.CraftingStates;

            CurrentDurability = CurrentRecipe.Durability;

            if (CraftingActionsLength == 0)
            {
                //FinishedStep(this, 0);
                FinishedExecution(this);
                return;
            }

            ExtendedArray<ushort> actions = null;
            if (states != null)
            {
                actions = GetCraftingActions().Select(x => x.Id).ToArray();
                if (states.ContainsKey(actions))
                {
                    states[actions].CopyTo(this, true);
                    FinishedExecution(this);
                    return;
                }
            }

            for (int i = 0; i < CraftingActionsLength; i++)
            {
                CraftingAction action = CraftingActions[i];


                if (CurrentDurability <= 0 ||
                    action.AsFirstActionOnly && i > 0 ||
                    CurrentProgress >= CurrentRecipe.MaxProgress ||
                    action.GetCPCost(this) > CurrentCP ||
                    action.CheckInner(this) != CraftingActionResult.Success)
                {
                    RemoveRedundantActions();
                    if (states != null)
                    {
                        actions = GetCraftingActions().Select(x => x.Id).ToArray();
                        CraftingSim sim = Clone();
                        CopyTo(sim, true);
                        states[actions] = sim;
                    }
                    FinishedExecution(this);
                    return;
                }

                
                action.IncreaseProgress(this);
                action.IncreaseQuality(this);

                CurrentDurability -= action.GetDurabilityCost(this);
                if (CurrentDurability > CurrentRecipe.Durability)
                    CurrentDurability = CurrentRecipe.Durability;
                CurrentCP -= action.GetCPCost(this);

                
                foreach (var buff in CraftingBuffs)
                    buff.Step(this);

                for (int j = 0; j < CraftingBuffs.Count; j++)
                {
                    var buff = CraftingBuffs[j];
                    if (buff.NeedsRemove)
                    {
                        buff.Remove(this);
                        j--;
                    }
                }
                Step++;

                if (action.AddsBuff)
                    action.AddBuff(this);

                FinishedStep(this, i);
            }

            if (states != null)
            {
                actions = GetCraftingActions().Select(x => x.Id).ToArray();
                CraftingSim sim = Clone();
                CopyTo(sim, true);
                states[actions] = sim;
            }
            FinishedExecution(this);
        }

        public virtual int GetProgressIncrease(double efficiency)
        {
            double realEfficiency = efficiency;
            if (MuscleMemoryBuff != null)
                realEfficiency += efficiency;
            if (VenerationBuff != null)
                realEfficiency += efficiency * 0.5;

            int value = (int)((Craftsmanship + 10000d) / (CurrentRecipe.RequiredCraftsmanship + 10000d) * (Craftsmanship * 21 / 100d + 2) * LevelDifference.ProgressFactor / 100d);
            return (int)(value * realEfficiency);
        }

        public virtual int GetQualityIncrease(double efficiency)
        {
            double realEfficiency = efficiency;
            if (GreatStridesBuff != null)
                realEfficiency += efficiency;
            if (InnovationBuff != null)
                realEfficiency += efficiency * 0.5;
            CraftingSimStepSettings settings = GetStepSettings();
            double conditionMultiplier = 1;
            if (settings.RecipeCondition == RecipeCondition.Good)
                conditionMultiplier = 1.5;
            if (settings.RecipeCondition == RecipeCondition.Excellent)
                conditionMultiplier = 4;
            if (settings.RecipeCondition == RecipeCondition.Poor)
                conditionMultiplier = 0.5;
            ActualControl = Control;
            if (InnerQuietBuff != null)
            {
                ActualControl += (InnerQuietBuff.Stack - 1) * 0.2 * Control;
            }

            double dValue = (ActualControl + 10000d) / (CurrentRecipe.RequiredControl + 10000d) * (ActualControl * 0.35 + 35) * LevelDifference.QualityFactor / 100d;
            int value = (int)(dValue * conditionMultiplier);
            return (int)(value * realEfficiency);
        }

        public virtual CraftingSimStepSettings GetStepSettings()
        {
            if (Step > MaxActions - 1)
                Debugger.Break();
            return StepSettings[Step];
        }

        public virtual void SetStepSetting(int step, CraftingSimStepSettings settings)
        {
            if (settings == null)
            {
                StepSettings[step].RecipeCondition = RecipeCondition.Normal;
                return;
            }
            StepSettings[step] = settings;
        }

        public virtual bool CustomRecipe
        {
            get
            {
                return StepSettings.Any(x => x.RecipeCondition != RecipeCondition.Normal);
            }
        }

        public double Score
        {
            get
            {
                if (ScoreFunction == null)
                    return DefaultGetScore();
                return ScoreFunction(this);
            }
        }

        public override string ToString()
        {
            return base.ToString() + CraftingActionsLength.ToString();
        }

        public double DefaultGetScore()
        {
            if (CurrentRecipe == null) return 0;

            double score = 0;

            double maxQuality = CurrentRecipe.MaxQuality;

            int progress = CurrentProgress;
            if (progress > CurrentRecipe.MaxProgress)
                progress = CurrentRecipe.MaxProgress;

            score += progress * 1000d / CurrentRecipe.MaxProgress;

            if (progress < CurrentRecipe.MaxProgress)
                return score;

            int quality = CurrentQuality;
            if (quality > CurrentRecipe.MaxQuality)
                quality = CurrentRecipe.MaxQuality;
            double qualityPercent = quality * 1000d / CurrentRecipe.MaxQuality;

            score += qualityPercent;
            score *= 10000;

            double actionScore = 0;

            for (int i = 0; i < CraftingActionsLength; i++)
                actionScore += CraftingActions[i].IsBuff ? 2 : 3;

            score += 1000d / actionScore;

            score += CurrentCP / 1000d;
            return score;
        }

        public delegate double ScoreDelegate(CraftingSim sim);
    }
}
