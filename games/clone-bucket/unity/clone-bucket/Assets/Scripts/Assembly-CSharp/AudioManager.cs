using System.Collections;
using UnityEngine;

public class AudioManager : SingletonBehaviour<AudioManager>
{
	private const float VoiceCooldown = 0.1f;

	private const int BallVoiceCount = 5;

	private const int BucketVoiceCount = 2;

	private const int MultiplierVoiceCount = 5;

	private const int RecoverFrames = 2;

	public AudioSource MainSource;

	public AudioSource CupstarSource;

	public AudioClip ButtonClickClip;

	public AudioClip CompleteLevelClip;

	public AudioClip GemClip;

	public AudioClip ConfettiClip;

	public AudioClip SmallBallClip;

	public AudioClip MediumBallClip;

	public AudioClip BigBallClip;

	public AudioClip MultiplierClip;

	public AudioClip BucketFillClip;

	public AudioClip EndPanelClip;

	public AudioClip starAppears;

	public AudioClip _3starAppears;

	public AudioClip starCup;

	[HideInInspector]
	public bool isSoundsPlay = true;

	private float[] ballList = new float[BallVoiceCount];

	private float[] bucketList = new float[BucketVoiceCount];

	private float[] multiplierList = new float[MultiplierVoiceCount];

	private float timer;

	private bool _recoverQueued;

	protected override void Awake()
	{
		base.Awake();
		EnsureSourcesConfigured();
		ApplySoundState();
	}

	private void OnEnable()
	{
		AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
	}

	private void OnDisable()
	{
		AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigurationChanged;
	}

	public void Reset()
	{
	}

	public void SoundOn()
	{
		isSoundsPlay = true;
		ApplySoundState();
		QueueRecover();
	}

	public void SoundOff()
	{
		isSoundsPlay = false;
		ApplySoundState();
		StopAllSfxImmediate();
	}

	public void PlayButtonClick()
	{
		Play(MainSource, ButtonClickClip);
	}

	public void PlayEndPanel()
	{
		Play(MainSource, EndPanelClip);
	}

	public void PlayCompleteLevel()
	{
		Play(MainSource, CompleteLevelClip);
	}

	public void PlayGem()
	{
		Play(MainSource, GemClip);
	}

	public void PlayConfetti()
	{
		Play(MainSource, ConfettiClip);
	}

	public void PlayMultiplier()
	{
		if (isSoundsPlay && CanPlayAt(multiplierList, timer, VoiceCooldown))
		{
			Play(MainSource, MultiplierClip);
		}
	}

	public void PlayBucketFill()
	{
		if (isSoundsPlay && CanPlayAt(bucketList, timer, VoiceCooldown))
		{
			Play(MainSource, BucketFillClip);
		}
	}

	public void PlaySmallBall()
	{
		if (isSoundsPlay && CanPlayAt(ballList, timer, VoiceCooldown))
		{
			Play(MainSource, SmallBallClip);
		}
	}

	public void PlayMediumBall()
	{
		if (isSoundsPlay && CanPlayAt(ballList, timer, VoiceCooldown))
		{
			Play(MainSource, MediumBallClip);
		}
	}

	public void PlayBigBall()
	{
		if (isSoundsPlay && CanPlayAt(ballList, timer, VoiceCooldown))
		{
			Play(MainSource, BigBallClip);
		}
	}

	public void PlayStarAppears()
	{
		Play(MainSource, starAppears);
	}

	public void Play3StarAppears()
	{
		Play(MainSource, _3starAppears);
	}

	public void PlayStarCup()
	{
		if (!isSoundsPlay || CupstarSource == null || starCup == null)
		{
			return;
		}

		EnsureSourceSafe(CupstarSource);
		if (CupstarSource.isPlaying)
		{
			return;
		}

		CupstarSource.PlayOneShot(starCup);
	}

	private void Play(AudioSource source, AudioClip clip)
	{
		if (!isSoundsPlay || clip == null)
		{
			return;
		}

		if (source == null)
		{
			EnsureSourcesConfigured();
			source = MainSource;
		}

		if (source == null)
		{
			return;
		}

		EnsureSourceSafe(source);
		source.PlayOneShot(clip);
	}

	private void EnsureSourcesConfigured()
	{
		if (MainSource == null)
		{
			MainSource = CreateSource();
		}

		if (CupstarSource == null)
		{
			CupstarSource = CreateSource();
		}

		EnsureSourceSafe(MainSource);
		EnsureSourceSafe(CupstarSource);
	}

	private static void EnsureSourceSafe(AudioSource s)
	{
		if (s == null)
		{
			return;
		}

		s.playOnAwake = false;
		s.loop = false;
		if (s.clip != null)
		{
			s.clip = null;
		}
	}

	private void ApplySoundState()
	{
		if (MainSource != null)
		{
			MainSource.mute = !isSoundsPlay;
		}

		if (CupstarSource != null)
		{
			CupstarSource.mute = !isSoundsPlay;
		}
	}

	private void StopAllSfxImmediate()
	{
		StopSource(MainSource);
		StopSource(CupstarSource);
	}

	private static void StopSource(AudioSource s)
	{
		if (s == null)
		{
			return;
		}

		s.loop = false;
		s.clip = null;
		s.Stop();
	}

	public static bool CanPlayAt(float[] pool, float now, float cooldown)
	{
		if (pool == null)
		{
			return false;
		}

		for (int i = 0; i < pool.Length; i++)
		{
			if (pool[i] + cooldown < now)
			{
				pool[i] = now;
				return true;
			}
		}

		return false;
	}

	private AudioSource CreateSource()
	{
		AudioSource source = gameObject.AddComponent<AudioSource>();
		source.playOnAwake = false;
		source.loop = false;
		return source;
	}

	private void Update()
	{
		timer += Time.unscaledDeltaTime;
	}

	private void OnApplicationPause(bool pause)
	{
		if (!pause)
		{
			QueueRecover();
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			QueueRecover();
		}
	}

	private void OnAudioConfigurationChanged(bool deviceWasChanged)
	{
		QueueRecover();
	}

	private void QueueRecover()
	{
		if (_recoverQueued || !isActiveAndEnabled)
		{
			return;
		}

		_recoverQueued = true;
		StartCoroutine(RecoverAudioNextFrames());
	}

	private IEnumerator RecoverAudioNextFrames()
	{
		StopAllSfxImmediate();
		for (int i = 0; i < RecoverFrames; i++)
		{
			yield return null;
		}

		EnsureSourcesConfigured();
		ApplySoundState();
		_recoverQueued = false;
	}
}
