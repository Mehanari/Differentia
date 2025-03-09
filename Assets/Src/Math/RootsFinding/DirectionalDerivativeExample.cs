using System;
using Src.VisualisationTools.Plotting;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Math.RootsFinding
{
    public class DirectionalDerivativeExample : MonoBehaviour
    {
        [SerializeField] private Button nextGuessButton;
        [SerializeField] private Slider directionSlider;
        [SerializeField] private Plotter3D plotter;
        [SerializeField] private int samplesCount;
        [SerializeField] private Vector2 guess;
        [SerializeField] private float lineLength = 10f;
        [SerializeField] private float from;
        [SerializeField] private float to;
        [SerializeField] private float derivationDelta = 0.001f;

        private Vector2 direction = Vector2.right;
        private Func<Vector2, float> f = (vector) =>
        {
            return vector.x * vector.x + vector.y * vector.y;
        };

        private void Start()
        {
            directionSlider.minValue = 0f;
            directionSlider.maxValue = Mathf.PI * 2;
            directionSlider.onValueChanged.AddListener(DirectionChanged);
            nextGuessButton.onClick.AddListener(NextGuess);
            
            plotter.PlotHeat(new Vector2(from, to), new Vector2(from, to), f, samplesCount, samplesCount, "Function");
            UpdatePlot();
        }

        private void NextGuess()
        {
            var fGuess = f(guess);
            var derivative = Derivative(guess, direction);
            var linePivot = new Vector3(guess.x, fGuess, guess.y);
            var lineDirection = new Vector3(direction.x, derivative, direction.y);
            var distanceToZero = fGuess / derivative; //Distance to point where line (not f) crosses zero.
            var zeroPoint = linePivot - lineDirection * distanceToZero;
            guess = new Vector2(zeroPoint.x, zeroPoint.z);
            UpdatePlot();
        }

        private void UpdatePlot()
        {
            var fGuess = f(guess);
            plotter.PlotSingleDot(new Vector3(guess.x, fGuess, guess.y), "Guess", Color.yellow, dotSize: 0.1f);
            var derivative = Derivative(guess, direction);
            var linePivot = new Vector3(guess.x, fGuess, guess.y);
            var lineDirection = new Vector3(direction.x, derivative, direction.y);
            plotter.PlotStraightLine(lineLength, linePivot, lineDirection, "Tangent line", Color.blue, lineWidth: 0.05f);
            var distanceToZero = fGuess / derivative; //Distance to point where line (not f) crosses zero.
            var zeroPoint = linePivot - lineDirection * distanceToZero;
            plotter.PlotSingleDot(zeroPoint, "Zero", Color.magenta, dotSize: 0.1f);
        }

        private void DirectionChanged(float angle)
        {
            var x = Mathf.Cos(angle);
            var y = Mathf.Sin(angle);
            direction = new Vector2(x, y);
            UpdatePlot();
        }

        private float Derivative(Vector2 pos, Vector2 dir)
        {
            dir = dir.normalized;
            var offset = dir * derivationDelta;
            var fPos = f(pos);
            var fNext = f(pos + offset);
            var derivative = (fNext - fPos) / derivationDelta;
            return derivative;
        }
    }
}