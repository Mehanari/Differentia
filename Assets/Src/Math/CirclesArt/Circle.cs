using System;
using JetBrains.Annotations;
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

        //A function for getting angular velocity over time (or whatever you want).
        //Needed for the case if you want to experiment with non-constant angular velocities.
        //Returns angularVelocity by default.
        //The StateCalculator class uses angularVelocity field if this func is null, otherwise
        //it will use this func.
        //This system is not very safe and can be broken accidentally, but for the sake of experiments, I let it be.
        [CanBeNull] public Func<float, float> AngularVelocityFunc;
    }
}