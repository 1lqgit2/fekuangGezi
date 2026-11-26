using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/// <summary>
/// 在线管理：每日奖励校时 + 存档工具。
///
/// 原来这里还接了 Google Play Games（登录 / 云存档 / 成就 / 排行榜），
/// 项目现在只保留抖音小游戏 SDK，GPGS 已整体移除，相关入口保留成空方法，
/// 这样 Gameplay / Skins / StartScreen 等调用点不用改。
/// 以后要在抖音侧做排行榜 / 成就，换成 TTSDK 的开放数据域接口即可。
/// </summary>
public class OnlineManager : BaseGameManager<OnlineManager>
{
	private double currentTimeDB;

	private DateTime lastTimeConnectedServer;

	private double lastTimeRewardedDB;

	private int seriesLengthDB;

	// 授时源必须走 https：
	// 1) Unity 2022 默认 insecureHttpOption=NotAllowed，http:// 会直接抛
	//    InvalidOperationException: Insecure connection not allowed（编辑器与真机都一样）；
	// 2) WebGL 页面本身是 https，明文请求还会被浏览器按混合内容拦掉。
	//
	// 抖音小游戏（WebGL + TTSDK）另有一条硬限制：无论页面是不是 https，
	// tt.request 只能访问「抖音开放平台后台配置的 request 合法域名」，
	// 未配置的域名一律返回 errNo 21100（request:fail url not in domain list）。
	// 而合法域名需要通过后台的归属校验（上传校验文件），timeapi.io / acs.m.taobao.com
	// 都不是我们的域名，永远加不进去，所以在抖音构建里直接不请求它们。
	private const string TimeApiUrl = "https://timeapi.io/api/Time/current/zone?timeZone=UTC";

	private const string TaobaoTimeUrl = "https://acs.m.taobao.com/gw/mtop.common.getTimestamp/";

	// 想用自建授时接口（抖音云托管 / 自己的服务器）时，把完整 HTTPS 地址填在这里，
	// 并把它加到抖音后台「开发设置 -> 服务器域名 -> request 合法域名」。
	// 留空表示抖音包里只用本机时间（带防回拨）。
	private const string CustomTimeServerUrl = "";

	// 本机时间兜底时用来防回拨的最近已知时间（Unix 秒）。
	private const string LastKnownTimeKey = "timeSyncLastKnown";

	// 兜底时间多久落盘一次（秒）。currentTimeDB 是逐帧递增的，没必要每帧写 PlayerPrefs。
	private const float LastKnownTimeSaveInterval = 60f;

	private static string[] BuildTimeServerUrls()
	{
		List<string> list = new List<string>();
#if UNITY_WEBGL && !UNITY_EDITOR
		// 抖音小游戏：默认一个外部域都不请求，避免每次启动都刷 21100 错误。
		if (!string.IsNullOrEmpty(CustomTimeServerUrl))
		{
			list.Add(CustomTimeServerUrl);
		}
#else
		// 编辑器 / 原生平台：直连公共授时源，用来验证每日奖励的 24 小时逻辑。
		list.Add(TimeApiUrl);
		list.Add(TaobaoTimeUrl);
#endif
		return list.ToArray();
	}

	private static readonly string[] TimeServerUrls = BuildTimeServerUrls();

	private const float RetryIntervalOnSuccess = 900f;

	private const float RetryIntervalOnFailure = 30f;

	private const float RequestTimeoutSeconds = 10f;

	private bool timeRequestSucceeded;

	private long timeRequestValue;

	private float totalTimePlayed;

	// 上一次把兜底时间写进 PlayerPrefs 的时刻（realtime），用来限流。
	private float lastKnownTimeSavedAt;

	public bool Authenticating => false;

	public bool Authenticated => false;

	private void Awake()
	{
		InitDailyBonus();
		totalTimePlayed = (PlayerPrefs.HasKey("userTimePlayed") ? PlayerPrefs.GetFloat("userTimePlayed") : 0f);
	}

