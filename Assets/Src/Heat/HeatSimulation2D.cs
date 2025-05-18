using System.Collections.Generic;
using MehaMath;
using MehaMath.VisualisationTools;
using UnityEngine;

namespace Src.Heat
{
    public class HeatSimulation2D : SimulationBase
    {
        [Header("Simulation parameters")]
        [Tooltip("Thermal diffusivity")]
        [SerializeField] private float alpha;
        [SerializeField] private float width;
        [SerializeField] private float length;
        [SerializeField] private int xDotsCount;
        [SerializeField] private int yDotsCount;
        [SerializeField] private float initialTemperature;
        [Tooltip("Points with initial temperature")]
        [SerializeField] private List<Vector2Int> heatSources;
        [Header("Visualisation")]
        [Tooltip("We use colored plane as a heatmap")]
        [SerializeField] private ColoredPlane coloredPlane;

        //The heat distributions for different discrete points in time
        private float[][][] _temperatures;
        private float _minTemperature;
        private float _maxTemperature;
        
        private float XStep => width / xDotsCount;
        private float YStep => length / yDotsCount;
        
        protected override void Start()
        {
            base.Start();
            coloredPlane.Initialize(width, length, xDotsCount, yDotsCount);
            _temperatures = new float[samplesCount][][];
            for (int i = 0; i < samplesCount; i++)
            {
                float[][] state = new float[xDotsCount][];
                for (int j = 0; j < xDotsCount; j++)
                {
                    state[j] = new float[yDotsCount];
                }
                _temperatures[i] = state;
            }
            foreach (var heatSource in heatSources)
            {
                _temperatures[0][heatSource.x][heatSource.y] = initialTemperature;
            }
            CalculateStates();
            FindInitialMinAndMaxTemperatures();
            
            SetSimulationState(0);
        }

        private void CalculateStates()
        {
            for (int i = 1; i < samplesCount; i++)
            {
                var previousState = _temperatures[i - 1];
                for (int x = 0; x < xDotsCount; x++)
                {
                    for (int y = 0; y < yDotsCount; y++)
                    {
                        var t = previousState[x][y];
                        var tLeft = x > 0 ? previousState[x - 1][y] : t;
                        var tRight = x < xDotsCount - 1 ? previousState[x + 1][y] : t;
                        var tTop = y < yDotsCount - 1 ? previousState[x][y + 1] : t;
                        var tBottom = y > 0 ? previousState[x][y - 1] : t;
                        var horizontalAcceleration = alpha * (tLeft + tRight - 2 * t)/(XStep*XStep);
                        var verticalAcceleration = alpha * (tTop + tBottom - 2 * t)/(YStep*YStep);
                        var newT = t + TimeStep * (horizontalAcceleration + verticalAcceleration);
                        _temperatures[i][x][y] = newT;
                    }
                }
            }
        }
        
        private void FindInitialMinAndMaxTemperatures()
        {
            _minTemperature = _temperatures[0][0][0];
            _maxTemperature = _temperatures[0][0][0];
            for (int i = 0; i < xDotsCount; i++)
            {
                for (int j = 0; j < yDotsCount; j++)
                {
                    var t = _temperatures[0][i][j];
                    if (t < _minTemperature)
                    {
                        _minTemperature = t;
                    }
                    if (t > _maxTemperature)
                    {
                        _maxTemperature = t;
                    }
                }
            }
        }

        protected override void SetSimulationState(int stateIndex)
        {
            var state = _temperatures[stateIndex];
            for (int i = 0; i < xDotsCount; i++)
            {
                for (int j = 0; j < yDotsCount; j++)
                {
                    var t = state[i][j];
                    var color = ColorUtils.HeatToColor(t, _minTemperature, _maxTemperature);
                    coloredPlane.SetDotColor(i, j, color);
                }
            }
        }
    }
}