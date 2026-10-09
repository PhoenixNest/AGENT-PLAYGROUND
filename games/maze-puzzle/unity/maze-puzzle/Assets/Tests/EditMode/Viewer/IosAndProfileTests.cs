using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using OrbitalSalvage.Viewer.Editor;
using UnityEditor;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    public class ProfileResetTests
    {
        [Test]
        public void Cancel_DoesNotDeleteTheProfileFile()
        {
            string dir = Path.Combine(Path.GetTempPath(), "menu-ios-profile-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, "orbital-salvage-profile.json");
            File.WriteAllText(file, "{\"keep\":true}");
            ParityViewerMenu.ProfilePathOverride = () => file;
            ParityViewerMenu.ConfirmReset = () => false;
            try
            {
                ParityViewerMenu.ResetProfile();
                Assert.IsTrue(File.Exists(file));
                Assert.AreEqual("{\"keep\":true}", File.ReadAllText(file));
            }
            finally
            {
                ParityViewerMenu.ProfilePathOverride = null;
                ParityViewerMenu.ConfirmReset = null;
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }
    }

    public class IosExportTests
    {
        [Test]
        public void IosItems_AreDisabledWhenTheModuleIsMissing()
        {
            bool supported = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS);
            Assert.IsFalse(supported, "this Editor does not have the iOS Build Support module");
            var ios = Find("OrbitalSalvage.Editor.IosExport");
            Assert.IsFalse((bool)ios.GetProperty("ModuleInstalled").GetValue(null));
            Assert.IsFalse((bool)ios.GetMethod("ValidateSimulator").Invoke(null, null));
            Assert.IsFalse((bool)ios.GetMethod("ValidateDevice").Invoke(null, null));
            string simulator = (string)ios.GetField("SimulatorMenu").GetRawConstantValue();
            string device = (string)ios.GetField("DeviceMenu").GetRawConstantValue();
            Assert.IsFalse(Menu.GetEnabled(simulator));
            Assert.IsFalse(Menu.GetEnabled(device));
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
