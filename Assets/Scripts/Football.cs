using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Football : BaseSceneManager<Football>
{
	public enum FootballState
	{
		FS_NotStarted,
		FS_WaitingForKick,
		FS_WaitingForBalance,
		FS_Balance,
		FS_BallDropped
	}

	[Serializable]
	private struct FootballKick
	{
		public List<Transform> kickPoints;

		public string idleAnimationName;

		public List<string> kickAnimationNames;

		public int stake;
	}

	[Serializable]
	private struct FootballBalancer
	{
		public Transform center;

		public Transform top;

		public Transform left;

		public Transform right;

		public string idleAnimationName;

		public string playAnimationName;

		public int stake;
	}

	[SerializeField]
	private SkeletonAnimation ball;

	[SerializeField]
	private Material multiplyVersionOfMaterial;

	[SerializeField]
	private SkeletonAnimation bird;

	[SerializeField]
	private List<FootballKick> kicks;

	[SerializeField]
	private List<FootballBalancer> balances;

	[SerializeField]
	private FootballHud hud;

	private const float g = 30f;

	private Vector3 ballVelocity;

	private float ballAngularVelocity;

	private Coroutine throwCoroutine;

	private Coroutine catchCoroutine;

	private int currentKickNumber;

	private int currentKickLeftOrRight;

	private int currentBalanceNumber;

	private float balanceTimer;

	private TrackEntry balanceTrackEntry;

	private float currentBalanceBallPosition;

	private float currentBalanceBallVelocity;

	private float currentBalanceBirdPosition;

	private FootballState currentState;

	private Bone rotateBone;

	private int stakeScore;

	private int score;

	private void Start()
	{
		hud.gameObject.SetActive(value: true);
		SkeletonRenderer component = ball.GetComponent<SkeletonRenderer>();
		Dictionary<Slot, Material> customSlotMaterials = component.CustomSlotMaterials;
		foreach (Slot slot in component.Skeleton.Slots)
		{
			if (slot.Data.BlendMode == BlendMode.Multiply)
			{
				customSlotMaterials[slot] = multiplyVersionOfMaterial;
			}
		}
		rotateBone = ball.Skeleton.FindBone("IK_rotate");
		score = 0;
		NextMove();
	}

	private void NextMove()
	{
		score += stakeScore;
		int num = UnityEngine.Random.Range(0, 10);
		if (num == 0)
		{
			StartBalanceTask(0);
		}
		else
		{
			StartKickTask((num < 3) ? 1 : 0);
		}
	}

	private void StartKickTask(int kickNumber)
	{
		currentKickNumber = kickNumber;
		currentKickLeftOrRight = UnityEngine.Random.Range(0, 2);
		FootballKick footballKick = kicks[currentKickNumber];
		ThrowBall(footballKick.kickPoints[currentKickLeftOrRight].position);
		currentState = FootballState.FS_WaitingForKick;
		Spine.AnimationState animationState = bird.AnimationState;
		FootballKick footballKick2 = kicks[currentKickNumber];
		animationState.SetAnimation(0, footballKick2.idleAnimationName, loop: true);
		FootballKick footballKick3 = kicks[currentKickNumber];
		stakeScore = footballKick3.stake;
	}

	private void StartBalanceTask(int balanceNumber)
	{
		currentBalanceNumber = balanceNumber;
		FootballBalancer footballBalancer = balances[currentBalanceNumber];
		ThrowBall(footballBalancer.top.position);
		currentState = FootballState.FS_WaitingForBalance;
		Spine.AnimationState animationState = bird.AnimationState;
		FootballBalancer footballBalancer2 = balances[currentBalanceNumber];
		animationState.SetAnimation(0, footballBalancer2.idleAnimationName, loop: true);
		FootballBalancer footballBalancer3 = balances[currentBalanceNumber];
		stakeScore = footballBalancer3.stake;
	}

	private void StartBalance()
	{
		currentState = FootballState.FS_Balance;
		currentBalanceBirdPosition = 0f;
		currentBalanceBallPosition = 0f;
		currentBalanceBallVelocity = UnityEngine.Random.Range(0.1f, 0.5f) * ((UnityEngine.Random.Range(0, 2) != 0) ? (-1f) : 1f);
		Spine.AnimationState animationState = bird.AnimationState;
		FootballBalancer footballBalancer = balances[currentBalanceNumber];
		balanceTrackEntry = animationState.SetAnimation(0, footballBalancer.playAnimationName, loop: false);
		balanceTrackEntry.TrackTime = (currentBalanceBirdPosition + 1f) / 2f * balanceTrackEntry.AnimationEnd;
		balanceTimer = 3f;
	}

	private void Update()
	{
		if (currentState == FootballState.FS_WaitingForKick)
		{
			if (Input.GetMouseButtonDown(0))
			{
				Vector3 mousePosition = UnityEngine.Input.mousePosition;
				if (mousePosition.x < (float)(Screen.width / 2))
				{
					goto IL_0040;
				}
			}
			if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
			{
				goto IL_0040;
			}
			if (Input.GetMouseButtonDown(0))
			{
				Vector3 mousePosition2 = UnityEngine.Input.mousePosition;
				if (mousePosition2.x >= (float)(Screen.width / 2))
				{
					goto IL_0080;
				}
			}
			if (!Input.GetKeyDown(KeyCode.RightArrow))
			{
				return;
			}
			goto IL_0080;
		}
		if (currentState == FootballState.FS_WaitingForBalance)
		{
			if (!(ballVelocity.y < 0f))
			{
				return;
			}
			Vector3 position = ball.transform.position;
			float y = position.y;
			FootballBalancer footballBalancer = balances[currentBalanceNumber];
			Vector3 position2 = footballBalancer.top.position;
			if (y < position2.y)
			{
				if (throwCoroutine != null)
				{
					StopCoroutine(throwCoroutine);
				}
				Transform transform = ball.transform;
				FootballBalancer footballBalancer2 = balances[currentBalanceNumber];
				transform.position = footballBalancer2.top.position;
				StartBalance();
			}
			return;
		}
		if (currentState != FootballState.FS_Balance)
		{
			return;
		}
		currentBalanceBallPosition += currentBalanceBallVelocity * Time.deltaTime;
		currentBalanceBallVelocity += 5f * currentBalanceBallPosition * Time.deltaTime;
		currentBalanceBallVelocity += 5f * currentBalanceBirdPosition * Time.deltaTime;
		if (Input.GetMouseButton(0))
		{
			Vector3 mousePosition3 = UnityEngine.Input.mousePosition;
			if (mousePosition3.x < (float)(Screen.width / 2))
			{
				goto IL_01da;
			}
		}
		if (UnityEngine.Input.GetKey(KeyCode.LeftArrow))
		{
			goto IL_01da;
		}
		if (Input.GetMouseButton(0))
		{
			Vector3 mousePosition4 = UnityEngine.Input.mousePosition;
			if (mousePosition4.x >= (float)(Screen.width / 2))
			{
				goto IL_022c;
			}
		}
		if (UnityEngine.Input.GetKey(KeyCode.RightArrow))
		{
			goto IL_022c;
		}
		float num = 5f * Time.deltaTime * Mathf.Sign(currentBalanceBirdPosition);
		if (Mathf.Abs(currentBalanceBirdPosition) > Mathf.Abs(num))
		{
			currentBalanceBirdPosition -= num;
		}
		else
		{
			currentBalanceBirdPosition = 0f;
		}
		goto IL_0298;
		IL_01da:
		currentBalanceBirdPosition -= 5f * Time.deltaTime;
		goto IL_0298;
		IL_0298:
		currentBalanceBirdPosition = Mathf.Clamp(currentBalanceBirdPosition, -1f, 1f);
		if (Mathf.Abs(currentBalanceBallPosition) > 1f)
		{
			Fail();
			return;
		}
		balanceTrackEntry.TrackTime = (currentBalanceBirdPosition + 1f) / 2f * balanceTrackEntry.AnimationEnd;
		Vector3 right = Vector3.right;
		FootballBalancer footballBalancer3 = balances[currentBalanceNumber];
		Vector3 position3 = footballBalancer3.left.position;
		FootballBalancer footballBalancer4 = balances[currentBalanceNumber];
		float a = (float)Math.PI / 180f * Vector3.Angle(right, position3 - footballBalancer4.center.position);
		Vector3 right2 = Vector3.right;
		FootballBalancer footballBalancer5 = balances[currentBalanceNumber];
		Vector3 position4 = footballBalancer5.right.position;
		FootballBalancer footballBalancer6 = balances[currentBalanceNumber];
		float b = (float)Math.PI / 180f * Vector3.Angle(right2, position4 - footballBalancer6.center.position);
		float f = Mathf.Lerp(a, b, (currentBalanceBallPosition + 1f) / 2f);
		float num2;
		if (currentBalanceBallPosition < 0f)
		{
			FootballBalancer footballBalancer7 = balances[currentBalanceNumber];
			Vector3 position5 = footballBalancer7.center.position;
			FootballBalancer footballBalancer8 = balances[currentBalanceNumber];
			float magnitude = (position5 - footballBalancer8.top.position).magnitude;
			FootballBalancer footballBalancer9 = balances[currentBalanceNumber];
			Vector3 position6 = footballBalancer9.center.position;
			FootballBalancer footballBalancer10 = balances[currentBalanceNumber];
			num2 = Mathf.Lerp(magnitude, (position6 - footballBalancer10.left.position).magnitude, Mathf.Abs(currentBalanceBallPosition));
		}
		else
		{
			FootballBalancer footballBalancer11 = balances[currentBalanceNumber];
			Vector3 position7 = footballBalancer11.center.position;
			FootballBalancer footballBalancer12 = balances[currentBalanceNumber];
			float magnitude2 = (position7 - footballBalancer12.top.position).magnitude;
			FootballBalancer footballBalancer13 = balances[currentBalanceNumber];
			Vector3 position8 = footballBalancer13.center.position;
			FootballBalancer footballBalancer14 = balances[currentBalanceNumber];
			num2 = Mathf.Lerp(magnitude2, (position8 - footballBalancer14.right.position).magnitude, Mathf.Abs(currentBalanceBallPosition));
		}
		Vector3 position9 = ball.transform.position;
		Transform transform2 = ball.transform;
		FootballBalancer footballBalancer15 = balances[currentBalanceNumber];
		transform2.position = footballBalancer15.center.position + new Vector3(num2 * Mathf.Cos(f), num2 * Mathf.Sin(f), 0f);
		float magnitude3 = (ball.transform.position - position9).magnitude;
		Vector3 vector = ball.transform.position - position9;
		float num3 = magnitude3 * Mathf.Sign(vector.x);
		rotateBone.Rotation -= num3 * 100f;
		ballVelocity = (ball.transform.position - position9) / Time.deltaTime;
		ballAngularVelocity = (0f - num3) * 100f / Time.deltaTime;
		balanceTimer -= Time.deltaTime;
		if (balanceTimer < 0f)
		{
			NextMove();
		}
		return;
		IL_0080:
		Kick(1);
		return;
		IL_0040:
		Kick(0);
		return;
		IL_022c:
		currentBalanceBirdPosition += 5f * Time.deltaTime;
		goto IL_0298;
	}

	private void Kick(int leftOrRight)
	{
		Spine.AnimationState animationState = bird.AnimationState;
		FootballKick footballKick = kicks[currentKickNumber];
		animationState.SetAnimation(0, footballKick.kickAnimationNames[leftOrRight], loop: false);
		Spine.AnimationState animationState2 = bird.AnimationState;
		FootballKick footballKick2 = kicks[currentKickNumber];
		animationState2.AddAnimation(0, footballKick2.idleAnimationName, loop: true, 0f);
		if (leftOrRight == currentKickLeftOrRight)
		{
			if (catchCoroutine != null)
			{
				StopCoroutine(catchCoroutine);
			}
			catchCoroutine = StartCoroutine(CatchCoroutine());
		}
	}

	private IEnumerator CatchCoroutine()
	{
		yield return new WaitForSeconds(0.05f);
		float timer = 0.25f;
		Vector3 position = ball.transform.position;
		float ballPreviousPosition = position.y;
		while (true)
		{
			if (!(timer > 0f))
			{
				yield break;
			}
			Vector3 position2 = ball.transform.position;
			float ballCurrentPosition = position2.y;
			yield return null;
			float num = ballCurrentPosition;
			FootballKick footballKick = kicks[currentKickNumber];
			Vector3 position3 = footballKick.kickPoints[currentKickLeftOrRight].position;
			if (num <= position3.y)
			{
				float num2 = ballPreviousPosition;
				FootballKick footballKick2 = kicks[currentKickNumber];
				Vector3 position4 = footballKick2.kickPoints[currentKickLeftOrRight].position;
				if (num2 > position4.y)
				{
					break;
				}
			}
			ballPreviousPosition = ballCurrentPosition;
			timer -= Time.deltaTime;
		}
		Transform transform = ball.transform;
		FootballKick footballKick3 = kicks[currentKickNumber];
		transform.position = footballKick3.kickPoints[currentKickLeftOrRight].position;
		NextMove();
	}

	private void ThrowBall(Vector3 targetPosition)
	{
		if (throwCoroutine != null)
		{
			StopCoroutine(throwCoroutine);
		}
		throwCoroutine = StartCoroutine(ThrowCoroutine(targetPosition));
	}

	private IEnumerator ThrowCoroutine(Vector3 targetPosition)
	{
		Vector3 startPosition = ball.transform.position;
		Vector3 topPosition = Vector3.zero;
		float y = startPosition.y + 1f;
		float y2 = targetPosition.y + 2.5f;
		topPosition.y = Mathf.Max(y, y2);
		ballVelocity = Vector3.zero;
		float h3 = topPosition.y - startPosition.y;
		float t3 = Mathf.Sqrt(2f * h3 / 30f);
		ballVelocity.y = t3 * 30f;
		float h2 = topPosition.y - targetPosition.y;
		float t2 = Mathf.Sqrt(2f * h2 / 30f);
		float t = t3 + t2;
		topPosition.x = Mathf.Lerp(startPosition.x, targetPosition.x, t3 / t);
		ballVelocity.x = (targetPosition.x - startPosition.x) / t;
		ballAngularVelocity = UnityEngine.Random.Range(-300f, 300f);
		while (true)
		{
			ball.transform.position += ballVelocity * Time.deltaTime;
			ballVelocity.y -= 30f * Time.deltaTime;
			rotateBone.Rotation += ballAngularVelocity * Time.deltaTime;
			Vector3 position = ball.transform.position;
			if (position.y < -2.4f && ballVelocity.y < 0f)
			{
				break;
			}
			yield return null;
		}
		Fail();
	}

	public FootballState GetState()
	{
		return currentState;
	}

	public float GetBalancePosition()
	{
		return currentBalanceBallPosition;
	}

	private void Fail()
	{
		currentState = FootballState.FS_BallDropped;
		StartCoroutine(DropCoroutine());
		int num = PlayerPrefs.HasKey("footballScore") ? PlayerPrefs.GetInt("footballScore") : 0;
		if (score > num)
		{
			hud.Fail("新纪录");
		}
		else
		{
			hud.Fail("最高：" + num.ToString());
		}
		num = Mathf.Max(score, num);
		PlayerPrefs.SetInt("footballScore", num);
	}

	private IEnumerator DropCoroutine()
	{
		for (float timer = 2f; timer > 0f; timer -= Time.deltaTime)
		{
			yield return null;
			ball.transform.position += ballVelocity * Time.deltaTime;
			ballVelocity.y -= 30f * Time.deltaTime;
			rotateBone.Rotation += ballAngularVelocity * Time.deltaTime;
			Vector3 position = ball.transform.position;
			if (position.y < -2.5f)
			{
				Transform transform = ball.transform;
				Vector3 position2 = ball.transform.position;
				float x = position2.x;
				Vector3 position3 = ball.transform.position;
				float y = -2.5f - (position3.y + 2.5f);
				Vector3 position4 = ball.transform.position;
				transform.position = new Vector3(x, y, position4.z);
				ballVelocity.x *= 0.5f;
				ballVelocity.y *= -0.5f;
				ballAngularVelocity *= 0.5f;
			}
		}
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public int GetScore()
	{
		return score;
	}
}
