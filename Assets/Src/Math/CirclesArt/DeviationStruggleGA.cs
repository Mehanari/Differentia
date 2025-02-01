using System;
using System.Collections.Generic;
using UnityEngine;

namespace Src.Math.CirclesArt
{
    /// <summary>
    /// This GA starts to prioritize specimens with deviations in case if it converged into some local minimum.
    /// Not sure if it really helps though.
    /// </summary>
    public class DeviationStruggleGA : ElitismGA
    {
        //If current population`s max appeal doesn't differ from previous population`s max appeal,
        //then the sorting of the current population should additionally prioritize specimens with bigger deviation value. 
        public float MinAppealDifference { get; set; } = 0.05f;

        private float _previousPopulationMaxAppeal = 0f;

        public override Circle[] Fit(Circle[] initialState, float timeStep, int samplesCount, (int index, Vector3 point)[] keyPoints)
        {
            _previousPopulationMaxAppeal = 0f;
            return base.Fit(initialState, timeStep, samplesCount, keyPoints);
        }

        protected class Deviations 
        {
            //How much radii of this specimen`s circles differ from other specimens of the same population
            public float AverageRadiusDeviation;

            //Same thing as with radius, but with angular velocity
            public float AverageAngularVelocityDeviation;
            
        }
        
        /// <summary>
        /// This comparer additionally prioritizes specimens with bigger deviations in radius and angular velocities.
        /// It multiplies the appeal by specimen deviations.
        /// </summary>
        private class DeviatedSpecimenComparer : IComparer<Specimen>
        {
            private Dictionary<Specimen, Deviations> _specimensToDeviations;

            public DeviatedSpecimenComparer(Dictionary<Specimen, Deviations> specimensToDeviations)
            {
                _specimensToDeviations = specimensToDeviations;
            }


            public int Compare(Specimen x, Specimen y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                var yDeviations = _specimensToDeviations[y];
                var xDeviations = _specimensToDeviations[x];
                var yComplexCriterion = y.Appeal * yDeviations.AverageRadiusDeviation * yDeviations.AverageAngularVelocityDeviation;
                var xComplexCriterion = x.Appeal * xDeviations.AverageRadiusDeviation * xDeviations.AverageAngularVelocityDeviation;
                return yComplexCriterion.CompareTo(xComplexCriterion);
            }
        }

        protected override void SortPopulation(Specimen[] population)
        {
            var specimensDeviations = new Dictionary<Specimen, Deviations>();
            foreach (var specimen in population)
            {
                specimensDeviations.Add(specimen, new Deviations());
            }
            CalculateAverageRadiusDeviations(specimensDeviations, population);
            CalculateAverageAngularVelocityDeviations(specimensDeviations, population);
            (var currentMaxAppeal, _) = GetMaxAndMinAppeal(population);
            IComparer <Specimen> comparer = new AppealSpecimenComparer();
            if (Mathf.Abs(currentMaxAppeal - _previousPopulationMaxAppeal) < MinAppealDifference)
            {
                comparer = new DeviatedSpecimenComparer(specimensDeviations);
            }
            Array.Sort(population, comparer);
            _previousPopulationMaxAppeal = currentMaxAppeal;
        }
        
        private void CalculateAverageRadiusDeviations(Dictionary<Specimen, Deviations> specimensToDeviations, Specimen[] population)
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
                specimensToDeviations[specimen].AverageRadiusDeviation= deviationsSum / circlesCount;
            }
        }

        private void CalculateAverageAngularVelocityDeviations(Dictionary<Specimen, Deviations> specimensToDeviations, Specimen[] population)
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

                specimensToDeviations[specimen].AverageAngularVelocityDeviation = deviationsSum / circlesCount;
            }
        }

        //The formulation "MaxAverage" may sound strange, but "average" means average in context of one specimen.
        //One specimen has several circles and each of them may differ from the corresponding average value differently.
        //Check the CalculateAverageRadiusDeviations method for better understanding.
        private float GetMaxAverageRadiusDeviation(Deviations[] population)
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
        private float GetMaxAverageAngularVelocityDeviation(Deviations[] population)
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
    }
}