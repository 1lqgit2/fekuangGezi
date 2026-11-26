using Spine;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Durian : Corn
{
	[SerializeField]
	private SkeletonAnimation bomb;

	[SerializeField]
	private SkeletonAnimation counter;

	[SerializeField]
	private SkeletonAnimation explosion;

	[SerializeField]
	private AudioSource explosionBoom;

	[SerializeField]
	private int detonatorsOnStart;

	[SerializeField]
	private GameObject detonatorReference;

	[SerializeField]
	private Transform detonatorHolder;

	[SerializeField]
	private Sprite detonatorClicked;

	[SerializeField]
	private AudioSource detonatorClick;

	[SerializeField]
	private AudioSource timerSound;

	[SerializeField]
	private GameObject hull;

	[SerializeField]
	private GameObject backHull;

	private TrackEntry counterTrack;

	private bool hullPresent;

	private List<GameObject> detonators = new List<GameObject>();

	protected override void SpecialFruitSetup()
	{
		hullPresent = true;
		counter.gameObject.SetActive(value: true);
		counter.Initialize(overwrite: false);
		counterTrack = counter.AnimationState.SetAnimation(0, "timer_ide", loop: true);
		bomb.Initialize(overwrite: false);
		while (detonators.Count < detonatorsOnStart)
		{
			float num = (float)UnityEngine.Random.Range(0, 24) * 15f;
			bool flag = true;
			foreach (GameObject detonator in detonators)
			{
				Vector3 eulerAngles = detonator.transform.localRotation.eulerAngles;
				if (Mathf.Abs(Mathf.DeltaAngle(eulerAngles.z, num)) < 1f)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(detonatorReference, detonatorHolder, worldPositionStays: true);
				gameObject.SetActive(value: true);
				gameObject.transform.Rotate(0f, 0f, num);
				detonators.Add(gameObject);
			}
		}
		UpdateBomb();
	}

	protected override void SpecialSeedSetup(Seed seed, int n)
	{
		float radius = base.radius;
		float f = angles[n] * ((float)Math.PI / 180f);
		seed.transform.localPosition = new Vector3(radius * Mathf.Sin(f), radius * Mathf.Cos(f), 0f);
		seed.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f - angles[n]));
		((DurianSeed)seed).InitForm(seedsAmount, base.radius, -1);
	}

	private void UpdateBomb()
	{
		if (hullPresent)
		{
			float num = 1f - (float)detonators.Count / (float)detonatorsOnStart;
			counterTrack.TrackTime = num;
			counterTrack.TimeScale = 0f;
			if (num < 0.25f)
			{
				bomb.AnimationState.SetAnimation(0, "bomb_idle1", loop: true);
			}
			else if (num < 0.5f)
			{
				bomb.AnimationState.SetAnimation(0, "bomb_idle2", loop: true);
			}
			else if (num < 0.75f)
			{
				bomb.AnimationState.SetAnimation(0, "bomb_idle3", loop: true);
			}
			else if (num < 1f)
			{
				bomb.AnimationState.SetAnimation(0, "bomb_idle4", loop: true);
			}
			else
			{
				henHolder.transform.localPosition = new Vector3(0f, 0f - radius, 0f);
				hullPresent = false;
				hull.SetActive(value: false);
				detonatorHolder.gameObject.SetActive(value: false);
				backHull.SetActive(value: true);
				counter.gameObject.SetActive(value: false);
				bomb.gameObject.SetActive(value: false);
				explosion.AnimationState.SetAnimation(0, "explode", loop: false);
				explosionBoom.Play();
				BaseSceneManager<CameraBehavior>.Instance.Shake();
			}
			timerSound.gameObject.SetActive(hullPresent && num > 0f);
			timerSound.pitch = 1f + num / 4f;
		}
	}

	public override bool HullPecked(Vector3 point)
	{
		if (!hullPresent)
		{
			return false;
		}
		Vector3 vector = point - seedHolder.position;
		float y = vector.y;
		Vector3 vector2 = point - seedHolder.position;
		float target = 57.29578f * Mathf.Atan2(y, vector2.x);
		foreach (GameObject detonator in detonators)
		{
			Vector3 eulerAngles = detonator.transform.rotation.eulerAngles;
			if (Mathf.Abs(Mathf.DeltaAngle(eulerAngles.z, target)) < 7.5f)
			{
				detonatorClick.Play();
				detonator.GetComponentInChildren<SpriteRenderer>().sprite = detonatorClicked;
				detonators.Remove(detonator);
				UpdateBomb();
				return false;
			}
		}
		return true;
	}

	public override bool HullPresent()
	{
		return hullPresent;
	}

	public override FruitType GetFruitType()
	{
		return FruitType.Durian;
	}

	protected override float GetSizeRandomizationRange()
	{
		return 0f;
	}

	public override Transform GetHenHolder(Vector3 initialPosition)
	{
		return henHolder;
	}
}
