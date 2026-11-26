using UnityEngine;

public class FillScaler : MonoBehaviour
{
	private void Start()
	{
		float num = 0.5625f;
		float num2 = (float)Screen.width / (float)Screen.height;
		if (num2 > num)
		{
			Transform transform = base.transform;
			Vector3 localScale = base.transform.localScale;
			float x = localScale.x * (num2 / num);
			Vector3 localScale2 = base.transform.localScale;
			float y = localScale2.y;
			Vector3 localScale3 = base.transform.localScale;
			transform.localScale = new Vector3(x, y, localScale3.z);
		}
		else if (num2 < num)
		{
			Transform transform2 = base.transform;
			Vector3 localScale4 = base.transform.localScale;
			float x2 = localScale4.x;
			Vector3 localScale5 = base.transform.localScale;
			float y2 = localScale5.y / (num2 / num);
			Vector3 localScale6 = base.transform.localScale;
			transform2.localScale = new Vector3(x2, y2, localScale6.z);
		}
	}
}
