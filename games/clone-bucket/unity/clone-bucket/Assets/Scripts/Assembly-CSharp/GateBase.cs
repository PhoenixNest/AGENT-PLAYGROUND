using System;
using DG.Tweening;
using UnityEngine;

public abstract class GateBase : MonoBehaviour
{
	private const float MinMoveTime = 0.01f;

	public bool isMoveX;

	[NonSerialized]
	public bool stopped;

	private Tweener moveTweener;

	[NonSerialized]
	public float startPosX;

	public float MoveTime;

	public abstract GateType Type { get; protected set; }

	public abstract int Value { get; protected set; }

	public abstract void SetValue(int value);

	internal bool IsMoving
	{
		get { return moveTweener != null && moveTweener.IsActive(); }
	}

	private void Awake()
	{
		GatesController.Register(this);
	}

	protected virtual void Start()
	{
		startPosX = transform.localPosition.x;
		if (isMoveX)
		{
			Move();
		}
	}

	public void StopMove()
	{
		if (!isMoveX)
		{
			return;
		}
		KillMove();
		stopped = true;
	}

	public void Move()
	{
		if (!isMoveX || stopped)
		{
			return;
		}
		KillMove();
		float localX = transform.localPosition.x;
		float target = -Mathf.Sign(localX) * Mathf.Abs(startPosX);
		float duration = Mathf.Max(MoveTime, MinMoveTime);
		moveTweener = transform.DOLocalMoveX(target, duration)
			.SetEase(Ease.InOutSine)
			.OnComplete(Move)
			.SetLink(gameObject);
	}

	protected virtual void OnDisable()
	{
		KillMove();
	}

	private void OnDestroy()
	{
		KillMove();
		GatesController.Unregister(this);
	}

	private void KillMove()
	{
		if (moveTweener != null)
		{
			moveTweener.Kill();
			moveTweener = null;
		}
	}
}
