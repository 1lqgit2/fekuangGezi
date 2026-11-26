using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Feeding : BaseSceneManager<Feeding>
{
	[Serializable]
	private struct FeedLevel
	{
		public float canonVelocity;

		public List<float> sizes;
	}

	[Serializable]
	private struct Segment
	{
		[SerializeField]
		private Transform start;

		[SerializeField]
		private Transform end;

		public Vector3 Start => start.position;

		public Vector3 Direction => (end.position - start.position).normalized;

		public float Size => (end.position - start.position).magnitude;

		public Quaternion ChickRotaion
		{
			get
			{
				Vector3 direction = Direction;
				return Quaternion.Euler(0f, 0f, 57.29578f * Mathf.Atan2(direction.y, direction.x));
			}
		}
	}

	[SerializeField]
	private List<FeedLevel> levels;

	[SerializeField]
	private List<Segment> segments;

	[SerializeField]
	private Transform canonHolder;

	[SerializeField]
	private Transform startPoint;

	[SerializeField]
	private float bulletVelocity;

	[SerializeField]
	private float bulletAngularVelocity;

	[SerializeField]
	private Chick chickPrefab;

	[SerializeField]
	private Rigidbody2D bulletPrefab;

	[SerializeField]
	private Rigidbody2D wormPrefab;

	[SerializeField]
	private AudioSource shootSound;

	[SerializeField]
	private ParticleSystem konfeti;

	[SerializeField]
	private AudioSource fireworks;

	[SerializeField]
	private AudioSource overSound;

	[SerializeField]
	private Image fade;

	[SerializeField]
	private FeedingHud hud;

	[SerializeField]
	private FinalScreenInfinite finalScreen;

	[SerializeField]
	private GameObject redBorder;

	private int currentLevel;

	private float canonVelocity;

	private int currentComboLevel;

	private List<Chick> chicks = new List<Chick>();

	private Chick previousChick;

	private float currentPath;

	private float previousChickPath;

	private bool isPaused;

	private bool isFailed;

	private List<Rigidbody2D> bullets = new List<Rigidbody2D>();

	/// <summary>
	/// 上一次开火时间。用于给「点击屏幕开火」和 TapZone 的 PointerClick 去重：
	/// 同一次点击会同时命中两个入口，不去重会一次开两枪。
	/// </summary>
	private float lastShootTime = -1f;

	private int score;

	private RestartType currentRestartType;

	private Rigidbody2D bullet;

	private void Start()
	{
		FadeIn();
		isPaused = true;
		isFailed = false;
		canonVelocity = 0f;
		score = 0;
		currentComboLevel = 0;
		currentPath = 0f;
		hud.gameObject.SetActive(value: true);
		GenerateBullet();
		BaseSceneManager<BackgroundSwitcher>.Instance.SetFeedingLevelBackground(0);
		StartCoroutine(InitLevelCoroutine(0));
	}

	private IEnumerator InitLevelCoroutine(int n)
	{
		currentLevel = n;
		FeedLevel feedLevel = GetLevel(currentLevel);
		if (feedLevel.sizes == null || feedLevel.sizes.Count == 0)
		{
			Debug.LogError("[喂食] 关卡配置为空，停止生成");
			isPaused = false;
			yield break;
		}
		int chickCount = feedLevel.sizes.Count;
		List<Vector3> startPositions = new List<Vector3>();
		List<Vector3> targetPositions = new List<Vector3>();
		List<int> zoneNumbers = new List<int>();
		for (int i = 0; i < 6; i++)
		{
			zoneNumbers.Add(i);
		}
		for (int j = 0; j < chickCount; j++)
		{
			int num = zoneNumbers[UnityEngine.Random.Range(0, zoneNumbers.Count)];
			zoneNumbers.Remove(num);
			int index = 0;
			int num2 = 0;
			int num3 = 1;
			switch (num)
			{
			case 1:
				index = 1;
				num2 = 0;
				num3 = 2;
				break;
			case 2:
				index = 1;
				num2 = 1;
				num3 = 2;
				break;
			case 3:
				index = 2;
				num2 = 0;
				num3 = 1;
				break;
			case 4:
				index = 3;
				num2 = 0;
				num3 = 2;
				break;
			case 5:
				index = 3;
				num2 = 1;
				num3 = 2;
				break;
			}
			float num4 = feedLevel.sizes[j];
			float num5 = segments[index].Size / (float)num3;
			// 原来的随机区间是 [num5*num2+num4, num5*(num2+1)-num4]，
			// 当小鸡尺寸 num4 比半段还大时 min > max，Random.Range 会把小鸡丢到段外，
			// 表现为小鸡落在圆环外侧、根本打不到，关卡永远清不完。
			// 这里改成先把区间夹紧到段内，再做随机。
			float size = segments[index].Size;
			float num6 = num5 * (float)num2 + num4;
			float num7 = num5 * (float)(num2 + 1) - num4;
			if (num6 > num7)
			{
				float num8 = (num6 + num7) * 0.5f;
				num6 = num8;
				num7 = num8;
			}
			float d = Mathf.Clamp(UnityEngine.Random.Range(num6, num7), 0f, size);
			targetPositions.Add(segments[index].Start + segments[index].Direction * d);
			Chick chick = UnityEngine.Object.Instantiate(chickPrefab, targetPositions[j], segments[index].ChickRotaion);
			chick.Init(num4);
			chicks.Add(chick);
			startPositions.Add(chick.GetBottom());
		}
		float timer = 0f;
		float duration = 1f;
		while (timer < duration)
		{
			timer += Time.deltaTime;
			if (timer > duration)
			{
				timer = duration;
			}
			for (int k = 0; k < chickCount; k++)
			{
				Transform transform = chicks[k].transform;
				Vector3 vector = startPositions[k];
				float x = vector.x;
				Vector3 vector2 = targetPositions[k];
				float x2 = Mathf.Lerp(x, vector2.x, timer / duration);
				Vector3 vector3 = startPositions[k];
				float y = vector3.y;
				Vector3 vector4 = targetPositions[k];
				float y2 = Mathf.Lerp(y, vector4.y, timer / duration);
				Vector3 position = chicks[k].transform.position;
				transform.position = new Vector3(x2, y2, position.z);
			}
			yield return null;
		}
		isPaused = false;
	}

	private IEnumerator ClearLevelCoroutine()
	{
		isPaused = true;
		List<Vector3> startPositions = new List<Vector3>();
		List<Vector3> targetPositions = new List<Vector3>();
		foreach (Chick chick in chicks)
		{
			startPositions.Add(chick.transform.position);
			targetPositions.Add(chick.GetBottom());
		}
		float timer = 0f;
		float duration = 1f;
		while (timer < duration)
		{
			timer += Time.deltaTime;
			if (timer > duration)
			{
				timer = duration;
			}
			for (int i = 0; i < chicks.Count; i++)
			{
				Transform transform = chicks[i].transform;
				Vector3 vector = startPositions[i];
				float x = vector.x;
				Vector3 vector2 = targetPositions[i];
				float x2 = Mathf.Lerp(x, vector2.x, timer / duration);
				Vector3 vector3 = startPositions[i];
				float y = vector3.y;
				Vector3 vector4 = targetPositions[i];
				float y2 = Mathf.Lerp(y, vector4.y, timer / duration);
				Vector3 position = chicks[i].transform.position;
				transform.position = new Vector3(x2, y2, position.z);
			}
			yield return null;
		}
		foreach (Chick chick2 in chicks)
		{
			UnityEngine.Object.Destroy(chick2.gameObject);
		}
		previousChick = null;
		chicks.Clear();
		// 关卡表打完后回到第 1 关继续循环。原来是 currentLevel + 1 直接越界，
		// 之后 Update 里每帧取 levels[currentLevel] 都会抛异常，游戏彻底卡死。
		StartCoroutine(InitLevelCoroutine(NextLevelIndex(currentLevel + 1)));
	}

	/// <summary>安全取关卡：索引越界时按关卡表长度循环，绝不会抛 ArgumentOutOfRange。</summary>
	private FeedLevel GetLevel(int index)
	{
		if (levels == null || levels.Count == 0)
		{
			return default(FeedLevel);
		}

		return levels[WrapLevelIndex(index)];
	}

	private int WrapLevelIndex(int index)
	{
		int count = (levels == null) ? 0 : levels.Count;
		if (count <= 0)
		{
			return 0;
		}

		int wrapped = index % count;
		return (wrapped < 0) ? (wrapped + count) : wrapped;
	}

	private int NextLevelIndex(int index)
	{
		return WrapLevelIndex(index);
	}

	private void Update()
	{
		if (!isFailed)
		{
			FeedLevel feedLevel = GetLevel(currentLevel);
			float num = feedLevel.canonVelocity - canonVelocity;
			if (num != 0f)
			{
				float num2 = Time.deltaTime * 300f * Mathf.Sign(num);
				if (Mathf.Abs(num) < Mathf.Abs(num2))
				{
					num2 = num;
				}
				canonVelocity += num2;
			}
			float num3 = canonVelocity * Time.deltaTime;
			currentPath += Mathf.Abs(num3);
			canonHolder.Rotate(0f, 0f, num3);
		}
		// 手机上也要能开火：原来只有空格键 + TapZone 的 PointerClick，
		// 一旦 TapZone 被其它全屏 UI 盖住就完全打不出子弹，表现为「无法过关」。
		// 这里和主玩法保持一致，直接用鼠标/触摸按下兜底，Shoot 内部有冷却去重。
		if (UnityEngine.Input.GetKeyDown(KeyCode.Space) || UnityEngine.Input.GetMouseButtonDown(0))
		{
			Shoot();
		}
		if (!isPaused)
		{
			bool flag = false;
			foreach (Chick chick in chicks)
			{
				if (chick.IsActive())
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				BaseSceneManager<BackgroundSwitcher>.Instance.SetFeedingLevelBackground(currentLevel + 1);
				konfeti.Play();
				fireworks.Play();
				StartCoroutine(ClearLevelCoroutine());
			}
		}
	}

	private void GenerateBullet()
	{
		bullet = UnityEngine.Object.Instantiate((UnityEngine.Random.Range(0, 5) != 0) ? bulletPrefab : wormPrefab, startPoint);
		bullet.constraints = RigidbodyConstraints2D.FreezeAll;
		bullet.GetComponent<FeedingBullet>().SetComboLevel(currentComboLevel);
		bullets.Add(bullet);
	}

	public void Shoot()
	{
		if (!isPaused)
		{
			// 同一帧里 Update 的鼠标按下和 TapZone 的 PointerClick 都会进来，这里去重，
			// 保证一次点击只打一发。
			if (Time.unscaledTime - lastShootTime < 0.05f)
			{
				return;
			}
			lastShootTime = Time.unscaledTime;

			Vector3 eulerAngles = canonHolder.rotation.eulerAngles;
			float f = (float)Math.PI / 180f * eulerAngles.z;
			Vector2 velocity = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * bulletVelocity;
			bullet.transform.parent = null;
			bullet.constraints = RigidbodyConstraints2D.None;
			bullet.velocity = velocity;
			bullet.angularVelocity = bulletAngularVelocity;
			shootSound.Play();
			GenerateBullet();
		}
	}

	public void ChickHit(Chick chick, Rigidbody2D bullet)
	{
		if (isPaused || isFailed || !bullet || !bullets.Contains(bullet))
		{
			return;
		}
		ParticleSystem componentInChildren = bullet.GetComponentInChildren<ParticleSystem>();
		componentInChildren.transform.parent = null;
		componentInChildren.Play();
		bullets.Remove(bullet);
		UnityEngine.Object.Destroy(bullet.gameObject);
		if ((bool)previousChick)
		{
			if (currentPath - previousChickPath < 360f)
			{
				Vector3 position = previousChick.transform.position;
				float y = position.y;
				Vector3 position2 = previousChick.transform.position;
				float target = Mathf.Atan2(y, position2.x) * 57.29578f;
				Vector3 position3 = chick.transform.position;
				float y2 = position3.y;
				Vector3 position4 = chick.transform.position;
				float current = Mathf.Atan2(y2, position4.x) * 57.29578f;
				float f = Mathf.DeltaAngle(current, target);
				bool flag = true;
				foreach (Chick chick2 in chicks)
				{
					if (chick2 != chick && chick2.IsActive())
					{
						Vector3 position5 = chick2.transform.position;
						float y3 = position5.y;
						Vector3 position6 = chick2.transform.position;
						float current3 = Mathf.Atan2(y3, position6.x) * 57.29578f;
						float f2 = Mathf.DeltaAngle(current3, target);
						if (Mathf.Sign(f2) == Mathf.Sign(f) && Mathf.Abs(f2) < Mathf.Abs(f))
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					currentComboLevel++;
				}
				else
				{
					currentComboLevel = 0;
				}
			}
			else
			{
				currentComboLevel = 0;
			}
		}
		else if (currentComboLevel > 0)
		{
			currentComboLevel++;
		}
		foreach (Rigidbody2D bullet2 in bullets)
		{
			bullet2.GetComponent<FeedingBullet>().SetComboLevel(currentComboLevel);
		}
		hud.Combo(currentComboLevel);
		previousChick = chick;
		previousChickPath = currentPath;
		if (bullet.GetComponent<FeedingBullet>().IsWorm())
		{
			BaseGameManager<MainManager>.GetInstance().CoinsEarned(1);
		}
		else
		{
			score += 1 + currentComboLevel;
		}
	}

	public void BorderHit(Rigidbody2D bullet)
	{
		if (!isFailed && !isPaused)
		{
			overSound.Play();
			isFailed = true;
			isPaused = true;
			hud.gameObject.SetActive(value: false);
			redBorder.SetActive(value: true);
			BaseGameManager<OnlineManager>.GetInstance().ReportFeedingScore(score);
			StartCoroutine(FailCoroutine());
		}
	}

	private IEnumerator FailCoroutine()
	{
		yield return new WaitForSeconds(1f);
		FadeOutIn(FailAction);
	}

	private void FailAction()
	{
		int num = PlayerPrefs.HasKey("feedingScore") ? PlayerPrefs.GetInt("feedingScore") : 0;
		finalScreen.gameObject.SetActive(value: true);
		List<string> list = new List<string>();
		foreach (Chick chick in chicks)
		{
			list.Add(chick.GetSkinName());
		}
		if (score > num)
		{
			finalScreen.Init(isRecord: true, num, score, list);
			PlayerPrefs.SetInt("feedingScore", score);
			PlayerPrefs.Save();
			BaseGameManager<OnlineManager>.GetInstance().SaveToGP();
		}
		else
		{
			finalScreen.Init(isRecord: false, num, score, list);
		}
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

	public void Restart(RestartType restartType)
	{
		currentRestartType = restartType;
		FadeOut(Restart);
	}

	private void Restart()
	{
		BaseGameManager<MainManager>.GetInstance().Restart(currentRestartType);
	}

	public int GetScore()
	{
		return score;
	}
}
