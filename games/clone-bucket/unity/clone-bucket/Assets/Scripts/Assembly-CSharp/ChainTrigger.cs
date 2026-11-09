using Balls;
using UnityEngine;

public class ChainTrigger : MonoBehaviour
{
	private const string BallTag = "Ball";

	public Chain chain;

	private void OnTriggerEnter(Collider ballCollider)
	{
		if (chain == null || !ballCollider.CompareTag(BallTag))
		{
			return;
		}

		chain.AddBall(BallsController.GetBallValue(ballCollider));
	}

	private void OnTriggerExit(Collider ballCollider)
	{
		if (chain == null || !ballCollider.CompareTag(BallTag))
		{
			return;
		}

		chain.RemoveBall(BallsController.GetBallValue(ballCollider));
	}
}
