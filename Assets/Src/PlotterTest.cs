using System;
using MehaMath.VisualisationTools.Plotting;
using UnityEngine;

public class PlotterTest : MonoBehaviour
{
   [SerializeField] private Plotter2D plotter;

   private void Start()
   {
      Func<float, float> func = x => Mathf.Cos(x)*Mathf.Sin(x*10);
      plotter.Plot(-10f, 10f, func, 10000, "Tan Function", Color.red);
   }
}
