using System;
using UnityEditor;

namespace OrbitalSalvage.Render.Editor
{
    public static class LiveConformanceBatch
    {
        public static void Run()
        {
            string outDir = Arg("-live-out");
            string streams = Arg("-live-streams");
            if (string.IsNullOrEmpty(outDir) || string.IsNullOrEmpty(streams))
                throw new InvalidOperationException("pass -live-out <dir> -live-streams <dir>");
            LiveConformance.Export(outDir, streams);
        }

        static string Arg(string flag)
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, flag);
            if (i < 0 || i + 1 >= args.Length) return null;
            return args[i + 1];
        }
    }
}
