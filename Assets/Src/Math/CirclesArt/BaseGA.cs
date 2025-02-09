using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Src.Math.CirclesArt
{
    /// <summary>
    /// Basic genetic algorithm for finding a set of circles that draws a figure (or something close to it)
    /// </summary>
    public class BaseGA
    {
        protected class MaxErrorComparer : IComparer<Specimen>
        {
            public int Compare(Specimen x, Specimen y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.MaxError.CompareTo(y.MaxError);
            }
        }
        
        public int PopulationSize { get; set; } = 100;
        public int Iterations { get; set; } = 10000;
        public float RadiusMutationProbability { get; set; } = 0.2f;
        public float AngularVelocityMutationProbability { get; set; } = 0.1f;
        public float InitialAngleMutationProbability { get; set; } = 0.1f;
        public float InitialAngleMutationStep { get; set; } = 0.05f;
        public float RadiusMutationStep { get; set; } = 0.0125f;
        public float AngularVelocityMutationStep { get; set; } = 0.05f;

        //Distances between improvements, e.g. how many iterations took the each next improvement.
        //This list should be cleared at the beginning of every Fit method call.
        //This field is public so that I can plot the values.
        public List<int> ImprovementIntervals = new();

        //Should be cleared on every Fit call.
        public List<int> ImprovementIndices = new();

        //How much each iteration took.
        //Clear on every Fit call.
        public TimeSpan[] IterationTimes;

        //Should be set to 0f on every Fit call.
        protected float CurrentBestInverseMaxError;

        //This field is for the case if in one of the derived classes I happen to need key points values.
        //I know it is not a very good practice to save an intermediate state for the time of one method call,
        //but I don't see any significant problems and other easy enough solutions as for now.
        protected (int index, Vector3 point)[] CurrentFitKeyPoints;

        protected class Specimen
        {
            public Circle[] Circles;

            //Appeal is a general utility function value used to sort specimens.
            //In basic GA implementation it is 1 divided by max error module. 
            public float Appeal;

            public Vector3[] LineDots;

            //Vectors from drawing key points to specimen's actual drawing points.
            public Vector3[] Errors;
            //Array of error vectors magnitudes. For the sake of not recalculating 
            //this module every time it is needed.
            public float[] ErrorsModules;
            public float MaxError;
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
            IterationTimes = new TimeSpan[Iterations];
            ImprovementIndices.Clear();
            ImprovementIntervals.Clear();
            CurrentBestInverseMaxError = 0f;
            CurrentFitKeyPoints = keyPoints;
            var stopwatch = new Stopwatch();
            var population = GenerateInitialPopulation(initialState);

            for (int i = 0; i < Iterations; i++)
            {
                stopwatch.Start();
                GenerateLineDots(population, timeStep, samplesCount);
                CalculatePopulationErrors(population, keyPoints);
                SortPopulation(population);

                LogPopulationInfo(population, i);

                var newPopulation = new Specimen[population.Length];
                GenerateNewPopulation(newPopulation, population);
                population = newPopulation;
                stopwatch.Stop();
                IterationTimes[i] = stopwatch.Elapsed;
                Debug.Log("Iteration #" + i + " time: " + stopwatch.Elapsed);
                stopwatch.Reset();
            }

            //Processing the last population
            GenerateLineDots(population, timeStep, samplesCount);
            CalculatePopulationErrors(population, keyPoints);
            SortPopulation(population);
            var bestSpecimen = GetBest(population);
            Debug.Log("Best specimen inverse error: " + (1/bestSpecimen.MaxError));
            return bestSpecimen.Circles;
        }
        

        protected virtual Specimen GetBest(Specimen[] lastPopulation)
        {
            Array.Sort(lastPopulation, new MaxErrorComparer());
            return lastPopulation[0];
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
                var parentAIndex = PickIndexPoison(previousPopulation.Length);
                var parentBIndex = PickIndexPoison(previousPopulation.Length, lambda: 0.5f, parentAIndex);
                var child = Breed(previousPopulation[parentAIndex], previousPopulation[parentBIndex]);
                Mutate(child);
                newPopulation[j] = child;
            }
        }

        protected virtual void LogPopulationInfo(Specimen[] population, int iteration)
        {

            var (maxAppeal, minAppeal) = GetMaxAndMinAppeal(population);
            var currentMinMaxError = float.MaxValue;
            foreach (var specimen in population)    
            {
                if (specimen.MaxError < currentMinMaxError)
                {
                    currentMinMaxError = specimen.MaxError;
                }
            }

            var bestInverseMaxError = 1 / currentMinMaxError;
            if (bestInverseMaxError > CurrentBestInverseMaxError)
            {
                CurrentBestInverseMaxError = bestInverseMaxError;
                var lastImprovement = 0;
                if (ImprovementIndices.Count > 0)
                {
                    lastImprovement = ImprovementIndices[^1];
                }

                ImprovementIntervals.Add(iteration - lastImprovement);
                ImprovementIndices.Add(iteration);
            }

            Debug.Log("Iteration #" + iteration +
                      ". Inverse max error: " + bestInverseMaxError +
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
            CalculatePopulationAppeals(population);
            IComparer<Specimen> comparer = new AppealSpecimenComparer();
            Array.Sort(population, comparer);
        }

        protected virtual void CalculatePopulationAppeals(Specimen[] population)
        {
            foreach (var specimen in population)
            {
                CalculateAppealAsMaxErrorInverse(specimen);
            }
        }

        protected void CalculateAppealAsMaxErrorInverse(Specimen specimen)
        {
            var maxErrorModule = float.MinValue;
            foreach (var error in specimen.Errors)
            {
                if (error.magnitude > maxErrorModule)
                {
                    maxErrorModule = error.magnitude;
                }
            }

            specimen.Appeal = 1 / maxErrorModule;
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
        /// objective function defined by virtual CalculateSpecimenErrors method.
        /// </summary>
        /// <param name="population"></param>
        /// <param name="keyPoints"></param>
        private void CalculatePopulationErrors(Specimen[] population, (int index, Vector3 point)[] keyPoints)
        {
            for (int i = 0; i < population.Length; i++)
            {
                var specimen = population[i];
                CalculateSpecimenErrors(specimen, keyPoints);
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
        protected int PickIndexPoison(int populationLength, float lambda = 0.5f, int indexToAvoid = -1)
        {
            var r = Random.value;
            int index = (int)(-Mathf.Log(1 - r) / lambda);
            if (index == indexToAvoid)
            {
                index++;
            }
            index = System.Math.Min(index, populationLength - 1);
            if (index < 0)
            {
                index = 0;
            }
            return index;
        }
        
        private void CalculateSpecimenErrors(Specimen specimen, (int index, Vector3 point)[] keyPoints)
        {
            specimen.Errors = new Vector3[keyPoints.Length];
            specimen.ErrorsModules = new float[keyPoints.Length];
            var maxError = float.MinValue;
            foreach (var point in keyPoints)
            {
                var actualDot = specimen.LineDots[point.index];
                var expectedDot = point.point;
                var errorVector = expectedDot - actualDot;
                specimen.Errors[point.index] = errorVector;
                specimen.ErrorsModules[point.index] = errorVector.magnitude;
                if (errorVector.magnitude > maxError)
                {
                    maxError = errorVector.magnitude;
                }
            }

            specimen.MaxError = maxError;
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
