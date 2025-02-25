using Src.VisualisationTools.Plotting;
using UnityEngine;

namespace Src.OptimalControlProblems.PendulumControl
{
    public class VisualDebugging : MonoBehaviour
    {
        [SerializeField] private Plotter2D plotter;
        [SerializeField] private float pendulumLength = 1f;
        [SerializeField] private float gravity = 9.81f;
        [SerializeField] private float initialAngle = 0f;
        [SerializeField] private float initialVelocity = 0f;
        [SerializeField] private float lambda1_0start = 0f;
        [SerializeField] private float lambda2_0 = 0f;
        [SerializeField] private float time = 1f;
        [SerializeField] private float lambda1_0final = 5f;
        [SerializeField] private int lambdaSamples = 1000;
        [SerializeField] private int odeSamples = 1000;

        private PendulumODE _ode;
        private PendulumODE.State _currentState;
        

        protected void Start()
        {
            var lambda1Values = new float[lambdaSamples];
            var lambdaStep = (lambda1_0final - lambda1_0start) / lambdaSamples;
            var thetaFinalValues = new float[lambdaSamples];
            for (int i = 0; i < lambdaSamples; i++)
            {
                var lambda1_0 = lambda1_0start + i * lambdaStep;
                var ode = new PendulumODE()
                {
                    InitialState = new PendulumODE.State
                    {
                        Theta = initialAngle,
                        Omega = initialVelocity,
                        Lambda1 = lambda1_0,
                        Lambda2 = lambda2_0
                    },
                    Gravity = gravity,
                    Length = pendulumLength
                };
                var thetaFinal = ode.Solve(time, odeSamples)[odeSamples - 1].Theta;
                lambda1Values[i] = lambda1_0;
                thetaFinalValues[i] = thetaFinal;
            }

            plotter.Plot(lambda1Values, thetaFinalValues, "Theta sensitivity", Color.green);
        }
    }
}