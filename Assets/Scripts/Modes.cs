using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Modes : MonoBehaviour
{
	[SerializeField]
	private AudioSource click;

	[SerializeField]
	private Text feedingScoreText;

	[SerializeField]
	private Text feedingLevelText;

	[SerializeField]
	private List<int> feedingLevelValues;

	[SerializeField]
	private Text footballScoreText;

	[SerializeField]
	private Text footballLevelText;

	[SerializeField]
	private List<int> footballLevelValues;

	[SerializeField]
	private List<string> levelNames;

	private void Update()
	{
		if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
		{
			CloseModes();
		}
	}

	private void OnEnable()
	{
		int num = PlayerPrefs.HasKey("feedingScore") ? PlayerPrefs.GetInt("feedingScore") : 0;
		string text = string.Empty;
		for (int i = 0; i < feedingLevelValues.Count; i++)
		{
			if (num >= feedingLevelValues[i])
			{
				text = levelNames[i];
			}
		}
		feedingScoreText.text = num.ToString();
		feedingLevelText.text = text;
		int num2 = PlayerPrefs.HasKey("footballScore") ? PlayerPrefs.GetInt("footballScore") : 0;
		string text2 = string.Empty;
		for (int j = 0; j < footballLevelValues.Count; j++)
		{
			if (num2 >= footballLevelValues[j])
			{
				text2 = levelNames[j];
			}
		}
		footballScoreText.text = num2.ToString();
		footballLevelText.text = text2;
	}

	public void CloseModes()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.CloseModes();
	}

	public void StartFeeding()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.Restart(RestartType.RT_Feeding);
	}

	public void StartFootball()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.Restart(RestartType.RT_Football);
	}
}
