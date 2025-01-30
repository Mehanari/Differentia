using System;
using UnityEngine;

namespace Src.Math.CirclesArt
{
    public static class StatesCalculator
    {
        public static Circle[][] CalculateCircleStates(Circle[] initialState, float timeStep, int samplesCount)
        {
            var circlesStates = new Circle[samplesCount][];
            for (int i = 0; i < samplesCount; i++)
            {
                circlesStates[i] = new Circle[initialState.Length];
                Array.Copy(initialState, circlesStates[i], initialState.Length);
                for (int j = 0; j < circlesStates[i].Length; j++)
                {
                    var circle = circlesStates[i][j];
                    circle.arrowAngle += circle.angularVelocity * i * timeStep;
                    circlesStates[i][j] = circle;
                }
            }

            return circlesStates;
        }

        public static (Vector3[] cartesian, Vector2[] circular) CalculateAllEdgePositions(Circle[] initialState, float timeStep, int samplesCount)
        {
            Vector3[] cartesianPositions = new Vector3[samplesCount];
            Vector2[] circularPositions = new Vector2[samplesCount];
            var circlesStates = CalculateCircleStates(initialState, timeStep, samplesCount);
            for (int i = 0; i < samplesCount; i++)
            {
                var circlesState = circlesStates[i];
                var position = CalculateEdgePosition(circlesState);
                cartesianPositions[i] = position.cartesian;
                circularPositions[i] = position.circular;
            }

            return (cartesianPositions, circularPositions);
        }

        public static (Vector3 cartesian, Vector2 circular) CalculateEdgePosition(Circle[] circles)
        {
            Vector3 cartesian = Vector3.zero;
            var theta = 0f;
            for (int i = 0; i < circles.Length; i++)
            {
                var circle = circles[i];
                theta += circle.arrowAngle;
                if (i > 0)
                {
                    theta -= Mathf.PI;
                }
                var cartesianPosition = CircularToCartesian(circle.radius, theta);
                cartesian += cartesianPosition;
            }

            return (cartesian, CartesianToCircular(cartesian));
        }
        
        public static Vector3 CircularToCartesian(float r, float theta)
        {
            return new Vector3(r * Mathf.Cos(theta), r * Mathf.Sin(theta), 0);
        }
        
        public static Vector2 CartesianToCircular(Vector3 cartesian)
        {
            return new Vector2(Mathf.Atan2(cartesian.y, cartesian.x), cartesian.magnitude);
        }
    }
}