using System;
using System.Collections;
using UnityEngine;

public class Apple : Corn
{
	[SerializeField]
	private float period;

	protected override void SpecialFruitSetup()
	{
		StartCoroutine(ChangeSpeedCoroutine());
	}

	protected override void SpecialSeedSetup(Seed seed, int n)
	{
		float radius = base.radius;
		float f = angles[n] * ((float)Math.PI / 180f);
		seed.transform.localPosition = new Vector3(radius * Mathf.Sin(f), radius * Mathf.Cos(f), 0f);
		seed.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f - angles[n]));
		((DurianSeed)seed).InitForm(seedsAmount, base.radius, n % 3);
	}

	private IEnumerator ChangeSpeedCoroutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(period);
			float duration = period / 6f;
			float timer = 0f;
			while (timer < duration)
			{
				timer += Time.deltaTime;
				if (timer > duration)
				{
					timer = duration;
				}
				speedMultiplicator = 1f - 2f * (timer / duration);
				yield return null;
			}
			yield return new WaitForSeconds(period);
			while (timer > 0f)
			{
				timer -= Time.deltaTime;
				if (timer < 0f)
				{
					timer = 0f;
				}
				speedMultiplicator = 1f - 2f * (timer / duration);
				yield return null;
			}
		}
	}

	public override FruitType GetFruitType()
	{
		return FruitType.Apple;
	}

	protected override float GetSizeRandomizationRange()
	{
		return 0f;
	}
}
