using System;
using Src.Math.Components;

namespace Src.Math.RootsFinding
{
    public static class Algorithms
    {
        public static FuncVector PartialDerivatives(Func<Vector, double> func, int paramsCount, double derivationDelta = 0.0000001d)
        {
            var derivs = new Func<Vector, double>[paramsCount];
            for (int i = 0; i < paramsCount; i++)
            {
                var index = i;
                derivs[i] = (v) =>
                {
                    var perturb = new Vector(paramsCount);
                    perturb[index] += derivationDelta;
                    var input = v + perturb;
                    return (func(input) - func(v))/derivationDelta;
                };
            }

            return new FuncVector(derivs);
        }

        /// <summary>
        /// Find such a vector of input values, so that values of objective function with this input are zeros. 
        /// </summary>
        /// <param name="objective"></param>
        /// <param name="guess"></param>
        /// <param name="derivationDelta"></param>
        /// <param name="tolerance"></param>
        /// <param name="iterationsLimit"></param>
        /// <param name="lambda"></param>
        /// <returns></returns>
        public static Vector NewtonRaphson(FuncVector objective, Vector guess,
            double derivationDelta = 0.0000001d, double tolerance = 0.00001d, int iterationsLimit = 1000, double lambda = 1d)
        {
            var iteration = 0;
            var distance = objective.Calculate(guess).Magnitude();
            while (distance > tolerance && iteration < iterationsLimit)
            {
                iteration++;
                var J = SquareJacobian(objective, guess, derivationDelta);
                var I = SquareMatrix.I(guess.Length); //Addition of identity matrix multiplied by lambda is needed to avoid uninversible jacobians.
                guess = guess - objective.Calculate(guess) * (J+I*lambda).Inverse();
                distance = objective.Calculate(guess).Magnitude();
            }

            return guess;
        }
        
        /// <summary>
        /// Create a Jacobian approximation for a given list of multivariable input functions.
        /// </summary>
        /// <param name="objective"></param>
        /// <param name="state"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static SquareMatrix SquareJacobian(FuncVector objective, Vector state, double delta = 0.0000001d)
        {
            if (state.Length != objective.Count)
            {
                throw new InvalidOperationException(
                    "Variables count and objectives functions count do not match. Cannot create a square Jacobian.");
            }

            var size = state.Length;
            var result = new SquareMatrix(size);
            for (int i = 0; i < size; i++)
            {
                var func = objective[i];
                var noPerturbResult = func(state);
                for (int j = 0; j < size; j++)
                {
                    var perturb = new Vector(size);
                    perturb[j] += delta;
                    var input = state + perturb;
                    result[i, j] = (func(input) - noPerturbResult) / delta;
                }
            }

            return result;
        }
    }
}
