using System;
using System.Collections;
using UnityEngine;

public class CameraBehavior : BaseSceneManager<CameraBehavior>
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private new Camera camera;

	[SerializeField]
	private GameObject fog;

	private void Start()
	{
		if ((float)Screen.height / (float)Screen.width > 2f)
		{
			camera.orthographicSize = 6.0869565f;
		}
	}

	public void Shake()
	{
		animator.Play("Base Layer.CameraShake", 0, 0f);
	}

	public void Up()
	{
		animator.Play("Base Layer.Up", 0, 0f);
	}

	public void Down()
	{
		animator.Play("Base Layer.Down", 0, 0f);
	}

	public void Push(float angle)
	{
		StartCoroutine(PushCoroutine(angle));
	}

	private IEnumerator PushCoroutine(float angle)
	{
		animator.enabled = false;
		Vector3 startPosition = camera.transform.position;
		float duration = 0.2f;
		float timer = 0f;
		while (timer < duration)
		{
			timer += Time.deltaTime;
			if (timer > duration)
			{
				timer = duration;
			}
			camera.transform.position = startPosition + new Vector3(Mathf.Cos((float)Math.PI / 180f * angle), Mathf.Sin((float)Math.PI / 180f * angle), 0f) * 0.5f * Mathf.Sin((float)Math.PI * 2f * timer / 0.1f) * (1f - timer / duration);
			yield return null;
		}
		animator.enabled = true;
	}

	public void SetFogEnabled(bool isOn)
	{
		fog.SetActive(isOn);
	}
}
