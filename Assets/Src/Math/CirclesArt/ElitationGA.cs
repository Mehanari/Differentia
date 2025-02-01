using UnityEngine;

namespace Src.Math.CirclesArt
{
    /// <summary>
    /// This version of genetic algorithm has an elitism mechanism which lets specified percent of best specimens of the
    /// current generation go to the next generation.
    /// </summary>
    public class ElitismGA : BaseGA
    {
        public float ElitismPercent { get; set; } = 0.1f;

        public ElitismGA()
        {
            if (ElitismPercent >= 1)
            {
                Debug.Log("Elitism percent must be in range [0, 1)");
                ElitismPercent = 0.1f;
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
                var parentAIndex = PickIndexPoison(previousPopulation);
                var parentBIndex = PickIndexPoison(previousPopulation, lambda: 0.5f, parentAIndex);
                var child = Breed(previousPopulation[parentAIndex], previousPopulation[parentBIndex]);
                Mutate(child);
                newPopulation[i] = child;
            }
        }
    }
}