using Spine.Unity;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
	private int currentlyStars = 3;

	[SerializeField]
	private Animator completed;

	[SerializeField]
	private Animator failed;

	[SerializeField]
	private GameObject pauseMenu;

	[SerializeField]
	private SkeletonGraphic birdButton;

	[SerializeField]
	private Text level;

	[SerializeField]
	private GameObject revive;

	[SerializeField]
	private SkeletonGraphic tutorial;

	[SerializeField]
	private SkeletonGraphic tap;

	[SerializeField]
	private AudioSource tutorialSoundGood;

	[SerializeField]
	private AudioSource tutorialSoundBad;

	[SerializeField]
	private GameObject tapToContinue;

	[SerializeField]
	private AudioSource click;

	[SerializeField]
	private Animator starsAnimator;

	[SerializeField]
	private Image star2;

	[SerializeField]
	private Image star3;

	[SerializeField]
	private AudioSource error;

	[SerializeField]
	private GameObject powerupBar;

	[SerializeField]
	private Image color;

	[SerializeField]
	private Image ice;

	[SerializeField]
	private Image fire;

	private void Start()
	{
		level.text = "关卡：" + (BaseSceneManager<Gameplay>.Instance.GetLevelNumber() + 1).ToString();
	}

	public void Completed()
	{
		completed.Play("Base Layer.Completed", 0, 0f);
	}

	public void Failed()
	{
		failed.Play("Base Layer.Completed", 0, 0f);
	}

	public void Pause()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.FadeOutIn(PauseAction);
	}

	private void PauseAction()
	{
		BaseSceneManager<Gameplay>.Instance.Pause();
		pauseMenu.SetActive(value: true);
		birdButton.Initialize(overwrite: false);
		birdButton.AnimationState.SetAnimation(0, "birds_appear", loop: false);
		birdButton.AnimationState.AddAnimation(0, "birds_idle", loop: true, 0f);
	}

	private void Update()
	{
		if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
		{
			if (pauseMenu.activeInHierarchy)
			{
				UnPause();
			}
			else
			{
				Pause();
			}
		}
	}

	public void UnPause()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.FadeOutIn(UnPauseAction);
	}

	private void UnPauseAction()
	{
		pauseMenu.SetActive(value: false);
		BaseSceneManager<Gameplay>.Instance.UnPause();
	}

	public void MyBirds()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.Restart(RestartType.RT_Skins);
	}

	public void Levels()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.Restart(RestartType.RT_Levels);
	}

	public void Home()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.Restart(RestartType.RT_None);
	}

	public void ShowRevive()
	{
		click.Play();
		revive.SetActive(value: true);
		StartCoroutine(CloseReviveIn5Sec());
	}

	private IEnumerator CloseReviveIn5Sec()
	{
		yield return new WaitForSeconds(5f);
		if (revive.activeInHierarchy)
		{
			SkipRevive();
		}
	}

	public void SkipRevive()
	{
		click.Play();
		revive.SetActive(value: false);
		BaseSceneManager<Gameplay>.Instance.ShowFinalScreen(isFailed: true);
	}

	public void Revive()
	{
		click.Play();
		revive.SetActive(value: false);
		BaseGameManager<AdsManager>.GetInstance().ShowVideo(ReviveCallback);
	}

	private void ReviveCallback(bool success)
	{
		if (success)
		{
			BaseSceneManager<Gameplay>.Instance.Revive();
		}
		else
		{
			BaseSceneManager<Gameplay>.Instance.ShowFinalScreen(isFailed: true);
		}
	}

	public IEnumerator ShowTutorialCoroutine(Gameplay.TutorialType tutorialType)
	{
		tutorial.gameObject.SetActive(value: true);
		tutorial.Initialize(overwrite: false);
		tutorial.AnimationState.SetAnimation(0, "appear", loop: false);
		yield return new WaitForSeconds(0.5f);
		tapToContinue.SetActive(value: true);
		string animationName = "corn";
		float timer = 8.267f;
		switch (tutorialType)
		{
		case Gameplay.TutorialType.TT_GoodAndBad:
			tutorialSoundGood.Play();
			animationName = "corn2";
			break;
		case Gameplay.TutorialType.TT_Bomb:
			tutorialSoundBad.Play();
			animationName = "tnt";
			break;
		case Gameplay.TutorialType.TT_Worm:
			tutorialSoundGood.Play();
			animationName = "worm";
			break;
		}
		tutorial.AnimationState.SetAnimation(0, animationName, loop: false);
		while (timer > 0f)
		{
			timer -= Time.deltaTime;
			if (Input.GetMouseButtonDown(0))
			{
				break;
			}
			yield return null;
		}
		tutorialSoundGood.Stop();
		tutorialSoundBad.Stop();
		tapToContinue.SetActive(value: false);
		tutorial.AnimationState.SetAnimation(0, "disappear", loop: false);
		yield return new WaitForSeconds(0.5333f);
		tutorial.gameObject.SetActive(value: false);
	}

	public void SetTapVisible(bool visible)
	{
		tap.gameObject.SetActive(visible);
	}

	public void SetStarsProgress(int stars, float progress)
	{
		if (currentlyStars == 3 && stars == 2)
		{
			starsAnimator.Play("Base Layer.NoThirdStar", 0, 0f);
			error.Play();
		}
		else if (currentlyStars == 2 && stars == 1)
		{
			starsAnimator.Play("Base Layer.NoSecondStar", 0, 0f);
			error.Play();
		}
		currentlyStars = stars;
		if (currentlyStars == 3)
		{
			star3.fillAmount = progress;
		}
		else if (currentlyStars == 2)
		{
			star2.fillAmount = progress;
		}
	}

	public void SetPowerups(float colorProgress, float iceProgress, float fireProgress)
	{
		if (colorProgress > 0f || iceProgress > 0f || fireProgress > 0f)
		{
			powerupBar.SetActive(value: true);
			color.fillAmount = colorProgress;
			ice.fillAmount = iceProgress;
			fire.fillAmount = fireProgress;
		}
		else
		{
			powerupBar.SetActive(value: false);
		}
	}
}
