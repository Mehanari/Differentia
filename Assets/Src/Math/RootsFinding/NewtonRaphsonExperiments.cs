using System;
using Src.Math.Components;
using Src.VisualisationTools.Plotting;
using UnityEngine;

namespace Src.Math.RootsFinding
{
    public class NewtonRaphsonExperiments : MonoBehaviour
    {
        [SerializeField] private Vector2 initialGuess;
        [SerializeField] private int samplesCount;
        [SerializeField] private float from;
        [SerializeField] private float to;
        [SerializeField] private Plotter3D plotter;

        private Func<Vector2, float> f1 = (vector) =>
        {
            return vector.x * vector.x + vector.y * vector.y - 1;
        };

        private Func<Vector2, float> f2 = (vector) =>
        {
            return vector.x + vector.y;
        };

        private void Start()
        {
            plotter.PlotHeat(from, to, f1, samplesCount, "F1");
            plotter.PlotHeat(from, to, f2, samplesCount, "F2");
            var objective = new FuncVector(Utils.ToDoubleFunc(f1), Utils.ToDoubleFunc(f2));
            var zero = Algorithms.NewtonRaphson(objective, new Vector(initialGuess)).ToVector2();
            var fZero = f1(zero);
            plotter.PlotSingleDot(new Vector3(zero.x, fZero, zero.y), "Zero", Color.yellow);
        }
    }
}
