using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//#if TickTok
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;

/*
 * 抖音平台广告
 */
public class TikTokPlatform : IAdPlatform
{
    /// <summary>
    /// 是否向第三方（ecpm.xxhygames.com）上报 openid/机型等数据做 ECPM 与转化回传。
    /// 默认关闭：上面报会把用户信息发到外部服务器，抖音审核和隐私政策里必须申报，
    /// 确认是自己的后台、且已在上架材料里写明后再改成 true。
    /// </summary>
    public static bool EnableRemoteTracking = false;

    // 小游戏ID
    private readonly string miniAppid;

    // 激励视频
    private TTRewardedVideoAd rewardedVideoAd = null;
    private string videoAdId = "";
    private Action onRewardSuccess;
    private Action<string> onRewardFail;
    private float prevTimeScale = 1f;

    // 插屏广告
    private TTInterstitialAd interAdIns;
    private string interAdId = "";
    private float moduleStartTime;
    private float lastShowTime = -999f;

    // 上次插屏创建/加载失败的时间，用于冷却，避免每次都重新创建导致疯狂刷错误日志
    private float interFailedTime = -999f;

    // 当前实例开始 load 的时间（<=0 表示没有正在进行的加载），用于加载超时判定
    private float interLoadStartTime = -1f;

    // 已请求展示、但还在等「加载完成」或保护期结束的插屏
    private bool interPendingShow;
    private Coroutine interRetryRoutine;

    // 抖音插屏频控（官方文档：启动后 30 秒内不展示；两次插屏间隔 60 秒；插屏与激励视频间隔 60 秒）
    private const float InterstitialStartupGuard = 30f;
    private const float InterstitialInterval = 60f;
    private const float InterstitialRewardedGap = 60f;

    // 需要展示但跳过 / 失败后的冷却时间，防止玩家连点触发一串创建请求
    private const float InterstitialFailureCooldown = 60f;

    // 创建后 10 秒还没回调 OnLoad，按加载失败处理，避免 pending 永远挂着
    private const float InterstitialLoadTimeout = 10f;

    /// <summary>激励视频（成功/失败/关闭）结束时间，插屏与激励视频之间要间隔 60 秒</summary>
    private float lastRewardedCloseTime = -999f;

    // 激励视频是否正在播放，防止玩家连点导致一次点击发两份奖励 / 回调错乱
    private bool rewardedPlaying;
    private float rewardedStartTime;

    // 埋点
    private readonly string ecpmURL = "https://ecpm.xxhygames.com/openid/openlog/insertlog";  // ECPM后台地址
    private readonly string conversionURL = "https://analytics.oceanengine.com/api/v2/conversion";   // 巨量引擎API接口

    private TTSystemInfo systemInfo;

    public TikTokPlatform(string appId, string videoAdId, string interAdId)
    {
        this.miniAppid = appId ?? "";
        this.videoAdId = videoAdId;
        this.interAdId = interAdId;
        if (string.IsNullOrEmpty(this.miniAppid))
        {
            // AppID 只用于 ECPM/转化回传，广告本身按广告位 ID 播放，不影响收益流程。
            Debug.Log("[抖音广告] 未填写小游戏 AppID，广告可正常播放，仅埋点上报不可用");
        }
        else if (EnableRemoteTracking && !this.miniAppid.EndsWith("07"))
        {
            // 只有开启回传时才需要 AppID 格式校验，否则这条警告纯属噪音
            Debug.LogWarning($"[抖音广告] AppID「{this.miniAppid}」格式可能不匹配回传接口，广告播放不受影响");
        }

        if (EnableRemoteTracking)
        {
            try
            {
                this.Login();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[抖音广告] 埋点初始化失败（不影响广告）：{e.Message}");
            }
        }

        // 编辑器里 TT 接口是 mock，可能直接抛异常；真机上按正常流程创建。
        // 注意：插屏**不在这里预创建**。构造函数跑在 TT.InitSDK 的回调之前，
        // 这时候调用 tt.createInterstitialAd 会直接返回 createInterstitialAd:fail（errMsg 为空），
        // 之后每次进设置都会重新失败一次。插屏改成「第一次需要展示时才创建」，那时 SDK 早已初始化完成。
        try
        {
            this.CreateRewardedVideoAd();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[抖音广告] 初始化广告实例失败（真机上会自动重试）：{e.Message}");
        }

        moduleStartTime = Time.realtimeSinceStartup;
        lastShowTime = -999f;

        // 版本标记：看到这行才说明跑的是新包（旧包会在启动时就 createInterstitialAd，日志里是「插屏广告异常 ： 0」）
        Debug.Log("[广告] 插屏策略 v2：延迟创建 / 启动保护30s / 间隔60s / 失败冷却60s（2026-11-23）");
    }


