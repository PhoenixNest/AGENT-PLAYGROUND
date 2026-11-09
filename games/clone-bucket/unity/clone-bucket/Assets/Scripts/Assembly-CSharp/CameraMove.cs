using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
	private const string DataSetId = "CameraData";

	public float Damping;

	public float BossLevelBucketOffset;

	public float yOffset;

	public float yOffsetDefault;

	public bool isZooming;

	[Header("Zoom to Fit")]
	public float ZoomPadding;

	public float MinZoomSize;

	public float MaxZoomSize;

	private float maxPosY;

	private float minPosY;

	private bool isBossLevel;

	private bool movingToDefault;

	private Transform transformCache;

	private Vector3 defaultPos;

	private float posYLimit;

	private Camera cam;

	private float defaultOrthoSize;

	private float defaultFOV;

	private float defaultCamZ;

	private List<Transform> balls;

	private bool defaultsRecorded;

	private float targetY;

	private Tween zoomTween;

	private Tween chunkTween;

	private void Awake()
	{
		cam = GetComponent<Camera>();
		transformCache = transform;
		balls = new List<Transform>();
		defaultPos = transformCache.position;
		defaultFOV = cam != null ? cam.fieldOfView : 50f;
		defaultCamZ = defaultPos.z;
		defaultOrthoSize = SizeAtDistance(Mathf.Abs(defaultCamZ), defaultFOV);
		posYLimit = float.NegativeInfinity;
		minPosY = float.NegativeInfinity;
		maxPosY = float.PositiveInfinity;
		targetY = defaultPos.y;
	}

	private void OnDestroy()
	{
		DOTween.Kill(this);
		KillZoomTween();
		if (balls != null)
		{
			balls.Clear();
		}
	}

	private void OnDisable()
	{
		DOTween.Kill(this);
		KillZoomTween();
		movingToDefault = false;
	}

	private void HandleOffset()
	{
		yOffset = isBossLevel ? yOffsetDefault : 0f;
	}

	private void RemoveBalls()
	{
		if (balls != null)
		{
			balls.Clear();
		}
		if (defaultsRecorded)
		{
			maxPosY = defaultPos.y;
			minPosY = defaultPos.y;
		}
		if (!isBossLevel || !defaultsRecorded || transformCache == null)
		{
			return;
		}

		movingToDefault = true;
		transformCache.DOMoveX(defaultPos.x, 1f)
			.SetTarget(this)
			.OnComplete(() => movingToDefault = false);
		transformCache.DOMoveY(defaultPos.y, 1f).SetTarget(this);
		RestoreDefaultZoom(1f);
	}

	public void AddBall(Transform ball)
	{
		if (ball == null)
		{
			return;
		}
		if (balls == null)
		{
			balls = new List<Transform>();
		}
		if (!balls.Contains(ball))
		{
			balls.Add(ball);
		}
	}

	public void DeleteBall(Transform ball)
	{
		if (balls == null || ball == null)
		{
			return;
		}
		balls.Remove(ball);
	}

	public void MoveToChunkX(float targetX, float duration)
	{
		if (transformCache == null)
		{
			transformCache = transform;
		}
		KillChunkTween();
		chunkTween = transformCache.DOMoveX(targetX, duration).SetTarget(this);
	}

	private void KillChunkTween()
	{
		if (chunkTween != null)
		{
			if (chunkTween.IsActive())
			{
				chunkTween.Kill();
			}
			chunkTween = null;
		}
	}

	public void Reset()
	{
		KillChunkTween();
		if (balls != null)
		{
			balls.Clear();
		}
		if (!defaultsRecorded || transformCache == null)
		{
			return;
		}

		maxPosY = defaultPos.y;
		minPosY = defaultPos.y;
		transformCache.position = defaultPos;
		RestoreDefaultZoom(0f);
	}

	private void RestoreDefaultZoom(float duration)
	{
		if (cam == null || transformCache == null)
		{
			return;
		}

		KillZoomTween();
		if (!cam.orthographic)
		{
			if (duration > 0f)
			{
				zoomTween = transformCache.DOMoveZ(defaultCamZ, duration).SetTarget(this);
			}
			else
			{
				Vector3 p = transformCache.position;
				p.z = defaultCamZ;
				transformCache.position = p;
			}
		}
		else
		{
			if (duration > 0f)
			{
				zoomTween = DOTween.To(
						() => cam.orthographicSize,
						size => cam.orthographicSize = size,
						defaultOrthoSize,
						duration)
					.SetTarget(this);
			}
			else
			{
				cam.orthographicSize = defaultOrthoSize;
			}
		}
	}

	public void PosYLimitUpdate(float bucketPosY)
	{
		if (isBossLevel)
		{
			posYLimit = BossLevelBucketOffset + bucketPosY;
		}
	}

	public void SetBossLevel(bool value)
	{
		isBossLevel = value;
	}

	private IEnumerator Start()
	{
		yield return null;
		if (transformCache == null)
		{
			transformCache = transform;
		}
		if (cam == null)
		{
			cam = GetComponent<Camera>();
		}
		defaultPos = transformCache.position;
		defaultFOV = cam != null ? cam.fieldOfView : defaultFOV;
		defaultCamZ = defaultPos.z;
		defaultOrthoSize = SizeAtDistance(Mathf.Abs(defaultCamZ), defaultFOV);
		minPosY = defaultPos.y;
		maxPosY = float.PositiveInfinity;
		targetY = defaultPos.y;
		defaultsRecorded = true;
	}

	private void Update()
	{
		if (!defaultsRecorded)
		{
			return;
		}
		if (isBossLevel)
		{
			UpdateTargetPosition();
		}
	}

	private void UpdateTargetPosition()
	{
		float averageY = defaultPos.y;
		float lowestY = defaultPos.y;
		float highestY = defaultPos.y;
		if (balls != null && balls.Count > 0)
		{
			float sum = 0f;
			int count = 0;
			for (int i = 0; i < balls.Count; i++)
			{
				if (balls[i] == null)
				{
					continue;
				}
				float y = balls[i].position.y;
				sum += y;
				if (count == 0)
				{
					lowestY = y;
					highestY = y;
				}
				else
				{
					lowestY = Mathf.Min(lowestY, y);
					highestY = Mathf.Max(highestY, y);
				}
				count++;
			}
			if (count > 0)
			{
				averageY = sum / count;
			}
		}
		minPosY = lowestY;
		maxPosY = highestY;
		targetY = ClampTargetY(averageY + yOffset, minPosY, maxPosY, posYLimit);
	}

	private void FixedUpdate()
	{
		if (!defaultsRecorded || !isBossLevel || movingToDefault || transformCache == null)
		{
			return;
		}

		float t = Mathf.Clamp01(Time.fixedDeltaTime * Damping);
		Vector3 p = transformCache.position;
		p.y = Mathf.Lerp(p.y, targetY, t);
		transformCache.position = p;

		if (cam != null && balls != null && balls.Count > 0)
		{
			UpdateZoom();
		}
	}

	private void UpdateZoom()
	{
		if (!isZooming || cam == null || transformCache == null)
		{
			return;
		}

		float size = ZoomSize(maxPosY - minPosY, ZoomPadding, MinZoomSize, MaxZoomSize);
		float t = Mathf.Clamp01(Time.fixedDeltaTime * Damping);

		if (!cam.orthographic)
		{
			if (zoomTween != null && zoomTween.IsActive())
			{
				return;
			}
			float distance = SizeToDistance(size, defaultFOV);
			float goalZ = defaultCamZ < 0f ? -distance : distance;
			Vector3 p = transformCache.position;
			p.z = Mathf.Lerp(p.z, goalZ, t);
			transformCache.position = p;
		}
		else
		{
			cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, size, t);
		}
	}

	private void KillZoomTween()
	{
		if (zoomTween != null)
		{
			if (zoomTween.IsActive())
			{
				zoomTween.Kill();
			}
			zoomTween = null;
		}
	}

	private static float SizeAtDistance(float distance, float fovDeg)
	{
		return distance * Mathf.Tan(fovDeg * 0.5f * Mathf.Deg2Rad);
	}

	internal static float SizeToDistance(float size, float fovDeg)
	{
		float t = Mathf.Tan(fovDeg * 0.5f * Mathf.Deg2Rad);
		if (t <= 0f)
		{
			return size;
		}
		return size / t;
	}

	internal static float ZoomSize(float rangeY, float padding, float minSize, float maxSize)
	{
		return Mathf.Clamp(padding + rangeY * 0.5f, minSize, maxSize);
	}

	internal static float ClampTargetY(float desiredY, float minY, float maxY, float limitY)
	{
		return Mathf.Max(limitY, Mathf.Clamp(desiredY, minY, maxY));
	}

	internal static float DampStep(float current, float target, float damping, float deltaTime)
	{
		float t = Mathf.Clamp01(Mathf.Max(0f, damping) * Mathf.Max(0f, deltaTime));
		return current + (target - current) * t;
	}
}
