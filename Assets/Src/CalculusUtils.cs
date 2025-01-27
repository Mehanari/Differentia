using System;

namespace Src
{
    public static class CalculusUtils
    {
        public static float[] Sample(Func<float, float> function, float step, int samplesCount)
        {
            var values = new float[samplesCount];
            for (int i = 0; i < samplesCount; i++)
            {
                values[i] = function(i * step);
            }

            return values;
        }

        public static float[] Sample(Func<float, float> function, float from, float to, int samplesCount)
        {
            var values = new float[samplesCount];
            var step = (to - from) / samplesCount;
            for (int i = 0; i < samplesCount; i++)
            {
                values[i] = function(from + i * step);
            }

            return values;
        }

        public static float[] Differentiate(Func<float, float> function, float from, float to, int samplesCount)
        {
            var values = Sample(function, from, to, samplesCount);
            var step = (to - from) / samplesCount;
            return Differentiate(values, step);
        }

        public static float[] Differentiate(Func<float, float> function, float step, int samplesCount)
        {
            var values = Sample(function, step, samplesCount);
            return Differentiate(values, step);
        }

        public static float[] Differentiate(float[] values, float step)
        {
            var differences = new float[values.Length];
            differences[0] = values[0];
            for (int i = 0; i < values.Length - 1; i++)
            {
                differences[i] = (values[i + 1] - values[i]) / step;
            }

            differences[values.Length - 1] = differences[values.Length - 2];
            return differences;
        }

        public static float[] Integrate(float[] derivative, float h, float initial = 0)
        {
            var integrals = new float[derivative.Length];
            integrals[0] = initial;
            for (int i = 1; i < derivative.Length; i++)
            {
                integrals[i] = integrals[i - 1] + derivative[i] * h;
            }

            return integrals;        
        }
    }
}