using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : BaseGameManager<MainManager>
{
	private int coins;

	private static RestartType currentRestartType;

	private void Awake()
	{
		coins = (PlayerPrefs.HasKey("coins") ? PlayerPrefs.GetInt("coins") : 0);
	}

	public int GetCoins()
	{
		return coins;
	}

	public void CoinsEarned(int earned)
	{
		if (earned > 0)
		{
			switch (earned)
			{
			case 1:
				BaseSceneManager<MoneyBar>.Instance.PlusOne();
				break;
			case 3:
				BaseSceneManager<MoneyBar>.Instance.PlusThree();
				break;
			case 5:
				BaseSceneManager<MoneyBar>.Instance.PlusFive();
				break;
			case 10:
				BaseSceneManager<MoneyBar>.Instance.PlusTen();
				break;
			}
			coins += earned;
			PlayerPrefs.SetInt("coins", coins);
			PlayerPrefs.Save();
		}
	}

	public void CoinsSpent(int spent)
	{
		coins -= spent;
		PlayerPrefs.SetInt("coins", coins);
		PlayerPrefs.Save();
	}

	public RestartType GetCurrentRestartType()
	{
		RestartType result = currentRestartType;
		currentRestartType = RestartType.RT_None;
		return result;
	}

	public void Restart(RestartType restartType)
	{
		Time.timeScale = 1f;
		BaseGameManager<MusicManager>.GetInstance().SetPitch(1f);
		currentRestartType = restartType;
		switch (restartType)
		{
		case RestartType.RT_Feeding:
			SceneManager.LoadScene("Feeding");
			break;
		case RestartType.RT_Football:
			SceneManager.LoadScene("Football");
			break;
		default:
			SceneManager.LoadScene("Scene1");
			break;
		}
	}
}
