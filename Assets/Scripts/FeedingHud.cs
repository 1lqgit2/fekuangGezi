using UnityEngine;
using UnityEngine.UI;

public class FeedingHud : MonoBehaviour
{
	[SerializeField]
	private Text scoreText;

	[SerializeField]
	private Text combo;

	private int currentComboLevel;

	private int currentScore;

	private void Start()
	{
		currentComboLevel = 0;
		currentScore = 0;
	}

	public void Shoot()
	{
		BaseSceneManager<Feeding>.Instance.Shoot();
	}

	private void Update()
	{
		if (currentScore != BaseSceneManager<Feeding>.Instance.GetScore())
		{
			scoreText.text = BaseSceneManager<Feeding>.Instance.GetScore().ToString();
			currentScore = BaseSceneManager<Feeding>.Instance.GetScore();
			scoreText.GetComponent<Animator>().Play("Base Layer.FeedScoreAdded", 0, 0f);
		}
	}

	public void Combo(int level)
	{
		if (level != currentComboLevel)
		{
			combo.GetComponent<Animator>().Play("Base Layer.Combo" + Mathf.Clamp(level, 0, 8).ToString(), 0, 0f);
			combo.text = "连击 " + level.ToString();
			currentComboLevel = level;
		}
	}
}
