using UnityEngine;
using UnityEngine.UI;

public class MoneyBar : BaseSceneManager<MoneyBar>
{
	[SerializeField]
	private Text coins;

	[SerializeField]
	private Animator animator;

	private void Update()
	{
		coins.text = BaseGameManager<MainManager>.GetInstance().GetCoins().ToString();
	}

	public void SetTextColor(Color textColor)
	{
		coins.color = textColor;
	}

	public void PlusOne()
	{
		animator.Play("Base Layer.OneWormEaten", 0, 0f);
	}

	public void PlusThree()
	{
		animator.Play("Base Layer.ThreeWormsEaten", 0, 0f);
	}

	public void PlusFive()
	{
		animator.Play("Base Layer.FiveWormsEaten", 0, 0f);
	}

	public void PlusTen()
	{
		animator.Play("Base Layer.TenWormsEaten", 0, 0f);
	}
}
