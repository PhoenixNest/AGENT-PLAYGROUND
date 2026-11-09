using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Pool;

namespace Balls
{
	public static class BallsPool
	{
		[CompilerGenerated]
		private static Action<BallView> m_OnBallAboutToBeDestroyed;

		[CompilerGenerated]
		private static Action<BallView> m_OnBallInstantiated;

		private static BallsConfig _config;

		private static Transform _objectParent;

		private static ObjectPool<BallView> _ballPool;

		public static event Action<BallView> OnBallAboutToBeDestroyed
		{
			[CompilerGenerated]
			add
			{
				m_OnBallAboutToBeDestroyed = (Action<BallView>)Delegate.Combine(m_OnBallAboutToBeDestroyed, value);
			}
			[CompilerGenerated]
			remove
			{
				m_OnBallAboutToBeDestroyed = (Action<BallView>)Delegate.Remove(m_OnBallAboutToBeDestroyed, value);
			}
		}

		public static event Action<BallView> OnBallInstantiated
		{
			[CompilerGenerated]
			add
			{
				m_OnBallInstantiated = (Action<BallView>)Delegate.Combine(m_OnBallInstantiated, value);
			}
			[CompilerGenerated]
			remove
			{
				m_OnBallInstantiated = (Action<BallView>)Delegate.Remove(m_OnBallInstantiated, value);
			}
		}

		internal static int InactiveCount => _ballPool != null ? _ballPool.CountInactive : 0;

		internal static int ActiveCount => _ballPool != null ? _ballPool.CountActive : 0;

		public static void Initialize(BallsConfig config, Transform objectParent)
		{
			if (config == null)
			{
				Debug.LogError("[MM3:Balls] BallsPool.Initialize called without a BallsConfig.");
				return;
			}

			_config = config;
			_objectParent = objectParent;

			_ballPool = new ObjectPool<BallView>(
				createFunc: Instantiate,
				actionOnGet: OnGetBall,
				actionOnRelease: OnReleaseBall,
				actionOnDestroy: NotifyDestroy,
				collectionCheck: false,
				defaultCapacity: config.initialPoolAmount,
				maxSize: int.MaxValue);

			PreWarm(config.initialPoolAmount);
		}

		private static void PreWarm(int amount)
		{
			if (_ballPool == null || amount <= 0)
			{
				return;
			}

			var warm = new List<BallView>(amount);
			for (int i = 0; i < amount; i++)
			{
				BallView view = _ballPool.Get();
				if (view != null)
				{
					warm.Add(view);
				}
			}
			foreach (BallView view in warm)
			{
				_ballPool.Release(view);
			}
		}

		public static BallView Get()
		{
			if (_ballPool == null)
			{
				Debug.LogError("[MM3:Balls] BallsPool.Get called before Initialize.");
				return null;
			}
			return _ballPool.Get();
		}

		public static void Release(BallView view)
		{
			if (view == null || _ballPool == null)
			{
				return;
			}
			_ballPool.Release(view);
		}

		private static BallView Instantiate()
		{
			if (_config == null || _config.prefab == null)
			{
				Debug.LogError("[MM3:Balls] BallsConfig has no ball prefab.");
				return null;
			}

			BallView view = UnityEngine.Object.Instantiate(_config.prefab, _objectParent);
			view.gameObject.SetActive(false);
			m_OnBallInstantiated?.Invoke(view);
			return view;
		}

		private static void OnGetBall(BallView view)
		{
			view.gameObject.SetActive(true);
		}

		private static void OnReleaseBall(BallView view)
		{
			view.gameObject.SetActive(false);
			if (_objectParent != null)
			{
				view.transform.SetParent(_objectParent, false);
			}
		}

		private static void NotifyDestroy(BallView view)
		{
			m_OnBallAboutToBeDestroyed?.Invoke(view);
			UnityEngine.Object.Destroy(view.gameObject);
		}
	}
}
