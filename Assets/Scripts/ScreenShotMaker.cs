using System.Collections;
using System.IO;
using UnityEngine;

public class ScreenShotMaker : MonoBehaviour
{
	[SerializeField]
	private new Camera camera;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private RenderTexture iPhonePlus;

	[SerializeField]
	private RenderTexture iPadPro;

	private void Update()
	{
		if (UnityEngine.Input.GetKeyDown(KeyCode.C))
		{
			StartCoroutine(MakeScreenShots());
		}
	}

	private IEnumerator MakeScreenShots()
	{
		int j = 0;
		if (PlayerPrefs.HasKey("ScreenShotNumber"))
		{
			j = PlayerPrefs.GetInt("ScreenShotNumber");
		}
		j++;
		camera.targetTexture = iPhonePlus;
		yield return new WaitForEndOfFrame();
		Texture2D tex2 = new Texture2D(iPhonePlus.width, iPhonePlus.height, TextureFormat.RGB24, mipChain: false);
		RenderTexture.active = iPhonePlus;
		camera.Render();
		tex2.ReadPixels(new Rect(0f, 0f, iPhonePlus.width, iPhonePlus.height), 0, 0);
		tex2.Apply();
		byte[] bytes2 = tex2.EncodeToPNG();
		UnityEngine.Object.Destroy(tex2);
		string screenShotPath2 = Application.persistentDataPath + "/iPhonePlus_" + j.ToString() + ".png";
		File.WriteAllBytes(screenShotPath2, bytes2);
		camera.targetTexture = null;
		yield return new WaitForEndOfFrame();
		camera.targetTexture = iPadPro;
		canvas.SendMessage("Awake");
		canvas.SendMessage("Start");
		yield return new WaitForEndOfFrame();
		tex2 = new Texture2D(iPadPro.width, iPadPro.height, TextureFormat.RGB24, mipChain: false);
		RenderTexture.active = iPadPro;
		tex2.ReadPixels(new Rect(0f, 0f, iPadPro.width, iPadPro.height), 0, 0);
		tex2.Apply();
		bytes2 = tex2.EncodeToPNG();
		UnityEngine.Object.Destroy(tex2);
		screenShotPath2 = Application.persistentDataPath + "/iPadPro_" + j.ToString() + ".png";
		File.WriteAllBytes(screenShotPath2, bytes2);
		PlayerPrefs.SetInt("ScreenShotNumber", j);
		camera.targetTexture = null;
		UnityEngine.Debug.Log("Screenshots saved to: " + screenShotPath2);
	}
}
