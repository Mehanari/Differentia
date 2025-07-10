using MehaMath.Math.Components;

namespace Src.NeuralNetworkExperiments
{
	public class SumNeuron : Neuron
	{
		protected override double ActivationImpl(Vector input)
		{
			var sum = 0d;
			for (int i = 0; i < input.Length; i++)
			{
				sum += input[i] * Weights[i];
			}

			return sum + Biases.Sum();
		}
	}
}