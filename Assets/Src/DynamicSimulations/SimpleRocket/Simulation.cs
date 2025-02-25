using UnityEngine;
using UnityEngine.UI;

namespace Src.DynamicSimulations.SimpleRocket
{
    /// <summary>
    /// Here I simulate a rocket according to the ideal rocket differential equation.
    /// Rocket is represented by a point (even though I will probably add some shape just for visuals),
    /// which starts off from the ground level (y == 0) and flies upwards.
    /// The rocket is subject to gravity and will eventually fall down.
    /// After hitting the ground the rocket just stops, there is no bounce simulation or any other complicated stuff.
    /// </summary>
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private Button launchButton;
        [SerializeField] private Transform rocket;
        [SerializeField] private float rocketMass;
        [SerializeField] private float fuelMass;
        [SerializeField] private float exhaustVelocity;
        [Tooltip("How much fuel is consumed per second")]
        [SerializeField] private float fuelConsumptionRate;
        [SerializeField] private float gravity = 9.81f;
        [SerializeField] private Vector3 exhaustDirection = Vector3.down;

        //I'm not really interested in changing these parameters, so they will be const.
        private readonly Vector3 ROCKET_INITIAL_POSITION = Vector3.zero;
        private readonly Vector3 ROCKET_INITIAL_VELOCITY = Vector3.zero;
        private readonly Vector3 GRAVITY_DIRECTION = Vector3.down;
        
        //Rocket state
        private Vector3 _position;
        private Vector3 _velocity;
        private float _mass;
        private bool _launched;

        private void Start()
        {
            rocket.position = ROCKET_INITIAL_POSITION;
            _position = ROCKET_INITIAL_POSITION;
            _velocity = ROCKET_INITIAL_VELOCITY;
            _mass = fuelMass + rocketMass;
            launchButton.onClick.AddListener(Launch);
        }

        private void Launch()
        {
            _launched = true;
        }

        private void FixedUpdate()
        {
            if(!_launched) return;
            var deltaTime = Time.fixedDeltaTime;
            var currentMass = _mass;
            var nextMass = _mass - deltaTime * fuelConsumptionRate;
            if (nextMass < rocketMass)
            {
                nextMass = rocketMass;
            }

            var currentVelocity = _velocity;
            //Variable below is a change of velocity caused ONLY by fuel exhaust during this time period.
            var velocityIncrease = -exhaustVelocity * exhaustDirection * Mathf.Log(currentMass / nextMass);
            var gravityImpact = deltaTime * gravity * GRAVITY_DIRECTION / 2;
            var nextVelocity = currentVelocity + velocityIncrease + gravityImpact;
            var nextPosition = _position + deltaTime * nextVelocity;
            if (nextPosition.y < 0f)
            {
                nextPosition = new Vector3(_position.x, 0f, _position.y);
                nextVelocity = Vector3.zero;
            }

            _mass = nextMass;
            _position = nextPosition;
            _velocity = nextVelocity;

            rocket.position = _position;
        }
    }
}
