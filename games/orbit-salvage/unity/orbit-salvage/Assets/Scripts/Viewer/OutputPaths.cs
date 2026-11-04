using System;
using System.IO;

namespace OrbitalSalvage.Viewer
{
    /// <summary>
    /// Build folder names under <c>games/orbit-salvage/output</c>.
    /// The Unity project directory is <c>games/orbit-salvage/unity/orbit-salvage</c>, two levels
    /// below the game folder, so the project-relative path is <c>../../output/&lt;name&gt;</c>.
    /// </summary>
    public static class OutputPaths
    {
        public const string BaseName = "orbit-salvage";

        /// <summary>
        /// <paramref name="variant"/> empty is the base folder <c>orbit-salvage</c>.
        /// Any other suffix is <c>orbit-salvage-&lt;variant&gt;</c>.
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
