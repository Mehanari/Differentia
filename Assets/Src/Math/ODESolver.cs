using Src.Math.Components;

namespace Src.Math
{
    public static class ODESolver
    {
        public enum Method
        {
            RK4,
            Newton
        }
        
        public static Vector NextStateRK4(FuncVector dynamics, Vector currentState, double deltaTime)
        {
            var k1 = dynamics.Calculate(currentState) * deltaTime;
            var k2 = dynamics.Calculate(currentState + k1) * deltaTime;
            var k3 = dynamics.Calculate(currentState + k2 / 2) * deltaTime;
            var k4 = dynamics.Calculate(currentState + k3) * deltaTime;
            var result = currentState + (k1 + k2 * 2 + k3 * 3 + k4) / 6;
            return result;
        }

        public static Vector NextStateNewton(FuncVector dynamics, Vector currentState, double deltaTime)
        {
            return currentState + dynamics.Calculate(currentState) * deltaTime;
        }

        public static Vector[] CalculateStates(FuncVector dynamics, Vector initialState, double timePeriod, int samples, Method method = Method.RK4)
        {
            var timeStep = timePeriod / samples;
            var states = new Vector[samples];
            states[0] = initialState;
            for (int i = 1; i < samples; i++)
            {
                var previous = states[i - 1];
                var next = NextStateRK4(dynamics, previous, timeStep);
                if (method == Method.Newton)
                {
                    next = NextStateNewton(dynamics, previous, timeStep);
                }

                states[i] = next;
            }

            return states;
        }

        /// <summary>
        /// Unlike the CalculateStates method, this method does not create an array for all the states and hence requires less memory.
        /// </summary>
        /// <param name="dynamics"></param>
        /// <param name="initialState"></param>
        /// <param name="timePeriod"></param>
        /// <param name="samples"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static Vector CalculateFinalState(FuncVector dynamics, Vector initialState, double timePeriod,
            int samples, Method method = Method.RK4)
        {
            var timeStep = timePeriod / samples;
            var currentState = initialState;
            for (int i = 0; i < samples; i++)
            {
                var previous = currentState;
                var next = NextStateRK4(dynamics, previous, timeStep);
                if (method == Method.Newton)
                {
                    next = NextStateNewton(dynamics, previous, timeStep);
                }

                currentState = next;
            }

            return currentState;
        }
    }
}