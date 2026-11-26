using UnityEngine;
using UnityEngine.UI;

public class UI : BaseSceneManager<UI>
{
	[SerializeField]
	private RectTransform uiMargins;

	private void Awake()
	{
		UpdateMargins();
	}

	public void UpdateMargins()
	{
		float y = 0f;
		float y2 = 170f;
		if ((float)Screen.height / (float)Screen.width > 2f)
		{
			y = -100f;
			y2 = 250f;
			GetComponent<CanvasScaler>().matchWidthOrHeight = 0f;
		}
		uiMargins.offsetMax = new Vector2(0f, y);
		uiMargins.offsetMin = new Vector2(0f, y2);
	}
}
