using UnityEngine;

namespace Src.VisualisationTools.Plotting
{
    public class MeshPlotParameters : PlotParameters
    {
        public MeshFilter MeshFilter { get; set; }
        
        public override void Destroy()
        {
            Object.Destroy(MeshFilter.gameObject);
        }
    }
}