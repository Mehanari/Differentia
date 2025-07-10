using System;
using System.Collections.Generic;
using Src.Math;
using UnityEngine;

namespace Src.Examples
{
    public class ScaledCircles : MonoBehaviour
    {
        private List<LineRenderer> _figures = new();
        private const int PointsCount = 10000;

        private void Start()
        {
            DrawFigure(JustACircle, Vector3.zero);
            DrawFigure(CircleWithConstantScale, Vector3.up*2.5f);
            DrawFigure(CircleWithQuadraticScale, Vector3.up*5);
            DrawFigure(CircleWithQuadraticScaleForBothCoordinates, (Vector3.up + Vector3.right)*5f);
        }

        private Vector3 JustACircle(double input)
        {
            var r = 1f;
            var angle = input * System.Math.PI * 2;
            var x = (float)System.Math.Cos(angle);
            var y = (float)System.Math.Sin(angle);
            return new Vector3(x, y, 0) * r;
        }

        private Vector3 CircleWithConstantScale(double input)
        {
            var xScaleFactor = 2f;
            var circlePoint = JustACircle(input);
            circlePoint.x *= xScaleFactor;
            return circlePoint;
        }

        private Vector3 CircleWithQuadraticScale(double input)
        {
            Func<float, float> scaleFactor = (x) => x * x;
            var circlePoint = JustACircle(input);
            circlePoint.x *= scaleFactor((float)circlePoint.x);
            return circlePoint;
        }

        private Vector3 CircleWithQuadraticScaleForBothCoordinates(double input)
        {
            Func<float, float> scaleFactor = (x) => x * x;
            var circlePoint = JustACircle(input);
            circlePoint.x *= scaleFactor((float)circlePoint.x);
            circlePoint.y *= scaleFactor(circlePoint.y);
            return circlePoint;
        }

        private void DrawFigure(Func<double, Vector3> pointsDefinition, Vector3 offset)
        {
            var step = 1d / (PointsCount - 1d);
            var points = new Vector3[PointsCount];
            var line = LineRendererFactory.GetLineRenderer(Color.yellow, 0.1f, PointsCount);
            for (int i = 0; i < PointsCount; i++)
            {
                points[i] = pointsDefinition(i * step) + offset;
            }
            line.SetPositions(points);
            _figures.Add(line);
        }
    }
}
