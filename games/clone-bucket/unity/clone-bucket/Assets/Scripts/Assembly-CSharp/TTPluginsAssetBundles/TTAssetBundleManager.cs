using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TTPluginsAssetBundles
{
	public class TTAssetBundleManager : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003CGetAssetBundle_003Ed__10 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public string assetBundleName;

			public int timeoutInSeconds;

			public Action<AssetBundle, string> callback;

			private float _003CrealtimeSinceStartup_003E5__2;

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[DebuggerHidden]
			public _003CGetAssetBundle_003Ed__10(int _003C_003E1__state)
			{
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
			}

			private bool MoveNext()
			{
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
			}
		}

		[CompilerGenerated]
		private sealed class _003CInitializeCoro_003Ed__6 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[DebuggerHidden]
			public _003CInitializeCoro_003Ed__6(int _003C_003E1__state)
			{
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
			}

			private bool MoveNext()
			{
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
			}
		}

		[CompilerGenerated]
		private sealed class _003CPeekAssetBundle_003Ed__12 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public string assetBundleName;

			public Action<bool> callback;

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[DebuggerHidden]
			public _003CPeekAssetBundle_003Ed__12(int _003C_003E1__state)
			{
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
			}

			private bool MoveNext()
			{
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
			}
		}

		private bool m_AssetBundleManagerInitialized;

		private bool m_AssetBundleManagerInitializationFinished;

		private const string m_TTAssetBundlePrefix = "TTAssetBundle_";

		private static TTAssetBundleManager m_Instance;

		private void Start()
		{
		}

		private static void Initialize()
		{
		}

		[IteratorStateMachine(typeof(_003CInitializeCoro_003Ed__6))]
		protected IEnumerator InitializeCoro()
		{
			return null;
		}

		protected void InitializeSourceURL()
		{
		}

		public void UnloadAssetBundle(string assetBundleName)
		{
		}

		public static void GetAssetBundleAsync(string assetBundleName, Action<AssetBundle, string> callback, int timeoutInSeconds = 15)
		{
		}

		[IteratorStateMachine(typeof(_003CGetAssetBundle_003Ed__10))]
		private IEnumerator GetAssetBundle(string assetBundleName, Action<AssetBundle, string> callback, int timeoutInSeconds)
		{
			return null;
		}

		public static void PeekAssetBundleAsync(string assetBundleName, Action<bool> callback)
		{
		}

		[IteratorStateMachine(typeof(_003CPeekAssetBundle_003Ed__12))]
		private IEnumerator PeekAssetBundle(string assetBundleName, Action<bool> callback)
		{
			return null;
		}

		public static bool IsAssetBundleAlreadyExists(string assetBundleName)
		{
			return false;
		}
	}
}
