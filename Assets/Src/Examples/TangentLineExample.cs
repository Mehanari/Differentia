using System;
using MehaMath;
using MehaMath.VisualisationTools.Plotting;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Examples
{
    public class TangentLineExample : MonoBehaviour
    {
        [SerializeField] private Plotter2D plotter2D;
        [SerializeField] private int samplesCount = 500;
        [SerializeField] private float from = -2;
        [SerializeField] private float to = 2;
        [SerializeField] private Slider linePointSlider; 
    
        private string tangentLineName = "Tangent line";
        private Func<float, float> function = (x) => Mathf.Sin(x);
        private float Step => Mathf.Abs(to - from) / samplesCount;
        
        private void Start()
        {
            var values = CalculusUtils.Sample(function, from, to, samplesCount);
            plotter2D.Plot(from, to, values, "f(x) = x^2 + x", Color.red);
            
            DrawTangentLine((from+to)/2);
            
            linePointSlider.minValue = from;
            linePointSlider.maxValue = to;
            linePointSlider.value = (from+to)/2;
            linePointSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
        
        private void OnSliderValueChanged(float value)
        {
            plotter2D.RemovePlotByName(tangentLineName);
            DrawTangentLine(value);
        }
        
        
        private void DrawTangentLine(float point)
        {
            point = Mathf.Clamp(point, from, to);
            var differentialValue = (function(point + Step) - function(point))/Step;
            Func<float, float> tangentLine = (x) => function(point) + differentialValue * (x - point);
            var tangentLineValues = CalculusUtils.Sample(tangentLine, from, to, samplesCount);
            plotter2D.Plot(from, to, tangentLineValues, tangentLineName, Color.blue);
        }
    }
}
