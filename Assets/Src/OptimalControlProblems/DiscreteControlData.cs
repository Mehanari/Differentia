using System;
using Newtonsoft.Json;

namespace Src.OptimalControlProblems
{
    [Serializable]
    public class DiscreteControlData
    {
        [JsonProperty] public double Time { get; set; }
        [JsonProperty] public double[] ControlSamples { get; set; }
    }
}