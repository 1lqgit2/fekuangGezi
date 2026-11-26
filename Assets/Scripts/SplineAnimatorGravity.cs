using UnityEngine;

public class SplineAnimatorGravity : MonoBehaviour
{
	public Spline spline;

	public float gravityConstant = 9.81f;

	public int iterations = 5;

	private void FixedUpdate()
	{
		if (!(GetComponent<Rigidbody>() == null) && !(spline == null))
		{
			Vector3 shortestConnection = spline.GetShortestConnection(GetComponent<Rigidbody>().position, iterations);
			GetComponent<Rigidbody>().AddForce(shortestConnection * (Mathf.Pow(shortestConnection.magnitude, -3f) * gravityConstant * GetComponent<Rigidbody>().mass));
		}
	}
}
