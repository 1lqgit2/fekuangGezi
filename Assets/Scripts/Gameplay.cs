using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Gameplay : BaseSceneManager<Gameplay>
{
	public enum TutorialType
	{
		TT_None,
		TT_GoodAndBad,
		TT_Bomb,
		TT_Worm
	}

	[SerializeField]
	private List<Level> levels;

	[SerializeField]
	private List<AudioSource> jumpEmptySounds;

	[SerializeField]
	private List<AudioSource> eatSounds;

	[SerializeField]
	private StartScreen startScreen;

	[SerializeField]
	private Hud hud;

	[SerializeField]
	private Skins skins;

	[SerializeField]
	private MoneyBar moneyBar;

	[SerializeField]
	private GameSettings settings;

	[SerializeField]
	private Modes modes;

	[SerializeField]
	private DailyBonus dailyBonus;

	[SerializeField]
	private GameObject updateScreen;

	[SerializeField]
	private AudioSource colorSwitchSound;

	[SerializeField]
	private AudioSource wood;

	[SerializeField]
	private LevelScreen levelScreen;

	[SerializeField]
	private FinalScreen finalScreen;

	[SerializeField]
	private GamePassed gamePassedScreen;

	[SerializeField]
	private Image fade;

	[SerializeField]
	private SkeletonAnimation colorSwitch;

	[SerializeField]
	private SkeletonAnimation finalExplosion;

	[SerializeField]
	private AudioSource finalExplosionSound;

	[SerializeField]
	private ParticleSystem fruitParts;

	[SerializeField]
	private ParticleSystem fruitParts2;

	[SerializeField]
	private Transform pauseButton;

	public const int levelsCount = 260;

	private int currentLevelNumber;

	private int lastLevelUnlocked;

	private List<int> stars = new List<int>();

	private Level currentLevel;

	private bool isPaused = true;

	private bool revived;

	private bool trimphSecond;

	private bool isWormGold;

	private float timer;

	[SerializeField]
	private SkeletonAnimation space;

	private SkeletonAnimation UFO;

	private bool isUFOInTheSky;

	private List<Vector3> memories = new List<Vector3>();

	private List<float> angleMemories = new List<float>();

	private Coroutine ufoCoroutine;

	[SerializeField]
	private AudioSource ufoStartSound;

	[SerializeField]
	private AudioSource ufoAttackSound;

	[SerializeField]
	private AudioSource ufoIdleSound;

	[SerializeField]
	private GameObject freeze;

	private const float freezeLength = 5f;

	private float freezeTimer;

	private int fireCounter;

	private const int fireAmount = 6;

	[SerializeField]
	private AudioSource freezeSound;

	[SerializeField]
	private AudioMixer mixer;

	[SerializeField]
	private PhotoLab photoLab;

	/// <summary>
	/// 只允许在「主界面」显示的常驻 UI：抖音侧边栏按钮 cebianBtn、激励视频按钮 jiliBtn。
	/// 这两个按钮挂在常驻 Canvas 的 Margins 下，不归任何界面管，所以在运行时统一按首页显隐控制。
	/// </summary>
	[SerializeField]
	private List<GameObject> homeOnlyButtons;

	/// <summary>上一次应用的显隐状态，避免每帧写 SetActive</summary>
	private bool homeOnlyVisible = true;

	private RestartType currentRestartType;

	private void Awake()
	{
		Application.targetFrameRate = 60;
	}

	private void Start()
	{
		FadeIn();
		moneyBar.gameObject.SetActive(value: true);
		bool flag = PlayerPrefs.HasKey("stars") && !PlayerPrefs.HasKey("update04Shown");
		currentLevelNumber = (PlayerPrefs.HasKey("currentLevelNumber") ? PlayerPrefs.GetInt("currentLevelNumber") : 0);
		lastLevelUnlocked = (PlayerPrefs.HasKey("lastLevelUnlocked") ? PlayerPrefs.GetInt("lastLevelUnlocked") : 0);
		currentRestartType = BaseGameManager<MainManager>.GetInstance().GetCurrentRestartType();
		if (currentRestartType != RestartType.RT_Gameplay)
		{
			SetLevelNumber(Mathf.Clamp(lastLevelUnlocked, 0, 259));
		}
		string text = (!PlayerPrefs.HasKey("stars")) ? string.Empty : PlayerPrefs.GetString("stars");
		for (int i = 0; i < 260; i++)
		{
			stars.Add((i < text.Length) ? int.Parse(text.Substring(i, 1)) : 0);
		}
		if (flag)
		{
			OpenUpdateAction();
		}
		else if (currentRestartType == RestartType.RT_None)
		{
			startScreen.gameObject.SetActive(value: true);
		}
		else if (currentRestartType == RestartType.RT_Gameplay)
		{
			PlayAction();
		}
		else if (currentRestartType == RestartType.RT_Levels)
		{
			OpenLevelsAction();
		}
		else if (currentRestartType == RestartType.RT_Skins)
		{
			OpenSkinsAction();
		}
		PlayerPrefs.SetInt("update04Shown", 1);
		UpdateSoundSettings();
		RefreshWorm();
		ApplyHomeOnlyVisibility(force: true);
	}

	/// <summary>
	/// 主界面（首页 startScreen）是否正在显示。关卡选择 / 皮肤 / 设置 / 每日奖励 / 模式 / 更新 / 游戏内 / 结算
	/// 都会先关掉 startScreen，所以这一个判断就够覆盖「其余界面」。
	/// </summary>
	public bool IsHomeVisible => startScreen != null && startScreen.gameObject.activeInHierarchy;

	/// <summary>
	/// 让 homeOnlyButtons（cebianBtn、jiliBtn）跟随首页显隐。
	/// force=true 用于初始化，把当前状态强制同步一次。
	/// </summary>
	private void ApplyHomeOnlyVisibility(bool force = false)
	{
		bool visible = this.IsHomeVisible;
		if (!force && visible == this.homeOnlyVisible)
		{
			return;
		}

		this.homeOnlyVisible = visible;
		if (this.homeOnlyButtons == null)
		{
			return;
		}

		for (int i = 0; i < this.homeOnlyButtons.Count; i++)
		{
			GameObject button = this.homeOnlyButtons[i];
			if (button != null && button.activeSelf != visible)
			{
				button.SetActive(visible);
			}
		}
	}

	public void OpenSkins()
	{
		FadeOutIn(OpenSkinsAction);
	}

	private void OpenSkinsAction()
	{
		startScreen.gameObject.SetActive(value: false);
		skins.gameObject.SetActive(value: true);
	}

	public void CloseSkins()
	{
		FadeOutIn(CloseSkinsAction);
	}

	private void CloseSkinsAction()
	{
		startScreen.gameObject.SetActive(value: true);
		skins.gameObject.SetActive(value: false);
	}

	public void OpenLevels()
	{
		FadeOutIn(OpenLevelsAction);
	}

	private void OpenLevelsAction()
	{
		startScreen.gameObject.SetActive(value: false);
		levelScreen.gameObject.SetActive(value: true);
	}

	public void CloseLevels()
	{
		FadeOutIn(CloseLevelsAction);
	}

	private void CloseLevelsAction()
	{
		startScreen.gameObject.SetActive(value: true);
		levelScreen.gameObject.SetActive(value: false);
	}

	public void OpenSettings()
	{
		FadeOutIn(OpenSettingsAction);
	}

	private void OpenSettingsAction()
	{
		startScreen.gameObject.SetActive(value: false);
		settings.gameObject.SetActive(value: true);
	}

	public void CloseSettings()
	{
		FadeOutIn(CloseSettingsAction);
	}

	private void CloseSettingsAction()
	{
		startScreen.gameObject.SetActive(value: true);
		settings.gameObject.SetActive(value: false);
	}

	public void OpenModes()
	{
		FadeOutIn(OpenModesAction);
	}

	private void OpenModesAction()
	{
		startScreen.gameObject.SetActive(value: false);
		modes.gameObject.SetActive(value: true);
	}

	public void CloseModes()
	{
		FadeOutIn(CloseModesAction);
	}

	private void CloseModesAction()
	{
		startScreen.gameObject.SetActive(value: true);
		modes.gameObject.SetActive(value: false);
	}

	public void OpenDailyBonus()
	{
		FadeOutIn(OpenDailyBonusAction);
	}

	private void OpenDailyBonusAction()
	{
		startScreen.gameObject.SetActive(value: false);
		dailyBonus.gameObject.SetActive(value: true);
	}

	public void CloseDailyBonus()
	{
		FadeOutIn(CloseDailyBonusAction);
	}

	private void CloseDailyBonusAction()
	{
		startScreen.gameObject.SetActive(value: true);
		dailyBonus.gameObject.SetActive(value: false);
	}

	public void OpenUpdate()
	{
		FadeOutIn(OpenUpdateAction);
	}

	private void OpenUpdateAction()
	{
		startScreen.gameObject.SetActive(value: false);
		updateScreen.gameObject.SetActive(value: true);
	}

	public void CloseUpdate()
	{
		FadeOutIn(CloseUpdateAction);
	}

	private void CloseUpdateAction()
	{
		startScreen.gameObject.SetActive(value: true);
		updateScreen.gameObject.SetActive(value: false);
	}

	public void Play()
	{
		FadeOutIn(PlayAction);
	}

	private void PlayAction()
	{
		hud.gameObject.SetActive(value: true);
		startScreen.gameObject.SetActive(value: false);
		StartCoroutine(HenAppearCoroutine());
		BaseGameManager<OnlineManager>.GetInstance().GameStartedAchievement();
	}

	public void StartColorSwitch()
	{
		colorSwitch.gameObject.SetActive(value: true);
		colorSwitchSound.Play();
		colorSwitch.Initialize(overwrite: false);
		colorSwitch.AnimationState.SetAnimation(0, "appear", loop: false);
		colorSwitch.AnimationState.AddAnimation(0, "idle", loop: true, 0f);
	}

	public void EndColorSwitch()
	{
		colorSwitch.Initialize(overwrite: false);
		colorSwitch.AnimationState.SetAnimation(0, "disappear", loop: false);
	}

	public void DisableColorSwitch()
	{
		colorSwitchSound.Stop();
		colorSwitch.gameObject.SetActive(value: false);
	}

	private IEnumerator HenAppearCoroutine()
	{
		currentLevel = UnityEngine.Object.Instantiate(levels[currentLevelNumber], levels[currentLevelNumber].GetStartPosition(), Quaternion.identity);
		BaseSceneManager<Hen>.Instance.SetSkin(BaseGameManager<SkinManager>.GetInstance().GetCurrentSkinName());
		BaseSceneManager<Hen>.Instance.Run();
		BaseSceneManager<Hen>.Instance.SetFogEnabled(currentLevel.HasFog());
		BaseSceneManager<CameraBehavior>.Instance.SetFogEnabled(currentLevel.HasFog());
		BaseSceneManager<BackgroundSwitcher>.Instance.SetBackground(currentLevel.GetFruitType(), currentLevel.HasFog());
		if (currentLevel.GetTutorialType() != 0)
		{
			yield return StartCoroutine(hud.ShowTutorialCoroutine(currentLevel.GetTutorialType()));
		}
		Transform henHolder = currentLevel.GetHenHolder(BaseSceneManager<Hen>.Instance.transform.position);
		float duration = 0.5f;
		float timer = 0f;
		Vector3 startPosition = new Vector3(0f, -4f, 0f);
		Vector3 targetPosition = henHolder.position;
		while (timer < duration)
		{
			timer += Time.deltaTime;
			if (timer > duration)
			{
				timer = duration;
			}
			BaseSceneManager<Hen>.Instance.transform.position = startPosition + (targetPosition - startPosition) * timer / duration;
			yield return null;
		}
		BaseSceneManager<Hen>.Instance.transform.parent = henHolder;
		isPaused = false;
	}

	private void Update()
	{
		// 放在 isPaused 判断之外：暂停/设置界面打开时也要保证这两个常驻按钮跟着首页一起消失
		ApplyHomeOnlyVisibility();

		if (!isPaused)
		{
			UpdateInput();
			UpdateUFO();
			UpdateFreezeAndFire();
			UpdatePowerupUI();
			UpdateTapTutorial();
			UpdateStars();
		}
	}

	private void UpdatePowerupUI()
	{
		hud.SetPowerups(currentLevel.GetColorSwichProgress(), freezeTimer / 5f, (float)fireCounter / 6f);
	}

	private void UpdateStars()
	{
		if (!trimphSecond)
		{
			timer += Time.deltaTime;
			if (timer < currentLevel.GetTimeFor3Stars())
			{
				hud.SetStarsProgress(3, (currentLevel.GetTimeFor3Stars() - timer) / currentLevel.GetTimeFor3Stars());
			}
			else if (timer < currentLevel.GetTimeFor2Stars())
			{
				hud.SetStarsProgress(2, (currentLevel.GetTimeFor2Stars() - timer) / (currentLevel.GetTimeFor2Stars() - currentLevel.GetTimeFor3Stars()));
			}
			else
			{
				hud.SetStarsProgress(1, 0f);
			}
		}
	}

	private void UpdateTapTutorial()
	{
		if (currentLevelNumber == 0)
		{
			if (currentLevel.SeedsLeft() == currentLevel.GoodSeedsOnStart())
			{
				hud.SetTapVisible(visible: true);
			}
			else
			{
				hud.SetTapVisible(visible: false);
			}
		}
	}

	private void UpdateUFO()
	{
		if (isUFOInTheSky)
		{
			for (int num = memories.Count - 1; num >= 1; num--)
			{
				memories[num] = memories[num - 1];
				angleMemories[num] = angleMemories[num - 1];
			}
			Vector3 vector = currentLevel.GetCenterOfTheWorld().worldToLocalMatrix.MultiplyPoint(BaseSceneManager<Hen>.Instance.transform.position);
			memories[0] = new Vector3(vector.x, vector.y, Time.time);
			List<float> list = angleMemories;
			Vector3 eulerAngles = BaseSceneManager<Hen>.Instance.transform.rotation.eulerAngles;
			float z = eulerAngles.z;
			Vector3 eulerAngles2 = currentLevel.GetCenterOfTheWorld().rotation.eulerAngles;
			list[0] = z - eulerAngles2.z;
		}
	}

	private void UpdateInput()
	{
		if (((Input.GetMouseButtonDown(0) && (UnityEngine.Input.mousePosition - pauseButton.position).magnitude > 200f) || UnityEngine.Input.GetKeyDown(KeyCode.Space)) && fireCounter == 0 && !trimphSecond)
		{
			Attack();
		}
	}

	private void UpdateFreezeAndFire()
	{
		if (freezeTimer > 0f)
		{
			freezeTimer -= Time.unscaledDeltaTime;
			if (freezeTimer <= 0f)
			{
				FinishFreeze();
			}
		}
		if (fireCounter > 0)
		{
			Seed seed = null;
			float num = BaseSceneManager<Gameplay>.Instance.currentLevel.seedsWidth;
			foreach (Seed allSeed in currentLevel.GetAllSeeds())
			{
				if (allSeed.seedState != Seed.SeedState.Eaten && allSeed.seedState != Seed.SeedState.Respawn && currentLevel.IsSeedAccesible(allSeed) && allSeed.IsOpened())
				{
					float num2 = allSeed.HowIsPointIn(BaseSceneManager<Hen>.Instance.transform.position + BaseSceneManager<Hen>.Instance.correction);
					if (num2 < num)
					{
						seed = allSeed;
						num = num2;
					}
				}
			}
			if (currentLevel.GetInsect() != null && currentLevel.GetInsect().HowIsPointIn(BaseSceneManager<Hen>.Instance.transform.position) < 1f)
			{
				seed = null;
			}
			if (seed != null && seed.seedType != Seed.SeedType.Bad)
			{
				SeedPecked(seed);
				fireCounter--;
			}
			if (fireCounter <= 0)
			{
				FinishFire();
			}
		}
	}

	private void StartFreeze()
	{
		freezeTimer = 5f;
		Time.timeScale = 0.5f;
		freeze.gameObject.SetActive(value: true);
		freezeSound.Play();
		BaseGameManager<MusicManager>.GetInstance().SetPitch(0.8f);
	}

	private void FinishFreeze()
	{
		freezeTimer = 0f;
		Time.timeScale = 1f;
		freeze.gameObject.SetActive(value: false);
		freezeSound.Stop();
		BaseGameManager<MusicManager>.GetInstance().SetPitch(1f);
	}

	private void StartFire()
	{
		fireCounter = 6;
		BaseSceneManager<Hen>.Instance.SetTrailActive(active: true);
	}

	private void FinishFire()
	{
		fireCounter = 0;
		BaseSceneManager<Hen>.Instance.SetTrailActive(active: false);
	}

	private void FinishUFO()
	{
		ufoIdleSound.Stop();
		ufoStartSound.Play();
		space.AnimationState.SetAnimation(0, "disappear", loop: false);
		UFO.AnimationState.SetAnimation(0, "disappear", loop: false);
		isUFOInTheSky = false;
	}

	public Level GetCurrentLevel()
	{
		return currentLevel;
	}

	private void Attack()
	{
		Seed seed = null;
		float num = BaseSceneManager<Gameplay>.Instance.currentLevel.seedsWidth;
		if (currentLevel.GetInsect() != null && currentLevel.GetInsect().HowIsPointIn(BaseSceneManager<Hen>.Instance.transform.position) < 1f)
		{
			currentLevel.GetInsect().WasEaten();
			if (currentLevel.GetInsect().IsGood())
			{
				BaseSceneManager<Hen>.Instance.TakeWorn();
				currentLevel.WormEaten(3);
			}
			else
			{
				BaseSceneManager<Hen>.Instance.FailFly();
				Finish(isFailed: true);
			}
			return;
		}
		if (currentLevel.GetFruitType() == Level.FruitType.Rotator)
		{
			(currentLevel as Rotator).Pecked();
		}
		if (currentLevel.HullPresent())
		{
			if (currentLevel.HullPecked(BaseSceneManager<Hen>.Instance.transform.position))
			{
				BaseSceneManager<Hen>.Instance.Fail();
				Finish(isFailed: true);
			}
			else
			{
				BaseSceneManager<Hen>.Instance.Attack();
			}
			return;
		}
		foreach (Seed allSeed in currentLevel.GetAllSeeds())
		{
			if (allSeed.seedState != Seed.SeedState.Eaten && allSeed.seedState != Seed.SeedState.Respawn && currentLevel.IsSeedAccesible(allSeed))
			{
				float num2 = allSeed.HowIsPointIn(BaseSceneManager<Hen>.Instance.transform.position + BaseSceneManager<Hen>.Instance.correction);
				if (num2 < num)
				{
					seed = allSeed;
					num = num2;
				}
			}
		}
		if (seed != null)
		{
			SeedPecked(seed);
			return;
		}
		BaseSceneManager<Hen>.Instance.Attack();
		if (currentLevel.GetFruitType() != Level.FruitType.Watermelon && currentLevel.GetFruitType() != Level.FruitType.Pea && currentLevel.GetFruitType() != Level.FruitType.Pistachio && currentLevel.GetFruitType() != Level.FruitType.Almond && currentLevel.GetFruitType() != Level.FruitType.Hazelnut && currentLevel.GetFruitType() != Level.FruitType.Durian)
		{
			currentLevel.FailAttack();
			wood.Play();
			BaseSceneManager<Hen>.Instance.dust.Play();
		}
	}

	private void SeedPecked(Seed nearestSeed)
	{
		if (!nearestSeed.IsOpened())
		{
			BaseSceneManager<Hen>.Instance.Fail();
			Finish(isFailed: true);
		}
		else if (nearestSeed.seedState == Seed.SeedState.Hull)
		{
			nearestSeed.KickHull();
			BaseSceneManager<Hen>.Instance.Attack();
		}
		else if (nearestSeed.seedType == Seed.SeedType.Bomb)
		{
			currentLevel.BombExplode(nearestSeed);
			nearestSeed.WasEaten();
			BaseSceneManager<CameraBehavior>.Instance.Shake();
			if (currentLevel.SeedsLeft() <= 0)
			{
				Finish(isFailed: false);
			}
			BaseSceneManager<Hen>.Instance.Attack();
		}
		else if (nearestSeed.seedType == Seed.SeedType.UFO)
		{
			currentLevel.ShrinkFruit(nearestSeed);
			UFO = nearestSeed.TakeUFO();
			ufoCoroutine = StartCoroutine(LaunchUFOCoroutine());
			nearestSeed.WasEaten();
			BaseSceneManager<Hen>.Instance.Attack();
		}
		else if (nearestSeed.seedType == Seed.SeedType.Fire)
		{
			currentLevel.ShrinkFruit(nearestSeed);
			StartFire();
			nearestSeed.WasEaten();
			BaseSceneManager<Hen>.Instance.Attack();
		}
		else if (nearestSeed.seedType == Seed.SeedType.Freeze)
		{
			currentLevel.ShrinkFruit(nearestSeed);
			StartFreeze();
			nearestSeed.WasEaten();
			BaseSceneManager<Hen>.Instance.Attack();
		}
		else if (nearestSeed.seedType == Seed.SeedType.Laser)
		{
			currentLevel.LaserHuyak(nearestSeed);
			nearestSeed.WasEaten();
			BaseSceneManager<CameraBehavior>.Instance.Shake();
			BaseSceneManager<Hen>.Instance.Attack();
		}
		else if (nearestSeed.seedType == Seed.SeedType.Color)
		{
			currentLevel.ShrinkFruit(nearestSeed);
			currentLevel.ColorSwitch();
			nearestSeed.WasEaten();
			BaseSceneManager<Hen>.Instance.Attack();
		}
		else if (nearestSeed.seedType == Seed.SeedType.Good)
		{
			nearestSeed.WasEaten();
			currentLevel.GoodSeedEaten();
			currentLevel.ShrinkFruit(nearestSeed);
			if (currentLevel.SeedsLeft() <= 0)
			{
				Finish(isFailed: false);
			}
			BaseSceneManager<Hen>.Instance.Attack();
		}
		else if (nearestSeed.seedType == Seed.SeedType.Respawn)
		{
			nearestSeed.WasEaten();
			currentLevel.GoodSeedEaten();
			if (currentLevel.SeedsLeft() <= 0)
			{
				Finish(isFailed: false);
			}
			BaseSceneManager<Hen>.Instance.Attack();
		}
		else
		{
			if (nearestSeed.seedType != Seed.SeedType.Bad)
			{
				return;
			}
			if (nearestSeed.seedState == Seed.SeedState.Worm)
			{
				BaseSceneManager<Hen>.Instance.TakeWorn();
				nearestSeed.WormEaten();
				currentLevel.WormEaten(nearestSeed.wormSize);
				eatSounds[UnityEngine.Random.Range(0, eatSounds.Count)].Play();
				return;
			}
			BaseSceneManager<Hen>.Instance.Fail();
			if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() != 0)
			{
				nearestSeed.WasEaten();
				BaseSceneManager<Hen>.Instance.BadPshik();
			}
			Finish(isFailed: true);
		}
	}

	public void Finish(bool isFailed)
	{
		if (!isPaused && !trimphSecond)
		{
			StartCoroutine(FinishCoroutine(isFailed));
		}
	}

	private IEnumerator FinishCoroutine(bool isFailed)
	{
		EndColorSwitch();
		DisableColorSwitch();
		if (ufoCoroutine != null)
		{
			StopCoroutine(ufoCoroutine);
		}
		if (fireCounter > 0)
		{
			FinishFire();
		}
		if (freezeTimer > 0f)
		{
			FinishFreeze();
		}
		if (isUFOInTheSky)
		{
			FinishUFO();
		}
		if (isFailed)
		{
			isPaused = true;
			if (!revived && currentLevel.SeedsLeft() <= Mathf.CeilToInt((float)currentLevel.GoodSeedsOnStart() / 4f) && BaseGameManager<AdsManager>.GetInstance().IsVideoReady())
			{
				revived = true;
				yield return new WaitForSeconds(0.25f);
				hud.ShowRevive();
				if (currentLevel.GetType().Name != "Watermelon")
				{
					BaseSceneManager<CameraBehavior>.Instance.Up();
				}
			}
			else
			{
				hud.Failed();
				yield return new WaitForSeconds(1f);
				ShowFinalScreen(isFailed: true);
			}
		}
		else
		{
			hud.Completed();
			currentLevel.Clean();
			finalExplosion.transform.position = currentLevel.GetCenterOfExplosion().position;
			finalExplosion.AnimationState.SetAnimation(0, "explode_big", loop: false);
			finalExplosionSound.Play();
			fruitParts.Play();
			fruitParts2.Play();
			BaseSceneManager<CameraBehavior>.Instance.Shake();
			trimphSecond = true;
			yield return new WaitForSeconds(1f);
			trimphSecond = false;
			isPaused = true;
			ShowFinalScreen(isFailed: false);
		}
	}

	public void Revive()
	{
		isPaused = false;
		BaseSceneManager<Hen>.Instance.Run();
		if (currentLevel.GetType().Name != "Watermelon")
		{
			BaseSceneManager<CameraBehavior>.Instance.Down();
		}
		if (currentLevel.SeedsLeft() <= 0)
		{
			Finish(isFailed: false);
		}
	}

	public void ShowFinalScreen(bool isFailed)
	{
		if (isFailed)
		{
			FadeOutIn(ShowFailScreen);
		}
		else
		{
			FadeOutIn(ShowWinScreen);
		}
	}

	private void ShowWinScreen()
	{
		if (currentLevel.GetInsect() != null)
		{
			UnityEngine.Object.Destroy(currentLevel.GetInsect().gameObject);
		}
		if ((bool)currentLevel && (bool)currentLevel.gameObject)
		{
			UnityEngine.Object.Destroy(currentLevel.gameObject);
		}
		hud.gameObject.SetActive(value: false);
		int num = stars[currentLevelNumber];
		if (num == 0)
		{
			BaseGameManager<OnlineManager>.GetInstance().LevelPassedAchievement(GetLevelNumber());
		}
		int s = 1;
		if (timer <= currentLevel.GetTimeFor2Stars())
		{
			s = 2;
		}
		if (timer <= currentLevel.GetTimeFor3Stars())
		{
			s = 3;
		}
		SaveStars(s);
		UnlockLevel(GetLevelNumber() + 1);
		finalScreen.gameObject.SetActive(value: true);
		finalScreen.Init(isFail: false, num, stars[currentLevelNumber]);
	}

	private void ShowFailScreen()
	{
		if (currentLevel.GetInsect() != null)
		{
			UnityEngine.Object.Destroy(currentLevel.GetInsect().gameObject);
		}
		UnityEngine.Object.Destroy(currentLevel.gameObject);
		hud.gameObject.SetActive(value: false);
		finalScreen.gameObject.SetActive(value: true);
		finalScreen.Init(isFail: true, stars[currentLevelNumber], stars[currentLevelNumber]);
	}

	public void Restart(RestartType restartType)
	{
		currentRestartType = restartType;
		FadeOut(Restart);
	}

	private void Restart()
	{
		BaseGameManager<MainManager>.GetInstance().Restart(currentRestartType);
	}

	public bool IsPaused()
	{
		return isPaused;
	}

	public int GetLevelNumber()
	{
		return currentLevelNumber;
	}

	public int GetLastLevelUnlocked()
	{
		return lastLevelUnlocked;
	}

	public void SetLevelNumber(int n)
	{
		currentLevelNumber = n;
		PlayerPrefs.SetInt("currentLevelNumber", currentLevelNumber);
		PlayerPrefs.Save();
		BaseGameManager<MusicManager>.GetInstance().LevelStarted();
	}

	public void UnlockLevel(int n)
	{
		if (lastLevelUnlocked < n)
		{
			lastLevelUnlocked = n;
			PlayerPrefs.SetInt("lastLevelUnlocked", lastLevelUnlocked);
			PlayerPrefs.Save();
			BaseGameManager<OnlineManager>.GetInstance().SaveToGP();
		}
	}

	public void SaveStars(int s)
	{
		if (s > stars[currentLevelNumber])
		{
			stars[currentLevelNumber] = s;
			string text = string.Empty;
			for (int i = 0; i < 260; i++)
			{
				text += stars[i].ToString();
			}
			PlayerPrefs.SetString("stars", text);
			PlayerPrefs.Save();
			int num = 0;
			for (int j = 0; j < 260; j++)
			{
				num += stars[j];
			}
			BaseGameManager<OnlineManager>.GetInstance().ReportStars(num);
		}
	}

	public void Pause()
	{
		isPaused = true;
		moneyBar.gameObject.SetActive(value: false);
	}

	public void UnPause()
	{
		isPaused = false;
		moneyBar.gameObject.SetActive(value: true);
	}

	private IEnumerator LaunchUFOCoroutine()
	{
		ufoStartSound.Play();
		memories.Clear();
		angleMemories.Clear();
		for (int j = 0; j < 300; j++)
		{
			Vector3 vector = currentLevel.GetCenterOfTheWorld().worldToLocalMatrix.MultiplyPoint(BaseSceneManager<Hen>.Instance.transform.position);
			memories.Add(new Vector3(vector.x, vector.y, Time.time));
			List<float> list = angleMemories;
			Vector3 eulerAngles = BaseSceneManager<Hen>.Instance.transform.rotation.eulerAngles;
			float z = eulerAngles.z;
			Quaternion rotation = currentLevel.GetCenterOfTheWorld().rotation;
			list.Add(z - rotation.z);
		}
		isUFOInTheSky = true;
		ufoIdleSound.Play();
		UFO.AnimationState.SetAnimation(0, "start", loop: false);
		UFO.AnimationState.AddAnimation(0, "fly", loop: true, 0f);
		space.gameObject.transform.position = currentLevel.GetStartPosition();
		space.AnimationState.SetAnimation(0, "appear", loop: false);
		space.AnimationState.AddAnimation(0, "idle", loop: true, 0f);
		UFO.transform.parent = currentLevel.GetCenterOfTheWorld();
		CameraBehavior instance = BaseSceneManager<CameraBehavior>.Instance;
		Vector3 eulerAngles2 = UFO.transform.eulerAngles;
		instance.Push(eulerAngles2.z - 90f);
		yield return new WaitForSeconds(1f);
		float delay = 1f;
		for (int i = 0; i < 5; i++)
		{
			Seed skippedSeed = null;
			while (skippedSeed == null)
			{
				yield return null;
				for (int k = 1; k < memories.Count; k++)
				{
					Vector3 vector2 = memories[k];
					if (vector2.z < Time.time - delay)
					{
						float num = Time.time - delay;
						Vector3 vector3 = memories[k];
						float num2 = num - vector3.z;
						Vector3 vector4 = memories[k - 1];
						float z2 = vector4.z;
						Vector3 vector5 = memories[k];
						float t = num2 / (z2 - vector5.z);
						Vector3 vector6 = currentLevel.GetCenterOfTheWorld().localToWorldMatrix.MultiplyPoint(memories[k]);
						Vector3 vector7 = currentLevel.GetCenterOfTheWorld().localToWorldMatrix.MultiplyPoint(memories[k - 1]);
						UFO.transform.position = new Vector3(Mathf.Lerp(vector6.x, vector7.x, t), Mathf.Lerp(vector6.y, vector7.y, t), 0f);
						Transform transform = UFO.transform;
						float num3 = angleMemories[k];
						Vector3 eulerAngles3 = currentLevel.GetCenterOfTheWorld().rotation.eulerAngles;
						float a = num3 + eulerAngles3.z;
						float num4 = angleMemories[k - 1];
						Vector3 eulerAngles4 = currentLevel.GetCenterOfTheWorld().rotation.eulerAngles;
						transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(a, num4 + eulerAngles4.z, t) + 90f);
						break;
					}
				}
				foreach (Seed allSeed in currentLevel.GetAllSeeds())
				{
					if ((allSeed.seedType == Seed.SeedType.Good || allSeed.seedType == Seed.SeedType.Respawn) && allSeed.seedState == Seed.SeedState.Default && currentLevel.IsSeedAccesible(allSeed) && allSeed.IsOpened())
					{
						float num5 = allSeed.HowIsPointIn(UFO.transform.position);
						if (num5 < 0.5f)
						{
							skippedSeed = allSeed;
							break;
						}
					}
				}
			}
			UFO.AnimationState.SetAnimation(0, "fire", loop: false);
			UFO.AnimationState.AddAnimation(0, "fly", loop: true, 0f);
			yield return new WaitForSeconds(0.15f);
			currentLevel.ShrinkFruit(skippedSeed);
			skippedSeed.WasShoot();
			ufoAttackSound.Play();
			currentLevel.GoodSeedEaten();
			if (currentLevel.SeedsLeft() <= 0)
			{
				Finish(isFailed: false);
			}
			yield return new WaitForSeconds(0.35f);
			delay += 0.5f;
		}
		FinishUFO();
	}

	public void UpdateSoundSettings()
	{
		bool flag = !PlayerPrefs.HasKey("shotsOn") || PlayerPrefs.GetInt("shotsOn") == 1;
		bool flag2 = !PlayerPrefs.HasKey("musicOn") || PlayerPrefs.GetInt("musicOn") == 1;
		mixer.SetFloat("ShotsVolume", (!flag) ? (-80f) : 0f);
		mixer.SetFloat("MusicVolume", (!flag2) ? (-80f) : 0f);
	}

	public void SetMoneyColor(Color textColor)
	{
		moneyBar.SetTextColor(textColor);
	}

	public void FadeOutIn(Action action)
	{
		fade.gameObject.SetActive(value: true);
		StartCoroutine(FadeOutCoroutine(action, fadeIn: true));
	}

	public void FadeOut(Action action)
	{
		fade.gameObject.SetActive(value: true);
		StartCoroutine(FadeOutCoroutine(action, fadeIn: false));
	}

	private void FadeIn()
	{
		fade.gameObject.SetActive(value: true);
		StartCoroutine(FadeInCoroutine());
	}

	private IEnumerator FadeOutCoroutine(Action action, bool fadeIn)
	{
		float timer = 0f;
		float duration = 0.25f;
		while (timer < duration)
		{
			timer += Time.deltaTime;
			if (timer > duration)
			{
				timer = duration;
			}
			Image image = fade;
			Color color = fade.color;
			float r = color.r;
			Color color2 = fade.color;
			float g = color2.g;
			Color color3 = fade.color;
			image.color = new Color(r, g, color3.b, timer / duration);
			yield return null;
		}
		action();
		if (fadeIn)
		{
			StartCoroutine(FadeInCoroutine());
		}
	}

	private IEnumerator FadeInCoroutine()
	{
		float timer = 0f;
		float duration = 0.25f;
		while (timer < duration)
		{
			timer += Time.deltaTime;
			if (timer > duration)
			{
				timer = duration;
			}
			Image image = fade;
			Color color = fade.color;
			float r = color.r;
			Color color2 = fade.color;
			float g = color2.g;
			Color color3 = fade.color;
			image.color = new Color(r, g, color3.b, 1f - timer / duration);
			yield return null;
		}
		fade.gameObject.SetActive(value: false);
	}

	public bool IsWormGold()
	{
		return isWormGold;
	}

	public void RefreshWorm()
	{
		isWormGold = (UnityEngine.Random.Range(0, 10) == 7);
	}

	public int GetStarsOnLevel(int l)
	{
		return stars[l];
	}

	public Level.FruitType GetLevelFruitType(int l)
	{
		return levels[l].GetFruitType();
	}

	public bool DoesLevelHasFog(int l)
	{
		return levels[l].HasFog();
	}

	public void ShowGamePassedScreen()
	{
		FadeOutIn(GamePassedAction);
	}

	private void GamePassedAction()
	{
		finalScreen.gameObject.SetActive(value: false);
		gamePassedScreen.gameObject.SetActive(value: true);
	}

	public void SharePhoto()
	{
		photoLab.gameObject.SetActive(value: true);
		photoLab.SharePhotoLevels(currentLevelNumber, stars[currentLevelNumber], "Hey! I passed level " + currentLevelNumber.ToString() + " in Pigeon Pop!");
	}
}
