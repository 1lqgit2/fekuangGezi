using System;
using System.Collections.Generic;
using UnityEngine;

public class SectorSeed : Seed
{
	private float damageDistance;

	private bool wormOutside;

	private int randomSeed;

	[SerializeField]
	private Transform center;

	[SerializeField]
	private Transform damagePoint;

	[SerializeField]
	private SpriteRenderer flat;

	[SerializeField]
	private List<Color> goodColors;

	[SerializeField]
	private List<Color> badColors;

	public void InitSector(float innerRadius, float outerRadius, float leftAngle, float rightAngle, int randomSeedValue, bool wormOutsideValue)
	{
		int num = 128;
		float num2 = 1f;
		float num3 = (innerRadius + outerRadius) / 2f;
		float num4 = (leftAngle + rightAngle) / 2f;
		Vector3 vector = new Vector3(num3 * Mathf.Cos(num4 * ((float)Math.PI / 180f)), num3 * Mathf.Sin(num4 * ((float)Math.PI / 180f)), 0f);
		float num5 = Mathf.Lerp(innerRadius, outerRadius, 0.3f);
		damagePoint.transform.localPosition = new Vector3(num5 * Mathf.Cos(num4 * ((float)Math.PI / 180f)), num5 * Mathf.Sin(num4 * ((float)Math.PI / 180f)), 0f);
		damageDistance = (damagePoint.transform.localPosition - new Vector3(num5 * Mathf.Cos(leftAngle * ((float)Math.PI / 180f)), num5 * Mathf.Sin(leftAngle * ((float)Math.PI / 180f)), 0f)).magnitude;
		Texture2D texture2D = new Texture2D(num, num);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				Vector3 vector2 = vector + new Vector3(i - num / 2, j - num / 2, 0f) / ((float)num / num2);
				float magnitude = vector2.magnitude;
				float num6 = Mathf.Atan2(vector2.y, vector2.x) * 57.29578f;
				if (num6 < 0f)
				{
					num6 += 360f;
				}
				if (innerRadius - magnitude > 0.01f || magnitude - outerRadius > 0.01f || leftAngle - num6 > 0.1f || num6 - rightAngle > 0.1f)
				{
					texture2D.SetPixel(i, j, new Color(1f, 1f, 1f, 0f));
				}
				else
				{
					texture2D.SetPixel(i, j, Color.white);
				}
			}
		}
		texture2D.Apply();
		flat.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, num, num), new Vector2(0.5f, 0.5f), (float)num / num2);
		flat.transform.localPosition = vector;
		randomSeed = randomSeedValue;
		wormOutside = wormOutsideValue;
		center.localPosition = vector;
		center.localRotation = Quaternion.Euler(0f, 0f, num4 - 90f);
		worm.transform.localPosition = new Vector3(0f, (outerRadius - innerRadius) / 2f, 0f);
		detonator.transform.localScale = Vector3.one * damageDistance / 0.4f;
		UpdateVisual();
	}

	public override void UpdateVisual()
	{
		detonator.gameObject.SetActive(value: false);
		worm.gameObject.SetActive(value: false);
		fire.gameObject.SetActive(value: false);
		if ((bool)UFO)
		{
			UFO.gameObject.SetActive(value: false);
		}
		freeze.gameObject.SetActive(value: false);
		color.gameObject.SetActive(value: false);
		if (seedType == SeedType.Good)
		{
			if (seedState == SeedState.Default)
			{
				flat.color = goodColors[randomSeed];
			}
			else if (seedState == SeedState.Eaten)
			{
				flat.color = Color.clear;
			}
		}
		else if (seedType == SeedType.Bad)
		{
			if (seedState == SeedState.Default)
			{
				flat.color = badColors[randomSeed];
			}
			else if (seedState == SeedState.Eaten)
			{
				flat.color = Color.clear;
			}
		}
		else if (seedType == SeedType.Bomb)
		{
			flat.color = Color.clear;
			if (seedState == SeedState.Default)
			{
				detonator.gameObject.SetActive(value: true);
			}
		}
		else if (seedType == SeedType.Freeze)
		{
			flat.color = Color.clear;
			if (seedState == SeedState.Default)
			{
				freeze.gameObject.SetActive(value: true);
			}
		}
		else if (seedType == SeedType.Fire)
		{
			flat.color = Color.clear;
			if (seedState == SeedState.Default)
			{
				fire.gameObject.SetActive(value: true);
			}
		}
		else if (seedType == SeedType.Color)
		{
			flat.color = Color.clear;
			if (seedState == SeedState.Default)
			{
				color.gameObject.SetActive(value: true);
			}
		}
		else if (seedType == SeedType.UFO)
		{
			flat.color = Color.clear;
			if (seedState == SeedState.Default)
			{
				UFO.gameObject.SetActive(value: true);
			}
		}
	}

	public override GameObject TakeSeed()
	{
		seedState = SeedState.Eaten;
		GameObject gameObject;
		if (seedType == SeedType.Bad)
		{
			gameObject = UnityEngine.Object.Instantiate(flat.gameObject, flat.transform.position, flat.transform.rotation);
			gameObject.transform.localScale = flat.transform.lossyScale;
		}
		else if (seedType == SeedType.Bomb)
		{
			gameObject = UnityEngine.Object.Instantiate(detonator.gameObject, detonator.transform.position, detonator.transform.rotation);
			gameObject.transform.localScale = detonator.transform.lossyScale;
		}
		else
		{
			gameObject = UnityEngine.Object.Instantiate(flat.gameObject, flat.transform.position, flat.transform.rotation);
			gameObject.transform.localScale = flat.transform.lossyScale;
		}
		UpdateVisual();
		return gameObject;
	}

	public override float HowIsPointIn(Vector3 point)
	{
		return (damagePoint.position - point).magnitude / damageDistance;
	}
}
