using FFXIVCraftingSim.Actions;
using FFXIVCraftingSim.Actions.Buffs;
using FFXIVCraftingSim.Solving;
using FFXIVCraftingSim.Types;
using FFXIVCraftingSim.Types.GameData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim
{
    public class CraftingSim : INotifyPropertyChanged
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
                if (level == value) return;
                ActualLevel = G.GetPlayerLevel(value);
                level = value;
                if (CurrentRecipe != null)
                    LevelDifference = G.GetCraftingLevelDifference(ActualLevel - CurrentRecipe.Level);
                ExecuteActions();
                PropertyChanged(this, new PropertyChangedEventArgs("Level"));
            }
        }

        private int _BaseCraftsmanship { get; set; }
        private int _BaseControl { get; set; }
        private int _BaseMaxCP { get; set; }


        public int BaseCraftsmanship
        {
            get { return _BaseCraftsmanship; }
            set
            {
                if (_BaseCraftsmanship == value) return;
                _BaseCraftsmanship = value;
                ExecuteActions();
                PropertyChanged(this, new PropertyChangedEventArgs("BaseCraftsmanship"));
            }
        }

        public int BaseControl
        {
            get { return _BaseControl; }
            set
            {
                if (_BaseControl == value) return;
                _BaseControl = value;
                PropertyChanged(this, new PropertyChangedEventArgs("BaseControl"));
            }
        }

        public int BaseMaxCP
        {
            get { return _BaseMaxCP; }
            set
            {
                if (_BaseMaxCP == value) return;
                _BaseMaxCP = value;

                PropertyChanged(this, new PropertyChangedEventArgs("BaseMaxCP"));
                PropertyChanged(this, new PropertyChangedEventArgs("MaxCP"));
                ExecuteActions();
            }
        }

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

        private int _CraftsmanshipBuff { get; set; }
        public int CraftsmanshipBuff
        {
            get { return _CraftsmanshipBuff; }
            set
            {
                if (_CraftsmanshipBuff == value) return;
                _CraftsmanshipBuff = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CraftsmanshipBuff"));
                PropertyChanged(this, new PropertyChangedEventArgs("Craftsmanship"));
            }
        }

        private int _ControlBuff { get; set; }
        public int ControlBuff
        {
            get { return _ControlBuff; }
            set
            {
                if (_ControlBuff == value) return;
                _ControlBuff = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ControlBuff"));
                PropertyChanged(this, new PropertyChangedEventArgs("Control"));
            }
        }

        private int _MaxCPBuff { get; set; }
        public int MaxCPBuff
        {
            get { return _MaxCPBuff;  }
            set
            {
                if (_MaxCPBuff == value) return;
                _MaxCPBuff = value;
                PropertyChanged(this, new PropertyChangedEventArgs("MaxCPBuff"));
                PropertyChanged(this, new PropertyChangedEventArgs("MaxCP"));
            }
        }
        
        public int Step { get; set; }

        private int _CurrentDurability { get; set; }
        public int CurrentDurability
        {
            get { return _CurrentDurability; }
            set
            {
                _CurrentDurability = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentDurability"));
            }
        }

        private int _CurrentProgress { get; set; }
        public int CurrentProgress
        {
            get { return _CurrentProgress;  }
            set
            {
                //if (_CurrentProgress == value) return;
                _CurrentProgress = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentProgress"));
            }
        }

        private int _CurrentQuality { get; set; }
        public int CurrentQuality
        {
            get { return _CurrentQuality; }
            set
            {
                //if (_CurrentQuality == value) return;
                _CurrentQuality = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentQuality"));
            }
        }

        public int _CurrentCP { get; set; }
        public int CurrentCP
        {
            get
            { return _CurrentCP; }

            set
            {
                if (_CurrentCP == value) return;
                _CurrentCP = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentCP"));
            }
        }



        public CraftingSimStepSettings[] StepSettings { get; private set; }

        private RecipeInfo _CurrentRecipe { get; set; }

        public RecipeInfo CurrentRecipe
        {
            get { return _CurrentRecipe;  }
            private set
            {
                if (_CurrentRecipe == value) return;
                _CurrentRecipe = value;
                ExecuteActions();
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentRecipe"));
                
            }
        }

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
                PropertyChanged(this, new PropertyChangedEventArgs("CraftingActionsLength"));
                PropertyChanged(this, new PropertyChangedEventArgs("CraftingActionsTime"));
            }
        }

        public int CraftingActionsTime
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
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

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
;
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

        public void AddActions(bool execute, IEnumerable < CraftingAction> actions)
        {
            AddActions(execute, actions.ToArray());
        }

        public void AddActions(bool execute = true, params CraftingAction[] actions)
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

        public void RemoveActionAt(int index)
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
            if (CurrentRecipe == recipe)
                return;
            CurrentRecipe = recipe;
            LevelDifference = G.GetCraftingLevelDifference(ActualLevel - recipe.Level);
            ExecuteActions();
        }


        public void ExecuteActions(Dictionary<ExtendedArray<ushort>, CraftingSim> states = null)
        {
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
                Step = i;

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
