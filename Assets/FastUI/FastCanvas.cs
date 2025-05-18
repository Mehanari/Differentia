using System;

namespace FastUI
{
    public class FastCanvas : Container
    {
        public ButtonBuilder AddButton()
        {
            var (button, rectTransform, label) = ElementsCreator.CreateButton();
            PlaceAtCorner(rectTransform, Corner.TopLeft);
            return new ButtonBuilder(button, label, this, rectTransform);
        }
    }

    public enum Corner
    {
        TopLeft, 
        TopRight,
        BottomLeft,
        BottomRight
    }
}
