using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;

public class UpdateScreen : MonoBehaviour
{
	[SerializeField]
	private List<SkeletonGraphic> signs;

	private void OnEnable()
	{
		foreach (SkeletonGraphic sign in signs)
		{
			sign.Initialize(overwrite: false);
			sign.AnimationState.SetAnimation(0, "appear", loop: false);
			sign.AnimationState.AddAnimation(0, "idle", loop: true, 0f);
		}
	}
}
