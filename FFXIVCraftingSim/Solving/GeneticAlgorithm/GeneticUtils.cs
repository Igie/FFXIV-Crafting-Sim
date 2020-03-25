using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Solving.GeneticAlgorithm
{
    public static class GeneticUtils
    {
        private static Object lockObj;
        private static Random Random { get; set; }

        static GeneticUtils()
        {
            lockObj = new object();
            Random = new Random();
        }

        public static int GetRandom(int min, int max)
        {
            lock (lockObj)
                return Random.Next(min, max);
        }

        public static int GetRandom(int max)
        {
            lock (lockObj)
                return Random.Next(max);
        }

        public static T GetRandom<T>(this T[] array)
        {
            lock (lockObj)
                return array[GetRandom(array.Length)];
        }

        public static bool GetChance(double probability)
        {
            lock (lockObj)
                return Random.NextDouble() < probability;
        }
    }
}
