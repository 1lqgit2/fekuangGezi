using System.Collections.Generic;
using UnityEngine;

public class SkinManager : BaseGameManager<SkinManager>
{
	public enum WeaponType
	{
		WT_Beak,
		WT_Rifle,
		WT_Pistol,
		WT_ShotGun,
		WT_Knife
	}

	[SerializeField]
	private List<string> animationNames;

	[SerializeField]
	private List<int> prices;

	private int currentSkin;

	private List<bool> skinsUnlocked;

	public static int skinsCount = 22;

	public const int skinFor1Video = 2;

	public const int skinFor3Video = 12;

	public const int skinFor5Video = 4;

	public const int skinFor3_1Video = 14;

	private int watchesLeftFor1Video;

	private int watchesLeftFor3Video;

	private int watchesLeftFor5Video;

	private int watchesLeftFor3_1Video;

	private WeaponType weaponType;

	private void Awake()
	{
		LoadFromSaves();
	}

	public void LoadFromSaves()
	{
		currentSkin = (PlayerPrefs.HasKey("currentSkin") ? PlayerPrefs.GetInt("currentSkin") : 0);
		weaponType = GetWeaponType(currentSkin);
		skinsUnlocked = new List<bool>();
		skinsUnlocked.Add(item: true);
		for (int i = 1; i < skinsCount; i++)
		{
			bool item = PlayerPrefs.HasKey("skinUnlocked" + i.ToString()) && PlayerPrefs.GetInt("skinUnlocked" + i.ToString()) == 1;
			if (i == 18)
			{
				item = true;
			}
			skinsUnlocked.Add(item);
		}
		watchesLeftFor1Video = ((!PlayerPrefs.HasKey("watchesLeftFor1Video")) ? 1 : PlayerPrefs.GetInt("watchesLeftFor1Video"));
		watchesLeftFor3Video = ((!PlayerPrefs.HasKey("watchesLeftFor3Video")) ? 3 : PlayerPrefs.GetInt("watchesLeftFor3Video"));
		watchesLeftFor5Video = ((!PlayerPrefs.HasKey("watchesLeftFor5Video")) ? 5 : PlayerPrefs.GetInt("watchesLeftFor5Video"));
		watchesLeftFor3_1Video = ((!PlayerPrefs.HasKey("watchesLeftFor3_1Video")) ? 3 : PlayerPrefs.GetInt("watchesLeftFor3_1Video"));
	}

	public WeaponType GetWeaponType(int skinNumber)
	{
		if (animationNames[skinNumber] == "army_chick")
		{
			return WeaponType.WT_Rifle;
		}
		if (animationNames[skinNumber] == "army_chicken")
		{
			return WeaponType.WT_Knife;
		}
		if (animationNames[skinNumber] == "army_pigeon")
		{
			return WeaponType.WT_ShotGun;
		}
		if (animationNames[skinNumber] == "army_seagull")
		{
			return WeaponType.WT_Pistol;
		}
		return WeaponType.WT_Beak;
	}

	public WeaponType GetCurrentWeaponType()
	{
		return weaponType;
	}

	public string GetAnimationName(int skinNumber)
	{
		return animationNames[skinNumber];
	}

	public int GetCurrentSkin()
	{
		return currentSkin;
	}

	public bool IsSkinUnlocked(int n)
	{
		return skinsUnlocked[n];
	}

	public void UnlockSkin(int n)
	{
		skinsUnlocked[n] = true;
		for (int i = 1; i < skinsCount; i++)
		{
			PlayerPrefs.SetInt("skinUnlocked" + i.ToString(), skinsUnlocked[i] ? 1 : 0);
		}
		PlayerPrefs.Save();
		BaseGameManager<OnlineManager>.GetInstance().SaveToGP();
	}

	public string GetCurrentSkinName()
	{
		return animationNames[currentSkin];
	}

	public void SetCurrentSkin(int n)
	{
		currentSkin = n;
		weaponType = GetWeaponType(currentSkin);
		PlayerPrefs.SetInt("currentSkin", n);
		PlayerPrefs.Save();
	}

	public int GetSkinPrice(int n)
	{
		return prices[n];
	}

	public int GetUnlockedSkinsAmount()
	{
		int num = 0;
		foreach (bool item in skinsUnlocked)
		{
			if (item)
			{
				num++;
			}
		}
		return num;
	}

	public bool CanBuyBird()
	{
		for (int i = 0; i < skinsCount; i++)
		{
			if (!skinsUnlocked[i] && BaseGameManager<MainManager>.GetInstance().GetCoins() >= prices[i] && i != 2 && i != 4 && i != 14)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanBuyNewBird()
	{
		int num = PlayerPrefs.HasKey("couldBuyEver") ? PlayerPrefs.GetInt("couldBuyEver") : 0;
		int num2 = 0;
		for (int i = 0; i < skinsCount; i++)
		{
			if (!skinsUnlocked[i] && BaseGameManager<MainManager>.GetInstance().GetCoins() >= prices[i] && i != 2 && i != 4 && i != 14 && i > num2)
			{
				num2 = i;
			}
		}
		if (num2 <= num)
		{
			return false;
		}
		PlayerPrefs.SetInt("couldBuyEver", num2);
		return true;
	}

	public int GetVideosLeftForVideoSkin(int n)
	{
		switch (n)
		{
		case 2:
			return watchesLeftFor1Video;
		case 12:
			return watchesLeftFor3Video;
		case 4:
			return watchesLeftFor5Video;
		case 14:
			return watchesLeftFor3_1Video;
		default:
			return -1;
		}
	}

	public void VideoWatchedForVideoSkin(int n)
	{
		switch (n)
		{
		case 2:
			watchesLeftFor1Video--;
			PlayerPrefs.SetInt("watchesLeftFor1Video", watchesLeftFor1Video);
			if (watchesLeftFor1Video <= 0)
			{
				UnlockSkin(n);
			}
			break;
		case 12:
			watchesLeftFor3Video--;
			PlayerPrefs.SetInt("watchesLeftFor3Video", watchesLeftFor3Video);
			if (watchesLeftFor3Video <= 0)
			{
				UnlockSkin(n);
			}
			break;
		case 4:
			watchesLeftFor5Video--;
			PlayerPrefs.SetInt("watchesLeftFor5Video", watchesLeftFor5Video);
			if (watchesLeftFor5Video <= 0)
			{
				UnlockSkin(n);
			}
			break;
		case 14:
			watchesLeftFor3_1Video--;
			PlayerPrefs.SetInt("watchesLeftFor3_1Video", watchesLeftFor3_1Video);
			if (watchesLeftFor3_1Video <= 0)
			{
				UnlockSkin(n);
			}
			break;
		}
	}

	public int GetRandomUnlockedSkin()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < skinsCount; i++)
		{
			if (IsSkinUnlocked(i))
			{
				list.Add(i);
			}
		}
		return list[Random.Range(0, list.Count)];
	}
}
