using System.Collections.Generic;
using Src.Figures;
using Src.ForceFields;
using UnityEngine;
using Touch = Src.Figures.Touch;

namespace Src.DynamicSimulations.BallAndFans
{
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private float airResistance = 0.01f;
        [SerializeField] private float ballMass = 1f;
        [SerializeField] private float hitSpeedLoss = 0.01f;
        [SerializeField] private float gravity = 9.81f;
        [SerializeField] private Vector2 initialVelocity;
        [SerializeField] private GameObject ball;
        [SerializeField] private List<Rectangle2D> obstacles;
        [SerializeField] private List<ForceField> forces;

        private Vector2 _ballPosition;
        private Vector2 _ballVelocity;

        private void Start()
        {
            _ballVelocity = initialVelocity;
            _ballPosition = ball.transform.position;
        }

        private void FixedUpdate()
        {
            var deltaTime = Time.fixedDeltaTime;
            var gravityVector = new Vector2(0, -gravity);
            var touches = new List<Touch>();
            var anyInnerTouches = false;
            
            foreach (var obstacle in obstacles)
            {
                if (obstacle.IsTouching(_ballPosition, out var obstacleTouch))
                {
                    var normal = obstacleTouch.Vector;
                    var velocityReflection = 2 *
                                             (Vector3.Dot(_ballVelocity, normal)
                                              / Mathf.Pow(normal.magnitude, 2))
                                             * normal
                                             - (Vector3)_ballVelocity;
                    _ballVelocity = -(velocityReflection*(1-hitSpeedLoss));
                    touches.Add(obstacleTouch);
                    if (obstacleTouch.IsInner)
                    {
                        anyInnerTouches = true;
                    }
                }  
            }

            foreach (var force in forces)
            {
                _ballVelocity += deltaTime * (Vector2)force.GetForce(_ballPosition / ballMass);
            }

            var gravityUse = anyInnerTouches ? 0 : 1; //We do not apply gravity if there is an inner touch.
            _ballVelocity += deltaTime * (gravityVector * gravityUse - airResistance * _ballVelocity);
            
            _ballPosition += _ballVelocity * deltaTime;
            var innerTouchesCount = 0;
            var innerTouchesVectorSum = Vector2.zero;
            foreach (var touch in touches)
            {
                if (touch.IsInner)
                {
                    innerTouchesVectorSum += (Vector2) touch.Vector;
                    innerTouchesCount++;
                }
            }
            if (innerTouchesCount > 0)
            {
                var correction = new Vector2(innerTouchesVectorSum.x/innerTouchesCount, 
                    innerTouchesVectorSum.y/innerTouchesCount);
                _ballPosition += correction;
            }

            
            ball.transform.position = _ballPosition;
        }
    }
}
