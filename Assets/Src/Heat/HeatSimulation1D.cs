using Src.VisualisationTools;
using UnityEngine;

namespace Src.Heat
{
    /// <summary>
    /// This is a script for simulating the heat distribution change over time in a 1D line.
    /// The initial heat distribution is determined by min and max temperatures and an animation curve.
    /// The states of the system are calculated using the heat equation and Finite Difference Method (FDM).
    /// 
    /// The heat values for points beyond edges (-1 and pointsCount), used to calculate heat on edges with FDM,
    /// are considered to be equal to the ones on edges (0 and pointsCount-1).
    /// By doing so we imitate absence of heat exchange with the environment.
    /// The average temperature may change though due to the approximation error (I suppose).
    /// </summary>
    public class HeatSimulation1D : SimulationBase
    {
        [Header("Simulation parameters")]
        [SerializeField] private float lineLenght;
        [SerializeField] private int pointsCount;
        [Tooltip("Thermal diffusivity")]
        [SerializeField] private float a;
        [Tooltip("Temperatures before the first and after the last point. They will be constant.")]
        [SerializeField] private float minTemperature;
        [SerializeField] private float maxTemperature;
        [SerializeField] private AnimationCurve initialHeatDistribution;
        [Header("Visualisation")]
        [SerializeField] private CustomStraightLine customStraightLine;

        private float StepX => lineLenght / pointsCount;
        private float[][] _temperatureStates;

        protected override void Start()
        {
            base.Start();
            customStraightLine.Initialize();
            customStraightLine.SetLength(lineLenght);
            _temperatureStates = new float[samplesCount][];
            for (int i = 0; i < samplesCount; i++)
            {
                _temperatureStates[i] = new float[pointsCount];
            }

            for (int i = 1; i < pointsCount-1; i++)
            {
                var x = i * StepX;
                customStraightLine.AddDot(x);
            }
            
            //Initializing the initial state
            for (int i = 0; i < pointsCount; i++)
            {
                var x = i * StepX;
                var t = initialHeatDistribution.Evaluate(x / lineLenght) * (maxTemperature - minTemperature) + minTemperature;
                _temperatureStates[0][i] = t;
            }
            
            CalculateStates();
            DisplayState(0);
            Debug.Log("Average temperature in the beginning: " + CalculateAverageTemperature(0));
            Debug.Log("Average temperature in the end: " + CalculateAverageTemperature(samplesCount - 1));
        }
        
        private float CalculateAverageTemperature(int stateIndex)
        {
            float sum = 0;
            for (int i = 0; i < pointsCount; i++)
            {
                sum += _temperatureStates[stateIndex][i];
            }

            return sum / pointsCount;
        }

        protected override void SetSimulationState(int stateIndex)
        {
            DisplayState(stateIndex);
        }

        private void CalculateStates()
        {
            for (int i = 1; i < samplesCount; i++)
            {
                for (int x = 0; x < pointsCount; x++)
                {
                    var previousTemperatures = _temperatureStates[i - 1];
                    var leftTemperature = x == 0 ? previousTemperatures[x] : previousTemperatures[x - 1];
                    var rightTemperature = x == pointsCount - 1 ? previousTemperatures[x] : previousTemperatures[x + 1];
                    var currentTemperature = previousTemperatures[x];
                    var heatChangeSpeed = a * (rightTemperature - 2 * currentTemperature + leftTemperature) / (StepX * StepX);
                    var heat = _temperatureStates[i - 1][x] + heatChangeSpeed * TimeStep;
                    _temperatureStates[i][x] = heat;
                }
            }
        }
        
        private void DisplayState(int stateIndex)
        {
            for (int i = 0; i < pointsCount; i++)
            {
                var t = _temperatureStates[stateIndex][i];
                var color = ColorUtils.HeatToColor(t, minTemperature, maxTemperature);
                customStraightLine.SetDotColor(i, color);
            }
        }
    }
}
