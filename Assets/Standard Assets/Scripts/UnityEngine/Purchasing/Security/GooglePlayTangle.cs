using System;

namespace UnityEngine.Purchasing.Security
{
	public class GooglePlayTangle
	{
		private static byte[] data = Convert.FromBase64String("laE3crr7kkrOpYD8+lXa4hBbN6At6TsuY7MqJOfrlQQ40tacvMtT90YniB9/LZq3LgUaiCXUlLfgBxN0YuHv4NBi4eriYuHh4D585sUBOS9vM42LQjIqalwmwbITq9wiL2pAxQjDpbRXunMOvPNQEBK94vYmOlrsxSaG9QoF8JC8wM5Sg3y+tjRWcmbVdNB4UjI8H9gFfVkw4HunjMGVEvHAGaJOt0g6vVLKlQ+zqoO4zKV9rkIX4D5oBDCVVdjy/eICk6EbFs7QYuHC0O3m6cpmqGYX7eHh4eXg45Su/+I/ynrvsOQBL+TiUns4CM1mvW9ZRJrn1IDSC+sp3bL+hi6vpln/wGyZb5a4fyxdz+6nFZXT35AFWEfeMlV12TGoz+Lj4eDh");

		private static int[] order = new int[15]
		{
			2,
			10,
			8,
			10,
			6,
			9,
			12,
			12,
			10,
			12,
			10,
			11,
			13,
			13,
			14
		};

		private static int key = 224;

		public static readonly bool IsPopulated = true;

		public static byte[] Data()
		{
			if (!IsPopulated)
			{
				return null;
			}
			return Obfuscator.DeObfuscate(data, order, key);
		}
	}
}
