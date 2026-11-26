using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 侧边栏复访面板（cebianPanel）的跳转逻辑。
///
/// 背景：抖音审核驳回——「侧边栏跳转按钮，点击后无法跳转」。
/// 原因有两条，都在这里兜住：
///   1. cebianPanel.prefab 里 box/go 这个「前往侧边栏」按钮的 m_OnClick 是空的，
///      场景 Scene1.unity 又把它显式 override 成 m_Calls.Array.size = 0，
///      点下去不会有任何反应；
///   2. 本脚本之前从来没挂在任何物体上（guid 在 .unity/.prefab 里都搜不到），
///      NavigateToSideBar 是一段死代码。
///
/// 所以这里改成「运行时自举」：场景加载后自动把本组件挂到 cebianPanel 上，
/// 再按层级找到 box/go 按钮重新绑定，不需要改场景 YAML。
/// </summary>
public class cebian : MonoBehaviour
{
	//前往按钮
	public Button btn_Go;

	/// <summary>场景加载回调只注册一次</summary>
	private static bool registered;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void RegisterSceneCallback()
	{
		if (registered)
		{
			return;
		}

		registered = true;
		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	/// <summary>
	/// 每次场景加载后，在所有根节点（包括 inactive 的面板）里找 cebianPanel 并补挂组件。
	/// 用 GetComponentsInChildren(true) 而不是 Find：面板默认是隐藏的，Find 找不到。
	/// </summary>
	private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (!scene.IsValid())
		{
			return;
		}

		GameObject[] roots = scene.GetRootGameObjects();
		for (int i = 0; i < roots.Length; i++)
		{
			Transform[] all = roots[i].GetComponentsInChildren<Transform>(includeInactive: true);
			for (int j = 0; j < all.Length; j++)
			{
				if (all[j].name != "cebianPanel")
				{
					continue;
				}

				if (all[j].GetComponent<cebian>() == null)
				{
					all[j].gameObject.AddComponent<cebian>();
					Debug.Log("[侧边栏] 已为 cebianPanel 补挂跳转组件");
				}
			}
		}
	}

	private void Awake()
	{
		BindGoButton();
	}

	private void OnEnable()
	{
		// 面板每次打开都重新绑一遍，防止场景里的空绑定把它冲掉
		BindGoButton();
	}

	/// <summary>
	/// 找到 box/go 按钮并绑定跳转。场景里这个按钮的持久化监听是空的，
	/// 这里补的是运行时监听，不会和场景 YAML 冲突。
	/// </summary>
	private void BindGoButton()
	{
		if (btn_Go == null)
		{
			Transform go = transform.Find("box/go");
			if (go != null)
			{
				btn_Go = go.GetComponent<Button>();
			}
		}

		if (btn_Go == null)
		{
			Debug.LogWarning("[侧边栏] 没找到 box/go 按钮，跳转未绑定");
			return;
		}

		btn_Go.onClick.RemoveListener(NavigateToSideBar);
		btn_Go.onClick.AddListener(NavigateToSideBar);
	}

	/// <summary>
	/// 跳转到侧边栏。走 AdManager 统一链路（内部就是 TT.NavigateToScene({scene:"sidebar"})），
	/// 成功后就关掉引导面板。
	/// </summary>
	public void NavigateToSideBar()
	{
		Debug.Log("[侧边栏] 点击「前往侧边栏」");

		if (AdManager.Instance == null)
		{
			Debug.LogError("[侧边栏] 场景里没有 AdManager，无法跳转");
			return;
		}

		AdManager.Instance.NavigateToScene(
			() =>
			{
				Debug.Log("[侧边栏] 唤起成功");
				gameObject.SetActive(false);
			},
			(string errMsg) => { Debug.LogError("[侧边栏] 唤起失败：" + errMsg); });
	}
}
