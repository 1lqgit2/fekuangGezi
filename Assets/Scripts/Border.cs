using UnityEngine;

public class Border : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Bullet")
		{
			BaseSceneManager<Feeding>.Instance.BorderHit(other.GetComponent<Rigidbody2D>());
		}
	}
}
