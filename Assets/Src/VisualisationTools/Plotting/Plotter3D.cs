using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Src.VisualisationTools.Plotting
{
    public class Plotter3D : MonoBehaviour
    {  
        /// <summary>
        /// Imposing a necessity of having a mesh renderer on a dot.
        /// </summary>
        [SerializeField] private MeshRenderer dotPrefab;

        private List<PlotParameters> _plots = new();

        public void PlotHeat(Vector2 xBounds, Vector2 yBounds, Func<Vector2, float> zFunction, int xSamplesCount, 
            int ySamplesCount, string plotName, Vector3 offset = default)
        {
            var xStart = xBounds.x;
            var xEnd = xBounds.y;
            var yStart = yBounds.x;
            var yEnd = yBounds.y;
            var xStep = (xEnd - xStart) / xSamplesCount;
            var yStep = (yEnd - yStart) / ySamplesCount;
            offset = offset + new Vector3(xStart, 0f, yStart);
            var z = new float[xSamplesCount, ySamplesCount];
            for (int xIndex = 0; xIndex < xSamplesCount; xIndex++)
            {
                var x = xStart + xIndex * xStep;
                for (int yIndex = 0; yIndex < ySamplesCount; yIndex++)
                {
                    var y = yStart + yIndex * yStep;
                    z[xIndex, yIndex] = zFunction(new Vector2(x, y));
                }
            }
            PlotHeat(xEnd - xStart, yEnd - yStart, z, plotName, offset);
        }
        
        /// <summary>
        /// Points with bigger z are colored as cold and points with lower z are colored as hot.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void PlotHeat(float length, float width, float[,] z, string plotName, Vector3 offset = default)
        {
            DeletePlot(plotName);
            var xDots = z.GetLength(0);
            var yDots = z.GetLength(1);
            var (min, max) = GetMinMax(z);
            var mesh = MeshMaker.GetPlaneMesh(length, width, xDots, yDots, Color.red);
            var meshFilter = new GameObject(plotName).AddComponent<MeshFilter>();
            var meshRenderer = meshFilter.gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
            var vertices = mesh.vertices;
            var colors = mesh.colors;
            for (int x = 0; x < xDots; x++)
            {
                for (int y = 0; y < yDots; y++)
                {
                    var index = x * yDots + y;
                    var point = vertices[index];
                    point.y = z[x, y];
                    vertices[index] = point;
                    var color = ColorUtils.HeatToColor(z[x, y], min, max);
                    colors[index] = color;
                }
            }

            mesh.vertices = vertices;
            mesh.colors = colors;
            meshFilter.mesh = mesh;

            meshFilter.transform.position = transform.position + offset;
            var plotParameters = new MeshPlotParameters()
            {
                MeshFilter = meshFilter,
                Name = plotName
            };
            _plots.Add(plotParameters);
        }

        /// <summary>
        /// Plot a line of a given length that goes through pivot in a given direction.
        /// Pivot is in the middle of the line.
        /// </summary>
        /// <param name="length"></param>
        /// <param name="pivot"></param>
        /// <param name="direction"></param>
        /// <param name="plotName"></param>
        /// <param name="lineColor"></param>
        /// <param name="lineWidth"></param>
        public void PlotStraightLine(float length, Vector3 pivot, Vector3 direction, string plotName, Color lineColor, float lineWidth = 0.1f)
        {
            DeletePlot(plotName);
            direction = direction.normalized;
            var start = pivot - direction * (length / 2);
            var end = pivot + direction * (length / 2);
            var line = new GameObject(plotName).AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startColor = line.endColor = lineColor;
            line.startWidth = line.endWidth = lineWidth;
            line.material = new Material(Shader.Find("Sprites/Default"));
            var plotParameters = new LinePlotParameters()
            {
                Line = line,
                Name = plotName
            };
            _plots.Add(plotParameters);
        }

        /// <summary>
        /// WARNING: the height of the spawned dot is defined by dot Vector3 y component, not z.
        /// </summary>
        /// <param name="dot"></param>
        /// <param name="plotName"></param>
        /// <param name="dotColor"></param>
        /// <param name="dotSize"></param>
        public void PlotSingleDot(Vector3 position, string plotName, Color dotColor, float dotSize = 0.05f)
        {
            DeletePlot(plotName);
            var dot = Instantiate(dotPrefab, position, Quaternion.identity);
            dot.transform.localScale = new Vector3(dotSize, dotSize, dotSize);
            dot.material.color = dotColor;
            var plotParameters = new DotPlotParameters
            {
                Name = plotName,
                Dots = new List<GameObject> { dot.gameObject }
            };
            _plots.Add(plotParameters);
        }

        /// <summary>
        /// The height of each dot is defined by z array.
        /// </summary>
        /// <param name="length"></param>
        /// <param name="width"></param>
        /// <param name="z"></param>
        /// <param name="plotName"></param>
        /// <param name="dotsColor"></param>
        /// <param name="dotSize"></param>
        public void PlotDots(float length, float width, float[,] z, string plotName, Color dotsColor, float dotSize = 0.05f)
        {
            DeletePlot(plotName);
            var xDots = z.GetLength(0);
            var yDots = z.GetLength(1);
            var xStep = length / xDots;
            var yStep = width / yDots;
            var dots = new List<GameObject>();
            for (int x = 0; x < xDots; x++)
            {
                for (int y = 0; y < yDots; y++)
                {
                    var position = new Vector3(x * xStep, z[x, y], y * yStep);
                    var dot = Instantiate(dotPrefab, position, Quaternion.identity);
                    var dotTransform = dot.transform;
                    dotTransform.parent = transform;
                    dotTransform.localScale = new Vector3(dotSize, dotSize, dotSize);
                    dot.material.color = dotsColor;
                    dots.Add(dot.gameObject);
                }
            }

            var plotParameters = new DotPlotParameters()
            {
                Name = plotName,
                Dots = dots
            };
            _plots.Add(plotParameters);
        }

        public void DeletePlot(string plotName)
        {
            var plot = _plots.FirstOrDefault(p => p.Name == plotName);
            if (plot != null)
            {
                plot.Destroy();
                _plots.Remove(plot);
            }
        }

        private (float min, float max) GetMinMax(float[,] values)
        {
            var min = float.MaxValue;
            var max = float.MinValue;

            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    var value = values[i, j];
                    if (value > max)
                    {
                        max = value;
                    }

                    if (value < min)
                    {
                        min = value;
                    }
                }
            }

            return (min, max);
        }
    }
}