using UnityEngine;

public class BaseSceneManager<T> : MonoBehaviour where T : BaseSceneManager<T>
{
	private static T s_instance;

	public static T Instance
	{
		get
		{
			if ((Object)s_instance == (Object)null)
			{
				s_instance = UnityEngine.Object.FindObjectOfType<T>();
			}
			return s_instance;
		}
	}
}
