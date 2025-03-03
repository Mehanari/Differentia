using Src.Math;
using Src.Math.Components;
using Src.Pendulum;
using UnityEngine;

namespace Src.OptimalControlProblems.PendulumControl
{
    public class Simulation : SimulationBase
    {
        [Header("Simulation parameters")]
        [Tooltip("Free fall acceleration")]
        [SerializeField] private float g;
        [Tooltip("Length of the pendulum")]
        [SerializeField] private float length;
        [SerializeField] private float initialAngle;
        [SerializeField] private float initialAngularVelocity;

        [Header("Control generation parameters")] 
        [SerializeField] private int controlSamples = 10000;
        [SerializeField] private float controlTime = 1f;
        [SerializeField] private float targetAngle = Mathf.PI / 2;
        [SerializeField] private float controlTolerance = 0.0001f;
        [SerializeField] private float derivativeDelta = 0.00001f;
        [Header("Visualisation")]
        [SerializeField] private PendulumView view;

        //State of a pendulum at any given moment in time can be described by angle and velocity pair.
        private float[] _angles;
        private float[] _velocities;

        private FuncVector _pendulumDynamics;
        
        protected override void Start()
        {
            base.Start();
            
            var controlGenerator = new ThrowControlGenerator
            {
                Gravity = g,
                PendulumLength = length,
                Tolerance = controlTolerance,
                DerivativeDelta = derivativeDelta,
                ODESamplesCount = controlSamples
            };
            Control control = controlGenerator.GenerateControl(initialAngle, initialAngularVelocity, targetAngle, controlTime);
            
            //Here state vector has 3 values inside: angle, velocity and time.
            //The time increases with a speed of 1.
            _pendulumDynamics = new FuncVector(
                (state) => state[1],
                (state) => - (g/length)*System.Math.Sin(state[0]) /* + control.ControlInput(state[2]) */,
                (state) => 1 
            );

            _angles = new float[samplesCount];
            _velocities = new float[samplesCount];
            _angles[0] = initialAngle;
            _velocities[0] = initialAngularVelocity;

            var initialState = new Vector(3);
            initialState[0] = initialAngle;
            initialState[1] = initialAngularVelocity;
            initialState[2] = 0;

            var stateVectors = ODESolver.CalculateStates(_pendulumDynamics, initialState, simulationTime, samplesCount);
            for (int i = 1; i < stateVectors.Length; i++)
            {
                _angles[i] = (float) stateVectors[i][0];
                _velocities[i] = (float)stateVectors[i][1];
            }
            
            SetSimulationState(0);
        }

        private void OnValidate()
        {
            if(view is null) return;
            view.SetAngleRadians(initialAngle);
            view.SetLength(length);
        }

        protected override void SetSimulationState(int stateIndex)
        {
            view.SetAngleRadians(_angles[stateIndex]);
        }
    }
}