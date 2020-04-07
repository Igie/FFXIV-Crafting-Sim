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
        public ushort[] AvailableActions { get; private set; }

        public int TaskCount { get; private set; }
        public Population[] Populations { get; private set; }
        private int BestIndex { get; set; }
        private Chromosome BestChromosome { get; set; }
        private Task[] Tasks { get; set; }

        public int Iterations { get; private set; }

        public bool Continue { get; set; }
        private bool NeedsUpdate { get; set; }

        private bool LeaveStartingActions { get; set; }

        public event Action<Population> GenerationRan = delegate { };
        public event Action<CraftingAction[]> SolutionFound = delegate { };

        public bool CopyBestRotationToPopulations { get; set; }

        public Solver(CraftingSim sim, ushort[] availableActions, int taskCount)
        {
            Sim = sim;
            AvailableActions = availableActions;
            TaskCount = taskCount;
            Tasks = new Task[TaskCount];

            CopyBestRotationToPopulations = false;

            LeaveStartingActions = false;

            Populations = new Population[TaskCount];
            for (int i = 0; i < TaskCount; i++)
                Populations[i] = new Population(i, Sim.Clone(), 150, CraftingSim.MaxActions, AvailableActions);
        }

        public void Start(bool leaveStartingActions = false)
        {
            Continue = true;
            NeedsUpdate = false;
            Iterations = 0;
            LeaveStartingActions = leaveStartingActions;
            BestChromosome = new Chromosome(Sim.Clone(), AvailableActions,CraftingSim.MaxActions, Sim.GetCraftingActions().Select(x => (ushort)x.Id).ToArray());

            for (int i = 0; i < Populations.Length; i++)
                Populations[i].PendingBest = BestChromosome;

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
            Populations[i].Reevaluate(Sim, LeaveStartingActions);
            while (Continue)
            {
                Populations[i].RunOnce();
                Iterations++;
                GenerationRan(Populations[i]);
                
                var best = Populations[i].Best;
                if (BestChromosome.Fitness < best.Fitness && !NeedsUpdate)
                {
                    BestChromosome = best.Clone();
                    BestIndex = i;
                    NeedsUpdate = true;
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
                    Sim.AddActions(BestChromosome.Values.Where(y => y > 0).Select(x => CraftingAction.CraftingActions[x]));
                    NeedsUpdate = false;
                   
                    if (CopyBestRotationToPopulations)
                    for (int i = 0; i < Populations.Length; i++)
                        Populations[i].PendingBest = BestChromosome;

                    G.AddRotationFromSim(Sim);
                }
            }
        }
    }
}
