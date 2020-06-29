using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSimLib.Solving
{
    public class ArrayCounter<T>
    {
        public T[][] Numbers { get; private set; }

        public T[] Current { get; private set; }
        private int[] CurrentIndexes { get; set; }

        private int NumbersLength { get; set; }
        public ArrayCounter(T[][] numbers)
        {
            NumbersLength = numbers.Length;
            Numbers = numbers;
            Current = new T[NumbersLength];
            CurrentIndexes = new int[NumbersLength];
            for (int i = 0; i < NumbersLength; i++)
            {
                Current[i] = Numbers[i][0];
                CurrentIndexes[i] = 0;
            }
        }

        public bool Increase()
        {
            return IncreaseAtIndex(0);
        }

        private bool IncreaseAtIndex(int index)
        {
            bool result = true;
            if (index == NumbersLength - 1 && CurrentIndexes[index] == Numbers[index].Length - 1)
                return false;
            CurrentIndexes[index]++;
            if (CurrentIndexes[index] >= Numbers[index].Length)
            {
                CurrentIndexes[index] = 0;
                result = IncreaseAtIndex(index + 1);
            }
            Current[index] = Numbers[index][CurrentIndexes[index]];
            return result;
        }
    }
}
