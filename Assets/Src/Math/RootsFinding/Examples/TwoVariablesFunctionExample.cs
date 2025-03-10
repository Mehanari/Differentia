using System;
using System.Collections;
using Src.Math.Components;
using Src.VisualisationTools.Plotting;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Math.RootsFinding.Examples
{
    public class TwoVariablesFunctionExample : MonoBehaviour
    {
        [SerializeField] private Button stepButton;
        [SerializeField] private Plotter3D plotter;
        [SerializeField] private GameObject dotPrefab;
        //Next three parameters are applied to both x and y values. Just for simplicity.
        [SerializeField] private int samplesCount;
        [SerializeField] private float from;
        [SerializeField] private float to;

        private float Step => (to - from) / samplesCount;
        private Func<Vector, double> f = (v) =>
        {
            return System.Math.Sin(5*v[0]) + System.Math.Sin(5*v[1]);
        };

        private bool _step = false;

        private void Start()
        {
            var objective = new FuncVector(f);
            var guess = new Vector(2);
            guess[0] = 1;
            guess[1] = 1;
            guess = Algorithms.NewtonRaphson(objective, guess, iterationsLimit: 1);
            var z = f(guess);

            PlotFunction(f);
            var dot = Instantiate(dotPrefab, new Vector3((float)guess[0] - from, (float)z, (float)guess[1] - from), Quaternion.identity);
            dot.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            StartCoroutine(VisualiseNewtonRaphson());
            stepButton.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            _step = true;
        }


        private IEnumerator VisualiseNewtonRaphson()
        {
            var iterationsLimit = 1000;
            var tolerance = 0.0001d;
            var objective = Algorithms.PartialDerivatives(f, 2);
            var guess = new Vector(2);
            guess[0] = 0f;
            guess[1] = 0;
            var z =  f(guess);
            var dot = Instantiate(dotPrefab, new Vector3((float)guess[0] - from, (float)z, (float)guess[1] - from), Quaternion.identity);
            dot.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            var iteration = 0;
            while (objective.Calculate(guess).Magnitude() > tolerance && iteration < iterationsLimit)
            {
                yield return new WaitUntil(() =>
                {
                    if (!_step) return false;
                    _step = !_step;
                    return true;
                });
                var J = Algorithms.SquareJacobian(objective, guess);
                var I = SquareMatrix.I(guess.Length); //Addition of identity matrix multiplied by lambda is needed to avoid uninversible jacobians.
                guess = guess - objective.Calculate(guess) * (J+I*1d).Inverse();
                iteration++;
                z = f(guess);
                dot.transform.position = new Vector3((float)guess[0] - from, (float)z, (float)guess[1] - from);
            }
        }

        private void PlotFunction(Func<Vector, double> f)
        {
            var x = new float[samplesCount];
            var y = new float[samplesCount];
            var z = new float[samplesCount, samplesCount];
            for (int i = 0; i < samplesCount; i++)
            {
                x[i] = from + i * Step;
                y[i] = from + i * Step;
            }

            for (int i = 0; i < samplesCount; i++)
            {
                for (int j = 0; j < samplesCount; j++)
                {
                    var input = new Vector(2);
                    input[0] = x[i];
                    input[1] = y[j];
                    z[i, j] = (float)f(input);
                }
            }
            
            plotter.PlotHeat(Step*samplesCount, Step*samplesCount, z, "Plot");
        }
    }
}