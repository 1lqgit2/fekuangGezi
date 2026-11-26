using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hen : BaseSceneManager<Hen>
{
	[SerializeField]
	private SkeletonAnimation body;

	[SerializeField]
	private Transform beakTarget;

	[SerializeField]
	private List<ParticleSystem> trailParticles;

	[SerializeField]
	private List<RenderTexture> henPhotos;

	[SerializeField]
	private Camera trailCamera;

	[SerializeField]
	private GameObject fog;

	[SerializeField]
	private AudioSource AK47Sound;

	[SerializeField]
	private AudioSource glockSound;

	[SerializeField]
	private AudioSource shotgunSound;

	[SerializeField]
	private List<AudioSource> coughSounds;

	[SerializeField]
	private AudioSource failSound;

	[SerializeField]
	private ParticleSystem badPshik;

	public Vector3 correction;

	public ParticleSystem dust;

	private int trailCounter;

	private Coroutine trailCoroutine;

	private void Start()
	{
		body.AnimationState.Event += HandleEvent;
	}

	private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == "couch")
		{
			coughSounds[Random.Range(0, coughSounds.Count)].Play();
		}
		else if (e.Data.Name == "fail")
		{
			failSound.Play();
		}
	}

	private void Update()
	{
		if (BaseSceneManager<Gameplay>.Instance.IsPaused() || !(base.transform.parent != null))
		{
			return;
		}
		if (base.transform.localPosition != Vector3.zero)
		{
			float num = Time.deltaTime * 5f;
			if (base.transform.localPosition.magnitude < num)
			{
				base.transform.localPosition = Vector3.zero;
			}
			else
			{
				base.transform.localPosition -= base.transform.localPosition.normalized * num;
			}
		}
		Vector3 eulerAngles = base.transform.localRotation.eulerAngles;
		float num2 = Mathf.DeltaAngle(eulerAngles.z, 0f);
		if (num2 != 0f)
		{
			float num3 = Time.deltaTime * 180f;
			if (Mathf.Abs(num2) < num3)
			{
				base.transform.localRotation = Quaternion.identity;
				return;
			}
			Transform transform = base.transform;
			Vector3 eulerAngles2 = base.transform.localRotation.eulerAngles;
			transform.localRotation = Quaternion.Euler(0f, 0f, eulerAngles2.z + Mathf.Sign(num2) * num3);
		}
	}

	public void Attack()
	{
		if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_Rifle)
		{
			body.AnimationState.SetAnimation(0, "run_ak47_attack", loop: false);
			AK47Sound.Play();
		}
		else if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_Pistol)
		{
			body.AnimationState.SetAnimation(0, "run_glock_attack", loop: false);
			glockSound.Play();
		}
		else if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_ShotGun)
		{
			body.AnimationState.SetAnimation(0, "run_shotgun_attack", loop: false);
			shotgunSound.Play();
		}
		else if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_Knife)
		{
			body.AnimationState.SetAnimation(1, "run_knife_attack", loop: false);
			body.AnimationState.AddAnimation(1, GetRunAnimationString(), loop: true, 0f);
		}
		else
		{
			body.AnimationState.SetAnimation(1, "jump", loop: false);
			body.AnimationState.AddAnimation(1, GetRunAnimationString(), loop: true, 0f);
		}
		body.Update(0f);
	}

	public void TakeWorn()
	{
		if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_Rifle)
		{
			body.AnimationState.SetAnimation(0, "run_ak47_attack", loop: false);
			AK47Sound.Play();
		}
		else if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_Pistol)
		{
			body.AnimationState.SetAnimation(0, "run_glock_attack", loop: false);
			glockSound.Play();
		}
		else if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_ShotGun)
		{
			body.AnimationState.SetAnimation(0, "run_shotgun_attack", loop: false);
			shotgunSound.Play();
		}
		else if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_Knife)
		{
			body.AnimationState.SetAnimation(1, "run_knife_attack", loop: false);
			body.AnimationState.AddAnimation(1, GetRunAnimationString(), loop: true, 0f);
		}
		else
		{
			body.AnimationState.SetAnimation(1, "jump_empty", loop: false);
			body.AnimationState.AddAnimation(1, GetRunAnimationString(), loop: true, 0f);
		}
		body.Update(0f);
	}

	public void FailFly()
	{
		if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_Rifle)
		{
			AK47Sound.Play();
			body.AnimationState.SetAnimation(1, "run_ak47_fail", loop: false);
		}
		else if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_Pistol)
		{
			glockSound.Play();
			body.AnimationState.SetAnimation(1, "run_glock_fail", loop: false);
		}
		else if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_ShotGun)
		{
			shotgunSound.Play();
			body.AnimationState.SetAnimation(1, "run_shotgun_fail", loop: false);
		}
		else if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_Knife)
		{
			body.AnimationState.SetAnimation(1, "run_knife_fail", loop: false);
		}
		else
		{
			body.AnimationState.SetAnimation(1, "jump_fly", loop: false);
		}
		body.Update(0f);
	}

	public void Run()
	{
		body.AnimationState.SetAnimation(1, GetRunAnimationString(), loop: true);
		body.Update(0f);
	}

	public void Fail()
	{
		if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_Rifle)
		{
			body.AnimationState.SetAnimation(1, "run_ak47_fail", loop: false);
			AK47Sound.Play();
		}
		else if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_Pistol)
		{
			body.AnimationState.SetAnimation(1, "run_glock_fail", loop: false);
			glockSound.Play();
		}
		else if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_ShotGun)
		{
			body.AnimationState.SetAnimation(1, "run_shotgun_fail", loop: false);
			shotgunSound.Play();
		}
		else if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_Knife)
		{
			body.AnimationState.SetAnimation(1, "run_knife_fail", loop: false);
		}
		else
		{
			body.AnimationState.SetAnimation(1, "jump_fail", loop: false);
		}
		body.Update(0f);
	}

	public void SetSkin(string skinName)
	{
		body.Skeleton.SetSkin(skinName);
	}

	public void SetTrailActive(bool active)
	{
		trailCamera.gameObject.SetActive(active);
		if (active)
		{
			trailCoroutine = StartCoroutine(TrailCoroutine());
			BaseGameManager<MusicManager>.GetInstance().SetPitch(3f);
			body.timeScale = 3f;
		}
		else
		{
			StopCoroutine(trailCoroutine);
			BaseGameManager<MusicManager>.GetInstance().SetPitch(1f);
			body.timeScale = 1f;
		}
	}

	private IEnumerator TrailCoroutine()
	{
		while (true)
		{
			trailCamera.targetTexture = henPhotos[trailCounter];
			trailParticles[trailCounter].Play();
			trailCounter++;
			if (trailCounter > trailParticles.Count - 1)
			{
				trailCounter = 0;
			}
			yield return new WaitForSecondsRealtime(0.05f);
		}
	}

	private string GetRunAnimationString()
	{
		if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_Rifle)
		{
			return "run_ak47";
		}
		if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_Pistol)
		{
			return "run_glock";
		}
		if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_ShotGun)
		{
			return "run_shotgun";
		}
		if (BaseGameManager<SkinManager>.GetInstance().GetCurrentWeaponType() == SkinManager.WeaponType.WT_Knife)
		{
			return "run_knife";
		}
		if (BaseSceneManager<Gameplay>.Instance.GetCurrentLevel().GetFruitType() == Level.FruitType.Watermelon)
		{
			return "run_water";
		}
		return "run";
	}

	public void SetFogEnabled(bool isOn)
	{
		fog.SetActive(isOn);
	}

	public void BadPshik()
	{
		badPshik.Play();
	}
}
