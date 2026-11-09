using UnityEngine;

public static class TransformEx
{
	public static Transform Clear(this Transform transform)
	{
		if (transform == null)
		{
			return null;
		}
		for (int i = transform.childCount - 1; i >= 0; i--)
		{
			GameObject child = transform.GetChild(i).gameObject;
			if (Application.isPlaying)
			{
				Object.Destroy(child);
			}
			else
			{
				Object.DestroyImmediate(child);
			}
		}
		return transform;
	}
}
