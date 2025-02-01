using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Src.Math.CirclesArt
{
    /// <summary>
    /// Basic genetic algorithm for finding a set of circles that draws a figure (or something close to it)
    /// </summary>
    public class BaseGA
    {
        public int PopulationSize { get; set; } = 50;
        public int Iterations { get; set; } = 2000;
        public float RadiusMutationProbability { get; set; } = 0.2f;
        public float AngularVelocityMutationProbability { get; set; } = 0.1f;
        public float InitialAngleMutationProbability { get; set; } = 0.1f;
        public float InitialAngleMutationStep { get; set; } = 0.05f;
        public float RadiusMutationStep { get; set; } = 0.05f;
        public float AngularVelocityMutationStep { get; set; } = 0.05f;

        //This field is for the case if in one of the derived classes I happen to need key points values.
        //I know it is not a very good practice to save an intermediate state for the time of one method call,
        //but I don't see any significant problems and other easy enough solutions as for now.
        protected (int index, Vector3 point)[] CurrentFitKeyPoints;
        
        protected class Specimen
        {
            public Circle[] Circles;
            public float Appeal;
            public Vector3[] LineDots;
        }

        /// <summary>
        /// Compares specimens just by their appeal.
        /// Sorts array in an descending order.
        /// </summary>
        protected class AppealSpecimenComparer : IComparer<Specimen>
        {
            public int Compare(Specimen x, Specimen y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return y.Appeal.CompareTo(x.Appeal);
            }
        }

        
        //This methods finds a configuration of circles such that a shape drawn by them
        //crosses the key points or goes as close to them as possible. The key points 
        //are defined by an index and a cartesian coordinate. The index represents the dot's index
        //in the drawing and the coordinate represents position, obviously. It is important for indices to
        //be smaller than samplesCount.
        //Also, the amount of key points (the length of keyPoints array) should not be bigger than samplesCount.
        //And the indices should not repeat of course.
        public virtual Circle[] Fit(Circle[] initialState, float timeStep, int samplesCount,
            (int index, Vector3 point)[] keyPoints)
        {
            CurrentFitKeyPoints = keyPoints;
            var population = GenerateInitialPopulation(initialState);
            
            for (int i = 0; i < Iterations; i++)
            {
                GenerateLineDots(population, timeStep, samplesCount);
                EvaluatePopulation(population, keyPoints);
                SortPopulation(population);
                LogPopulationInfo(population, i);
                
                var newPopulation = new Specimen[population.Length];
                GenerateNewPopulation(newPopulation, population);
                population = newPopulation;
            }
            //Processing the last population
            GenerateLineDots(population, timeStep, samplesCount);
            EvaluatePopulation(population, keyPoints);
            Array.Sort(population, new AppealSpecimenComparer());
            var bestSpecimen = population[0];
            Debug.Log("Best specimen appeal: " + bestSpecimen.Appeal);
            return bestSpecimen.Circles;
        }

        private Specimen[] GenerateInitialPopulation(Circle[] initialState)
        {
            var population = new Specimen[PopulationSize];
            for (int i = 0; i < population.Length; i++)
            {
                var specimen = new Specimen
                {
                    Appeal = 0f,
                    Circles = new Circle[initialState.Length]
                };
                Array.Copy(initialState, specimen.Circles, initialState.Length);
                Mutate(specimen);
                population[i] = specimen;
            }

            return population;
        }

        protected virtual void GenerateNewPopulation(Specimen[] newPopulation, Specimen[] previousPopulation)
        {
            for (int j = 0; j < newPopulation.Length; j++)
            {
                var parentAIndex = PickIndexPoison(previousPopulation);
                var parentBIndex = PickIndexPoison(previousPopulation, lambda: 0.5f, parentAIndex);
                var child = Breed(previousPopulation[parentAIndex], previousPopulation[parentBIndex]);
                Mutate(child);
                newPopulation[j] = child;
            }
        }

        protected virtual void LogPopulationInfo(Specimen[] population, int iteration)
        {
            
            var (maxAppeal, minAppeal) = GetMaxAndMinAppeal(population);
            Debug.Log("Iteration #" + iteration + 
                      ". Best appeal: " + maxAppeal
                      + ". Worst appeal: " + minAppeal + ".");
        }

        /// <summary>
        /// In its base variant this method sorts the population according to appeal value of each specimen
        /// in descending order.
        ///
        /// Override this method if you want to customize the way best specimens are determined.
        /// </summary>
        /// <param name="population"></param>
        protected virtual void SortPopulation(Specimen[] population)
        {
            IComparer<Specimen> comparer = new AppealSpecimenComparer();
            Array.Sort(population, comparer);
        }

        protected (float max, float min) GetMaxAndMinAppeal(Specimen[] population)
        {
            var maxAppeal = float.MinValue;
            var minAppeal = float.MaxValue;
            foreach (var specimen in population)
            {
                if (specimen.Appeal > maxAppeal)
                {
                    maxAppeal = specimen.Appeal;
                }

                if (specimen.Appeal < minAppeal)
                {
                    minAppeal = specimen.Appeal;
                }
            }

            return (maxAppeal, minAppeal);
        }

        private void GenerateLineDots(Specimen[] population, float timeStep, int samplesCount)
        {
            for (int i = 0; i < population.Length; i++)
            {
                var specimen = population[i];
                (specimen.LineDots, _) = StatesCalculator.CalculateAllEdgePositions(specimen.Circles, 
                    timeStep, samplesCount);
            }
        }

        
        /// <summary>
        /// Evaluates how close each specimen's drawing is to the target drawing based on the
        /// objective function defined by virtual Evaluate method.
        /// </summary>
        /// <param name="population"></param>
        /// <param name="keyPoints"></param>
        private void EvaluatePopulation(Specimen[] population, (int index, Vector3 point)[] keyPoints)
        {
            for (int i = 0; i < population.Length; i++)
            {
                var specimen = population[i];
                Evaluate(specimen, keyPoints);
            }
        }

        protected Specimen Breed(Specimen first, Specimen second)
        {
            var child = new Specimen
            {
                Appeal = 0f,
                Circles = new Circle[first.Circles.Length]
            };
            var divisionPoint = Random.Range(0, first.Circles.Length);
            Array.Copy(first.Circles, child.Circles, divisionPoint+1);
            Array.Copy(second.Circles, divisionPoint+1, child.Circles, divisionPoint+1, child.Circles.Length - divisionPoint - 1);
            return child;
        }

        /// <summary>
        /// Picks and index from a given population according to a poison distribution.
        /// That is, the lower indices are more likely to be chosen.
        /// 
        /// You can specify <para>indexToAvoid</para> if you want some index to not be chosen.
        /// </summary>
        /// <param name="sortedPopulation"></param>
        /// <param name="lambda"></param>
        /// <param name="indexToAvoid"></param>
        /// <returns></returns>
        protected int PickIndexPoison(Specimen[] sortedPopulation, float lambda = 0.5f, int indexToAvoid = -1)
        {
            var r = Random.value;
            int index = (int)(-Mathf.Log(1 - r) / lambda);
            if (index == indexToAvoid)
            {
                index++;
            }
            index = System.Math.Min(index, sortedPopulation.Length - 1);
            if (index < 0)
            {
                index = 0;
            }
            return index;
        }

        /// <summary>
        /// The base implementation of this method calculates the maximum error (maximum distance) for a specimen
        /// and sets appeal as a inverse of this value.
        /// </summary>
        /// <param name="specimen"></param>
        /// <param name="keyPoints"></param>
        protected virtual void Evaluate(Specimen specimen, (int index, Vector3 point)[] keyPoints)
        {
            var maxDistance = float.MinValue;
            foreach (var point in keyPoints)
            {
                var dot = specimen.LineDots[point.index];
                var distance = Vector3.Distance(dot, point.point);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                }
            }

            specimen.Appeal = 1 / maxDistance;
        }

        /// <summary>
        /// For each circle of the specimen it adds a random difference in radius, angular velocity
        /// and initial angle.
        ///
        /// The sign of difference is determined randomly.
        /// Both -1 and 1 have equal probabilities by default.
        ///
        /// You can override this method to try other mutation strategies.
        /// </summary>
        /// <param name="specimen"></param>
        protected virtual void Mutate(Specimen specimen)
        {
            for (int i = 0; i < specimen.Circles.Length; i++)
            {
                var circle = specimen.Circles[i];
                var number = Random.value;
                if (number <= RadiusMutationProbability)
                {
                    var sign = Random.value > 0.5f ? 1 : -1;
                    circle.radius += RadiusMutationStep * sign;
                }

                number = Random.value;
                if (number <= AngularVelocityMutationProbability)
                {
                    var sign = Random.value > 0.5f ? 1 : -1;
                    circle.angularVelocity += AngularVelocityMutationStep * sign;
                }
                
                number = Random.value;
                if (number <= InitialAngleMutationProbability)
                {
                    var sign = Random.value > 0.5f ? 1 : -1;
                    circle.arrowAngle += InitialAngleMutationStep * sign;
                }
                
                specimen.Circles[i] = circle;
            }
        }
    }
}
