using UnityEngine;
using UnityEngine.SceneManagement;

public class EmptyStart : MonoBehaviour
{
	private void Start()
	{
		BaseGameManager<AdsManager>.GetInstance();
		BaseGameManager<OnlineManager>.GetInstance();
		SceneManager.LoadScene("Scene1");
	}
}
