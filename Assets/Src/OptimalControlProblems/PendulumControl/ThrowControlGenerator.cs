using System;
using Src.Math.Components;
using Src.Math.RootsFinding;

namespace Src.OptimalControlProblems.PendulumControl
{
    /// <summary>
    /// This is a script that generates a control function that translates pendulum from one angle (initial) to another (target)
    /// in a given period of time with a given initial angular velocity. Control function minimizes the overall force used,
    /// but does not enforce final velocity.
    /// Multidimensional Newton-Raphson method is used for finding best values of lambda1 and lambda2 (Check PendulumODE to know whats lambda).
    /// </summary>
    public class ThrowControlGenerator
    {
        /// <summary>
        /// The samples count is used when solving pendulum ODE in Distance local function.
        /// </summary>
        public int SamplesCount { get; set; } = 1000;
        
        public double Tolerance = 0.001f;
        
        /// <summary>
        /// Used in Jacobian calculation.
        /// </summary>
        public double DerivativeDelta { get; set; } = 0.0001f;

        public int MaxIterations { get; set; } = 1000;
        public double Gravity { get; set; } = 9.81f;
        public double PendulumLength { get; set; } = 1f;
        
        public Control GenerateControl(double initialAngle, double initialVelocity, double targetAngle,
            double time)
        {
            var optimalTrajectory = FindOptimalTrajectory(initialAngle, initialVelocity, targetAngle, time);
            var controlForces = new double[SamplesCount];
            for (int i = 0; i < SamplesCount; i++)
            {
                controlForces[i] = -optimalTrajectory[i].Lambda2 / 2;
            }

            var control = new LerpControl(time, controlForces);
            return control;
        }

        /// <summary>
        /// The term "Trajectory" is used in a broader sense here. Trajectory means the sequence of states,
        /// which system takes during its "movement" time.
        /// </summary>
        /// <param name="initialAngle"></param>
        /// <param name="initialVelocity"></param>
        /// <param name="targetAngle"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private PendulumODE.State[] FindOptimalTrajectory(double initialAngle, double initialVelocity, double targetAngle,
            double time)
        {
            //Vector of lambda values. 
            var lambdas = new Vector(2);
            lambdas[0] = 0f;
            lambdas[1] = 0f;
            var objective = new FuncVector(
                (v) =>
                {
                    var ode = GetODE(initialAngle, initialVelocity, v);
                    return ode.Solve(time, SamplesCount)[SamplesCount - 1].Theta - targetAngle;
                },
                (v) =>
                {
                    var ode = GetODE(initialAngle, initialVelocity, v);
                    return ode.Solve(time, SamplesCount)[SamplesCount - 1].Lambda2;
                }
                );
            lambdas = Algorithms.NewtonRaphson(objective, lambdas, iterationsLimit: MaxIterations, tolerance: Tolerance, lambda: 0.1f);
            var ode = GetODE(initialAngle, initialVelocity, lambdas);
            return ode.Solve(time, SamplesCount);
        }

        private PendulumODE GetODE(double theta0, double omega0, Vector lambdas0)
        {
            return new PendulumODE()
            {
                InitialState = new PendulumODE.State()
                {
                    Theta = theta0,
                    Omega = omega0,
                    Lambda1 = lambdas0[0],
                    Lambda2 = lambdas0[1]
                },
                Gravity = this.Gravity,
                Length = this.PendulumLength
            };
        }

    }
}