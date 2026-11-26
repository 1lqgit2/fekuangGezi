using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pea : Level
{
	[SerializeField]
	private List<Spline> pods;

	[SerializeField]
	private PeaSeed seedPrefab;

	[SerializeField]
	private Transform henHolder;

	[SerializeField]
	private Transform center;

	[SerializeField]
	private float peaSpeed;

	[SerializeField]
	private Insect insectPrefab;

	private List<List<PeaSeed>> mobileSeedsOnPods;

	private float henWayLength;

	private List<float> podStarts;

	private List<float> podEnds;

	private float currentHenholderPositionOnWay;

	private float currentInsectPositionOnWay;

	protected override void Init()
	{
		int num = -1;
		mobileSeedsOnPods = new List<List<PeaSeed>>();
		int num2 = seedsAmount / pods.Count;
		BaseSceneManager<Hen>.Instance.correction = new Vector3(0f, 0f, 0f);
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		UnityEngine.Random.InitState(Mathf.RoundToInt((float)DateTime.Now.Second * 1000f));
		if (gameplayChangingSeedType != 0)
		{
			num = UnityEngine.Random.Range(0, seedsAmount);
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
		int num3 = 0;
		while (list2.Count < bombsAmount && num3 < 100)
		{
			num3++;
			int num4 = UnityEngine.Random.Range(0, seedsAmount);
			bool flag = true;
			foreach (int item in list2)
			{
				if (Mathf.Abs(item - num4) < 4 || Mathf.Abs(item + seedsAmount - num4) < 4)
				{
					flag = false;
				}
			}
			if (Mathf.Abs(num - num4) < 4 || Mathf.Abs(num + seedsAmount - num4) < 4)
			{
				flag = false;
			}
			if (flag)
			{
				list2.Add(num4);
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
		num3 = 0;
		while (list.Count < badSeedsAmount && num3 < 100)
		{
			num3++;
			int num5 = UnityEngine.Random.Range(0, seedsAmount);
			if (!list.Contains(num5) && !list2.Contains(num5) && num5 != num)
			{
				list.Add(num5);
			}
		}
		foreach (Spline pod in pods)
		{
			pod.CalculateAccurateValues();
		}
		for (int i = 0; i < pods.Count; i++)
		{
			mobileSeedsOnPods.Add(new List<PeaSeed>());
			float accurateLength = pods[i].GetAccurateLength();
			List<float> list3 = new List<float>();
			float num6 = 0f;
			for (int j = 0; j < num2; j++)
			{
				float num7 = (!list2.Contains(j + num2 * i) && j != num) ? UnityEngine.Random.Range(80f, 120f) : 100f;
				list3.Add(num7);
				num6 += num7;
			}
			for (int k = 0; k < num2; k++)
			{
				List<float> list4;
				int index;
				(list4 = list3)[index = k] = list4[index] * (accurateLength / num6);
			}
			List<float> list5 = new List<float>();
			list5.Add(list3[0] / 2f);
			float num8 = list3[0] / 2f;
			for (int l = 1; l < num2; l++)
			{
				num8 += list3[l - 1] / 2f;
				num8 += list3[l] / 2f;
				list5.Add(num8);
			}
			for (int m = 0; m < num2; m++)
			{
				PeaSeed peaSeed = UnityEngine.Object.Instantiate(seedPrefab, center);
				peaSeed.podNumber = i;
				peaSeed.positionInPea = list5[m] / accurateLength;
				peaSeed.transform.position = pods[i].GetAccuratePositionOnSpline(peaSeed.positionInPea);
				peaSeed.transform.rotation = Quaternion.Euler(0f, 0f, pods[i].GetAccurateRotationOnSpline(peaSeed.positionInPea) + 180f);
				peaSeed.transform.localScale = new Vector3(list3[m] / (2f * seedPrefab.GetRadius()), list3[m] / (2f * seedPrefab.GetRadius()), 1f);
				peaSeed.movementDirection = 0;
				mobileSeedsOnPods[i].Add(peaSeed);
				seeds.Add(peaSeed);
			}
		}
		foreach (Seed seed in seeds)
		{
			int num9 = seeds.IndexOf(seed);
			if (list.Contains(num9))
			{
				seed.InitType(Seed.SeedType.Bad);
				badSeeds.Add(seed);
			}
			else if (list2.Contains(num9))
			{
				seed.InitType(Seed.SeedType.Bomb);
			}
			else if (num == num9)
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
				goodSeedCounter++;
			}
		}
		podStarts = new List<float>();
		podEnds = new List<float>();
		henWayLength = 0f;
		podStarts.Add(henWayLength);
		henWayLength += pods[0].GetAccurateLength();
		podEnds.Add(henWayLength);
		for (int n = 1; n < pods.Count; n++)
		{
			henWayLength += (pods[n].GetAccuratePositionOnSpline(0f) - pods[n - 1].GetAccuratePositionOnSpline(1f)).magnitude;
			podStarts.Add(henWayLength);
			henWayLength += pods[n].GetAccurateLength();
			podEnds.Add(henWayLength);
		}
		henWayLength += (pods[pods.Count - 1].GetAccuratePositionOnSpline(1f) - pods[0].GetAccuratePositionOnSpline(0f)).magnitude;
		if (insectPresent)
		{
			StartCoroutine(InsectAppearCoroutine());
		}
		wormCoroutine = StartCoroutine(WormCoroutine());
		wormCounter = 3;
		goodSeedsOnStart = goodSeedCounter;
	}

	private IEnumerator InsectAppearCoroutine()
	{
		currentInsectPositionOnWay = henWayLength / 2f;
		float duration = 0.33f;
		float timer = 0f;
		Vector3 startPosition = base.transform.position + new Vector3(0f, 3f, 0f);
		Vector3 targetPosition = GetPositionOnHenWay(currentInsectPositionOnWay);
		Insect newInsect = UnityEngine.Object.Instantiate(insectPrefab);
		newInsect.transform.position = startPosition;
		newInsect.transform.rotation = Quaternion.Euler(0f, 0f, GetRotationOnHenWay(currentInsectPositionOnWay) - 90f);
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

	private void Update()
	{
		if (BaseSceneManager<Gameplay>.Instance.IsPaused())
		{
			return;
		}
		float radius = seedPrefab.GetRadius();
		for (int i = 0; i < pods.Count; i++)
		{
			List<PeaSeed> list = new List<PeaSeed>();
			for (int j = 0; j < mobileSeedsOnPods[i].Count; j++)
			{
				if (mobileSeedsOnPods[i][j].seedState == Seed.SeedState.Eaten)
				{
					list.Add(mobileSeedsOnPods[i][j]);
				}
			}
			foreach (PeaSeed item in list)
			{
				mobileSeedsOnPods[i].Remove(item);
			}
		}
		for (int k = 0; k < pods.Count; k++)
		{
			float num = Time.deltaTime * peaSpeed / pods[k].GetAccurateLength();
			for (int l = 0; l < mobileSeedsOnPods[k].Count; l++)
			{
				if (mobileSeedsOnPods[k][l].seedType != Seed.SeedType.Bomb && mobileSeedsOnPods[k][l].seedType != Seed.SeedType.UFO && mobileSeedsOnPods[k][l].seedState != Seed.SeedState.Worm)
				{
					if (mobileSeedsOnPods[k][l].movementDirection < 0)
					{
						mobileSeedsOnPods[k][l].predictedPositionInPea = mobileSeedsOnPods[k][l].positionInPea - num;
					}
					else
					{
						mobileSeedsOnPods[k][l].predictedPositionInPea = mobileSeedsOnPods[k][l].positionInPea + num;
					}
					mobileSeedsOnPods[k][l].predictedPosition = pods[k].GetAccuratePositionOnSpline(mobileSeedsOnPods[k][l].predictedPositionInPea);
				}
				else
				{
					mobileSeedsOnPods[k][l].predictedPositionInPea = mobileSeedsOnPods[k][l].positionInPea;
					mobileSeedsOnPods[k][l].predictedPosition = pods[k].GetAccuratePositionOnSpline(mobileSeedsOnPods[k][l].predictedPositionInPea);
				}
			}
		}
		for (int m = 0; m < pods.Count; m++)
		{
			for (int n = 0; n < mobileSeedsOnPods[m].Count; n++)
			{
				float predictedPositionInPea = mobileSeedsOnPods[m][n].predictedPositionInPea;
				float num2 = radius;
				Vector3 localScale = mobileSeedsOnPods[m][n].transform.localScale;
				if (predictedPositionInPea < num2 * localScale.x / pods[m].GetAccurateLength())
				{
					mobileSeedsOnPods[m][n].movementDirection = 1;
					PeaSeed peaSeed = mobileSeedsOnPods[m][n];
					float num3 = radius;
					Vector3 localScale2 = mobileSeedsOnPods[m][n].transform.localScale;
					peaSeed.positionInPea = num3 * localScale2.x / pods[m].GetAccurateLength();
					mobileSeedsOnPods[m][n].transform.position = pods[m].GetAccuratePositionOnSpline(mobileSeedsOnPods[m][n].positionInPea);
					continue;
				}
				float predictedPositionInPea2 = mobileSeedsOnPods[m][n].predictedPositionInPea;
				float num4 = radius;
				Vector3 localScale3 = mobileSeedsOnPods[m][n].transform.localScale;
				if (predictedPositionInPea2 > 1f - num4 * localScale3.x / pods[m].GetAccurateLength())
				{
					mobileSeedsOnPods[m][n].movementDirection = -1;
					PeaSeed peaSeed2 = mobileSeedsOnPods[m][n];
					float num5 = radius;
					Vector3 localScale4 = mobileSeedsOnPods[m][n].transform.localScale;
					peaSeed2.positionInPea = 1f - num5 * localScale4.x / pods[m].GetAccurateLength();
					mobileSeedsOnPods[m][n].transform.position = pods[m].GetAccuratePositionOnSpline(mobileSeedsOnPods[m][n].positionInPea);
					continue;
				}
				if (mobileSeedsOnPods[m][n].movementDirection < 0 && n > 0)
				{
					float magnitude = (mobileSeedsOnPods[m][n].predictedPosition - mobileSeedsOnPods[m][n - 1].transform.position).magnitude;
					float num6 = radius;
					Vector3 localScale5 = mobileSeedsOnPods[m][n].transform.localScale;
					float x = localScale5.x;
					Vector3 localScale6 = mobileSeedsOnPods[m][n - 1].transform.localScale;
					if (magnitude < num6 * (x + localScale6.x))
					{
						mobileSeedsOnPods[m][n].movementDirection++;
						continue;
					}
				}
				if (mobileSeedsOnPods[m][n].movementDirection >= 0 && n < mobileSeedsOnPods[m].Count - 1)
				{
					float magnitude2 = (mobileSeedsOnPods[m][n].predictedPosition - mobileSeedsOnPods[m][n + 1].transform.position).magnitude;
					float num7 = radius;
					Vector3 localScale7 = mobileSeedsOnPods[m][n].transform.localScale;
					float x2 = localScale7.x;
					Vector3 localScale8 = mobileSeedsOnPods[m][n + 1].transform.localScale;
					if (magnitude2 < num7 * (x2 + localScale8.x))
					{
						mobileSeedsOnPods[m][n].movementDirection--;
						continue;
					}
				}
				mobileSeedsOnPods[m][n].positionInPea = mobileSeedsOnPods[m][n].predictedPositionInPea;
				float num8 = 360f * (mobileSeedsOnPods[m][n].predictedPosition - mobileSeedsOnPods[m][n].transform.position).magnitude;
				float num9 = (float)Math.PI * 2f * radius;
				Vector3 localScale9 = mobileSeedsOnPods[m][n].transform.localScale;
				float num10 = num8 / (num9 * localScale9.x);
				Transform transform = mobileSeedsOnPods[m][n].transform;
				Vector3 position = mobileSeedsOnPods[m][n].transform.position;
				transform.Rotate(0f, 0f, (!(position.x > mobileSeedsOnPods[m][n].predictedPosition.x)) ? (0f - num10) : num10);
				mobileSeedsOnPods[m][n].transform.position = mobileSeedsOnPods[m][n].predictedPosition;
			}
		}
		currentHenholderPositionOnWay -= Time.deltaTime * henSpeed;
		if (currentHenholderPositionOnWay < 0f)
		{
			currentHenholderPositionOnWay = henWayLength + currentHenholderPositionOnWay;
		}
		henHolder.transform.position = GetPositionOnHenWay(currentHenholderPositionOnWay);
		henHolder.transform.rotation = Quaternion.Euler(0f, 0f, GetRotationOnHenWay(currentHenholderPositionOnWay) + 90f);
		if (insect != null)
		{
			currentInsectPositionOnWay += Time.deltaTime * henSpeed;
			if (currentInsectPositionOnWay > henWayLength)
			{
				currentInsectPositionOnWay -= henWayLength;
			}
			insect.transform.position = GetPositionOnHenWay(currentInsectPositionOnWay);
			insect.transform.rotation = Quaternion.Euler(0f, 0f, GetRotationOnHenWay(currentInsectPositionOnWay) - 90f);
		}
		center.Rotate(0f, 0f, speed * Time.deltaTime);
	}

	private Vector3 GetPositionOnHenWay(float distance)
	{
		if (distance < 0f || distance > henWayLength)
		{
			return Vector3.zero;
		}
		for (int i = 0; i < pods.Count; i++)
		{
			if (distance < podStarts[i])
			{
				float num = (distance - podEnds[i - 1]) / (podStarts[i] - podEnds[i - 1]);
				num = ((num < 0.25f) ? 0f : ((num > 0.75f) ? 1f : ((!(num < 0.5f)) ? (2f * (num - 0.5f) + 0.5f) : (num * 2f - 0.5f))));
				Vector3 accuratePositionOnSpline = pods[i - 1].GetAccuratePositionOnSpline(1f);
				float x = accuratePositionOnSpline.x;
				Vector3 accuratePositionOnSpline2 = pods[i].GetAccuratePositionOnSpline(0f);
				float x2 = Mathf.Lerp(x, accuratePositionOnSpline2.x, num);
				Vector3 accuratePositionOnSpline3 = pods[i - 1].GetAccuratePositionOnSpline(1f);
				float y = accuratePositionOnSpline3.y;
				Vector3 accuratePositionOnSpline4 = pods[i].GetAccuratePositionOnSpline(0f);
				float y2 = Mathf.Lerp(y, accuratePositionOnSpline4.y, num);
				Vector3 accuratePositionOnSpline5 = pods[i - 1].GetAccuratePositionOnSpline(1f);
				float z = accuratePositionOnSpline5.z;
				Vector3 accuratePositionOnSpline6 = pods[i].GetAccuratePositionOnSpline(0f);
				return new Vector3(x2, y2, Mathf.Lerp(z, accuratePositionOnSpline6.z, num));
			}
			if (distance <= podEnds[i])
			{
				float t = (distance - podStarts[i]) / (podEnds[i] - podStarts[i]);
				return pods[i].GetAccuratePositionOnSpline(t);
			}
		}
		float num2 = (distance - podEnds[pods.Count - 1]) / (henWayLength - podEnds[pods.Count - 1]);
		num2 = ((num2 < 0.25f) ? 0f : ((num2 > 0.75f) ? 1f : ((!(num2 < 0.5f)) ? (2f * (num2 - 0.5f) + 0.5f) : (num2 * 2f - 0.5f))));
		int index = pods.Count - 1;
		Vector3 accuratePositionOnSpline7 = pods[index].GetAccuratePositionOnSpline(1f);
		float x3 = accuratePositionOnSpline7.x;
		Vector3 accuratePositionOnSpline8 = pods[0].GetAccuratePositionOnSpline(0f);
		float x4 = Mathf.Lerp(x3, accuratePositionOnSpline8.x, num2);
		Vector3 accuratePositionOnSpline9 = pods[index].GetAccuratePositionOnSpline(1f);
		float y3 = accuratePositionOnSpline9.y;
		Vector3 accuratePositionOnSpline10 = pods[0].GetAccuratePositionOnSpline(0f);
		float y4 = Mathf.Lerp(y3, accuratePositionOnSpline10.y, num2);
		Vector3 accuratePositionOnSpline11 = pods[index].GetAccuratePositionOnSpline(1f);
		float z2 = accuratePositionOnSpline11.z;
		Vector3 accuratePositionOnSpline12 = pods[0].GetAccuratePositionOnSpline(0f);
		return new Vector3(x4, y4, Mathf.Lerp(z2, accuratePositionOnSpline12.z, num2));
	}

	private float GetRotationOnHenWay(float distance)
	{
		if (distance < 0f || distance > henWayLength)
		{
			return 0f;
		}
		for (int i = 0; i < pods.Count; i++)
		{
			if (distance < podStarts[i])
			{
				float t = (distance - podEnds[i - 1]) / (podStarts[i] - podEnds[i - 1]);
				return Mathf.LerpAngle(pods[i - 1].GetAccurateRotationOnSpline(1f), pods[i].GetAccurateRotationOnSpline(0f), t);
			}
			if (distance <= podEnds[i])
			{
				float t2 = (distance - podStarts[i]) / (podEnds[i] - podStarts[i]);
				return pods[i].GetAccurateRotationOnSpline(t2);
			}
		}
		float t3 = (distance - podEnds[pods.Count - 1]) / (henWayLength - podEnds[pods.Count - 1]);
		int index = pods.Count - 1;
		return Mathf.LerpAngle(pods[index].GetAccurateRotationOnSpline(1f), pods[0].GetAccurateRotationOnSpline(0f), t3);
	}

	public override Transform GetHenHolder(Vector3 initialPosition)
	{
		float num = 0f;
		float distance = num;
		float magnitude = (initialPosition - GetPositionOnHenWay(distance)).magnitude;
		while (num <= henWayLength)
		{
			num += 0.1f;
			if ((initialPosition - GetPositionOnHenWay(num)).magnitude < magnitude)
			{
				magnitude = (initialPosition - GetPositionOnHenWay(distance)).magnitude;
				distance = num;
			}
		}
		currentHenholderPositionOnWay = distance;
		henHolder.transform.position = GetPositionOnHenWay(currentHenholderPositionOnWay);
		henHolder.transform.rotation = Quaternion.Euler(0f, 0f, GetRotationOnHenWay(currentHenholderPositionOnWay) + 90f);
		return henHolder;
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

	public override Transform GetCenterOfTheWorld()
	{
		return center;
	}

	private IEnumerator WormCoroutine()
	{
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
			PeaSeed peaSeed = badSeeds[UnityEngine.Random.Range(0, badSeeds.Count)] as PeaSeed;
			peaSeed.SetWormDirection(pods[peaSeed.podNumber].GetAccurateRotationOnSpline(peaSeed.positionInPea) + 180f);
			peaSeed.WormGetOut();
			yield return new WaitForSeconds(5f);
		}
	}

	public override FruitType GetFruitType()
	{
		return FruitType.Pea;
	}

	public override Transform GetSeedHolder()
	{
		return center;
	}
}
