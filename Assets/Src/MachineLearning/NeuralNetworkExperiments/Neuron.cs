using System;
using System.Numerics;
using Vector = MehaMath.Math.Components.Vector;

namespace Src.NeuralNetworkExperiments
{
    public abstract class Neuron
    {
	    public Vector Weights { get; set; }
	    public Vector Biases { get; set; }

	    public double Activation(Vector input)
	    {
		    if (input.Length != Weights.Length)
		    {
			    throw new InvalidOperationException(
				    "Weights vector length must correspond to the input vector length.");
		    }

		    return ActivationImpl(input);
	    }

	    protected abstract double ActivationImpl(Vector input);

    }
}
