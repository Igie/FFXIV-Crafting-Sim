using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Solving.GeneticAlgorithm
{
    public class Population
    {
        private double CrossoverRate = 0.8;
        private double MutationRate = 0.7;

        public int Index { get; private set; }
        private CraftingSim Sim { get; set; }

        
        public int MaxSize { get; private set; }
        public int DefaultChromosomeSize { get; private set; }
        public int ChromosomeSize { get; private set; }
        public ushort[] PossibleValues { get; private set; }

        public Chromosome[] Chromosomes { get; private set; }

        public int CurrentGeneration { get; private set; }

        public int NonProgressiveGenerations { get; private set; }
        public double NonProgressiveScore { get; private set; }

        public Chromosome Best { get; private set; }

        public Chromosome PendingBest { get; set; }
        
        public bool LeaveInitialValues { get; set; }
        public ushort[] InitialValues { get; set; }

        public event Action<Population> GenerationRan = delegate { };

        public Population(int index, CraftingSim sim, int maxSize, int chromosomeSize, ushort[] possibleValues, ushort[] initialValues = null)
        {
            Index = index;
            Sim = sim;
            MaxSize = maxSize;
            Chromosomes = new Chromosome[MaxSize];
            DefaultChromosomeSize = chromosomeSize;
            ChromosomeSize = chromosomeSize;
            PossibleValues = possibleValues;
            LeaveInitialValues = false;
            for (int i = 0; i < MaxSize; i++)
            {
                Chromosomes[i] = new Chromosome(sim, possibleValues, chromosomeSize);
            }

            if (initialValues != null)
            {
                    initialValues.CopyTo(Chromosomes[0].Values, 0);
                
            }

            Array.Sort(Chromosomes);
            Best = Chromosomes[0];
            for (int i = 1; i < Chromosomes.Length; i++)
            {
                if (Chromosomes[i - 1].Fitness < Chromosomes[i].Fitness)
                    Debugger.Break();
            }

            CurrentGeneration = 0;
        }

        public void RunOnce()
        {
           
            for (int i = 0; i < MaxSize / 1.1; i++)
            {
                if (GeneticUtils.GetChance(CrossoverRate))
                {
                    int i1 = i * 2;
                    //int i2 = i * 2 + 1;
                    int i2 = Chromosomes.Length - 1 - i;
                    Crossover(Chromosomes[0], Chromosomes[i2], i);
                }
            }

            for (int i = 0; i < MaxSize / 1.1; i++)
            {
                if (GeneticUtils.GetChance(CrossoverRate))
                {
                    int i1 = i * 2;
                    //int i2 = i * 2 + 1;
                    int i2 = Chromosomes.Length - 1 - i;
                    Crossover(Chromosomes[1], Chromosomes[i2], i);
                }
            }

            for (int i = 0; i < MaxSize / 1.1; i++)
            {
                if (GeneticUtils.GetChance(CrossoverRate))
                {
                    int i1 = i * 2;
                    //int i2 = i * 2 + 1;
                    int i2 = Chromosomes.Length - 1 - i;
                    Crossover(Chromosomes[i], Chromosomes[i2], i);
                }
            }



            for (int i = 1; i < MaxSize; i++)
            {
                if (GeneticUtils.GetChance(MutationRate))
                {
                    Mutation(Chromosomes[i]);
                }
            }

            Array.Sort(Chromosomes);
            Best = Chromosomes[0];
            try
            {
                if (PendingBest != null)
                {
                    if (LeaveInitialValues)
                    {
                        Array.Copy(InitialValues, PendingBest.Values, InitialValues.Length);
                        PendingBest.Fitness = PendingBest.Evaluate();
                    }

                    if (!Contains(PendingBest) && Chromosomes[Chromosomes.Length - 1].Fitness < PendingBest.Fitness)
                    {
                        Chromosomes[Chromosomes.Length - 1] = PendingBest.Clone();
                        Array.Sort(Chromosomes);
                        Best = Chromosomes[0];
                    }
                    PendingBest = null;
                }
            }
            catch (Exception e) {
                Debugger.Break();
            }

            CurrentGeneration++;

            if (Best.Fitness <= NonProgressiveScore)
                NonProgressiveGenerations++;
            else
            {
                NonProgressiveScore = Best.Fitness;
                NonProgressiveGenerations = 0;
            }


            if (NonProgressiveGenerations >= 500)
            {
                for (int i = MaxSize / 2; i < MaxSize; i++)
                {
                    Chromosomes[i] = Chromosomes[i].Clone();
                    if (LeaveInitialValues)
                    {
                        Array.Copy(InitialValues, Chromosomes[i].Values, InitialValues.Length);
                        Chromosomes[i].Fitness = Chromosomes[i].Evaluate();
                    }
                }
                Array.Sort(Chromosomes);

                Best = Chromosomes[0];
              
                NonProgressiveGenerations = 0;
                NonProgressiveScore = Best.Fitness;
            }

            GenerationRan(this);
        }

        public void Crossover(Chromosome first, Chromosome second, int pos)
        {
            if (first == null || second == null) return;
            int index = GeneticUtils.GetRandom(ChromosomeSize);
            int size = GeneticUtils.GetRandom(ChromosomeSize - index);
            if (size == ChromosomeSize) size = ChromosomeSize / 2;

            ushort[] firstArray = new ushort[ChromosomeSize];
            ushort[] secondArray = new ushort[ChromosomeSize];

            Array.Copy(first.Values, firstArray, ChromosomeSize);
            Array.Copy(second.Values, secondArray, ChromosomeSize);

            Array.Copy(first.Values, index, secondArray, index, size);
            Array.Copy(second.Values, index, firstArray, index, size);

            Chromosome firstNew = new Chromosome(Sim, PossibleValues, firstArray.Length, firstArray);
            Chromosome secondNew = new Chromosome(Sim, PossibleValues, secondArray.Length, secondArray);

            if (LeaveInitialValues)
            {
                Array.Copy(InitialValues, firstNew.Values, InitialValues.Length);
                Array.Copy(InitialValues, secondNew.Values, InitialValues.Length);
                firstNew.Fitness = firstNew.Evaluate();
                secondNew.Fitness = secondNew.Evaluate();
            }

            if (Chromosomes[Chromosomes.Length - 1 - pos].Fitness < firstNew.Fitness && !Contains(firstNew))
                Chromosomes[Chromosomes.Length - 1 - pos] = firstNew;

            if (Chromosomes[Chromosomes.Length - 2 - pos].Fitness < secondNew.Fitness && !Contains(secondNew))
                Chromosomes[Chromosomes.Length - 2 - pos] = secondNew;

        }

        public void Mutation(Chromosome chromosome)
        {   if (chromosome == null) return;
            int index = GeneticUtils.GetRandom(ChromosomeSize);
            int end = GeneticUtils.GetRandom((ChromosomeSize - index) / 2) + index;

            //Chromosome clone = chromosome.Clone();

            for (int i = index; i < end; i++)
            {
                chromosome.Values[i] = PossibleValues.GetRandom();
            }
            if (LeaveInitialValues)
            {
                Array.Copy(InitialValues, chromosome.Values, InitialValues.Length);
            }
            chromosome.Fitness = chromosome.Evaluate();
            //if (chromosome.Fitness < clone.Fitness && !Contains(clone))
            //{
             //   Array.Copy(clone.Values, chromosome.Values, ChromosomeSize);
            //    chromosome.Fitness = chromosome.Evaluate();
            //}
        }

        public void ChangeSize(int newSize)
        {
            MaxSize = newSize;

            Chromosome[] newArray = new Chromosome[MaxSize];
            Array.Copy(Chromosomes, newArray, Math.Min(MaxSize, Chromosomes.Length));
            Chromosomes = newArray;

            for (int i = 0; i < MaxSize; i++)
                if (Chromosomes[i] is null)
                    Chromosomes[i] = new Chromosome(Sim, PossibleValues, ChromosomeSize);
        }

        public bool Contains(Chromosome chromosome)
        {
            int length = Chromosomes.Length;
            for (int i = 0; i < length; i++)
                if (Chromosomes[i] == chromosome)
                    return true;
            return false;
        }

        public void Reevaluate(CraftingSim sim, bool leaveInitialValues)
        {
            LeaveInitialValues = leaveInitialValues;
            sim.CopyTo(Sim);
            InitialValues = sim.GetCraftingActions().Select(x => (ushort)x.Id).ToArray();
            ChromosomeSize = DefaultChromosomeSize;


            for (int i = 0; i < ChromosomeSize; i++)
            {
                if (LeaveInitialValues)
                    Array.Copy(InitialValues, Chromosomes[i].Values, InitialValues.Length);
                sim.CopyTo(Chromosomes[i].Sim);
                Chromosomes[i].Fitness = Chromosomes[i].Evaluate();
            }
        }
    }
}
