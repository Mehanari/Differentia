using MehaMath.Math.Components;

namespace Src.NeuralNetworkExperiments
{
	public class MultiplicationNeuron : Neuron
	{
		protected override double ActivationImpl(Vector input)
		{
			var mult = input[0]*Weights[0]+Biases[0];
			for (int i = 1; i < input.Length; i++)
			{
				mult *= (input[i] * Weights[i] + Biases[i]);
			}

			return mult;
		}
	}
}