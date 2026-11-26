using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using TTSDK;
#endif

// 广告平台枚举
public enum AdPlatformType
{
    [InspectorName("抖音小游戏")]
    TickTok,
    [InspectorName("TopOn聚合")]
    TopOn,
    [InspectorName("微信小游戏")]
    WebChat,
}


/// <summary>
/// 广告管理
/// </summary>
public class AdManager : MonoBehaviour
{
   // public GameObject aaa;

    private static AdManager instance;

    /// <summary>
    /// 全局唯一实例。
    /// 这里用属性而不是裸静态字段：初始场景 Loadscreen 是「additive 加载 Master → 再卸载自己」的流程，
    /// 一旦静态字段指向的物体被场景卸载销毁，属性会自动重新找一个，避免 Instance 悄悄变成 null。
    /// </summary>
    public static AdManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AdManager>();
            }

            return instance;
        }
    }

    [Header("平台")]
    public AdPlatformType adPlatformType = AdPlatformType.TickTok;
    [Header("小游戏ID")]
    public string AppId;
    [Header("激励视频广告位")]
    public List<string> videoAdIdList = new();
    [Header("插屏广告位")]

    public List<string> interAdIdList = new();
    private IAdPlatform platFormIns = null;
    private float lastAdClickTime;

     void Awake()
    {
        if (instance != null && instance != this)
        {
            // 已经有一个实例了（通常是初始场景里的那个），本场景内的重复实例直接销毁，
            // 避免广告平台被初始化两次。
            // 先 enabled=false：Destroy 是延迟到帧末执行的，不禁用的话本帧 Start() 仍会跑，
            // 会把 TT.InitSDK 和广告平台再初始化一遍。
            enabled = false;
            Destroy(gameObject);
            return;
        }

        instance = this;

        // 关键：LoadscreenScene 用 LoadSceneAsync(1, Additive) 加载 Master，随后 UnloadSceneAsync(0)
        // 卸载初始场景。AdManager 如果留在初始场景里，会跟着一起被销毁，静态 Instance 变成
        // 「已销毁对象」（Unity 里 == null 为 true），点广告时就报「场景里没有 AdManager」。
        // 所以这里把它移到根节点并设为跨场景常驻。
        if (transform.parent != null)
        {
            transform.SetParent(null, true);
        }

        if (transform.root == transform)
        {
            DontDestroyOnLoad(gameObject);
            Debug.Log("[广告] AdManager 已设为跨场景常驻");
        }
        else
        {
            Debug.LogWarning("[广告] AdManager 不在根节点，无法跨场景常驻，切换场景后广告可能失效");
        }
    }

    private void Start()
    {
        // 抖音小游戏 SDK 初始化（WebGL 真机环境；编辑器下 TTSDK 为 mock，不需要初始化）
        InitTTSdk();

        if (this.AppId == "")
        {
            Debug.LogWarning("请填写APPID");
        }
        
        if (this.videoAdIdList == null || this.videoAdIdList.Count == 0)
        {
            Debug.LogWarning("视频广告位ID列表为空, 将使用默认广告位");
        }

        if (this.interAdIdList == null || this.interAdIdList.Count == 0)
        {
            Debug.LogWarning("插屏广告位ID列表为空, 将使用默认广告位");
        }

        switch (adPlatformType)
        {
            case AdPlatformType.TickTok:
                platFormIns = new TikTokPlatform(this.AppId, this.GetRandVideoAdId(), this.GetRandScreenAdId());

                break;
            case AdPlatformType.TopOn:
                Debug.LogWarning("TopOn广告平台尚未实现");
                break;
        }
        
        // 添加广告平台初始化检查
        if (this.platFormIns == null)
        {
            Debug.LogError($"未初始化广告平台：{this.adPlatformType}");
        }
        else
        {
            // 把 appid 一起打出来：广告报 1002（广告位不存在）时，第一步就是核对这个 appid 与广告位是否同一批
            Debug.Log($"[广告] 平台已就绪：抖音小游戏，AppId={this.AppId}，激励视频={this.GetRandVideoAdId()}，插屏={this.GetRandScreenAdId()}");
        }
        
        this.lastAdClickTime = Time.time;
    }

    private void Update()
    {
        // 兜底：抖音广告没有音频回调，万一某次 OnClose / OnError 都没来，
        // 不能让游戏一直静音，超时后强制恢复一次。
        if (AdAudioMute.IsMuted && AdAudioMute.MutedSeconds > AdAudioMute.WatchdogSeconds)
        {
            Debug.LogWarning("[广告] 广告静音超时仍未收到关闭回调，强制恢复游戏音效");
            AdAudioMute.Unmute();
        }
    }

    private void InitTTSdk()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            // OnTTContainerInitCallback 的签名是 void Invoke(int code, ContainerEnv env)
            TT.InitSDK((code, env) =>
            {
                if (code == 0)
                {
                    Debug.Log("[抖音SDK] 初始化完成");
                }
                else
                {
                    // 1 = SDK 版本不支持，2 = 引擎版本不支持
                    Debug.LogWarning($"[抖音SDK] 初始化失败，错误码 {code}");
                }
            });
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[抖音SDK] 初始化异常（不影响广告接口调用）：{e.Message}");
        }
