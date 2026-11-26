using UnityEngine;

public class BaseGameManager<T> : MonoBehaviour where T : BaseGameManager<T>
{
	private static T s_instance;

	public static T GetInstance()
	{
		if ((Object)s_instance == (Object)null)
		{
			GameObject gameObject = Resources.Load(typeof(T).Name) as GameObject;
			if (gameObject == null || (Object)gameObject.GetComponent<T>() == (Object)null)
			{
				UnityEngine.Debug.LogError("Prefab for game manager " + typeof(T).Name + " is not found");
			}
			else
			{
				GameObject gameObject2 = GameObject.Find("GameManagers");
				if (gameObject2 == null)
				{
					gameObject2 = new GameObject("GameManagers");
					Object.DontDestroyOnLoad(gameObject2);
				}
				GameObject gameObject3 = UnityEngine.Object.Instantiate(gameObject);
				gameObject3.transform.parent = gameObject2.transform;
				s_instance = gameObject3.GetComponent<T>();
			}
		}
		return s_instance;
	}
}
