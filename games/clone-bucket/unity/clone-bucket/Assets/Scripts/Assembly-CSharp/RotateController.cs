using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;

public class RotateController : MonoBehaviour
{
	public GameObject Target;

	public List<RotateTarget> Targets;

	public Space Space;

	public Vector3 Axis;

	public float Multiplier;

	public float Damping;

	public bool ScaleByTime;

	private Vector2 refecencePoint;

	private float targetDistance;

	private Quaternion remainingDelta = Quaternion.identity;

	public void RotateAB()
	{
		if (Target == null)
		{
			return;
		}
		Camera cam = Camera.main;
		if (cam == null)
		{
			return;
		}

		Vector3 targetPosition = Target.transform.position;
		Vector3 targetScreen = cam.WorldToScreenPoint(targetPosition);
		refecencePoint = new Vector2(targetScreen.x, targetScreen.y);
		targetDistance = targetScreen.z;

		if (LeanTouch.Fingers.Count == 0)
		{
			return;
		}
		LeanFinger finger = LeanTouch.Fingers[0];
		if (finger == null || !finger.Set)
		{
			return;
		}

		float dist = Vector3.Distance(finger.GetWorldPosition(targetDistance, cam), targetPosition) * 0.25f;
		if (Targets != null && Targets.Count > 0)
		{
			dist = Mathf.Min(dist, 1f);
		}

		float degrees = finger.GetDeltaDegrees(refecencePoint);
		if (ScaleByTime)
		{
			degrees *= Time.deltaTime;
		}

		float angle = dist * degrees * Multiplier;
		Transform targetTransform = Target.transform;
		Quaternion oldRotation = targetTransform.localRotation;
		targetTransform.Rotate(Axis, angle, Space);
		remainingDelta *= Quaternion.Inverse(oldRotation) * targetTransform.localRotation;
		targetTransform.localRotation = oldRotation;
	}

	private void Awake()
	{
		if (Targets == null)
		{
			Targets = new List<RotateTarget>();
		}

		if (Targets.Count > 0)
		{
			Transform reference = GameManager.instance != null ? GameManager.instance.ReferencePoint : null;
			if (reference != null)
			{
				Target = reference.gameObject;
			}
		}
	}

	public void RotateToDefault()
	{
		if (Target != null)
		{
			Target.transform.rotation = Quaternion.identity;
		}
	}

	private void Start()
	{
		GameManager gameManager = GameManager.instance;
		if (gameManager != null)
		{
			Multiplier = gameManager.ControllerMultiplier;
			Damping = gameManager.ControllerDamping;
		}
	}

	private void FixedUpdate()
	{
		UpdateRotation(LeanHelper.GetDampenFactor(Damping, Time.deltaTime));
	}

	private void UpdateRotation(float factor)
	{
		Quaternion newDelta = Quaternion.Slerp(remainingDelta, Quaternion.identity, factor);

		if (Target != null)
		{
			Transform targetTransform = Target.transform;
			targetTransform.localRotation = targetTransform.localRotation * Quaternion.Inverse(newDelta) * remainingDelta;
		}

		for (int i = 0; i < Targets.Count; i++)
		{
			RotateTarget rotateTarget = Targets[i];
			if (rotateTarget == null || rotateTarget.Target == null)
			{
				continue;
			}

			Transform rotated = rotateTarget.Target;
			if (rotateTarget.Dir == 1)
			{
				rotated.localRotation = rotated.localRotation * Quaternion.Inverse(remainingDelta) * newDelta;
			}
			else
			{
				rotated.localRotation = rotated.localRotation * Quaternion.Inverse(newDelta) * remainingDelta;
			}
		}

		remainingDelta = newDelta;
	}
}
