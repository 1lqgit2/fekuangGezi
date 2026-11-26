using UnityEngine;

public class ChickHelper : MonoBehaviour
{
	[SerializeField]
	private Chick chick;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Bullet")
		{
			chick.AddTarget(other.transform);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.tag == "Bullet")
		{
			chick.RemoveTarget(other.transform);
		}
	}
}
