using Spine;
using Spine.Unity;
using Spine.Unity.Modules.AttachmentTools;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
	[SerializeField]
	private Text level;

	[SerializeField]
	private SkeletonGraphic bird;

	[SerializeField]
	private SkeletonGraphic birdButton;

	[SerializeField]
	private SkeletonGraphic logo;

	[SerializeField]
	private AudioSource click;

	[SerializeField]
	private AudioSource shot;

	[SerializeField]
	private SkeletonGraphic dailyBonus;

	private bool dailyBonusActive;

	private void OnEnable()
	{
		level.text = "关卡：" + (BaseSceneManager<Gameplay>.Instance.GetLevelNumber() + 1).ToString();
		bird.Clear();
		bird.Initialize(overwrite: false);
		bird.Skeleton.SetSkin(BaseGameManager<SkinManager>.GetInstance().GetCurrentSkinName());
		if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_Rifle)
		{
			bird.AnimationState.SetAnimation(0, "idle_ak47", loop: true);
		}
		else if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_Pistol)
		{
			bird.AnimationState.SetAnimation(0, "idle_glock", loop: true);
		}
		else if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_ShotGun)
		{
			bird.AnimationState.SetAnimation(0, "idle_shotgun", loop: true);
		}
		else if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_Knife)
		{
			bird.AnimationState.SetAnimation(0, "idle_knife", loop: true);
		}
		else
		{
			bird.AnimationState.SetAnimation(0, "idle" + UnityEngine.Random.Range(1, 3).ToString(), loop: true);
		}
		birdButton.Initialize(overwrite: false);
		birdButton.AnimationState.SetAnimation(0, "birds_appear", loop: false);
		birdButton.AnimationState.AddAnimation(0, "birds_idle", loop: true, 0f);
		StartCoroutine(DailyBonusCoroutine());
		logo.AnimationState.Event += HandleEvent;
	}

	private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == "shot")
		{
			shot.Play();
		}
	}

	private void ChangeClothes()
	{
		Skeleton skeleton = bird.Skeleton;
		Skin skin = new Skin("custom skin");
		Skin skin2 = skeleton.Data.FindSkin("pigeon");
		int slotIndex = skeleton.FindSlotIndex("body");
		Attachment clone = skin2.GetAttachment(slotIndex, "body").GetClone(cloneMeshesAsLinked: true);
		clone.SetRegion(bird.SkeletonDataAsset.GetAtlasArray()[0].FindRegion("body_army2"));
		skin.SetAttachment(slotIndex, "body", clone);
		bird.Skeleton.SetSkin(skin);
	}

	private IEnumerator DailyBonusCoroutine()
	{
		dailyBonus.Initialize(overwrite: false);
		while (true)
		{
			if (BaseGameManager<OnlineManager>.GetInstance().GetRewardSize() > 0)
			{
				if (!dailyBonusActive)
				{
					dailyBonus.AnimationState.SetAnimation(0, "daily_bonus_active", loop: true);
					dailyBonusActive = true;
				}
			}
			else if (dailyBonusActive)
			{
				dailyBonus.AnimationState.SetAnimation(0, "daily_bonus", loop: true);
				dailyBonusActive = false;
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private void Update()
	{
		if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
		{
			click.Play();
			ShowCloseDialogAndroid();
		}
	}

	public void Play()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.Play();
	}

	public void OpenSkins()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.OpenSkins();
	}

	public void OpenLevels()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.OpenLevels();
	}

	public void OpenSettings()
	{
		click.Play();
		// 插屏广告在 GameSettings.OnEnable（设置面板真正打开时）触发，
		// 这样从任何入口打开设置都会弹，而不会被重复调用。
		BaseSceneManager<Gameplay>.Instance.OpenSettings();
	}

	public void OpenModes()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.OpenModes();
	}

	public void OpenDailyBonus()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.OpenDailyBonus();
	}

	public void CloseUpdate()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.CloseUpdate();
	}

	public void ShowLeaderboards()
	{
		click.Play();
		BaseGameManager<OnlineManager>.GetInstance().ShowLeaderboards();
	}

	public void ShowAchievements()
	{
		click.Play();
		BaseGameManager<OnlineManager>.GetInstance().ShowAchievements();
	}

	public void ShowCloseDialogAndroid()
	{
		MNPopup mNPopup = new MNPopup("Pigeon Pop", "Quit game?");
		mNPopup.AddAction("YES", delegate
		{
			Application.Quit();
		});
		mNPopup.AddDismissListener(delegate
		{
		});
		mNPopup.Show();
	}
}
