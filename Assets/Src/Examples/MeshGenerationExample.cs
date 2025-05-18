using MehaMath.VisualisationTools;
using UnityEngine;

namespace Src.Examples
{
    public class MeshGenerationExample : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        
        void Start()
        {
            var mesh = MeshMaker.GetPlaneMesh(1, 1, 4, 2, Color.white);
            meshFilter.mesh = mesh;
        }
    }
}
