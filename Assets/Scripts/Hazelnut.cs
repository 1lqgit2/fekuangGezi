using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazelnut : Garnet
{
	protected override void SpecialSeedSetup(Seed seed)
	{
		bool openedOnStart = Random.Range(0, 2) == 0;
		float smallestPeriod = 360f / speed * 0.2f;
		float largestPeriod = 360f / speed * 0.5f;
		StartCoroutine(((PistachioSeed)seed).OpenCloseCoroutine(openedOnStart, smallestPeriod, largestPeriod));
	}

	protected override IEnumerator DestroyLayerCoroutine(int layer)
	{
		List<GameObject> oldLayer = new List<GameObject>();
		List<float> oldLayerSpeeds = new List<float>();
		List<float> oldLayerAngleSpeeds = new List<float>();
		float duration = 0.5f;
		float timer = 0f;
		int summ = 0;
		for (int i = 0; i < layer; i++)
		{
			summ += seedsOnRadiuses[i];
		}
		for (int j = summ; j < summ + seedsOnRadiuses[layer]; j++)
		{
			GameObject exists = seeds[j].TakeSeed();
			if ((bool)exists)
			{
				oldLayer.Add(seeds[j].TakeSeed());
				oldLayerSpeeds.Add(Random.Range(5f, 7f));
				oldLayerAngleSpeeds.Add(Random.Range(30f, 50f) * Mathf.Sign(Random.Range(-10f, 10f)));
			}
		}
		while (timer < duration)
		{
			timer += Time.deltaTime;
			if (timer > duration)
			{
				timer = duration;
			}
			for (int k = 0; k < oldLayer.Count; k++)
			{
				oldLayer[k].transform.position += oldLayerSpeeds[k] * Time.deltaTime * (oldLayer[k].transform.position - GetCenterOfTheWorld().position).normalized;
				oldLayer[k].transform.Rotate(new Vector3(0f, 0f, oldLayerAngleSpeeds[k] * Time.deltaTime));
			}
			yield return null;
		}
		foreach (GameObject item in oldLayer)
		{
			UnityEngine.Object.Destroy(item);
		}
	}

	public override FruitType GetFruitType()
	{
		return FruitType.Hazelnut;
	}

	protected override IEnumerator ExplodeCoroutine(List<Seed> neighbours)
	{
		foreach (Seed neighbour in neighbours)
		{
			neighbour.Explode();
		}
		yield return null;
	}
}
