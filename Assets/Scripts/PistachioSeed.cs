using Spine.Unity;
using System.Collections;
using UnityEngine;

public class PistachioSeed : Seed
{
	[SerializeField]
	protected SkeletonAnimation body;

	[SerializeField]
	private AudioSource pistachioOpenSound;

	private bool isOpened;

	public IEnumerator OpenCloseCoroutine(bool openedOnStart, float smallestPeriod, float largestPeriod)
	{
		isOpened = openedOnStart;
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
		while (seedState == SeedState.Default)
		{
			if (isOpened)
			{
				body.AnimationState.SetAnimation(0, "open_idle", loop: true);
			}
			else
			{
				body.AnimationState.SetAnimation(0, "close_idle", loop: true);
			}
			if (seedType != SeedType.Bad)
			{
				yield return new WaitForSeconds(Random.Range(smallestPeriod, largestPeriod));
			}
			else
			{
				yield return new WaitForSeconds(4.66f);
			}
			if (seedState != 0)
			{
				break;
			}
			pistachioOpenSound.Play();
			if (isOpened)
			{
				yield return new WaitForSpineAnimationComplete(body.AnimationState.SetAnimation(0, "close", loop: false));
			}
			else
			{
				yield return new WaitForSpineAnimationComplete(body.AnimationState.SetAnimation(0, "open", loop: false));
			}
			isOpened = !isOpened;
		}
	}

	public override bool IsOpened()
	{
		return isOpened;
	}

	public override void UpdateVisual()
	{
		worm.gameObject.SetActive(value: false);
		if (seedState == SeedState.Eaten && !body.AnimationName.Contains("disappear"))
		{
			body.AnimationState.SetAnimation(0, "open_empty", loop: false);
		}
	}

	public override void Explode()
	{
		if (!body.AnimationName.Contains("disappear"))
		{
			seedState = SeedState.Eaten;
			body.AnimationState.SetAnimation(0, "disappear" + Random.Range(1, 6).ToString(), loop: false);
			StopWorm();
		}
	}

	public override GameObject TakeSeed()
	{
		StopWorm();
		return body.gameObject;
	}

	public override float GetRadius()
	{
		return 0.5f;
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
