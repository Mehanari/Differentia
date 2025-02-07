using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Src
{
    public class Plotter2D : MonoBehaviour
    {
        private class PlotParameters
        {
            public string Name { get; set; }
            public float[] X { get; set; }
            public float[] Y { get; set; }
            public Color Color { get; set; }
            [CanBeNull] public LineRenderer Line { get; set; }
            [CanBeNull] public List<GameObject> Dots { get; set; }
        }

        [SerializeField] private GameObject dotPrefab;
        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private float dotSize = 0.1f;
        
        private List<PlotParameters> _plots = new List<PlotParameters>();
        
        public void PlotDots(Vector2[] dots, string plotName, Color color, Vector3 shift = default)
        {
            var dotsList = new List<GameObject>();
            foreach (var dot in dots)
            {
                var dotGo = Instantiate(dotPrefab, transform);
                dotGo.transform.localPosition = shift + new Vector3(dot.x, dot.y, 0);
                dotGo.transform.localScale = new Vector3(dotSize, dotSize, dotSize);
                dotGo.GetComponent<SpriteRenderer>().color = color;
                dotsList.Add(dotGo);
            }
            _plots.Add(new PlotParameters
            {
                Name = plotName,
                Dots = dotsList
            });
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
            var line = new GameObject(plotName).AddComponent<LineRenderer>();
            line.transform.SetParent(transform);
            line.transform.localPosition = Vector3.zero;
            line.positionCount = x.Length;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.startColor = color;
            line.endColor = color;
            line.material = new Material(Shader.Find("Sprites/Default"));
            for (int i = 0; i < x.Length; i++)
            {
                line.SetPosition(i, shift + new Vector3(x[i], y[i], 0));
            }
            _plots.Add(new PlotParameters
            {
                Name = plotName,
                X = x,
                Y = y,
                Color = color,
                Line = line
            });
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
                if (plot.Line != null)
                {
                    Destroy(plot.Line.gameObject);
                }
            }
        }
    }
}
