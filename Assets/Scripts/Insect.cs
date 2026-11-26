using UnityEngine;

public class Insect : MonoBehaviour
{
	[SerializeField]
	private GameObject spineAnimation;

	[SerializeField]
	private ParticleSystem pshik;

	[SerializeField]
	private AudioSource chvak;

	[SerializeField]
	private AudioSource buzz;

	[SerializeField]
	private bool isGood;

	[SerializeField]
	private GameObject tutorArrow;

	private bool eaten;

	private void Start()
	{
		buzz.Play();
		if (BaseSceneManager<Gameplay>.Instance.GetLevelNumber() == 3)
		{
			tutorArrow.SetActive(value: true);
		}
	}

	private void Update()
	{
		spineAnimation.transform.rotation = Quaternion.identity;
	}

	public float HowIsPointIn(Vector3 point)
	{
		float magnitude = (base.transform.position - point).magnitude;
		Vector3 lossyScale = base.transform.lossyScale;
		float num = magnitude / (lossyScale.x * 0.2f);
		return (!eaten) ? num : float.PositiveInfinity;
	}

	public void WasEaten()
	{
		if (!eaten)
		{
			eaten = true;
			pshik.Play();
			chvak.Play();
			buzz.Stop();
			spineAnimation.SetActive(value: false);
			if ((bool)tutorArrow && tutorArrow.activeInHierarchy)
			{
				tutorArrow.SetActive(value: false);
			}
		}
	}

	public bool IsGood()
	{
		return isGood;
	}
}
