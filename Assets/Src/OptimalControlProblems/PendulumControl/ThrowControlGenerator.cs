using System;

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

        public int MaxIterations { get; set; } = 100;
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
            var displacement = new Vector(2);
            PendulumODE.State[] trajectory;
            var iteration = 0;
            do
            {
                iteration++;
                var ode = new PendulumODE
                {
                    InitialState = new PendulumODE.State
                    {
                        Theta = initialAngle,
                        Omega = initialVelocity,
                        Lambda1 = lambdas[0],
                        Lambda2 = lambdas[1]
                    },
                    Gravity = this.Gravity,
                    Length = this.PendulumLength
                };
                trajectory = ode.Solve(time, SamplesCount);
                var final = trajectory[SamplesCount - 1];
                displacement = Displacement(final);
                var jacobian = Jacobian(lambdas[0], lambdas[1]);
                if (jacobian[0, 0] == 0 || jacobian[1, 0] == 0 || jacobian[1, 0] == 0 || jacobian[1, 1] == 0)
                {
                    throw new Exception("Sensitivity issue occured. Breaking the process");
                }
                var jacobianInverse = jacobian.Inverse();
                var step = displacement * jacobianInverse;
                lambdas -= step;
            } while (displacement.Magnitude() > Tolerance || iteration < MaxIterations);

            return trajectory;
            
            
            // Function below calculates displacement of final state and target state.
            // Returns a 2-elements vector.
            // First is angle difference (final angle - target angle).
            // Second is lambda2 difference (same principle here).
            Vector Displacement(PendulumODE.State finalState)
            {
                var result = new Vector(2);
                result[0] = targetAngle - finalState.Theta;
                result[1] = finalState.Lambda2;
                return result;
            }

            //Returns a Jacobian (partial derivatives matrix) with 4 derivatives:
            // [0, 0] - how final theta changes with a small change of initial lambda1?
            // [0, 1] - how final theta changes with a small change of initial lambda2?
            // [1, 0] - how final lambda2 changes with a small change of initial lambda1?
            // [1, 1] - how final lambda2 changes with a small change of initial lambda2?
            SquareMatrix Jacobian(double lambda1_0, double lambda2_0)
            {
                var unperturbedOde = new PendulumODE()
                {
                    InitialState = new PendulumODE.State
                    {
                        Theta = initialAngle,
                        Omega = initialVelocity,
                        Lambda1 = lambda1_0 ,
                        Lambda2 = lambda2_0
                    },
                    Gravity = this.Gravity,
                    Length = this.PendulumLength
                };
                var l1PerturbOde = new PendulumODE
                {
                    InitialState = new PendulumODE.State
                    {
                        Theta = initialAngle,
                        Omega = initialVelocity,
                        Lambda1 = lambda1_0 + DerivativeDelta,
                        Lambda2 = lambda2_0
                    },
                    Gravity = this.Gravity,
                    Length = this.PendulumLength
                }; 
                var l2PerturbOde = new PendulumODE()
                {
                    InitialState = new PendulumODE.State()
                    {
                        Theta = initialAngle,
                        Omega = initialVelocity,
                        Lambda1 = lambda1_0,
                        Lambda2 = lambda2_0 + DerivativeDelta
                    },
                    Gravity = this.Gravity,
                    Length = this.PendulumLength
                };
                var unperturbedFinalState = unperturbedOde.Solve(time, SamplesCount)[SamplesCount - 1];
                var l1FinalState = l1PerturbOde.Solve(time, SamplesCount)[SamplesCount - 1]; //The final state of a system with small lambda1 change
                var l2FinalState = l2PerturbOde.Solve(time, SamplesCount)[SamplesCount - 1]; //Same here, but for lambda2
                var jacobian = new SquareMatrix(2);
                jacobian[0, 0] = (l1FinalState.Theta - unperturbedFinalState.Theta) / DerivativeDelta;
                jacobian[0, 1] = (l2FinalState.Theta - unperturbedFinalState.Theta) / DerivativeDelta;
                jacobian[1, 0] = (l1FinalState.Lambda2 - unperturbedFinalState.Lambda2) / DerivativeDelta;
                jacobian[1, 1] = (l2FinalState.Lambda2 - unperturbedFinalState.Lambda2) / DerivativeDelta;
                return jacobian;
            }
        }


    }
}