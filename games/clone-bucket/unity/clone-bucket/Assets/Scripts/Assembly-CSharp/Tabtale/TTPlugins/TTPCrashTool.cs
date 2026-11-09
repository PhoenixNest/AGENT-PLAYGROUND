using System;
using UnityEngine;

namespace Tabtale.TTPlugins
{
	public class TTPCrashTool
	{
		private interface ICrashTool
		{
			void AddBreadCrumb(string crumb);

			void SetUserId(string userId);

			void LogNonFatalException(string log, string stackTrace);
		}

		private class EditorImpl : ICrashTool
		{
			public void AddBreadCrumb(string crumb)
			{
			}

			public void SetUserId(string userId)
			{
			}

			public void LogNonFatalException(string log, string stackTrace)
			{
			}
		}

		private static ICrashTool _impl;

		private static ICrashTool Impl => null;

		public static void SetCrashlyticsUserId(string userId)
		{
		}

		public static void LogNonFatalCrash(string log, string stackTrace)
		{
		}

		public static void AddBreadCrumb(string crumb)
		{
		}

		[Obsolete("This is an obsolete method, will be removed in the future versions...")]
		public static void ClearAllBreadCrumbs()
		{
		}
	}
}
