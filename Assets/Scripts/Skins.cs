using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Skins : MonoBehaviour
{
	[SerializeField]
	private RectTransform skinHolder;

	[SerializeField]
	private float minScrollNoVideo;

	[SerializeField]
	private float minScrollVideo;

	[SerializeField]
	private float maxScroll;

	[SerializeField]
	private List<GameObject> birdHolders;

	[SerializeField]
	private List<GameObject> priceHolders;

	[SerializeField]
	private List<GameObject> yellowBacks;

	[SerializeField]
	private AudioSource click;

	private bool isPointerDown;

	private bool isPointerMoved;

	private Vector3 previousPointerPosition;

	private float currentSkinsSpeed;

	private Vector2 currentSkinsPosition;

	private float minScroll;

	[SerializeField]
	private GameObject newSkinScreen;

	[SerializeField]
	private Animator newSkinAnimator;

	[SerializeField]
	private SkeletonGraphic newSkinSkeleton;

	[SerializeField]
	private SkeletonGraphic transition;

	[SerializeField]
	private AudioSource tear;

	[SerializeField]
	private AudioSource magic;

	[SerializeField]
	private SkeletonGraphic rays;

	private void OnEnable()
	{
		for (int i = 0; i < SkinManager.skinsCount; i++)
		{
			birdHolders[i].GetComponentInChildren<SkeletonGraphic>().Initialize(overwrite: false);
			if (i == BaseGameManager<SkinManager>.GetInstance().GetCurrentSkin())
			{
				if (BaseGameManager<SkinManager>.GetInstance().GetWeaponType(i) == SkinManager.WeaponType.WT_Knife)
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "idle_knife", loop: true);
				}
				else if (BaseGameManager<SkinManager>.GetInstance().GetWeaponType(i) == SkinManager.WeaponType.WT_Pistol)
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "idle_glock", loop: true);
				}
				else if (BaseGameManager<SkinManager>.GetInstance().GetWeaponType(i) == SkinManager.WeaponType.WT_ShotGun)
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "idle_shotgun", loop: true);
				}
				else if (BaseGameManager<SkinManager>.GetInstance().GetWeaponType(i) == SkinManager.WeaponType.WT_Rifle)
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "idle_ak47", loop: true);
				}
				else
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "dance2", loop: true);
				}
				birdHolders[i].GetComponentInChildren<SkeletonGraphic>().transform.localScale = Vector3.one * 0.7f;
				birdHolders[i].GetComponentInChildren<SkeletonGraphic>().GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 30f);
			}
			else
			{
				if (BaseGameManager<SkinManager>.GetInstance().GetWeaponType(i) == SkinManager.WeaponType.WT_Knife)
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "idle_knife", loop: true);
				}
				else if (BaseGameManager<SkinManager>.GetInstance().GetWeaponType(i) == SkinManager.WeaponType.WT_Pistol)
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "idle_glock", loop: true);
				}
				else if (BaseGameManager<SkinManager>.GetInstance().GetWeaponType(i) == SkinManager.WeaponType.WT_ShotGun)
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "idle_shotgun", loop: true);
				}
				else if (BaseGameManager<SkinManager>.GetInstance().GetWeaponType(i) == SkinManager.WeaponType.WT_Rifle)
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "idle_ak47", loop: true);
				}
				else
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "shop", loop: true);
				}
				birdHolders[i].GetComponentInChildren<SkeletonGraphic>().transform.localScale = Vector3.one * 0.6f;
				birdHolders[i].GetComponentInChildren<SkeletonGraphic>().GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f);
			}
			if (BaseGameManager<SkinManager>.GetInstance().IsSkinUnlocked(i))
			{
				birdHolders[i].GetComponentInChildren<SkeletonGraphic>().color = Color.white;
				priceHolders[i].GetComponentInChildren<BoneFollowerGraphic>().gameObject.SetActive(value: false);
				yellowBacks[i].SetActive(value: false);
				continue;
			}
			birdHolders[i].GetComponentInChildren<SkeletonGraphic>().color = Color.black;
			priceHolders[i].gameObject.SetActive(value: true);
			if (i != 2 && i != 12 && i != 4 && i != 14)
			{
				priceHolders[i].GetComponentInChildren<Text>().text = BaseGameManager<SkinManager>.GetInstance().GetSkinPrice(i).ToString();
			}
			else
			{
				priceHolders[i].GetComponentInChildren<Text>().text = BaseGameManager<SkinManager>.GetInstance().GetVideosLeftForVideoSkin(i).ToString();
			}
			yellowBacks[i].SetActive(value: true);
		}
		minScroll = ((!BaseGameManager<AdsManager>.GetInstance().IsVideoReady()) ? minScrollNoVideo : minScrollVideo);
		currentSkinsPosition = new Vector2(0f, minScroll);
		currentSkinsSpeed = 0f;
	}

	public void VideoTapped()
	{
		if (BaseGameManager<AdsManager>.GetInstance().IsVideoReady())
		{
			BaseGameManager<AdsManager>.GetInstance().ShowVideo(VideoGiftCallback);
		}
	}

	private void VideoGiftCallback(bool success)
	{
		if (success)
		{
			BaseGameManager<MainManager>.GetInstance().CoinsEarned(10);
		}
	}

	private void Update()
	{
		if (isPointerDown)
		{
			Vector3 mousePosition = UnityEngine.Input.mousePosition;
			float num = mousePosition.y - previousPointerPosition.y;
			currentSkinsSpeed = num / Time.deltaTime;
			if (num > 0f)
			{
				isPointerMoved = true;
			}
			previousPointerPosition = UnityEngine.Input.mousePosition;
		}
		currentSkinsPosition.y += currentSkinsSpeed * Time.deltaTime;
		if (currentSkinsPosition.y > maxScroll)
		{
			currentSkinsPosition.y = maxScroll;
			currentSkinsSpeed = 0f;
		}
		else if (currentSkinsPosition.y < minScroll)
		{
			currentSkinsPosition.y = minScroll;
			currentSkinsSpeed = 0f;
		}
		float num2 = Time.deltaTime * 1000f;
		if (Mathf.Abs(currentSkinsSpeed) < num2)
		{
			currentSkinsSpeed = 0f;
		}
		else
		{
			currentSkinsSpeed -= num2 * Mathf.Sign(currentSkinsSpeed);
		}
		skinHolder.anchoredPosition = currentSkinsPosition;
		if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
		{
			CloseSkins();
		}
	}

	public void OnSkinClicked(int n)
	{
		if (isPointerMoved)
		{
			return;
		}
		if (BaseGameManager<SkinManager>.GetInstance().IsSkinUnlocked(n))
		{
			click.Play();
			BaseGameManager<SkinManager>.GetInstance().SetCurrentSkin(n);
			CloseSkins();
			return;
		}
		switch (n)
		{
		case 2:
			if (BaseGameManager<AdsManager>.GetInstance().IsVideoReady())
			{
				BaseGameManager<AdsManager>.GetInstance().ShowVideo(SkinFor1Videocallback);
			}
			return;
		case 12:
			if (BaseGameManager<AdsManager>.GetInstance().IsVideoReady())
			{
				BaseGameManager<AdsManager>.GetInstance().ShowVideo(SkinFor3Videocallback);
			}
			return;
		case 4:
			if (BaseGameManager<AdsManager>.GetInstance().IsVideoReady())
			{
				BaseGameManager<AdsManager>.GetInstance().ShowVideo(SkinFor5Videocallback);
			}
			return;
		case 14:
			if (BaseGameManager<AdsManager>.GetInstance().IsVideoReady())
			{
				BaseGameManager<AdsManager>.GetInstance().ShowVideo(SkinFor3_1Videocallback);
			}
			return;
		}
		if (BaseGameManager<MainManager>.GetInstance().GetCoins() >= BaseGameManager<SkinManager>.GetInstance().GetSkinPrice(n))
		{
			click.Play();
			BaseGameManager<MainManager>.GetInstance().CoinsSpent(BaseGameManager<SkinManager>.GetInstance().GetSkinPrice(n));
			BaseGameManager<SkinManager>.GetInstance().UnlockSkin(n);
			BaseGameManager<SkinManager>.GetInstance().SetCurrentSkin(n);
			StartCoroutine(NewSkinCoroutine(n));
			BaseGameManager<OnlineManager>.GetInstance().BirdUnlockedAchievement();
		}
	}

	private void SkinFor1Videocallback(bool success)
	{
		if (success)
		{
			VideoForVideoSkinWatched(2);
		}
	}

	private void SkinFor3Videocallback(bool success)
	{
		if (success)
		{
			VideoForVideoSkinWatched(12);
		}
	}

	private void SkinFor5Videocallback(bool success)
	{
		if (success)
		{
			VideoForVideoSkinWatched(4);
		}
	}

	private void SkinFor3_1Videocallback(bool success)
	{
		if (success)
		{
			VideoForVideoSkinWatched(14);
		}
	}

	private void VideoForVideoSkinWatched(int n)
	{
		BaseGameManager<SkinManager>.GetInstance().VideoWatchedForVideoSkin(n);
		if (BaseGameManager<SkinManager>.GetInstance().IsSkinUnlocked(n))
		{
			BaseGameManager<SkinManager>.GetInstance().SetCurrentSkin(n);
			StartCoroutine(NewSkinCoroutine(n));
			BaseGameManager<OnlineManager>.GetInstance().BirdUnlockedAchievement();
		}
		else
		{
			priceHolders[n].GetComponentInChildren<Text>().text = BaseGameManager<SkinManager>.GetInstance().GetVideosLeftForVideoSkin(n).ToString();
		}
	}

	public void OnPointerDown()
	{
		isPointerDown = true;
		isPointerMoved = false;
		previousPointerPosition = UnityEngine.Input.mousePosition;
	}

	public void OnPointerUp()
	{
		isPointerDown = false;
	}

	public void CloseSkins()
	{
		click.Play();
		transition.gameObject.SetActive(value: false);
		BaseSceneManager<Gameplay>.Instance.CloseSkins();
	}

	private IEnumerator NewSkinCoroutine(int n)
	{
		newSkinScreen.SetActive(value: true);
		newSkinAnimator.Play("Base Layer.BirdAppear", 0, 0f);
		newSkinSkeleton.Initialize(overwrite: false);
		newSkinSkeleton.Skeleton.SetSkin(BaseGameManager<SkinManager>.GetInstance().GetCurrentSkinName());
		newSkinSkeleton.AnimationState.SetAnimation(0, "dance2", loop: true);
		newSkinSkeleton.timeScale = 0f;
		float timer = 0f;
		float duration = 0.166f;
		Vector3 startPoint = birdHolders[n].transform.position;
		Vector3 finishPoint = newSkinScreen.transform.position;
		newSkinAnimator.transform.position = startPoint;
		while (timer < duration)
		{
			yield return null;
			timer += Time.deltaTime;
			if (timer > duration)
			{
				timer = duration;
			}
			newSkinAnimator.transform.position = new Vector3(Mathf.Lerp(startPoint.x, finishPoint.x, timer / duration), Mathf.Lerp(startPoint.y, finishPoint.y, timer / duration), Mathf.Lerp(startPoint.z, finishPoint.z, timer / duration));
		}
		newSkinSkeleton.timeScale = 1f;
		rays.AnimationState.SetAnimation(0, "appear", loop: false);
		magic.Play();
		yield return new WaitForSeconds(3f);
		yield return new WaitUntil(() => Input.GetMouseButtonUp(0));
		birdHolders[n].GetComponentInChildren<SkeletonGraphic>().color = Color.white;
		priceHolders[n].GetComponentInChildren<BoneFollowerGraphic>().gameObject.SetActive(value: false);
		yellowBacks[n].SetActive(value: false);
		for (int i = 0; i < SkinManager.skinsCount; i++)
		{
			if (i == n)
			{
				if (BaseGameManager<SkinManager>.GetInstance().GetWeaponType(i) == SkinManager.WeaponType.WT_Knife)
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "idle_knife", loop: true);
				}
				else if (BaseGameManager<SkinManager>.GetInstance().GetWeaponType(i) == SkinManager.WeaponType.WT_Pistol)
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "idle_glock", loop: true);
				}
				else if (BaseGameManager<SkinManager>.GetInstance().GetWeaponType(i) == SkinManager.WeaponType.WT_ShotGun)
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "idle_shotgun", loop: true);
				}
				else if (BaseGameManager<SkinManager>.GetInstance().GetWeaponType(i) == SkinManager.WeaponType.WT_Rifle)
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "idle_ak47", loop: true);
				}
				else
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "dance2", loop: true);
				}
				birdHolders[i].GetComponentInChildren<SkeletonGraphic>().transform.localScale = Vector3.one * 0.7f;
				birdHolders[i].GetComponentInChildren<SkeletonGraphic>().GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 30f);
			}
			else
			{
				if (BaseGameManager<SkinManager>.GetInstance().GetWeaponType(i) == SkinManager.WeaponType.WT_Knife)
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "idle_knife", loop: true);
				}
				else if (BaseGameManager<SkinManager>.GetInstance().GetWeaponType(i) == SkinManager.WeaponType.WT_Pistol)
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "idle_glock", loop: true);
				}
				else if (BaseGameManager<SkinManager>.GetInstance().GetWeaponType(i) == SkinManager.WeaponType.WT_ShotGun)
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "idle_shotgun", loop: true);
				}
				else if (BaseGameManager<SkinManager>.GetInstance().GetWeaponType(i) == SkinManager.WeaponType.WT_Rifle)
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "idle_ak47", loop: true);
				}
				else
				{
					birdHolders[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "shop", loop: true);
				}
				birdHolders[i].GetComponentInChildren<SkeletonGraphic>().transform.localScale = Vector3.one * 0.6f;
				birdHolders[i].GetComponentInChildren<SkeletonGraphic>().GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f);
			}
		}
		transition.gameObject.SetActive(value: true);
		transition.Initialize(overwrite: false);
		TrackEntry page = transition.AnimationState.SetAnimation(0, "page_animation", loop: false);
		yield return null;
		newSkinScreen.SetActive(value: false);
		tear.Play();
		yield return new WaitForSpineAnimationComplete(page);
		transition.gameObject.SetActive(value: false);
	}
}
