using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Almond : Corn
{
	protected override void SpecialSeedSetup(Seed seed, int n)
	{
		float radius = base.radius;
		float f = angles[n] * ((float)Math.PI / 180f);
		float num = angleSizes[n] / (360f / (float)seedsAmount) * base.radius * (5.5f / (float)seedsAmount);
		seed.transform.localPosition = new Vector3(radius * Mathf.Sin(f), radius * Mathf.Cos(f), 0f);
		seed.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f - angles[n]));
		seed.transform.localScale = new Vector3(num, num, 1f);
	}

	public override FruitType GetFruitType()
	{
		return FruitType.Almond;
	}

	protected override IEnumerator CleanCoroutine()
	{
		if (wormCoroutine != null)
		{
			StopCoroutine(wormCoroutine);
		}
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
			shkurki.Add(seed.TakeSeed());
			shkurkiSpeed.Add(UnityEngine.Random.Range(3f, 9f));
			shkurkiAngleSpeed.Add(UnityEngine.Random.Range(-20f, 20f));
		}
		while (timer < duration)
		{
			timer += Time.deltaTime;
			if (timer > duration)
			{
				timer = duration;
			}
			for (int i = 0; i < shkurki.Count; i++)
			{
				shkurki[i].transform.position += (shkurki[i].transform.position - GetCenterOfTheWorld().position).normalized * shkurkiSpeed[i] * Time.deltaTime;
				shkurki[i].transform.Rotate(new Vector3(0f, 0f, shkurkiAngleSpeed[i] * Time.deltaTime));
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

	public override void BombExplode(Seed bomb)
	{
		((AlmondSeed)bomb).Explode();
		int num = seeds.IndexOf(bomb);
		int num2 = 0;
		int num3 = num;
		for (int i = 0; i < 2; i++)
		{
			num3++;
			if (num3 > seeds.Count - 1)
			{
				num3 = 0;
			}
			if (seeds[num3].seedType == Seed.SeedType.Good && (seeds[num3].seedState == Seed.SeedState.Default || seeds[num3].seedState == Seed.SeedState.Hull))
			{
				GoodSeedEaten();
			}
			else if (seeds[num3].seedType == Seed.SeedType.Bad && seeds[num3].seedState == Seed.SeedState.Worm)
			{
				num2++;
				WormEaten(seeds[num3].wormSize);
			}
			((AlmondSeed)seeds[num3]).Explode();
		}
		num3 = num;
		for (int j = 0; j < 2; j++)
		{
			num3--;
			if (num3 < 0)
			{
				num3 = seeds.Count - 1;
			}
			if (seeds[num3].seedType == Seed.SeedType.Good && (seeds[num3].seedState == Seed.SeedState.Default || seeds[num3].seedState == Seed.SeedState.Hull))
			{
				GoodSeedEaten();
			}
			else if (seeds[num3].seedType == Seed.SeedType.Bad && seeds[num3].seedState == Seed.SeedState.Worm)
			{
				num2++;
				WormEaten(seeds[num3].wormSize);
			}
			((AlmondSeed)seeds[num3]).Explode();
		}
	}
}
