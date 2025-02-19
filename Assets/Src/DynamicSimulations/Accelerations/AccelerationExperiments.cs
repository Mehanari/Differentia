using UnityEngine;

namespace Src.DynamicSimulations.Accelerations
{
    public class AccelerationExperiments : MonoBehaviour
    {
        [SerializeField] private Transform curvatureCenter;
        [SerializeField] private float velocityModule;
        [SerializeField] private Vector3 direction;
        [SerializeField] private Transform point;

        private Vector3 _pointVelocity;

        private void Start()
        {
            direction = direction.normalized;
            _pointVelocity = velocityModule * direction;
        }

        private void FixedUpdate()
        {
            var curvatureCenterPosition = curvatureCenter.position;
            var pointPosition = point.position;
            var radiusVector = curvatureCenterPosition - pointPosition;
            var radius = (radiusVector).magnitude;
            var normalAccelerationModule = velocityModule * velocityModule / radius;
            var normalAccelerationDirection = radiusVector.normalized;
            var previousVelocity = _pointVelocity;
            _pointVelocity += Time.fixedDeltaTime * normalAccelerationDirection * normalAccelerationModule;
            _pointVelocity = _pointVelocity.normalized * velocityModule;
            Debug.Log("Velocity module: " + _pointVelocity.magnitude);
            var newPosition = pointPosition + Time.fixedDeltaTime * _pointVelocity;
            point.position = newPosition;
        }
        
    }
}
