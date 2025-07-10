using Src.Math;
using UnityEngine;

namespace Src.NeuralNetworkExperiments
{
    public class DraggableTriangle : MonoBehaviour
    {
        [Header("Triangle Points")] private Vector2 _pointA = new (-1f, 0f);
        private Vector2 _pointB = new (1f, 0f);
        private Vector2 _pointC = new (0f, 1.5f);

        [Header("Visual Settings")] 
        [SerializeField] private DraggablePoint vertexPrefab;
        [SerializeField] private SpriteRenderer pointPrefab;
        [SerializeField] private Color lineColor = Color.white;
        [SerializeField] private Color perpendicularsLineColor = Color.blue;
        [SerializeField] private float lineWidth = 0.05f;

        private readonly DraggablePoint[] _points = new DraggablePoint[3];
        private readonly GameObject[] _middlePoints = new GameObject[3];
        private LineRenderer _triangleLineRenderer;

        private GameObject _circumcenter;
        private LineRenderer[] _perpendiculars = new LineRenderer[3];

        void Start()
        {

            // Create LineRenderer for triangle
            _triangleLineRenderer = LineRendererFactory.GetLineRenderer(lineColor, lineWidth, 4);
            _triangleLineRenderer.positionCount = 4; // 4 _points to close the triangle
            _triangleLineRenderer.sortingOrder = 1;
            _triangleLineRenderer.gameObject.name = "Triangle";

            // Create point visual objects
            Vector2[] points = { _pointA, _pointB, _pointC };

            for (int i = 0; i < 3; i++)
            {
                _points[i] = CreatePoint(points[i], i, _triangleLineRenderer.transform);
                _points[i].PointMoved += OnPointMoved;
            }

            _middlePoints[0] = Instantiate(pointPrefab,  (_pointA + _pointB) / 2, Quaternion.identity, _triangleLineRenderer.transform).gameObject;
            _middlePoints[1] = Instantiate(pointPrefab,  (_pointB + _pointC) / 2, Quaternion.identity, _triangleLineRenderer.transform).gameObject; 
            _middlePoints[2] = Instantiate(pointPrefab,  (_pointC + _pointA) / 2, Quaternion.identity, _triangleLineRenderer.transform).gameObject;

            for (int i = 0; i < 3; i++)
            {
                _perpendiculars[i] = LineRendererFactory.GetLineRenderer(perpendicularsLineColor, lineWidth, 2, _triangleLineRenderer.transform);
            }

            _circumcenter = new GameObject("Circumcenter");
            _circumcenter.transform.parent = _triangleLineRenderer.transform;
            
            UpdateCircumcenter();
            UpdateTriangleLines();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < 3; i++)
            {
                _points[i].PointMoved -= OnPointMoved;
            }
        }



        private void OnPointMoved(Vector2 obj)
        {
            UpdateTrianglePoints();
            UpdateMiddlePoints();
            UpdateCircumcenter();
            UpdateTriangleLines();
        }
        
        private void UpdateCircumcenter()
        {
            var j = J();
            var shouldDraw = System.Math.Abs(j) >= 0.01f;
            _circumcenter.SetActive(shouldDraw);
            for (int i = 0; i < 3; i++)
            {
                _perpendiculars[i].gameObject.SetActive(shouldDraw);
            }

            if (shouldDraw)
            {
                var t = _pointA.x * _pointA.x + _pointA.y * _pointA.y - _pointB.x * _pointB.x - _pointB.y * _pointB.y;
                var u = _pointA.x * _pointA.x + _pointA.y * _pointA.y - _pointC.x * _pointC.x - _pointC.y * _pointC.y;
                var centerX = (-(_pointA.y - _pointB.y) * u + (_pointA.y - _pointC.y) * t) / (2 * j);
                var centerY = ((_pointA.x - _pointB.x) * u - (_pointA.x - _pointC.x) * t) / (2 * j);
                var circumcenter = new Vector3(centerX, centerY);

                _circumcenter.transform.position = circumcenter;

                for (int i = 0; i < 3; i++)
                {
                    _perpendiculars[i].SetPositions(new Vector3[]{_middlePoints[i].transform.position, circumcenter});
                }
            }
        }

        private float J()
        {
            return (_pointA.x - _pointB.x) * (_pointA.y - _pointC.y) -
                   (_pointA.x - _pointC.x) * (_pointA.y - _pointB.y);
        }
        
        DraggablePoint CreatePoint(Vector2 position, int index, Transform parent)
        {
            // Add a circle sprite renderer
            var point = Instantiate(vertexPrefab, parent);
            var pointObj = point.gameObject;
            pointObj.name = $"Point{(char)('A' + index)}";
            pointObj.transform.position = position;
            return point;
        }

        private void UpdateTrianglePoints()
        {
            _pointA = _points[0].transform.position;
            _pointB = _points[1].transform.position;
            _pointC = _points[2].transform.position;
        }

        private void UpdateMiddlePoints()
        {
            _middlePoints[0].transform.position = (_pointA + _pointB) / 2;
            _middlePoints[1].transform.position = (_pointB + _pointC) / 2;
            _middlePoints[2].transform.position = (_pointC + _pointA) / 2;
        }

        void UpdateTriangleLines()
        {
            if (_triangleLineRenderer != null)
            {
                Vector3[] positions = new Vector3[4];
                positions[0] = _pointA;
                positions[1] = _pointB;
                positions[2] = _pointC;
                positions[3] = _pointA; // Close the triangle

                _triangleLineRenderer.SetPositions(positions);
            }
        }
    }
}