    #region 插屏广告

    /// <summary>
    /// 插屏广告频控（以抖音官方文档为准，SDK 注释里的 15s/30s 是旧值）：
    /// 1. 小游戏启动后前 30 秒不能展示插屏；
    /// 2. 已经展示过一次插屏后，第二次展示需要距离上一次 60 秒；
    /// 3. 展示过激励视频后，插屏需要与激励视频间隔 60 秒。
    /// 插屏实例只支持展示一次，展示成功或加载失败后实例会被平台自动销毁，下次展示需重新创建。
    /// </summary>
    public override void ShowInterstitialAd(string AdId)
    {
        Debug.Log("播放插屏广告");

        // 已购买去广告的玩家不再弹插屏（AdManager 层也会拦一道，这里兜底）
        if (PlayerPrefs.HasKey("NoAds"))
        {
            Debug.Log("[抖音广告] 已购买去广告，跳过插屏");
            return;
        }

        // 广告位变化时重建实例，避免继续使用旧广告位
        if (!string.IsNullOrEmpty(AdId) && this.interAdId != AdId)
        {
            this.interAdId = AdId;
            this.RemoveInterstitialAd();
        }

        // 编辑器里 TT 插屏是 mock：Show() 不会真的弹，OnClose 也不会回调。
        // 这里只做一次「假广告」，让开发者能在 Console/静音表现上确认流程被触发了。
        if (Application.isEditor)
        {
            this.PlayEditorMockInterstitial();
            return;
        }

        float now = Time.realtimeSinceStartup;

        // 1.启动保护期：前 30 秒不能展示。
        // 排队等保护期结束再弹，而不是直接丢掉——否则刚进游戏就点设置永远看不到插屏。
        float startupLeft = InterstitialStartupGuard - (now - moduleStartTime);
        if (startupLeft > 0f)
        {
            Debug.LogWarning($"小游戏启动 {InterstitialStartupGuard:F0} 秒内不能展示插屏，{startupLeft:F1} 秒后自动展示");
            this.QueueInterstitialShow(startupLeft);
            return;
        }

        // 2.两次插屏间隔 60 秒，这个窗口内的请求直接丢弃，避免玩家反复点设置被连续打扰
        float intervalLeft = InterstitialInterval - (now - lastShowTime);
        if (intervalLeft > 0f)
        {
            Debug.LogWarning($"两次插屏广告展示间隔需大于 {InterstitialInterval:F0} 秒，剩余 {intervalLeft:F1} 秒，本次跳过");
            return;
        }

        // 3.和激励视频的间隔同样是 60 秒
        float rewardedLeft = InterstitialRewardedGap - (now - lastRewardedCloseTime);
        if (rewardedLeft > 0f)
        {
            Debug.LogWarning($"距上次激励视频不足 {InterstitialRewardedGap:F0} 秒，剩余 {rewardedLeft:F1} 秒，本次插屏跳过");
            return;
        }

        // 4.刚失败过就先冷却，别连着创建实例把错误刷满控制台（也容易被平台限频）
        float cooldownLeft = InterstitialFailureCooldown - (now - interFailedTime);
        if (cooldownLeft > 0f)
        {
            Debug.LogWarning($"[抖音广告] 插屏刚创建失败，{cooldownLeft:F0} 秒后再试");
            return;
        }

        // 标记「有展示需求」，实例没建好 / 还没加载完时由 OnLoad 回调补上
        this.interPendingShow = true;

        if (interAdIns == null)
        {
            this.CreateInterstitialAd();
            return;
        }

        this.TryShowLoadedInterstitial();
    }

