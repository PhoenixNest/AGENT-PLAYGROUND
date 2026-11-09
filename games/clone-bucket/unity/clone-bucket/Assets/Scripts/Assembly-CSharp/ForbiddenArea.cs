using UnityEngine;

public class ForbiddenArea : MonoBehaviour
{
	private static string _ballLayer;

	public Transform targetPoint;

	private void OnTriggerStay(Collider other)
	{
		if (targetPoint == null || other == null || !other.CompareTag("Ball"))
		{
			return;
		}

		Rigidbody body = other.attachedRigidbody;
		if (body == null)
		{
			return;
		}

		Vector3 toTarget = targetPoint.position - body.position;
		toTarget.z = 0f;
		if (toTarget.sqrMagnitude < 0.000001f)
		{
			return;
		}

		float speed = body.linearVelocity.magnitude;
		body.linearVelocity = toTarget.normalized * speed;
	}
}
