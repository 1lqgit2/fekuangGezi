using Spine;
using Spine.Unity;
using UnityEngine;

public class StepSpawner : MonoBehaviour
{
	[SerializeField]
	private SkeletonAnimation henSpine;

	[SerializeField]
	private GameObject stepPrefab;

	[SerializeField]
	private AudioSource stepSound;

	[SerializeField]
	private string stepEvent;

	private void Start()
	{
		henSpine.AnimationState.Event += HandleEvent;
	}

	private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == stepEvent && !BaseSceneManager<Gameplay>.Instance.IsPaused())
		{
			GameObject original = stepPrefab;
			Vector3 position = base.transform.position;
			Vector3 eulerAngles = base.transform.rotation.eulerAngles;
			Object.Instantiate(original, position, Quaternion.Euler(0f, 0f, eulerAngles.z + UnityEngine.Random.Range(-10f, 10f)));
			stepSound.Play();
		}
	}
}