    /// <summary>
    /// 有展示需求时，等广告加载完再弹。加载没完成就保留 pending，由 OnLoad 再调一次。
    /// </summary>
    private void TryShowLoadedInterstitial()
    {
        if (!this.interPendingShow || this.interAdIns == null)
        {
            return;
        }

        float now = Time.realtimeSinceStartup;

        // 加载迟迟不回调：超时后按失败处理，避免 pending 永远挂着、下次请求还复用坏实例
        if (this.interLoadStartTime > 0f && now - this.interLoadStartTime > InterstitialLoadTimeout)
        {
            this.OnInterstitialFailed(-1, $"加载超时（{InterstitialLoadTimeout:F0} 秒未回调 onLoad）");
            return;
        }

        bool loaded = false;
        try
        {
            loaded = this.interAdIns.IsLoaded();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[抖音广告] 查询插屏状态异常：{e.Message}");
        }

        if (!loaded)
        {
            Debug.LogWarning("插屏广告尚未加载完成，加载完成后自动展示");
            return;
        }

        float intervalLeft = InterstitialInterval - (now - this.lastShowTime);
        if (intervalLeft > 0f)
        {
            this.interPendingShow = false;
            Debug.LogWarning($"两次插屏广告展示间隔需大于 {InterstitialInterval:F0} 秒，剩余 {intervalLeft:F1} 秒，本次跳过");
            return;
        }

        float rewardedLeft = InterstitialRewardedGap - (now - this.lastRewardedCloseTime);
        if (rewardedLeft > 0f)
        {
            this.interPendingShow = false;
            Debug.LogWarning($"距上次激励视频不足 {InterstitialRewardedGap:F0} 秒，剩余 {rewardedLeft:F1} 秒，本次插屏跳过");
            return;
        }

        var ad = this.interAdIns;
        this.interPendingShow = false;
        this.interLoadStartTime = -1f;
        try
        {
            // TTSDK 没有 audio_starting / audio_finished 事件，静音由我们自己按「开始播 / 关闭」控制
            AdAudioMute.Mute();
            ad.Show();
            this.lastShowTime = now;
            Debug.Log("[抖音广告] 插屏广告已展示");
        }
        catch (Exception e)
        {
            AdAudioMute.Unmute();
            this.OnInterstitialFailed(-1, $"展示插屏异常：{e.Message}");
        }
    }

    /// <summary>
    /// 把展示请求延后到启动保护期结束（只排一个，避免连点排一串）。
    /// </summary>
    private void QueueInterstitialShow(float delay)
    {
        if (this.interRetryRoutine != null)
        {
            return;
        }

        var host = AdManager.Instance;
        if (host == null)
        {
            Debug.LogWarning("[抖音广告] 场景里没有 AdManager，无法排队展示插屏");
            return;
        }

        this.interRetryRoutine = host.StartCoroutine(this.InterstitialShowAfter(delay));
    }

    private IEnumerator InterstitialShowAfter(float delay)
    {
        yield return new WaitForSecondsRealtime(delay + 0.1f);
        this.interRetryRoutine = null;
        this.ShowInterstitialAd(this.interAdId);
    }

    /// <summary>
    /// 编辑器专用：模拟一次插屏展示（静音 2 秒后恢复），方便验证「打开设置 → 弹插屏」的流程。
    /// </summary>
    private void PlayEditorMockInterstitial()
    {
        var host = AdManager.Instance;
        if (host == null)
        {
            Debug.Log("[抖音广告-编辑器] 场景里没有 AdManager，插屏流程未走到");
            return;
        }

        host.StartCoroutine(this.EditorMockInterstitialRoutine());
    }

    private IEnumerator EditorMockInterstitialRoutine()
    {
        AdAudioMute.Mute();
        Debug.Log("[抖音广告-编辑器] 插屏广告展示中……（模拟 2 秒，真机/抖音开发者工具会弹出真实广告）");
        yield return new WaitForSecondsRealtime(2f);
        AdAudioMute.Unmute();
        this.lastShowTime = Time.realtimeSinceStartup;
        Debug.Log("[抖音广告-编辑器] 插屏广告已关闭（模拟）");
    }

