using System;
using System.Collections.Generic;

namespace Implementation
{
    public static class Utils
    {
        public static void Shuffle<T>(IList<T> array)
        {
            Random _rng = new Random();
            for (int n = array.Count; n > 1;)
            {
                int k = _rng.Next(n);
                --n;
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
    }
}
