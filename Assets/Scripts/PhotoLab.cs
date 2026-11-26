using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PhotoLab : MonoBehaviour
{
	[SerializeField]
	private Text levelCompleted;

	[SerializeField]
	private Text scoreText;

	[SerializeField]
	private SkeletonGraphic bird;

	[SerializeField]
	private List<GameObject> birdHolders;

	[SerializeField]
	private List<string> poses;

	[SerializeField]
	private List<SkeletonGraphic> starSkeletons;

	[SerializeField]
	private RenderTexture photo;

	[SerializeField]
	private Camera photoCamera;

	private const string photoName = "pigeon_photo.png";

	public void SharePhotoLevels(int level, int stars, string text)
	{
		bird.Initialize(overwrite: false);
		bird.Skeleton.SetSkin(BaseGameManager<SkinManager>.GetInstance().GetCurrentSkinName());
		bird.AnimationState.SetAnimation(0, poses[Random.Range(0, poses.Count)], loop: true);
		bird.Update(0f);
		for (int i = 0; i < 3; i++)
		{
			starSkeletons[i].Initialize(overwrite: false);
			if (i + 1 <= stars)
			{
				starSkeletons[i].AnimationState.SetAnimation(0, "white", loop: true);
			}
			else
			{
				starSkeletons[i].AnimationState.SetAnimation(0, "black", loop: true);
			}
			starSkeletons[i].Update(0f);
		}
		levelCompleted.text = "关卡：" + (level + 1).ToString();
		StartCoroutine(SharePhotoCoroutine(text));
	}

	public void SharePhotoInfinite(int score, string text, List<string> skinNames)
	{
		GameObject gameObject = birdHolders[skinNames.Count - 1];
		gameObject.SetActive(value: true);
		SkeletonGraphic[] componentsInChildren = gameObject.GetComponentsInChildren<SkeletonGraphic>();
		string animationName = poses[Random.Range(0, poses.Count)];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Initialize(overwrite: false);
			componentsInChildren[i].Skeleton.SetSkin(skinNames[i]);
			componentsInChildren[i].AnimationState.SetAnimation(0, animationName, loop: true);
			componentsInChildren[i].Update(0f);
		}
		scoreText.text = score.ToString();
		StartCoroutine(SharePhotoCoroutine(text));
	}

	private IEnumerator SharePhotoCoroutine(string text)
	{
		int width = photo.width;
		int height = photo.height;
		yield return new WaitForEndOfFrame();
		photoCamera.gameObject.SetActive(value: true);
		photoCamera.Render();
		photoCamera.gameObject.SetActive(value: false);
		Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, mipChain: false);
		RenderTexture.active = photo;
		texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
		texture.Apply();
		byte[] bytes = texture.EncodeToPNG();
		UnityEngine.Object.Destroy(texture);
		string photoPath = Application.persistentDataPath + "/pigeon_photo.png";
		File.WriteAllBytes(photoPath, bytes);
		UnityEngine.Debug.Log("Photo saved to: " + photoPath);
		SharePhotoSystem(text);
		base.gameObject.SetActive(value: false);
	}

	public void SharePhotoSystem(string text)
	{
		string photoPath = Application.persistentDataPath + "/pigeon_photo.png";
		Share(text, photoPath, "https://play.google.com/store/apps/details?id=com.fortafygames.pigeonpop", string.Empty);
	}

	private void Share(string shareText, string photoPath, string url, string subject = "")
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("android.content.Intent");
		AndroidJavaObject androidJavaObject = new AndroidJavaObject("android.content.Intent");
		androidJavaObject.Call<AndroidJavaObject>("setAction", new object[1]
		{
			androidJavaClass.GetStatic<string>("ACTION_SEND")
		});
		AndroidJavaClass androidJavaClass2 = new AndroidJavaClass("android.net.Uri");
		AndroidJavaObject androidJavaObject2 = androidJavaClass2.CallStatic<AndroidJavaObject>("parse", new object[1]
		{
			"file://" + photoPath
		});
		androidJavaObject.Call<AndroidJavaObject>("putExtra", new object[2]
		{
			androidJavaClass.GetStatic<string>("EXTRA_STREAM"),
			androidJavaObject2
		});
		androidJavaObject.Call<AndroidJavaObject>("setType", new object[1]
		{
			"image/png"
		});
		androidJavaObject.Call<AndroidJavaObject>("putExtra", new object[2]
		{
			androidJavaClass.GetStatic<string>("EXTRA_TEXT"),
			shareText
		});
		AndroidJavaClass androidJavaClass3 = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject @static = androidJavaClass3.GetStatic<AndroidJavaObject>("currentActivity");
		AndroidJavaObject androidJavaObject3 = androidJavaClass.CallStatic<AndroidJavaObject>("createChooser", new object[2]
		{
			androidJavaObject,
			subject
		});
		@static.Call("startActivity", androidJavaObject3);
	}
}
