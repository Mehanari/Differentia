using System;
using UnityEngine;

namespace FastUI
{
	public struct Placement
	{
		public Vector2 AnchorMin { get; set; }
		public Vector2 AnchorMax { get; set; }
		public Vector2 Pivot { get; set; }
		public Vector2 AnchoredPosition { get; set; }

		public void Apply(RectTransform transform)
		{
			transform.pivot = Pivot;
			transform.anchorMin = AnchorMin;
			transform.anchorMax = AnchorMax;
			transform.anchoredPosition = AnchoredPosition;
		}

		/// <summary>
		/// Creates a placement for a given corner.
		/// Does not account for any other elements.
		/// </summary>
		/// <param name="corner"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static Placement ForCorner(Corner corner)
		{
			return corner switch
			{
				Corner.TopLeft => new Placement()
				{
					AnchorMin = new Vector2(0, 1),
					AnchorMax = new Vector2(0, 1),
					Pivot = new Vector2(0, 1),
					AnchoredPosition = new Vector2(0, 0)
				},
				Corner.TopRight => new Placement()
				{
					AnchorMin = new Vector2(1, 1),
					AnchorMax = new Vector2(1, 1),
					Pivot = new Vector2(1, 1),
					AnchoredPosition = new Vector2(0, 0)
				},
				Corner.BottomLeft => new Placement()
				{
					AnchorMin = new Vector2(0, 0),
					AnchorMax = new Vector2(0, 0),
					Pivot = new Vector2(0, 0),
					AnchoredPosition = new Vector2(0, 0)
				},
				Corner.BottomRight => new Placement()
				{
					AnchorMin = new Vector2(1, 0),
					AnchorMax = new Vector2(1, 0),
					Pivot = new Vector2(1, 0),
					AnchoredPosition = new Vector2(0, 0)
				},
				_ => throw new ArgumentOutOfRangeException(nameof(corner), corner, null)
			};
		}
	}
}