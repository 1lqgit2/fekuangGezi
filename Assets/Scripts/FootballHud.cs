using UnityEngine;
using UnityEngine.UI;

public class FootballHud : MonoBehaviour
{
	[SerializeField]
	private GameObject balanceHolder;

	[SerializeField]
	private RectTransform balanceBall;

	[SerializeField]
	private Text score;

	[SerializeField]
	private Text bestScore;

	private void Update()
	{
		balanceHolder.gameObject.SetActive(BaseSceneManager<Football>.Instance.GetState() == Football.FootballState.FS_Balance);
		score.gameObject.SetActive(BaseSceneManager<Football>.Instance.GetState() != Football.FootballState.FS_Balance);
		RectTransform rectTransform = balanceBall;
		float x = BaseSceneManager<Football>.Instance.GetBalancePosition() * 250f;
		Vector2 anchoredPosition = balanceBall.anchoredPosition;
		rectTransform.anchoredPosition = new Vector2(x, anchoredPosition.y);
		score.text = BaseSceneManager<Football>.Instance.GetScore().ToString();
	}

	public void Fail(string recordText)
	{
		bestScore.text = recordText;
	}
}
