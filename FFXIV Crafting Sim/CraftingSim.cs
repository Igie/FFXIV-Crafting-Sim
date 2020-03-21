using FFXIV_Crafting_Sim.Actions;
using FFXIV_Crafting_Sim.Types.GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV_Crafting_Sim
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
        public int CraftingActionsLength { get; private set; }



        public event Action<CraftingSim> FinishedExecution = delegate { };
        public CraftingSim()
        {
            CraftingActions = new CraftingAction[MaxActions];
            CraftingActionsLength = 0;
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
            CurrentDurability = CurrentRecipe.Durability;
            CurrentCP = MaxCP;
            CurrentProgress = 0;
            CurrentQuality = 0;
            Step = 0;
            for (int i = 0; i < CraftingActionsLength; i++)
            {

                CraftingAction action = CraftingActions[i];
                if (action.Check(this, i) != CraftingActionResult.Success)
                {
                    RemoveRedundantActions();
                    return;
                }
                Step++;

                CurrentDurability -= action.GetDurabilityCost(this);
                CurrentCP -= action.CPCost;
                action.IncreaseProgress(this);
                action.IncreaseQuality(this);

                //buffs
            }

            FinishedExecution(this);
        }

        public int GetProgressIncrease(double efficiency)
        {
            int value = (int)((Craftsmanship + 10000d) / (CurrentRecipe.RequiredCraftsmanship + 10000d) * (Craftsmanship * 21 / 100d + 2) * LevelDifference.ProgressFactor / 100d);
            return (int)(value * efficiency);
        }

        public int GetQualityIncrease(double efficiency)
        {
            ActualControl = Control;
           int value = (int)((ActualControl + 10000d) / (CurrentRecipe.RequiredControl + 10000d) * (ActualControl * 35d / 100d + 35) * LevelDifference.QualityFactor / 100d);
            return (int)(value * efficiency);
        }
    }
}
