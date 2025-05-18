using System;
using System.Collections.Generic;
using UnityEngine;

namespace FastUI
{
	[RequireComponent(typeof(RectTransform))]
	public class Container : MonoBehaviour
	{
		private readonly Dictionary<Corner, List<RectTransform>> _corners = new Dictionary<Corner, List<RectTransform>>()
		{
			{ Corner.TopLeft, new List<RectTransform>() },
			{ Corner.TopRight, new List<RectTransform>() },
			{ Corner.BottomLeft, new List<RectTransform>() },
			{ Corner.BottomRight, new List<RectTransform>() }
		};

		private RectTransform _containerRect;

		private void Awake()
		{
			_containerRect = GetComponent<RectTransform>();
		}

		public void PlaceAtCorner(RectTransform element, Corner corner)
		{
			if (_containerRect is null)
			{
				Debug.LogError("Cannot place an element. Container's rect transform is null. " +
				               "Do not place elements in the Awake method or before.");
			}
			
			if (element.parent != _containerRect)
			{
				element.SetParent(_containerRect, false);
			}

			var cornerElements = _corners[corner];
			if (!cornerElements.Contains(element))
			{
				cornerElements.Add(element);
			}

			var placement = GetPlacement(corner);
			placement.Apply(element);
		}

		private Placement GetPlacement(Corner corner)
		{
			return corner switch
			{
				Corner.TopLeft => GetTopLeftPlacement(),
				Corner.TopRight => GetTopRightPlacement(),
				Corner.BottomLeft => GetBottomLeftPlacement(),
				Corner.BottomRight => GetBottomRightPlacement(),
				_ => throw new ArgumentOutOfRangeException(nameof(corner), corner, null)
			};
		}

		private Placement GetTopLeftPlacement()
		{
			var cornerElements = _corners[Corner.TopLeft];
			var placement = Placement.ForCorner(Corner.TopLeft);

			if (cornerElements.Count <= 1) return placement;
			var lastElement = cornerElements[^2];
			placement.AnchoredPosition = new Vector2(
				0,
				lastElement.anchoredPosition.y - lastElement.rect.height
			);

			return placement;
		}

		private Placement GetTopRightPlacement()
		{
			var cornerElements = _corners[Corner.TopRight];
			var placement = Placement.ForCorner(Corner.TopRight);
			
			if (cornerElements.Count <= 1) return placement;
			var lastElement = cornerElements[^2];
			placement.AnchoredPosition = new Vector2(
				0,
				lastElement.anchoredPosition.y - lastElement.rect.height
			);

			return placement;
		}
		
		private Placement GetBottomLeftPlacement()
		{
			var cornerElements = _corners[Corner.BottomLeft];
			var placement = Placement.ForCorner(Corner.BottomLeft);

			if (cornerElements.Count <= 1) return placement;
			var lastElement = cornerElements[^2];
			placement.AnchoredPosition = new Vector2(
				0,
				lastElement.anchoredPosition.y + lastElement.rect.height
			);

			return placement;
		}

		private Placement GetBottomRightPlacement()
		{
			var cornerElements = _corners[Corner.BottomRight];
			var placement = Placement.ForCorner(Corner.BottomRight);

			if (cornerElements.Count <= 1) return placement;
			var lastElement = cornerElements[^2];
			placement.AnchoredPosition = new Vector2(
				0,
				lastElement.anchoredPosition.y + lastElement.rect.height
			);

			return placement;
		}
	}
}