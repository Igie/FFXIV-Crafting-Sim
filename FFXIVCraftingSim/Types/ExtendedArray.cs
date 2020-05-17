using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVCraftingSim.Types
{
    public class ExtendedArray<T> : IEquatable<ExtendedArray<T>>
    {
        public T[] Array { get; set; }

        public ExtendedArray(T[] array)
        {
            Array = array;
        }

        public ExtendedArray(int length)
        {
            Array = new T[length];
        }

        public bool Equals(ExtendedArray<T> other)
        {
            if (other is null)
                return false;
            if (Array.Length != other.Array.Length)
                return false;
            for (int i = 0; i < Array.Length; i++)
                if (!(Array[i].Equals(other.Array[i])))
                    return false;
            return true;
        }

        public override int GetHashCode()
        {
            int result = 13;
            for(int i = 0; i < Array.Length; i++)
            {
                result *= 29;
                result ^= Array[i].GetHashCode();
            }
            return result;
        }

        public static implicit operator ExtendedArray<T>(T[] array)
        {
            return new ExtendedArray<T>(array);
        }

        public static implicit operator T[](ExtendedArray<T> array)
        {
            return array.Array;
        }
    }
}
