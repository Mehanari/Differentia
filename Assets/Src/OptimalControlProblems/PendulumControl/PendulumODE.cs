using UnityEngine;

namespace Src.OptimalControlProblems.PendulumControl
{
    /// <summary>
    /// This is a solver for a system of differential equations for a controlled pendulum.
    /// These differential equations were obtained via Pontryagin's maximum (minimum in this case) principle.
    /// </summary>
    public class PendulumODE
    {
        public struct State
        {
            /// <summary>
            /// Angle of the pendulum.
            /// </summary>
            public float Theta { get; set; }
        
            /// <summary>
            /// Angular velocity of a pendulum.
            /// </summary>
            public float Omega { get; set; }
        
            /// <summary>
            /// Lagrange multiplier for the angle.
            /// </summary>
            public float Lambda1 { get; set; }
        
            /// <summary>
            /// Lagrange multiplier for the angular velocity.
            /// </summary>
            public float Lambda2 { get; set; }

            public static State operator * (State state, float num)
            {
                return new State
                {
                    Theta = state.Theta * num,
                    Omega = state.Omega * num,
                    Lambda1 = state.Lambda1 * num,
                    Lambda2 = state.Lambda2 * num,
                };
            }
            
            public static State operator / (State state, float num)
            {
                return new State
                {
                    Theta = state.Theta / num,
                    Omega = state.Omega / num,
                    Lambda1 = state.Lambda1 / num,
                    Lambda2 = state.Lambda2 / num,
                };
            }
            
            public static State operator + (State a, State b)
            {
                return new State
                {
                    Theta = a.Theta + b.Theta,
                    Omega = a.Omega + b.Omega,
                    Lambda1 = a.Lambda1 + b.Lambda1,
                    Lambda2 = a.Lambda2 + b.Lambda2,
                };
            }
        }
        
        public State InitialState { get; set; }
        public float Gravity { get; set; } = 9.81f;
        public float Length { get; set; } = 1f;

        private State Difference(State current)
        {
            return new State
            {
                Theta = current.Omega,
                Omega = -(Gravity/Length)*Mathf.Sin(current.Theta) - current.Lambda2,
                Lambda1 = current.Lambda2 * (Gravity/Length) * Mathf.Cos(current.Theta),
                Lambda2 = -current.Lambda1
            };
        }

        /// <summary>
        /// Calculates next state of the pendulum with Runge-Kutta 4 method.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public State GetNextStateRK4(State current, float deltaTime)
        {
            var k1 = Difference(current) * deltaTime;
            var k2 = Difference(current + k1 / 2) * deltaTime;
            var k3 = Difference(current + k2 / 2) * deltaTime;
            var k4 = Difference(current + k3) * deltaTime;
            var final = current + (k1 + k2 * 2 + k3 * 3 + k4) / 6;
            return final;
        }

        public State GetNextStateNewton(State current, float deltaTime)
        {
            return current + Difference(current) * deltaTime;
        }

        public State[] Solve(float timePeriod, int samples)
        {
            var timeStep = timePeriod / samples;
            var states = new State[samples];
            states[0] = InitialState;
            for (int i = 1; i < samples; i++)
            {
                var previous = states[i - 1];
                var next = GetNextStateRK4(previous, timeStep);
                states[i] = next;
            }

            return states;
        }
    }
}