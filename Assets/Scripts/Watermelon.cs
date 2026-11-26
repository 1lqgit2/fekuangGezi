using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Watermelon : Level
{
	[SerializeField]
	private List<Color> badSeedsColors;

	[SerializeField]
	private List<Color> goodSeedsColors;

	[SerializeField]
	private Seed seedPrefab;

	[SerializeField]
	private Insect insectPrefab;

	[SerializeField]
	private Transform seedHolder;

	[SerializeField]
	private Transform henHolder;

	[SerializeField]
	private Transform middle;

	[SerializeField]
	private SpriteRenderer middleSprite;

	[SerializeField]
	private List<int> seedsOnRadiuses;

	[SerializeField]
	private List<int> badSeedsOnRadiuses;

	[SerializeField]
	private List<int> bombsOnRadiuses;

	private List<int> goodSeedsOnRadiuses;

	private int currentRow;

	public float angleAmplitude;

	private float currentAngle;

	private float currentAngleHen;

	private float rand;

	private float randPrev;

	public float seedHeight = 0.1f;

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
		currentRow = seedsOnRadiuses.Count - 1;
		middle.transform.localScale = new Vector3(3f * radius, 3f * radius, 1f);
		henHolder.transform.localPosition = new Vector3(0f, radius - seedHeight * radius * (float)seedsOnRadiuses.Count + seedHeight, 0f);
		goodSeedsOnRadiuses = new List<int>();
		BaseSceneManager<Hen>.Instance.correction = new Vector3(0f, 0f - seedHeight, 0f);
		for (int i = 0; i < seedsOnRadiuses.Count; i++)
		{
			goodSeedsOnRadiuses.Add(0);
		}
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		List<int> list4 = new List<int>();
		UnityEngine.Random.InitState(Mathf.RoundToInt((float)DateTime.Now.Second * 1000f));
		int num = 0;
		int num2 = 0;
		for (int j = 0; j < seedsOnRadiuses.Count; j++)
		{
			list2.Clear();
			list.Clear();
			list3.Clear();
			list4.Clear();
			num += seedsOnRadiuses[j];
			int num3 = 0;
			while (list2.Count < bombsOnRadiuses[j] && num3 < 100)
			{
				num3++;
				int num4 = UnityEngine.Random.Range(num2, num);
				bool flag = true;
				foreach (int item2 in list2)
				{
					if (Mathf.Abs(item2 - num4) < 4 || Mathf.Abs(item2 + seedsOnRadiuses[j] - num4) < 4)
					{
						flag = false;
					}
				}
				if (flag)
				{
					list2.Add(num4);
					if (num4 + 1 < num)
					{
						list.Add(num4 + 1);
					}
					else
					{
						list.Add(0);
					}
					if (num4 - 1 >= num2)
					{
						list.Add(num4 - 1);
					}
					else
					{
						list.Add(seedsAmount - 1);
					}
				}
			}
			num3 = 0;
			while (list3.Count < laserSeedsAmount[j] && num3 < 100)
			{
				num3++;
				int num5 = UnityEngine.Random.Range(num2, num);
				bool flag2 = true;
				foreach (int item3 in list3)
				{
					if (Mathf.Abs(item3 - num5) < 4 || Mathf.Abs(item3 + seedsOnRadiuses[j] - num5) < 4)
					{
						flag2 = false;
					}
				}
				foreach (int item4 in list2)
				{
					if (Mathf.Abs(item4 - num5) < 4 || Mathf.Abs(item4 + seedsOnRadiuses[j] - num5) < 4)
					{
						flag2 = false;
					}
				}
				if (flag2)
				{
					list3.Add(num5);
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
			num3 = 0;
			while (list4.Count < colorSeedsAmount[j] && num3 < 100)
			{
				int num6 = UnityEngine.Random.Range(num2, num);
				bool flag3 = true;
				foreach (int item5 in list3)
				{
					if (Mathf.Abs(item5 - num6) < 4 || Mathf.Abs(item5 + seedsOnRadiuses[j] - num6) < 4)
					{
						flag3 = false;
					}
				}
				foreach (int item6 in list2)
				{
					if (Mathf.Abs(item6 - num6) < 4 || Mathf.Abs(item6 + seedsOnRadiuses[j] - num6) < 4)
					{
						flag3 = false;
					}
				}
				if (flag3)
				{
					list4.Add(num6);
				}
			}
			num3 = 0;
			while (list.Count < badSeedsOnRadiuses[j] && num3 < 100)
			{
				num3++;
				int item = UnityEngine.Random.Range(num2, num);
				if (!list4.Contains(item) && !list.Contains(item) && !list2.Contains(item) && !list3.Contains(item))
				{
					list.Add(item);
				}
			}
			List<float> list5 = new List<float>();
			float num7 = 0f;
			for (int k = 0; k < seedsOnRadiuses[j]; k++)
			{
				float num8 = (!list4.Contains(k) && !list2.Contains(k) && !list3.Contains(k)) ? UnityEngine.Random.Range(90f, 110f) : 100f;
				list5.Add(num8);
				num7 += num8;
			}
			for (int l = 0; l < seedsOnRadiuses[j]; l++)
			{
				List<float> list6;
				int index;
				(list6 = list5)[index = l] = list6[index] * (2f * angleAmplitude / num7);
			}
			List<float> list7 = new List<float>();
			list7.Add(0f - angleAmplitude + list5[0] / 2f);
			float num9 = 0f - angleAmplitude + list5[0] / 2f;
			for (int m = 1; m < seedsOnRadiuses[j]; m++)
			{
				num9 += list5[m - 1] / 2f;
				num9 += list5[m] / 2f;
				list7.Add(num9);
			}
			int num10 = 0;
			for (int n = 0; n < seedsOnRadiuses[j]; n++)
			{
				Seed seed = UnityEngine.Object.Instantiate(seedPrefab, seedHolder);
				if (list.Contains(n + num2))
				{
					int num11;
					for (num11 = UnityEngine.Random.Range(0, badSeedsColors.Count); num11 == num10; num11 = UnityEngine.Random.Range(0, badSeedsColors.Count))
					{
					}
					num10 = num11;
					seed.SetColorBad(badSeedsColors[num11]);
					seed.SetColorGood(goodSeedsColors[0]);
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
				else
				{
					seed.InitType(Seed.SeedType.Good);
					int num12;
					for (num12 = UnityEngine.Random.Range(0, goodSeedsColors.Count); num12 == num10; num12 = UnityEngine.Random.Range(0, goodSeedsColors.Count))
					{
					}
					num10 = num12;
					seed.SetColorBad(badSeedsColors[0]);
					seed.SetColorGood(goodSeedsColors[num12]);
					List<int> list8;
					int index2;
					(list8 = goodSeedsOnRadiuses)[index2 = j] = list8[index2] + 1;
					goodSeedCounter++;
				}
				seeds.Add(seed);
				randPrev = rand;
				rand = UnityEngine.Random.Range(-0.1f, 0.1f);
				if (n == 0)
				{
					randPrev = 0f;
				}
				if (n == seedsOnRadiuses[j] - 1)
				{
					rand = 0f;
				}
				float num13 = seedHeight * (float)(j + 1) * radius;
				float num14 = seedHeight * (float)j * radius;
				float num15 = radius - num13;
				float num16 = radius - num14;
				float f = list7[n] * ((float)Math.PI / 180f);
				float num17 = list5[n] / (2f * angleAmplitude / (float)seedsAmount) * num15 * (5f / (float)seedsAmount) / radius * angleAmplitude / 46f;
				float num18 = list5[n] / (2f * angleAmplitude / (float)seedsAmount) * num16 * (5f / (float)seedsAmount) / radius * angleAmplitude / 46f;
				seed.SetLayer(50 * (seedsOnRadiuses.Count - j) - n);
				Vector3 vector = base.transform.position + new Vector3(num15 * Mathf.Sin(f), num15 * Mathf.Cos(f), 0f);
				Quaternion quaternion = Quaternion.Euler(new Vector3(0f, 0f, 0f - list7[n] + 180f));
				List<Vector3> list9 = new List<Vector3>();
				List<Vector3> list10 = new List<Vector3>();
				List<Vector3> list11 = list9;
				float num19 = 0f - num17;
				Vector3 localScale = base.transform.localScale;
				list11.Add(new Vector3(num19 / localScale.x / 2f, 0f, 0f));
				List<Vector3> list12 = list9;
				float num20 = 0f - num18;
				Vector3 localScale2 = base.transform.localScale;
				float x = num20 / localScale2.x / 2f + rand;
				float num21 = (0f - (seedHeight + 0.01f)) * radius;
				Vector3 localScale3 = base.transform.localScale;
				list12.Add(new Vector3(x, num21 / localScale3.x, 0f));
				List<Vector3> list13 = list9;
				float num22 = num18;
				Vector3 localScale4 = base.transform.localScale;
				float x2 = num22 / localScale4.x / 2f + randPrev;
				float num23 = (0f - (seedHeight + 0.01f)) * radius;
				Vector3 localScale5 = base.transform.localScale;
				list13.Add(new Vector3(x2, num23 / localScale5.x, 0f));
				List<Vector3> list14 = list9;
				float num24 = num17;
				Vector3 localScale6 = base.transform.localScale;
				list14.Add(new Vector3(num24 / localScale6.x / 2f, 0f, 0f));
				float num25 = 0.05f;
				List<Vector3> list15 = list10;
				float num26 = 0f - num17;
				Vector3 localScale7 = base.transform.localScale;
				list15.Add(new Vector3(num26 / localScale7.x / 2f - num25, num25, 0f));
				List<Vector3> list16 = list10;
				float num27 = 0f - num18;
				Vector3 localScale8 = base.transform.localScale;
				float x3 = num27 / localScale8.x / 2f - num25;
				float num28 = (0f - (seedHeight + 0.01f)) * radius;
				Vector3 localScale9 = base.transform.localScale;
				list16.Add(new Vector3(x3, num28 / localScale9.x - num25, 0f));
				List<Vector3> list17 = list10;
				float num29 = num18;
				Vector3 localScale10 = base.transform.localScale;
				float x4 = num29 / localScale10.x / 2f + num25;
				float num30 = (0f - (seedHeight + 0.01f)) * radius;
				Vector3 localScale11 = base.transform.localScale;
				list17.Add(new Vector3(x4, num30 / localScale11.x - num25, 0f));
				List<Vector3> list18 = list10;
				float num31 = num17;
				Vector3 localScale12 = base.transform.localScale;
				list18.Add(new Vector3(num31 / localScale12.x / 2f + num25, num25, 0f));
				seed.SetLinePositions(list9, list10);
				seed.transform.localPosition = new Vector3(num15 * Mathf.Sin(f), num15 * Mathf.Cos(f), 0f);
				seed.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f - list7[n] + 180f));
				seed.transform.localScale = new Vector3(1f, 1f, 1f);
				seed.worm.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
				seed.viemka.transform.localScale = new Vector3(num17 * 0.2f, (0f - num17) * 0.1f, 1f);
				seed.viemka.gameObject.SetActive(value: false);
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

	public void GoodSeedEaten(int row)
	{
		goodSeedCounter--;
		List<int> list;
		int index;
		(list = goodSeedsOnRadiuses)[index = row] = list[index] - 1;
		if (goodSeedsOnRadiuses[row] == 0)
		{
			StartCoroutine(DestroyLayerCoroutine(row));
			henHolder.transform.localPosition += new Vector3(0f, seedHeight * radius, 0f);
			currentRow--;
			UpdateSeedVisual();
			if (wormCoroutine != null)
			{
				StopCoroutine(wormCoroutine);
			}
			wormCoroutine = StartCoroutine(WormCoroutine());
		}
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
			henHolder.transform.localPosition += new Vector3(0f, seedHeight, 0f) * radius;
			currentRow--;
			UpdateSeedVisual();
			if (wormCoroutine != null)
			{
				StopCoroutine(wormCoroutine);
			}
			wormCoroutine = StartCoroutine(WormCoroutine());
		}
	}

	public override bool IsSeedAccesible(Seed seed)
	{
		int num = seeds.IndexOf(seed);
		int num2 = 0;
		for (int i = 0; i < currentRow; i++)
		{
			num2 += seedsOnRadiuses[i];
		}
		return num >= num2;
	}

	public void UpdateSeedVisual()
	{
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
	}

	private void Update()
	{
		if (!BaseSceneManager<Gameplay>.Instance.IsPaused())
		{
			currentAngle += speed * Time.deltaTime;
			currentAngleHen += henSpeed * Time.deltaTime;
			if (currentRow >= 0 && Mathf.Abs(currentAngle) + Mathf.Abs(currentAngleHen) > angleAmplitude + 0.5f)
			{
				speed *= -1f;
				henSpeed *= -1f;
				currentAngle += speed * Time.deltaTime;
				currentAngleHen += henSpeed * Time.deltaTime;
			}
			seedHolder.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, currentAngle));
			henHolder.parent.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, currentAngleHen));
			if (insect != null)
			{
				Vector3 eulerAngles = insect.transform.eulerAngles;
				float z = eulerAngles.z;
				z += Time.deltaTime * speed * 2.5f;
				insect.transform.position = base.transform.position + new Vector3(radius * Mathf.Cos(z * ((float)Math.PI / 180f)), radius * Mathf.Sin(z * ((float)Math.PI / 180f)), 0f);
				insect.transform.rotation = Quaternion.Euler(0f, 0f, z);
			}
		}
	}

	public override Transform GetHenHolder(Vector3 initialPosition)
	{
		henHolder.transform.localPosition = new Vector3(0f, radius - seedHeight * radius * (float)seedsOnRadiuses.Count + seedHeight, 0f);
		henHolder.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
		return henHolder;
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
			henHolder.transform.localPosition += new Vector3(0f, seedHeight, 0f) * radius;
			currentRow--;
			if (wormCoroutine != null)
			{
				StopCoroutine(wormCoroutine);
			}
			wormCoroutine = StartCoroutine(WormCoroutine());
		}
	}

	private IEnumerator DestroyLayerCoroutine(int layer)
	{
		List<GameObject> notEatens = new List<GameObject>();
		float duration = 0.5f;
		float timer = 0f;
		int summMin = 0;
		for (int i = 0; i < layer; i++)
		{
			summMin += seedsOnRadiuses[i];
		}
		int summMax = 0;
		for (int j = 0; j <= layer; j++)
		{
			summMax += seedsOnRadiuses[j];
		}
		for (int k = summMin; k < summMax; k++)
		{
			if (seeds[k].seedType == Seed.SeedType.Bad)
			{
				notEatens.Add(seeds[k].TakeSeed());
				continue;
			}
			seeds[k].seedState = Seed.SeedState.Eaten;
			seeds[k].UpdateVisual();
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
				item.SetActive(value: true);
				item.GetComponent<SpriteRenderer>().enabled = true;
				item.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
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

	public override void BombExplode(Seed bomb)
	{
		int num = seeds.IndexOf(bomb);
		int num2 = 0;
		int num3 = currentRow;
		List<Seed> list = new List<Seed>();
		int num4 = num;
		int[] array = new int[2];
		int[] array2 = new int[2];
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < num3 - i; j++)
			{
				array[i] += seedsOnRadiuses[j];
			}
			for (int k = 0; k < Mathf.Max(num3 - i + 1, 0); k++)
			{
				array2[i] += seedsOnRadiuses[k];
			}
		}
		for (int l = Mathf.Max(0, num3 - 1); l <= num3; l++)
		{
			num4 = ((l != num3) ? (array[num3 - l] + (num - array[0]) * seedsOnRadiuses[l] / seedsOnRadiuses[num3]) : num);
			for (int m = 0; m < 2; m++)
			{
				num4++;
				if (num4 < array2[num3 - l] && seeds[num4].seedState != Seed.SeedState.Eaten)
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
			num4 = ((l != num3) ? (array[num3 - l] + (num - array[0]) * seedsOnRadiuses[l] / seedsOnRadiuses[num3] + 1) : num);
			for (int n = 0; n < 2; n++)
			{
				num4--;
				if (num4 > array[num3 - l] - 1 && seeds[num4].seedState != Seed.SeedState.Eaten)
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

	private IEnumerator ExplodeCoroutine(List<Seed> neighbours)
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
			for (int i = 0; i < currentRow; i++)
			{
				summRow += (float)seedsOnRadiuses[i];
			}
			int bdSeeds = 0;
			for (int j = 0; j < seeds.Count; j++)
			{
				if ((float)j >= summRow && seeds[j].seedType == Seed.SeedType.Bad && seeds[j].seedState == Seed.SeedState.Default)
				{
					badSeeds.Add(seeds[j]);
					bdSeeds++;
				}
			}
			if (badSeeds.Count == 0)
			{
				break;
			}
			int min = 0;
			badSeeds[UnityEngine.Random.Range(min, bdSeeds)].WormGetOut();
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

	public override Vector3 GetStartPosition()
	{
		return new Vector3(0f, -0.5f, 0f);
	}

	public override FruitType GetFruitType()
	{
		return FruitType.Watermelon;
	}

	public override Transform GetSeedHolder()
	{
		return seedHolder;
	}

	public override Transform GetCenterOfExplosion()
	{
		return middleSprite.transform;
	}
}
