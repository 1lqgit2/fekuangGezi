using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class SplineMesh : MonoBehaviour
{
	public Spline spline;

	public Spline.UpdateMode uMode;

	public float deltaSeconds = 0.1f;

	public int deltaFrames = 2;

	public Mesh baseMesh;

	public int segmentCount = 100;

	public Vector2 xyScale = Vector2.one;

	public Vector2 uvScale = Vector2.one;

	public bool swapUV;

	public int splineSegment = -1;

	private Mesh bentMesh;

	private float passedTime;

	public Mesh BentMesh => bentMesh;

	private void Start()
	{
		if (!(spline == null))
		{
			spline.UpdateSplineNodes();
			UpdateMesh();
		}
	}

	private void OnEnable()
	{
		if (!(spline == null))
		{
			spline.UpdateSplineNodes();
			UpdateMesh();
		}
	}

	private void LateUpdate()
	{
		switch (uMode)
		{
		case Spline.UpdateMode.EveryFrame:
			UpdateMesh();
			break;
		case Spline.UpdateMode.EveryXFrames:
			if (deltaFrames <= 0)
			{
				deltaFrames = 1;
			}
			if (Time.frameCount % deltaFrames == 0)
			{
				UpdateMesh();
			}
			break;
		case Spline.UpdateMode.EveryXSeconds:
			passedTime += Time.deltaTime;
			if (passedTime >= deltaSeconds)
			{
				UpdateMesh();
				passedTime = 0f;
			}
			break;
		}
	}

	public void UpdateMesh()
	{
		Setup();
		if ((bool)BentMesh)
		{
			BentMesh.Clear();
		}
		if (baseMesh == null || spline == null || segmentCount <= 0)
		{
			return;
		}
		Vector3[] vertices = baseMesh.vertices;
		Vector3[] normals = baseMesh.normals;
		Vector4[] tangents = baseMesh.tangents;
		Vector2[] uv = baseMesh.uv;
		int[] triangles = baseMesh.triangles;
		Vector3[] array = new Vector3[vertices.Length * segmentCount];
		Vector3[] array2 = new Vector3[normals.Length * segmentCount];
		Vector4[] array3 = new Vector4[tangents.Length * segmentCount];
		Vector2[] array4 = new Vector2[uv.Length * segmentCount];
		int[] array5 = new int[triangles.Length * segmentCount];
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		Vector3 a = Vector3.zero;
		Vector3 a2 = Vector3.zero;
		for (int i = 0; i < vertices.Length; i++)
		{
			if (vertices[i].z > 0f)
			{
				list.Add(i);
				a += vertices[i];
			}
			else if (vertices[i].z < 0f)
			{
				list2.Add(i);
				a2 += vertices[i];
			}
		}
		a /= list.Count;
		a2 /= list2.Count;
		if (this.splineSegment >= 0 && this.splineSegment < spline.SegmentCount)
		{
			SplineSegment splineSegment = spline.SplineSegments[this.splineSegment];
			int vIndex = 0;
			for (int j = 0; j < segmentCount; j++)
			{
				float param = (float)j / (float)segmentCount;
				float num = (float)(j + 1) / (float)segmentCount;
				if (num == 1f)
				{
					num -= 1E-05f;
				}
				param = splineSegment.ConvertSegmentToSplineParamter(param);
				num = splineSegment.ConvertSegmentToSplineParamter(num);
				CalculateBentMesh(ref vIndex, list, list2, ref a, ref a2, param, num, vertices, normals, tangents, uv, array, array2, array3, array4);
				for (int k = 0; k < triangles.Length; k++)
				{
					array5[k + j * triangles.Length] = triangles[k] + vertices.Length * j;
				}
			}
			BentMesh.vertices = array;
			BentMesh.uv = array4;
			if (normals.Length > 0)
			{
				BentMesh.normals = array2;
			}
			if (tangents.Length > 0)
			{
				BentMesh.tangents = array3;
			}
			BentMesh.triangles = array5;
			return;
		}
		int vIndex2 = 0;
		for (int l = 0; l < segmentCount; l++)
		{
			float param2 = (float)l / (float)segmentCount;
			float num2 = (float)(l + 1) / (float)segmentCount;
			if (num2 == 1f)
			{
				num2 -= 1E-05f;
			}
			CalculateBentMesh(ref vIndex2, list, list2, ref a, ref a2, param2, num2, vertices, normals, tangents, uv, array, array2, array3, array4);
			for (int m = 0; m < triangles.Length; m++)
			{
				array5[m + l * triangles.Length] = triangles[m] + vertices.Length * l;
			}
		}
		BentMesh.vertices = array;
		BentMesh.uv = array4;
		if (normals.Length > 0)
		{
			BentMesh.normals = array2;
		}
		if (tangents.Length > 0)
		{
			BentMesh.tangents = array3;
		}
		BentMesh.triangles = array5;
	}

	private void Setup()
	{
		if (!(spline == null))
		{
			if (bentMesh == null)
			{
				bentMesh = new Mesh();
				bentMesh.name = "BentMesh";
				bentMesh.hideFlags = HideFlags.HideAndDontSave;
			}
			MeshFilter component = GetComponent<MeshFilter>();
			if (component.sharedMesh != BentMesh)
			{
				component.sharedMesh = BentMesh;
			}
			MeshCollider component2 = GetComponent<MeshCollider>();
			if (component2 != null)
			{
				component2.sharedMesh = null;
				component2.sharedMesh = BentMesh;
			}
		}
	}

	private void CalculateBentMesh(ref int vIndex, List<int> verticesFront, List<int> verticesBack, ref Vector3 centerFront, ref Vector3 centerBack, float param0, float param1, Vector3[] verticesBase, Vector3[] normalsBase, Vector4[] tangentsBase, Vector2[] uvBase, Vector3[] verticesNew, Vector3[] normalsNew, Vector4[] tangentsNew, Vector2[] uvNew)
	{
		Vector3 b = spline.transform.InverseTransformPoint(spline.GetPositionOnSpline(param0));
		Vector3 b2 = spline.transform.InverseTransformPoint(spline.GetPositionOnSpline(param1));
		Quaternion rotation = spline.GetOrientationOnSpline(param0) * Quaternion.Inverse(spline.transform.localRotation);
		Quaternion rotation2 = spline.GetOrientationOnSpline(param1) * Quaternion.Inverse(spline.transform.localRotation);
		for (int i = 0; i < verticesBase.Length; i++)
		{
			Vector3 vector = verticesBase[i];
			Vector3 v = uvBase[i];
			Vector3 vector2 = (normalsBase.Length <= 0) ? Vector3.zero : normalsBase[i];
			Vector3 vector3 = (tangentsBase.Length <= 0) ? Vector3.zero : ((Vector3)tangentsBase[i]);
			if (verticesBack.Contains(i))
			{
				vector -= centerBack;
				vector.Scale(new Vector3(xyScale[0], xyScale[1], 1f));
				vector = rotation * vector;
				vector += b;
				vector2 = rotation * vector2;
				vector3 = rotation * vector3;
				if (!swapUV)
				{
					v.y = param0;
				}
				else
				{
					v.x = param0;
				}
			}
			else if (verticesFront.Contains(i))
			{
				vector -= centerFront;
				vector.Scale(new Vector3(xyScale[0], xyScale[1], 1f));
				vector = rotation2 * vector;
				vector += b2;
				vector2 = rotation2 * vector2;
				vector3 = rotation2 * vector3;
				if (!swapUV)
				{
					v.y = param1;
				}
				else
				{
					v.x = param1;
				}
			}
			verticesNew[vIndex] = vector;
			uvNew[vIndex] = Vector2.Scale(v, uvScale);
			if (normalsBase.Length > 0)
			{
				normalsNew[vIndex] = vector2;
			}
			if (tangentsBase.Length > 0)
			{
				tangentsNew[vIndex] = vector3;
			}
			vIndex++;
		}
	}
}
