//Fisher-Yates shuffle by https://stackoverflow.com/a/1262619

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ZekstersLab.Helpers
{
    /// <summary>
    /// Generates a new random number on a static thread
    /// </summary>
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        /// <summary>
        /// Generates a new random number on a static thread
        /// </summary>
        public static Random ThisThreadsRandom
        {
            get { return Local ??= new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)); }
        }
    }

    /// <summary>
    /// Class to provide extensions to existing types
    /// </summary>
    static class MyExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                // use tuple to swap values efficiently
                (list[n], list[k]) = (list[k], list[n]);
            }
        }
    }
}
