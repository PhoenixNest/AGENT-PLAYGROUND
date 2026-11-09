using System.Collections.Generic;
using DG.Tweening;
using Levels;
using TMPro;
using UnityEngine;

public class Chain : MonoBehaviour
{
	private const float UnlockHideDelay = 0.5f;

	private static readonly Color PulseColor = new Color(1f, 0.4f, 0.4f, 1f);
	private const float PulseDuration = 0.5f;

	public int UnlockNumber;

	public TextMeshProUGUI CounterText;

	public CounterBG counterBG;

	public GameObject P_Lock;

	[HideInInspector]
	public int ChainCount;

	private bool unlocked;

	private Tweener colorTween;

	private readonly List<Collider> segmentColliders = new List<Collider>();

	private void Awake()
	{
		unlocked = false;

		foreach (ChainSegment segment in GetComponentsInChildren<ChainSegment>(true))
		{
			foreach (Collider col in segment.GetComponentsInChildren<Collider>(true))
			{
				if (!col.isTrigger)
				{
					segmentColliders.Add(col);
				}
			}
		}
	}

	private void OnDestroy()
	{
		KillTweens();
	}

	private void OnDisable()
	{
		KillTweens();
	}

	private void Start()
	{
		ChainUpdate();
	}

	public void ChainUpdate()
	{
		if (CounterText != null)
		{
			CounterText.text = ChainCount + "/" + UnlockNumber;
		}

		if (counterBG != null)
		{
			counterBG.SetScaleX(CounterFraction(ChainCount, UnlockNumber));
		}

		if (ChainCount < UnlockNumber || unlocked)
		{
			return;
		}

		unlocked = true;
		colorTween?.Kill();
		colorTween = null;
		SetSegmentsBlocking(false);
		if (CounterText != null)
		{
			CounterText.color = new Color(0f, 1f, 0f, 1f);
		}

		if (P_Lock != null)
		{
			P_Lock.SetActive(true);
			DOTween.Sequence().AppendInterval(UnlockHideDelay).AppendCallback(() => gameObject.SetActive(false));
		}
	}

	public void AddBall(int ballValue)
	{
		ChainCount += ballValue;
		ChainUpdate();
	}

	public void RemoveBall(int ballValue)
	{
		ChainCount -= ballValue;
		ChainUpdate();
	}

	private void Update()
	{
		if (unlocked)
		{
			return;
		}

		if (colorTween != null && colorTween.IsPlaying() && ChainCount == 0)
		{
			colorTween.Kill();
			colorTween = null;
			if (CounterText != null)
			{
				CounterText.color = Color.white;
			}
		}

		if (colorTween != null && colorTween.IsPlaying())
		{
			return;
		}

		if (ChainCount > 0 && ChainCount < UnlockNumber)
		{
			colorTween?.Kill();
			ColorRed();
		}
	}

	private void ColorRed()
	{
		if (CounterText == null)
		{
			return;
		}

		colorTween = CounterText.DOColor(PulseColor, PulseDuration).SetLoops(-1, LoopType.Yoyo);
	}

	internal static bool ReachesUnlock(int chainCount, int unlockNumber)
	{
		return chainCount >= unlockNumber;
	}

	internal static float CounterFraction(int chainCount, int unlockNumber)
	{
		if (unlockNumber <= 0)
		{
			return 1f;
		}

		return Mathf.Clamp01((float)chainCount / unlockNumber);
	}

	private void SetSegmentsBlocking(bool blocking)
	{
		for (int i = 0; i < segmentColliders.Count; i++)
		{
			if (segmentColliders[i] != null)
			{
				segmentColliders[i].enabled = blocking;
			}
		}
	}

	private void KillTweens()
	{
		colorTween?.Kill();
		colorTween = null;
	}
}
