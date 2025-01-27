using System;
using UnityEngine;

namespace Src.Math
{
    public class CirclesArt : SimulationBase
    {
        [Serializable]
        public struct Circle
        {
            [Tooltip("Angle with respect to the previous circle arrow. For the very first circle this angle is with respect to the x-axis.")]
            public float initialArrowAngle;
            public float angularVelocity;
            public float radius;
        }
        
        [SerializeField] private Circle[] circles;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float lineWidth;
        [SerializeField] private Color lineColor;
        
        private Circle[] _initialCircles;

        //The array of line dots for each state of the simulation.
        //Each state of the simulation has different dots count.
        //It makes algorithm more memory-consuming, but it will take less time to run SetSimulationState method.
        private Vector3[][] _lineDots;

        protected override void Start()
        {
            base.Start();
            _initialCircles = new Circle[circles.Length];
            Array.Copy(circles, _initialCircles, circles.Length);
            _lineDots = new Vector3[samplesCount][];
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            
            SetSimulationState(0);
        }

        private void CalculateStates()
        {
            for (int i = 0; i < samplesCount; i++)
            {
                _lineDots[i] = new Vector3[i+1];
                if (i > 0)
                {
                    Array.Copy(_lineDots[i-1], _lineDots[i], i);
                }
                var circlesState = CalculateCirclesState(i * TimeStep);
                
            }
        }

        /// <summary>
        /// Calculates the position of the last circle`s arrow edge.
        /// </summary>
        /// <param name="circles"></param>
        /// <returns></returns>
        private Vector3 CalculateEdgePosition(Circle[] circles)
        {
            
        }

        private Circle[] CalculateCirclesState(float time)
        {
            var states = new Circle[_initialCircles.Length];
            for (int i = 0; i < _initialCircles.Length; i++)
            {
                var circle = _initialCircles[i];
                circle.initialArrowAngle += circle.angularVelocity * time;
                states[i] = circle;
            }

            return states;
        }

        private Vector3 GetPosition(float r, float theta)
        {
            return new Vector3(r * Mathf.Cos(theta), r * Mathf.Sin(theta), 0);
        }
        
        
        protected override void SetSimulationState(int stateIndex)
        {
            var dots = _lineDots[stateIndex];
            lineRenderer.positionCount = dots.Length;
            lineRenderer.SetPositions(dots);
        }
    }
}
