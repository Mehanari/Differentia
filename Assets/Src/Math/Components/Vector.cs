using System;
using System.Text;
using UnityEngine;

namespace Src.Math.Components
{
    /// <summary>
    /// A vector of values.
    /// I need it to perform more flexible operations, like matrix multiplication.
    /// </summary>
    public readonly struct Vector
    {
        public int Length => _values.Length;

        private readonly double[] _values;

        public Vector(int length)
        {
            _values = new double[length];
        }

        public Vector(Vector2 vector2)
        {
            _values = new double[2];
            _values[0] = vector2.x;
            _values[1] = vector2.y;
        }

        /// <summary>
        /// Creates a new vector of lenght equal to baseVector.Length + 1.
        /// Puts values of baseVector in the beginning and value of tail to the end. 
        /// </summary>
        /// <param name="baseVector"></param>
        /// <param name="tail"></param>
        public Vector(Vector baseVector, double tail)
        {
            _values = new double[baseVector.Length + 1];
            for (int i = 0; i < baseVector.Length; i++)
            {
                _values[i] = baseVector[i];
            }

            _values[^1] = tail;
        }

        public double this[int i]
        {
            get => _values[i];
            set => _values[i] = value;
        }

        /// <summary>
        /// Converts this vector to a Unity Vector2.
        /// If length of this vector is bigger than 2, then remaining values will be lost.
        /// If length of this vector is less than 2, then missing values will be set to zero.
        /// </summary>
        /// <returns></returns>
        public Vector2 ToVector2()
        {
            var x = 0d;
            var y = 0d;
            if (Length > 0)
            {
                x = _values[0];
            }

            if (Length > 1)
            {
                y = _values[1];
            }

            return new Vector2((float)x, (float)y);
        }

        public Vector LeftPart(int length)
        {
            if (length > Length)
            {
                throw new InvalidOperationException("Cannot take left part of length " + length +
                                                    "from a vector of length " + Length);
            }

            var result = new Vector(length);
            for (int i = 0; i < length; i++)
            {
                result[i] = _values[i];
            }

            return result;
        }

        public Vector Normalized()
        {
            return this / Magnitude();
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

        public static Vector operator *(Vector vector, double num)
        {
            var result = new Vector(vector.Length);
            for (int i = 0; i < vector.Length; i++)
            {
                result[i] = vector[i] * num;
            }

            return result;
        }

        public static Vector operator /(Vector vector, double num)
        {
            return vector * (1 / num);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("( ");
            for (int i = 0; i < _values.Length; i++)
            {
                builder.Append(_values[i]);
                if (i == _values.Length - 1)
                {
                    builder.Append(" )");
                }
                else
                {
                    builder.Append(", ");
                }
            }

            return builder.ToString();
        }
    }
}