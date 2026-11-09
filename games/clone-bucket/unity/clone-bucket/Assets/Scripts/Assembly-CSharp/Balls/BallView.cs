using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Balls
{
	public class BallView : MonoBehaviour
	{
		[CompilerGenerated]
		private Action<Collider> m_OnTriggerEntered;

		[CompilerGenerated]
		private Action<Collider> m_OnTriggerExited;

		public GameObject gameObjectCached;

		public Transform transformCached;

		public Rigidbody rigidbodyCached;

		public Collider colliderCached;

		public MeshRenderer meshRendererCached;

		public Transform skinRotator;

		public event Action<Collider> OnTriggerEntered
		{
			[CompilerGenerated]
			add
			{
				m_OnTriggerEntered = (Action<Collider>)Delegate.Combine(m_OnTriggerEntered, value);
			}
			[CompilerGenerated]
			remove
			{
				m_OnTriggerEntered = (Action<Collider>)Delegate.Remove(m_OnTriggerEntered, value);
			}
		}

		public event Action<Collider> OnTriggerExited
		{
			[CompilerGenerated]
			add
			{
				m_OnTriggerExited = (Action<Collider>)Delegate.Combine(m_OnTriggerExited, value);
			}
			[CompilerGenerated]
			remove
			{
				m_OnTriggerExited = (Action<Collider>)Delegate.Remove(m_OnTriggerExited, value);
			}
		}

		private void Awake()
		{
			CacheFields();
		}

		private void OnTriggerExit(Collider other)
		{
			m_OnTriggerExited?.Invoke(other);
		}

		private void OnTriggerEnter(Collider other)
		{
			m_OnTriggerEntered?.Invoke(other);
		}

		private void Reset()
		{
			CacheFields();
		}

		internal void CacheFieldsIfNeeded()
		{
			CacheFields();
		}

		private void CacheFields()
		{
			if (gameObjectCached == null)
			{
				gameObjectCached = gameObject;
			}
			if (transformCached == null)
			{
				transformCached = transform;
			}
			if (rigidbodyCached == null)
			{
				rigidbodyCached = GetComponent<Rigidbody>();
			}
			if (colliderCached == null)
			{
				colliderCached = GetComponent<Collider>();
			}
			if (meshRendererCached == null)
			{
				meshRendererCached = GetComponentInChildren<MeshRenderer>();
			}
		}
	}
}
