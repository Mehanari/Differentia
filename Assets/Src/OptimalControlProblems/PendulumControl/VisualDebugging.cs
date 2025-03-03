using System;
using Src.Math.Components;
using Src.VisualisationTools.Plotting;
using UnityEngine;

namespace Src.OptimalControlProblems.PendulumControl
{
    public class VisualDebugging : MonoBehaviour
    {
        [SerializeField] private Plotter3D plotter;
        [SerializeField] private int plotSamplesCountRoot = 100;
        [SerializeField] private int odeSamplesCount = 1000;
        [SerializeField] private float odeSimulationTime = 1f;
        [SerializeField] private float from = -5f;
        [SerializeField] private float to = 5f;
        [SerializeField] private float Gravity = 9.81f;
        [SerializeField] private float PendulumLength = 1f;
        [SerializeField] private float initialAngle = 0f;
        [SerializeField] private float initialAngularVelocity = 0f;
        [SerializeField] private float targetAngle = Mathf.PI / 2;
 
        private double Sin(double angle) => System.Math.Sin(angle);

        private double Cos(double angle) => System.Math.Cos(angle);
        
        protected void Start()
        {
            var dynamics = new FuncVector(
                (state) => state[1], //Theta rate of change
                (state) => -(Gravity / PendulumLength)*Sin(state[0]) - state[3], //Omega rate of change
                (state) => state[3] * (Gravity/PendulumLength) * Cos(state[0]), //Lambda1 rate of change
                (state) => - state[2] //Lambda2 rate of change
            );
            Func<float, float, float> thetaDiffOverLambdas;
            var lambda1 = new float[plotSamplesCountRoot];
            var lambda2 = new float[plotSamplesCountRoot];
        }
    }
}