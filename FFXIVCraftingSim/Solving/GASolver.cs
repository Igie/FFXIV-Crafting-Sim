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
    public class GASolver
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

        public GASolver(CraftingSim sim)
        {
            Sim = sim;
           

            CopyBestRotationToPopulations = false;
            LeaveStartingActions = false;

           
        }


        public void Start(int taskCount = 8, int chromosomeCount = 550, bool leaveStartingActions = false)
        {
            AvailableActions = CraftingAction.CraftingActions.Values.Where(x => x.Level <= Sim.Level).Select(y => y.Id).ToArray();
            if (Populations == null)
            {
                Populations = new Population[taskCount];
                for (int i = 0; i < taskCount; i++)
                    Populations[i] = new Population(i, Sim.Clone(), chromosomeCount, CraftingSim.MaxActions, AvailableActions);
            }
            

            Continue = true;
            NeedsUpdate = false;
            Iterations = 0;
            G.CraftingStates.Clear();
            TaskCount = taskCount;

            Tasks = new Task[TaskCount];

            if (Populations == null)
            {
                Populations = new Population[TaskCount];
                for (int i = 0; i < TaskCount; i++)
                    Populations[i] = new Population(i, Sim.Clone(), chromosomeCount, CraftingSim.MaxActions, AvailableActions);
            }


            if (Populations.Length != TaskCount)
            {
                Population[] newPopulations = new Population[TaskCount];
                Array.Copy(Populations, newPopulations, Math.Min(TaskCount, Populations.Length));
                Populations = newPopulations;
            }

            for (int i = 0; i < TaskCount; i++)
            {
                if (Populations[i] == null)
                {
                    Populations[i] = new Population(i, Sim.Clone(), chromosomeCount, CraftingSim.MaxActions, AvailableActions);
                }
                else if (Populations[i].Chromosomes.Length != chromosomeCount)
                {
                    Populations[i].ChangeSize(chromosomeCount);
                }
            }

            for (int i = 0; i < TaskCount; i++)
                Populations[i].ChangeAvailableValues(AvailableActions);

            LeaveStartingActions = leaveStartingActions;
            BestChromosome = new Chromosome(Sim.Clone(), AvailableActions,CraftingSim.MaxActions, Sim.GetCraftingActions().Select(x => (ushort)x.Id).ToArray());

            for (int i = 0; i < Populations.Length; i++)
                Populations[i].PendingBest = BestChromosome.Clone();

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
                    Sim.AddActions(true, BestChromosome.Values.Where(y => y > 0).Select(x => CraftingAction.CraftingActions[x]));
                    NeedsUpdate = false;
                   
                    if (CopyBestRotationToPopulations)
                    for (int i = 0; i < Populations.Length; i++)
                        Populations[i].PendingBest = BestChromosome.Clone();

                    G.AddRotationFromSim(Sim);
                }
            }
        }
    }
}
