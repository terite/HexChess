using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public static class Extensions 
    {
        public static float Sqr(this int value) => value * value;
        public static float Sqr(this float value) => value * value;
        public static float Sqrt(this float value) => Mathf.Sqrt(value);
        public static int Floor(this float val) => Mathf.FloorToInt(val);

        public static void ForEach<T>(this IEnumerable<T> source, System.Action<T> action)
        {
            foreach(T item in source)
                action(item);
        }

        public static HexNeighborDirection OppositeDirection(this HexNeighborDirection direction) =>
            (HexNeighborDirection)(((int)direction + 3) % 6);
    }
}