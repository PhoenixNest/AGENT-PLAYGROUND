using UnityEngine;
using UnityEngine.Scripting;

namespace Tabtale.TTPlugins
{
	public class TTPPopupMgr
	{
		private interface ITTPPopupMgrInternal
		{
			bool ShouldShow(string source);

			void OnShown(string source);

			void OnClosed(string source);
		}

		private class UnityTTPPopupMgr : ITTPPopupMgrInternal
		{
			public bool ShouldShow(string source)
			{
				return false;
			}

			public void OnShown(string source)
			{
			}

			public void OnClosed(string source)
			{
			}
		}

		private static ITTPPopupMgrInternal _impl;

		private static ITTPPopupMgrInternal Impl => null;

		public static bool ShouldShow(string source)
		{
			return false;
		}

		public static void OnShown(string source)
		{
		}

		public static void OnClosed(string source)
		{
		}
	}
}
