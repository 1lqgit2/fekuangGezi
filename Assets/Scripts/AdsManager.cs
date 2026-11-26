using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Purchasing;

/// <summary>
/// 广告管理（抖音小游戏）。
///
/// 这个类原本接的是 Heyzap 聚合广告（Standard Assets 里的 HZ* 系列），现在统一改走
/// AdSDK 的 AdManager（抖音小游戏 TTSDK），项目里只剩一套广告 SDK。
///
/// 所有公开方法名与签名都保持原样，Gameplay / Feeding / Skins / GameSettings 等
/// 二十多处调用点不需要改动。
/// </summary>
public class AdsManager : BaseGameManager<AdsManager>, IStoreListener
{
	private int interstitialCounter;

	private Action<bool> videoCallback;

	private static IStoreController m_StoreController;

	private static IExtensionProvider m_StoreExtensionProvider;

	private bool isPurchaseInProgress;

	public static string productIDNoAds = "iap_pigeonpop_noads";

	private bool isVideoReady;

	private bool isVideoPlaying;

	private float videoStartTime;

	private void Awake()
	{
		// 抖音小游戏跑在 WebGL 上，没有应用商店内购，UnityPurchasing 在这里初始化会失败。
		// 只在原生平台初始化，抖音包里「去广告」入口保留但不可购买。
		if (Application.platform != RuntimePlatform.WebGLPlayer)
		{
			InitializePurchasing();
		}
		StartCoroutine(IsVideoReadyCoroutine());
	}

	/// <summary>
	/// 抖音的激励视频是「点了才去拉广告位」，没有 Heyzap 那种「预加载好没好」的状态。
	/// 这里用 AdManager 是否已就绪作为判断依据，避免 UI 上的看视频按钮永远置灰；
	/// 真正拉取失败会在 ShowVideo 的回调里返回 false。
	/// </summary>
	private IEnumerator IsVideoReadyCoroutine()
	{
		while (true)
		{
			isVideoReady = (AdManager.Instance != null);
			yield return new WaitForSeconds(1f);
		}
	}

	public bool IsVideoReady()
	{
		return isVideoReady;
	}

	public void ShowVideo(Action<bool> callback)
	{
		// 已经在播了就拒绝：否则会把上一次的回调覆盖掉，先点的那次永远收不到结果（奖励丢失）
		if (isVideoPlaying)
		{
			if (Time.realtimeSinceStartup - videoStartTime < 300f)
			{
				UnityEngine.Debug.LogWarning("[广告] 激励视频正在播放中，忽略重复请求");
				callback?.Invoke(false);
				return;
			}

			// 上一次的广告回调彻底丢了（抖音偶发），超过 5 分钟就复位，否则之后再也看不了广告
			UnityEngine.Debug.LogWarning("[广告] 上一次激励视频超时未返回结果，已复位");
			videoCallback = null;
			isVideoPlaying = false;
		}

		isVideoPlaying = true;
		videoStartTime = Time.realtimeSinceStartup;
		videoCallback = callback;

#if UNITY_EDITOR
		// 编辑器里没有真实广告环境，直接模拟一次「看完」，方便验证奖励流程。
		// 与 AdConfirm.OnConfirm 的处理保持一致。
		UnityEngine.Debug.Log("[广告-编辑器] 模拟观看激励视频完成（真机走抖音激励视频）");
		StartCoroutine(SimulateVideoWatched());
#else
		if (AdManager.Instance == null)
		{
			UnityEngine.Debug.LogError("[广告] 场景里没有 AdManager，无法播放激励视频");
			FinishVideo(false);
			return;
		}
		// 抖音平台实现内部会暂停 Time.timeScale，并在 OnClose 里恢复
		AdManager.Instance.ShowRewardedVideoAd(OnVideoWatched, OnVideoError);
#endif
	}

	private IEnumerator SimulateVideoWatched()
	{
		yield return new WaitForSecondsRealtime(0.3f);
		OnVideoWatched();
	}

	private void OnVideoWatched()
	{
		interstitialCounter = 0;
		UnityEngine.Debug.Log("[广告] 激励视频看完，开始发放奖励");
		FinishVideo(true);
	}

	private void OnVideoError(string errorMessage)
	{
		UnityEngine.Debug.LogWarning("[广告] 激励视频未完成：" + errorMessage);
		FinishVideo(false);
	}

	private void FinishVideo(bool result)
	{
		isVideoPlaying = false;
		Action<bool> callback = videoCallback;
		videoCallback = null;
		if (callback != null)
		{
			callback(result);
		}
		else
		{
			UnityEngine.Debug.LogWarning("[广告] 激励视频结束，但调用方没有传回调，奖励不会被发放");
		}
	}

