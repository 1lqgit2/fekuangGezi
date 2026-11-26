using System;
using UnityEngine;

public class Rotator : Corn
{
	private float currentRotationSpeed;

	[SerializeField]
	private float topSpeed = 20f;

	[SerializeField]
	private float timeToStop = float.MaxValue;

	[SerializeField]
	private float rotationSpeedStep;

	[SerializeField]
	private float layerDepth;

	protected override void SpecialSeedSetup(Seed seed, int n)
	{
		((SectorSeed)seed).InitSector(radius - 0.3f * layerDepth, radius + 0.7f * layerDepth, angles[n] - angleSizes[n] / 2f, angles[n] + angleSizes[n] / 2f, n % 3, wormOutsideValue: true);
	}

	protected override void UpdateMovement()
	{
		if (currentRotationSpeed > Mathf.Lerp(speed, topSpeed, ((float)goodSeedsOnStart - (float)goodSeedCounter) / (float)goodSeedsOnStart))
		{
			currentRotationSpeed -= Time.deltaTime * (rotationSpeedStep / timeToStop);
		}
		seedHolder.Rotate(0f, 0f, currentRotationSpeed * Time.deltaTime);
		henHolder.parent.Rotate(0f, 0f, henSpeed * Time.deltaTime);
		if (insect != null)
		{
			Vector3 eulerAngles = insect.transform.eulerAngles;
			float z = eulerAngles.z;
			z += (0f - Time.deltaTime) * henSpeed;
			insect.transform.position = base.transform.position + new Vector3(radius * Mathf.Cos(z * ((float)Math.PI / 180f)), radius * Mathf.Sin(z * ((float)Math.PI / 180f)), 0f);
			insect.transform.rotation = Quaternion.Euler(0f, 0f, z);
		}
	}

	public override FruitType GetFruitType()
	{
		return FruitType.Rotator;
	}

	public void Pecked()
	{
		currentRotationSpeed += rotationSpeedStep;
	}
}
