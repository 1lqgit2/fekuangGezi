using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corn : Level
{
	[SerializeField]
	protected Seed seedPrefab;

	[SerializeField]
	private Insect insectPrefab;

	[SerializeField]
	protected Transform seedHolder;

	[SerializeField]
	protected Transform henHolder;

	[SerializeField]
	protected SpriteRenderer middle;

	[SerializeField]
	private SpriteRenderer back;

	[SerializeField]
	protected int laserSeedsAmount;

	[SerializeField]
	protected int colorSeedsAmount;

	[SerializeField]
	private GameObject laserPrefab;

	[SerializeField]
	private GameObject laserPrefabBoom;

	protected float speedMultiplicator = 1f;

	protected List<float> angles = new List<float>();

	protected List<float> angleSizes = new List<float>();

	protected override void Init()
	{
		SpecialFruitSetup();
		int num = -1;
		if ((bool)middle)
		{
			middle.transform.localScale = new Vector3(2f * radius, 2f * radius, 1f);
		}
		if ((bool)back)
		{
			back.transform.localScale = new Vector3(2f * radius, 2f * radius, 1f);
		}
		BaseSceneManager<Hen>.Instance.correction = new Vector3(0f, 0f, 0f);
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		List<int> list4 = new List<int>();
		UnityEngine.Random.InitState(Mathf.RoundToInt((float)DateTime.Now.Second * 1000f));
		if (gameplayChangingSeedType != 0)
		{
			num = UnityEngine.Random.Range(0, seedsAmount);
			if (GetFruitType() != FruitType.Pistachio)
			{
				if (num + 1 < seedsAmount)
				{
					list.Add(num + 1);
				}
				else
				{
					list.Add(0);
				}
				if (num - 1 >= 0)
				{
					list.Add(num - 1);
				}
				else
				{
					list.Add(seedsAmount - 1);
				}
			}
		}
		int num2 = 0;
		while (list2.Count < bombsAmount && num2 < 100)
		{
			num2++;
			int num3 = UnityEngine.Random.Range(0, seedsAmount);
			bool flag = true;
			foreach (int item in list2)
			{
				if (Mathf.Abs(item - num3) < 4 || Mathf.Abs(item + seedsAmount - num3) < 4)
				{
					flag = false;
				}
			}
			if (Mathf.Abs(num - num3) < 4 || Mathf.Abs(num + seedsAmount - num3) < 4)
			{
				flag = false;
			}
			if (!flag)
			{
				continue;
			}
			list2.Add(num3);
			if (GetFruitType() != FruitType.Pistachio)
			{
				if (num3 + 1 < seedsAmount)
				{
					list.Add(num3 + 1);
				}
				else
				{
					list.Add(0);
				}
				if (num3 - 1 >= 0)
				{
					list.Add(num3 - 1);
				}
				else
				{
					list.Add(seedsAmount - 1);
				}
			}
		}
		num2 = 0;
		while (list3.Count < laserSeedsAmount && num2 < 100)
		{
			num2++;
			int num4 = UnityEngine.Random.Range(0, seedsAmount);
			bool flag2 = true;
			foreach (int item2 in list3)
			{
				if (Mathf.Abs(item2 - num4) < 4 || Mathf.Abs(item2 + seedsAmount - num4) < 4)
				{
					flag2 = false;
				}
			}
			foreach (int item3 in list2)
			{
				if (Mathf.Abs(item3 - num4) < 4 || Mathf.Abs(item3 + seedsAmount - num4) < 4)
				{
					flag2 = false;
				}
			}
			if (Mathf.Abs(num - num4) < 4 || Mathf.Abs(num + seedsAmount - num4) < 4)
			{
				flag2 = false;
			}
			if (!flag2)
			{
				continue;
			}
			list3.Add(num4);
			if (GetFruitType() != FruitType.Pistachio)
			{
				if (num4 + 1 < seedsAmount)
				{
					list.Add(num4 + 1);
				}
				else
				{
					list.Add(0);
				}
				if (num4 - 1 >= 0)
				{
					list.Add(num4 - 1);
				}
				else
				{
					list.Add(seedsAmount - 1);
				}
			}
		}
		num2 = 0;
		while (list4.Count < colorSeedsAmount && num2 < 100)
		{
			num2++;
			int num5 = UnityEngine.Random.Range(0, seedsAmount);
			bool flag3 = true;
			foreach (int item4 in list3)
			{
				if (Mathf.Abs(item4 - num5) < 4 || Mathf.Abs(item4 + seedsAmount - num5) < 4)
				{
					flag3 = false;
				}
			}
			foreach (int item5 in list2)
			{
				if (Mathf.Abs(item5 - num5) < 4 || Mathf.Abs(item5 + seedsAmount - num5) < 4)
				{
					flag3 = false;
				}
			}
			foreach (int item6 in list4)
			{
				if (Mathf.Abs(item6 - num5) < 4 || Mathf.Abs(item6 + seedsAmount - num5) < 4)
				{
					flag3 = false;
				}
			}
			if (Mathf.Abs(num - num5) < 4 || Mathf.Abs(num + seedsAmount - num5) < 4)
			{
				flag3 = false;
			}
			if (flag3)
			{
				list4.Add(num5);
			}
		}
		num2 = 0;
		while (list.Count < badSeedsAmount && num2 < 100)
		{
			num2++;
			int num6 = UnityEngine.Random.Range(0, seedsAmount);
			if (!list.Contains(num6) && !list2.Contains(num6) && !list3.Contains(num6) && num6 != num)
			{
				list.Add(num6);
			}
		}
		float num7 = 0f;
		for (int i = 0; i < seedsAmount; i++)
		{
			float num8 = (!list2.Contains(i) && !list3.Contains(i) && !list4.Contains(i) && i != num) ? UnityEngine.Random.Range(100f * (1f - GetSizeRandomizationRange()), 100f * (1f + GetSizeRandomizationRange())) : 100f;
			angleSizes.Add(num8);
			num7 += num8;
		}
		for (int j = 0; j < seedsAmount; j++)
		{
			List<float> list5;
			int index;
			(list5 = angleSizes)[index = j] = list5[index] * (360f / num7);
		}
		angles.Add(angleSizes[0] / 2f);
		float num9 = angleSizes[0] / 2f;
		for (int k = 1; k < seedsAmount; k++)
		{
			num9 += angleSizes[k - 1] / 2f;
			num9 += angleSizes[k] / 2f;
			angles.Add(num9);
		}
		for (int l = 0; l < seedsAmount; l++)
		{
			Seed seed = UnityEngine.Object.Instantiate(seedPrefab, seedHolder);
			if (list.Contains(l))
			{
				seed.InitType(Seed.SeedType.Bad);
				badSeeds.Add(seed);
			}
			else if (list2.Contains(l))
			{
				seed.InitType(Seed.SeedType.Bomb);
			}
			else if (num == l)
			{
				if (gameplayChangingSeedType == GameplayChangingSeedType.GCST_Freeze)
				{
					seed.InitType(Seed.SeedType.Freeze);
				}
				else if (gameplayChangingSeedType == GameplayChangingSeedType.GCST_Fire)
				{
					seed.InitType(Seed.SeedType.Fire);
				}
				else if (gameplayChangingSeedType == GameplayChangingSeedType.GCST_UFO)
				{
					seed.InitType(Seed.SeedType.UFO);
				}
			}
			else if (list3.Contains(l))
			{
				seed.InitType(Seed.SeedType.Laser);
			}
			else if (list4.Contains(l))
			{
				seed.InitType(Seed.SeedType.Color);
			}
			else
			{
				seed.InitType(Seed.SeedType.Good);
				goodSeedCounter++;
			}
			SpecialSeedSetup(seed, l);
			seeds.Add(seed);
		}
		if (insectPresent)
		{
			StartCoroutine(InsectAppearCoroutine());
		}
		if (GetTutorialType() == Gameplay.TutorialType.TT_None)
		{
			wormCoroutine = StartCoroutine(WormCoroutine());
			wormCounter = 3;
		}
		goodSeedsOnStart = goodSeedCounter;
	}

	protected virtual void SpecialFruitSetup()
	{
	}

	protected virtual void SpecialSeedSetup(Seed seed, int n)
	{
		float radius = base.radius;
		float f = angles[n] * ((float)Math.PI / 180f);
		float num = angleSizes[n] / (360f / (float)seedsAmount) * base.radius * (20f / (float)seedsAmount);
		seed.transform.localPosition = new Vector3(radius * Mathf.Sin(f), radius * Mathf.Cos(f), 0f);
		seed.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f - angles[n]));
		seed.transform.localScale = new Vector3(num, num, 1f);
	}

	private void Update()
	{
		if (!BaseSceneManager<Gameplay>.Instance.IsPaused())
		{
			UpdateMovement();
		}
	}

	protected virtual void UpdateMovement()
	{
		seedHolder.Rotate(0f, 0f, speed * speedMultiplicator * Time.deltaTime);
		henHolder.parent.Rotate(0f, 0f, henSpeed * Time.deltaTime);
		if (insect != null)
		{
			Vector3 eulerAngles = insect.transform.eulerAngles;
			float z = eulerAngles.z;
			z += Time.deltaTime * speed * 2.5f;
			insect.transform.position = base.transform.position + new Vector3(radius * Mathf.Cos(z * ((float)Math.PI / 180f)), radius * Mathf.Sin(z * ((float)Math.PI / 180f)), 0f);
			insect.transform.rotation = Quaternion.Euler(0f, 0f, z);
		}
	}

	public override Transform GetHenHolder(Vector3 initialPosition)
	{
		henHolder.transform.localPosition = new Vector3(0f, 0f - radius, 0f);
		Vector3 vector = BaseSceneManager<Hen>.Instance.transform.position - henHolder.parent.position;
		float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		henHolder.transform.parent.localRotation = Quaternion.Euler(0f, 0f, num - 270f);
		return henHolder;
	}

	protected IEnumerator InsectAppearCoroutine()
	{
		yield return new WaitUntil(() => !HullPresent());
		float duration = 0.33f;
		float timer = 0f;
		Vector3 startPosition = base.transform.position + new Vector3(0f, 3f, 0f);
		Vector3 position = base.transform.position;
		Vector3 targetPosition = new Vector3(0f, position.y + radius, 0f);
		Insect newInsect = UnityEngine.Object.Instantiate(insectPrefab);
		newInsect.transform.position = startPosition;
		newInsect.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
		while (timer < duration)
		{
			timer += Time.deltaTime;
			if (timer > duration)
			{
				timer = duration;
			}
			newInsect.transform.position = startPosition + (targetPosition - startPosition) * (timer / duration);
			yield return null;
		}
		insect = newInsect;
	}

	public override int LaserHuyak(Seed laser)
	{
		int num = seeds.IndexOf(laser);
		int num2 = 0;
		List<Seed> list = new List<Seed>();
		int num3 = num;
		int num4 = 0;
		for (int i = 0; i < seedsAmount; i++)
		{
			if ((seeds[i].seedType == Seed.SeedType.Good || seeds[i].seedType == Seed.SeedType.Bad) && seeds[i].seedState == Seed.SeedState.Default)
			{
				num4++;
			}
		}
		int num5 = 0;
		while (num5 < Mathf.Min(5, num4))
		{
			num3 = UnityEngine.Random.Range(0, seedsAmount);
			if (seeds[num3].seedState != Seed.SeedState.Eaten && seeds[num3].seedState != Seed.SeedState.Respawn && !list.Contains(seeds[num3]))
			{
				num5++;
				list.Add(seeds[num3]);
			}
		}
		for (int j = 0; j < num5; j++)
		{
			if (list[j].seedType == Seed.SeedType.Good)
			{
				GoodSeedEaten();
			}
			else if (list[j].seedType == Seed.SeedType.Respawn)
			{
				GoodSeedEaten();
			}
			else if (list[j].seedType == Seed.SeedType.Bad && list[j].seedState == Seed.SeedState.Worm)
			{
				num2++;
				WormEaten(list[j].wormSize);
			}
		}
		if (SeedsLeft() <= 0)
		{
			BaseSceneManager<Gameplay>.Instance.Finish(isFailed: false);
		}
		StartCoroutine(LaserExplodeCoroutine(laser, list));
		int num6 = 0;
		foreach (Seed item in list)
		{
			num6 = ((item.seedType != Seed.SeedType.Bad) ? (num6 + 1) : (num6 + 3));
		}
		return num6 + num2 * 10;
	}

	private IEnumerator LaserExplodeCoroutine(Seed laser, List<Seed> neighbours)
	{
		float duration = 1f;
		float timer = 0f;
		List<GameObject> toExplode = new List<GameObject>();
		List<GameObject> toExplodeVisual = new List<GameObject>();
		foreach (Seed neighbour in neighbours)
		{
			Vector3 a = laser.transform.position - neighbour.transform.position;
			float num = a.magnitude / 4f;
			GameObject gameObject = UnityEngine.Object.Instantiate(laserPrefab, neighbour.transform.position + a / 2f, Quaternion.Euler(0f, 0f, 57.29578f * Mathf.Atan2(a.y, a.x)), seedHolder);
			gameObject.transform.localScale = new Vector3(num, num, num);
			toExplode.Add(gameObject);
		}
		while (timer < duration)
		{
			timer += Time.deltaTime;
			if (timer > duration)
			{
				timer = duration;
			}
			yield return null;
		}
		foreach (Seed neighbour2 in neighbours)
		{
			neighbour2.WasEaten();
			GameObject item = UnityEngine.Object.Instantiate(laserPrefabBoom, neighbour2.transform.position, Quaternion.identity, seedHolder);
			toExplodeVisual.Add(item);
		}
		foreach (GameObject item2 in toExplode)
		{
			UnityEngine.Object.Destroy(item2);
		}
		yield return new WaitForSeconds(0.5f);
		foreach (GameObject item3 in toExplodeVisual)
		{
			UnityEngine.Object.Destroy(item3);
		}
	}

	public override void BombExplode(Seed bomb)
	{
		int num = seeds.IndexOf(bomb);
		int num2 = 0;
		List<Seed> list = new List<Seed>();
		int num3 = num;
		for (int i = 0; i < 2; i++)
		{
			num3++;
			if (num3 > seeds.Count - 1)
			{
				num3 = 0;
			}
			if (seeds[num3].seedState != Seed.SeedState.Eaten && seeds[num3].seedState != Seed.SeedState.Respawn)
			{
				list.Add(seeds[num3]);
				if (seeds[num3].seedType == Seed.SeedType.Good)
				{
					GoodSeedEaten();
				}
				else if (seeds[num3].seedType == Seed.SeedType.Respawn)
				{
					GoodSeedEaten();
				}
				else if (seeds[num3].seedType == Seed.SeedType.Bad && seeds[num3].seedState == Seed.SeedState.Worm)
				{
					num2++;
					WormEaten(seeds[num3].wormSize);
				}
			}
		}
		num3 = num;
		for (int j = 0; j < 2; j++)
		{
			num3--;
			if (num3 < 0)
			{
				num3 = seeds.Count - 1;
			}
			if (seeds[num3].seedState != Seed.SeedState.Eaten && seeds[num3].seedState != Seed.SeedState.Respawn)
			{
				list.Add(seeds[num3]);
				if (seeds[num3].seedType == Seed.SeedType.Good)
				{
					GoodSeedEaten();
				}
				else if (seeds[num3].seedType == Seed.SeedType.Respawn)
				{
					GoodSeedEaten();
				}
				else if (seeds[num3].seedType == Seed.SeedType.Bad && seeds[num3].seedState == Seed.SeedState.Worm)
				{
					num2++;
					WormEaten(seeds[num3].wormSize);
				}
			}
		}
		StartCoroutine(ExplodeCoroutine(list));
	}

	public override void GoodSeedEaten()
	{
		goodSeedCounter--;
	}

	protected virtual IEnumerator ExplodeCoroutine(List<Seed> neighbours)
	{
		float duration = 1f;
		float timer = 0f;
		List<GameObject> toExplode = new List<GameObject>();
		foreach (Seed neighbour in neighbours)
		{
			toExplode.Add(neighbour.TakeSeed());
		}
		while (timer < duration)
		{
			timer += Time.deltaTime;
			if (timer > duration)
			{
				timer = duration;
			}
			foreach (GameObject item in toExplode)
			{
				Transform transform = item.transform;
				Vector3 position = transform.position;
				float d = 10f * Time.deltaTime;
				Vector3 eulerAngles = item.transform.rotation.eulerAngles;
				float x = Mathf.Sin((0f - eulerAngles.z) * ((float)Math.PI / 180f));
				Vector3 eulerAngles2 = item.transform.rotation.eulerAngles;
				transform.position = position + d * new Vector3(x, Mathf.Cos((0f - eulerAngles2.z) * ((float)Math.PI / 180f)), 0f);
			}
			yield return null;
		}
		foreach (GameObject item2 in toExplode)
		{
			UnityEngine.Object.Destroy(item2);
		}
	}

	protected IEnumerator WormCoroutine()
	{
		yield return new WaitUntil(() => !HullPresent());
		yield return new WaitForSeconds(2f);
		while (wormCounter > 0)
		{
			List<Seed> badSeeds = new List<Seed>();
			foreach (Seed seed in seeds)
			{
				if (seed.seedType == Seed.SeedType.Bad && seed.seedState == Seed.SeedState.Default)
				{
					badSeeds.Add(seed);
				}
			}
			if (badSeeds.Count == 0)
			{
				break;
			}
			int i = UnityEngine.Random.Range(0, badSeeds.Count);
			yield return new WaitUntil(() => badSeeds[i].IsOpened());
			if (badSeeds[i].seedState == Seed.SeedState.Default)
			{
				badSeeds[i].WormGetOut();
			}
			yield return new WaitForSeconds(5f);
		}
	}

	public override Seed GetNextComboSeed(Seed seed)
	{
		int num = seeds.IndexOf(seed);
		for (int i = 1; i < seeds.Count; i++)
		{
			int num2 = num + i;
			if (num2 > seeds.Count - 1)
			{
				num2 = num - seeds.Count + i;
			}
			if (seeds[num2].seedType == Seed.SeedType.Good && seeds[num2].seedState == Seed.SeedState.Default)
			{
				return seeds[num2];
			}
		}
		return null;
	}

	public override FruitType GetFruitType()
	{
		return FruitType.Corn;
	}

	public override Transform GetSeedHolder()
	{
		return seedHolder;
	}

	protected virtual float GetSizeRandomizationRange()
	{
		return 0.1f;
	}
}
