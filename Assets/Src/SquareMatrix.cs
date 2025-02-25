using System;
using System.Text;

namespace Src
{
    public struct SquareMatrix
    {
        public int Size { get; }

        private readonly float[,] _values;
        
        public SquareMatrix(int size)
        {
            Size = size;
            _values = new float[size, size];
        }

        public SquareMatrix(float[,] floatMatrix)
        {
            if (floatMatrix.GetLength(0) != floatMatrix.GetLength(1))
            {
                throw new InvalidOperationException(
                    "Cannot create SquareMatrix from two-dimensional array with different length and height");
            }

            Size = floatMatrix.GetLength(0);
            _values = new float[Size, Size];
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    _values[i, j] = floatMatrix[i, j];
                }
            }
        }

        public float this[int i, int j]
        {
            get => _values[i, j];
            set => _values[i, j] = value;
        }

        public SquareMatrix CofactorMatrix()
        {
            var result = new SquareMatrix(Size);
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    result[i, j] = Cofactor(i, j);
                }
            }

            return result;
        }

        public float Cofactor(int i, int j)
        {
            if (i+1 > Size || j + 1 > Size)
            {
                throw new InvalidOperationException(
                    "Cannot find cofactor. Indices are out of the matrix boundaries.");
            }

            var subMatrix = SubMatrix(i, j);
            return subMatrix.Determinant();
        }

        public float Determinant()
        {
            if (Size == 1)
            {
                return _values[0, 0];
            }
            if (Size == 2)
            {
                return _values[0, 0] * _values[1, 1] - _values[0, 1] * _values[1, 0];
            }

            var determinant = 0f;
            var sign = 1;
            for (int i = 0; i < Size; i++)
            {
                var subDeterminant = _values[0, i] * SubMatrix(0, i).Determinant();
                determinant += sign * subDeterminant;
                sign = -sign;
            }

            return determinant;
        }
        

        /// <summary>
        /// Returns a matrix without i-th row and j-th column.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public SquareMatrix SubMatrix(int i, int j)
        {
            if (i + 1 > Size || j + 1 > Size)
            {
                throw new InvalidOperationException(
                    "Cannot find sub matrix. Indices are out of the matrix boundaries.");
            }

            var sub = new SquareMatrix(Size - 1);
            var row = 0;
            for (int k = 0; k < Size; k++)
            {
                var col = 0;
                if(k == i) continue;
                for (int l = 0; l < Size; l++)
                {
                    if(l == j) continue;
                    sub[row, col] = _values[k, l];
                    col++;
                }

                row++;
            }

            return sub;
        }

        public static bool operator ==(SquareMatrix a, SquareMatrix b)
        {
            if (a.Size != b.Size)
            {
                return false;
            }

            for (int i = 0; i < a.Size; i++)
            {
                for (int j = 0; j < b.Size; j++)
                {
                    if (a[i, j] != b[i, j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        
        public static bool operator !=(SquareMatrix a, SquareMatrix b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    builder.Append(_values[i, j] + " ");
                }

                builder.Append("\n");
            }
            return builder.ToString();
        }
    }
}