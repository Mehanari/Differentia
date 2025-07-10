using UnityEngine;

namespace FastUI
{
	public class Test : MonoBehaviour
	{
		[SerializeField] private FastCanvas _canvas;

		private void Start()
		{
			_canvas.AddButton().WithListener(()=>Debug.Log("First")).WithLabel("First").AtCorner(Corner.BottomLeft);
			_canvas.AddButton().WithListener(() => Debug.Log("Second")).WithLabel("Second").AtCorner(Corner.BottomRight);
			_canvas.AddButton().WithLabel("Third").WithListener(() => Debug.Log("Third")).AtCorner(Corner.TopLeft);
			_canvas.AddButton().WithLabel("Fourth").WithListener(() => Debug.Log("Fourth")).AtCorner(Corner.TopRight);
		}
		
	}
}