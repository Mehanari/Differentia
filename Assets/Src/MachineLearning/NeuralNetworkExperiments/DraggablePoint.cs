using System;
using UnityEngine;

namespace Src.NeuralNetworkExperiments
{
    public class DraggablePoint : MonoBehaviour
    {
        [Header("Drag Settings")]
        [SerializeField] private bool isDraggable = true;
        [SerializeField] private float dragRadius = 0.5f;
    
        private Camera _mainCamera;
        private bool _isDragging = false;
        private Vector3 _dragOffset;
        private Collider2D _pointCollider;
    
        // Events for external components to listen to
        public event Action<Vector2> PointMoved;
        public event Action DragStarted;
        public event Action DragEnded;
    
        void Start()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
                _mainCamera = FindObjectOfType<Camera>();
            
            // Get or add collider for mouse detection
            _pointCollider = GetComponent<Collider2D>();
            if (_pointCollider == null)
            {
                CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
                circleCollider.radius = dragRadius;
                _pointCollider = circleCollider;
            }
        }
    
        void Update()
        {
            if (!isDraggable) return;
        
            HandleMouseInput();
        
            if (_isDragging)
            {
                UpdateDragPosition();
            }
        }
    
        void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = transform.position.z;
            
                // Check if mouse is over this point
                if (_pointCollider.OverlapPoint(mouseWorldPos))
                {
                    StartDrag(mouseWorldPos);
                }
            }
            else if (Input.GetMouseButtonUp(0) && _isDragging)
            {
                EndDrag();
            }
        }
    
        void StartDrag(Vector3 mouseWorldPos)
        {
            _isDragging = true;
            _dragOffset = transform.position - mouseWorldPos;
            DragStarted?.Invoke();
        }
    
        void UpdateDragPosition()
        {
            var mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = transform.position.z;
        
            Vector3 newPosition = mouseWorldPos + _dragOffset;
            transform.position = newPosition;
        
            // Notify listeners of position change
            PointMoved?.Invoke(transform.position);
        }
    
        void EndDrag()
        {
            _isDragging = false;
            DragEnded?.Invoke();
        }
    }
}