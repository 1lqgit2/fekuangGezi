using UnityEngine;

public class MusicManager : BaseGameManager<MusicManager>
{
	[SerializeField]
	private AudioSource music;

	[SerializeField]
	private AudioSource fogmusic;

	public void SetPitch(float value)
	{
		music.pitch = value;
		fogmusic.pitch = value;
	}

	public void LevelStarted()
	{
		if (BaseSceneManager<Gameplay>.Instance.DoesLevelHasFog(BaseSceneManager<Gameplay>.Instance.GetLevelNumber()))
		{
			if (!fogmusic.gameObject.activeInHierarchy)
			{
				music.gameObject.SetActive(value: false);
				fogmusic.gameObject.SetActive(value: true);
				fogmusic.Play();
			}
		}
		else if (!music.gameObject.activeInHierarchy)
		{
			fogmusic.gameObject.SetActive(value: false);
			music.gameObject.SetActive(value: true);
			music.Play();
		}
	}
}
