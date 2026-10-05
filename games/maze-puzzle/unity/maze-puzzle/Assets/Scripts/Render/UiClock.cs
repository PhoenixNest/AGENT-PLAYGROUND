using System;

namespace OrbitalSalvage.Render
{
    /// <summary>
    /// Seconds since a page was shown. Independent of <c>ParityClock</c>. <see cref="Scale"/> slows or speeds playback.
    /// Tests inject <see cref="NowSeconds"/>.
    /// </summary>
    public sealed class UiClock
    {
        public double Scale = 1;
        public Func<double> NowSeconds = () => 0;
        double origin;

        public void Restart()
        {
            origin = NowSeconds();
        }

        public double Seconds => (NowSeconds() - origin) * Scale;
        public double Milliseconds => Seconds * 1000.0;
    }
}
