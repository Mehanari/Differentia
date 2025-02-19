using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Src.Math.CirclesArt.GeneticAlgorithms.GAVariants
{
    /// <summary>
    /// The idea of this GA modification is that it additionally prioritizes those specimen that
    /// have better accuracy in the point where the specimen with the best appeal has the biggest error.
    ///
    /// During sorting the specimens are compared by a complex criterion with two components: normalized appeal and normalized correction.
    /// The weights of the components can be configured.
    /// </summary>
    public class BestMistakeCorrectionGA : SevereMutationGA
    {
        public float AppealWeight { get; set; } = 3f;
        public float CorrectionWeight { get; set; } = 2f;
        
        private Specimen _bestInHistory;
        
        private class CorrectionSpecimenComparer : IComparer<Specimen>
        {
            private Dictionary<Specimen, float> _specimensToNormalizedCorrections;
            private Dictionary<Specimen, float> _specimensToNormalizedAppeals;
            private float _appealWeight;
            private float _correctionWeight;

            public CorrectionSpecimenComparer(Dictionary<Specimen, float> specimensToNormalizedAppeals, Dictionary<Specimen, float> specimensToNormalizedCorrections, float appealWeight, float correctionWeight)
            {
                _specimensToNormalizedAppeals = specimensToNormalizedAppeals;
                _specimensToNormalizedCorrections = specimensToNormalizedCorrections;
                _appealWeight = appealWeight;
                _correctionWeight = correctionWeight;
            }

            public int Compare(Specimen x, Specimen y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                var xCriterion = _appealWeight * _specimensToNormalizedAppeals[x] +
                                 _correctionWeight * _specimensToNormalizedCorrections[x];
                var yCriterion = _appealWeight * _specimensToNormalizedAppeals[y] +
                                 _correctionWeight * _specimensToNormalizedCorrections[y];
                return yCriterion.CompareTo(xCriterion);
            }
        }

        public override Circle[] Fit(Circle[] initialState, float timeStep, int samplesCount, (int index, Vector3 point)[] keyPoints)
        {
            _bestInHistory = new Specimen
            {
                Appeal = 0f,
                Circles = initialState
            };
            return base.Fit(initialState, timeStep, samplesCount, keyPoints);
        }

        protected override void SortPopulation(Specimen[] population)
        {
            base.SortPopulation(population);
            var bestSpecimen = population[0];

            if (bestSpecimen.Appeal > _bestInHistory.Appeal)
            {
                _bestInHistory = bestSpecimen;
            }
            
            
            var mistake = GetMistake(bestSpecimen);
            var corrections = new Dictionary<Specimen, float>();
            foreach (var specimen in population)
            {
                corrections.Add(specimen, 0f);
            }
            CalculateCorrections(corrections, population, mistake.pointIndex, mistake.error);
            Debug.Log("Max correction non-normalized: " + GetMaxCorrection(corrections));
            corrections = NormalizeCorrections(corrections);
            var normalizedAppeals = NormalizeAppeals(population);
            var comparer = new CorrectionSpecimenComparer(normalizedAppeals, corrections, 
                AppealWeight, CorrectionWeight);
            Array.Sort(population, comparer);
        }

        protected override Specimen GetBest(Specimen[] lastPopulation)
        {
            if (lastPopulation[0].Appeal < _bestInHistory.Appeal)
            {
                return _bestInHistory;
            }
            return lastPopulation[0];
        }

        private float GetMaxCorrection(Dictionary<Specimen, float> corrections)
        {
            return GetMinMax(corrections.Values.ToArray()).max;
        }

        private Dictionary<Specimen, float> NormalizeAppeals(Specimen[] population)
        {
            var specimensAppeals = new float[population.Length];
            for (int i = 0; i < population.Length; i++)
            {
                specimensAppeals[i] = population[i].Appeal;
            }
            var (minAppeal, maxAppeal) = GetMinMax(specimensAppeals);


            var specimensToNormalizedAppeals = new Dictionary<Specimen, float>();
            foreach (var specimen in population)
            {
                var appeal = specimen.Appeal;
                var normalizedAppeal = (appeal - minAppeal) / (maxAppeal - minAppeal);
                specimensToNormalizedAppeals.Add(specimen, normalizedAppeal);
            }
            return specimensToNormalizedAppeals;
        }

        private Dictionary<Specimen, float> NormalizeCorrections(Dictionary<Specimen, float> corrections)
        {
            var (minCorrection, maxCorrection) = GetMinMax(corrections.Values.ToArray());
            var normalizedCorrections = new Dictionary<Specimen, float>();
            foreach (var specimen in corrections.Keys)
            {
                var correction = corrections[specimen];
                var normalizedCorrection = (correction - minCorrection) / (maxCorrection - minCorrection);
                normalizedCorrections.Add(specimen, normalizedCorrection);
            }

            return normalizedCorrections;
        }

        private (float min, float max) GetMinMax(float[] values)
        {
            var min = float.MaxValue;
            var max = float.MinValue;

            foreach (var val in values)
            {
                if (val < min)
                {
                    min = val;
                }

                if (val > max)
                {
                    max = val;
                }
            }

            return (min, max);
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
                var elite = previousPopulationCopy[i];
                newPopulation[i] = elite;
            }
            
            for (int i = eliteMaxIndex+1; i < newPopulation.Length; i++)
            {
                var parentAIndex = PickIndexPoison(previousPopulation.Length);
                var parentBIndex = PickIndexPoison(previousPopulation.Length, lambda: 0.5f, parentAIndex);
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