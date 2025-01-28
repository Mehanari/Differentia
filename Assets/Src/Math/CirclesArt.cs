using System;
using System.Collections.Generic;
using Src.VisualisationTools;
using UnityEngine;

namespace Src.Math
{
    public class CirclesArt : SimulationBase
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
        
        [SerializeField] private Circle[] circles;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float lineWidth;
        [SerializeField] private Color lineColor;
        [SerializeField] private ArrowsChain arrowsChain;
        [SerializeField] private Plotter2D plotter2D;
        [SerializeField] private List<Transform> dotsDrawing;
        
        
        private Circle[][] _circlesStates;

        //The array of line dots for each state of the simulation used to draw the line.
        //Each next state of the simulation has all the dots of the previous state and one new.
        //It makes algorithm more memory-consuming, but it will take less time to run SetSimulationState method.
        //TODO: Check if memory consumption is worth the time saved.
        private Vector3[][] _lineDots;
        //Array of circular coordinates of all dots of the line.
        //X is the angle, Y is the radius.
        private Vector2[] _allLineDotsCircular;

        protected override void Start()
        {
            base.Start();
            _circlesStates = new Circle[samplesCount][];
            _allLineDotsCircular = new Vector2[samplesCount];
            _lineDots = new Vector3[samplesCount][];
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            
            arrowsChain.SetArrowsCount(circles.Length);
            
            CalculateCirclesStates();
            CalculateDotsPositions();
            SetSimulationState(0);

            DrawPlot();
        }

        private void DrawPlot()
        {
            plotter2D.RemovePlotByName("Radius over time");
            plotter2D.RemovePlotByName("Dots");
            var drawingDots = new Vector2[dotsDrawing.Count];
            foreach (var dot in dotsDrawing)
            {
                var circular = CartesianToCircular(dot.localPosition);
                if (circular.x < 0)
                {
                    circular.x += 2 * Mathf.PI;
                }
                drawingDots[dotsDrawing.IndexOf(dot)] = circular;
            }
            plotter2D.PlotDots(drawingDots, "Dots", Color.green);
            
            //Plotting the radius dynamics.
            var radii = new float[samplesCount];
            for (int i = 0; i < samplesCount; i++)
            {
                radii[i] = _allLineDotsCircular[i].y;
            }
            plotter2D.Plot(TimeStep, radii, "Radius over time", Color.red);
        }

        private void OnValidate()
        {
            if (_circlesStates == null || _circlesStates.Length != samplesCount)
            {
                return;
            }
            CalculateCirclesStates();
            CalculateDotsPositions();
            
            
            DrawPlot();
            SetSimulationState(0);
            SetUpSliderInterval();

        }

        private void CalculateDotsPositions()
        {
            for (int i = 0; i < samplesCount; i++)
            {
                _lineDots[i] = new Vector3[i+1];
                if (i > 0)
                {
                    Array.Copy(_lineDots[i-1], _lineDots[i], i);
                }
                var circlesState = _circlesStates[i];
                var (cartesian, circular) = CalculateEdgePosition(circlesState);
                _lineDots[i][i] = cartesian;
                _allLineDotsCircular[i] = circular;
            }
        }

        /// <summary>
        /// Calculates the position of the last circle`s arrow edge.
        /// </summary>
        /// <param name="circles"></param>
        /// <returns></returns>
        private (Vector3 cartesian, Vector2 circular) CalculateEdgePosition(Circle[] circles)
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

        private void CalculateCirclesStates()
        {
            for (int i = 0; i < samplesCount; i++)
            {
                _circlesStates[i] = new Circle[circles.Length];
                Array.Copy(circles, _circlesStates[i], circles.Length);
                for (int j = 0; j < _circlesStates[i].Length; j++)
                {
                    var circle = _circlesStates[i][j];
                    circle.arrowAngle += circle.angularVelocity * i * TimeStep;
                    _circlesStates[i][j] = circle;
                }
            }
        }

        private Vector3 CircularToCartesian(float r, float theta)
        {
            return new Vector3(r * Mathf.Cos(theta), r * Mathf.Sin(theta), 0);
        }
        
        private Vector2 CartesianToCircular(Vector3 cartesian)
        {
            return new Vector2(Mathf.Atan2(cartesian.y, cartesian.x), cartesian.magnitude);
        }
        
        
        protected override void SetSimulationState(int stateIndex)
        {
            var dots = _lineDots[stateIndex];
            lineRenderer.positionCount = dots.Length;
            lineRenderer.SetPositions(dots);
            var circlesState = _circlesStates[stateIndex];
            arrowsChain.SetArrowsCount(circlesState.Length);
            for (int i = 0; i < circlesState.Length; i++)
            {
                var circle = circlesState[i];
                arrowsChain.SetAngle(i, circle.arrowAngle);
                arrowsChain.SetLength(i, circle.radius);
            }
        }
    }
}
