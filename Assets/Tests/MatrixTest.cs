using System.Collections.Generic;
using NUnit.Framework;
using Src;
using UnityEngine;

namespace Tests
{
    public class SquareMatrixTest
    {
        [Test]
        [TestCaseSource(nameof(EqualityOperatorsData))]
        public void TestEqualityOperators(SquareMatrix a, SquareMatrix b, bool areEqual)
        {
            var equality = a == b;
            var inequality = a != b;
            Assert.AreEqual(equality, areEqual);
            Assert.AreEqual(inequality, !areEqual);
        }
        
        public static IEnumerable<TestCaseData> EqualityOperatorsData()
        {
            var a = new SquareMatrix(new float[,]
            {
                {1, 2, 3},
                {4, 5, 6},
                {7, 8, 9}
            });
            var b = new SquareMatrix(new float[,]
            {
                {1, 2, 3},
                {4, 5, 6},
                {7, 8, 9}
            });
            var areEqual = true;
            yield return new TestCaseData(a, b, areEqual);
            
            a = new SquareMatrix(new float[,]
            {
                {1, 2, 3},
                {4, 5, 6},
                {7, 8, 9}
            });
            b = new SquareMatrix(new float[,]
            {
                {1, 3, 3},
                {4, -5, 6},
                {7, 8, 9}
            });
            areEqual = false;
            yield return new TestCaseData(a, b, areEqual);
        }
        
        [Test]
        [TestCaseSource(nameof(SubMatrixTestData))]
        public void TestSubMatrix(SquareMatrix matrix, int i, int j, SquareMatrix expected)
        {
            var actual = matrix.SubMatrix(i, j);
            if (actual != expected)
            {
                Assert.Fail("Expected matrix:\n" + expected + "\nActual:\n" + actual);
            }
            else
            {
                Assert.Pass();
            }
        }

        public static IEnumerable<TestCaseData> SubMatrixTestData()
        {
            var matrix = new SquareMatrix(new float[,]
            {
                {1, 2, 3},
                {4, 5, 6},
                {7, 8, 9}
            });
            var i = 0;
            var j = 0;
            var expected = new SquareMatrix(new float[,]
            {
                {5, 6},
                {8, 9}
            });
            yield return new TestCaseData(matrix, i, j, expected);
            
            //Matrix stays the same
            i = 0;
            j = 1;
            expected = new SquareMatrix(new float[,]
            {
                {4, 6},
                {7, 9}
            });
            yield return new TestCaseData(matrix, i, j, expected);

            //Matrix stays the same
            i = 0;
            j = 2;
            expected = new SquareMatrix(new float[,]
            {
                {4, 5},
                {7, 8}
            });
            yield return new TestCaseData(matrix, i, j, expected);
            
            //MATRIX CHANGES HERE
            matrix = new SquareMatrix(new float[,]
            {
                {2, 1, 3, 4},
                {0, -1, 2, 1},
                {3, 2, 0, 5},
                {-1, 3, 2, 1}
            });
            i = 0;
            j = 0;
            expected = new SquareMatrix(new float[,]
            {
                {-1, 2, 1},
                {2, 0, 5},
                {3, 2, 1}
            });
            yield return new TestCaseData(matrix, i, j, expected);

            //matrix stays the same
            i = 0;
            j = 1;
            expected = new SquareMatrix(new float[,]
            {
                {0, 2, 1},
                {3, 0, 5},
                {-1, 2, 1}
            });
            yield return new TestCaseData(matrix, i, j, expected);
            
            //matrix stays the same
            i = 0;
            j = 2;
            expected = new SquareMatrix(new float[,]
            {
                {0, -1, 1},
                {3, 2, 5},
                {-1, 3, 1}
            });
            yield return new TestCaseData(matrix, i, j, expected);
            
            //matrix stays the same
            i = 0;
            j = 3;
            expected = new SquareMatrix(new float[,]
            {
                {0, -1, 2},
                {3, 2, 0},
                {-1, 3, 2}
            });
            yield return new TestCaseData(matrix, i, j, expected);
        }

        [Test]
        [TestCaseSource(nameof(DeterminantTestData))]
        public void TestDeterminant(SquareMatrix matrix, float expectedDeterminant)
        {
            var precision = 0.000001f;
            var actual = matrix.Determinant();
            Assert.AreEqual(expectedDeterminant, actual, precision);
        }

        public static IEnumerable<TestCaseData> DeterminantTestData()
        {
            var matrix = new SquareMatrix(new float[,]
            {
                { 6, 1, 1 },
                { 4, -2, 5 },
                { 2, 8, 7 }
            });
            var determinant = -306f;
            yield return new TestCaseData(matrix, determinant);
            
            matrix = new SquareMatrix(new float[,]
            {
                { 2, 1, 3, 4 },
                { 0, -1, 2, 1 },
                { 3, 2, 0, 5},
                { -1, 3, 2, 1 }
            });
            determinant = 35;
            yield return new TestCaseData(matrix, determinant);
        }

