using System;
using Src.Math.Components;
using UnityEngine;

namespace Src.Math
{
    public static class Utils
    {
        /// <summary>
        /// Convert a function that is handy for Unity into a function that is handy for math algorithms.
        /// </summary>
        /// <param name="floatFunc"></param>
        /// <returns></returns>
        public static Func<Vector, double> ToDoubleFunc(Func<Vector2, float> floatFunc)
        {
            return (vector) =>
            {
                return floatFunc(vector.ToVector2());
            };
        }
    }
}