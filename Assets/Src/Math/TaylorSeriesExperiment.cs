using System;
using MehaMath.Math.Components;
using MehaMath.Math.RootsFinding;
using MehaMath.VisualisationTools.Plotting;
using Src.Math.RootsFinding;
using UnityEngine;

namespace Src.Math
{
    public class TaylorSeriesExperiment : MonoBehaviour
    {
        [SerializeField] private Plotter2D plotter;
        [SerializeField] private int approxSamplesCount = 100;
        [Header("Approximation boundaries")]
        [SerializeField] private float from = -1;
        [SerializeField] private float to = 1;

        private void Start()
        {
            Func<Vector, double> objective = (coefficients) =>
            {
                Polynomial polynomial = new Polynomial(coefficients);
                var maxSquareError = double.MinValue;
                var squareErrorsSum = 0d;
                var step = (to - from) / approxSamplesCount;
                for (int i = 0; i < approxSamplesCount; i++)
                {
                    var x = from + step * i;
                    var poly = polynomial.Compute(x);
                    var actual = System.Math.Cos(x);
                    var diff = poly - actual;
                    var diffSquare = diff * diff;
                    squareErrorsSum += diffSquare;
                    if (diffSquare > maxSquareError)
                    {
                        maxSquareError = diffSquare;
                    }
                }

                return squareErrorsSum / approxSamplesCount;
            };
            var coefficients = Algorithms.MomentumGradientDescent(objective, new Vector(5), 
                initialVelocity: new Vector(8d, 8d, 8d, 8d, 8d), step: 0.0001d, iterationsLimit: 400000);
            
            Polynomial poly = new Polynomial(coefficients);
            plotter.Plot(from, to, poly.ToFloatFunc(), 1000, "Polynomial", Color.yellow);
            plotter.Plot(from, to, Mathf.Cos, 1000, "Cos", Color.green, shift: new Vector3(0, 0.2f, 0));
            Debug.Log(poly);
            Debug.Log(poly.ComputeIntegral(-1, 1));
        }

        private double CosIntegral(double from, double to)
        {
            return System.Math.Sin(to) - System.Math.Sin(from);
        }
    }
}