using System;
using Src.VisualisationTools.Plotting;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Math.RootsFinding.Examples
{
    /// <summary>
    /// This example visualizes how the Newton's method works with a single-variable function.
    /// You can specify your function as f and set the drawing boundaries in 'from' and 'to' fields. Parts of the function
    /// outside those boundaries won't be plotted.
    ///
    /// After specifying the parameters you run the scene. It will plot the graph, mark the guess point and draw a tangent line through this point.
    /// After clicking the stepButton the guess point and its tangent line will move according to Newton's method.
    /// </summary>
    public class SingleVariableExample : MonoBehaviour
    {
        [SerializeField] private Button stepButton;
        [SerializeField] private Plotter2D plotter;
        [SerializeField] private int samplesCount;
        [SerializeField] private float from;
        [SerializeField] private float to;
        [SerializeField] private float guess = 3f;
        [SerializeField] private float derivativeDelta = 0.0000001f;

        private Func<float, float> f = (x) =>
        {
            return Mathf.Sin(x);
        };

        private readonly string FUNCTION_PLOT_NAME = "Function";
        private readonly string GUESS_PLOT_NAME = "Guess";
        private readonly string TANGENT_LINE_PLOT_NAME = "Tangent";
        
        private void Start()
        {
            UpdatePlots();
            stepButton.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            //Applying Newton's method to find next guess.
            guess = guess - f(guess) / Derivative(guess);
            UpdatePlots();
        }

        //I used the commented method below to check how my Newton-Raphson method implementation works with different single-variable functions.
        //You don't need this method to run the example, but if  you are interested in applying Newton-Raphson, you can uncomment and use it.
        //Don't forget to duplicate the function f as an objective FuncVector though.
        // private void ApplyNewtonRaphson()
        // {
        //     var objective = new FuncVector((x) => System.Math.Sin(x[0]));
        //     var guessVector = new Vector(1);
        //     guessVector[0] = guess;
        //     var zero = (float)Algorithms.NewtonRaphson(objective, guessVector, derivativeDelta)[0];
        //     guess = zero;
        // }

        private void UpdatePlots()
        {
            plotter.Plot(from, to, f, samplesCount, FUNCTION_PLOT_NAME, Color.red);
            plotter.PlotSingleDot(guess, f(guess), GUESS_PLOT_NAME, Color.yellow);
            plotter.Plot(from, to, GetTangentLine(guess), samplesCount, TANGENT_LINE_PLOT_NAME, Color.blue);
        }


        private Func<float, float> GetTangentLine(float x)
        {
            var derivative = Derivative(x);
            var fX = f(x);
            Func<float, float> tangentLine = (xArg) => fX + derivative * (xArg - x);
            return tangentLine;
        }

        private float Derivative(float x)
        {
            var valueInX = f(x);
            var valueNextToX = f(x + derivativeDelta);
            var difference = valueNextToX - valueInX; //If less then zero, then we are descending.
            var derivative = difference / derivativeDelta;
            return derivative;
        }
    }
}
