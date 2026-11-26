using System;
using UnityEngine;

public class MovingCorn : Corn
{
	[SerializeField]
	private float verticalAmplitude;

	[SerializeField]
	private float verticalPeriod;

	[SerializeField]
	private float horizontalAmplitude;

	[SerializeField]
	private float horizontalPeriod;

	private float timeSinceStart;

	private Vector3 positionUpdated = Vector3.zero;

	protected override void UpdateMovement()
	{
		seedHolder.Rotate(0f, 0f, speed * Time.deltaTime);
		henHolder.parent.Rotate(0f, 0f, henSpeed * Time.deltaTime);
		timeSinceStart += Time.deltaTime;
		positionUpdated.x = Mathf.Sin((float)Math.PI * 2f * timeSinceStart / horizontalPeriod) * horizontalAmplitude;
		positionUpdated.y = Mathf.Sin((float)Math.PI * 2f * timeSinceStart / verticalPeriod) * verticalAmplitude;
		seedHolder.transform.localPosition = positionUpdated + shift;
		if (insect != null)
		{
			Vector3 eulerAngles = insect.transform.eulerAngles;
			float z = eulerAngles.z;
			z += Time.deltaTime * speed * 2.5f;
			insect.transform.position = seedHolder.position + new Vector3(radius * Mathf.Cos(z * ((float)Math.PI / 180f)), radius * Mathf.Sin(z * ((float)Math.PI / 180f)), 0f);
			insect.transform.rotation = Quaternion.Euler(0f, 0f, z);
		}
		henHolder.parent.localPosition = positionUpdated;
	}

	public override FruitType GetFruitType()
	{
		return FruitType.Raspberry;
	}

	public override Transform GetSeedHolder()
	{
		return seedHolder;
	}

	public override Transform GetCenterOfTheWorld()
	{
		return seedHolder;
	}
}
