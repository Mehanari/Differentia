using System;
using UnityEngine;

namespace Src
{
    /// <summary>
    /// A vector of values.
    /// I need it to perform more flexible operations, like matrix multiplication.
    /// </summary>
    public struct Vector
    {
        public int Length { get; }

        private readonly float[] _values;

        public Vector(int length)
        {
            Length = length;
            _values = new float[length];
        }

        public float this[int i]
        {
            get => _values[i];
            set => _values[i] = value;
        }

        public float MagnitudeSquare()
        {
            var sum = 0f;
            foreach (var val in _values)
            {
                sum += val * val;
            }

            return sum;
        }

        public float Magnitude()
        {
            return Mathf.Sqrt(MagnitudeSquare());
        }

        public static Vector operator *(Vector vector, SquareMatrix matrix)
        {
            if (vector.Length != matrix.Size)
            {
                throw new InvalidOperationException(
                    "Trying to multiply vector and square matrix with different sizes!");
            }

            var result = new Vector(matrix.Size);
            for (int i = 0; i < matrix.Size; i++)
            {
                for (int j = 0; j < matrix.Size; j++)
                {
                    result[i] += vector[i] * matrix[i, j];
                }
            }

            return result;
        }
    }
}