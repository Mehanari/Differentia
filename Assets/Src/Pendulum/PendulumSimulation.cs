using System;
using Src.VisualisationTools.Plotting;
using UnityEngine;

namespace Src.Pendulum
{
    /// <summary>
    /// This is a script that simulates the movement of two pendulum models: one with air resistance and one without.
    /// The one with air resistance is red, the one without is blue.
    /// We use Euler's method to calculate the states of the pendulum.
    /// </summary>
    public class PendulumSimulation : SimulationBase
    {
        [Header("Simulation parameters")]
        [Tooltip("Free fall acceleration")]
        [SerializeField] private float g;
        [Tooltip("Length of the pendulum")]
        [SerializeField] private float length;
        [SerializeField] private float initialAngle;
        [SerializeField] private float initialAngularVelocity;
        [SerializeField] private float airResistance;
        [Header("Visualisation")]
        [SerializeField] private bool showPlot = true;
        [SerializeField] private Plotter2D plotter2D;
        [SerializeField] private PendulumView pendulumWithResistanceView;
        [SerializeField] private PendulumView pendulumNoResistanceView;
        
        private float[] _anglesWithResistance;
        private float[] _anglesWithoutResistance;
        
        protected override void Start()
        {
            base.Start();
            _anglesWithResistance = CalculateStates(airResistance);
            _anglesWithoutResistance = CalculateStates();
            
            if (showPlot)
            {
                plotter2D.Plot(0, simulationTime, 
                    _anglesWithResistance, "Pendulum angle", Color.red);
                plotter2D.Plot(0, simulationTime,
                    _anglesWithoutResistance, "Pendulum angle without resistance", Color.blue);
            }
            
            
            pendulumWithResistanceView.SetLength(length);
            pendulumWithResistanceView.SetAngleRadians(initialAngle);
            pendulumNoResistanceView.SetLength(length);
            pendulumNoResistanceView.SetAngleRadians(initialAngle);
        }
        

        protected override void SetSimulationState(int stateIndex)
        {
            var angle = _anglesWithResistance[stateIndex];
            pendulumWithResistanceView.SetAngleRadians(angle);
            angle = _anglesWithoutResistance[stateIndex];
            pendulumNoResistanceView.SetAngleRadians(angle);
        }

        private void OnValidate()
        {
            if (length < 0)
            {
                Debug.LogWarning("Length cannot be negative");
                length = 1;
            }
            pendulumWithResistanceView.SetLength(length);
            pendulumWithResistanceView.SetAngleRadians(initialAngle);
            pendulumNoResistanceView.SetLength(length);
            pendulumNoResistanceView.SetAngleRadians(initialAngle);
        }

        private float[] CalculateStates(float airResistance = 0)
        {
            var step = simulationTime / samplesCount;
            var angles = new float[samplesCount];
            angles[0] = initialAngle;
            var velocities = new float[samplesCount];
            var accelerations = new float[samplesCount];
            velocities[0] = initialAngularVelocity;
            Func<float, float, float> nextAcceleration = (previousVelocity, previousAngle) =>
                -airResistance * previousVelocity - g / length * Mathf.Sin(previousAngle);
            for (int i = 1; i < samplesCount; i++)
            {
                accelerations[i] = nextAcceleration(velocities[i-1], angles[i-1]);
                velocities[i] = velocities[i-1] + step * accelerations[i];
                angles[i] = angles[i-1] + step * velocities[i-1];
            }

            return angles;
        }
    }
}
