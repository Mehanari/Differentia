using System;
using System.Collections.Generic;
using System.Linq;
using Src.VisualisationTools;
using Src.VisualisationTools.Plotting;
using UnityEngine;

namespace Src.Math.CirclesArt
{
    public class Visualisation : SimulationBase
    {
        [SerializeField] private GameObject dotPrefab;
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

            var algorithm = new AttentionDistributionGA();
            var keyPoints = GenerateKeyPoints();
            var criticalPointsIndices = keyPoints.Where(k => dotsDrawing.Exists(d => d.position == k.point))
                .Select(k => k.index).ToHashSet();
            //algorithm.CriticalPointsIndices = criticalPointsIndices;
            //ShowKeyPoints(keyPoints);
            circles = algorithm.Fit(circles, TimeStep, samplesCount, keyPoints);
            plotter2D.Plot(0f, 10f, algorithm.ImprovementIntervals.ToArray(), "Improvement",  Color.yellow, new Vector3(0, 10, 0));
            Debug.Log("Total improvements: " + algorithm.ImprovementIndices.Count);
            _circlesStates = new Circle[samplesCount][];
            _allLineDotsCircular = new Vector2[samplesCount];
            _lineDots = new Vector3[samplesCount];
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

        private (int index, Vector3 point)[] GenerateKeyPoints()
        {
            (int index, Vector3 point)[] keyPoints = new(int index, Vector3 point)[samplesCount];
            HashSet<int> indices = new();
            
            var lastIndex = 0;
            foreach (var dot in dotsDrawing)
            {
                var pos = dot.position;
                var angle = GetXAxisAngle(pos);
                var fraction = angle / (2 * Mathf.PI);
                var index = (int) (samplesCount * fraction);
                while (indices.Contains(index))
                {
                    index++;
                }
                if (index >= samplesCount)
                {
                    index = samplesCount - 1;
                }
                indices.Add(index);
                keyPoints[index] = (index, pos);
                //Interpolating values between specified key points
                for (int j = lastIndex + 1; j < index; j++)
                {
                    var a = (j - lastIndex) / (float)(index - lastIndex);
                    var from = keyPoints[lastIndex].point;
                    var to = pos;
                    var interpolation = Vector3.Lerp(from, to, a);
                    keyPoints[j] = (j, interpolation);
                    indices.Add(j);
                }
                lastIndex = index;
            }

            //Interpolating last segment
            for (int i = lastIndex+1; i < keyPoints.Length; i++)
            {
                var a = (i - lastIndex) / (float)(keyPoints.Length - lastIndex);
                var from = keyPoints[lastIndex].point;
                var to = keyPoints[0].point;
                var interpolation = Vector3.Lerp(from, to, a);
                keyPoints[i] = (i, interpolation);
                indices.Add(i);
            }

            return keyPoints;
        }

        private void ShowKeyPoints((int index, Vector3 point)[] points)
        {
            foreach (var point in points)
            {
                Instantiate(dotPrefab, point.point, Quaternion.identity);
            }
        }

        private float GetXAxisAngle(Vector3 vector)
        {
            var angle = Mathf.Acos((Vector3.Dot(Vector3.right, vector)) / (vector.magnitude * Vector3.right.magnitude));
            if (vector.y < 0)
            {
                angle = 2 * Mathf.PI - angle;
            }
            return angle;
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
