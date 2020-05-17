using FFXIVCraftingSim.Actions;
using FFXIVCraftingSim.Actions.Buffs;
using FFXIVCraftingSim.Types;
using FFXIVCraftingSim.Types.GameData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim
{
    public class CraftingSim
    {
        public const int MaxActions = 40;

        private int level;
        public int Level
        {
            get
            {
                return level;
            }
            set
            {
                ActualLevel = G.GetPlayerLevel(value);
                level = value;
                if (CurrentRecipe != null)
                    LevelDifference = G.GetCraftingLevelDifference(ActualLevel - CurrentRecipe.Level);
            }
        }
        public int BaseCraftsmanship { get; set; }
        public int BaseControl { get; set; }
        public int BaseMaxCP { get; set; }

        public int ActualLevel { get; private set; }
        public int Craftsmanship
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
        public int Control
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
        public double ActualControl { get; set; }
        public int MaxCP
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
        
        public int CraftsmanshipBuff { get; set; }
        public int ControlBuff { get; set; }
        public int MaxCPBuff { get; set; }
        
        public int Step { get; private set; }
        public int CurrentDurability { get; set; }
        public int CurrentProgress { get; set; }
        public int CurrentQuality { get; set; }
        public int CurrentCP { get; set; }

        public CraftingSimStepSettings[] StepSettings { get; private set; }

        public RecipeInfo CurrentRecipe { get; private set; }

        public LevelDifferenceInfo LevelDifference { get; private set; }
        

        public CraftingAction[] CraftingActions { get; private set; }
        public List<CraftingBuff> CraftingBuffs { get; private set; }
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

        private int _CraftingActionsLength;
        public int CraftingActionsLength
        {
            get
            {
                return _CraftingActionsLength;
            }
            private set
            {
                _CraftingActionsLength = value;
                if (value > MaxActions)
                    Debugger.Break();
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

        public CraftingSim Clone()
        {
            CraftingSim result = new CraftingSim();

            result.Level = Level;
            result.BaseCraftsmanship = BaseCraftsmanship;
            result.BaseControl = BaseControl;
            result.BaseMaxCP = BaseMaxCP;
            result.CraftsmanshipBuff = CraftsmanshipBuff;
            result.ControlBuff = ControlBuff;
            result.MaxCPBuff = MaxCPBuff;
            result.StepSettings = new CraftingSimStepSettings[MaxActions];
            for (int i = 0; i < MaxActions; i++)
                result.StepSettings[i] = StepSettings[i].Clone();
            if (CurrentRecipe != null)result.SetRecipe(CurrentRecipe);
            return result;
        }

        public void CopyTo(CraftingSim sim, bool copyActions = false)
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

            sim.SetRecipe(CurrentRecipe);
            if (copyActions)
            {
                sim.RemoveActions();
                sim.AddActions(GetCraftingActions());
            }
        }

        public void AddActions(IEnumerable<CraftingAction> actions)
        {
            AddActions(actions.ToArray());
        }

        public void AddActions(params CraftingAction[] actions)
        {
            if (CraftingActionsLength >= MaxActions)
                return;
            for (int i = 0; i < actions.Length; i++)
                CraftingActions[CraftingActionsLength++] = actions[i];
            if (CraftingActionsLength > MaxActions)
                Debugger.Break();
            ExecuteActions();
        }

        public void RemoveActionAt(int index)
        {
            if (index >= CraftingActionsLength || index < 0)
                throw new IndexOutOfRangeException();
            CraftingActionsLength--;
            for (int i = index; i < CraftingActionsLength; i++)
            {
                CraftingActions[i] = CraftingActions[i + 1];
            }
            CraftingActions[CraftingActionsLength] = null;

            ExecuteActions();
        }

        public void RemoveActions()
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

        public void SetRecipe(RecipeInfo recipe)
        {
            CurrentRecipe = recipe;
            LevelDifference = G.GetCraftingLevelDifference(ActualLevel - recipe.Level);
            ExecuteActions();
        }

        public void ExecuteActions()
        {
            if (CurrentRecipe == null)
                return;
            CurrentDurability = CurrentRecipe.Durability;
            CurrentCP = MaxCP;
            CurrentProgress = 0;
            CurrentQuality = 0;
            Step = 0;

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

            for (int i = 0; i < CraftingActionsLength; i++)
            {
                CraftingAction action = CraftingActions[i];
                Step = i;

                if (CurrentDurability <= 0 ||
                    action.AsFirstActionOnly && i > 0 ||
                    CurrentProgress >= CurrentRecipe.MaxProgress ||
                    action.GetCPCost(this) > CurrentCP ||
                    action.CheckInner(this) != CraftingActionResult.Success)
                {
                    RemoveRedundantActions();
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

                if (action.AddsBuff)
                    action.AddBuff(this);

                FinishedStep(this, i);
            }

            FinishedExecution(this);
        }

        public int GetProgressIncrease(double efficiency)
        {
            double realEfficiency = efficiency;
            if (MuscleMemoryBuff != null)
                realEfficiency += efficiency;
            if (VenerationBuff != null)
                realEfficiency += efficiency * 0.5;

                int value = (int)((Craftsmanship + 10000d) / (CurrentRecipe.RequiredCraftsmanship + 10000d) * (Craftsmanship * 21 / 100d + 2) * LevelDifference.ProgressFactor / 100d);
            return (int)(value * realEfficiency);
        }

        public int GetQualityIncrease(double efficiency)
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
            //double ddValue = (ActualControl + 10000d) / (CurrentRecipe.RequiredControl + 10000d) * (ActualControl * 0.35 + 35) * LevelDifference.QualityFactor / 100d;
            int value = (int)(dValue * conditionMultiplier);
                return (int)(value * realEfficiency);
        }

        public CraftingSimStepSettings GetStepSettings()
        {
            return StepSettings[Step];
        }

        public void SetStepSetting(int step, CraftingSimStepSettings settings)
        {
            if (settings == null)
            {
                StepSettings[step].RecipeCondition = RecipeCondition.Normal;
                return;
            }
            StepSettings[step] = settings;
        }

        public bool CustomRecipe
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



            double progressPercent = progress * maxQuality;
            //score += progressPercent;
            score += progress * 1000d / CurrentRecipe.MaxProgress;

            if (progress < CurrentRecipe.MaxProgress)
                return score;

            int quality = CurrentQuality;
            if (quality > CurrentRecipe.MaxQuality)
                quality = CurrentRecipe.MaxQuality;
            double qualityPercent = quality * 1000d / CurrentRecipe.MaxQuality;

            //score += quality;

           
            score += qualityPercent;
            score *= 10000;
            //if (quality < CurrentRecipe.MaxQuality)
            //return score;

            double actionScore = 0;

            for (int i = 0; i < CraftingActionsLength; i++)
                actionScore += CraftingActions[i].IsBuff ? 2 : 3;

            score +=  1000d / actionScore;

            score += CurrentCP / 1000d;
            //score /= 1000000;
            return score;
        }

        public delegate double ScoreDelegate(CraftingSim sim);
    }
}
