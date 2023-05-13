// Fisher-Yates shuffle by https://stackoverflow.com/a/1262619

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace ZekstersLab.Helpers
{
    /// <summary>
    /// Generates a new random number on a static thread
    /// </summary>
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static System.Random s_local;

        /// <summary>
        /// Generates a new random number on a static thread
        /// </summary>
        public static System.Random ThisThreadsRandom
        {
            get { return s_local ??= new System.Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)); }
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
                // Use tuple to swap values efficiently
                (list[n], list[k]) = (list[k], list[n]);
            }
        }

        /// <summary>
        /// Check if two vectors are approximately equal using Mathf.Approximately
        /// </summary>
        /// <param name="vector">The first vector</param>
        /// <param name="other">The second vector</param>
        /// <returns>True if the vectors are approximately equal, false otherwise</returns>
        public static bool Vector2Approximately(this Vector2 vector, Vector2 other)
        {
            return Mathf.Approximately(vector.x, other.x) && Mathf.Approximately(vector.y, other.y);
        }
    }
}