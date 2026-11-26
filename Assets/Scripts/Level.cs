using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
	public enum FruitType
	{
		Corn,
		Pea,
		Watermelon,
		Garnet,
		Raspberry,
		Blackberry,
		Cherry,
		Rotator,
		Pistachio,
		Almond,
		Hazelnut,
		Durian,
		Apple
	}

	public enum GameplayChangingSeedType
	{
		GCST_None,
		GCST_UFO,
		GCST_Freeze,
		GCST_Fire
	}

	[SerializeField]
	protected int seedsAmount;

	[SerializeField]
	protected int badSeedsAmount;

	[SerializeField]
	protected int bombsAmount;

	[SerializeField]
	public float radius;

	[SerializeField]
	protected float speed;

	[SerializeField]
	protected float henSpeed;

	[SerializeField]
	protected bool insectPresent;

	[SerializeField]
	public float seedsWidth = 1.1f;

	[SerializeField]
	private Gameplay.TutorialType tutorialType;

	[SerializeField]
	private float timeFor2Stars = 20f;

	[SerializeField]
	private float timeFor3Stars = 10f;

	[SerializeField]
	private float turnsToRespawn = 1.8f;

	[SerializeField]
	private bool hasFog;

	protected List<Seed> seeds = new List<Seed>();

	protected List<Seed> badSeeds = new List<Seed>();

	protected Insect insect;

	protected int goodSeedCounter;

	protected int goodSeedsOnStart;

	protected int wormCounter;

	protected int currentLevel;

	protected Coroutine wormCoroutine;

	protected const float wormStartInterval = 2f;

	protected const float wormInterval = 5f;

	private float colorSwitchTimer;

	protected Vector3 shift;

	[SerializeField]
	protected GameplayChangingSeedType gameplayChangingSeedType;

	private void Awake()
	{
		Init();
	}

	protected virtual void Init()
	{
	}

	public float GetTimeFor2Stars()
	{
		return timeFor2Stars;
	}

	public float GetTimeFor3Stars()
	{
		return timeFor3Stars;
	}

	public virtual Transform GetHenHolder(Vector3 initialPosition)
	{
		return base.transform;
	}

	public virtual void ShrinkFruit(Seed s)
	{
	}

	public virtual void GoodSeedEaten()
	{
		goodSeedCounter--;
	}

	public void WormEaten(int wormSize)
	{
		switch (wormSize)
		{
		case 1:
			BaseGameManager<MainManager>.GetInstance().CoinsEarned(wormSize);
			break;
		case 3:
			BaseGameManager<MainManager>.GetInstance().CoinsEarned(wormSize);
			break;
		case 5:
			BaseGameManager<MainManager>.GetInstance().CoinsEarned(wormSize);
			break;
		}
		wormCounter--;
		BaseSceneManager<Gameplay>.Instance.RefreshWorm();
	}

	public void SeedRespawned()
	{
		goodSeedCounter++;
	}

	public float GetCornSpeed()
	{
		return speed;
	}

	public float GetHenSpeed()
	{
		return henSpeed;
	}

	public int SeedsLeft()
	{
		return goodSeedCounter;
	}

	public int GoodSeedsOnStart()
	{
		return goodSeedsOnStart;
	}

	public virtual Seed GetNextComboSeed(Seed seed)
	{
		return null;
	}

	public float GetTurnsForRespawn()
	{
		return turnsToRespawn;
	}

	public virtual bool IsSeedAccesible(Seed seed)
	{
		return true;
	}

	public virtual void BombExplode(Seed bomb)
	{
	}

	public virtual int LaserHuyak(Seed laser)
	{
		return 0;
	}

	public void ColorSwitch()
	{
		StartCoroutine(ColorSwitchCoroutine());
	}

	public IEnumerator ColorSwitchCoroutine()
	{
		BaseSceneManager<Gameplay>.Instance.StartColorSwitch();
		foreach (Seed badSeed in badSeeds)
		{
			if (badSeed.seedState != Seed.SeedState.Eaten)
			{
				badSeed.ConvertIn();
				IncreaseSeedCounter(badSeed);
			}
		}
		colorSwitchTimer = 3f;
		while (colorSwitchTimer > 0.4f)
		{
			colorSwitchTimer -= Time.deltaTime;
			if (colorSwitchTimer < 0.4f)
			{
				colorSwitchTimer = 0.4f;
			}
			yield return null;
		}
		BaseSceneManager<Gameplay>.Instance.EndColorSwitch();
		while (colorSwitchTimer > 0f)
		{
			colorSwitchTimer -= Time.deltaTime;
			if (colorSwitchTimer < 0f)
			{
				colorSwitchTimer = 0f;
			}
			yield return null;
		}
		BaseSceneManager<Gameplay>.Instance.DisableColorSwitch();
		foreach (Seed badSeed2 in badSeeds)
		{
			if (badSeed2.seedState != Seed.SeedState.Eaten)
			{
				badSeed2.ConvertOut();
				DecreaseSeedCounter(badSeed2);
			}
		}
		if (SeedsLeft() <= 0)
		{
			BaseSceneManager<Gameplay>.Instance.Finish(isFailed: false);
		}
	}

	public float GetColorSwichProgress()
	{
		return colorSwitchTimer / 3f;
	}

	public virtual void IncreaseSeedCounter(Seed s)
	{
		goodSeedCounter++;
	}

	public virtual void DecreaseSeedCounter(Seed s)
	{
		goodSeedCounter--;
	}

	public List<Seed> GetAllSeeds()
	{
		return seeds;
	}

	public Insect GetInsect()
	{
		return insect;
	}

	public virtual Transform GetCenterOfTheWorld()
	{
		return base.transform;
	}

	public virtual Transform GetCenterOfExplosion()
	{
		return GetCenterOfTheWorld();
	}

	public void Clean()
	{
		StartCoroutine(CleanCoroutine());
	}

	public void FailAttack()
	{
		StartCoroutine(Shake());
	}

	public virtual Transform GetSeedHolder()
	{
		return null;
	}

	private IEnumerator Shake()
	{
		float time = 0f;
		float period = 0.1f;
		float amplitude = 0.1f;
		Transform seedHolder = GetSeedHolder();
		Vector3 direction = -BaseSceneManager<Hen>.Instance.transform.position + seedHolder.transform.position;
		direction.Normalize();
		while (time < period)
		{
			time += Time.deltaTime;
			if (GetFruitType() == FruitType.Raspberry)
			{
				shift = direction * Mathf.Sin(time / period * (float)Math.PI) * amplitude;
			}
			else
			{
				seedHolder.transform.localPosition = direction * Mathf.Sin(time / period * (float)Math.PI) * amplitude;
			}
			yield return 0;
		}
		if (GetFruitType() == FruitType.Raspberry)
		{
			shift = Vector3.zero;
		}
		else
		{
			seedHolder.transform.localPosition = Vector3.zero;
		}
	}

	protected virtual IEnumerator CleanCoroutine()
	{
		GameObject oldInsect = null;
		if (insect != null)
		{
			oldInsect = insect.gameObject;
			insect = null;
		}
		float duration = 1f;
		float timer = 0f;
		List<GameObject> shkurki = new List<GameObject>();
		List<float> shkurkiSpeed = new List<float>();
		List<float> shkurkiAngleSpeed = new List<float>();
		foreach (Seed seed in seeds)
		{
			if (seed.seedState != Seed.SeedState.Eaten)
			{
				shkurki.Add(seed.TakeSeed());
				shkurkiSpeed.Add(UnityEngine.Random.Range(9f, 12f));
				shkurkiAngleSpeed.Add(UnityEngine.Random.Range(-20f, 20f));
			}
		}
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			spriteRenderer.enabled = false;
		}
		while (timer < duration)
		{
			timer += Time.deltaTime;
			if (timer > duration)
			{
				timer = duration;
			}
			for (int j = 0; j < shkurki.Count; j++)
			{
				shkurki[j].transform.position += (shkurki[j].transform.position - GetCenterOfTheWorld().position).normalized * shkurkiSpeed[j] * Time.deltaTime;
				shkurki[j].transform.Rotate(new Vector3(0f, 0f, shkurkiAngleSpeed[j] * Time.deltaTime));
			}
			if (oldInsect != null)
			{
				oldInsect.transform.position += (oldInsect.transform.position - GetCenterOfTheWorld().position).normalized * Time.deltaTime * 5f;
			}
			yield return null;
		}
		foreach (GameObject item in shkurki)
		{
			UnityEngine.Object.Destroy(item);
		}
		if (oldInsect != null)
		{
			UnityEngine.Object.Destroy(oldInsect);
		}
	}

	public virtual Vector3 GetStartPosition()
	{
		return new Vector3(0f, 0.5f, 0f);
	}

	public Gameplay.TutorialType GetTutorialType()
	{
		return tutorialType;
	}

	public virtual FruitType GetFruitType()
	{
		return FruitType.Corn;
	}

	public bool HasFog()
	{
		return hasFog;
	}

	public virtual bool HullPresent()
	{
		return false;
	}

	public virtual bool HullPecked(Vector3 point)
	{
		return true;
	}
}
