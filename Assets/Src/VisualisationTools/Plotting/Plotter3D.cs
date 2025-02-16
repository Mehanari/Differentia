using UnityEngine;

namespace Src.VisualisationTools.Plotting
{
    public class Plotter3D : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;

        /// <summary>
        /// Points with bigger z are colored as cold and points with lower z are colored as hot.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void PlotHeat(float length, float width, float[,] z)
        {
            var xDots = z.GetLength(0);
            var yDots = z.GetLength(1);
            var (min, max) = GetMinMax(z);
            var mesh = MeshMaker.GetPlaneMesh(length, width, xDots, yDots, Color.red);
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