using UnityEngine;
using UnityEngine.EventSystems;

public class ScalingButton : EventTrigger
{
	private Vector3 startScale;

	private bool pointerDown;

	private float downTimer;

	private void Awake()
	{
		startScale = base.transform.localScale;
	}

	private void Update()
	{
		if (pointerDown)
		{
			if (downTimer < 0.1f)
			{
				downTimer += Time.deltaTime;
			}
			else
			{
				downTimer = 0.1f;
			}
		}
		else if (downTimer > 0f)
		{
			downTimer -= Time.deltaTime;
		}
		else
		{
			downTimer = 0f;
		}
		base.transform.localScale = startScale * (1f - downTimer);
	}

	public override void OnPointerDown(PointerEventData data)
	{
		pointerDown = true;
	}

	public override void OnPointerExit(PointerEventData data)
	{
		pointerDown = false;
	}

	public override void OnPointerUp(PointerEventData data)
	{
		pointerDown = false;
	}
}
