using System;
using System.Collections.Generic;
using MehaMath.Math.Components;
using UnityEngine;

namespace MehaMath.VisualisationTools.Plotting
{
    public class Plotter2D : MonoBehaviour
    {
        private const float BREAK_DISTANCE = 20f;
        
        [SerializeField] private SpriteRenderer dotPrefab;
        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private float dotSize = 0.1f;
        
        private readonly List<PlotParameters2D> _plots = new List<PlotParameters2D>();
        
        public void PlotDots(Vector2[] positions, string plotName, Color color, Vector3 shift = default)
        {
            RemovePlotByName(plotName);
            var dotsList = new List<GameObject>();
            foreach (var pos in positions)
            {
                var dot = Instantiate(dotPrefab, transform);
                dot.transform.localPosition = shift + new Vector3(pos.x, pos.y, 0);
                dot.transform.localScale = new Vector3(dotSize, dotSize, dotSize);
                dot.GetComponent<SpriteRenderer>().color = color;
                dotsList.Add(dot.gameObject);
            }
            _plots.Add(new PlotParameters2D
            {
                Name = plotName,
                Dots = dotsList
            });
        }

        public void Plot(float from, float to, Polynomial polynomial, int samplesCount, string plotName, Color color,
            Vector3 shift = default)
        {
            Plot(from, to, (x) => (float)polynomial.Compute(x), samplesCount, plotName, color, shift);
        }

        public void Plot(float from, float to, Func<float, float> f, int samplesCount, string plotName, Color color,
            Vector3 shift = default)
        {
            var fValues = new float[samplesCount];
            var step = (to - from) / samplesCount;
            for (int i = 0; i < samplesCount; i++)
            {
                fValues[i] = f(from + step * i);
            }
            Plot(from, to, fValues, plotName, color, shift);
        }
        
        public void Plot(float from, float to, float[] y, string plotName, Color color, Vector3 shift = default)
        {
            var x = new float[y.Length];
            var step = (to - from) / y.Length;
            for (int i = 0; i < y.Length; i++)
            {
                x[i] = from + i * step;
            }
            Plot(x, y, plotName, color, shift);
        }

        public void PlotSingleDot(float x, float y, string plotName, Color color, Vector3 shift = default)
        {
            PlotDots(new []{new Vector2(x, y)}, plotName, color, shift);
        }

        //Increase iteration step parameter to reduce the number of points to draw.
        public void PlotLogarithmic(float from, float to, float[] y, string plotName, Color color,
            Vector3 shift = default, int iterationStep = 1)
        {
            var x = new float[y.Length/iterationStep];
            var step = (to - from) / y.Length;
            for (int i = 0; i < y.Length; i += iterationStep)
            {
                x[i/iterationStep] = Mathf.Log10(10 + from + i * step);
            }
            Plot(x, y, plotName, color, shift);
        }
        
        public void Plot(float from, float to, int[] y, string plotName, Color color, Vector3 shift = default)
        {
            var x = new float[y.Length];
            var step = (to - from) / y.Length;
            var yFloat = new float[y.Length];
            for (int i = 0; i < y.Length; i++)
            {
                yFloat[i] = y[i];
                x[i] = from + i * step;
            }
            Plot(x, yFloat, plotName, color, shift);
        }
        
        public void Plot(float xStep, float[] y, string plotName, Color color, Vector3 shift = default)
        {
            var x = new float[y.Length];
            for (int i = 0; i < y.Length; i++)
            {
                x[i] = i * xStep;
            }
            Plot(x, y, plotName, color, shift);
        }
        
        public void Plot(float[] x, float[] y, string plotName, Color color, Vector3 shift = default)
        {
            RemovePlotByName(plotName);
            var continuousSegments = GetContinuousSegments(x, y);
            var lines = new List<LineRenderer>();
            foreach (var segment in continuousSegments)
            {
                var line = CreateLine(lineWidth, color, plotName + "_" + lines.Count);
                line.positionCount = segment.Length;
                for (int i = 0; i < segment.Length; i++)
                {
                    line.SetPosition(i, shift + new Vector3(segment[i].x, segment[i].y, 0));
                }
                lines.Add(line);
            }
            _plots.Add(new PlotParameters2D
            {
                Name = plotName,
                X = x,
                Y = y,
                Color = color,
                Lines = lines,
            });
        }

        private Vector2[][] GetContinuousSegments(float[] x, float[] y)
        {
            var segments = new List<Vector2[]>();
            var currentSegment = new List<Vector2>();
            for (int i = 0; i < x.Length; i++)
            {
                if (i == 0 || Vector2.Distance(new Vector2(x[i], y[i]), new Vector2(x[i - 1], y[i - 1])) < BREAK_DISTANCE)
                {
                    currentSegment.Add(new Vector2(x[i], y[i]));
                }
                else
                {
                    if (currentSegment.Count > 0)
                    {
                        segments.Add(currentSegment.ToArray());
                        currentSegment.Clear();
                    }
                    currentSegment.Add(new Vector2(x[i], y[i]));
                }
            }
            if (currentSegment.Count > 0)
            {
                segments.Add(currentSegment.ToArray());
            }
            return segments.ToArray();
        }
        
        private static LineRenderer CreateLine(float width, Color color, string plotName)
        {
            var line = new GameObject(plotName).AddComponent<LineRenderer>();
            line.startColor = color;
            line.endColor = color;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.transform.localPosition = Vector3.zero;
            line.startWidth = width;
            line.endWidth = width;
            return line;
        }

        public void RemovePlotByName(string plotName)
        {
            var plot = _plots.Find(p => p.Name == plotName);
            if (plot != null)
            {
                _plots.Remove(plot);
                if (plot.Dots != null)
                {
                    foreach (var dot in plot.Dots)
                    {
                        Destroy(dot);
                    }
                }
                foreach (var line in plot.Lines)
                {
                    Destroy(line.gameObject);
                }
                plot.Lines.Clear();
            }
        }
    }
}
