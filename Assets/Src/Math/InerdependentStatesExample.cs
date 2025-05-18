using System;
using MehaMath.VisualisationTools.Plotting;
using UnityEngine;

namespace Src.Math
{
    public class InerdependentStatesExample : MonoBehaviour
    {
        [SerializeField] private Plotter2D plotter;
        [SerializeField] private int xSamples = 1000;
        [SerializeField] private float xStep = 0.01f;
        [SerializeField] private int timeSamples = 10;
        [SerializeField] private float timeStep = 0.5f;
        void Start()
        {
            var initialY = 0f;
            for (int i = 0; i < timeSamples; i++)
            {
                var x = XOverYAtGivenTime(i*timeStep, 0f);
                var y = YOverXAtGivenTime(i*timeStep, x(0f));
                var yValues = new float[xSamples];
                var xValues = new float[xSamples];
                for (int j = 0; j < xSamples; j++)
                {
                    xValues[j] = j * xStep;
                    yValues[j] = y(xValues[j]);
                }
                plotter.Plot(xValues, yValues, i + "", Color.yellow, new Vector3(0, 0, i*timeStep));
            }
        }

        //The dynamic equation for y
        private Func<float, float> YOverXAtGivenTime(float time, float xAtThisTime)
        {
            var initialY = 0f;
            Func<float, float> y = (xArg) =>
            {
                return initialY + time*xAtThisTime*xAtThisTime/2;
            };
            return y;
        }

        //The dynamic equation for x
        private Func<float, float> XOverYAtGivenTime(float time, float yAtThisTime)
        {
            var initialX = 0f;
            Func<float, float> x = (y) =>
            {
                return initialX + time;
            };
            return x;
        }
    }
}
