using UnityEngine;

/// <summary>
/// 广告播放期间的全局静音开关。
///
/// 老版本用 Heyzap 时会收到 audio_starting / audio_finished 两个事件，在回调里把
/// AudioListener.volume 置 0 / 1。抖音 TTSDK 没有对应的音频事件，所以改成由广告
/// 播放流程自己控制：Show() 之前 Mute()，广告关闭或报错时 Unmute()。
///
/// 注意：TTSDK 的广告声音走原生层，不受 AudioListener.volume 影响，
/// 所以这里静音只会关掉游戏自己的音效，不会把广告声音一起关掉。
/// </summary>
public static class AdAudioMute
{
    private static bool muted;

    private static float savedVolume = 1f;

    private static float mutedAt;

    /// <summary>广告静音兜底时长：正常广告几十秒内必然回调 OnClose，超过这个时间说明回调丢了。</summary>
    public const float WatchdogSeconds = 600f;

    public static bool IsMuted
    {
        get { return muted; }
    }

    /// <summary>已静音时长（秒），用于 AdManager 的兜底恢复。</summary>
    public static float MutedSeconds
    {
        get { return muted ? Time.realtimeSinceStartup - mutedAt : 0f; }
    }

    public static void Mute()
    {
        if (muted)
        {
            // 连续弹广告时刷新兜底计时
            mutedAt = Time.realtimeSinceStartup;
            return;
        }

        savedVolume = AudioListener.volume;
        if (savedVolume <= 0f)
        {
            // 玩家自己把音量关了，恢复时按 1 处理，避免广告结束后游戏一直没声音
            savedVolume = 1f;
        }

        AudioListener.volume = 0f;
        muted = true;
        mutedAt = Time.realtimeSinceStartup;
        Debug.Log("[广告] 广告开始，已静音游戏音效");
    }

    public static void Unmute()
    {
        if (!muted)
        {
            return;
        }

        AudioListener.volume = savedVolume;
        muted = false;
        Debug.Log("[广告] 广告结束，已恢复游戏音效");
    }
}
