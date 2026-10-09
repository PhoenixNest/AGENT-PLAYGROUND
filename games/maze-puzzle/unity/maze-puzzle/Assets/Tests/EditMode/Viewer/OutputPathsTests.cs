using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using OrbitalSalvage.Viewer;

namespace OrbitalSalvage.Viewer.Tests
{
    public class OutputPathsTests
    {
        [Test]
        public void For_UsesBaseNameAndOptionalSuffix()
        {
            Assert.AreEqual("maze-puzzle", OutputPaths.For(null));
            Assert.AreEqual("maze-puzzle", OutputPaths.For(""));
            Assert.AreEqual("maze-puzzle-simulator", OutputPaths.For("simulator"));
            Assert.AreEqual("maze-puzzle-windows", OutputPaths.For("windows"));
            Assert.Throws<ArgumentException>(() => OutputPaths.For("sim/extra"));
        }

        [Test]
        public void Absolute_ResolvesTwoLevelsAboveTheUnityProject()
        {
            string root = Path.Combine(Path.GetTempPath(), "menu-ios-paths", "games", "maze-puzzle", "unity", "maze-puzzle");
            Assert.AreEqual(
                Path.GetFullPath(Path.Combine(root, "..", "..", "output", "maze-puzzle")),
                OutputPaths.Absolute(root, null));
            Assert.AreEqual(
                Path.GetFullPath(Path.Combine(root, "..", "..", "output", "maze-puzzle-simulator")),
                OutputPaths.Absolute(root, "simulator"));
            Assert.AreEqual(
                Path.GetFullPath(Path.Combine(root, "..", "..", "output", "maze-puzzle-windows")),
                OutputPaths.Absolute(root, "windows"));
            Assert.AreEqual("../../output/maze-puzzle-windows", OutputPaths.RelativeToUnityProject("windows"));
        }

        [Test]
        public void PlayBuild_WritesTheWindowsVariant()
        {
            var playBuild = Find("OrbitalSalvage.Editor.PlayBuild");
            string output = (string)playBuild.GetField("OutputRelative").GetRawConstantValue();
            Assert.AreEqual(OutputPaths.RelativeToUnityProject("windows") + "/OrbitalSalvage.exe", output);
        }

        static Type Find(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType(fullName);
                if (type != null) return type;
            }
            Assert.Fail("missing " + fullName);
            return null;
        }
    }
}
