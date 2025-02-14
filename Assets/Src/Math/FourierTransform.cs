using UnityEngine;

namespace Src.Math
{
    public class FourierTransform
    {
        public static (float[] xMass, float[] frequencies) SampleMassCenters(float[] signal, float signalTime, float minFrequency,
            float maxFrequency, int samplesCount)
        {
            float[] xMasses = new float[samplesCount];
            float[] frequencies = new float[samplesCount];
            float frequencyStep = (maxFrequency - minFrequency) / samplesCount;
            for (int i = 0; i < samplesCount; i++)
            {
                var frequency = minFrequency + i * frequencyStep;
                var angleStep = (signalTime * frequency * 2 * Mathf.PI) / signal.Length;
                var xSum = 0f;
                for (int j = 0; j < signal.Length; j++)
                {
                    var angle = j * angleStep;
                    var radius = signal[j];
                    var xComponent = radius * Mathf.Cos(angle);
                    xSum += xComponent;
                }

                var averageX = xSum / signal.Length;
                averageX *= signalTime;
                xMasses[i] = averageX;
                frequencies[i] = frequency;
            }

            return (xMasses, frequencies);
        }
    }
}