using System;
using System.Collections.Generic;
using UnityEngine;

namespace Src.Math.CirclesArt.GeneticAlgorithms.GAVariants
{
    /// <summary>
    /// The idea behind this GA is that each individual in a population may have different preference for a partner.
    /// That is, each specimen has a special "love" function which helps rank other specimen based on how much they may help
    /// improve the current specimen.
    ///
    /// This particular implementation picks the first parent by appeal, and the second by the first's preference.
    /// The preference is determined by how much the other specimen fixes the first's mistake (same as in BestMistakeCorrectionGA).
    /// 
    /// </summary>
    public class LoveGA : SevereMutationGA
    {
        private class SpecimenToCorrection
        {
            public Specimen Specimen;
            public float Correction;
        }
        
        private class CorrectionComparer : IComparer<SpecimenToCorrection>
        {
            public int Compare(SpecimenToCorrection x, SpecimenToCorrection y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return y.Correction.CompareTo(x.Correction);
            }
        }
        
        protected override void GenerateNewPopulation(Specimen[] newPopulation, Specimen[] previousPopulation)
        {
            var eliteMaxIndex = (int) (previousPopulation.Length * ElitismPercent);
            for (int i = 0; i <= eliteMaxIndex; i++)
            {
                newPopulation[i] = previousPopulation[i];
            }
            
            for (int i = eliteMaxIndex+1; i < newPopulation.Length; i++)
            {
                var parentAIndex = PickIndexPoison(previousPopulation.Length);
                var parentA = previousPopulation[parentAIndex];
                var parentB = FindFavorite(parentA, previousPopulation);
                var child = Breed(parentA, parentB);
                Mutate(child);
                newPopulation[i] = child;
            }
        }

        protected virtual Specimen FindFavorite(Specimen picker, Specimen[] population)
        {
            var pickerMistake = GetMistake(picker);
            var specimensToCorrections = CalculateCorrections(population, pickerMistake.pointIndex, pickerMistake.error);
            Array.Sort(specimensToCorrections, new CorrectionComparer());
            var favoriteIndex = PickIndexPoison(specimensToCorrections.Length);
            var favorite = specimensToCorrections[favoriteIndex].Specimen;
            return favorite;
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
        
        private SpecimenToCorrection[] CalculateCorrections(Specimen[] population,
            int mistakeIndex, float error)
        {
            var specimensToCorrections = new SpecimenToCorrection[population.Length];
            for (int i = 0; i < population.Length; i++)
            {
                var specimen = population[i];
                var targetPosition = CurrentFitKeyPoints[mistakeIndex].point;
                var specimenPosition = specimen.LineDots[mistakeIndex];
                var distance = Vector3.Distance(targetPosition, specimenPosition);
                var correction = error - distance; //May be negative
                var correctionPercentage = correction / error;
                specimensToCorrections[i] = new SpecimenToCorrection
                {
                    Specimen = specimen,
                    Correction = correctionPercentage
                };
            }

            return specimensToCorrections;
        }
    }
}