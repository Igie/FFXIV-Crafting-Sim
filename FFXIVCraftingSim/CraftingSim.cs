using FFXIVCraftingSim.Actions;
using FFXIVCraftingSim.Actions.Buffs;
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
        public const int MaxActions = 30;

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
        public int ActualLevel { get; private set; }
        public int Craftsmanship { get; set; }
        public int Control { get; set; }
        public double ActualControl { get; set; }
        public int MaxCP { get; set; }
        
        public int Step { get; private set; }
        public int CurrentDurability { get; set; }
        public int CurrentProgress { get; set; }
        public int CurrentQuality { get; set; }
        public int CurrentCP { get; set; }

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


        public event Action<CraftingSim> FinishedExecution = delegate { };
        public event Action<CraftingSim, int> FinishedStep = delegate { };

        public CraftingSim()
        {
            CraftingActions = new CraftingAction[MaxActions];
            CraftingActionsLength = 0;
            CraftingBuffs = new List<CraftingBuff>();
        }

        public CraftingSim Clone()
        {
            CraftingSim result = new CraftingSim();

            result.Level = Level;
            result.Craftsmanship = Craftsmanship;
            result.Control = Control;
            result.MaxCP = MaxCP;
            if (CurrentRecipe != null)result.SetRecipe(CurrentRecipe);
            return result;
        }

        public void CopyTo(CraftingSim sim, bool copyActions = false)
        {
            sim.Level = Level;
            sim.Craftsmanship = Craftsmanship;
            sim.Control = Control;
            sim.MaxCP = MaxCP;
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
            ExectueActions();
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

            ExectueActions();
        }

        public void RemoveActions()
        {
            if (CraftingActionsLength > MaxActions)
                Debugger.Break();
            for (int i = 0; i < CraftingActionsLength; i++)
                CraftingActions[i] = null;
            CraftingActionsLength = 0;
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
            ExectueActions();
        }

        public void ExectueActions()
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
            for (int i = 0; i < CraftingActionsLength; i++)
            {

                CraftingAction action = CraftingActions[i];
                if (action == null)
                    break;
                if (action.Check(this, i) != CraftingActionResult.Success)
                {
                    RemoveRedundantActions();
                    FinishedExecution(this);
                    return;
                }
                Step++;

             
                action.IncreaseProgress(this);
                action.IncreaseQuality(this);

                CurrentDurability -= action.GetDurabilityCost(this);
                if (CurrentDurability > CurrentRecipe.Durability)
                    CurrentDurability = CurrentRecipe.Durability;
                CurrentCP -= action.CPCost;

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

            efficiency = realEfficiency;
                int value = (int)((Craftsmanship + 10000d) / (CurrentRecipe.RequiredCraftsmanship + 10000d) * (Craftsmanship * 21 / 100d + 2) * LevelDifference.ProgressFactor / 100d);
            return (int)(value * efficiency);
        }

        public int GetQualityIncrease(double efficiency)
        {
            double realEfficiency = efficiency;
            if (GreatStridesBuff != null)
                realEfficiency += efficiency;
            if (InnovationBuff != null)
                realEfficiency += efficiency * 0.5;
            efficiency = realEfficiency;
            ActualControl = Control;
            if (InnerQuietBuff != null)
            {
                ActualControl += (InnerQuietBuff.Stack - 1) * 0.2 * Control;
            }
           int value = (int)((ActualControl + 10000d) / (CurrentRecipe.RequiredControl + 10000d) * (ActualControl * 35d / 100d + 35) * LevelDifference.QualityFactor / 100d);
            return (int)(value * efficiency);
        }

        public double Score
        {
            get
            {
                if (CurrentRecipe == null) return -5000;
                double progress = CurrentProgress;
                if (progress > CurrentRecipe.MaxProgress)
                    progress = CurrentRecipe.MaxProgress;
                double result = progress / CurrentRecipe.MaxProgress * 10000;
                if (progress < CurrentRecipe.MaxProgress)
                    return result - 5000;
                double quality = CurrentQuality;
                if (quality > CurrentRecipe.MaxQuality)
                    quality = CurrentRecipe.MaxQuality;
                result += quality / CurrentRecipe.MaxQuality * 10000;
                for (int i = 0; i < CraftingActionsLength; i++)
                {
                    result -= CraftingActions[i].IsBuff ? 20 : 30;
                }
                return result; 
            }
        }
    }
}
