using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : MonoBehaviour
{
	public enum SeedType
	{
		Good,
		Bad,
		Bomb,
		Laser,
		Respawn,
		Color,
		UFO,
		Freeze,
		Fire
	}

	public enum SeedState
	{
		Default,
		Eaten,
		Worm,
		Respawn,
		Hull
	}

	[SerializeField]
	private SpriteRenderer good;

	[SerializeField]
	private GameObject eatenGood;

	[SerializeField]
	private GameObject eatenGoodMiddle;

	[SerializeField]
	private SpriteRenderer bad;

	[SerializeField]
	private SpriteRenderer eatenBad;

	[SerializeField]
	private SpriteRenderer eatenBadMiddle;

	[SerializeField]
	private Transform goodPivot;

	[SerializeField]
	private SpriteRenderer background;

	[SerializeField]
	public SpriteRenderer detonator;

	[SerializeField]
	public SpriteRenderer bomb;

	[SerializeField]
	public SkeletonAnimation laser;

	[SerializeField]
	public SkeletonAnimation color;

	[SerializeField]
	public GameObject freeze;

	[SerializeField]
	public GameObject fire;

	[SerializeField]
	public SkeletonAnimation explosion;

	[SerializeField]
	protected ParticleSystem pshik;

	[SerializeField]
	private AudioSource chvak;

	[SerializeField]
	private AudioSource boom;

	[SerializeField]
	private AudioSource laserSound;

	[SerializeField]
	public SkeletonAnimation worm;

	[SerializeField]
	private ParticleSystem wormPshik;

	[SerializeField]
	private AudioSource wormChvak;

	[SerializeField]
	private List<AudioSource> wormHelloSounds;

	[SerializeField]
	private LineRenderer line;

	[SerializeField]
	private MeshFilter mesh;

	[SerializeField]
	private MeshFilter meshBad;

	[SerializeField]
	private MeshFilter meshEaten;

	[SerializeField]
	private MeshFilter meshColor;

	[SerializeField]
	public SkeletonAnimation UFO;

	[SerializeField]
	public LineRenderer lineRend;

	[SerializeField]
	public SpriteRenderer viemka;

	[SerializeField]
	public GameObject tutorArrow;

	private Coroutine respawnCoroutine;

	protected Coroutine wormCoroutine;

	private bool isGold;

	public bool notResp;

	private float meshSize;

	public bool isOnTopRadius = true;

	public SeedType seedType;

	public SeedState seedState;

	public int wormSize;

	public virtual void InitType(SeedType type)
	{
		seedType = type;
		if (lineRend == null)
		{
			UpdateVisual();
		}
	}

	public virtual void UpdateVisual()
	{
		good.gameObject.SetActive(value: false);
		eatenGood.SetActive(value: false);
		bad.gameObject.SetActive(value: false);
		eatenBad.gameObject.SetActive(value: false);
		worm.gameObject.SetActive(value: false);
		if ((bool)eatenGoodMiddle)
		{
			eatenGoodMiddle.gameObject.SetActive(value: false);
		}
		if ((bool)eatenBadMiddle)
		{
			eatenBadMiddle.gameObject.SetActive(value: false);
		}
		if ((bool)detonator)
		{
			detonator.gameObject.SetActive(value: false);
		}
		if ((bool)bomb)
		{
			bomb.gameObject.SetActive(value: false);
		}
		if ((bool)laser)
		{
			laser.gameObject.SetActive(value: false);
		}
		if ((bool)color)
		{
			color.gameObject.SetActive(value: false);
		}
		if ((bool)freeze)
		{
			freeze.gameObject.SetActive(value: false);
		}
		if ((bool)fire)
		{
			fire.gameObject.SetActive(value: false);
		}
		if (lineRend != null)
		{
			lineRend.SetPosition(0, new Vector3(0f, 0f, 0f));
			lineRend.SetPosition(1, new Vector3(0f, (0f - BaseSceneManager<Gameplay>.Instance.GetCurrentLevel().radius) * 1.3f, 0f));
		}
		if ((bool)mesh)
		{
			meshColor.gameObject.SetActive(value: false);
			mesh.gameObject.SetActive(value: false);
			line.gameObject.SetActive(value: false);
			meshEaten.gameObject.SetActive(value: false);
			meshBad.gameObject.SetActive(value: false);
		}
		if (seedType == SeedType.Good)
		{
			if (seedState == SeedState.Default)
			{
				if ((bool)mesh)
				{
					mesh.gameObject.SetActive(value: true);
					line.gameObject.SetActive(value: false);
				}
				else
				{
					good.gameObject.SetActive(value: true);
				}
			}
			else
			{
				if (seedState != SeedState.Eaten)
				{
					return;
				}
				if ((bool)mesh)
				{
					meshEaten.gameObject.SetActive(value: true);
					viemka.gameObject.SetActive(value: false);
				}
				else if (isOnTopRadius)
				{
					eatenGood.gameObject.SetActive(value: true);
					if ((bool)eatenGoodMiddle)
					{
						eatenGoodMiddle.gameObject.SetActive(value: false);
					}
				}
				else
				{
					eatenGood.gameObject.SetActive(value: false);
					if ((bool)eatenGoodMiddle)
					{
						eatenGoodMiddle.gameObject.SetActive(value: true);
					}
				}
				if (lineRend != null)
				{
					lineRend.gameObject.SetActive(value: false);
				}
			}
		}
		else if (seedType == SeedType.Respawn)
		{
			if (seedState == SeedState.Default)
			{
				good.gameObject.SetActive(value: true);
			}
			else if (seedState == SeedState.Respawn)
			{
				eatenGood.gameObject.SetActive(value: true);
			}
			else if (seedState != SeedState.Eaten)
			{
			}
		}
		else if (seedType == SeedType.Bad)
		{
			if (seedState == SeedState.Default)
			{
				if ((bool)mesh)
				{
					mesh.gameObject.SetActive(value: false);
					line.gameObject.SetActive(value: false);
					meshBad.gameObject.SetActive(value: true);
				}
				else
				{
					bad.gameObject.SetActive(value: true);
				}
			}
			else
			{
				if (seedState != SeedState.Eaten)
				{
					return;
				}
				if ((bool)mesh)
				{
					mesh.gameObject.SetActive(value: false);
					line.gameObject.SetActive(value: false);
					viemka.gameObject.SetActive(value: false);
				}
				else if (isOnTopRadius)
				{
					eatenBad.gameObject.SetActive(value: true);
					if ((bool)eatenBadMiddle)
					{
						eatenBadMiddle.gameObject.SetActive(value: false);
					}
				}
				else
				{
					eatenBad.gameObject.SetActive(value: false);
					if ((bool)eatenBadMiddle)
					{
						eatenBadMiddle.gameObject.SetActive(value: true);
					}
				}
				if (lineRend != null)
				{
					lineRend.gameObject.SetActive(value: false);
				}
			}
		}
		else if (seedType == SeedType.Bomb)
		{
			if (seedState == SeedState.Default)
			{
				detonator.gameObject.SetActive(value: true);
				if ((bool)bomb)
				{
					bomb.gameObject.SetActive(value: true);
				}
				if ((bool)mesh)
				{
					mesh.gameObject.SetActive(value: true);
					line.gameObject.SetActive(value: false);
				}
				if (BaseSceneManager<Gameplay>.Instance.GetLevelNumber() < 4)
				{
					tutorArrow.SetActive(value: true);
				}
			}
			else if (seedState == SeedState.Eaten)
			{
				if (isOnTopRadius)
				{
					eatenGood.gameObject.SetActive(!notResp);
				}
				else
				{
					eatenGoodMiddle.gameObject.SetActive(!notResp);
				}
				if ((bool)mesh)
				{
					meshEaten.gameObject.SetActive(value: true);
					viemka.gameObject.SetActive(value: false);
				}
				if (lineRend != null)
				{
					lineRend.gameObject.SetActive(value: false);
				}
				if (BaseSceneManager<Gameplay>.Instance.GetLevelNumber() < 4)
				{
					tutorArrow.SetActive(value: false);
				}
			}
		}
		else if (seedType == SeedType.Laser)
		{
			if (seedState == SeedState.Default)
			{
				laser.gameObject.SetActive(value: true);
			}
			else if (seedState == SeedState.Eaten)
			{
				eatenGood.gameObject.SetActive(!notResp);
				if (lineRend != null)
				{
					lineRend.gameObject.SetActive(value: false);
				}
			}
		}
		else if (seedType == SeedType.Color)
		{
			if (seedState == SeedState.Default)
			{
				if ((bool)mesh)
				{
					meshColor.gameObject.SetActive(value: true);
				}
				else
				{
					color.gameObject.SetActive(value: true);
				}
			}
			else if (seedState == SeedState.Eaten)
			{
				if ((bool)mesh)
				{
					meshEaten.gameObject.SetActive(value: true);
					viemka.gameObject.SetActive(value: false);
				}
				else if (isOnTopRadius)
				{
					eatenGood.gameObject.SetActive(!notResp);
				}
				else
				{
					eatenGoodMiddle.gameObject.SetActive(!notResp);
				}
				if (lineRend != null)
				{
					lineRend.gameObject.SetActive(value: false);
				}
			}
		}
		else if (seedType == SeedType.UFO)
		{
			if (UFO != null)
			{
				UFO.gameObject.SetActive(value: true);
			}
			if (seedState != SeedState.Eaten)
			{
				return;
			}
			if (UFO != null)
			{
				UFO.gameObject.SetActive(value: false);
			}
			if (lineRend != null)
			{
				lineRend.gameObject.SetActive(value: false);
				if ((bool)viemka)
				{
					viemka.gameObject.SetActive(value: false);
				}
			}
		}
		else if (seedType == SeedType.Freeze)
		{
			if (seedState == SeedState.Default)
			{
				freeze.gameObject.SetActive(value: true);
			}
			else if (seedState == SeedState.Eaten)
			{
				eatenGood.gameObject.SetActive(!notResp);
				if (lineRend != null)
				{
					lineRend.gameObject.SetActive(value: false);
				}
			}
		}
		else
		{
			if (seedType != SeedType.Fire)
			{
				return;
			}
			if (seedState == SeedState.Default)
			{
				fire.gameObject.SetActive(value: true);
			}
			else if (seedState == SeedState.Eaten)
			{
				eatenGood.gameObject.SetActive(!notResp);
				if (lineRend != null)
				{
					lineRend.gameObject.SetActive(value: false);
				}
			}
		}
	}

	public void ConvertIn()
	{
		seedType = SeedType.Good;
		if (wormCoroutine != null)
		{
			StopCoroutine(wormCoroutine);
		}
		if (seedState == SeedState.Worm)
		{
			seedState = SeedState.Default;
		}
		UpdateVisual();
	}

	public void ConvertOut()
	{
		seedType = SeedType.Bad;
		UpdateVisual();
	}

	public void SetLayer(int layer)
	{
		good.sortingOrder = layer;
		if ((bool)eatenGood.GetComponent<SpriteRenderer>())
		{
			eatenGood.GetComponent<SpriteRenderer>().sortingOrder = layer;
		}
		else
		{
			eatenGood.GetComponent<MeshRenderer>().sortingOrder = layer;
		}
		bad.sortingOrder = layer;
		eatenBad.sortingOrder = layer;
		if (background != null)
		{
			background.sortingOrder = layer - 1;
		}
	}

	public void SetMeshPos(Mesh m, List<Vector3> positions)
	{
		Vector3[] vertices = m.vertices;
		vertices[0] = positions[0];
		vertices[1] = positions[2];
		vertices[2] = positions[1];
		vertices[3] = positions[3];
		m.vertices = vertices;
		m.RecalculateBounds();
	}

	public void SetColorGood(Color c)
	{
		mesh.GetComponent<MeshRenderer>().material.color = c;
	}

	public void SetColorBad(Color c)
	{
		meshBad.GetComponent<MeshRenderer>().material.color = c;
	}

	public void SetLinePositions(List<Vector3> positions, List<Vector3> positionsOverlay)
	{
		line.SetPositions(positions.ToArray());
		SetMeshPos(mesh.mesh, positions);
		SetMeshPos(meshEaten.mesh, positionsOverlay);
		SetMeshPos(meshBad.mesh, positions);
		SetMeshPos(meshColor.mesh, positions);
		meshSize = (positions[0] - positions[3]).magnitude / 2.8f;
	}

	public void SetLocalPosRot(Vector3 pos, Quaternion rot)
	{
		goodPivot.transform.localPosition = pos;
		goodPivot.transform.localRotation = rot;
	}

	public void WasEaten()
	{
		if (seedType == SeedType.Good)
		{
			seedState = SeedState.Eaten;
			pshik.Play();
			chvak.Play();
		}
		if (seedType == SeedType.Bad)
		{
			seedState = SeedState.Eaten;
			chvak.Play();
		}
		else if (seedType == SeedType.Bomb)
		{
			seedState = SeedState.Eaten;
			explosion.AnimationState.SetAnimation(0, "explode", loop: false);
			boom.Play();
		}
		else if (seedType == SeedType.Laser)
		{
			seedState = SeedState.Eaten;
			laserSound.Play();
		}
		else if (seedType == SeedType.Color)
		{
			seedState = SeedState.Eaten;
			pshik.Play();
			chvak.Play();
		}
		else if (seedType == SeedType.Respawn && seedState != SeedState.Respawn)
		{
			pshik.Play();
			chvak.Play();
			SkeletonAnimation component = eatenGood.GetComponent<SkeletonAnimation>();
			component.Initialize(overwrite: false);
			Spine.AnimationState animationState = component.AnimationState;
			animationState.SetAnimation(0, "respawn", loop: false);
			float num = BaseSceneManager<Gameplay>.Instance.GetCurrentLevel().GetTurnsForRespawn() * 360f / (Mathf.Abs(BaseSceneManager<Gameplay>.Instance.GetCurrentLevel().GetHenSpeed()) + Mathf.Abs(BaseSceneManager<Gameplay>.Instance.GetCurrentLevel().GetCornSpeed()));
			eatenGood.GetComponent<SkeletonAnimation>().timeScale = 5.83333f / num;
			respawnCoroutine = StartCoroutine(Respawn(num));
		}
		else if (seedType != SeedType.Respawn || seedState != SeedState.Respawn)
		{
			seedState = SeedState.Eaten;
		}
		UpdateVisual();
	}

	public void WasShoot()
	{
		if (seedType == SeedType.Good || seedType == SeedType.Respawn)
		{
			seedState = SeedState.Eaten;
			pshik.Play();
		}
		UpdateVisual();
	}

	public void StopRespawn()
	{
		if (respawnCoroutine != null)
		{
			StopCoroutine(respawnCoroutine);
		}
		seedType = SeedType.Respawn;
		seedState = SeedState.Eaten;
		UpdateVisual();
	}

	private IEnumerator Respawn(float t)
	{
		seedState = SeedState.Respawn;
		yield return new WaitForSeconds(t);
		seedState = SeedState.Default;
		BaseSceneManager<Gameplay>.Instance.GetCurrentLevel().SeedRespawned();
		UpdateVisual();
	}

	public virtual GameObject TakeSeed()
	{
		seedState = SeedState.Eaten;
		GameObject gameObject;
		if (seedType == SeedType.Bad)
		{
			gameObject = UnityEngine.Object.Instantiate(bad.gameObject, bad.transform.position, bad.transform.rotation);
			gameObject.transform.localScale = bad.transform.lossyScale;
		}
		else if (seedType == SeedType.Bomb)
		{
			gameObject = UnityEngine.Object.Instantiate(detonator.gameObject, detonator.transform.position, detonator.transform.rotation);
			gameObject.transform.localScale = detonator.transform.lossyScale;
		}
		else if (seedType == SeedType.Laser)
		{
			gameObject = UnityEngine.Object.Instantiate(laser.gameObject, laser.transform.position, laser.transform.rotation);
			gameObject.transform.localScale = laser.transform.lossyScale;
		}
		else if (seedType == SeedType.Color)
		{
			gameObject = UnityEngine.Object.Instantiate(color.gameObject, color.transform.position, color.transform.rotation);
			gameObject.transform.localScale = color.transform.lossyScale;
		}
		else if (seedType == SeedType.UFO)
		{
			gameObject = UnityEngine.Object.Instantiate(UFO.gameObject, UFO.transform.position, UFO.transform.rotation);
			gameObject.transform.localScale = UFO.transform.lossyScale;
		}
		else if (seedType == SeedType.Freeze)
		{
			gameObject = UnityEngine.Object.Instantiate(freeze.gameObject, freeze.transform.position, freeze.transform.rotation);
			gameObject.transform.localScale = freeze.transform.lossyScale;
		}
		else if (seedType == SeedType.Fire)
		{
			gameObject = UnityEngine.Object.Instantiate(fire.gameObject, fire.transform.position, fire.transform.rotation);
			gameObject.transform.localScale = fire.transform.lossyScale;
		}
		else
		{
			gameObject = UnityEngine.Object.Instantiate(good.gameObject, good.transform.position, good.transform.rotation);
		}
		UpdateVisual();
		return gameObject;
	}

	public virtual float HowIsPointIn(Vector3 point)
	{
		if ((bool)mesh)
		{
			return (base.transform.position - point).magnitude / meshSize;
		}
		float magnitude = (base.transform.position - point).magnitude;
		Vector3 lossyScale = base.transform.lossyScale;
		return magnitude / (lossyScale.x * GetRadius());
	}

	public virtual float GetRadius()
	{
		return 0.2f;
	}

	public virtual void WormGetOut()
	{
		wormCoroutine = StartCoroutine(WormCoroutine());
	}

	protected IEnumerator WormCoroutine()
	{
		if (BaseGameManager<MainManager>.GetInstance().GetCoins() == 0 && BaseSceneManager<Gameplay>.Instance.GetLevelNumber() == 1)
		{
			tutorArrow.SetActive(value: true);
		}
		isGold = BaseSceneManager<Gameplay>.Instance.IsWormGold();
		wormSize = ((!isGold) ? 1 : 5);
		seedState = SeedState.Worm;
		worm.gameObject.SetActive(value: true);
		worm.AnimationState.SetAnimation(0, "hole", loop: true);
		worm.Update(0f);
		yield return new WaitForSeconds(0.333f);
		if (!BaseSceneManager<Gameplay>.Instance.IsPaused() && seedType != 0)
		{
			wormHelloSounds[isGold ? 1 : 0].Play();
			worm.AnimationState.SetAnimation(0, ((!isGold) ? string.Empty : "gold_") + "appear", loop: false);
			worm.AnimationState.AddAnimation(0, ((!isGold) ? string.Empty : "gold_") + "idle", loop: true, 0f);
			yield return new WaitForSeconds(4f);
			worm.AnimationState.SetAnimation(0, ((!isGold) ? string.Empty : "gold_") + "hide", loop: false);
			yield return new WaitForSeconds(0.333f);
			if (seedState == SeedState.Worm)
			{
				seedState = SeedState.Default;
				worm.gameObject.SetActive(value: false);
			}
			if ((bool)tutorArrow && tutorArrow.activeInHierarchy)
			{
				tutorArrow.SetActive(value: false);
			}
		}
	}

	public void WormEaten()
	{
		seedState = SeedState.Default;
		worm.gameObject.SetActive(value: false);
		if (isGold)
		{
			pshik.Play();
		}
		else
		{
			wormPshik.Play();
		}
		wormChvak.Play();
		if ((bool)tutorArrow && tutorArrow.activeInHierarchy)
		{
			tutorArrow.SetActive(value: false);
		}
	}

	public SkeletonAnimation TakeUFO()
	{
		UFO.gameObject.SetActive(value: true);
		UFO.transform.parent = null;
		SkeletonAnimation uFO = UFO;
		UFO = null;
		return uFO;
	}

	public virtual bool IsOpened()
	{
		return true;
	}

	public virtual void KickHull()
	{
	}

	public virtual void Explode()
	{
	}
}
