using Src.VisualisationTools.Plotting;
using UnityEngine;

namespace Src.Movement
{
    /// <summary>
    /// This script simulates the movement of two balls thrown vertically from a certain height with a certain initial velocity.
    /// The balls bounce off the floor without the loss of impulse if they hit it.
    /// The first ball (red) is affected by air resistance, the second ball (blue) is not.
    /// You can adjust gravity (g) and air resistance (airResistance) to see how it affects the movement of the balls.
    /// Additionally, you can plot the graph of the balls' heights over time.
    /// The simulation can be played as a coroutine or by dragging the time slider.
    /// </summary>
    public class VerticalMovementSimulation : SimulationBase
    {
        [Header("Simulation parameters")]
        [SerializeField] private float initialHeight = 0;
        [SerializeField] private float floorHeight = 0;
        [SerializeField] private float initialVelocity = 10;
        [SerializeField] private float g = 9.81f;
        [SerializeField] private float airResistance = 0;
        [Header("Visualisation")]
        [SerializeField] private bool showPlot = true;
        [SerializeField] private Plotter2D plotter2D;
        [SerializeField] private Color plotWithResistance = Color.red;
        [SerializeField] private Color plotNoAirResistance = Color.blue;
        [SerializeField] private GameObject ballWithResistance;
        [SerializeField] private GameObject ballNoAirResistance;
        
        private float[] _heightValuesWithResistance;
        private float[] _heightValuesNoAirResistance;
        
        protected override void Start()
        {
            base.Start();
            _heightValuesWithResistance = CalculateHeightValues(airResistance);
            _heightValuesNoAirResistance = CalculateHeightValues();
            
            if (showPlot)
            {
                plotter2D.Plot(0, simulationTime, 
                    _heightValuesWithResistance, "With air resistance", plotWithResistance, new Vector3(2, 0, 0));
                plotter2D.Plot(0, simulationTime, 
                    _heightValuesNoAirResistance, "No air resistance", plotNoAirResistance, new Vector3(4, 0, 0));
            }
            
            SetSimulationState(0);
        }

        private float[] CalculateHeightValues(float airResistance = 0)
        {
            var step = simulationTime / samplesCount;
            var velocityValues = new float[samplesCount];
            velocityValues[0] = initialVelocity;
            var heightValues = new float[samplesCount];
            heightValues[0] = initialHeight;
            for (int i = 1; i < samplesCount; i++)
            {
                var bounce = false;
                heightValues[i] = heightValues[i - 1] + step * velocityValues[i - 1];
                var newVelocity = velocityValues[i - 1] + step*(- g - airResistance * velocityValues[i - 1]);
                if (heightValues[i] > floorHeight)
                {
                    velocityValues[i] = newVelocity;
                }
                else
                {
                    velocityValues[i] = -newVelocity;
                }
            }
            return heightValues;
        }

        protected override void SetSimulationState(int stateIndex)
        {
            var ballWithResistanceX = ballWithResistance.transform.position.x;
            var ballNoAirResistanceX = ballNoAirResistance.transform.position.x;
            ballWithResistance.transform.position = new Vector3(ballWithResistanceX, _heightValuesWithResistance[stateIndex], 0);
            ballNoAirResistance.transform.position = new Vector3(ballNoAirResistanceX, _heightValuesNoAirResistance[stateIndex], 0);
        }
    }
}
