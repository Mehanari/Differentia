using System;
using System.Collections.Generic;
using Src.Math;
using Src.VisualisationTools.Plotting;
using UnityEngine;

namespace Src.Examples
{
    public class CircularPlotExample : MonoBehaviour
    {
        [SerializeField] private CircularPlot2D circlePlot;
        [SerializeField] private Plotter2D plotter;
        [SerializeField] private int samplesCount = 1000;
        [Tooltip("Plot length.")]
        [SerializeField] private float plotSeconds = 1;
        [SerializeField] private float cyclesPerSecond = 1;
        [SerializeField] private List<float> signalsFrequencies;
        
        private float XStep => plotSeconds / samplesCount;

        private void Start()
        {
            plotter.RemovePlotByName("Normal graph");
            circlePlot.RemovePlotByName("Circular graph");
            Func<float, float> function = (angle) =>
            {
                var value = 1f;
                foreach (var frequency in signalsFrequencies)
                {
                    value += Mathf.Cos(angle * 2 * frequency * Mathf.PI);
                }
                return value;
            };
            float[] x = new float[samplesCount];
            float[] y = new float[samplesCount];
            for (int i = 0; i < samplesCount; i++)
            {
                x[i] = i * XStep;
                y[i] = function(x[i]);
            }

            float radiansToDraw = (plotSeconds * cyclesPerSecond)*2*Mathf.PI;
            float angleStep = radiansToDraw / samplesCount;
            float[] angles = new float[samplesCount];
            for (int i = 0; i < samplesCount; i++)
            {
                angles[i] = angleStep * i;
            }

            var fourier = FourierTransform.SampleMassCenters(y, plotSeconds, 0f, 15f, samplesCount);
            plotter.Plot(fourier.frequencies, fourier.xMass, "Fourier", Color.green, new Vector3(0, -5, 0));
            plotter.Plot(x, y, "Normal graph", Color.yellow, new Vector3(0, 5, 0));
            circlePlot.Plot(y, angles, "Circular graph", Color.red);
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            if (circlePlot is null 
                || plotter is null
                || samplesCount < 0)
            {
                return;
            }

        }
    }
}