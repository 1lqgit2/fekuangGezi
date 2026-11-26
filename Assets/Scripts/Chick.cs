using Spine;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;

public class Chick : MonoBehaviour
{
	[SerializeField]
	private SkeletonAnimation body;

	[SerializeField]
	private Transform dummy;

	[SerializeField]
	private Transform leftDummy;

	[SerializeField]
	private Transform rightDummy;

	[SerializeField]
	private Transform bottomDummy;

	[SerializeField]
	private SpriteRenderer platformSprite;

	[SerializeField]
	private BoxCollider2D platformCollider;

	[SerializeField]
	private List<AudioSource> eatSounds;

	private bool isActive;

	private Bone targetBone;

	private List<Transform> targets = new List<Transform>();

	private float size;

	private string skinName;

	public void Init(float sizeValue)
	{
		size = sizeValue;
	}

	private void Start()
	{
		isActive = true;
		body.Initialize(overwrite: false);
		body.AnimationState.SetAnimation(0, "small_idle", loop: true);
		skinName = BaseGameManager<SkinManager>.GetInstance().GetAnimationName(BaseGameManager<SkinManager>.GetInstance().GetRandomUnlockedSkin());
		body.Skeleton.SetSkin(skinName);
		targetBone = body.Skeleton.FindBone("IK");
		SpriteRenderer spriteRenderer = platformSprite;
		float x = size;
		Vector2 vector = platformSprite.size;
		spriteRenderer.size = new Vector2(x, vector.y);
		BoxCollider2D boxCollider2D = platformCollider;
		float x2 = size;
		Vector2 vector2 = platformSprite.size;
		boxCollider2D.size = new Vector2(x2, vector2.y);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (isActive && other.tag == "Bullet")
		{
			BaseSceneManager<Feeding>.Instance.ChickHit(this, other.attachedRigidbody);
			isActive = false;
			body.AnimationState.SetAnimation(0, "small_eat", loop: false);
			body.AnimationState.AddAnimation(0, "small_eat_idle", loop: true, 0f);
			platformSprite.color = Color.white;
			eatSounds[Random.Range(0, eatSounds.Count)].Play();
		}
	}

	public void AddTarget(Transform target)
	{
		if (!targets.Contains(target))
		{
			targets.Add(target);
		}
	}

	public void RemoveTarget(Transform target)
	{
		if (targets.Contains(target))
		{
			targets.Remove(target);
		}
	}

	private void Update()
	{
		if (!isActive)
		{
			return;
		}
		if (targets.Count > 0)
		{
			float num = float.MaxValue;
			Transform transform = null;
			foreach (Transform target in targets)
			{
				if ((bool)target)
				{
					float magnitude = (target.position - body.transform.position).magnitude;
					if (magnitude < num)
					{
						transform = target;
						num = magnitude;
					}
				}
			}
			if ((bool)transform)
			{
				if (body.AnimationName == "small_idle")
				{
					body.AnimationState.SetAnimation(0, "small_run", loop: true);
				}
				leftDummy.localPosition = body.transform.localPosition + 2f * Vector3.left * Time.deltaTime;
				Vector3 localPosition = leftDummy.localPosition;
				if (localPosition.x < (0f - size) / 2f)
				{
					leftDummy.localPosition = Vector3.left * size / 2f;
				}
				float magnitude2 = (leftDummy.position - transform.position).magnitude;
				if (magnitude2 < num)
				{
					body.transform.position = leftDummy.position;
				}
				else
				{
					rightDummy.localPosition = body.transform.localPosition + 2f * Vector3.right * Time.deltaTime;
					Vector3 localPosition2 = rightDummy.localPosition;
					if (localPosition2.x > size / 2f)
					{
						rightDummy.localPosition = Vector3.right * size / 2f;
					}
					magnitude2 = (rightDummy.position - transform.position).magnitude;
					if (magnitude2 < num)
					{
						body.transform.position = rightDummy.position;
					}
				}
				dummy.position = transform.position;
				targetBone.SetPositionSkeletonSpace(dummy.localPosition);
			}
			else
			{
				targets.Clear();
			}
		}
		else if (body.AnimationName == "small_run")
		{
			body.AnimationState.SetAnimation(0, "small_idle", loop: true);
			dummy.localPosition = Vector3.up * 2f;
			targetBone.SetPositionSkeletonSpace(dummy.localPosition);
		}
	}

	public Vector3 GetBottom()
	{
		return bottomDummy.position;
	}

	public bool IsActive()
	{
		return isActive;
	}

	public string GetSkinName()
	{
		return skinName;
	}
}
