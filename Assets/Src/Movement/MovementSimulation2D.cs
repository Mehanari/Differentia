using System;
using MehaMath;
using UnityEngine;

namespace Src.Movement
{
    public class MovementSimulation2D : SimulationBase
    {
        [Header("Simulation parameters")]
        [SerializeField] private Vector2 initialVelocity;
        [SerializeField] private float g;
        [SerializeField] private Vector2 initialPosition;
        [SerializeField] private float floorHeight;
        [SerializeField] private float airResistance;
        [Header("Visualisation")]
        [SerializeField] private GameObject ballWithResistance;
        [SerializeField] private GameObject ballNoAirResistance;

        private Vector2[] _positionsWithResistance;
        private Vector2[] _positionsNoAirResistance;
        private Vector3 _ballWithResistanceInitialPosition;
        private Vector3 _ballNoAirResistanceInitialPosition;
        
        protected override void Start()
        {
            base.Start();
            _positionsWithResistance = CalculatePositions(airResistance);
            _positionsNoAirResistance = CalculatePositions();
            _ballWithResistanceInitialPosition = ballWithResistance.transform.position;
            _ballNoAirResistanceInitialPosition = ballNoAirResistance.transform.position;
        }

        protected override void SetSimulationState(int stateIndex)
        {
            SetBallsPositions(stateIndex);
        }
        
        private void SetBallsPositions(int positionIndex)
        {
            var positionWithResistance = _positionsWithResistance[positionIndex];
            var positionNoAirResistance = _positionsNoAirResistance[positionIndex];
            ballWithResistance.transform.position = _ballWithResistanceInitialPosition 
                                                    + new Vector3(positionWithResistance.x, positionWithResistance.y, 0);
            ballNoAirResistance.transform.position = _ballNoAirResistanceInitialPosition 
                                                     + new Vector3(positionNoAirResistance.x, positionNoAirResistance.y, 0);
        }
        

        private Vector2[] CalculatePositions(float airResistance = 0)
        {
            var velocities = new Vector2[samplesCount];
            velocities[0] = initialVelocity;
            var positions = new Vector2[samplesCount];
            positions[0] = initialPosition;
            Func<float, float> nextYVelocity = 
                (previousYVelocity) => previousYVelocity + TimeStep * (-g - airResistance * previousYVelocity);
            Func<float, float> nextXVelocity =
                (previousXVelocity) => previousXVelocity + TimeStep * (- airResistance * previousXVelocity);
            Func<Vector2, Vector2> nextVelocity =
                (previousVelocity) => new Vector2(nextXVelocity(previousVelocity.x), nextYVelocity(previousVelocity.y));
            for (int i = 1; i < samplesCount; i++)
            {
                var nextVelocityValue = nextVelocity(velocities[i-1]);
                var nextPosition = positions[i-1] + TimeStep * nextVelocityValue;
                if (nextPosition.y < floorHeight)
                {
                    nextPosition.y = floorHeight;
                    nextVelocityValue.y = -nextVelocityValue.y;
                }
                positions[i] = nextPosition;
                velocities[i] = nextVelocityValue;
            }
            
            return positions;
        }
    }
}
