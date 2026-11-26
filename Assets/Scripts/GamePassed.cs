using UnityEngine;

public class GamePassed : MonoBehaviour
{
	[SerializeField]
	private AudioSource click;

	public void MainMenu()
	{
		click.Play();
		BaseSceneManager<Gameplay>.Instance.Restart(RestartType.RT_None);
	}
}
