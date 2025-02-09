using System.Collections.Generic;
using UnityEngine;

namespace Src.Math.CirclesArt
{
    /// <summary>
    /// The idea here is to define a set of most important dots of the drawing
    /// and assign a bigger weight to them, compared to the other dots.
    /// </summary>
    public class CriticalPointsGA : ElitismGA
    {
        public HashSet<int> CriticalPointsIndices { get; set; }
        public float CriticalPointsWeight { get; set; } = 10f;

        protected override void CalculatePopulationAppeals(Specimen[] population)
        {
            foreach (var specimen in population)
            {
                var maxDistance = float.MinValue;
                foreach (var point in CurrentFitKeyPoints)
                {
                    var dot = specimen.LineDots[point.index];
                    var distance = Vector3.Distance(dot, point.point);
                    if (CriticalPointsIndices.Contains(point.index))
                    {
                        distance *= CriticalPointsWeight;
                    }
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                    }
                }
            
                specimen.Appeal = 1 / maxDistance;
            }
        }
    }
}