    /// <summary>
    /// 创建插屏实例并开始加载。首次展示时才调用（不要放在构造函数里，那时 TT.InitSDK 还没回调，
    /// tt.createInterstitialAd 会直接返回 createInterstitialAd:fail）。
    /// </summary>
    private void CreateInterstitialAd()
    {
        // 旧实例先清掉（正常流程里它已经被平台自动销毁，这里只是兜底）
        this.RemoveInterstitialAd();

        TTInterstitialAd ad;
        try
        {
            ad = TT.CreateInterstitialAd(new CreateInterstitialAdParam { InterstitialAdId = this.interAdId });
        }
        catch (Exception e)
        {
            this.OnInterstitialFailed(-1, $"创建插屏异常：{e.Message}");
            return;
        }

        if (ad == null)
        {
            this.OnInterstitialFailed(-1, "创建插屏失败（SDK 返回 null，可能宿主不支持插屏）");
            return;
        }

        this.interAdIns = ad;
        this.interLoadStartTime = Time.realtimeSinceStartup;

        ad.OnClose += () =>
        {
            Debug.Log("插屏广告关闭");
            // 0.恢复游戏音效（对应老版本 Heyzap 的 audio_finished）
            AdAudioMute.Unmute();
            this.interPendingShow = false;
            this.interLoadStartTime = -1f;
            // 1.展示成功后实例由平台自动销毁，这里只清引用，下次展示再新建
            if (this.interAdIns == ad)
            {
                this.RemoveInterstitialAd();
            }
            // 2.发送埋点
            this.SendGameAddiction();
        };
        ad.OnLoad += () =>
        {
            Debug.Log("插屏广告加载完成");
            // 打开设置时发起过展示请求，但当时广告还在加载：这里补上
            if (this.interAdIns == ad)
            {
                this.interLoadStartTime = -1f;
                this.TryShowLoadedInterstitial();
            }
        };
        ad.OnError += (code, message) => this.OnInterstitialFailed(code, message);

        try
        {
            ad.Load();
        }
        catch (Exception e)
        {
            this.OnInterstitialFailed(-1, $"加载插屏异常：{e.Message}");
        }
    }

    /// <summary>
    /// 插屏创建 / 加载 / 展示失败的统一出口：清状态 + 记冷却 + 把错误码翻译成人话。
    /// 这里**不主动 Destroy**：官方文档写明「加载错误或展示成功后实例会自动销毁」，
    /// 自己再 Destroy 会和 SDK 里还没结束的 load（Promise）打架，控制台会多一条
    /// `Uncaught (in promise) API_ERROR: InterstitialAd.load:fail`。
    /// </summary>
    private void OnInterstitialFailed(int code, string message)
    {
        this.interAdIns = null;
        this.interPendingShow = false;
        this.interLoadStartTime = -1f;
        this.interFailedTime = Time.realtimeSinceStartup;
        AdAudioMute.Unmute();

        Debug.LogWarning($"[抖音广告] 插屏广告失败：code={code}，{DescribeInterstitialError(code, message)}" +
                         $"（{InterstitialFailureCooldown:F0} 秒内不再尝试创建）");
    }

    /// <summary>
    /// 插屏错误码翻译，参考
    /// https://developer.open-douyin.com/docs/resource/zh-CN/mini-game/develop/open-capacity/ads/interstitial-ad/interstitial-ad-on-error
    /// </summary>
    private static string DescribeInterstitialError(int code, string message)
    {
        switch (code)
        {
            case 158902:
                return $"广告位不可用（不存在/已关闭/被封禁，或不是「插屏」类型的广告位）：{message}";
            case 158987:
                return $"当前宿主不支持插屏广告（开发者工具、PC 版抖音常见，请用真机验证）：{message}";
            case 2001:
                return $"小游戏启动 30 秒内不允许展示插屏：{message}";
            case 2002:
                return $"距上次插屏/激励视频不足 60 秒：{message}";
            case 2003:
                return $"当前正在播放其它广告：{message}";
            case 2004:
                return $"广告渲染失败（非开发者异常，通常是页面切换导致）：{message}";
            case 2005:
                return $"插屏实例不允许跨页面调用：{message}";
            default:
                // createInterstitialAd:fail 且 errMsg 为空、code=0 的典型场景：
                // 宿主（开发者工具/PC 版）没有插屏能力，或广告位与该 appid 不匹配。
                return string.IsNullOrEmpty(message)
                    ? "错误信息为空（code=0 时多为宿主不支持插屏，或广告位与该 appid 不匹配）"
                    : message;
        }
    }

