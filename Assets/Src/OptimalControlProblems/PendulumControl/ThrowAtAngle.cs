using System;
using UnityEngine;

namespace Src.OptimalControlProblems.PendulumControl
{
    /// <summary>
    /// This is a script that generates a control function that translates pendulum from one angle (initial) to another (target)
    /// in a given period of time with a given initial angular velocity. Control function minimizes the overall force used,
    /// but does not enforce final velocity.
    /// Multidimensional Newton-Raphson method is used for finding best values of lambda1 and lambda2 (Check PendulumODE to know whats lambda).
    /// </summary>
    public class ThrowAtAngle
    {
        /// <summary>
        /// The samples count is used when solving pendulum ODE in Distance local function.
        /// </summary>
        public int SamplesCount { get; set; } = 1000;
        
        public float Tolerance = 0.001f;
        
        /// <summary>
        /// Used in Jacobian calculation.
        /// </summary>
        public float DerivativeDelta { get; set; } = 0.00001f;
        
        public Func<float, float> GenerateControl(float initialAngle, float initialVelocity, float targetAngle,
            float time)
        {
            //Lambda1 and lambda2. Next two lines are initial guesses.
            var l1 = 1f;
            var l2 = 1f;
            
            
            // Function below calculates how far is final angle and final lambda2 from their target values.
            // Takes in initial lambda values and time period, and solves the PendulumODE.
            // Returns a 2-elements vector.
            // First is angle difference (|final angle - target angle|).
            // Second is lambda2 difference (same principle here).
            Vector Distance(float lambda1_0, float lambda2_0, PendulumODE.State finalState)
            {
                var result = new Vector(2);
                result[0] = Mathf.Abs(targetAngle - finalState.Theta);
                result[1] = Mathf.Abs(finalState.Lambda2);
                return result;
            }

            SquareMatrix Jacobian(float lambda1_0, float lambda2_0, PendulumODE.State finalState)
            {
                throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }



    }
}