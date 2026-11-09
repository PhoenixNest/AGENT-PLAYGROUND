using System;

namespace Tabtale.TTPlugins
{
	public static class AnalyticsTargets
	{
		[Obsolete]
		public const long ANALYTICS_TARGET_FLURRY = 0L;

		public const long ANALYTICS_TARGET_TT_ANALYTICS = 0L;

		public const long ANALYTICS_TARGET_DELTA_DNA = 1L;

		public const long ANALYTICS_TARGET_FIREBASE = 2L;

		public const long ANALYTICS_PSDK_INTERNAL_TARGETS = 0L;
	}
}