    private void RemoveInterstitialAd()
    {
        var ad = this.interAdIns;
        this.interAdIns = null;
        this.interLoadStartTime = -1f;
        try
        {
            ad?.Destroy();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[抖音广告] 销毁插屏实例异常：{e.Message}");
        }
    }

    #endregion


    #region 侧边栏

    public override void NavigateToScene(Action onSuccess, Action<string> onFail)
    {
        var data = new JsonData { ["scene"] = "sidebar" };
        TT.NavigateToScene(data,
            () =>
            {
                Debug.Log("侧边栏唤起成功");
                onSuccess?.Invoke();
            },
            () => { },
            (errCode, errMsg) =>
            {
                onFail?.Invoke(errMsg);
                Debug.LogError($"侧边栏显示异常:{errCode}, {errMsg}");
            });
    }

    #endregion

    #region 激励视频
    public override void ShowRewardedVideoAd(string videoAdId, Action onSuccess, Action<string> onFail)
    {
        // 已经在播就直接拒绝：否则会覆盖 OnClose 里的奖励回调，出现「看完了没发奖」
        if (this.rewardedPlaying)
        {
            if (Time.realtimeSinceStartup - this.rewardedStartTime < 300f)
            {
                Debug.LogWarning("[抖音广告] 激励视频正在播放中，忽略重复请求");
                onFail?.Invoke("广告正在播放中");
                return;
            }

            // 上一次的回调彻底丢了（抖音偶发），先复位再重新播，否则之后再也不能看广告
            Debug.LogWarning("[抖音广告] 上一次激励视频 300 秒未收到关闭回调，强制复位");
            this.RestoreTimeScale();
        }

        Debug.Log("播放激励视频广告");
        this.rewardedPlaying = true;
        this.rewardedStartTime = Time.realtimeSinceStartup;
        this.prevTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0; // 看广告需要暂停游戏

        // 广告位发生变化时必须重建实例，否则会继续播旧的广告位
        bool needRecreate = rewardedVideoAd == null || this.videoAdId != videoAdId;
        this.videoAdId = videoAdId;
        onRewardSuccess = onSuccess;
        onRewardFail = onFail;

        if (needRecreate)
        {
            this.DelRewardedVideoAd();
            try
            {
                this.CreateRewardedVideoAd();
            }
            catch (Exception e)
            {
                Debug.LogError($"[抖音广告] 创建激励视频异常：{e.Message}");
            }
        }

        if (rewardedVideoAd == null)
        {
            // 创建失败必须恢复暂停，否则游戏会一直卡在 timeScale=0
            var fail = onRewardFail;
            this.RestoreTimeScale();
            fail?.Invoke("激励视频创建失败");
            return;
        }

        try
        {
            // TTSDK 没有音频事件，广告真正开始播之前主动静音游戏音效（广告自身的声音不受影响）
            AdAudioMute.Mute();
            rewardedVideoAd.Show();
            this.StartRewardedWatchdog();
        }
        catch (Exception e)
        {
            Debug.LogError($"[抖音广告] 展示激励视频异常：{e.Message}");
            var fail = onRewardFail;
            this.RestoreTimeScale();
            fail?.Invoke("激励视频展示失败");
        }
    }

    /// <summary>
    /// 抖音偶发「广告关闭了但 OnClose 没回调」，会让 timeScale 一直停在 0（游戏卡死）。
    /// 这里挂一个兜底：超过 300 秒仍未收到回调就强制恢复。
    /// </summary>
    private void StartRewardedWatchdog()
    {
        var host = AdManager.Instance;
        if (host == null)
        {
            return;
        }

        host.StartCoroutine(this.RewardedWatchdog());
    }

