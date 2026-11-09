using DG.Tweening;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
	private const float SwipeDuration = 1f;

	public Transform Finger;

	public Vector3 DefaultPos;

	public Vector3 TargetPos;

	private Tweener fingerTweenerX;

	private Tweener fingerTweenerY;

	private void OnEnable()
	{
		if (Finger == null)
		{
			return;
		}

		Finger.localPosition = DefaultPos;
		fingerTweenerX = Finger.DOLocalMoveX(TargetPos.x, SwipeDuration);
		fingerTweenerX.SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
		fingerTweenerY = Finger.DOLocalMoveY(TargetPos.y, SwipeDuration);
		fingerTweenerY.SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
	}

	private void OnDisable()
	{
		fingerTweenerX?.Kill();
		fingerTweenerX = null;
		fingerTweenerY?.Kill();
		fingerTweenerY = null;
	}
}
