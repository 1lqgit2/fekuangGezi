using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalScreenInfinite : MonoBehaviour
{
	[SerializeField]
	private GameObject backLoser;

	[SerializeField]
	private GameObject backWinner;

	[SerializeField]
	private GameObject restartLoser;

	[SerializeField]
	private GameObject restartWinner;

	[SerializeField]
	private Text header;

	[SerializeField]
	private Text scoreText;

	[SerializeField]
	private SkeletonGraphic shine;

	[SerializeField]
	private AudioSource shineSound;

	[SerializeField]
	private List<GameObject> birdHolders;

	[SerializeField]
	private SkeletonGraphic birdButton;

	[SerializeField]
	private AudioSource click;

	[SerializeField]
	private PhotoLab photoLab;

	private int score;

	private List<string> skinNames;

	public void Init(bool isRecord, int scoreBefore, int currentScore, List<string> currentSkinNames)
	{
		score = currentScore;
		skinNames = currentSkinNames;
		backLoser.SetActive(!isRecord);
		backWinner.SetActive(isRecord);
		restartLoser.SetActive(!isRecord);
		restartWinner.SetActive(isRecord);
		if (isRecord)
		{
			header.text = "新纪录";
			shine.AnimationState.SetAnimation(0, "appear", loop: false);
			shineSound.Play();
		}
		else
		{
			header.text = "最高：" + scoreBefore.ToString();
		}
		scoreText.text = score.ToString();
		scoreText.color = ((!isRecord) ? Color.black : Color.white);
		GameObject gameObject = birdHolders[skinNames.Count - 1];
		gameObject.SetActive(value: true);
		SkeletonGraphic[] componentsInChildren = gameObject.GetComponentsInChildren<SkeletonGraphic>();
		string animationName = (!isRecord) ? ("small_fail" + UnityEngine.Random.Range(1, 3).ToString()) : ("small_dance" + UnityEngine.Random.Range(1, 3).ToString());
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Initialize(overwrite: false);
			componentsInChildren[i].Skeleton.SetSkin(skinNames[i]);
			componentsInChildren[i].AnimationState.SetAnimation(0, animationName, loop: true);
		}
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
		BaseGameManager<AdsManager>.GetInstance().ShowInterstitial();
	}

	public void Restart()
	{
		click.Play();
		BaseSceneManager<Feeding>.Instance.Restart(RestartType.RT_Feeding);
	}

	public void Home()
	{
		click.Play();
		BaseSceneManager<Feeding>.Instance.Restart(RestartType.RT_None);
	}

	public void Skins()
	{
		click.Play();
		BaseSceneManager<Feeding>.Instance.Restart(RestartType.RT_Skins);
	}

	public void SharePhoto()
	{
		photoLab.gameObject.SetActive(value: true);
		photoLab.SharePhotoInfinite(score, "Hey! I earned " + score.ToString() + " in feeding mode in Pigeon Pop!", skinNames);
	}
}
