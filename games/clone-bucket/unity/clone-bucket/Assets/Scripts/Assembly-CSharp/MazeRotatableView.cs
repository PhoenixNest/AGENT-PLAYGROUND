using UnityEngine;

public class MazeRotatableView : MonoBehaviour
{
	public Transform transformCached;

	private void Reset()
	{
		transformCached = transform;
	}
}
