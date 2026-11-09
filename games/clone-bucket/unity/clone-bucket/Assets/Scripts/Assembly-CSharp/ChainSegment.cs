using UnityEngine;

public class ChainSegment : MonoBehaviour
{
	private Quaternion rotation;

	private void Start()
	{
		rotation = transform.localRotation;
	}

	private void FixedUpdate()
	{
		Vector3 euler = transform.localRotation.eulerAngles;
		transform.localRotation = Quaternion.Euler(euler.x, euler.y, 0f);
	}
}
