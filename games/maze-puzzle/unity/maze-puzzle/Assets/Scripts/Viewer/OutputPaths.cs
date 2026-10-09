using System;
using System.IO;

namespace OrbitalSalvage.Viewer
{
    /// <summary>
    /// Build folder names under <c>games/maze-puzzle/output</c>.
    /// The Unity project directory is <c>games/maze-puzzle/unity/maze-puzzle</c>, two levels
    /// below the game folder, so the project-relative path is <c>../../output/&lt;name&gt;</c>.
    /// </summary>
    public static class OutputPaths
    {
        public const string BaseName = "maze-puzzle";

        /// <summary>
        /// <paramref name="variant"/> empty is the base folder <c>maze-puzzle</c>.
        /// Any other suffix is <c>maze-puzzle-&lt;variant&gt;</c>.
        /// </summary>
        public static string For(string variant)
        {
            if (string.IsNullOrEmpty(variant)) return BaseName;
            for (int i = 0; i < variant.Length; i++)
            {
                char c = variant[i];
                if (c == '/' || c == '\\' || c == ':' || c == '.')
                    throw new ArgumentException("variant must be a single suffix", nameof(variant));
            }
            return BaseName + "-" + variant;
        }

        public static string RelativeToUnityProject(string variant)
        {
            return "../../output/" + For(variant);
        }

        public static string Absolute(string unityProjectRoot, string variant)
        {
            if (string.IsNullOrEmpty(unityProjectRoot)) throw new ArgumentException("unityProjectRoot");
            string relative = RelativeToUnityProject(variant).Replace('/', Path.DirectorySeparatorChar);
            return Path.GetFullPath(Path.Combine(unityProjectRoot, relative));
        }
    }
}
