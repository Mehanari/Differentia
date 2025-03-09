using System;
using System.Collections;
using Src.Math;
using Src.Math.Components;
using Src.VisualisationTools.Plotting;
using TMPro;
using UnityEngine;

namespace Src.OptimalControlProblems.PendulumControl
{
    public class VisualDebugging : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textMesh;
        [SerializeField] private Plotter3D plotter;
        [SerializeField] private float dotSize;
        [SerializeField] private int plotSamplesCountRoot = 100;
        [SerializeField] private int odeSamplesCount = 1000;
        [SerializeField] private float odeSimulationTime = 1f;
        [SerializeField] private float lambda1from = -5f;
        [SerializeField] private float lambda1to = 5f;
        [SerializeField] private float lambda2from = -5f;
        [SerializeField] private float lambda2to = 5f;
        [SerializeField] private float Gravity = 9.81f;
        [SerializeField] private float PendulumLength = 1f;
        [SerializeField] private float initialAngle = 0f;
        [SerializeField] private float initialAngularVelocity = 0f;
        [SerializeField] private float targetAngle = Mathf.PI / 2;
 
        private double Sin(double angle) => System.Math.Sin(angle);

        private double Cos(double angle) => System.Math.Cos(angle);

        private FuncVector _dynamics;
        private Func<float, float, float> _thetaDiffOverLambdas;
        
        protected void Start()
        {
            _dynamics = new FuncVector(
                (state) => state[1], //Theta rate of change
                (state) => -(Gravity / PendulumLength)*Sin(state[0]) - state[3]/2, //Omega rate of change
                (state) => state[3] * (Gravity/PendulumLength) * Cos(state[0]), //Lambda1 rate of change
                (state) => - state[2] //Lambda2 rate of change
            ); 
            _thetaDiffOverLambdas = (lambda1, lambda2) =>
            {
                var finalState = ODESolver.CalculateFinalState(_dynamics, GetState(lambda1, lambda2), odeSimulationTime,
                    odeSamplesCount);
                var theta = finalState[0];
                return Mathf.Abs(targetAngle - (float)theta);
            };
            StartCoroutine(Plot());
        }

        private IEnumerator Plot()
        {

            var diffs = new float[plotSamplesCountRoot, plotSamplesCountRoot];
            var l1step = (lambda1to - lambda1from) / plotSamplesCountRoot;
            var l2step = (lambda2to - lambda2from) / plotSamplesCountRoot;
            for (int l1 = 0; l1 < plotSamplesCountRoot; l1++)
            {
                for (int l2 = 0; l2 < plotSamplesCountRoot; l2++)
                {
                    var lambda1 = lambda1from + l1step * l1;
                    var lambda2 = lambda2from + l2step * l2;
                    diffs[l1, l2] = _thetaDiffOverLambdas(lambda1, lambda2);
                    yield return null;
                    textMesh.text = (l1 * plotSamplesCountRoot + l2) + " dots out of " +
                                    plotSamplesCountRoot * plotSamplesCountRoot;
                    if (diffs[l1, l2] < 0.01f)
                    {
                        Debug.Log("Small diff of " + diffs[l1, l2] + " is obtained with (lambda1to; lambda2) of (" + lambda1 + "; " + lambda2 + ")");
                        var initialState = GetState(lambda1, lambda2); 
                        var states = ODESolver.CalculateStates(_dynamics, initialState, odeSimulationTime,
                            odeSamplesCount);
                        Debug.Log("Final state for these lambdas: " + states[odeSamplesCount-1]);
                        Debug.Log("Initial state for these lambdas: " + initialState);
                    }
                }
            }
            plotter.PlotDots((lambda1to-lambda1from), (lambda1to-lambda1from), diffs, "Theta diffs", Color.yellow, dotSize);
        }

        private Vector GetState(float lambda1, float lambda2)
        {
            var vector = new Vector(4);
            vector[0] = initialAngle;
            vector[1] = initialAngularVelocity;
            vector[2] = lambda1;
            vector[3] = lambda2;
            return vector;
        }
    }
}