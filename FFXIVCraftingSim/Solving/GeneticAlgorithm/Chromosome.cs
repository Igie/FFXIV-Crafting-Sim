using FFXIVCraftingSim.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Solving.GeneticAlgorithm
{
    public class Chromosome : IComparable<Chromosome>, IEquatable<Chromosome>
    {
        public CraftingSim Sim { get; set; }
        public Population Population { get; private set; }
        public ushort[] Values { get; set; }
        public ushort[] UsableValues { get; set; }
        private ushort[] PossibleValues { get; set; }
        public double? Fitness { get; set; }

        public int Size { get; private set; }

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
            UsableValues = Values.Where(x => x != 0).ToArray();
            var values = UsableValues.Select(y => CraftingAction.CraftingActions[y]).ToArray();
            //for (int i = 0; i < values.Length; i++)
            //    Values[i] = (ushort)values[i].Id;
            Sim.AddActions(values);
            Size = Sim.CraftingActionsLength;
            //for (int i = Size; i < Values.Length; i++)
            //    Values[i] = 0;
            return Sim.Score;
        }

        public int CompareTo(Chromosome other)
        {
            if (this == other)
                return 0;
            if (this == null && other == null)
                return 0;
            if (this != null && other == null)
                return -1;
            if (this == null && other != null)
                return 1;

            if (Fitness == null && other.Fitness == null)
                return 0;
            if (this.Fitness != null && other.Fitness == null)
                return -1;
            if (this.Fitness == null && other.Fitness != null)
                return 1;

            if (Fitness > other.Fitness)
                return -1;
            if (Fitness == other.Fitness)
                return 0;
            if (Fitness < other.Fitness)
                return 1;
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
            if (ReferenceEquals(obj, null))
                return false;
            Chromosome other = obj as Chromosome;
            return Equals(other);
        }

        public bool Equals(Chromosome other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (Fitness != other.Fitness)
                return false;
            if (UsableValues != other.UsableValues) return false;

            for (int i = 0; i < UsableValues.Length; i++)
                if (UsableValues[i] != other.UsableValues[i])
                    return false;
            return true;
        }

        public static bool operator ==(Chromosome left, Chromosome right)
        {
            if (left is null && right is null)
                return true;
            return left.Equals(right);
        }

        public static bool operator !=(Chromosome left, Chromosome right)
        {
            return !left.Equals(right);
        }
    }
}
