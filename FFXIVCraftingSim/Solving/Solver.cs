using FFXIVCraftingSim.Actions;
using FFXIVCraftingSim.Solving.GeneticAlgorithm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Solving
{
    public class Solver
    {
        public CraftingSim Sim { get; private set; }
        private double SimScore { get; set; }
        public ushort[] AvailableActions { get; private set; }

        public int TaskCount { get; private set; }
        public Population[] Populations { get; private set; }
        private int BestIndex { get; set; }
        private Task[] Tasks { get; set; }

        public int Iterations { get; private set; }

        public bool Continue { get; set; }
        private bool NeedsUpdate { get; set; }

        public event Action<Population> GenerationRan = delegate { };

        public Solver(CraftingSim sim, ushort[] availableActions, int taskCount)
        {
            Sim = sim;
            AvailableActions = availableActions;
            TaskCount = taskCount;
            Tasks = new Task[TaskCount];
            Populations = new Population[TaskCount];
            for (int i = 0; i < TaskCount; i++)
                Populations[i] = new Population(Sim.Clone(), 250, CraftingSim.MaxActions, AvailableActions);
        }

        public void Start()
        {
            Continue = true;
            SimScore = Sim.Score;
            NeedsUpdate = false;
            Iterations = 0;
            Task.Run(() =>
            {
                Task.Run(UpdateLoop);
                for (int i = 0; i < TaskCount; i++)
                {
                    Tasks[i] = new Task(InnerStart, i);
                    Tasks[i].Start();
                }

            });
        }

        private void InnerStart(object index)
        {
            int i = (int)index;
            Populations[i].Reevaluate(Sim);
            while (Continue)
            {
                Populations[i].RunOnce();
                Iterations++;
                GenerationRan(Populations[i]);
                
                var best = Populations[i].Best;
                if (SimScore < best.Fitness && !NeedsUpdate)
                {
                    BestIndex = i;
                    NeedsUpdate = true;
                    SimScore = best.Fitness.Value;
                }
            }
        }

        private void UpdateLoop()
        {
            while(Continue)
            {
                if (NeedsUpdate)
                {
                    Sim.RemoveActions();
                    Sim.AddActions(Populations[BestIndex].Best.Values.Where(y => y > 0).Select(x => CraftingAction.CraftingActions[x]));
                    NeedsUpdate = false;
                }
            }
        }
    }
}
