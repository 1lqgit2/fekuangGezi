using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelScreen : MonoBehaviour
{
	[SerializeField]
	private RectTransform levelHolder;

	[SerializeField]
	private List<Transform> iconHolders;

	[SerializeField]
	private float minScroll;

	[SerializeField]
	private float maxScroll;

	[SerializeField]
	private LevelIcon originalLevelIcon;

	[SerializeField]
	private List<RectTransform> tops;

	[SerializeField]
	private Color colorTopLocked;

	[SerializeField]
	private AudioSource click;

	private bool isPointerDown;

	private bool isPointerMoved;

	private Vector3 previousPointerPosition;

	private float currentSkinsSpeed;

	private Vector2 currentSkinsPosition;

	private int currentLevelSelected;

	private Vector3 startTopPosition;

	private List<Vector3> startLocalPositions = new List<Vector3>();

	private int blocksAmount;

	private void Start()
	{
		blocksAmount = 13;
		for (int i = 0; i < iconHolders.Count; i++)
		{
			LevelIcon levelIcon = UnityEngine.Object.Instantiate(originalLevelIcon, iconHolders[i]);
			bool isActive = i == BaseSceneManager<Gameplay>.Instance.GetLevelNumber();
			bool isVideo = i == BaseSceneManager<Gameplay>.Instance.GetLastLevelUnlocked() + 1;
			bool isLocked = i > BaseSceneManager<Gameplay>.Instance.GetLastLevelUnlocked() + 1;
			levelIcon.gameObject.SetActive(value: true);
			levelIcon.transform.localPosition = Vector3.zero;
			levelIcon.transform.localRotation = Quaternion.identity;
			levelIcon.transform.localScale = Vector3.one;
			levelIcon.Init(isActive, isVideo, isLocked, BaseSceneManager<Gameplay>.Instance.GetLevelFruitType(i), BaseSceneManager<Gameplay>.Instance.DoesLevelHasFog(i), i, BaseSceneManager<Gameplay>.Instance.GetStarsOnLevel(i));
			EventTrigger.Entry entry = new EventTrigger.Entry();
			int index = i;
			entry.eventID = EventTriggerType.PointerClick;
			entry.callback.AddListener(delegate
			{
				OnLevelClicked(index);
			});
			levelIcon.GetComponent<EventTrigger>().triggers.Add(entry);
		}
		startTopPosition = tops[0].position;
		for (int j = 0; j < blocksAmount; j++)
		{
			startLocalPositions.Add(tops[j].localPosition);
		}
		for (int k = 0; k < blocksAmount; k++)
		{
			if (BaseSceneManager<Gameplay>.Instance.GetLastLevelUnlocked() < 20 * k)
			{
				tops[k].GetComponent<Image>().color = colorTopLocked;
			}
		}
	}

	private void OnEnable()
	{
		currentSkinsPosition = new Vector2(0f, minScroll);
		currentSkinsSpeed = 0f;
	}

	private void Update()
	{
		if (isPointerDown)
		{
			Vector3 mousePosition = UnityEngine.Input.mousePosition;
			float num = mousePosition.y - previousPointerPosition.y;
			currentSkinsSpeed = num / Time.deltaTime;
			if (num > 0f)
			{
				isPointerMoved = true;
			}
			previousPointerPosition = UnityEngine.Input.mousePosition;
		}
		currentSkinsPosition.y += currentSkinsSpeed * Time.deltaTime;
		if (currentSkinsPosition.y > maxScroll)
		{
			currentSkinsPosition.y = maxScroll;
			currentSkinsSpeed = 0f;
		}
		else if (currentSkinsPosition.y < minScroll)
		{
			currentSkinsPosition.y = minScroll;
			currentSkinsSpeed = 0f;
		}
		float num2 = Time.deltaTime * 1000f;
		if (Mathf.Abs(currentSkinsSpeed) < num2)
		{
			currentSkinsSpeed = 0f;
		}
		else
		{
			currentSkinsSpeed -= num2 * Mathf.Sign(currentSkinsSpeed);
		}
		levelHolder.anchoredPosition = currentSkinsPosition;
		if (currentSkinsPosition.y >= -960f && currentSkinsPosition.y < 250f)
		{
			tops[0].position = startTopPosition;
		}
		if (currentSkinsPosition.y < -960f)
		{
			tops[0].localPosition = startLocalPositions[0];
		}
		for (int i = 1; i < blocksAmount; i++)
		{
			if (currentSkinsPosition.y > -960f + (float)i * 1400f && currentSkinsPosition.y < 250f + (float)i * 1400f)
			{
				tops[i].position = startTopPosition;
			}
			if (currentSkinsPosition.y < -960f + (float)i * 1400f)
			{
				tops[i].localPosition = startLocalPositions[i];
			}
		}
		for (int j = 0; j < blocksAmount; j++)
		{
			Vector3 position = tops[j].position;
			if (position.y > startTopPosition.y - 50f)
			{
				RectTransform rectTransform = tops[j];
				Vector2 sizeDelta = tops[j].sizeDelta;
				float x = sizeDelta.x;
				Vector3 position2 = tops[j].position;
				rectTransform.sizeDelta = new Vector2(x, 200f * Mathf.Clamp((position2.y - (startTopPosition.y - 50f)) / 25f, 1f, 2f));
			}
			else
			{
				RectTransform rectTransform2 = tops[j];
				Vector2 sizeDelta2 = tops[j].sizeDelta;
				rectTransform2.sizeDelta = new Vector2(sizeDelta2.x, 200f);
			}
		}
		if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
		{
			CloseLevels();
		}
	}

	public void OnLevelClicked(int n)
	{
		if (!isPointerMoved)
		{
			if (n <= BaseSceneManager<Gameplay>.Instance.GetLastLevelUnlocked())
			{
				click.Play();
				BaseSceneManager<Gameplay>.Instance.SetLevelNumber(n);
				CloseLevels();
				BaseSceneManager<Gameplay>.Instance.Play();
			}
			else if (n == BaseSceneManager<Gameplay>.Instance.GetLastLevelUnlocked() + 1 && BaseGameManager<AdsManager>.GetInstance().IsVideoReady())
			{
				currentLevelSelected = n;
				BaseGameManager<AdsManager>.GetInstance().ShowVideo(VideoCallback);
			}
		}
	}

	private void VideoCallback(bool success)
	{
		if (success)
		{
			BaseSceneManager<Gameplay>.Instance.UnlockLevel(currentLevelSelected);
			BaseSceneManager<Gameplay>.Instance.SetLevelNumber(currentLevelSelected);
			CloseLevels();
			BaseSceneManager<Gameplay>.Instance.Play();
		}
	}

	public void OnPointerDown()
	{
		isPointerDown = true;
		isPointerMoved = false;
		previousPointerPosition = UnityEngine.Input.mousePosition;
	}

	public void OnPointerUp()
	{
		isPointerDown = false;
	}

	public void CloseLevels()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.CloseLevels();
	}
}
