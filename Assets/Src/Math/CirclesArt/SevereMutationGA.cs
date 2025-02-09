using UnityEngine;

namespace Src.Math.CirclesArt
{
    /// <summary>
    /// The idea of this GA is to introduce so-called severe mutation's: replacements of entire circles
    /// instead of subtle changes of circle's crucial values (radius, angular velocity).
    /// The purpose of this is to broaden the search space.
    /// </summary>
    public class SevereMutationGA : ElitismGA
    {
        public float SevereMutationProbability = 0.001f;
        public float MinRadius = 0f;
        public float MaxRadius = 0.5f;
        public float MinAngularVelocity = -2f;
        public float MaxAngularVelocity = 2f;
        public float MinInitialAngle = 0f;
        public float MaxInitialAngle = 2 * Mathf.PI;

        protected override void Mutate(Specimen specimen)
        {
            base.Mutate(specimen);
            if (Random.value < SevereMutationProbability)
            {
                var newCircle = new Circle
                {
                    radius = Random.Range(MinRadius, MaxRadius),
                    angularVelocity = Random.Range(MinAngularVelocity, MaxAngularVelocity),
                    arrowAngle = Random.Range(MinInitialAngle, MaxInitialAngle)
                };
                var index = Random.Range(0, specimen.Circles.Length);
                specimen.Circles[index] = newCircle;
                Debug.Log("Severe mutation occured.");
            }
        }
    }
}