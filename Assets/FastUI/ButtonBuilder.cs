using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FastUI
{
	public class ButtonBuilder
	{
		private readonly RectTransform _rectTransform;
		private readonly Button _button;
		private readonly TextMeshProUGUI _label;
		private readonly Container _container;

		public ButtonBuilder(Button button, TextMeshProUGUI label, Container container, RectTransform rectTransform)
		{
			_button = button;
			_label = label;
			_container = container;
			_rectTransform = rectTransform;
		}

		public ButtonBuilder WithLabel(string label)
		{
			_label.text = label;
			return this;
		}

		public ButtonBuilder WithListener(UnityAction action)
		{
			_button.onClick.AddListener(action);
			return this;
		}

		public ButtonBuilder AtCorner(Corner corner)
		{
			_container.PlaceAtCorner(_rectTransform, corner);
			return this;
		}

		public Button Finish()
		{
			return _button;
		}
	}
}