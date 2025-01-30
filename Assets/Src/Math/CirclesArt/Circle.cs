using System;
using UnityEngine;

namespace Src.Math.CirclesArt
{
    [Serializable]
    public struct Circle
    {
        [Tooltip("Angle with respect to the previous circle arrow. For the very first circle this angle is with respect to the x-axis.")]
        [Range(0, 2 * Mathf.PI)]
        public float arrowAngle;
        public float angularVelocity;
        public float radius;
    }
}