        [Test]
        [TestCaseSource(nameof(TransposeTestData))]
        public void TestTranspose(SquareMatrix matrix, SquareMatrix expectedTranspose)
        {
            var actualTranspose = matrix.Transpose();
            if (actualTranspose != expectedTranspose)
            {
                Assert.Fail("Transpose matrix is incorrect!\nExpected matrix:\n" + expectedTranspose + 
                            "\nActual matrix:\n" + actualTranspose);
            }
            else
            {
                Assert.Pass("Matrices match.\nExpected matrix:\n" + expectedTranspose + 
                            "\nActual matrix:\n" + actualTranspose);
            }
        }

        public static IEnumerable<TestCaseData> TransposeTestData()
        {
            var matrix = new SquareMatrix(new float[,]
            {
                { 1, 2 },
                { 3, 4 }
            });
            var transpose = new SquareMatrix(new float[,]
            {
                { 1, 3 },
                { 2, 4 }
            });
            yield return new TestCaseData(matrix, transpose);

            matrix = new SquareMatrix(new float[,]
            {
                { 1, 2, 3 },
                { 4, 5, 6 },
                { 7, 8, 9 }
            });
            transpose = new SquareMatrix(new float[,]
            {
                { 1, 4, 7 },
                { 2, 5, 8 },
                { 3, 6, 9 }
            });
            yield return new TestCaseData(matrix, transpose);
        }

        [Test]
        [TestCaseSource(nameof(DivisionTestData))]
        public void DivisionTest(SquareMatrix matrix, float divider, SquareMatrix expected)
        {
            var actual = matrix / divider;
            var precision = 0.0000001f;
            for (int i = 0; i < matrix.Size; i++)
            {
                for (int j = 0; j < matrix.Size; j++)
                {
                    Assert.AreEqual(expected[i, j], actual[i, j], precision);
                }
            }
        }

        public static IEnumerable<TestCaseData> DivisionTestData()
        {
            var matrix = new SquareMatrix(new float[,]
            {
                { 1, 2, },
                { 3, 4 }
            });
            var divider = 2f;
            var expected = new SquareMatrix(new float[,]
            {
                { 0.5f, 1f },
                { 1.5f, 2f }
            });
            yield return new TestCaseData(matrix, divider, expected);
        }

        [Test]
        [TestCaseSource(nameof(InverseTestData))]
        public void InverseTest(SquareMatrix matrix, SquareMatrix expected)
        {
            var actual = matrix.Inverse();
            var precision = 0.0000001f;
            for (int i = 0; i < matrix.Size; i++)
            {
                for (int j = 0; j < matrix.Size; j++)
                {
                    var distance = Mathf.Abs(actual[i, j] - expected[i, j]);
                    if (distance > precision)
                    {
                        Assert.Fail("Inverse matrix is incorrect!\nExpected matrix:\n" + expected + 
                                    "\nActual matrix:\n" + actual);
                    }
                }
            }
        }

        public static IEnumerable<TestCaseData> InverseTestData()
        {
            var matrix = new SquareMatrix(new float[,]
            {
                { 1, 2, 3 },
                { 3, 2, 1 },
                { 2, 1, 3 }
            });
            var inverse = new SquareMatrix(new float[,]
            {
                { -5, 3, 4 },
                { 7, 3, -8 },
                { 1, -3, 4 }
            }) / 12;
            yield return new TestCaseData(matrix, inverse);

            matrix = new SquareMatrix(new float[,]
            {
                { 5, 2, -3, 4 },
                { 3, 8, 0, 1 },
                { 2, -5, 17, 2 },
                { -1, 0, 1, 3 }
            });
            inverse = new SquareMatrix(new float[,]
            {
                { 387, -33, 102, -573 },
                { -166, 345, -37, 131 },
                { -114, 111, 132, 27 },
                { 167, -48, -10, 671 }
            }) / 2613;
            yield return new TestCaseData(matrix, inverse);
        }

        [Test]
        [TestCaseSource(nameof(CofactorTestData))]
        public void CofactorTest(SquareMatrix matrix, SquareMatrix expected)
        {
            var actual = matrix.CofactorMatrix();
            var precision = 0.0000001f;
            for (int i = 0; i < matrix.Size; i++)
            {
                for (int j = 0; j < matrix.Size; j++)
                {
                    var distance = Mathf.Abs(actual[i, j] - expected[i, j]);
                    if (distance > precision)
                    {
                        Assert.Fail("Cofactor matrix is incorrect!\nExpected matrix:\n" + expected + 
                                    "\nActual matrix:\n" + actual);
                    }
                }
            }
        }

        public static IEnumerable<TestCaseData> CofactorTestData()
        {
            var matrix = new SquareMatrix(new float[,]
            {
                { 1, 2, 3 },
                { 4, 5, 6 },
                { 5, 4, 3 }
            });
            var cofactors = new SquareMatrix(new float[,]
            {
                { -9, 18, -9 },
                { 6, -12, 6 },
                { -3, 6, -3 }
            });
            yield return new TestCaseData(matrix, cofactors);
        }
    }
}