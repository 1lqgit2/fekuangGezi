using System.Collections;
using UnityEngine;

public class DisappearInSomeTime : MonoBehaviour
{
	[SerializeField]
	private float disappearTime;

	private void Start()
	{
		StartCoroutine(DisappearInSmallTime());
	}

	private IEnumerator DisappearInSmallTime()
	{
		yield return new WaitForSeconds(disappearTime);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
