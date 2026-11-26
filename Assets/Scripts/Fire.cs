using Spine.Unity;
using UnityEngine;

public class Fire : MonoBehaviour
{
	private void OnEnable()
	{
		SkeletonAnimation[] componentsInChildren = GetComponentsInChildren<SkeletonAnimation>();
		foreach (SkeletonAnimation skeletonAnimation in componentsInChildren)
		{
			skeletonAnimation.Initialize(overwrite: false);
			skeletonAnimation.AnimationState.SetAnimation(0, "animation", loop: true).TrackTime = UnityEngine.Random.Range(0f, 1f);
			skeletonAnimation.Update(0f);
		}
	}
}
