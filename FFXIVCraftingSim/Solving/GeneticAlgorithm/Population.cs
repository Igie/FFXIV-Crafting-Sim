using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Solving.GeneticAlgorithm
{
    public class Population
    {
        private double CrossoverRate = 0.8;
        private double MutationRate = 0.1;

        private CraftingSim Sim { get; set; }

        
        public int MaxSize { get; private set; }
        public int DefaultChromosomeSize { get; private set; }
        public int ChromosomeSize { get; private set; }
        public ushort[] PossibleValues { get; private set; }

        public Chromosome[] Chromosomes { get; private set; }

        public int CurrentGeneration { get; private set; }

        public int NonProgressiveGenerations { get; private set; }
        public double NonProgressiveScore { get; private set; }

        public Chromosome Best
        {
            get
            {
                return Chromosomes[0];
            }
        }

        public Population(CraftingSim sim, int maxSize, int chromosomeSize, ushort[] possibleValues, ushort[] initialValues = null)
        {
            Sim = sim;
            MaxSize = maxSize;
            Chromosomes = new Chromosome[MaxSize];
            DefaultChromosomeSize = chromosomeSize;
            ChromosomeSize = chromosomeSize;
            PossibleValues = possibleValues;

            for (int i = 0; i < MaxSize; i++)
            {
                Chromosomes[i] = new Chromosome(sim, this, possibleValues, chromosomeSize);
            }

            if (initialValues != null)
            {
                    initialValues.CopyTo(Chromosomes[0].Values, 0);
                
            }

            Array.Sort(Chromosomes);
            CurrentGeneration = 0;
        }

        public void RunOnce()
        {

            for (int i = 0; i < MaxSize; i += 4)
            {
                if (GeneticUtils.GetChance(CrossoverRate))
                {
                    int i1 = i / 2;
                    int i2 = i / 2 + 1;
                    Crossover(Chromosomes[i1], Chromosomes[i2], i / 4);
                }
            }

            for (int i = 0; i < MaxSize; i++)
            {
                if (GeneticUtils.GetChance(MutationRate))
                {
                    Mutation(Chromosomes[i]);
                }
            }

            Array.Sort(Chromosomes);

            CurrentGeneration++;

            if (Best.Fitness <= NonProgressiveScore)
                NonProgressiveGenerations++;
            else
            {
                NonProgressiveScore = Best.Fitness.Value;
                NonProgressiveGenerations = 0;
            }


            if (NonProgressiveGenerations >= 600)
            {
                for (int i = MaxSize / 10; i < MaxSize; i++)
                {
                    Chromosomes[i] = new Chromosome(Sim, this, PossibleValues, ChromosomeSize);
                }

                Array.Sort(Chromosomes);

                NonProgressiveGenerations = 0;
                NonProgressiveScore = Best.Fitness.Value;
            }
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

            Chromosome firstNew = new Chromosome(Sim, this, PossibleValues, firstArray);
            Chromosome secondNew =new Chromosome(Sim, this, PossibleValues, secondArray);

            if (firstNew.Fitness > Chromosomes[Chromosomes.Length - 2 - pos].Fitness && !Contains(firstNew))
                Chromosomes[Chromosomes.Length - 2 - pos] = firstNew;

            if (secondNew.Fitness > Chromosomes[Chromosomes.Length - 1 - pos].Fitness && !Contains(secondNew))
                Chromosomes[Chromosomes.Length - 1 - pos] = secondNew;
        }

        public void Mutation(Chromosome chromosome)
        {   if (chromosome == null) return;
            int index = GeneticUtils.GetRandom(ChromosomeSize);
            int end = GeneticUtils.GetRandom(ChromosomeSize - index) + index;

            for (int i = index; i < end; i++)
            {
                chromosome.Values[i] = PossibleValues.GetRandom();
            }

            chromosome.Fitness = chromosome.Evaluate();
        }

        public bool Contains(Chromosome chromosome)
        {
            int length = Chromosomes.Length;
            for (int i = 0; i < length; i++)
                if (Chromosomes[i].Fitness == chromosome.Fitness || Chromosomes[i] == chromosome)
                    return true;
            return false;
        }

        public void Reevaluate(CraftingSim sim)
        {
            sim.CopyTo(Sim);

            ChromosomeSize = DefaultChromosomeSize;


            for (int i = 0; i < ChromosomeSize; i++)
            {
                var a = Chromosomes[i].Values;
                Array.Resize(ref a, ChromosomeSize);
                Chromosomes[i].Values = a;
                Chromosomes[i].Fitness = Chromosomes[i].Evaluate();
            }
        }
    }
}
