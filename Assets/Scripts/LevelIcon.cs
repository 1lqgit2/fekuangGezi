using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class LevelIcon : MonoBehaviour
{
	[SerializeField]
	private SkeletonGraphic activePicture;

	[SerializeField]
	private Image passedPicture;

	[SerializeField]
	private Image lockedPicture;

	[SerializeField]
	private SkeletonGraphic pictureForVideo;

	[SerializeField]
	private Text number;

	[SerializeField]
	private Text activeNumber;

	[SerializeField]
	private Image yellowStar1;

	[SerializeField]
	private Image yellowStar2;

	[SerializeField]
	private Image yellowStar3;

	[SerializeField]
	private Image blackStar1;

	[SerializeField]
	private Image blackStar2;

	[SerializeField]
	private Image blackStar3;

	[SerializeField]
	private Image cloud;

	[SerializeField]
	private Color cornColor;

	[SerializeField]
	private Color raspberryColor;

	[SerializeField]
	private Color peaColor;

	[SerializeField]
	private Color watermelonColor;

	[SerializeField]
	private Color garnetColor;

	[SerializeField]
	private Color blackberryColor;

	[SerializeField]
	private Color cherryColor;

	[SerializeField]
	private Color kiwiColor;

	[SerializeField]
	private Color pistachioColor;

	[SerializeField]
	private Color almondColor;

	[SerializeField]
	private Color hazelnutColor;

	[SerializeField]
	private Color durianColor;

	[SerializeField]
	private Color appleColor;

	private bool isNext;

	public void Init(bool isActive, bool isVideo, bool isLocked, Level.FruitType fruitType, bool hasFog, int n, int stars)
	{
		if (isActive)
		{
			lockedPicture.gameObject.SetActive(value: false);
			activePicture.gameObject.SetActive(value: true);
			pictureForVideo.gameObject.SetActive(value: false);
			passedPicture.gameObject.SetActive(value: false);
			cloud.gameObject.SetActive(value: false);
			activeNumber.text = (n + 1).ToString();
		}
		else if (isVideo)
		{
			isNext = true;
			lockedPicture.gameObject.SetActive(value: true);
			activePicture.gameObject.SetActive(value: false);
			pictureForVideo.gameObject.SetActive(value: false);
			passedPicture.gameObject.SetActive(value: false);
			cloud.gameObject.SetActive(value: false);
			blackStar1.gameObject.SetActive(value: false);
			blackStar2.gameObject.SetActive(value: false);
			blackStar3.gameObject.SetActive(value: false);
			number.text = (n + 1).ToString();
		}
		else if (isLocked)
		{
			lockedPicture.gameObject.SetActive(value: true);
			activePicture.gameObject.SetActive(value: false);
			pictureForVideo.gameObject.SetActive(value: false);
			passedPicture.gameObject.SetActive(value: false);
			cloud.gameObject.SetActive(value: false);
		}
		else
		{
			lockedPicture.gameObject.SetActive(value: false);
			activePicture.gameObject.SetActive(value: false);
			pictureForVideo.gameObject.SetActive(value: false);
			passedPicture.gameObject.SetActive(value: true);
			cloud.gameObject.SetActive(hasFog);
			number.text = (n + 1).ToString();
			if (stars >= 1)
			{
				yellowStar1.gameObject.SetActive(value: true);
				blackStar1.gameObject.SetActive(value: false);
			}
			if (stars >= 2)
			{
				yellowStar2.gameObject.SetActive(value: true);
				blackStar2.gameObject.SetActive(value: false);
			}
			if (stars == 3)
			{
				yellowStar3.gameObject.SetActive(value: true);
				blackStar2.gameObject.SetActive(value: false);
			}
		}
		switch (fruitType)
		{
		case Level.FruitType.Corn:
			passedPicture.color = cornColor;
			break;
		case Level.FruitType.Garnet:
			passedPicture.color = garnetColor;
			break;
		case Level.FruitType.Watermelon:
			passedPicture.color = watermelonColor;
			break;
		case Level.FruitType.Blackberry:
			passedPicture.color = blackberryColor;
			break;
		case Level.FruitType.Raspberry:
			passedPicture.color = raspberryColor;
			break;
		case Level.FruitType.Cherry:
			passedPicture.color = cherryColor;
			break;
		case Level.FruitType.Pea:
			passedPicture.color = peaColor;
			break;
		case Level.FruitType.Rotator:
			passedPicture.color = kiwiColor;
			break;
		case Level.FruitType.Pistachio:
			passedPicture.color = pistachioColor;
			break;
		case Level.FruitType.Almond:
			passedPicture.color = almondColor;
			break;
		case Level.FruitType.Hazelnut:
			passedPicture.color = hazelnutColor;
			break;
		case Level.FruitType.Durian:
			passedPicture.color = durianColor;
			break;
		case Level.FruitType.Apple:
			passedPicture.color = appleColor;
			break;
		}
	}

	private void Update()
	{
		if (isNext)
		{
			lockedPicture.gameObject.SetActive(!BaseGameManager<AdsManager>.GetInstance().IsVideoReady());
			passedPicture.gameObject.SetActive(BaseGameManager<AdsManager>.GetInstance().IsVideoReady());
			pictureForVideo.gameObject.SetActive(BaseGameManager<AdsManager>.GetInstance().IsVideoReady());
		}
	}
}
