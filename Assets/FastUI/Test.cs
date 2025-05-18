using UnityEngine;

namespace FastUI
{
	public class Test : MonoBehaviour
	{
		[SerializeField] private FastCanvas _canvas;

		private void Start()
		{
			_canvas.AddButton().WithListener(()=>Debug.Log("First")).WithLabel("First");
			_canvas.AddButton().WithListener(() => Debug.Log("Second")).WithLabel("Second");
			_canvas.AddButton().WithLabel("Third").WithListener(() => Debug.Log("Third"));
			_canvas.AddButton().WithLabel("Fourth").WithListener(() => Debug.Log("Fourth"));
		}
		
	}
}