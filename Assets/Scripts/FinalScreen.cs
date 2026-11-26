using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalScreen : MonoBehaviour
{
	[SerializeField]
	private Text levelFailed;

	[SerializeField]
	private Text failed;

	[SerializeField]
	private Text levelCompleted;

	[SerializeField]
	private Text completed;

	[SerializeField]
	private SkeletonGraphic bird;

	[SerializeField]
	private SkeletonGraphic birdButton;

	[SerializeField]
	private GameObject restartButtonFail;

	[SerializeField]
	private GameObject restartButtonWin;

	[SerializeField]
	private GameObject continueButton;

	[SerializeField]
	private GameObject backLoser;

	[SerializeField]
	private GameObject backWinner;

	[SerializeField]
	private GameObject skipButton;

	[SerializeField]
	private List<SkeletonGraphic> starSkeletons;

	[SerializeField]
	private List<SkeletonGraphic> glowSkeletons;

	[SerializeField]
	private AudioSource click;

	[SerializeField]
	private AudioSource blink;

	[SerializeField]
	private GameObject shareButton;

	[SerializeField]
	private SkeletonGraphic shareAnimation;

	private bool isFailScreen;

	private void Update()
	{
		skipButton.SetActive(BaseGameManager<AdsManager>.GetInstance().IsVideoReady() && isFailScreen && BaseSceneManager<Gameplay>.Instance.GetLevelNumber() < 259 && BaseSceneManager<Gameplay>.Instance.GetLevelNumber() + 1 > BaseSceneManager<Gameplay>.Instance.GetLastLevelUnlocked());
		if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
		{
			MainMenu();
		}
	}

	public void SkipTapped()
	{
		BaseGameManager<AdsManager>.GetInstance().ShowVideo(VideoSkipCallback);
	}

	private void VideoSkipCallback(bool success)
	{
		skipButton.SetActive(value: false);
		if (success)
		{
			BaseSceneManager<Gameplay>.Instance.UnlockLevel(BaseSceneManager<Gameplay>.Instance.GetLevelNumber() + 1);
			BaseSceneManager<Gameplay>.Instance.SetLevelNumber(BaseSceneManager<Gameplay>.Instance.GetLevelNumber() + 1);
			Restart();
		}
	}

	public void Init(bool isFail, int starsBefore, int stars)
	{
		isFailScreen = isFail;
		levelFailed.gameObject.SetActive(isFail);
		failed.gameObject.SetActive(isFail);
		levelCompleted.gameObject.SetActive(!isFail);
		completed.gameObject.SetActive(!isFail);
		restartButtonFail.SetActive(isFail);
		continueButton.SetActive(!isFail);
		backLoser.SetActive(isFail);
		backWinner.SetActive(!isFail);
		skipButton.SetActive(isFail);
		restartButtonWin.SetActive(!isFail && stars < 3);
		shareButton.SetActive(!isFail);
		bird.Initialize(overwrite: false);
		bird.Skeleton.SetSkin(BaseGameManager<SkinManager>.GetInstance().GetCurrentSkinName());
		birdButton.Initialize(overwrite: false);
		if (BaseGameManager<SkinManager>.GetInstance().CanBuyNewBird())
		{
			birdButton.AnimationState.SetAnimation(0, "birds_appear_alert", loop: false);
			birdButton.AnimationState.AddAnimation(0, "birds_idle_alert", loop: false, 0f);
			birdButton.AnimationState.AddAnimation(0, "birds_idle_alert", loop: false, 0f);
			birdButton.AnimationState.AddAnimation(0, "birds_idle", loop: true, 0f);
		}
		else
		{
			birdButton.AnimationState.SetAnimation(0, "birds_appear", loop: false);
			birdButton.AnimationState.AddAnimation(0, "birds_idle", loop: true, 0f);
		}
		if (isFail)
		{
			for (int i = 0; i < 3; i++)
			{
				starSkeletons[i].Initialize(overwrite: false);
				starSkeletons[i].AnimationState.SetAnimation(0, "black", loop: true);
			}
		}
		else
		{
			for (int j = 0; j < 3; j++)
			{
				starSkeletons[j].Initialize(overwrite: false);
				if (j + 1 <= starsBefore)
				{
					starSkeletons[j].AnimationState.SetAnimation(0, "white", loop: true);
				}
				else
				{
					starSkeletons[j].AnimationState.SetAnimation(0, "black", loop: true);
				}
			}
		}
		if (isFail)
		{
			bird.AnimationState.SetAnimation(0, "fail" + UnityEngine.Random.Range(1, 6).ToString(), loop: true);
			levelFailed.text = "关卡：" + (BaseSceneManager<Gameplay>.Instance.GetLevelNumber() + 1).ToString();
			BaseSceneManager<Gameplay>.Instance.SetMoneyColor(Color.black);
		}
		else
		{
			bird.AnimationState.SetAnimation(0, "dance" + UnityEngine.Random.Range(1, 6).ToString(), loop: true);
			levelCompleted.text = "关卡：" + (BaseSceneManager<Gameplay>.Instance.GetLevelNumber() + 1).ToString();
			if (stars > starsBefore)
			{
				StartCoroutine(StarsShow(starsBefore, stars));
			}
			BaseSceneManager<Gameplay>.Instance.SetMoneyColor(Color.black);
		}
		if (!isFail)
		{
			shareAnimation.Initialize(overwrite: false);
			shareAnimation.Skeleton.SetSkin("pigeon");
			shareAnimation.AnimationState.SetAnimation(0, "share_btn_appear", loop: false);
			if (!PlayerPrefs.HasKey("sharingGifted"))
			{
				shareAnimation.AnimationState.AddAnimation(0, "share_btn", loop: true, 0f);
			}
		}
		BaseGameManager<AdsManager>.GetInstance().ShowInterstitial();
	}

	private IEnumerator StarsShow(int starsBefore, int stars)
	{
		yield return new WaitForSeconds(0.5025f);
		for (int i = starsBefore; i < stars; i++)
		{
			starSkeletons[i].AnimationState.SetAnimation(0, "appear", loop: false);
			glowSkeletons[i].AnimationState.SetAnimation(0, "appear", loop: false);
			yield return new WaitForSeconds(0.3f);
			blink.Play();
			yield return new WaitForSeconds(0.3f);
		}
		bool rate12Shown = PlayerPrefs.HasKey("rate12Shown");
		bool rate32Shown = PlayerPrefs.HasKey("rate32Shown");
		if (BaseSceneManager<Gameplay>.Instance.GetLevelNumber() > 10 && !rate12Shown)
		{
			PlayerPrefs.SetInt("rate12Shown", 1);
			CallShowRateUs();
		}
		else if (BaseSceneManager<Gameplay>.Instance.GetLevelNumber() > 30 && !rate32Shown)
		{
			PlayerPrefs.SetInt("rate32Shown", 1);
			CallShowRateUs();
		}
	}

	public void Continue()
	{
		click.Play();
		if (BaseSceneManager<Gameplay>.Instance.GetLevelNumber() < 259)
		{
			BaseSceneManager<Gameplay>.Instance.SetLevelNumber(BaseSceneManager<Gameplay>.Instance.GetLevelNumber() + 1);
			BaseSceneManager<Gameplay>.Instance.Restart(RestartType.RT_Gameplay);
		}
		else
		{
			BaseSceneManager<Gameplay>.Instance.ShowGamePassedScreen();
		}
	}

	public void Restart()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.Restart(RestartType.RT_Gameplay);
	}

	public void MainMenu()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.Restart(RestartType.RT_Levels);
	}

	public void Skins()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.Restart(RestartType.RT_Skins);
	}

	private void CallShowRateUs()
	{
		MNRateUsPopup mNRateUsPopup = new MNRateUsPopup("Pigeon Pop", "Enjoying our game? Please take a moment to rate it. Thanks for your feedback!", "Rate it", "No, thanks", "Later");
		mNRateUsPopup.SetAppleId("1335578273");
		mNRateUsPopup.SetAndroidAppUrl("https://play.google.com/store/apps/details?id=com.fortafygames.pigeonpop");
		mNRateUsPopup.AddDeclineListener(delegate
		{
			PlayerPrefs.SetInt("rateLevel12Shown", 1);
			PlayerPrefs.SetInt("rateLevel32Shown", 1);
			UnityEngine.Debug.Log("rate us declined");
		});
		mNRateUsPopup.AddRemindListener(delegate
		{
			UnityEngine.Debug.Log("remind me later");
		});
		mNRateUsPopup.AddRateUsListener(delegate
		{
			PlayerPrefs.SetInt("rate12Shown", 1);
			PlayerPrefs.SetInt("rate32Shown", 1);
			UnityEngine.Debug.Log("rate us!!!");
		});
		mNRateUsPopup.AddDismissListener(delegate
		{
			UnityEngine.Debug.Log("rate us dialog dismissed :(");
		});
		mNRateUsPopup.Show();
	}

	public void ShareTapped()
	{
		BaseSceneManager<Gameplay>.Instance.SharePhoto();
		if (!PlayerPrefs.HasKey("sharingGifted"))
		{
			StartCoroutine(Add10WormsIn1Second());
		}
	}

	private IEnumerator Add10WormsIn1Second()
	{
		yield return new WaitForSeconds(1f);
		BaseGameManager<MainManager>.GetInstance().CoinsEarned(10);
		PlayerPrefs.SetInt("sharingGifted", 1);
		shareAnimation.AnimationState.SetAnimation(0, "share_btn_idle", loop: false);
	}
}
