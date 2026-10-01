using System;

namespace OrbitalSalvage.Logic
{
    public static class DailyReset
    {
        public static TimeZoneInfo FindLosAngeles()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            }
            catch (Exception)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
            }
        }

        public static string GetCountdownText(IWallClock clock, TimeZoneInfo zone)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            double nowMs = Math.Floor(clock.NowEpochMs);
            DateTime nowUtc = epoch.AddMilliseconds(nowMs);
            DateTime local = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, zone);
            DateTime tomorrowLocal = DateTime.SpecifyKind(local.Date.AddDays(1), DateTimeKind.Unspecified);
            if (zone.IsInvalidTime(tomorrowLocal)) tomorrowLocal = tomorrowLocal.AddHours(1);
            DateTime tomorrowUtc = TimeZoneInfo.ConvertTimeToUtc(tomorrowLocal, zone);
            double diff = (tomorrowUtc - nowUtc).TotalMilliseconds;
            double hours = Math.Floor(diff / (1000.0 * 60 * 60));
            double mins = Math.Floor((diff % (1000.0 * 60 * 60)) / (1000.0 * 60));
            return "Resets in " + JsCompat.JsNumber.ToString(hours) + "h " + JsCompat.JsNumber.ToString(mins) + "m";
        }
    }
}
