using System.Collections.Generic;
using MehaMath.Figures;
using MehaMath.ForceFields;
using MehaMath.Math;
using UnityEngine;

namespace Src.DynamicSimulations.BallAndFans
{
    /// <summary>
    /// This is a real-time simulation of a dot that bounces from obstacles and is impacted
    /// by force fields (fans).
    /// </summary>
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private float frameIterationsLimit = 5;
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

        //Conditions for stopping.
        private const float MIN_TIME_BEFORE_CONTACT = 0.0001f;
        private const float MIN_CONTACT_VELOCITY = 0.1f;

        private void Start()
        {
            _ballVelocity = initialVelocity;
            _ballPosition = ball.transform.position;
        }

        private void FixedUpdate()
        {
            var gravityVector = new Vector2(0, -gravity);
            var deltaTime = Time.fixedDeltaTime;
            var iterations = 0;
            while (deltaTime > 0 && iterations < frameIterationsLimit)
            {
                iterations++;
                var currentPosition = _ballPosition;
                var startVelocity = _ballVelocity;
                var endVelocity = startVelocity + deltaTime * (gravityVector - airResistance * startVelocity);
                endVelocity = ApplyForceFields(endVelocity, currentPosition, deltaTime);
                var averageVelocity = (startVelocity + endVelocity) / 2; //We assume that during this time period ball is moving with constant acceleration.
                var nextPosition = currentPosition + deltaTime * averageVelocity;
                var displacementVector = nextPosition - currentPosition;
                var initialDisplacementVectorLength = displacementVector.magnitude;
                var totalDeepening = 0f;
                var reflectionNormal = Vector2.zero;
                foreach (var obstacle in obstacles)
                {
                    if (obstacle.IsTouching(nextPosition, out var touch))
                    {
                        var touchNormal = touch.Vector;
                        if (touchNormal.magnitude == 0)
                        {
                            continue;
                        }
                        reflectionNormal = touchNormal; //We use the last touch normal to reflect speed vector.
                        var angle = Vector2Utils.MinAngleRad(displacementVector, touchNormal);
                        var deepening = touchNormal.magnitude / Mathf.Cos(angle);
                        totalDeepening += deepening;
                    }
                }

                reflectionNormal = reflectionNormal.normalized;

                var timeBeforeContact = deltaTime * (1f - totalDeepening / initialDisplacementVectorLength);
                if (initialDisplacementVectorLength == 0f)
                {
                    timeBeforeContact = deltaTime;
                }

                if (timeBeforeContact < MIN_TIME_BEFORE_CONTACT && startVelocity.magnitude < MIN_CONTACT_VELOCITY)
                {
                    _ballVelocity = Vector2.zero;
                    _ballPosition = currentPosition;
                    break;
                }
                endVelocity = startVelocity + timeBeforeContact * (gravityVector - airResistance * startVelocity);
                endVelocity = ApplyForceFields(endVelocity, currentPosition, timeBeforeContact);
                averageVelocity = (startVelocity + endVelocity) / 2;
                nextPosition = currentPosition + timeBeforeContact * averageVelocity;
                //If there was a contact (collision), then there must be a reflection normal and we must reflect
                //the velocity vector.
                //We also apply hit speed loss here.
                if (reflectionNormal.magnitude > 0)
                {
                    endVelocity = -(2 * (Vector2.Dot(endVelocity, reflectionNormal)
                                         / Mathf.Pow(reflectionNormal.magnitude, 2))
                                      * reflectionNormal - endVelocity);
                    endVelocity *= (1 - hitSpeedLossPercent);
                }

                _ballPosition = nextPosition;
                _ballVelocity = endVelocity;
                deltaTime -= timeBeforeContact; 
            }
            
            ball.transform.position = _ballPosition;
        }

        private Vector2 ApplyForceFields(Vector2 velocity, Vector3 currentPosition, float time)
        {
            foreach (var forceField in forces)
            {
                var force = forceField.GetForce(currentPosition);
                velocity += (Vector2) ( time * (force / ballMass) );
            }

            return velocity;
        }
    }
}
