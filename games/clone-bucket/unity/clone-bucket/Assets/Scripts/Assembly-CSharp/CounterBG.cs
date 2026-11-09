using UnityEngine;

public class CounterBG : MonoBehaviour
{
	public Transform Body;

	public Transform Left;

	public Transform Right;

	private float bodyWidth;

	private float leftWidth;

	private float rightWidth;

	private float bodyBaseX;

	private float rightOffset;

	private void Awake()
	{
		if (Body != null)
		{
			bodyWidth = Body.localScale.x;
			bodyBaseX = Body.localPosition.x;
		}

		if (Left != null)
		{
			leftWidth = Left.localScale.x;
		}

		if (Right != null)
		{
			rightWidth = Right.localScale.x;
			rightOffset = Right.localPosition.x - (bodyBaseX + bodyWidth);
		}
	}

	public void SetScaleX(float value)
	{
		if (Body == null)
		{
			return;
		}

		float fraction = Mathf.Clamp01(value);

		Vector3 bodyScale = Body.localScale;
		bodyScale.x = bodyWidth * fraction;
		Body.localScale = bodyScale;

		if (Right != null)
		{
			Vector3 rightPosition = Right.localPosition;
			rightPosition.x = bodyBaseX + bodyWidth * fraction + rightOffset;
			Right.localPosition = rightPosition;
		}
	}
}
