﻿using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Src.VisualisationTools.Plotting
{
    public class PlotParameters
    {
        public string Name { get; set; }
        public float[] X { get; set; }
        public float[] Y { get; set; }
        public Color Color { get; set; }
        [CanBeNull] public LineRenderer Line { get; set; }
        [CanBeNull] public List<GameObject> Dots { get; set; }
    }
}