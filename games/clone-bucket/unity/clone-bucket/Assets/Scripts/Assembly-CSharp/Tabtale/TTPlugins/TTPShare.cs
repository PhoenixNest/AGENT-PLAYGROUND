using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Tabtale.TTPlugins
{
	public class TTPShare
	{
		[Serializable]
		private class GlobalConfiguration
		{
			public string appId;
		}

		[CompilerGenerated]
		private sealed class _003CShareImageBytes_003Ed__11 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public byte[] imageBytes;

			private string _003Cdst_003E5__2;

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
			public _003CShareImageBytes_003Ed__11(int _003C_003E1__state)
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
		private sealed class _003CShareRoutine_003Ed__2 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public string ssPath;

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
			public _003CShareRoutine_003Ed__2(int _003C_003E1__state)
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

		private const string SS_NAME = "screenshot.png";

		private const string IMAGE_BASE64_TAG = "data:image/png;base64,";

		[IteratorStateMachine(typeof(_003CShareRoutine_003Ed__2))]
		private static IEnumerator ShareRoutine(string ssPath)
		{
			return null;
		}

		public static void ShareScreenshot()
		{
		}

		public static void ShareVideo(string pathToVideo)
		{
		}

		public static void ShareImage(string pathToImage)
		{
		}

		public static void ShareImage(byte[] imageBytes)
		{
		}

		public static void ShareLink(string link)
		{
		}

		public static void ShareAppLink()
		{
		}

		public static void ShareFirebaseInstanceId()
		{
		}

		[IteratorStateMachine(typeof(_003CShareImageBytes_003Ed__11))]
		private static IEnumerator ShareImageBytes(byte[] imageBytes)
		{
			return null;
		}
	}
}
