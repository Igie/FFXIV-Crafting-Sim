﻿using FFXIV_Crafting_Sim.Actions;
using FFXIV_Crafting_Sim.Solving.GeneticAlgorithm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV_Crafting_Sim.Solving
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

        public bool Continue { get; set; }
        private bool NeedsUpdate { get; set; }
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
                try
                {
                    Populations[i].RunOnce();
                }
                catch (Exception e)
                {
                    Debugger.Break();
                }
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
