using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garnet : Level
{
	[SerializeField]
	private Seed seedPrefab;

	[SerializeField]
	private Insect insectPrefab;

	[SerializeField]
	private Transform seedHolder;

	[SerializeField]
	private Transform henHolder;

	[SerializeField]
	private SpriteRenderer middle;

	[SerializeField]
	protected List<int> seedsOnRadiuses;

	[SerializeField]
	private List<int> badSeedsOnRadiuses;

	[SerializeField]
	private List<int> bombsOnRadiuses;

	private List<int> goodSeedsOnRadiuses;

	private int currentRow;

	public float radiusShift = 0.35f;

	[SerializeField]
	private List<int> laserSeedsAmount;

	[SerializeField]
	private List<int> colorSeedsAmount;

	[SerializeField]
	private GameObject laserPrefab;

	[SerializeField]
	private GameObject laserPrefabBoom;

	protected override void Init()
	{
		if ((bool)middle)
		{
			middle.transform.localScale = new Vector3(2.3f * radius, 2.3f * radius, 1f);
			middle.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
		}
		henHolder.transform.localPosition = new Vector3(0f, 0f - radius, 0f);
		goodSeedsOnRadiuses = new List<int>();
		for (int i = 0; i < seedsOnRadiuses.Count; i++)
		{
			goodSeedsOnRadiuses.Add(0);
		}
		BaseSceneManager<Hen>.Instance.correction = new Vector3(0f, 0f, 0f);
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		List<int> list4 = new List<int>();
		UnityEngine.Random.InitState(Mathf.RoundToInt((float)DateTime.Now.Second * 1000f));
		int num = 0;
		int num2 = 0;
		for (int j = 0; j < seedsOnRadiuses.Count; j++)
		{
			int num3 = -1;
			if (j == 0 && gameplayChangingSeedType != 0)
			{
				num3 = UnityEngine.Random.Range(0, seedsOnRadiuses[j]);
				if (num3 + 1 < seedsOnRadiuses[j])
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
					list.Add(seedsOnRadiuses[j] - 1);
				}
			}
			list2.Clear();
			list.Clear();
			list3.Clear();
			list4.Clear();
			num += seedsOnRadiuses[j];
			int num4 = 0;
			while (list2.Count < bombsOnRadiuses[j] && num4 < 100)
			{
				num4++;
				int num5 = UnityEngine.Random.Range(num2, num);
				bool flag = true;
				foreach (int item in list2)
				{
					if (Mathf.Abs(num3 - num5) < 4 || Mathf.Abs(item - num5) < 4 || Mathf.Abs(item + seedsOnRadiuses[j] - num5) < 4)
					{
						flag = false;
					}
				}
				if (flag)
				{
					list2.Add(num5);
					if (num5 + 1 < num)
					{
						list.Add(num5 + 1);
					}
					else
					{
						list.Add(0);
					}
					if (num5 - 1 >= num2)
					{
						list.Add(num5 - 1);
					}
					else
					{
						list.Add(seedsAmount - 1);
					}
				}
			}
			num4 = 0;
			while (list3.Count < laserSeedsAmount[j] && num4 < 100)
			{
				num4++;
				int num6 = UnityEngine.Random.Range(num2, num);
				bool flag2 = true;
				foreach (int item2 in list3)
				{
					if (Mathf.Abs(num3 - num6) < 4 || Mathf.Abs(item2 - num6) < 4 || Mathf.Abs(item2 + seedsOnRadiuses[j] - num6) < 4)
					{
						flag2 = false;
					}
				}
				foreach (int item3 in list2)
				{
					if (Mathf.Abs(num3 - num6) < 4 || Mathf.Abs(item3 - num6) < 4 || Mathf.Abs(item3 + seedsOnRadiuses[j] - num6) < 4)
					{
						flag2 = false;
					}
				}
				if (flag2)
				{
					list3.Add(num6);
					if (num6 + 1 < num)
					{
						list.Add(num6 + 1);
					}
					else
					{
						list.Add(0);
					}
					if (num6 - 1 >= num2)
					{
						list.Add(num6 - 1);
					}
					else
					{
						list.Add(seedsAmount - 1);
					}
				}
			}
			num4 = 0;
			while (list4.Count < colorSeedsAmount[j] && num4 < 100)
			{
				num4++;
				int num7 = UnityEngine.Random.Range(num2, num);
				bool flag3 = true;
				foreach (int item4 in list3)
				{
					if (Mathf.Abs(num3 - num7) < 4 || Mathf.Abs(item4 - num7) < 4 || Mathf.Abs(item4 + seedsOnRadiuses[j] - num7) < 4)
					{
						flag3 = false;
					}
				}
				foreach (int item5 in list2)
				{
					if (Mathf.Abs(num3 - num7) < 4 || Mathf.Abs(item5 - num7) < 4 || Mathf.Abs(item5 + seedsOnRadiuses[j] - num7) < 4)
					{
						flag3 = false;
					}
				}
				if (flag3)
				{
					list4.Add(num7);
				}
			}
			num4 = 0;
			while (list.Count < badSeedsOnRadiuses[j] && num4 < 100)
			{
				num4++;
				int num8 = UnityEngine.Random.Range(num2, num);
				if (num8 != num3 && !list.Contains(num8) && !list4.Contains(num8) && !list2.Contains(num8) && !list3.Contains(num8))
				{
					list.Add(num8);
				}
			}
			List<float> list5 = new List<float>();
			float num9 = 0f;
			for (int k = 0; k < seedsOnRadiuses[j]; k++)
			{
				float num10 = (!list2.Contains(k) && !list3.Contains(k) && k != num3 && !list4.Contains(k)) ? UnityEngine.Random.Range(90f, 110f) : 100f;
				list5.Add(num10);
				num9 += num10;
			}
			for (int l = 0; l < seedsOnRadiuses[j]; l++)
			{
				List<float> list6;
				int index;
				(list6 = list5)[index = l] = list6[index] * (360f / num9);
			}
			List<float> list7 = new List<float>();
			list7.Add(list5[0] / 2f);
			float num11 = list5[0] / 2f;
			for (int m = 1; m < seedsOnRadiuses[j]; m++)
			{
				num11 += list5[m - 1] / 2f;
				num11 += list5[m] / 2f;
				list7.Add(num11);
			}
			for (int n = 0; n < seedsOnRadiuses[j]; n++)
			{
				Seed seed = UnityEngine.Object.Instantiate(seedPrefab, seedHolder);
				if (list.Contains(n + num2))
				{
					seed.InitType(Seed.SeedType.Bad);
					badSeeds.Add(seed);
				}
				else if (list2.Contains(n + num2))
				{
					seed.InitType(Seed.SeedType.Bomb);
				}
				else if (list3.Contains(n + num2))
				{
					seed.InitType(Seed.SeedType.Laser);
				}
				else if (list4.Contains(n + num2))
				{
					seed.InitType(Seed.SeedType.Color);
				}
				else if (num3 == n)
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
				else
				{
					seed.InitType(Seed.SeedType.Good);
					List<int> list8;
					int index2;
					(list8 = goodSeedsOnRadiuses)[index2 = j] = list8[index2] + 1;
					goodSeedCounter++;
				}
				seed.isOnTopRadius = (j == 0);
				seeds.Add(seed);
				float num12 = radiusShift * (float)j;
				float num13 = radius - num12;
				float f = list7[n] * ((float)Math.PI / 180f);
				float num14 = list5[n] / (360f / (float)seedsAmount) * num13 * (20f / (float)seedsAmount);
				seed.transform.localPosition = new Vector3(num13 * Mathf.Sin(f), num13 * Mathf.Cos(f), 0f);
				seed.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f - list7[n]));
				seed.transform.localScale = new Vector3(num14, num14, 1f);
				SpecialSeedSetup(seed);
			}
			num2 += seedsOnRadiuses[j];
		}
		if (insectPresent)
		{
			StartCoroutine(InsectAppearCoroutine());
		}
		wormCoroutine = StartCoroutine(WormCoroutine());
		wormCounter = 3;
		goodSeedsOnStart = goodSeedCounter;
	}

	protected virtual void SpecialSeedSetup(Seed seed)
	{
	}

	public override void GoodSeedEaten()
	{
		goodSeedCounter--;
		List<int> list;
		int index;
		(list = goodSeedsOnRadiuses)[index = currentRow] = list[index] - 1;
		if (goodSeedsOnRadiuses[currentRow] == 0)
		{
			StartCoroutine(DestroyLayerCoroutine(currentRow));
			henHolder.transform.localPosition += new Vector3(0f, radiusShift * 1.1f, 0f);
			if (currentRow < goodSeedsOnRadiuses.Count - 1)
			{
				currentRow++;
			}
			UpdateSeedVisual();
			if (wormCoroutine != null)
			{
				StopCoroutine(wormCoroutine);
			}
			wormCoroutine = StartCoroutine(WormCoroutine());
		}
	}

	public override void IncreaseSeedCounter(Seed s)
	{
		int num = seeds.IndexOf(s);
		int num2 = 0;
		int i;
		for (i = 0; i < goodSeedsOnRadiuses.Count && (num < num2 || num >= num2 + seedsOnRadiuses[i]); i++)
		{
			num2 += seedsOnRadiuses[i];
		}
		goodSeedCounter++;
		List<int> list;
		int index;
		(list = goodSeedsOnRadiuses)[index = i] = list[index] + 1;
	}

	public override void DecreaseSeedCounter(Seed s)
	{
		int num = seeds.IndexOf(s);
		int num2 = 0;
		int i;
		for (i = 0; i < seedsOnRadiuses.Count && (num < num2 || num >= num2 + seedsOnRadiuses[i]); i++)
		{
			num2 += seedsOnRadiuses[i];
		}
		goodSeedCounter--;
		List<int> list;
		int index;
		(list = goodSeedsOnRadiuses)[index = i] = list[index] - 1;
		if (goodSeedsOnRadiuses[currentRow] == 0)
		{
			StartCoroutine(DestroyLayerCoroutine(currentRow));
			henHolder.transform.localPosition += new Vector3(0f, radiusShift * 1.1f, 0f);
			currentRow++;
			UpdateSeedVisual();
			if (wormCoroutine != null)
			{
				StopCoroutine(wormCoroutine);
			}
			wormCoroutine = StartCoroutine(WormCoroutine());
		}
	}

	public void GoodSeedEaten(int row)
	{
		goodSeedCounter--;
		List<int> list;
		int index;
		(list = goodSeedsOnRadiuses)[index = row] = list[index] - 1;
		if (goodSeedsOnRadiuses[currentRow] == 0)
		{
			StartCoroutine(DestroyLayerCoroutine(row));
			UpdateSeedVisual();
			henHolder.transform.localPosition += new Vector3(0f, radiusShift * 1.1f, 0f);
			currentRow++;
			UpdateSeedVisual();
			if (wormCoroutine != null)
			{
				StopCoroutine(wormCoroutine);
			}
			wormCoroutine = StartCoroutine(WormCoroutine());
		}
	}

	private void Update()
	{
		if (!BaseSceneManager<Gameplay>.Instance.IsPaused())
		{
			float num = radius - (float)currentRow * radiusShift * 1.1f;
			float num2 = (radius + 1f) / (num + 1f);
			seedHolder.Rotate(0f, 0f, num2 * speed * Time.deltaTime);
			henHolder.parent.Rotate(0f, 0f, num2 * henSpeed * Time.deltaTime);
			if (insect != null)
			{
				Vector3 eulerAngles = insect.transform.eulerAngles;
				float z = eulerAngles.z;
				z += Time.deltaTime * speed * 2.5f;
				insect.transform.position = base.transform.position + new Vector3(num * Mathf.Cos(z * ((float)Math.PI / 180f)), num * Mathf.Sin(z * ((float)Math.PI / 180f)), 0f);
				insect.transform.rotation = Quaternion.Euler(0f, 0f, z);
			}
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

	protected virtual IEnumerator DestroyLayerCoroutine(int layer)
	{
		List<GameObject> notEatens = new List<GameObject>();
		float duration = 0.5f;
		float timer = 0f;
		int summ = 0;
		for (int i = 0; i < layer; i++)
		{
			summ += seedsOnRadiuses[i];
		}
		for (int j = summ; j < summ + seedsOnRadiuses[layer]; j++)
		{
			if (seeds[j].seedState != Seed.SeedState.Eaten)
			{
				notEatens.Add(seeds[j].TakeSeed());
			}
		}
		while (timer < duration)
		{
			timer += Time.deltaTime;
			if (timer > duration)
			{
				timer = duration;
			}
			foreach (GameObject item in notEatens)
			{
				item.transform.position += new Vector3(0f, -10f * Time.deltaTime, 0f);
				item.transform.Rotate(new Vector3(0f, 0f, -10f * Time.deltaTime));
			}
			yield return null;
		}
		foreach (GameObject item2 in notEatens)
		{
			UnityEngine.Object.Destroy(item2);
		}
	}

	private IEnumerator InsectAppearCoroutine()
	{
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

	public void UpdateSeedVisual()
	{
		if ((bool)middle)
		{
			middle.transform.localScale = new Vector3(2.3f * (radius - radiusShift * (float)currentRow), 2.3f * (radius - radiusShift * (float)currentRow), 1f);
		}
		int num = currentRow;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < num; i++)
		{
			num2 += seedsOnRadiuses[i];
		}
		for (int j = 0; j < Mathf.Min(num + 1, seedsOnRadiuses.Count); j++)
		{
			num3 += seedsOnRadiuses[j];
		}
		for (int k = num2; k < num3; k++)
		{
			seeds[k].isOnTopRadius = true;
			seeds[k].UpdateVisual();
		}
	}

	public override int LaserHuyak(Seed laser)
	{
		int num = seeds.IndexOf(laser);
		int num2 = 0;
		List<Seed> list = new List<Seed>();
		int num3 = num;
		int num4 = 0;
		int num5 = currentRow;
		int num6 = 0;
		int num7 = 0;
		for (int i = 0; i < num5; i++)
		{
			num6 += seedsOnRadiuses[i];
		}
		for (int j = 0; j < Mathf.Min(num5 + 1, seedsOnRadiuses.Count); j++)
		{
			num7 += seedsOnRadiuses[j];
		}
		for (int k = num6; k < num7; k++)
		{
			if ((seeds[k].seedType == Seed.SeedType.Good || seeds[k].seedType == Seed.SeedType.Bad) && seeds[k].seedState == Seed.SeedState.Default)
			{
				num4++;
			}
		}
		int num8 = 0;
		while (num8 < Mathf.Min(5, num4))
		{
			num3 = UnityEngine.Random.Range(num6, num7);
			if (seeds[num3].seedState != Seed.SeedState.Eaten && seeds[num3].seedState != Seed.SeedState.Respawn && !list.Contains(seeds[num3]))
			{
				num8++;
				list.Add(seeds[num3]);
			}
		}
		StartCoroutine(LaserExplodeCoroutine(laser, list));
		int num9 = 0;
		foreach (Seed item in list)
		{
			num9 = ((item.seedType != Seed.SeedType.Bad) ? (num9 + 1) : (num9 + 3));
		}
		return num9 + num2 * 10;
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
		for (int i = 0; i < neighbours.Count; i++)
		{
			if (neighbours[i].seedState == Seed.SeedState.Default)
			{
				if (neighbours[i].seedType == Seed.SeedType.Good)
				{
					GoodSeedEaten();
				}
				else if (neighbours[i].seedType == Seed.SeedType.Respawn)
				{
					GoodSeedEaten();
				}
				else if (neighbours[i].seedType == Seed.SeedType.Bad && neighbours[i].seedState == Seed.SeedState.Worm)
				{
					WormEaten(neighbours[i].wormSize);
				}
			}
		}
		if (goodSeedCounter <= 0)
		{
			BaseSceneManager<Gameplay>.Instance.Finish(isFailed: false);
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
		bomb.Explode();
		int num = seeds.IndexOf(bomb);
		int num2 = 0;
		List<Seed> list = new List<Seed>();
		int[] array = new int[2];
		int[] array2 = new int[2];
		int num3 = currentRow;
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < num3 + i; j++)
			{
				array[i] += seedsOnRadiuses[j];
			}
			for (int k = 0; k < Mathf.Min(num3 + 1 + i, seedsOnRadiuses.Count); k++)
			{
				array2[i] += seedsOnRadiuses[k];
			}
		}
		for (int l = num3; l <= Mathf.Min(num3 + 1, seedsOnRadiuses.Count - 1); l++)
		{
			int num4 = array[l - num3] + (num - array[0]) * seedsOnRadiuses[l] / seedsOnRadiuses[num3];
			for (int m = 0; m < 2 - (l - num3); m++)
			{
				num4++;
				if (num4 > array2[l - num3] - 1)
				{
					num4 = array[l - num3];
				}
				if (seeds[num4].seedState != Seed.SeedState.Eaten && !list.Contains(seeds[num4]))
				{
					list.Add(seeds[num4]);
					if (seeds[num4].seedType == Seed.SeedType.Good)
					{
						GoodSeedEaten(l);
					}
					else if (seeds[num4].seedType == Seed.SeedType.Bad && seeds[num4].seedState == Seed.SeedState.Worm)
					{
						num2++;
						WormEaten(seeds[num4].wormSize);
					}
				}
			}
			num4 = array[l - num3] + (l - num3) + (num - array[0]) * seedsOnRadiuses[l] / seedsOnRadiuses[num3];
			for (int n = 0; n < 3 - (l - num3); n++)
			{
				num4--;
				if (num4 < array[l - num3])
				{
					num4 = array2[l - num3] - 1;
				}
				if (seeds[num4].seedState != Seed.SeedState.Eaten && !list.Contains(seeds[num4]))
				{
					list.Add(seeds[num4]);
					if (seeds[num4].seedType == Seed.SeedType.Good)
					{
						GoodSeedEaten(l);
					}
					else if (seeds[num4].seedType == Seed.SeedType.Bad && seeds[num4].seedState == Seed.SeedState.Worm)
					{
						num2++;
						WormEaten(seeds[num4].wormSize);
					}
				}
			}
		}
		StartCoroutine(ExplodeCoroutine(list));
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

	private IEnumerator WormCoroutine()
	{
		yield return new WaitForSeconds(2f);
		while (wormCounter > 0)
		{
			List<Seed> badSeeds = new List<Seed>();
			float summRow = 0f;
			for (int j = 0; j < currentRow + 1; j++)
			{
				summRow += (float)seedsOnRadiuses[j];
			}
			for (int k = 0; k < seeds.Count; k++)
			{
				if ((float)k < summRow && seeds[k].seedType == Seed.SeedType.Bad && seeds[k].seedState == Seed.SeedState.Default)
				{
					badSeeds.Add(seeds[k]);
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
		return FruitType.Garnet;
	}

	public override Transform GetSeedHolder()
	{
		return seedHolder;
	}
}
