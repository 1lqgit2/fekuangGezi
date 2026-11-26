using System.Collections.Generic;
using UnityEngine;

public class FeedingBullet : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> combos;

	[SerializeField]
	private bool isWorm;

	public void SetComboLevel(int level)
	{
		int num = 0;
		if (level > 0 && level < 4)
		{
			num = 1;
		}
		else if (level > 3 && level < 8)
		{
			num = 2;
		}
		else if (level > 7)
		{
			num = 3;
		}
		for (int i = 0; i < combos.Count; i++)
		{
			if (i == num)
			{
				combos[i].SetActive(value: true);
			}
			else
			{
				combos[i].SetActive(value: false);
			}
		}
	}

	public bool IsWorm()
	{
		return isWorm;
	}
}
