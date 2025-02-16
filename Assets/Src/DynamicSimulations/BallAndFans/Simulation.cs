using System.Collections.Generic;
using Src.Figures;
using Src.ForceFields;
using UnityEngine;
using Touch = Src.Figures.Touch;

namespace Src.DynamicSimulations.BallAndFans
{
    /// <summary>
    /// This is a real-time simulation of a dot that bounces from obstacles and is impacted
    /// by force fields (fans).
    /// </summary>
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private float airResistance = 0.01f;
        [SerializeField] private float ballMass = 1f;
        [SerializeField] private float hitSpeedLossPercent = 0.01f;
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
            var gravityVector = new Vector2(0, -gravity);
            var deltaTime = Time.fixedDeltaTime;

            while (deltaTime > 0)
            {
                var currentPosition = _ballPosition;
                var currentVelocity = _ballVelocity;
                var nextPosition = currentPosition + deltaTime * currentVelocity;
                var displacementVector = nextPosition - currentPosition;
                var initialDisplacementVectorLength = displacementVector.magnitude;
                var totalDeepening = 0f;
                var reflectionNormal = Vector2.zero;
                foreach (var obstacle in obstacles)
                {
                    if (obstacle.IsTouching(nextPosition, out var touch))
                    {
                        var touchNormal = touch.Vector;
                        reflectionNormal = touchNormal; //We use the last touch normal to reflect speed vector.
                        var angle = VectorUtils.MinAngleRad(displacementVector, touchNormal);
                        var deepening = touchNormal.magnitude / Mathf.Cos(angle);
                        totalDeepening += deepening;
                        var deepeningPercent = deepening / displacementVector.magnitude;
                        displacementVector *= (1 - deepeningPercent);
                        nextPosition = currentPosition + displacementVector;
                    }
                }

                var timeBeforeContact = deltaTime * (1f - totalDeepening / initialDisplacementVectorLength);
                if (initialDisplacementVectorLength == 0f)
                {
                    timeBeforeContact = deltaTime;
                }
                var nextVelocity = currentVelocity + timeBeforeContact * (gravityVector - airResistance * currentVelocity);
                foreach (var forceField in forces)
                {
                    var force = forceField.GetForce(currentPosition);
                    nextVelocity += (Vector2) ( timeBeforeContact * (force / ballMass) );
                }
                //If there was a contact (collision), then there must be a reflection normal and we must reflect
                //the velocity vector.
                //We also apply hit speed loss here.
                if (reflectionNormal.magnitude > 0)
                {
                    nextVelocity = -(2 * (Vector2.Dot(nextVelocity, reflectionNormal)
                                          / Mathf.Pow(reflectionNormal.magnitude, 2))
                                       * reflectionNormal - nextVelocity);
                    nextVelocity *= (1 - hitSpeedLossPercent);
                }

                _ballPosition = nextPosition;
                _ballVelocity = nextVelocity;
                deltaTime -= timeBeforeContact; 
            }
            
            ball.transform.position = _ballPosition;
        }
    }
}
