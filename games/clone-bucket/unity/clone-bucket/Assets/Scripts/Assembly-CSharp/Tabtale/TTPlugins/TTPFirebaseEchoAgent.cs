using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace Tabtale.TTPlugins
{
	internal class TTPFirebaseEchoAgent : MonoBehaviour
	{
		[CompilerGenerated]
		private static Action<string, Dictionary<string, object>> m_FirebaseLogEvent;

		public static event Action<string, Dictionary<string, object>> FirebaseLogEvent
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public void CallFirebaseLogEvent(string message)
		{
		}

		[Preserve]
		public static void SetInstanceIdFirebase(string firebaseInstanceId)
		{
		}
	}
}
