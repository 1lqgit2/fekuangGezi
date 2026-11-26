using UnityEngine;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
	[SerializeField]
	private Image soundsImage;

	[SerializeField]
	private Image musicImage;

	[SerializeField]
	private Text soundsText;

	[SerializeField]
	private Text musicText;

	[SerializeField]
	private Sprite soundsOnSprite;

	[SerializeField]
	private Sprite soundsOffSprite;

	[SerializeField]
	private Sprite musicOnSprite;

	[SerializeField]
	private Sprite musicOffSprite;

	[SerializeField]
	private AudioSource click;

	private void OnEnable()
	{
		UpdateSoundSettings();
		ShowInterstitialOnOpen();
	}

	/// <summary>
	/// 打开设置界面时展示一次插屏广告。
	/// 广告已加载 → 立刻弹；还在加载 / 处于抖音启动 15 秒保护期 → AdManager 内部排队，条件满足后自动弹；
	/// 30 秒内的重复请求会跳过，避免反复进出设置被连续打扰。
	/// </summary>
	private void ShowInterstitialOnOpen()
	{
		if (AdManager.Instance == null)
		{
			Debug.LogWarning("[广告] 场景里没有 AdManager，打开设置时无法展示插屏");
			return;
		}

		AdManager.Instance.ShowInterstitialAd();
	}

	private void Update()
	{
		if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
		{
			CloseSettings();
		}
	}

	private void UpdateSoundSettings()
	{
		bool flag = !PlayerPrefs.HasKey("shotsOn") || PlayerPrefs.GetInt("shotsOn") == 1;
		bool flag2 = !PlayerPrefs.HasKey("musicOn") || PlayerPrefs.GetInt("musicOn") == 1;
		soundsImage.sprite = ((!flag) ? soundsOffSprite : soundsOnSprite);
		musicImage.sprite = ((!flag2) ? musicOffSprite : musicOnSprite);
		soundsText.text = ((!flag) ? "音效：关" : "音效：开");
		musicText.text = ((!flag2) ? "音乐：关" : "音乐：开");
		BaseSceneManager<Gameplay>.Instance.UpdateSoundSettings();
	}

	public void SoundsClicked()
	{
		click.Play();
		PlayerPrefs.SetInt("shotsOn", (PlayerPrefs.HasKey("shotsOn") && PlayerPrefs.GetInt("shotsOn") != 1) ? 1 : 0);
		PlayerPrefs.Save();
		UpdateSoundSettings();
	}

	public void MusicClicked()
	{
		click.Play();
		PlayerPrefs.SetInt("musicOn", (PlayerPrefs.HasKey("musicOn") && PlayerPrefs.GetInt("musicOn") != 1) ? 1 : 0);
		PlayerPrefs.Save();
		UpdateSoundSettings();
	}

	public void Restore()
	{
		click.Play();
		BaseGameManager<AdsManager>.GetInstance().RestorePurchases();
	}

	public void CloseSettings()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.CloseSettings();
	}

	public void NoAds()
	{
		click.Play();
		BaseGameManager<AdsManager>.GetInstance().BuyNoAds();
	}

	public void ResetProgress()
	{
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
	}

	public void TestAds()
	{
		BaseGameManager<AdsManager>.GetInstance().TestAds();
	}
}