    private IEnumerator RewardedWatchdog()
    {
        float started = Time.realtimeSinceStartup;
        while (this.rewardedPlaying && Time.realtimeSinceStartup - started < 300f)
        {
            yield return null;
        }

        if (this.rewardedPlaying)
        {
            Debug.LogWarning("[抖音广告] 激励视频 300 秒仍未收到关闭回调，强制恢复游戏");
            this.RestoreTimeScale();
        }
    }

    private void RestoreTimeScale()
    {
        onRewardSuccess = null;
        onRewardFail = null;
        this.rewardedPlaying = false;
        Time.timeScale = prevTimeScale <= 0f ? 1f : prevTimeScale;
        // 与 timeScale 一起恢复，成功 / 未看完 / 报错三条路径都会走到这里
        AdAudioMute.Unmute();
        // 记下激励视频结束时间：抖音要求激励视频之后至少 60 秒才能展示插屏
        this.lastRewardedCloseTime = Time.realtimeSinceStartup;
    }

    private void CreateRewardedVideoAd()
    {
        rewardedVideoAd = TT.CreateRewardedVideoAd(new CreateRewardedVideoAdParam() { AdUnitId = videoAdId });
        rewardedVideoAd.OnClose += (ended, count) =>
        {

            Debug.Log($"是否看完广告:{ended} 次数:{count}");
            var success = onRewardSuccess;
            var fail = onRewardFail;
            this.RestoreTimeScale();
            if (ended)
            {
                Debug.Log("[抖音广告] 激励视频看完，开始发放奖励");
                success?.Invoke();
                this.SendGameAddiction();
            }
            else
            {
                Debug.Log("广告未看完，不发放奖励");
                fail?.Invoke("广告未看完");
            }

            // 关闭广告后,创建新的实例（失败也不影响下次点击，下次会自动重建）
            this.DelRewardedVideoAd();
            try { this.CreateRewardedVideoAd(); }
            catch (Exception e) { Debug.LogWarning($"[抖音广告] 预创建激励视频失败：{e.Message}"); }
        };
        rewardedVideoAd.OnError += (errorCode, errorMessage) =>
        {
            this.DelRewardedVideoAd();
            var fail = onRewardFail;
            this.RestoreTimeScale();
            Debug.LogError($"[抖音广告] 激励视频失败：code={errorCode}，{DescribeRewardedError(errorCode, errorMessage)}");
            // 给玩家看的文案不带英文技术细节：1002 是后台广告位配置问题，玩家看不懂也处理不了
            fail?.Invoke(errorCode == 1004 ? "暂时没有合适的广告，请稍后再试" : "广告暂时无法播放，请稍后再试");
            // 不在这里重建实例，避免广告位无效时反复创建；下次点击时 needRecreate 会自动重建
        };
    }

    /// <summary>
    /// 激励视频错误码翻译，参考
    /// https://developer.open-douyin.com/docs/resource/zh-CN/mini-game/develop/open-capacity/ads/ads-error-code-description
    /// </summary>
    private static string DescribeRewardedError(int code, string message)
    {
        switch (code)
        {
            case 1002:
                return $"广告位不存在于当前小游戏，或广告位类型不匹配" +
                       $"（去开放平台该 appid 下的「广告位管理」核对 ID/类型，小程序与小游戏的广告位不通用）：{message}";
            case 1004:
                return $"当前无合适广告（填充不足，正常现象，不要当 bug 修）：{message}";
            default:
                return string.IsNullOrEmpty(message) ? "错误信息为空" : message;
        }
    }

    private void DelRewardedVideoAd()
    {
        rewardedVideoAd?.Destroy();
        rewardedVideoAd = null;
    }

    #endregion


    #region 埋点

    private void Login()
    {
        // 1.获取系统信息
        systemInfo = TT.GetSystemInfo();
        if (systemInfo?.system == null)
            return;

        // 2.登录-获取临时登录凭证 
        TT.Login(this.onLoginSuccess, (errMsg) =>
        {
            Debug.LogError("登录失败:" + errMsg);
        }, true);
        // 3.初始化埋点
        this.InitMaidian();
    }

