using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GemAnimationEffect : MonoBehaviour
{
	private const int GemCount = 75;

	private const float OffsetRange = 200f;

	private const float WaitBeforeFlySeconds = 0.5f;

	private const float WaitBeforeDespawnSeconds = 0.7f;

	private const float FlyDuration = 0.6f;

	[SerializeField]
	private GameObject _gemPrefab;

	[SerializeField]
	private Image _targetImage;

	private readonly List<GameObject> _spawned = new List<GameObject>();

	private Sequence _sequence;

	public void Play()
	{
		if (_gemPrefab == null || _targetImage == null)
		{
			return;
		}

		KillSequence();
		Vector3 target = _targetImage.transform.position;

		var gems = new List<Transform>(GemCount);
		for (int i = 0; i < GemCount; i++)
		{
			GameObject gem = Instantiate(_gemPrefab, transform);
			gem.transform.position = transform.position;
			RectTransform rect = gem.GetComponent<RectTransform>();
			if (rect != null)
			{
				rect.anchoredPosition = new Vector2(
					UnityEngine.Random.Range(-OffsetRange, OffsetRange),
					UnityEngine.Random.Range(-OffsetRange, OffsetRange));
			}

			_spawned.Add(gem);
			gems.Add(gem.transform);
		}

		_sequence = DOTween.Sequence().AppendInterval(WaitBeforeFlySeconds);
		for (int i = 0; i < gems.Count; i++)
		{
			_sequence.Join(gems[i].DOMove(target, FlyDuration).SetTarget(gems[i]));
		}

		_sequence.AppendInterval(WaitBeforeDespawnSeconds)
			.OnComplete(RemoveAll)
			.SetTarget(this);
	}

	private void OnDisable()
	{
		KillSequence();
		RemoveAll();
	}

	private void KillSequence()
	{
		_sequence?.Kill();
		_sequence = null;
	}

	private void RemoveAll()
	{
		for (int i = 0; i < _spawned.Count; i++)
		{
			GameObject gem = _spawned[i];
			if (gem != null)
			{
				DOTween.Kill(gem.transform);
				Destroy(gem);
			}
		}

		_spawned.Clear();
	}
}
