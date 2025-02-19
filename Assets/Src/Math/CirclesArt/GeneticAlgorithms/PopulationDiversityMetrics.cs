using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Src.Math.CirclesArt.GeneticAlgorithms
{
    /// <summary>
    /// I use this script to experiment with metrics that evaluate initial population diversity for a genetic algorithms.
    /// I use metrics to find the best initial population generation algorithm.
    /// </summary>
    public class PopulationDiversityMetrics : MonoBehaviour
    {
        [SerializeField] private Circle[] circles;
        [SerializeField] private int populationSize = 1;
        [SerializeField] private float radiusMutationProbability = 1f;
        [SerializeField] private float angularVelocityMutationProbability = 0f;
        [SerializeField] private float initialAngleMutationProbability = 0.1f;
        [SerializeField] private float initialAngleMutationStep = 0.05f;
        [SerializeField] private float radiusMutationStep = 0.0125f;
        [SerializeField] private float angularVelocityMutationStep = 0.05f;

        private void Start()
        {
            var population = GenerateInitialPopulation(circles);
            var populationCasted = CastToFloatsArray(population);
            var populationNormalized = NormalizePopulation(populationCasted);
            var grefenstetteBiases = CalculateGrefenstetteBias(populationNormalized);
            Debug.Log("Radius bias: " + grefenstetteBiases[0]);
            Debug.Log("Velocity bias: " + grefenstetteBiases[1]);
            Debug.Log("Initial angle bias: " + grefenstetteBiases[2]);
        }

        /// <summary>
        /// Each circle can be represented as an array with three items.
        /// 0 - radius
        /// 1 - angular velocity
        /// 2 - initial angle.
        /// </summary>
        /// <param name="population"></param>
        /// <returns></returns>
        private float[][][] CastToFloatsArray(Circle[][] population)
        {
            var castedPopulation = new float[population.Length][][];
            for (int i = 0; i < population.Length; i++)
            {
                var specimen = population[i];
                castedPopulation[i] = new float[specimen.Length][];
                for (int j = 0; j < specimen.Length; j++)
                {
                    var gene = specimen[j];
                    castedPopulation[i][j] = new float[3];
                    castedPopulation[i][j][0] = gene.radius;
                    castedPopulation[i][j][1] = gene.angularVelocity;
                    castedPopulation[i][j][2] = gene.arrowAngle;
                }
            }

            return castedPopulation;
        }

        /// <summary>
        /// Make it so that radii, velocities and initial angles values are in range from 0 to 1.
        /// </summary>
        /// <param name="population"></param>
        /// <returns></returns>
        private float[][][] NormalizePopulation(float[][][] population)
        {
            var chromosomeLength = population[0].Length;
            var geneLength = population[0][0].Length;
            
            var normalizedPopulation = new float[population.Length][][];
            for (int i = 0; i < normalizedPopulation.Length; i++)
            {
                normalizedPopulation[i] = new float[chromosomeLength][];
                for (int j = 0; j < normalizedPopulation[i].Length; j++)
                {
                    normalizedPopulation[i][j] = new float[geneLength];
                }
            }
           
            //We perform normalization for each gene in a chromosome individually
            var basesMax = CreateArray2D(chromosomeLength, geneLength, float.MinValue);
            var basesMin = CreateArray2D(chromosomeLength, geneLength, float.MaxValue);
            
            for (int i = 0; i < chromosomeLength; i++)
            {
                for (int j = 0; j < population.Length; j++)
                {
                    var chromosome = population[j];
                    var gene = chromosome[i];
                    for (int k = 0; k < gene.Length; k++)
                    {
                        var value = gene[k];
                        if (value > basesMax[i][k])
                        {
                            basesMax[i][k] = value;
                        }

                        if (value < basesMin[i][k])
                        {
                            basesMin[i][k] = value;
                        }
                    }
                }
            }

            for (int i = 0; i < chromosomeLength; i++)
            {
                for (int j = 0; j < normalizedPopulation.Length; j++)
                {
                    for (int k = 0; k < geneLength; k++)
                    {
                        var value = population[j][i][k];
                        var max = basesMax[i][k];
                        var min = basesMin[i][k];
                        var norm = (value - min) / (max - min);
                        if (max == min)
                        {
                            norm = 1;
                        }
                        normalizedPopulation[j][i][k] = norm;
                    }
                }
            }

            return normalizedPopulation;
        }

        private float[][] CreateArray2D(int length, int width, float defaultValue)
        {
            var array = new float[length][];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new float[width];
                for (int j = 0; j < array[i].Length; j++)
                {
                    array[i][j] = defaultValue;
                }
            }

            return array;
        }

        private Circle[][] GenerateInitialPopulation(Circle[] initialState)
        {
            var chromosomeLength = initialState.Length;
            var population = new Circle[populationSize][];
            for (int i = 0; i < population.Length; i++)
            {
                var specimen = new Circle[chromosomeLength];
                Array.Copy(initialState, specimen, initialState.Length);
                var firstGene = specimen[0];
                specimen[0] = firstGene;
                //Mutate(specimen);
                population[i] = specimen;
            }
            return population;
        }
        
        private void Mutate(Specimen specimen)
        {
            for (int i = 0; i < specimen.Circles.Length; i++)
            {
                var circle = specimen.Circles[i];
                var number = Random.value;
                if (number <= radiusMutationProbability)
                {
                    var sign = Random.value > 0.5f ? 1 : -1;
                    circle.radius += radiusMutationStep * sign;
                }

                number = Random.value;
                if (number <= angularVelocityMutationProbability)
                {
                    var sign = Random.value > 0.5f ? 1 : -1;
                    circle.angularVelocity += angularVelocityMutationStep * sign;
                }
                
                number = Random.value;
                if (number <= initialAngleMutationProbability)
                {
                    var sign = Random.value > 0.5f ? 1 : -1;
                    circle.arrowAngle += initialAngleMutationStep * sign;
                }
                
                specimen.Circles[i] = circle;
            }
        }

        //These method returns biases for three main components forming each gene (circle)
        //The values are in range [0.0, 1.0]. If value is close to 1.0, then population is diverse with respect to this parameter.
        private float[] CalculateGrefenstetteBias(float[][][] populationNormalized)
        {
            //We calculate Grefenstette bias from Initial Population for Genetic Algorithms: A Metric Approach book
            var chromosomeLength = populationNormalized[0].Length;
            var geneLength = populationNormalized[0][0].Length;
            var components = new float[geneLength];
            for (int i = 0; i < chromosomeLength; i++)
            {
                var sums = new float[geneLength];
                var sumsInv = new float[geneLength];
                foreach (var specimen in populationNormalized)
                {
                    var gene = specimen[i];
                    for (int j = 0; j < gene.Length; j++)
                    {
                        sums[j] += gene[j];
                        sumsInv[j] += 1 - gene[j];
                    }
                }

                for (int j = 0; j < geneLength; j++)
                {
                    components[j] += Mathf.Max(sums[j], sumsInv[j]);
                }
            }

            var biases = new float[geneLength];
            for (int i = 0; i < geneLength; i++)
            {
                biases[i] = components[i] / (populationNormalized.Length * chromosomeLength);
                biases[i] = 2 * (1 - biases[i]);
            }
            
            return biases;
        }
    }
}
