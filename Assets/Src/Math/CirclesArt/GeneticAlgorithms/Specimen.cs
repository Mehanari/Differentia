using UnityEngine;

namespace Src.Math.CirclesArt.GeneticAlgorithms
{
    public class Specimen
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
        //Same as for ErrorsModules, but for angles.
        public float[] ErrorAngles;
        public float MaxError;
    }
}