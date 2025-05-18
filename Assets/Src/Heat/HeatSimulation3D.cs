using System;
using MehaMath;
using MehaMath.VisualisationTools;
using UnityEngine;

namespace Src.Heat
{
    public class HeatSimulation3D : SimulationBase
    {
        [Serializable]
        public class HeatToPosition
        {
            public Vector3Int positionIndices;
            public float temperature;
        }
        
        [Header("Simulation parameters")]
        [Tooltip("Thermal diffusivity")]
        [SerializeField] private float alpha;
        [SerializeField] private float height;
        [SerializeField] private float width;
        [SerializeField] private float length;
        [SerializeField] private int xDotsCount;
        [SerializeField] private int yDotsCount;
        [SerializeField] private int zDotsCount;
        [Tooltip("Default hit is applied to all dots not specified in the heat sources")]
        [SerializeField] private float defaultHeat;
        [SerializeField] private HeatToPosition[] heatSources;
        [Header("Visualisation")]
        [SerializeField] private DotsField3D dotsField3D;
        [SerializeField] private float dotScale = 0.1f;
        [Tooltip("Minimal transparency of the dots")]
        [SerializeField] private float minAlpha = 0.1f;
        [Tooltip("Maximal transparency of the dots")]
        [SerializeField] private float maxAlpha = 0.9f;
        
        private float[][,,] _temperatures;
        private float XStep => width / xDotsCount;
        private float YStep => height / yDotsCount;
        private float ZStep => length / zDotsCount;
        
        //Min and max temperatures for each state of the simulation
        private (float min, float max)[] _minMaxTemperatures;

        protected override void Start()
        {
            base.Start();
            dotsField3D.Initialize(width, height, length, xDotsCount, yDotsCount, zDotsCount);
            dotsField3D.SetDotsScale(dotScale);
            _temperatures = new float[samplesCount][,,];
            _minMaxTemperatures = new (float min, float max)[samplesCount];
            for (int i = 0; i < samplesCount; i++)
            {
                float[,,] state = new float[xDotsCount, yDotsCount, zDotsCount];
                for (int x = 0; x < xDotsCount; x++)
                {
                    for (int y = 0; y < yDotsCount; y++)
                    {
                        for (int z = 0; z < zDotsCount; z++)
                        {
                            state[x, y, z] = defaultHeat;
                        }
                    }
                }
                _temperatures[i] = state;
            }
            foreach (var heatSource in heatSources)
            {
                _temperatures[0][heatSource.positionIndices.x, heatSource.positionIndices.y, heatSource.positionIndices.z] = heatSource.temperature;
            }
            
            CalculateStates();
            GetMinAndMaxTemperatures();
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
                        for (int z = 0; z < zDotsCount; z++)
                        {
                            var t = previousState[x, y, z];
                            var tLeft = x > 0 ? previousState[x - 1, y, z] : t;
                            var tRight = x < xDotsCount - 1 ? previousState[x + 1, y, z] : t;
                            var tTop = y < yDotsCount - 1 ? previousState[x, y + 1, z] : t;
                            var tBottom = y > 0 ? previousState[x, y - 1, z] : t;
                            var tFront = z < zDotsCount - 1 ? previousState[x, y, z + 1] : t;
                            var tBack = z > 0 ? previousState[x, y, z - 1] : t;
                            var xAcceleration = alpha * (tLeft + tRight - 2 * t)/(XStep*XStep);
                            var yAcceleration = alpha * (tTop + tBottom - 2 * t)/(YStep*YStep);
                            var zAcceleration = alpha * (tFront + tBack - 2 * t)/(ZStep*ZStep);
                            var newT = t + TimeStep * (xAcceleration + yAcceleration + zAcceleration);
                            _temperatures[i][x, y, z] = newT;
                        }
                    }
                }
            }
        }
        
        private void GetMinAndMaxTemperatures()
        {
            for (int i = 0; i < samplesCount; i++)
            {
                var state = _temperatures[i];
                var min = float.MaxValue;
                var max = float.MinValue;
                for (int x = 0; x < xDotsCount; x++)
                {
                    for (int y = 0; y < yDotsCount; y++)
                    {
                        for (int z = 0; z < zDotsCount; z++)
                        {
                            var t = state[x, y, z];
                            if (t < min)
                            {
                                min = t;
                            }

                            if (t > max)
                            {
                                max = t;
                            }
                        }
                    }
                }
                _minMaxTemperatures[i] = (min, max);
            }
        }

        protected override void SetSimulationState(int stateIndex)
        {
            var state = _temperatures[stateIndex];
            for (int x = 0; x < xDotsCount; x++)
            {
                for (int y = 0; y < yDotsCount; y++)
                {
                    for (int z = 0; z < zDotsCount; z++)
                    {
                        var t = state[x, y, z];
                        var stateMin = _minMaxTemperatures[stateIndex].min;
                        var stateMax = _minMaxTemperatures[stateIndex].max;
                        var tNormalized = (t - stateMin) / (stateMax - stateMin);
                        var a = minAlpha + tNormalized * (maxAlpha - minAlpha);
                        var initialMin = _minMaxTemperatures[0].min;
                        var initialMax = _minMaxTemperatures[0].max;
                        var color = ColorUtils.HeatToColor(state[x, y, z], initialMin, initialMax);
                        color.a = a;
                        dotsField3D.SetDotColor(x, y, z, color);
                    }
                }
            }
        }
    }
}