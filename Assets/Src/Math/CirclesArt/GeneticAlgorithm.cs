using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Src.Math.CirclesArt
{
    public class GeneticAlgorithm
    {
        public int PopulationSize { get; set; } = 50;
        public int Iterations { get; set; } = 1000;
        public float RadiusMutationProbability { get; set; } = 0.2f;
        public float AngularVelocityMutationProbability { get; set; } = 0.1f;
        public float InitialAngleMutationProbability { get; set; } = 0.1f;
        public float InitialAngleMutationStep { get; set; } = 0.05f;
        public float RadiusMutationStep { get; set; } = 0.05f;
        public float AngularVelocityMutationStep { get; set; } = 0.05f;
        //If current population`s max appeal doesn't differ from previous population`s max appeal,
        //then the sorting of the current population should additionally prioritize specimens with bigger deviation value. 
        public float MinAppealDifference { get; set; } = 0.05f;

        //The array of best appeal value for each iteration of the last Fit method call.
        //I need it to plot a graph.
        public float[] LastFitBestAppeals;
        
        private class Specimen
        {
            public Circle[] Circles;
            public float Appeal;
            public Vector3[] LineDots;
            
            //How much radii of this specimen`s circles differ from other specimens of the same population
            public float AverageRadiusDeviation;

            //Same thing as with radius, but with angular velocity
            public float AverageAngularVelocityDeviation;
        }

        private class SpecimenComparer : IComparer<Specimen>
        {
            public int Compare(Specimen x, Specimen y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return y.Appeal.CompareTo(x.Appeal);
            }
        }

        //This comparer additionally prioritizes specimens with bigger deviations in radius and angular velocities 
        private class DeviatedSpecimenComparer : IComparer<Specimen>
        {
            public int Compare(Specimen x, Specimen y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                var yComplexCriterion = y.Appeal * y.AverageRadiusDeviation * y.AverageAngularVelocityDeviation;
                var xComplexCriterion = x.Appeal * x.AverageRadiusDeviation * y.AverageAngularVelocityDeviation;
                return yComplexCriterion.CompareTo(xComplexCriterion);
            }
        }
        
        //This methods finds a configuration of circles such that a shape drawn by them
        //crosses the key points or goes as close to them as possible. The key points 
        //are defined by an index and a cartesian coordinate. The index represents the dot's index
        //in the drawing and the coordinate represents position, obviously. It is important for indices to
        //be smaller than samplesCount.
        //Also, the amount of key points (the length of keyPoints array) should not be bigger than samplesCount.
        //And the indices should not repeat of course.
        public Circle[] Fit(Circle[] initialState, float timeStep, int samplesCount,
            (int index, Vector3 point)[] keyPoints)
        {
            LastFitBestAppeals = new float[Iterations];
            
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

            var bestSpecimen = population[0];
            var previousPopulationMaxAppeal = float.MinValue;
            for (int i = 0; i < Iterations; i++)
            {
                GenerateLineDots(population, timeStep, samplesCount);
                EvaluatePopulation(population, keyPoints);
                CalculateAverageRadiusDeviations(population);
                CalculateAverageAngularVelocityDeviations(population);
                var maxRadiusDeviation = GetMaxAverageRadiusDeviation(population);
                var maxAngularVelocityDeviation = GetMaxAverageAngularVelocityDeviation(population);
                var (maxAppeal, minAppeal) = GetMaxAndMinAppeal(population);
                LastFitBestAppeals[i] = maxAppeal;
                
                IComparer<Specimen> comparer = new SpecimenComparer();
                if (Mathf.Abs(maxAppeal - previousPopulationMaxAppeal) < MinAppealDifference)
                {
                    comparer = new DeviatedSpecimenComparer();
                }
                
                Array.Sort(population, comparer);
                Debug.Log("Iteration #" + i + 
                          ". Best appeal: " + maxAppeal
                          + ". Worst appeal: " + minAppeal + "."
                          + " Max radius average deviation: " + maxRadiusDeviation + "."
                          + " Max angular velocity average deviation: " + maxAngularVelocityDeviation + ".");
                var newPopulation = new Specimen[population.Length];
                for (int j = 0; j < newPopulation.Length; j++)
                {
                    var parentA = PickPoison(population);
                    var parentB = PickPoison(population);
                    var child = Breed(parentA, parentB);
                    Mutate(child);
                    newPopulation[j] = child;
                }

                Array.Sort(population, new SpecimenComparer());
                Debug.Log("Recorded best specimen appeal: " + bestSpecimen.Appeal);
                Debug.Log("Population #" + i + " best specimen appeal: " + population[0].Appeal);
                if (population[0].Appeal > bestSpecimen.Appeal)
                {
                    bestSpecimen = population[0];
                }
                
                population = newPopulation;
                previousPopulationMaxAppeal = maxAppeal;
            }
            GenerateLineDots(population, timeStep, samplesCount);
            EvaluatePopulation(population, keyPoints);
            Array.Sort(population, new SpecimenComparer());
            Debug.Log("Best specimen appeal: " + bestSpecimen.Appeal);
            return bestSpecimen.Circles;
        }

        private (float max, float min) GetMaxAndMinAppeal(Specimen[] population)
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
        
        private void CalculateAverageRadiusDeviations(Specimen[] population)
        {
            var circlesCount = population[0].Circles.Length;
            
            //First we calculate average radius for every circle
            var averageRadii = new float[circlesCount];
            for (int i = 0; i < circlesCount; i++)
            {
                var radiiSum = 0f;
                foreach (var specimen in population)
                {
                    radiiSum += specimen.Circles[i].radius;
                }

                averageRadii[i] = radiiSum / circlesCount;
            }

            foreach (var specimen in population)
            {
                var deviationsSum = 0f;
                for (int i = 0; i < circlesCount; i++)
                {
                    var radius = specimen.Circles[i].radius;
                    var averageRadius = averageRadii[i];
                    var deviation = Mathf.Abs(radius - averageRadius);
                    deviationsSum += deviation;
                }

                specimen.AverageRadiusDeviation = deviationsSum / circlesCount;
            }
        }

        private void CalculateAverageAngularVelocityDeviations(Specimen[] population)
        {
            var circlesCount = population[0].Circles.Length;

            var averageAngularVelocities = new float[circlesCount];
            for (int i = 0; i < circlesCount; i++)
            {
                var velocitiesSum = 0f;
                foreach (var specimen in population)
                {
                    velocitiesSum += specimen.Circles[i].angularVelocity;
                }

                averageAngularVelocities[i] = velocitiesSum / circlesCount;
            }
            
            foreach (var specimen in population)
            {
                var deviationsSum = 0f;
                for (int i = 0; i < circlesCount; i++)
                {
                    var velocity = specimen.Circles[i].angularVelocity;
                    var averageVelocity = averageAngularVelocities[i];
                    var deviation = Mathf.Abs(velocity - averageVelocity);
                    deviationsSum += deviation;
                }

                specimen.AverageAngularVelocityDeviation = deviationsSum / circlesCount;
            }
        }

        //The formulation "MaxAverage" may sound strange, but "average" means average in context of one specimen.
        //One specimen has several circles and each of them may differ from the corresponding average value differently.
        //Check the CalculateAverageRadiusDeviations method for better understanding.
        private float GetMaxAverageRadiusDeviation(Specimen[] population)
        {
            var maxDeviation = float.MinValue;
            foreach (var specimen in population)
            {
                if (specimen.AverageRadiusDeviation > maxDeviation)
                {
                    maxDeviation = specimen.AverageRadiusDeviation;
                }
            }

            return maxDeviation;
        }

        //Same thing as in GetMaxAverageRadiusDeviation, but with angular velocity.
        //Idk, there may be a way to make generalized methods for that, but I assume it will worsen the readability,
        //so I'll leave some minor code duplication here.
        private float GetMaxAverageAngularVelocityDeviation(Specimen[] population)
        {
            var maxDeviation = float.MinValue;
            foreach (var specimen in population)
            {
                if (specimen.AverageAngularVelocityDeviation > maxDeviation)
                {
                    maxDeviation = specimen.AverageAngularVelocityDeviation;
                }
            }

            return maxDeviation;
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

        private void EvaluatePopulation(Specimen[] population, (int index, Vector3 point)[] keyPoints)
        {
            for (int i = 0; i < population.Length; i++)
            {
                var specimen = population[i];
                Evaluate(specimen, keyPoints);
            }
        }

        private Specimen Breed(Specimen first, Specimen second)
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

        private Specimen PickPoison(Specimen[] sortedPopulation, float lambda = 0.5f)
        {
            var r = Random.value;
            int index = (int)(-Mathf.Log(1 - r) / lambda);
            index = System.Math.Min(index, sortedPopulation.Length - 1);
            if (index < 0)
            {
                index = 0;
            }
            return sortedPopulation[index];
        }

        private void Evaluate(Specimen specimen, (int index, Vector3 point)[] keyPoints)
        {
            // var totalDistance = 0f;
            // foreach (var point in keyPoints)
            // {
            //     var dot = specimen.LineDots[point.index];
            //     var distance = Vector3.Distance(dot, point.point);
            //     totalDistance += distance;
            // }
            //
            // var averageDistance = totalDistance / keyPoints.Length;
            // specimen.Appeal = 1 / averageDistance;

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

        private void Mutate(Specimen specimen)
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
