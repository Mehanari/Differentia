using MehaMath.Math;
using UnityEngine;

namespace Src.DynamicSimulations.BallOnAString
{
    /// <summary>
    /// When analysing Example 1.6 in the book "Orbital Mechanics for Engineering Students" by Howard Curtis, I was
    /// confused by the fact that part of the velocity of a ball disappears when the string, to which it is attached, gets taut.
    /// As I understand, disappeared portion of velocity is converted into "force" - a state parameter of a string which describes
    /// how the movement of an attached object will change afterwards. I assume that in case of an inextensible string this force is applied
    /// instantly and is directed along the string.
    /// I'm making this simulation to better understand the example and perform some experiments.
    /// </summary>
    public class Simulation : MonoBehaviour
    {
        [Tooltip("If ball is too far from string mount, its position will be corrected on the start.")]
        [SerializeField] private Transform ball;
        [SerializeField] private Vector2 initialVelocity;
        [SerializeField] private Transform stringMount;
        [SerializeField] private LineRenderer stringRenderer;
        [SerializeField] private Color stringColor = Color.green;
        [SerializeField] private float stringWidth = 0.05f;
        [SerializeField] private float stringLength;
        
        //Ball state
        private Vector2 _ballPosition;
        private Vector2 _ballVelocity;

        private void Start()
        {
            stringRenderer.startColor = stringColor;
            stringRenderer.endColor = stringColor;
            stringRenderer.startWidth = stringWidth;
            stringRenderer.endWidth = stringWidth;
            
            var ballPosition = ball.position;
            var mountPosition = stringMount.position;
            if (Vector2.Distance(ballPosition, mountPosition) > stringLength)
            {
                var vector = ballPosition - mountPosition;
                var displacement = vector.normalized * stringLength;
                var correctedPosition = mountPosition + displacement;
                ball.position = correctedPosition;
                ballPosition = correctedPosition;
            }

            _ballPosition = ballPosition;
            _ballVelocity = initialVelocity;
        }

        private void FixedUpdate()
        {
            var deltaTime = Time.fixedDeltaTime;
            var mountPosition = (Vector2) stringMount.position;
            while (deltaTime > 0)
            {
                var currentPosition = _ballPosition;
                var currentVelocity = _ballVelocity;
                var nextPosition = currentPosition + deltaTime * currentVelocity;
                var nextVelocity = currentVelocity;
                if (Vector2.Distance(nextPosition, mountPosition) > stringLength)
                {
                    var correctedPosition = CorrectPosition(nextPosition, currentPosition);
                    var timeSpent = deltaTime * (correctedPosition - currentPosition).magnitude /
                                    (nextPosition - currentPosition).magnitude;
                    deltaTime = 0f;
                    var mVector = mountPosition - correctedPosition;
                    var normal = Vector2.Perpendicular(mVector);
                    var newVelocity = Vector2.Reflect(currentPosition, normal);
                    nextPosition = correctedPosition;
                    nextVelocity = newVelocity;
                }
                else
                {
                    deltaTime = 0f;
                }

                _ballPosition = nextPosition;
                _ballVelocity = nextVelocity;
            }

            ball.position = _ballPosition;
            RenderString();
        }

        private void RenderString()
        {
            var positions = new Vector3[]
            {
                (Vector3)_ballPosition,
                stringMount.position
            };
            stringRenderer.SetPositions(positions);
        }

        //Returns a point on the line stretched from currentPosition to nextPosition such that distance to this
        //point is equal (approximately) to stringLength.
        private Vector2 CorrectPosition(Vector2 nextPosition, Vector2 currentPosition)
        {
            var mountPosition = (Vector2) stringMount.position;
            var displacementVector = nextPosition - currentPosition;
            var mVector = mountPosition - currentPosition; //Vector from current position to the mount position.
            var angle = Vector2Utils.MinAngleRad(mVector, displacementVector);
            var m = mVector.magnitude;
            var l = stringLength;
            if (m > l)
            {
                m = l;
            }
            var angleCos = Mathf.Cos(angle);
            var correctDisplacementModule =
                Mathf.Sqrt(angleCos * angleCos * m * m - m * m + l * l) - Mathf.Abs(angleCos) * m;
            var correctDisplacement = displacementVector.normalized * correctDisplacementModule;
            var correctedPosition = currentPosition + correctDisplacement;
            return correctedPosition;
        }
    }
}
