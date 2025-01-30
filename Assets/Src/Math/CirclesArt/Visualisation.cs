using System;
using System.Collections.Generic;
using Src.VisualisationTools;
using UnityEngine;

namespace Src.Math.CirclesArt
{
    public class Visualisation : SimulationBase
    {
        [SerializeField] private Circle[] circles;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float lineWidth;
        [SerializeField] private Color lineColor;
        [SerializeField] private ArrowsChain arrowsChain;
        [SerializeField] private Plotter2D plotter2D;
        [SerializeField] private List<Transform> dotsDrawing;
        
        
        private Circle[][] _circlesStates;

        //The array of dots that form the line drawn by circles.
        private Vector3[] _lineDots;
        //Array of circular coordinates of all dots of the line.
        //X is the angle, Y is the radius.
        private Vector2[] _allLineDotsCircular;

        protected override void Start()
        {
            base.Start();
            _circlesStates = new Circle[samplesCount][];
            _allLineDotsCircular = new Vector2[samplesCount];
            _lineDots = new Vector3[samplesCount];
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            
            arrowsChain.SetArrowsCount(circles.Length);
            circles[1].AngularVelocityFunc = (input) => Mathf.Cos(input*input)* (-3);
            
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
                var circular = StatesCalculator.CartesianToCircular(dot.localPosition);
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
            (_lineDots, _allLineDotsCircular) =
                StatesCalculator.CalculateAllEdgePositions(circles, TimeStep, samplesCount);
        }
        
        private void CalculateCirclesStates()
        {
            _circlesStates = StatesCalculator.CalculateCircleStates(circles, TimeStep, samplesCount);
        }

        
        protected override void SetSimulationState(int stateIndex)
        {
            var dotsToDraw = new Vector3[stateIndex + 1];
            Array.Copy(_lineDots, 0, dotsToDraw, 0, stateIndex+1);
            lineRenderer.positionCount = dotsToDraw.Length;
            lineRenderer.SetPositions(dotsToDraw);
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
