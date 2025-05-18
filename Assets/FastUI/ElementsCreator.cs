using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FastUI
{
	public static class ElementsCreator
	{
		public static (Button button, RectTransform rectTransform, TextMeshProUGUI label) CreateButton(Transform parent = null, string buttonText = "Button", float width = 160f, float height = 40f)
		{
			// Create the button GameObject
            GameObject buttonObject = new GameObject("Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            
            // Get and set up the RectTransform
            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(width, height);
            
            // Set parent if provided
            if (parent != null)
            {
                rectTransform.SetParent(parent, false);
            }
            
            // Configure Image component (button background)
            Image image = buttonObject.GetComponent<Image>();
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            
            // Try to get the default button sprite from resources
            Sprite defaultSprite = Resources.Load<Sprite>("UI/UISprite");
            if (defaultSprite != null)
            {
                image.sprite = defaultSprite;
            }
            else
            {
                // Fallback to Unity's default UI sprite
                image.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            }
            
            // Configure Button component
            Button button = buttonObject.GetComponent<Button>();
            
            // Set up default colors
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.highlightedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.disabledColor = new Color(0.78f, 0.78f, 0.78f, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            
            // Create text object as child
            GameObject lableGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            RectTransform labelRectTransform = lableGo.GetComponent<RectTransform>();
            labelRectTransform.SetParent(rectTransform, false);
            
            // Configure text transform to fill the button
            labelRectTransform.anchorMin = Vector2.zero;
            labelRectTransform.anchorMax = Vector2.one;
            labelRectTransform.offsetMin = Vector2.zero;
            labelRectTransform.offsetMax = Vector2.zero;
            
            // Configure TextMeshPro component
            TextMeshProUGUI label = lableGo.GetComponent<TextMeshProUGUI>();
            label.text = buttonText;
            label.color = Color.black;
            label.fontSize = 14;
            label.fontStyle = FontStyles.Normal;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Overflow;

            return (button, rectTransform, label);
		}
	}
}