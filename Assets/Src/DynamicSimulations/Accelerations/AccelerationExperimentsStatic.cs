using Src.VisualisationTools.Plotting;
using UnityEngine;

namespace Src.DynamicSimulations.Accelerations
{
    public class AccelerationExperimentsStatic : SimulationBase
    {
        [SerializeField] private Transform curvatureCenter;
        [SerializeField] private float velocityModule;
        [SerializeField] private Vector3 direction;
        [SerializeField] private Transform point;
        [SerializeField] private Plotter2D plotter2D;

        private Vector3[] _velocities;
        private Vector3[] _positions;
        private float[] _radii; //Radius should be constant, but in dynamic simulation it changes for some reason.

        protected override void Start()
        {
            base.Start();
            _velocities = new Vector3[samplesCount];
            _positions = new Vector3[samplesCount];
            _radii = new float[samplesCount];
            direction = direction.normalized;
            _velocities[0] = velocityModule * direction;
            _positions[0] = point.position;
            _radii[0] = Vector3.Distance(curvatureCenter.position, _positions[0]);
            CalculateStates();
            plotter2D.PlotLogarithmic(0, simulationTime, _radii, "Radii", Color.red, iterationStep: 100);
            SetSimulationState(0);
        }

        private void CalculateStates()
        {
            var curvatureCenterPosition = curvatureCenter.position;
            for (int i = 1; i < samplesCount; i++)
            {
                var previousPosition = _positions[i - 1];
                var previousVelocity = _velocities[i - 1];
                var radiusVector = curvatureCenterPosition - previousPosition;
                var radius = (radiusVector).magnitude;
                var normalAccelerationModule = velocityModule * velocityModule / radius;
                var normalAccelerationDirection = radiusVector.normalized;
                var velocityShifted = previousVelocity +
                                      TimeStep * normalAccelerationDirection * normalAccelerationModule;
                var nextVelocity = velocityShifted.normalized * velocityModule;
                var nextPosition = previousPosition + nextVelocity * TimeStep;
                _positions[i] = nextPosition;
                _velocities[i] = nextVelocity;

                _radii[i] = Vector3.Distance(_positions[i], curvatureCenterPosition);
            }
        }

        protected override void SetSimulationState(int stateIndex)
        {
            var position = _positions[stateIndex];
            point.position = position;
        }
        
    }
}