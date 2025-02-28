using System;

namespace Src.Math.Components
{
    /// <summary>
    /// A vector of values.
    /// I need it to perform more flexible operations, like matrix multiplication.
    /// </summary>
    public readonly struct Vector
    {
        public int Length { get; }

        private readonly double[] _values;

        public Vector(int length)
        {
            Length = length;
            _values = new double[length];
        }

        public double this[int i]
        {
            get => _values[i];
            set => _values[i] = value;
        }

        public double MagnitudeSquare()
        {
            var sum = 0d;
            foreach (var val in _values)
            {
                sum += val * val;
            }

            return sum;
        }

        public double Magnitude()
        {
            return System.Math.Sqrt(MagnitudeSquare());
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

        public static Vector operator -(Vector a, Vector b)
        {
            if (a.Length != b.Length)
            {
                throw new InvalidOperationException("Cannot subtract vectors of different length.");
            }

            var result = new Vector(a.Length);
            for (int i = 0; i < a.Length; i++)
            {
                result[i] = a[i] - b[i];
            }

            return result;
        }
        
        public static Vector operator + (Vector a, Vector b)
        {
            if (a.Length != b.Length)
            {
                throw new InvalidOperationException("Cannot add vectors of different length.");
            }

            var result = new Vector(a.Length);
            for (int i = 0; i < a.Length; i++)
            {
                result[i] = a[i] + b[i];
            }

            return result;
        }
    }
}