using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 激励广告弹窗
/// </summary>
public class AdConfirm : MonoBehaviour
{
    private static AdConfirm instance;

    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnConfirm;

    /// <summary>
    /// 场景按钮直接 SetActive 打开弹窗时发放的金币数，与预制体上写的「100」一致。
    /// 走 AdConfirm.Show(回调) 打开时以外部回调为准，不使用这个值。
    /// </summary>
    [SerializeField] private int defaultRewardCoins = 100;

    private Action onCompleted;

    // Show() 打开的标记。SetActive(true) 会同步触发 OnEnable，用它区分「脚本打开」和「按钮直接 SetActive」。
    private bool openedByShow;

    // 广告是否正在播放 / 本次是否已经发过奖，防止连点导致重复播放或重复发币
    private bool adPlaying;
    private bool rewarded;

    public static void Show(Action callBack)
    {
        if (instance == null)
        {
            // 场景里可能预先摆了一份（默认隐藏，靠按钮 SetActive 打开）。
            // 优先复用它，否则会同时存在两份弹窗，后 Awake 的那份会被销毁。
            instance = FindFirstObjectByType<AdConfirm>(FindObjectsInactive.Include);
        }

        if (instance == null)
        {
            // 加载 AdConfirm 预制体
            GameObject prefab = Resources.Load<GameObject>("AdConfirm");
            if (prefab == null)
            {
                Debug.LogError("AdConfirm 预制体未找到，请确保其位于 Resources 文件夹中。");
                return;
            }

            // 实例化 AdConfirm
            GameObject adConfirmInstance = Instantiate(prefab);
            instance = adConfirmInstance.GetComponent<AdConfirm>();

            if (instance == null)
            {
                Debug.LogError("AdConfirm 脚本未附加到预制体上，请检查预制体设置。");
                Destroy(adConfirmInstance);
                return;
            }
        }

        // 设置回调并显示弹窗
        instance.openedByShow = true;
        instance.onCompleted = callBack;
        // 弹窗已经开着时 SetActive(true) 不会再触发 OnEnable，这里显式重置一次
        instance.adPlaying = false;
        instance.rewarded = false;
        instance.gameObject.SetActive(true);
        instance.openedByShow = false;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        FitToParent();
        btnClose?.onClick.AddListener(this.OnClose);
        btnConfirm?.onClick.AddListener(this.OnConfirm);
    }

    private void OnEnable()
    {
        // 每次显示都校正一次，避免预制体被拖到别的 Canvas 下、或锚点被改动后弹窗跑偏
        FitToParent();

        if (!openedByShow)
        {
            // jiliBtn 的 UnityEvent 是直接 SetActive(true) 打开本弹窗，不经过 Show()，
            // 所以没有外部回调。这里清掉可能残留的旧回调，改走默认金币奖励。
            onCompleted = null;
        }

        adPlaying = false;
        rewarded = false;
        SetConfirmInteractable(true);
    }

    /// <summary>
    /// 把弹窗矩形铺满父级。
    /// 本预制体自带 Canvas，当它被放在另一个 Canvas 下面时（嵌套 Canvas），
    /// Unity 不会像根 Canvas 那样自动把矩形撑成屏幕大小，必须自己拉伸，
    /// 否则内部按 (0.5,0.5) 居中的 panel 会贴到父级某个角落。
    /// </summary>
    private void FitToParent()
    {
        if (!(transform is RectTransform rt))
        {
            return;
        }

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }


    private void OnClose()
    {
        this.gameObject.SetActive(false);
    }

    void OnConfirm()
    {
        if (adPlaying || rewarded)
        {
            // 已经在播 / 已经发过奖，连点直接忽略
            return;
        }

#if UNITY_EDITOR
        Debug.Log("[广告-编辑器] 模拟观看激励视频完成（真机走抖音激励视频）");
        OnAdWatched();
#else
        if (AdManager.Instance == null)
        {
            Debug.LogError("[广告] 场景里没有 AdManager，无法播放广告");
            OnAdError("广告服务未初始化");
            return;
        }

        adPlaying = true;
        // 播放期间禁用确认按钮，避免一次点击弹出多个广告、发多份奖励
        SetConfirmInteractable(false);
        AdManager.Instance.ShowRewardedVideoAd(OnAdWatched, OnAdError);
#endif
    }

    private void OnAdWatched()
    {
        adPlaying = false;
        SetConfirmInteractable(true);

        if (rewarded)
        {
            return;
        }

        rewarded = true;
        Debug.Log("[广告] 激励视频观看完成，开始发放奖励");

        if (onCompleted != null)
        {
            // 外部通过 AdConfirm.Show(回调) 打开，由调用方决定发什么奖励
            onCompleted.Invoke();
        }
        else
        {
            // jiliBtn 这个入口没有外部回调，按弹窗上标注的金币数发奖
            GrantDefaultReward();
        }

        OnClose();
    }

    /// <summary>
    /// 没有外部回调时发放默认金币奖励（jiliBtn 的「看广告获得奖励 100 金币」）。
    /// </summary>
    private void GrantDefaultReward()
    {
        int coins = Mathf.Max(1, defaultRewardCoins);

        MainManager main = BaseGameManager<MainManager>.GetInstance();
        if (main == null)
        {
            Debug.LogError("[广告] MainManager 不存在，无法发放金币奖励");
            return;
        }

        main.CoinsEarned(coins);
        Debug.Log($"[广告] 激励视频奖励已到账：+{coins} 金币，当前共 {main.GetCoins()}");
    }

    private void SetConfirmInteractable(bool value)
    {
        if (btnConfirm != null)
        {
            btnConfirm.interactable = value;
        }
    }

    private void OnAdError(string errorMessage)
    {
        adPlaying = false;
        SetConfirmInteractable(true);
        Debug.LogError($"广告播放失败: {errorMessage}");
        // 加载 AdError 预制体
        GameObject adErrorPrefab = Resources.Load<GameObject>("AdError");
        if (adErrorPrefab != null)
        {
            // 实例化 AdError
            GameObject adErrorInstance = Instantiate(adErrorPrefab, transform.position, Quaternion.identity);
            // 1 秒后销毁实例
            Destroy(adErrorInstance, 1.5f);
        }
        else
        {
            Debug.LogError("AdError 预制体未找到，请确保其位于 Resources 文件夹中。");
        }
    }
}