	public void ShowInterstitial()
	{
		if (PlayerPrefs.HasKey("NoAds"))
		{
			return;
		}
		interstitialCounter++;
		// 保留原来的频次控制：每 6 次触发一次插屏
		if (interstitialCounter <= 5)
		{
			return;
		}
		interstitialCounter = 0;
		if (AdManager.Instance == null)
		{
			Debug.LogWarning("[广告] 场景里没有 AdManager，跳过插屏");
			return;
		}
		// 抖音平台实现内部还有「启动 15 秒内不弹」「两次间隔 30 秒」两道限制
		AdManager.Instance.ShowInterstitialAd();
	}

	/// <summary>设置面板里的测试按钮：直接请求一次插屏广告。</summary>
	public void TestAds()
	{
		if (AdManager.Instance == null)
		{
			Debug.LogWarning("[广告] 场景里没有 AdManager，无法测试广告");
			return;
		}
		Debug.Log("[广告] 测试：请求插屏广告（需避开启动 15 秒与 30 秒间隔限制）");
		AdManager.Instance.ShowInterstitialAd();
	}

	public void InitializePurchasing()
	{
		if (!IsPurchasingInitialized())
		{
			ConfigurationBuilder configurationBuilder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
			configurationBuilder.AddProduct(productIDNoAds, ProductType.NonConsumable);
			UnityPurchasing.Initialize(this, configurationBuilder);
		}
	}

	private bool IsPurchasingInitialized()
	{
		return m_StoreController != null && m_StoreExtensionProvider != null;
	}

	private bool BuyProductID(string productId)
	{
		if (IsPurchasingInitialized())
		{
			Product product = m_StoreController.products.WithID(productId);
			if (product != null && product.availableToPurchase)
			{
				UnityEngine.Debug.Log($"Purchasing product asychronously: '{product.definition.id}'");
				m_StoreController.InitiatePurchase(product);
				return true;
			}
			UnityEngine.Debug.Log("Purchase failed. Not purchasing product, either is not found or is not available for purchase.");
			return false;
		}
		UnityEngine.Debug.Log("Purchase failed. Not initialized.");
		return false;
	}

	public bool RestorePurchases()
	{
		if (!IsPurchasingInitialized() || isPurchaseInProgress)
		{
			UnityEngine.Debug.Log("Restore purchases failed. Not initialized.");
			return false;
		}
		if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
		{
			UnityEngine.Debug.Log("RestorePurchases started ...");
			IAppleExtensions extension = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
			// Unity 2022 已移除 Handheld 及其 ActivityIndicator 系列 API，这里不再显示系统转圈指示器
			isPurchaseInProgress = true;
			extension.RestoreTransactions(delegate(bool result)
			{
				UnityEngine.Debug.Log("Restore purchases continuing: " + result + ". If no further messages, no purchases available to restore.");
				isPurchaseInProgress = false;
			});
			return true;
		}
		UnityEngine.Debug.Log("Restore purchases failed. Not supported on this platform. Current = " + Application.platform);
		return false;
	}

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		UnityEngine.Debug.Log("Purchasing initialized!");
		m_StoreController = controller;
		m_StoreExtensionProvider = extensions;
	}

	public void OnInitializeFailed(InitializationFailureReason error)
	{
		UnityEngine.Debug.Log("Purchasing initialization failure reason: " + error);
	}

	public void OnInitializeFailed(InitializationFailureReason error, string message)
	{
		UnityEngine.Debug.Log("Purchasing initialization failure: " + error + " / " + message);
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
	{
		isPurchaseInProgress = false;
		if (string.Equals(args.purchasedProduct.definition.id, productIDNoAds, StringComparison.Ordinal))
		{
			PlayerPrefs.SetInt("NoAds", 1);
			UnityEngine.Debug.Log("Process purchase successfull. Product: " + args.purchasedProduct.definition.id);
		}
		else
		{
			UnityEngine.Debug.Log("Process purchase failure. Unrecognized product: " + args.purchasedProduct.definition.id);
		}
		return PurchaseProcessingResult.Complete;
	}

	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
	{
		isPurchaseInProgress = false;
		UnityEngine.Debug.Log($"On purchase failed. Product: '{product.definition.storeSpecificId}', PurchaseFailureReason: {failureReason}");
	}

	public void BuyNoAds()
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			// 抖音小游戏内购要走抖音支付，Unity IAP 在 WebGL 下不可用。
			UnityEngine.Debug.LogWarning("[内购] 抖音小游戏未接入支付，去广告暂不可购买");
			return;
		}
		if (!isPurchaseInProgress && BuyProductID(productIDNoAds))
		{
			isPurchaseInProgress = true;
		}
	}

	/// <summary>
	/// 原来是销毁 Heyzap 的 banner。抖音这套 AdSDK 目前只封装了激励视频与插屏，
	/// 没有 banner，所以这里只保留一个入口，避免调用方改动。
	/// </summary>
	public void RefreshNoAds()
	{
		if (PlayerPrefs.HasKey("NoAds"))
		{
			UnityEngine.Debug.Log("[广告] 已去广告，当前接入没有 banner，无需隐藏");
		}
	}
}