	private void Update()
	{
		UpdateDailyBonus();
		if (Mathf.Floor(totalTimePlayed + Time.deltaTime) > Mathf.Floor(totalTimePlayed))
		{
			PlayerPrefs.SetFloat("userTimePlayed", totalTimePlayed);
		}
		totalTimePlayed += Time.deltaTime;
	}

	// ---------------------------------------------------------------------
	// 下面这些方法原来是把数据同步到 Google Play Games（云存档 / 成就 / 排行榜）。
	// 项目现在只保留抖音小游戏 SDK，这些能力已经移除，这里保留同名空实现，
	// 免得改动 Gameplay / Feeding / Skins / StartScreen 里的调用点。
	// ---------------------------------------------------------------------

	public void SaveToGP()
	{
		// 原：上传存档到 Play Games 云端。现在进度只保存在 PlayerPrefs。
	}

	public void Authenticate()
	{
		// 抖音小游戏没有 Google Play 登录，保留空实现避免调用点报错。
	}

	public void LoadFromCloud()
	{
	}

	public void SignOut()
	{
	}

	public void ShowLeaderboardUI()
	{
	}

	public void ShowAchievementsUI()
	{
	}

	public void LevelPassedAchievement(int level)
	{
	}

	public void BirdUnlockedAchievement()
	{
	}

	public void GameStartedAchievement()
	{
		// 这个计数原来用于上报成就，留着方便以后接抖音侧统计
		int gamesStarted = (PlayerPrefs.HasKey("gamesStarted") ? PlayerPrefs.GetInt("gamesStarted") : 0);
		PlayerPrefs.SetInt("gamesStarted", gamesStarted + 1);
	}

	public void ReportStars(int stars)
	{
	}

	public void ReportFeedingScore(int score)
	{
	}

	public void ShowAchievements()
	{
	}

	public void ShowLeaderboards()
	{
	}

	// ---------------------------------------------------------------------
	// 存档序列化：PlayerPrefs <-> byte[]。
	// 以前用于 GPGS 云存档，现在留作本地/后续抖音云端存档的格式基础。
	// ---------------------------------------------------------------------

	private byte[] PlayerprefsToBinary()
	{
		string stringForPrefInt = GetStringForPrefInt("coins");
		stringForPrefInt = stringForPrefInt + "|" + GetStringForPrefInt("currentLevelNumber");
		stringForPrefInt = stringForPrefInt + "|" + GetStringForPrefInt("lastLevelUnlocked");
		stringForPrefInt = stringForPrefInt + "|" + GetStringForPrefString("stars");
		stringForPrefInt = stringForPrefInt + "|" + GetStringForPrefInt("currentSkin");
		stringForPrefInt = stringForPrefInt + "|" + GetStringForPrefInt("watchesLeftFor1Video");
		stringForPrefInt = stringForPrefInt + "|" + GetStringForPrefInt("watchesLeftFor5Video");
		for (int i = 0; i < SkinManager.skinsCount; i++)
		{
			stringForPrefInt = stringForPrefInt + "|" + GetStringForPrefInt("skinUnlocked" + i.ToString());
		}
		stringForPrefInt = stringForPrefInt + "|" + GetStringForPrefInt("NoAds");
		stringForPrefInt = stringForPrefInt + "|" + GetStringForPrefInt("feedingScore");
		return Encoding.Default.GetBytes(stringForPrefInt);
	}

	private string GetStringForPrefInt(string key)
	{
		if (PlayerPrefs.HasKey(key))
		{
			return "int," + key + "," + PlayerPrefs.GetInt(key).ToString();
		}
		return "0,0,0";
	}

	private string GetStringForPrefString(string key)
	{
		if (PlayerPrefs.HasKey(key))
		{
			return "string," + key + "," + PlayerPrefs.GetString(key);
		}
		return "0,0,0";
	}