    private void onLoginSuccess(string code, string anonymousCode, bool isLogin)
    {
        // 2.1获取用户的基本信息
        TT.GetUserInfo((ref TTUserInfo scUserInfo) => this.onGetUserInfo(ref scUserInfo, code, anonymousCode),
            (msg) =>
            {
                Debug.LogError($"用户信息获取失败: {msg}");
            });
    }

    private void onGetUserInfo(ref TTUserInfo scUserInfo, string code, string anonymousCode)
    {
        // 2.2向ECPM后台上传设备信息
        Debug.Log(scUserInfo);
        AdManager.Instance.StartCoroutine(this.SendOpenIdLog(
            miniAppid, scUserInfo.nickName, code, anonymousCode, systemInfo, (success, res) =>
            {
                if (success && res.d != null && res.d.user != null && !string.IsNullOrEmpty(res.d.user.openid))
                {
                    string useropenid = res.d.user.openid;
                    Debug.Log("openidlog==succ== " + res);
                    string currentDate = DateTime.UtcNow.ToString("o");
                    PlayerPrefs.SetString(miniAppid + TT.GetSystemInfo().model, currentDate);
                }
                else
                {
                    Debug.Log("openidlog==fail== " + res);
                }
            }));
    }

    private IEnumerator SendOpenIdLog(string appid, string nickName, string code, string anonymousCode, TTSystemInfo sysinfores, Action<bool, OpenIdResponse> callback)
    {
        var query = $"?appid={Uri.EscapeDataString(appid)}" +
                    $"&nickName={Uri.EscapeDataString(nickName)}" +
                    $"&code={Uri.EscapeDataString(code)}" +
                    $"&anonymousCode={Uri.EscapeDataString(anonymousCode)}" +
                    $"&system={Uri.EscapeDataString(sysinfores.system)}" +
                    $"&platform={Uri.EscapeDataString(sysinfores.platform)}" +
                    $"&brand={Uri.EscapeDataString(sysinfores.brand)}" +
                    $"&modelx={Uri.EscapeDataString(sysinfores.model)}" +
                    $"&version={Uri.EscapeDataString(sysinfores.ttVersion)}" +
                    $"&SDKVersion={Uri.EscapeDataString(sysinfores.sdkVersion)}" +
                    $"&appName={Uri.EscapeDataString(sysinfores.hostName)}";

        var www = UnityWebRequest.Get(this.ecpmURL + query);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("openidlog==fail== " + www.error);
            callback(false, null);
        }
        else
        {
            try
            {
                var response = JsonUtility.FromJson<OpenIdResponse>(www.downloadHandler.text);
                callback(true, response);
            }
            catch (Exception ex)
            {
                Debug.LogError("SendOpenIdLog ERROR：" + ex.Message);
                callback(false, null);
            }
        }
    }

    private void InitMaidian()
    {
        // 3.1获取小游戏启动时的参数
        var launchOptions = TT.GetLaunchOptionsSync();
        Debug.Log($"小游戏启动参数: {launchOptions}");
        if (launchOptions == null || launchOptions.Query == null || !launchOptions.Query.ContainsKey("ad_params"))
        {
            Debug.LogWarning($"小游戏:{miniAppid}没添加埋点参数");
            return;
        }

        string decodedString = Uri.UnescapeDataString(launchOptions.Query["ad_params"]);
        AdParamsData jsonval = JsonUtility.FromJson<AdParamsData>(decodedString);
        string clickid = jsonval?.log_extra?.clickid;
        if (string.IsNullOrEmpty(clickid))
        {
            Debug.LogWarning($"小游戏:{miniAppid}没点击事件埋点");
            return;
        }

        Debug.Log($"clickid: {clickid}");
        PlayerPrefs.SetString(miniAppid + "ClickId", clickid);
        if (!CanClaimReward(miniAppid + "Active"))
        {
            Debug.LogWarning($"小游戏:{miniAppid}奖励已领取，无法重复领取");
            return;
        }

        // 3.2 API上报激活数据
        long timenow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        AdManager.Instance.StartCoroutine(SendConversionRequest(
            "active", clickid, timenow,
            (success, response) =>
            {
                Debug.Log($"激活埋点事件==={(success ? "success" : "fail")}");
                Debug.Log($"激活结果={response}");
            }
        ));
        string currentDate = DateTime.UtcNow.ToString("o");
        PlayerPrefs.SetString(miniAppid + "Active", currentDate);
    }

    /*是否允许领取奖励*/
    private bool CanClaimReward(string key)
    {
        string lastClaimDateStr = PlayerPrefs.GetString(miniAppid + key);
        if (string.IsNullOrEmpty(lastClaimDateStr))
        {
            return true;
        }

        if (!DateTime.TryParse(lastClaimDateStr, out DateTime lastClaimDate))
        {
            return true;
        }
        DateTime oneMonthAgo = DateTime.Now.AddMonths(-1);
        return lastClaimDate < oneMonthAgo;
    }

    /// <summary>
    /// API上报关键行为
    /// </summary>
    private void SendGameAddiction()
    {
        string clickid = PlayerPrefs.GetString(miniAppid + "ClickId");
        if (string.IsNullOrEmpty(clickid))
        {
            Debug.LogWarning($"小游戏:{miniAppid}没点击事件埋点");
            return;
        }

        long timenow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        AdManager.Instance.StartCoroutine(SendConversionRequest(
                "game_addiction", clickid, timenow,
                (success, response) =>
                {
                    Debug.Log($"埋点事件===={(success ? "success" : "fail")}");
                    Debug.Log($"关键行为结果={response}");
                }
            ));
        Debug.Log("已触发激活埋点");
    }

    /// <summary>
    /// 转化回传 API
    /// </summary>
    /// <param name="eventType">回传的事件：</param>
    /// <param name="clickid">clickid 参数</param>
    /// <param name="timestamp">事件发生的毫秒级时间戳</param>
    /// <param name="callback"></param>
    /// https://event-manager.oceanengine.com/docs/8650/app_api_docs
    /// <returns></returns>
    public IEnumerator SendConversionRequest(string eventType, string clickid, long timestamp, Action<bool, string> callback)
    {
        var data = new ConversionData
        {
            event_type = eventType,
            context = new Context { ad = new Ad { callback = clickid } },
            timestamp = timestamp
        };

        string json = JsonUtility.ToJson(data);
        byte[] postData = System.Text.Encoding.UTF8.GetBytes(json);

        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("Content-Type", "application/json");

        using var www = new UnityWebRequest(conversionURL, "POST");
        www.uploadHandler = new UploadHandlerRaw(postData);
        www.downloadHandler = new DownloadHandlerBuffer();
        foreach (var header in headers)
        {
            www.SetRequestHeader(header.Key, header.Value);
        }
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            callback(false, www.error);
        }
        else
        {
            callback(true, www.downloadHandler.text);
        }
    }
    public override void ShareGame(Action<bool> callBack)
    {
        // 通用分享
        JsonData shareJson = new JsonData();
        shareJson["channel"] = "video";
        shareJson["title"] = "Some Title";
        shareJson["extra"] = new JsonData();
        shareJson["extra"]["videoPath"] = "/xxx/xxx.mp4";//录屏分享的话，路径是 OnRecordComplete 拿到的路径
        JsonData videoTopics = new JsonData();
        videoTopics.SetJsonType(JsonType.Array);
        videoTopics.Add("Some Topic1");
        videoTopics.Add("Some Topic2");
        shareJson["extra"]["videoTopics"] = videoTopics;
        shareJson["extra"]["hashtag_list"] = videoTopics;

        Debug.Log($"ShareAppMessageBtnClicked jsonData: {videoTopics.ToJson()}");
        TT.ShareAppMessage(videoTopics, (data) =>
        {
            callBack?.Invoke(true);
            Debug.Log($"ShareAppMessage success: {videoTopics.ToJson()}");//
        },
            (errMsg) =>
            {
                callBack?.Invoke(false);
                Debug.Log($"ShareAppMessage failed: {errMsg}");
            },
            () =>
            {
                callBack?.Invoke(false);
                Debug.Log($"ShareAppMessage cancel");
            });
    }

    #endregion
}

//#endif