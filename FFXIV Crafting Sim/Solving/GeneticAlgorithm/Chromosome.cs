using FFXIV_Crafting_Sim.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIV_Crafting_Sim.Solving.GeneticAlgorithm
{
    public class Chromosome : IComparable<Chromosome>, IEquatable<Chromosome>
    {
        public CraftingSim Sim { get; set; }
        public Population Population { get; private set; }
        public ushort[] Values { get; private set; }
        private ushort[] PossibleValues { get; set; }
        public double? Fitness { get; set; }

        public Chromosome(CraftingSim sim, Population population, ushort[] possibleValues, ushort[] values)
        {
            Sim = sim;
            Population = population;
            PossibleValues = possibleValues;
            Values = values;
            Fitness = Evaluate();
        }

        public Chromosome(CraftingSim sim, Population population, ushort[] possibleValues, int valueCount)
        {
            Sim = sim;
            Population = population;
            PossibleValues = possibleValues;
            Values = new ushort[valueCount];
            for (int i = 0; i < Values.Length; i++)
            {
                Values[i] = PossibleValues.GetRandom();
            }
            Fitness = Evaluate();
        }

        public double Evaluate()
        {
            Sim.RemoveActions();
            var values = Values.Where(x => x != 0).Select(y => CraftingAction.CraftingActions[y]);
            Sim.AddActions(values);
            return Sim.Score;
        }

        public int CompareTo(Chromosome other)
        {
            if (this == other)
                return 0;
            if (this == null && other == null)
                return 0;
            if (this != null && other == null)
                return 1;
            if (this == null && other != null)
                return -1;

            if (Fitness == null && other.Fitness == null)
                return 0;
            if (this.Fitness != null && other.Fitness == null)
                return 1;
            if (this.Fitness == null && other.Fitness != null)
                return -1;

            if (Fitness > other.Fitness)
                return 1;
            if (Fitness == other.Fitness)
                return 0;
            if (Fitness < other.Fitness)
                return -1;
            return 0;
        }

        public override string ToString()
        {
            return $"Chromosome {Fitness}";
        }

        public override int GetHashCode()
        {
            int result = 29;
            for (int i = 0; i < Values.Length; i++)
            {
                result ^= Values[i];
                result *= 13;
            }
            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            Chromosome other = obj as Chromosome;
            return Equals(other);
        }

        public bool Equals(Chromosome other)
        {
            if (other == null)
                return false;
            if (Values.Length != other.Values.Length) return false;
            for (int i = 0; i < Values.Length; i++)
                if (Values[i] != other.Values[i])
                    return false;
            return true;
        }
    }
}
