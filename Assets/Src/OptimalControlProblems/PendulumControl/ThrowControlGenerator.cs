using System;
using Src.Math;
using Src.Math.Components;
using Src.Math.RootsFinding;

namespace Src.OptimalControlProblems.PendulumControl
{
    /// <summary>
    /// This is a script that generates a control function that translates pendulum from one angle (initial) to another (target)
    /// in a given period of time with a given initial angular velocity. Control function minimizes the overall force used,
    /// but does not enforce the final velocity.
    /// The pendulum is subject to gravity, but no friction or other forces considered.
    /// Multidimensional Newton-Raphson method is used for finding best values of lambda1 and lambda2.
    /// lambda1 and lambda2 are co-state variables obtained after applying Pontryagin's maximum principle.
    /// </summary>
    public class ThrowControlGenerator
    {
        /// <summary>
        /// The samples count is used when solving pendulum ODE in Distance local function.
        /// </summary>
        public int ODESamplesCount { get; set; } = 1000;
        
        /// <summary>
        /// Used in Jacobian calculation.
        /// </summary>
        public double DerivativeDelta { get; set; } = 0.0001f;

        public int MaxIterations { get; set; } = 1000;
        public double Tolerance = 0.001f;
        public double Gravity { get; set; } = 9.81f;
        public double PendulumLength { get; set; } = 1f;

        /// <summary>
        /// This func vector returns derivatives for states of the pendulum and its co-states.
        /// 0 - theta (angle) derivative;
        /// 1 - omega (angular velocity) derivative;
        /// 2 - lambda1 derivative;
        /// 3 - lambda2 derivative;
        /// </summary>
        private readonly FuncVector dynamics;

        /// <summary>
        /// Just for readability.
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private double Sin(double angle) => System.Math.Sin(angle);

        private double Cos(double angle) => System.Math.Cos(angle);

        public ThrowControlGenerator()
        {
            dynamics = new FuncVector(
                (state) => state[1], //Theta rate of change
                (state) => -(Gravity / PendulumLength)*Sin(state[0]) - state[3], //Omega rate of change
                (state) => state[3] * (Gravity/PendulumLength) * Cos(state[0]), //Lambda1 rate of change
                (state) => - state[2] //Lambda2 rate of change
                );
        }
        
        public Control GenerateControl(double initialAngle, double initialVelocity, double targetAngle,
            double time)
        {
            var optimalTrajectory = FindOptimalTrajectory(initialAngle, initialVelocity, targetAngle, time);
            var controlForces = new double[ODESamplesCount];
            for (int i = 0; i < ODESamplesCount; i++)
            {
                controlForces[i] = -optimalTrajectory[i][3] / 2;
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
        private Vector[] FindOptimalTrajectory(double initialAngle, double initialVelocity, double targetAngle,
            double time)
        {
            //Vector of lambda values. 
            var lambdas = new Vector(2);
            lambdas[0] = 0f;
            lambdas[1] = 0f;
            //On this stage our goal is to find such pair of initial lambda values so that final theta is equal to target angle
            //and final lambda2 is zero.
            //The objective function below takes lambdas vector as an input and returns (theta - targetAngle, lambda2) vector as an output.
            //We need to find such lambdas vector so this function returns (0, 0) or something close enough to (0, 0).
            var objective = new FuncVector(
                (inputLambdas) => System.Math.Abs(CalculateFinalState(ToStateVector(initialAngle, initialVelocity, inputLambdas), time)[0] - targetAngle),
                (inputLambdas) => CalculateFinalState(ToStateVector(initialAngle, initialVelocity, inputLambdas), time)[3]
                );
            lambdas = Algorithms.NewtonRaphson(objective, lambdas, iterationsLimit: MaxIterations, tolerance: Tolerance, lambda: 0.1f);
            var optimalTrajectory = CalculateStates(ToStateVector(initialAngle, initialVelocity, lambdas), time);
            return optimalTrajectory;
        }

        /// <summary>
        /// Just an utility function for transforming two variables and a lambdas vector into a single vector describing pendulum state and co-state. 
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="velocity"></param>
        /// <param name="lambdas"></param>
        /// <returns></returns>
        private Vector ToStateVector(double angle, double velocity, Vector lambdas)
        {
            var state = new Vector(4)
            {
                [0] = angle,
                [1] = velocity,
                [2] = lambdas[0],
                [3] = lambdas[1]
            };
            return state;
        }

        private Vector[] CalculateStates(Vector initialState, double time)
        {
            var states = ODESolver.CalculateStates(dynamics, initialState, time, ODESamplesCount);
            return states;
        }

        private Vector CalculateFinalState(Vector initialState, double time)
        {
            var final =  ODESolver.CalculateFinalState(dynamics, initialState, time, ODESamplesCount);
            return final;
        }

    }
}