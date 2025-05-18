using MehaMath.Math.Components;
using MehaMath.VisualisationTools.Plotting;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Math
{
    public class MyOptimization : MonoBehaviour
    {
        [SerializeField] private Plotter2D plotter;
        [SerializeField] private Slider firstTangentSlider;
        [SerializeField] private Slider secondTangentSlider;
        [SerializeField] private Button stepButton;
        
        private Polynomial _goal = new Polynomial(1.044, 2.428, 1.7529, 0.3978, 0.0289);
        private double _first;
        private double _second;
        private bool _nextFirst = true;
    
        private void Start()
        {
            plotter.Plot(-10, 5, _goal, 1000, "Plot", Color.red);
            firstTangentSlider.onValueChanged.AddListener(DrawFirstTangent);
            secondTangentSlider.onValueChanged.AddListener(DrawSecondTangent);
            firstTangentSlider.minValue = -10;
            firstTangentSlider.maxValue = 5;
            secondTangentSlider.minValue = -10;
            secondTangentSlider.maxValue = 5;
            _first = -10;
            _second = 5;
            DrawSecondTangent((float)_second);
            DrawFirstTangent((float)_first);
            
            stepButton.onClick.AddListener(Step);
        }

        private void Step()
        {
            var deriv = _goal.Derivative();
            var f1 = _goal.Compute(_first);
            var f2 = _goal.Compute(_second);
            var df1 = deriv.Compute(_first);
            var df2 = deriv.Compute(_second);
            var nextX = (f2 - f1 - _second * df2 + _first * df1) / (df1 - df2);
            if (f1 > f2)
            {
                _first = nextX;
            }
            else
            {
                _second = nextX;
            }
            Debug.Log("Next x: " + nextX);
            _nextFirst = !_nextFirst;
            DrawSecondTangent((float)_second);
            DrawFirstTangent((float)_first);
        }

        private void DrawSecondTangent(float x)
        {
            _second = x;
            DrawTangentLine(x, "Second tangent", Color.magenta);
        }

        private void DrawFirstTangent(float x)
        {
            _first = x;
            DrawTangentLine(x, "First tangent", Color.green);
        }

        private void DrawTangentLine(double at, string lineName, Color color)
        {
            var tangent = _goal.TangentLine(at);
            plotter.Plot(-10, 5, tangent, 1000, lineName, color);
        }
    }
}
