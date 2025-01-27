using UnityEngine;

namespace Src.VisualisationTools
{
    public class ColoredPlane : MonoBehaviour
    {
        [SerializeField] private Color defaultVertexColor;
        [SerializeField] private MeshFilter meshFilter;
        private Mesh _mesh;
        private int _xDots;
        private int _yDots;

        public void Initialize(float length, float width, int xDots, int yDots)
        {
            _xDots = xDots;
            _yDots = yDots;
            var vertices = new Vector3[xDots * yDots];
            var triangles = new int[(xDots - 1) * (yDots - 1) * 6];
            var colors = new Color[xDots * yDots];
            var xStep = length / (xDots - 1);
            var yStep = width / (yDots - 1);
            for (int x = 0; x < xDots; x++)
            {
                for (int y = 0; y < yDots; y++)
                {
                    var index = x * yDots + y;
                    vertices[index] = new Vector3(x * xStep, y * yStep, 0);
                    colors[index] = defaultVertexColor;
                }
            }

            for (int x = 0; x < xDots; x++)
            {
                for (int y = 0; y < yDots; y++)
                {
                    if (y == yDots - 1 || x == xDots - 1)
                    {
                        continue;
                    }
                    //Making lower left triangle
                    var lowerLeftIndex = x * yDots + y;
                    var upperLeftIndex = lowerLeftIndex + yDots;
                    var lowerRightIndex = lowerLeftIndex + 1;
                    triangles[(x * (yDots - 1) + y) * 6] = lowerLeftIndex;
                    triangles[(x * (yDots - 1) + y) * 6 + 1] = upperLeftIndex;
                    triangles[(x * (yDots - 1) + y) * 6 + 2] = lowerRightIndex;
                    //Making upper right triangle
                    var upperRightIndex = upperLeftIndex + 1;
                    triangles[(x * (yDots - 1) + y) * 6 + 3] = lowerRightIndex;
                    triangles[(x * (yDots - 1) + y) * 6 + 4] = upperLeftIndex;
                    triangles[(x * (yDots - 1) + y) * 6 + 5] = upperRightIndex;
                }
            }
            
            _mesh = new Mesh
            {
                name = "ColoredPlane",
                vertices = vertices,
                triangles = triangles,
                colors = colors
            };
            meshFilter.mesh = _mesh;
        }
        
        public void SetDotColor(int x, int y, Color color)
        {
            if (x < 0 || x >= _xDots || y < 0 || y >= _yDots)
            {
                Debug.LogError("Dot index must be in [0, dots count)");
                return;
            }
            var colors = _mesh.colors;
            colors[x * _yDots + y] = color;
            _mesh.colors = colors;
        }
    }
}
