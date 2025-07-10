using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Src.MachineLearning.SVM
{
    public class Svm2D : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer dotPrefab;
        [SerializeField] private Collider2D _redRegion;
        [SerializeField] private Collider2D _blueRegion;
        
        private List<GameObject> _redDots = new();
        private List<GameObject> _blueDots = new();
        private Camera _camera;


        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                var worldPoint = (Vector2) _camera.ScreenToWorldPoint(Input.mousePosition);
                var colliders = new Collider2D[2];
                Physics2D.OverlapPointNonAlloc(worldPoint, colliders);
                if (colliders.Contains(_blueRegion))
                {
                    var dot = Instantiate(dotPrefab, worldPoint, Quaternion.identity);
                    if (colliders.Contains(_redRegion))
                    {
                        dot.color = Color.red;
                        _redDots.Add(dot.gameObject);
                    }
                    else
                    {
                        dot.color = Color.blue;
                        _blueDots.Add(dot.gameObject);
                    }
                }
            }
        }
    }
}
