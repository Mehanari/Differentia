using System;
using Src.VisualisationTools.Plotting;
using UnityEngine;

namespace Src.Math.RootsFinding
{
    public class TwoVariablesFunctionExample : MonoBehaviour
    {
        [SerializeField] private Plotter3D plotter;
        [SerializeField] private GameObject dotPrefab;
        //Next three parameters are applied to both x and y values. Just for simplicity.
        [SerializeField] private int samplesCount;
        [SerializeField] private float from;
        [SerializeField] private float to;

        private float Step => (to - from) / samplesCount;

        private void Start()
        {
            //We are trying to minimize this
            Func<float, float, float> f = (float x, float y) =>
            {
                return 0.5f*x * x + 0.3f*x * y + 0.8f*y * y - 2*x - 3*y + 2f;
            };
            
            Func<float, float, float> dfdx = (float x, float y) =>
            {
                return x + 0.3f * y - 2;
            };
            Func<float, float, float> dfdy = (float x, float y) =>
            {
                return 0.3f * x + 1.6f * y - 3;
            };
            Func<float, float, Vector> differentials = (float x, float y) =>
            {
                var vector = new Vector(2);
                vector[0] = dfdx(x, y);
                vector[1] = dfdy(x, y);
                return vector;
            };
            Func<float, float, SquareMatrix> jacobianExact = (float x, float y) =>
            {
                var matrix = new SquareMatrix(2);
                matrix[0, 0] = 1; //(dfdx)/dx
                matrix[0, 1] = 0.3f; //(dfdx)/dy
                matrix[1, 0] = 0.3f; //(dfdy)/dx
                matrix[1, 1] = 1.6f; //(dfdy)/dy 
                return matrix;
            };
            Func<float, float, SquareMatrix> jacobianApprox = (float x, float y) =>
            {
                var diff = 0.0000001f;
                var matrix = new SquareMatrix(2);
                matrix[0, 0] = (dfdx(x + diff, y) - dfdx(x, y)) / diff;
                matrix[0, 1] = (dfdx(x, y + diff) - dfdx(x, y)) / diff;
                matrix[1, 0] = (dfdy(x + diff, y) - dfdy(x, y)) / diff;
                matrix[1, 1] = (dfdy(x, y + diff) - dfdy(x, y)) / diff;
                return matrix;
            };

            float x0 = 0f;
            float y0 = 0f;
            float tolerance = 0.000001f;
            while (differentials(x0, y0).Magnitude() > tolerance)
            {
                var vector = new Vector(2);
                vector[0] = x0;
                vector[1] = y0;
                var next = vector - differentials(x0, y0) * jacobianApprox(x0, y0).Inverse();
                x0 = (float) next[0];
                y0 = (float) next[1];
            }

            float zMin = f(x0, y0);

            PlotFunction(f);
            var dot = Instantiate(dotPrefab, new Vector3(x0, zMin, y0), Quaternion.identity);
            dot.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        }

        private void PlotFunction(Func<float, float, float> f)
        {
            var x = new float[samplesCount];
            var y = new float[samplesCount];
            var z = new float[samplesCount, samplesCount];
            for (int i = 0; i < samplesCount; i++)
            {
                x[i] = from + i * Step;
                y[i] = from + i * Step;
            }

            for (int i = 0; i < samplesCount; i++)
            {
                for (int j = 0; j < samplesCount; j++)
                {
                    z[i, j] = f(x[i], y[j]);
                }
            }
            
            plotter.PlotHeat(Step*samplesCount, Step*samplesCount, z);
        }
    }
}