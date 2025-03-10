using System;
using System.Collections.Generic;
using Src.Math;
using Src.Math.Components;
using Src.VisualisationTools.Plotting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Src.OptimalControlProblems.PendulumControl
{
    public class SolutionSpaceExploration : MonoBehaviour
    {
        [SerializeField] private Plotter3D plotter;
        [SerializeField] private Button lambda1Forward;
        [SerializeField] private Button lambda1Backward;
        [SerializeField] private Button lambda2Forward;
        [SerializeField] private Button lambda2Backward;
        [SerializeField] private TMP_InputField lambda1Step;
        [SerializeField] private TMP_InputField lambda2Step;
        [SerializeField] private Vector2 drawingRegionSize = new Vector2(5, 5);
        [SerializeField] private float scale = 1f;
        [SerializeField] private Vector2 drawingCenter = Vector2.zero;
        [SerializeField] private int odeSamplesCount = 1000;
        [SerializeField] private int drawingSamplesCount = 100;
        [SerializeField] private float InitialAngle = 0f;
        [SerializeField] private float InitialVelocity = 0f;
        [SerializeField] private float Gravity = 9.81f;
        [SerializeField] private float PendulumLength = 1f;
        [SerializeField] private float TargetAngle = Mathf.PI / 2;
        [SerializeField] private float Time = 1f;
        
        private double Sin(double angle) => System.Math.Sin(angle);
        private double Cos(double angle) => System.Math.Cos(angle);

        private FuncVector _dynamics;

        private void Start()
        {
            _dynamics = new FuncVector(
                (state) => state[1], //Theta rate of change
                (state) => -(Gravity / PendulumLength)*Sin(state[0]) - state[3]/2, //Omega rate of change
                (state) => state[3] * (Gravity/PendulumLength) * Cos(state[0]), //Lambda1 rate of change
                (state) => - state[2] //Lambda2 rate of change
            );

            var xStep = drawingRegionSize.x / drawingSamplesCount;
            var yStep = drawingRegionSize.y / drawingSamplesCount;
            var odeFinalStates = new Vector[drawingSamplesCount, drawingSamplesCount];
            var finalThetaDifferences = new float[drawingSamplesCount, drawingSamplesCount];
            var zeroThetaDiffPositions = new List<Vector3>();
            var finalLambda2Values = new float[drawingSamplesCount, drawingSamplesCount];
            var squaresSum = new float[drawingSamplesCount, drawingSamplesCount];
            var start = drawingCenter - new Vector2(drawingRegionSize.x / 2, drawingRegionSize.y / 2);
            for (int x = 0; x < drawingSamplesCount; x++)
            {
                for (int y = 0; y < drawingSamplesCount; y++)
                {
                    var lambdas = start + (new Vector2(x * xStep, y * yStep))*scale;
                    odeFinalStates[x, y] =
                        ODESolver.CalculateFinalState(_dynamics, InitialState(lambdas), Time, odeSamplesCount);

                    finalThetaDifferences[x, y] = TargetAngle - (float)odeFinalStates[x, y][0];
                    if (Mathf.Abs(finalThetaDifferences[x, y]) < 0.01f)
                    {
                        zeroThetaDiffPositions.Add(new Vector3(x*xStep, finalLambda2Values[x, y], y*yStep));
                    }
                    finalLambda2Values[x, y] = (float)odeFinalStates[x, y][3];
                    squaresSum[x, y] = finalLambda2Values[x, y] * finalLambda2Values[x, y] / 100f +
                                       finalThetaDifferences[x, y] * finalThetaDifferences[x, y] / 1000f;
                }
            }


            plotter.PlotMesh(drawingRegionSize.x, drawingRegionSize.y, finalThetaDifferences, "Theta differences", Color.yellow);
            plotter.PlotCurve(zeroThetaDiffPositions.ToArray(), "Theta diff zero", Color.magenta, 0.05f);
            plotter.PlotMesh(drawingRegionSize.x, drawingRegionSize.y, finalLambda2Values, "Lambda2", Color.blue);
            plotter.PlotMesh(drawingRegionSize.x, drawingRegionSize.y, squaresSum, "Squares sum", Color.green, new Vector3(6, 0, 0));
        }

        private Vector InitialState(Vector2 lambdas)
        {
            var state = new Vector(4);
            state[0] = InitialAngle;
            state[1] = InitialVelocity;
            state[2] = lambdas.x;
            state[3] = lambdas.y;
            return state;
        }
    }
}
