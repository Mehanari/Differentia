using System;
using Src.VisualisationTools;
using UnityEngine;

namespace Src
{
    public class VectorFieldExample : MonoBehaviour
    {
        [SerializeField] private VectorField2D vectorField2D;
        
        private void Start()
        {
            Func<Vector2, Vector2> nextPendulumStateDiff = (previousState) =>
            {
                var previousAngle = previousState.x;
                var previousAngularVelocity = previousState.y;
                var g = 1f;
                var length = 1f;
                var airResistance = 0f;
                var pendulumAcceleration = - airResistance*previousAngularVelocity - g / length * Mathf.Sin(previousAngle);
                return new Vector2(previousAngularVelocity, pendulumAcceleration);
            };

            vectorField2D.Plot(nextPendulumStateDiff);
        }
        
    }
}