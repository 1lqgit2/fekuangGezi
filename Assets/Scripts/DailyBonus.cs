using Spine.Unity;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DailyBonus : MonoBehaviour
{
	[SerializeField]
	private SkeletonGraphic bird;

	[SerializeField]
	private SkeletonGraphic wormBound;

	[SerializeField]
	private GameObject connection;

	[SerializeField]
	private GameObject timeLeft;

	[SerializeField]
	private Text timeLeftText;

	[SerializeField]
	private GameObject getButton;

	[SerializeField]
	private GameObject playEveryDay;

	[SerializeField]
	private GameObject prize;

	[SerializeField]
	private Text prizeText;

	[SerializeField]
	private GameObject tapToEat;

	[SerializeField]
	private AudioSource click;

	[SerializeField]
	private AudioSource chvak;

	[SerializeField]
	private ParticleSystem pshik;

	[SerializeField]
	private AudioSource fireworks;

	[SerializeField]
	private ParticleSystem konfeti;

	private bool getClicked;

	private void OnEnable()
	{
		tapToEat.SetActive(value: false);
		wormBound.gameObject.SetActive(value: false);
		connection.SetActive(value: false);
		timeLeft.SetActive(value: false);
		getButton.SetActive(value: false);
		playEveryDay.SetActive(value: false);
		prize.SetActive(value: false);
		bird.Initialize(overwrite: false);
		bird.Skeleton.SetSkin(BaseGameManager<SkinManager>.GetInstance().GetCurrentSkinName());
		bird.AnimationState.SetAnimation(0, "bonus_idle", loop: true);
		konfeti.gameObject.SetActive(value: true);
		pshik.gameObject.SetActive(value: true);
		StartCoroutine(DailyBonusScenario());
	}

	private void OnDisable()
	{
		konfeti.gameObject.SetActive(value: false);
		pshik.gameObject.SetActive(value: false);
	}

	private IEnumerator DailyBonusScenario()
	{
		connection.SetActive(value: true);
		while ((float)BaseGameManager<OnlineManager>.GetInstance().GetTimeTillBonus() == -1f)
		{
			yield return null;
		}
		connection.SetActive(value: false);
		while (true)
		{
			playEveryDay.SetActive(value: true);
			timeLeft.SetActive(value: true);
			for (int rounded4 = Mathf.RoundToInt(BaseGameManager<OnlineManager>.GetInstance().GetTimeTillBonus()); rounded4 > 0; rounded4 = Mathf.RoundToInt(BaseGameManager<OnlineManager>.GetInstance().GetTimeTillBonus()))
			{
				int seconds = rounded4 % 60;
				rounded4 -= seconds;
				rounded4 /= 60;
				int minutes = rounded4 % 60;
				rounded4 -= minutes;
				int hours = rounded4 / 60;
				timeLeftText.text = hours.ToString("D2") + ":" + minutes.ToString("D2") + ":" + seconds.ToString("D2");
				yield return null;
			}
			playEveryDay.SetActive(value: false);
			timeLeft.SetActive(value: false);
			wormBound.gameObject.SetActive(value: true);
			wormBound.Initialize(overwrite: false);
			yield return new WaitForSpineAnimationComplete(wormBound.AnimationState.SetAnimation(0, "appear", loop: false));
			wormBound.AnimationState.SetAnimation(0, "idle", loop: true);
			tapToEat.SetActive(value: true);
			int reward = BaseGameManager<OnlineManager>.GetInstance().GetRewardSize();
			int rewardTaken = 0;
			while (rewardTaken < reward)
			{
				if (Input.GetMouseButtonDown(0) || UnityEngine.Input.GetKeyDown(KeyCode.Space))
				{
					bird.AnimationState.SetAnimation(0, "bonus_eat", loop: false);
					bird.AnimationState.AddAnimation(0, "bonus_idle", loop: true, 0f);
					wormBound.AnimationState.SetAnimation(0, "hit", loop: false);
					wormBound.AnimationState.AddAnimation(0, "idle", loop: true, 0f);
					chvak.Play();
					pshik.Play();
					rewardTaken++;
					tapToEat.SetActive(value: false);
					prize.SetActive(value: true);
					prizeText.text = rewardTaken.ToString();
				}
				yield return null;
			}
			yield return new WaitForSpineAnimationComplete(wormBound.AnimationState.SetAnimation(0, "hide", loop: false));
			getButton.SetActive(value: true);
			yield return new WaitUntil(() => getClicked);
			getClicked = false;
			getButton.SetActive(value: false);
			prize.SetActive(value: false);
			konfeti.Play();
			fireworks.Play();
			BaseGameManager<OnlineManager>.GetInstance().ClaimBonus();
			BaseGameManager<MainManager>.GetInstance().CoinsEarned(10);
		}
	}

	public void GetClicked()
	{
		click.Play();
		getClicked = true;
	}

	public void CloseDailyBonus()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.CloseDailyBonus();
	}
}