#endif
    }


    /// <summary>
    /// 播放插屏广告（打开设置界面、游戏结束等入口调用）。
    /// 广告没加载好 / 还在启动保护期时内部会排队补播，不需要调用方重试。
    /// </summary>
    public void ShowInterstitialAd()
    {
        // 抖音小游戏的「去广告」是本地标记，买过的玩家不弹插屏
        if (PlayerPrefs.HasKey("NoAds"))
        {
            Debug.Log("[广告] 已购买去广告，跳过插屏");
            return;
        }

        this.EnsurePlatform();

        if (this.platFormIns == null)
        {
            Debug.LogWarning("[广告] 广告平台尚未就绪，本次插屏广告已跳过");
            return;
        }

        try
        {
            this.platFormIns.ShowInterstitialAd(this.GetRandScreenAdId());
        }
        catch (Exception e)
        {
            Debug.LogError($"显示插屏广告时发生异常：{e.Message}");
        }
    }

    /// <summary>
    /// 平台实例兜底创建：Start() 没跑（组件晚启用、场景切换）时也能正常拉起广告。
    /// </summary>
    private void EnsurePlatform()
    {
        if (this.platFormIns != null || this.adPlatformType != AdPlatformType.TickTok)
        {
            return;
        }

        try
        {
            this.platFormIns = new TikTokPlatform(this.AppId, this.GetRandVideoAdId(), this.GetRandScreenAdId());
            Debug.Log("[广告] 广告平台按需创建完成");
        }
        catch (Exception e)
        {
            Debug.LogError($"[广告] 广告平台按需创建失败：{e.Message}");
        }
    }

    /// <summary>
    /// 跳转场景
    /// </summary>
    /// <param name="onSuccess">成功回调</param>
    /// <param name="onFail">失败回调</param>
    public void NavigateToScene(Action onSuccess = null, Action<string> onFail = null)
    {
        if (this.platFormIns == null)
        {
            Debug.LogError("广告平台实例未初始化，无法跳转场景");
            onFail?.Invoke("广告平台实例未初始化");
            return;
        }
        
        try
        {
            this.platFormIns.NavigateToScene(onSuccess, onFail);
        }
        catch (Exception e)
        {
            Debug.LogError($"跳转场景时发生异常：{e.Message}");
            onFail?.Invoke($"跳转场景时发生异常：{e.Message}");
        }
    }
    public void ShareGame(Action<bool> callBack = null)
    {
        this.platFormIns?.ShareGame(callBack);
    }

    /// <summary>
    /// 播放激励视频广告
    /// </summary>
    /// <param name="onSuccess">成功回调</param>
    /// <param name="onFail">失败回调</param>
    public void ShowRewardedVideoAd(Action onSuccess, Action<string> onFail)
    {
        if (!this.CheckTimeLimit())
        {
            // 不能只是 return：调用方（AdConfirm）已经把按钮置灰，
            // 收不到任何回调就会一直卡在「播放中」，这里必须回一次失败。
            onFail?.Invoke("操作过于频繁，请稍后再试");
            return;
        }

        if (this.platFormIns == null)
        {
            this.EnsurePlatform();
        }

        if (this.platFormIns == null)
        {
            Debug.LogError("广告平台实例未初始化，无法显示广告");
            onFail?.Invoke("广告平台实例未初始化");
            return;
        }
        
        try
        {
            this.platFormIns.ShowRewardedVideoAd(this.GetRandVideoAdId(), onSuccess, onFail);
        }
        catch (Exception e)
        {
            Debug.LogError($"显示广告时发生异常：{e.Message}");
            onFail?.Invoke($"显示广告时发生异常：{e.Message}");
        }
    }


    // 随机取一个激励视频广告位ID
    private string GetRandVideoAdId()
    {
        if (this.videoAdIdList.Count == 0)
        {
            Debug.LogWarning("videoAdIdList is empty, using default ad id");
            return "f32bf9970ah83mq1kg"; // 默认广告ID，防止空字符串
        }
        int index = UnityEngine.Random.Range(0, this.videoAdIdList.Count);
        return this.videoAdIdList[index];
    }

    // 随机取一个插屏广告位ID
    private string GetRandScreenAdId()
    {
        if (this.interAdIdList.Count == 0)
        {
            Debug.LogWarning("interAdIdList is empty, using default ad id");
            return "dk217ga5e5b48uo4gh"; // 默认广告ID，防止空字符串
        }
        int index = UnityEngine.Random.Range(0, this.interAdIdList.Count);
        return this.interAdIdList[index];
    }

    // 限制 0.5 秒内广告只能触发一次
    private bool CheckTimeLimit()
    {
        if (Time.time - lastAdClickTime < 0.5f)
        {
            Debug.LogWarning("广告点击过于频繁，请稍后再试。");
            return false;
        }
        lastAdClickTime = Time.time;
        return true;
    }
}
