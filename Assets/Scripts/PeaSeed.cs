using UnityEngine;

public class PeaSeed : Seed
{
	public int movementDirection;

	public int podNumber;

	public float positionInPea;

	public float predictedPositionInPea;

	public Vector3 predictedPosition;

	private float wormAngle;

	public override float GetRadius()
	{
		return 0.25f;
	}

	public void SetWormDirection(float angle)
	{
		wormAngle = angle;
	}

	public override void WormGetOut()
	{
		worm.transform.parent.rotation = Quaternion.Euler(0f, 0f, wormAngle);
		wormCoroutine = StartCoroutine(WormCoroutine());
	}

	public override float HowIsPointIn(Vector3 point)
	{
		float num = 0.9f * (base.transform.position - point).magnitude;
		Vector3 lossyScale = base.transform.lossyScale;
		return num / (lossyScale.x * GetRadius());
	}
}
