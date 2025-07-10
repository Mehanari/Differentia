using UnityEngine;

namespace Src.Math
{
	public class LineRendererFactory
	{
		public static LineRenderer GetLineRenderer(Color color, float width, int pointsCount, Transform parent = null)
		{
			var line = new GameObject().AddComponent<LineRenderer>();
			if (parent)
			{
				line.transform.SetParent(parent);
			}
			line.transform.localPosition = Vector3.zero;
			line.positionCount = pointsCount;
			line.startWidth = width;
			line.endWidth = width;
			line.startColor = color;
			line.endColor = color;
			line.material = new Material(Shader.Find("Sprites/Default"));
			return line;
		}
	}
}