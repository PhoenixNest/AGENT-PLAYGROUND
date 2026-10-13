using System;

namespace OrbitalSalvage.Logic
{
    public static class InfiniteMode
    {
        public const int PoolFirst = 30;
        public const int PoolLast = 50;

        public static readonly Random DefaultRandom = new Random();

        public static int NextLevel(Func<int, int> randomBelow)
        {
            int span = PoolLast - PoolFirst + 1;
            long level = (long)PoolFirst + randomBelow(span);
            if (level < PoolFirst) return PoolFirst;
            if (level > PoolLast) return PoolLast;
            return (int)level;
        }

        public static int NextLevel()
        {
            return NextLevel(max => DefaultRandom.Next(max));
        }
    }
}
