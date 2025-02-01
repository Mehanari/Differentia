using System;
using System.Collections.Generic;
using UnityEngine;

namespace Src.Math.CirclesArt
{
    /// <summary>
    /// The idea of this GA modification is that it additionally prioritizes those specimen that
    /// have better accuracy in the point where the specimen with the best appeal has the biggest error.
    /// </summary>
    public class BestMistakeCorrectionGA : ElitismGA
    {
        private class CorrectionSpecimenComparer : IComparer<Specimen>
        {
            private Dictionary<Specimen, float> _specimensToCorrections;

            public CorrectionSpecimenComparer(Dictionary<Specimen, float> specimensToCorrections)
            {
                _specimensToCorrections = specimensToCorrections;
            }

            public int Compare(Specimen x, Specimen y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                var xCorrection = _specimensToCorrections[x];
                var yCorrection = _specimensToCorrections[y];
                return (y.Appeal + yCorrection).CompareTo(x.Appeal + xCorrection);
            }
        }
        
        protected override void SortPopulation(Specimen[] population)
        {
            base.SortPopulation(population);
            var bestSpecimen = population[0];
            var mistake = GetMistake(bestSpecimen);
            var corrections = new Dictionary<Specimen, float>();
            foreach (var specimen in population)
            {
                corrections.Add(specimen, 0f);
            }
            CalculateCorrections(corrections, population, mistake.pointIndex, mistake.error);
            var comparer = new CorrectionSpecimenComparer(corrections);
            Array.Sort(population, comparer);
        }

        protected override void GenerateNewPopulation(Specimen[] newPopulation, Specimen[] previousPopulation)
        {
            var previousPopulationCopy = new Specimen[previousPopulation.Length];
            
            //For elitism mechanism we ignore the correction value and pass best specimens based on just their appeal.
            Array.Copy(previousPopulation, previousPopulationCopy, previousPopulation.Length);
            Array.Sort(previousPopulationCopy, new AppealSpecimenComparer());
            var eliteMaxIndex = (int) (previousPopulation.Length * ElitismPercent);
            for (int i = 0; i <= eliteMaxIndex; i++)
            {
                newPopulation[i] = previousPopulationCopy[i];
            }
            
            for (int i = eliteMaxIndex+1; i < newPopulation.Length; i++)
            {
                var parentAIndex = PickIndexPoison(previousPopulation);
                var parentBIndex = PickIndexPoison(previousPopulation, lambda: 0.5f, parentAIndex);
                var child = Breed(previousPopulation[parentAIndex], previousPopulation[parentBIndex]);
                Mutate(child);
                newPopulation[i] = child;
            }
        }

        private (int pointIndex, float error) GetMistake(Specimen bestSpecimen)
        {
            var index = 0;
            var maxError = 0f;
            foreach (var point in CurrentFitKeyPoints)
            {
                var specimenPoint = bestSpecimen.LineDots[point.index];
                var error = Vector3.Distance(specimenPoint, point.point);
                if (error > maxError)
                {
                    maxError = error;
                    index = point.index;
                }
            }
            return (index, maxError);
        }

        private void CalculateCorrections(Dictionary<Specimen, float> specimensToCorrections, Specimen[] population,
            int mistakeIndex, float error)
        {
            foreach (var specimen in population)
            {
                var targetPosition = CurrentFitKeyPoints[mistakeIndex].point;
                var specimenPosition = specimen.LineDots[mistakeIndex];
                var distance = Vector3.Distance(targetPosition, specimenPosition);
                var correction = error - distance; //May be negative
                var correctionPercentage = correction / error;
                specimensToCorrections[specimen] = correctionPercentage;
            }
        }

    }
}