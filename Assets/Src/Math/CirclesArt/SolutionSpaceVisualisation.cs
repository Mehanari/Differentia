using System;
using Src.VisualisationTools.Plotting;
using UnityEngine;

namespace Src.Math.CirclesArt
{
    /// <summary>
    /// I use this script to visualise the fitness function values on the 2-dimensional solutions space.
    /// </summary>
    public class SolutionSpaceVisualisation : MonoBehaviour
    {
        [Header("Actual solution")]
        [SerializeField] private Circle firstCircle;
        [SerializeField] private Circle secondCircle;

        [Header("Calculation parameters")] 
        [SerializeField] private float radiusChangeStep = 0.1f;
        [SerializeField] private int firstRadiusSamples = 100;
        [SerializeField] private int secondRadiusSamples = 100;
        [SerializeField] private int drawingSamples = 10;
        [SerializeField] private float drawingTimeStep = 0.1f;

        [Header("Visualisation")] 
        [SerializeField] private Plotter3D plotter;

        private void Start()
        {

        }

        private void OnValidate()
        {
            var circlesArray = new Circle[]
            {
                firstCircle,
                secondCircle
            };
            var actualSolution =
                StatesCalculator.CalculateAllEdgePositions(circlesArray, drawingTimeStep, drawingSamples).cartesian;

            var fitnessValues = new float[firstRadiusSamples, secondRadiusSamples];
            for (int i = 0; i < firstRadiusSamples; i++)
            {
                circlesArray[0].radius = i * radiusChangeStep;
                for (int j = 0; j < secondRadiusSamples; j++)
                {
                    circlesArray[1].radius = j * radiusChangeStep;
                    var solution = StatesCalculator.CalculateAllEdgePositions(circlesArray, drawingTimeStep, drawingSamples).cartesian;
                    var fitness = AverageError(solution, actualSolution);
                    fitnessValues[i, j] = fitness;
                }
            }
            plotter.PlotHeat(firstRadiusSamples*radiusChangeStep, secondRadiusSamples*radiusChangeStep, fitnessValues, "Plot");
        }

        private float MaxError(Vector3[] solution, Vector3[] actualSolution)
        {
            var maxError = float.MinValue;
            for (int i = 0; i < solution.Length; i++)
            {
                var error = Vector3.Distance(solution[i], actualSolution[i]);
                if (error > maxError)
                {
                    maxError = error;
                }
            }

            return maxError;
        }

        private float AverageError(Vector3[] solution, Vector3[] actualSolution)
        {
            var errorsSum = 0f;
            for (int i = 0; i < solution.Length; i++)
            {
                errorsSum += Vector3.Distance(solution[i], actualSolution[i]);
            }

            return errorsSum / solution.Length;
        }
    }
}
