using Spine.Unity;
using UnityEngine;

public class AlmondSeed : Seed
{
	[SerializeField]
	private SkeletonAnimation body;

	[SerializeField]
	private AudioSource explodeSound;

	private void Awake()
	{
		seedState = SeedState.Hull;
	}

	public override void UpdateVisual()
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
			body.skeleton.SetSkin("normal");
			break;
		}
		worm.gameObject.SetActive(value: false);
		if (seedState == SeedState.Hull)
		{
			body.AnimationState.SetAnimation(0, "closed", loop: true);
		}
		else if (seedState == SeedState.Default)
		{
			if (body.AnimationName != "open" && body.AnimationName != "explode")
			{
				body.AnimationState.SetAnimation(0, "explode", loop: false);
				body.AnimationState.AddAnimation(0, "open", loop: true, 0f);
			}
		}
		else if (seedState == SeedState.Eaten)
		{
			body.AnimationState.SetAnimation(0, "empty", loop: true);
		}
	}

	public override GameObject TakeSeed()
	{
		worm.gameObject.SetActive(value: false);
		if (wormCoroutine != null)
		{
			StopCoroutine(wormCoroutine);
		}
		return body.gameObject;
	}

	public override void KickHull()
	{
		explodeSound.Play();
		seedState = SeedState.Default;
		UpdateVisual();
	}

	public override float GetRadius()
	{
		return 0.5f;
	}

	public new void Explode()
	{
		seedState = SeedState.Eaten;
		worm.gameObject.SetActive(value: false);
		if (wormCoroutine != null)
		{
			StopCoroutine(wormCoroutine);
		}
		UpdateVisual();
	}
}
