using System.Collections.Generic;
using UnityEngine;

public class BackgroundSwitcher : BaseSceneManager<BackgroundSwitcher>
{
	[SerializeField]
	private List<GameObject> backgrounds;

	[SerializeField]
	private List<GameObject> feedingBackgrounds;

	[SerializeField]
	private GameObject fog;

	private void Awake()
	{
		if ((float)Screen.height / (float)Screen.width > 2f)
		{
			base.transform.localScale = Vector3.one * 812f / 667f;
		}
	}

	public void SetBackground(Level.FruitType fruit, bool isFog)
	{
		if (isFog)
		{
			fog.SetActive(value: true);
			return;
		}
		for (int i = 0; i < backgrounds.Count; i++)
		{
			if (i == (int)fruit)
			{
				backgrounds[i].SetActive(value: true);
			}
		}
	}

	public void SetFeedingLevelBackground(int level)
	{
		foreach (GameObject feedingBackground in feedingBackgrounds)
		{
			feedingBackground.SetActive(value: false);
		}
		for (int i = 0; i < feedingBackgrounds.Count; i++)
		{
			if (i == level % feedingBackgrounds.Count)
			{
				feedingBackgrounds[i].SetActive(value: true);
			}
		}
	}
}
