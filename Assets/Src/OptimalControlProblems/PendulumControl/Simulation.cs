using System;
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
        [Header("Visualisation")]
        [SerializeField] private PendulumView view;

        //State of a pendulum at any given moment in time can be described by angle and velocity pair.
        private float[] _angles;
        private float[] _velocities;

        protected override void Start()
        {
            base.Start();

            Func<float, float> controlFunction = (t) =>
            {
                return 5f;
            };

            _angles = new float[samplesCount];
            _velocities = new float[samplesCount];
            _angles[0] = initialAngle;
            _velocities[0] = initialAngularVelocity;
            
            for (int i = 1; i < samplesCount; i++)
            {
                var startVelocity = _velocities[i-1];
                var currentAngle = _angles[i-1];
                var acceleration = -(g / length) * Mathf.Sin(currentAngle) + controlFunction(i * TimeStep);
                var endVelocity = startVelocity + TimeStep * acceleration;
                var averageVelocity = (startVelocity + endVelocity) / 2;
                var nextAngle = currentAngle + averageVelocity * TimeStep;
                var nextVelocity = endVelocity;
                _angles[i] = nextAngle;
                _velocities[i] = nextVelocity;
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