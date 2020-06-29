using FFXIVCraftingSimLib.Actions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSimLib.Solving.GeneticAlgorithm
{
    public class Chromosome : IComparable<Chromosome>, IEquatable<Chromosome>
    {
        public CraftingSim Sim { get; set; }
        public Population Population { get; private set; }
        public ushort[] Values { get; set; }
        public ushort[] UsableValues { get; set; }
        public ushort[] AvailableValues { get; set; }
        public double Fitness { get; set; }

        public int Hash { get; private set; }

        public int Size { get; private set; }

        public Chromosome(CraftingSim sim, ushort[] availableValues, int valueCount, ushort[] values)
        {
            Sim = sim.Clone();
            AvailableValues = availableValues;
            Values = new ushort[valueCount];
            values.CopyTo(Values, 0);
            Fitness = Evaluate();
        }

        public Chromosome(CraftingSim sim, ushort[] availableValues, int valueCount)
        {
            Sim = sim.Clone();
            AvailableValues = availableValues;
            Values = new ushort[valueCount];


            for (int i = 0; i < Values.Length; i++)
            {
                Values[i] = AvailableValues.GetRandom();
            }
            Fitness = Evaluate();
        }

        public Chromosome Clone()
        {
            return new Chromosome(Sim, AvailableValues, Values.Length, Values);
        }

        public double Evaluate()
        {
            Sim.RemoveActions();
            UsableValues = Values.Where(x => x > 0).ToArray();
            try
            {
                var values = UsableValues.Select(y => CraftingAction.CraftingActions[y]).ToArray();
                Sim.AddActions(true, values);
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
           
            Size = Sim.CraftingActionsLength;
            UsableValues = UsableValues.Take(Size).ToArray();
            Hash = GetHashCode();
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
            int result = 7;
            for (int i = 0; i < UsableValues.Length; i++)
            {
                result ^= UsableValues[i];
                result *= 29;
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


            if (UsableValues.Length != other.UsableValues.Length)
                return false;

            if (Hash == other.Hash)
                return true;

            if (Sim.CurrentProgress == other.Sim.CurrentProgress &&
                Sim.CurrentQuality == other.Sim.CurrentQuality &&
                Sim.CurrentDurability == other.Sim.CurrentDurability &&
                Sim.CurrentCP == other.Sim.CurrentCP && 
                Sim.CraftingActionsLength == other.Sim.CraftingActionsLength &&
                Sim.Score == other.Sim.Score)
                return true;

            return Hash == other.Hash;
            bool eq = true;
            for (int i = 0; i < UsableValues.Length; i++)
                if (UsableValues[i] != other.UsableValues[i])
                {
                    eq = false;
                    break;
                }

            if (eq && Hash != other.Hash)
                Debugger.Break();
            return true;
        }

        public static bool operator ==(Chromosome left, Chromosome right)
        {
            if (left is null)
                if (right is null)
                    return true;
                else
                    return right.Equals(left);
            return left.Equals(right);
        }

        public static bool operator !=(Chromosome left, Chromosome right)
        {
            if (left is null)
                if (right is null)
                    return false;
                else
                    return !right.Equals(left);

            return !left.Equals(right);
        }
    }
}
