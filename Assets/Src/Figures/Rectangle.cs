using System;
using UnityEngine;

namespace Src.Figures
{
    public class Rectangle2D : Figure
    {
        [SerializeField] private float Length;
        [SerializeField] private float Height;

        private void OnDrawGizmos()
        {
            Matrix4x4 rotationMatrix = pivot.localToWorldMatrix;
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(Length, Height, 0f));
        }

        public override bool IsTouching(Vector3 point, out Touch touch, float epsilon = 0.00001f)
        {
            var pivotPosition = pivot.position;
            var vector = pivotPosition - point;
            var toLocal = pivot.worldToLocalMatrix;
            var toWorld = pivot.localToWorldMatrix;
            var localVector = toLocal.MultiplyVector(vector);
            var inXRange = localVector.x <= Length / 2 + epsilon && localVector.x >= -Length / 2 - epsilon;
            var inYRange = localVector.y <= Height / 2 + epsilon && localVector.y >= -Height / 2 - epsilon;

            var isInner = false;
            var xOffset = localVector.x - Length / 2;
            if (localVector.x < 0)
            {
                xOffset = localVector.x + Length / 2;
            }
            var yOffset = localVector.y - Height / 2;
            if (localVector.y < 0)
            {
                yOffset = localVector.y + Height / 2;
            }
            var touchVector = new Vector3(xOffset, yOffset, 0f);
            if (inXRange)
            {
                touchVector.x = 0f;
            }
            if (inYRange)
            {
                touchVector.y = 0f;
            }
            if (inXRange && inYRange)
            {
                var xOffsetAbs = Mathf.Abs(xOffset);
                var yOffsetAbs = Mathf.Abs(yOffset);
                if (xOffsetAbs < yOffsetAbs)
                {
                    touchVector.x = xOffset;
                }
                else
                {
                    touchVector.y = yOffset;
                }

                isInner = true;
            }

            touchVector = toWorld.MultiplyVector(touchVector);
            touch = new Touch
            {
                Vector = touchVector,
                IsInner = isInner
            };
            
            return inXRange && inYRange;
        }
    }
}