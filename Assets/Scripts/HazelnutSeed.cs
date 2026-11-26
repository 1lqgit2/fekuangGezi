using UnityEngine;

public class HazelnutSeed : PistachioSeed
{
	public override GameObject TakeSeed()
	{
		seedState = SeedState.Eaten;
		StopWorm();
		if ((bool)body)
		{
			return body.gameObject;
		}
		return null;
	}

	public override float GetRadius()
	{
		return 0.3f;
	}
}