	private void BinaryToPlayerPrefs(byte[] data)
	{
		string text = Encoding.Default.GetString(data);
		string[] array = text.Split('|');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(',');
			if (array2.Length < 3)
			{
				continue;
			}
			switch (array2[0])
			{
				case "int":
					PlayerPrefs.SetInt(array2[1], int.Parse(array2[2]));
					break;
				case "string":
					PlayerPrefs.SetString(array2[1], array2[2]);
					break;
				case "float":
					PlayerPrefs.SetFloat(array2[1], float.Parse(array2[2]));
					break;
			}
		}
		PlayerPrefs.Save();
	}

	private void InitDailyBonus()
	{
		lastTimeRewardedDB = ((!PlayerPrefs.HasKey("lastTimeRewardedDB")) ? 0f : ((float)PlayerPrefs.GetInt("lastTimeRewardedDB")));
		seriesLengthDB = (PlayerPrefs.HasKey("seriesLengthDB") ? PlayerPrefs.GetInt("seriesLengthDB") : 0);
		StartCoroutine(GetTimeCoroutine());
	}

	private IEnumerator GetTimeCoroutine()
	{
		string[] urls = TimeServerUrls;
		bool loggedFallback = false;

		while (true)
		{
			lastTimeConnectedServer = DateTime.Now;
			bool success = false;

			for (int i = 0; i < urls.Length; i++)
			{
				yield return RequestTimeFrom(urls[i]);
				if (timeRequestSucceeded)
				{
					currentTimeDB = timeRequestValue;
					success = true;
					SaveLastKnownTime((long)currentTimeDB);
					UnityEngine.Debug.Log("[校时] 服务器时间: " + currentTimeDB.ToString("F0"));
					break;
				}
			}

			if (!success)
			{
				// 没有可用授时源时回落到本机 UTC 时间。
				// 这一步是必要的：currentTimeDB 只要停留在 0，DailyBonusScenario 就会一直卡在
				// while (GetTimeTillBonus() == -1) 的加载转圈上，玩家永远领不到每日奖励。
				currentTimeDB = GetMonotonicLocalTime();
				if (!loggedFallback)
				{
					// 只在状态变化时打一次，避免抖音里每 30 秒刷一条一样的日志。
					loggedFallback = true;
					UnityEngine.Debug.Log((urls.Length == 0)
						? "[校时] 当前平台不请求外部授时源，使用本机时间（带防回拨）: " + currentTimeDB.ToString("F0")
						: "[校时] 授时源不可用，使用本机时间（带防回拨）: " + currentTimeDB.ToString("F0"));
				}
			}
			else
			{
				loggedFallback = false;
			}

			// 一个外部源都不请求时（抖音包默认情况），重试没有意义，直接按成功间隔走。
			float interval = ((success || urls.Length == 0) ? RetryIntervalOnSuccess : RetryIntervalOnFailure);
			yield return new WaitUntil(() => (DateTime.Now - lastTimeConnectedServer).TotalSeconds > (double)interval);
		}
	}

	/// <summary>
	/// 本机时间兜底：取设备 UTC 时间，并与上次记录的最大值取大。
	/// 玩家把系统时间调回过去时，直接用上次的时间继续走，避免靠改时钟反复刷每日奖励。
	/// </summary>
	private double GetMonotonicLocalTime()
	{
		long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		string raw = PlayerPrefs.GetString(LastKnownTimeKey, string.Empty);
		long lastKnown;
		if (!string.IsNullOrEmpty(raw) && long.TryParse(raw, out lastKnown) && lastKnown > now)
		{
			now = lastKnown;
		}
		SaveLastKnownTime(now);
		PlayerPrefs.Save();
		return now;
	}

	private static void SaveLastKnownTime(long unixSeconds)
	{
		if (unixSeconds <= 0L)
		{
			return;
		}
		PlayerPrefs.SetString(LastKnownTimeKey, unixSeconds.ToString());
	}

	/// <summary>把兜底时间定期落盘（读广告/切后台不会丢进度），不必每帧写。</summary>
	private void SaveLastKnownTimeIfDue()
	{
		if (currentTimeDB <= 0.0)
		{
			return;
		}
		if (Time.realtimeSinceStartup - lastKnownTimeSavedAt < LastKnownTimeSaveInterval)
		{
			return;
		}
		lastKnownTimeSavedAt = Time.realtimeSinceStartup;
		SaveLastKnownTime((long)currentTimeDB);
		PlayerPrefs.Save();
	}

	private IEnumerator RequestTimeFrom(string url)
	{
		timeRequestSucceeded = false;
		timeRequestValue = 0L;

		UnityWebRequest request = UnityWebRequest.Get(url);
		request.timeout = 8;

		// SendWebRequest 在「协议不被允许」这类情况下会同步抛异常（例如 Unity 2022 默认
		// 禁止 http:// 明文请求，会抛 InvalidOperationException: Insecure connection not allowed）。
		// 协程里 yield 不能放进带 catch 的 try 块，所以这里把发起请求和处理结果拆开。
		UnityWebRequestAsyncOperation operation = null;
		try
		{
			operation = request.SendWebRequest();
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("[校时] 请求无法发起: " + url + " -> " + e.Message);
		}

		if (operation != null)
		{
			// WebGL 下 UnityWebRequest.timeout 不生效，这里用一个自己的等待上限兜底，
			// 避免请求永久挂起把整条校时协程卡死（那样 currentTimeDB 会一直是 0）。
			float deadline = Time.realtimeSinceStartup + RequestTimeoutSeconds;
			while (!operation.isDone && Time.realtimeSinceStartup < deadline)
			{
				yield return null;
			}
			if (!operation.isDone)
			{
				UnityEngine.Debug.LogWarning("[校时] 请求超时: " + url);
				request.Abort();
			}
		}

		if (operation != null && operation.isDone && request.result == UnityWebRequest.Result.Success && !string.IsNullOrEmpty(request.downloadHandler.text))
		{
			string body = request.downloadHandler.text;
			long unixSeconds;
			if (TryParseTimeApiUtc(body, out unixSeconds) || TryParseTaobaoTimestamp(body, out unixSeconds))
			{
				timeRequestValue = unixSeconds;
				timeRequestSucceeded = true;
			}
			else
			{
				UnityEngine.Debug.LogWarning("[校时] 返回内容无法解析: " + url + " body=" + body);
			}
		}
		else if (operation != null)
		{
			// 请求被 Abort 掉时 result 不一定可读，按 isDone 分开取信息，避免二次抛异常。
			string detail = (operation.isDone ? (request.result.ToString() + " " + request.error) : "aborted");
			UnityEngine.Debug.Log("[校时] 请求失败: " + url + " (" + detail + ")");
		}

		request.Dispose();
	}

	/// <summary>timeapi.io: {"year":2026,"month":11,"day":20,"hour":10,"minute":27,"seconds":23,...}（UTC）</summary>
	private static bool TryParseTimeApiUtc(string json, out long unixSeconds)
	{
		unixSeconds = 0L;
		if (string.IsNullOrEmpty(json))
		{
			return false;
		}
		int year;
		int month;
		int day;
		int hour;
		int minute;
		int second;
		// 注意：不能写成 && 串联，短路时后面的 out 变量不会被赋值，编译器会报 CS0165。
		if (!TryGetJsonInt(json, "year", out year))
		{
			return false;
		}
		if (!TryGetJsonInt(json, "month", out month))
		{
			return false;
		}
		if (!TryGetJsonInt(json, "day", out day))
		{
			return false;
		}
		if (!TryGetJsonInt(json, "hour", out hour))
		{
			return false;
		}
		if (!TryGetJsonInt(json, "minute", out minute))
		{
			return false;
		}
		if (!TryGetJsonInt(json, "seconds", out second))
		{
			return false;
		}
		if (year < 2000 || year > 2200 || month < 1 || month > 12 || day < 1 || day > 31 || hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 60)
		{
			return false;
		}
		try
		{
			unixSeconds = new DateTimeOffset(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc)).ToUnixTimeSeconds();
		}
		catch (ArgumentOutOfRangeException)
		{
			return false;
		}
		return true;
	}

	/// <summary>淘宝: {"api":"mtop.common.getTimestamp",...,"data":{"t":"1795170442423"}}（毫秒）</summary>
	private static bool TryParseTaobaoTimestamp(string json, out long unixSeconds)
	{
		unixSeconds = 0L;
		if (string.IsNullOrEmpty(json) || json.IndexOf("mtop.common.getTimestamp", StringComparison.Ordinal) < 0)
		{
			return false;
		}
		string raw;
		long milliseconds;
		if (!TryGetJsonString(json, "t", out raw) || !long.TryParse(raw, out milliseconds) || milliseconds <= 0)
		{
			return false;
		}
		unixSeconds = milliseconds / 1000L;
		return true;
	}

	private static bool TryGetJsonInt(string json, string key, out int value)
	{
		value = 0;
		int start;
		int end;
		if (!TryFindJsonValue(json, key, out start, out end))
		{
			return false;
		}
		return int.TryParse(json.Substring(start, end - start), out value);
	}

	private static bool TryGetJsonString(string json, string key, out string value)
	{
		value = null;
		int start;
		int end;
		if (!TryFindJsonValue(json, key, out start, out end))
		{
			return false;
		}
		string token = json.Substring(start, end - start);
		if (token.Length >= 2 && token[0] == '"' && token[token.Length - 1] == '"')
		{
			token = token.Substring(1, token.Length - 2);
		}
		value = token;
		return true;
	}

	/// <summary>定位 "key": value 中 value 的区间（引号字符串含引号，数字/裸值到 , } ] 或空白为止）。</summary>
	private static bool TryFindJsonValue(string json, string key, out int start, out int end)
	{
		start = 0;
		end = 0;
		int keyIndex = json.IndexOf("\"" + key + "\"", StringComparison.Ordinal);
		if (keyIndex < 0)
		{
			return false;
		}
		int colon = json.IndexOf(':', keyIndex + key.Length + 2);
		if (colon < 0)
		{
			return false;
		}
		int i = colon + 1;
		while (i < json.Length && char.IsWhiteSpace(json[i]))
		{
			i++;
		}
		if (i >= json.Length)
		{
			return false;
		}
		if (json[i] == '"')
		{
			int close = json.IndexOf('"', i + 1);
			if (close < 0)
			{
				return false;
			}
			start = i;
			end = close + 1;
			return true;
		}
		int stop = i;
		while (stop < json.Length && json[stop] != ',' && json[stop] != '}' && json[stop] != ']' && !char.IsWhiteSpace(json[stop]))
		{
			stop++;
		}
		if (stop <= i)
		{
			return false;
		}
		start = i;
		end = stop;
		return true;
	}

	private void UpdateDailyBonus()
	{
		if (currentTimeDB != 0.0)
		{
			currentTimeDB += Time.deltaTime;
			SaveLastKnownTimeIfDue();
		}
	}

	public int GetTimeTillBonus()
	{
		if (currentTimeDB == 0.0)
		{
			return -1;
		}
		if (currentTimeDB - lastTimeRewardedDB < 86400.0)
		{
			return Mathf.RoundToInt((float)(86400.0 - (currentTimeDB - lastTimeRewardedDB)));
		}
		return 0;
	}

	public int GetRewardSize()
	{
		if (currentTimeDB - lastTimeRewardedDB < 86400.0)
		{
			return 0;
		}
		if (currentTimeDB - lastTimeRewardedDB < 172800.0)
		{
			return 10;
		}
		return 10;
	}

	public void ClaimBonus()
	{
		if (!(currentTimeDB - lastTimeRewardedDB < 86400.0))
		{
			if (currentTimeDB - lastTimeRewardedDB < 172800.0)
			{
				seriesLengthDB++;
			}
			else
			{
				seriesLengthDB = 0;
			}
			lastTimeRewardedDB = currentTimeDB;
			PlayerPrefs.SetInt("seriesLengthDB", seriesLengthDB);
			PlayerPrefs.SetInt("lastTimeRewardedDB", Mathf.RoundToInt((float)lastTimeRewardedDB));
			PlayerPrefs.Save();
		}
	}
}
