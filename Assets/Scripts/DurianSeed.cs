using Spine.Unity;
using System;
using UnityEngine;

public class DurianSeed : Seed
{
	[SerializeField]
	protected SkeletonAnimation body;

	private float seedRadius;

	public void InitForm(int seedsAmount, float radius, int randomSeedValue)
	{
		body.Initialize(overwrite: false);
		switch (seedType)
		{
		case SeedType.Bomb:
			body.Skeleton.SetSkin("tnt");
			break;
		case SeedType.Bad:
			body.Skeleton.SetSkin("bad");
			break;
		case SeedType.Color:
			body.Skeleton.SetSkin("color");
			break;
		case SeedType.Fire:
			body.Skeleton.SetSkin("aim");
			break;
		case SeedType.Freeze:
			body.Skeleton.SetSkin("ice");
			break;
		case SeedType.UFO:
			body.Skeleton.SetSkin("space");
			break;
		default:
			body.skeleton.SetSkin("normal" + ((randomSeedValue != -1) ? (randomSeedValue + 1).ToString() : string.Empty));
			break;
		}
		switch (seedsAmount)
		{
		case 15:
			body.AnimationState.SetAnimation(0, "24", loop: true);
			break;
		case 18:
			body.AnimationState.SetAnimation(0, "20", loop: true);
			break;
		case 20:
			body.AnimationState.SetAnimation(0, "18", loop: true);
			break;
		default:
			body.AnimationState.SetAnimation(0, "15", loop: true);
			break;
		}
		seedRadius = radius * (180f / (float)seedsAmount) * ((float)Math.PI / 180f);
	}

	public override void UpdateVisual()
	{
		worm.gameObject.SetActive(value: false);
		if (seedState == SeedState.Eaten)
		{
			body.Skeleton.SetSkin("empty");
		}
	}

	public override GameObject TakeSeed()
	{
		SkeletonAnimation skeletonAnimation = UnityEngine.Object.Instantiate(body, body.transform.position, body.transform.rotation);
		skeletonAnimation.transform.localScale = body.transform.lossyScale;
		skeletonAnimation.Initialize(overwrite: false);
		skeletonAnimation.AnimationState.SetAnimation(0, body.AnimationName, loop: true);
		skeletonAnimation.Skeleton.SetSkin("bad");
		seedState = SeedState.Eaten;
		StopWorm();
		UpdateVisual();
		return skeletonAnimation.gameObject;
	}

	public override float HowIsPointIn(Vector3 point)
	{
		return (base.transform.position - point).magnitude / GetRadius();
	}

	public override float GetRadius()
	{
		return seedRadius;
	}

	public void StopWorm()
	{
		worm.gameObject.SetActive(value: false);
		if (wormCoroutine != null)
		{
			StopCoroutine(wormCoroutine);
		}
	}
}
