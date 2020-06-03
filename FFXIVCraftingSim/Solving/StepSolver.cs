using FFXIVCraftingSim.Actions;
using FFXIVCraftingSim.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using C = FFXIVCraftingSim.Actions.CraftingAction;

namespace FFXIVCraftingSim.Solving
{
    public class StepSolver
    {
        public CraftingSim Sim { get; private set; }

        private int MaxProgress { get; set; }
        private int MaxQuality { get; set; }

        public StepSolver()
        {
            
        }

        public void Solve(CraftingSim sim)
        {
            Sim = sim.Clone();
            MaxProgress = Sim.CurrentRecipe.MaxProgress;
            MaxQuality = Sim.CurrentRecipe.MaxQuality;

            var a = C.CraftingActions.Values.Where(x => x.IncreasesQuality).ToList();
            a.Add(C.InnerQuiet);
            a.Add(C.WasteNot);
            a.Add(C.WasteNotII);
            a.Add(C.GreatStrides);
            a.Add(C.Manipulation);
            a.Add(C.MastersMend);
            a.Add(C.Observe);

            Sim.RemoveActions();

            //Sim.AddActions(C.Reflect);
            ushort[] actions = new ushort[4];
            for (int i = 0; i < 4; i++)
                actions[i] = 0;


            Dictionary<ExtendedArray<ushort>, int> QValues = new Dictionary<ExtendedArray<ushort>, int>();
            PossibleCraftingAction pAction = new PossibleCraftingAction(a.Select(x => x.Id));
            
           while(true)
            {
                Sim.RemoveActions();
                Sim.AddActions(true, actions.Select(x => C.CraftingActions[x]));
                QValues[Sim.GetCraftingActions().Select(x => x.Id).ToArray()] = Sim.CurrentQuality;
                int index = 0;
                while (a[index] == a.Last())
                {
                    actions[index] = a[0].Id;
                    index++;
                }
                actions[index]++;

            }
        }


        private void SolveForCurrentActions(CraftingAction[] availableActions, CraftingAction[] actions)
        {
            Sim.RemoveActions();
            Sim.AddActions(true, actions);
            var currentActions = Sim.GetCraftingActions();

            if (currentActions.Length == actions.Length)
            {
                for (int i = 0; i < availableActions.Length; i++)
                {
                    var newActions = new CraftingAction[currentActions.Length + 1];
                    currentActions.CopyTo(newActions, 0);
                    newActions[newActions.Length - 1] = availableActions[i];
                    SolveForCurrentActions(availableActions, newActions);
                }
            }
        }


        private double GetQualityScore(CraftingSim sim)
        {
            int quality = sim.CurrentQuality;
            if (quality > sim.CurrentRecipe.MaxQuality)
                quality = sim.CurrentRecipe.MaxQuality;
            double result = quality * 100000 / sim.CurrentRecipe.MaxQuality;
            var actions = sim.GetCraftingActions();
            result += sim.CraftingActionsLength;
            return result;
        }

    }



    public class CraftingState
    {
        public int CurrentDurability { get; set; }
        public int CurrentCP { get; set; }
        public int CurrentProgress { get; set; }
        public int CurrentQuality { get; set; }

        public bool CanAddActions { get; set; }

        List<CraftingState> NextStates { get; set; }
        
        public ushort[] ActionIds { get; private set; }

        public static CraftingState GetState(CraftingSim sim)
        {
            return new CraftingState {
                CurrentDurability = sim.CurrentDurability,
                CurrentCP = sim.CurrentCP,
                CurrentProgress = sim.CurrentProgress,
                CurrentQuality = sim.CurrentQuality,
                ActionIds = sim.GetCraftingActions().Select(x => x.Id).ToArray(),
                CanAddActions = !(sim.CurrentDurability == 0 || 
                sim.CurrentProgress >= 
                sim.CurrentRecipe.MaxProgress || 
                sim.CurrentCP == sim.MaxCP)
            };
        }

        public CraftingState()
        {
            NextStates = new List<CraftingState>();
        }

        public CraftingState FindBetterState(CraftingSim sim, IEnumerable<int> actions, CraftingState betterThan)
        {
            
            int l = sim.CraftingActionsLength;

            foreach(var i in actions)
            {
                CraftingSim s = new CraftingSim();
                sim.CopyTo(s, true);

                s.AddActions(true, C.CraftingActions[i]);
                CraftingState state = GetState(s);
                if (s.CurrentQuality >= s.CurrentRecipe.MaxQuality)
                {
                    if (state.ActionIds.Length < betterThan.ActionIds.Length)
                        return state;
                }
                if (s.CraftingActionsLength == l)
                {
                    state.CanAddActions = false;
                } else 
                {
                    NextStates.Add(state);
                    if (state.CanAddActions)
                    {
                        return state.FindBetterState(s, actions, betterThan);
                    }
                }

            }

            return null;
        }

        public override int GetHashCode()
        {
            return (13 * CurrentDurability << 3) ^ (CurrentCP * 29) ^ (CurrentProgress * 31) ^ CurrentQuality;
        }
    }
}
