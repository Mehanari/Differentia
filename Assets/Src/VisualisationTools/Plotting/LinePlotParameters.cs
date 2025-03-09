using UnityEngine;

namespace Src.VisualisationTools.Plotting
{
    public class LinePlotParameters : PlotParameters
    {
        public LineRenderer Line { get; set; }
        
        public override void Destroy()
        {
            Object.Destroy(Line.gameObject);
        }
    }
}