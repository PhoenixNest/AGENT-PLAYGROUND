using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using OrbitalSalvage.Viewer;
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

        [Test]
        public void Confirm_ResetsARunningAppAndDeletesTheTempProfile()
        {
            string dir = Path.Combine(Path.GetTempPath(), "menu-ios-profile-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, "orbital-salvage-profile.json");
            File.WriteAllText(file, "{\"keep\":true}");
            var previous = ParityViewer.RequestReset;
            int resets = 0;
            ParityViewer.RequestReset = () => resets++;
            ParityViewerMenu.ProfilePathOverride = () => file;
            ParityViewerMenu.ConfirmReset = () => true;
            try
            {
                ParityViewerMenu.ResetProfile();
                Assert.AreEqual(1, resets, "confirm must reset the running app before deleting the file");
                Assert.IsFalse(File.Exists(file));
            }
            finally
            {
                ParityViewer.RequestReset = previous;
                ParityViewerMenu.ProfilePathOverride = null;
                ParityViewerMenu.ConfirmReset = null;
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }
    }

    public class IosExportTests
    {
        [Test]
        public void IosMenu_FollowsWhetherTheModuleIsInstalled()
        {
            bool supported = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS);
            var ios = Find("OrbitalSalvage.Editor.IosExport");
            bool module = (bool)ios.GetProperty("ModuleInstalled").GetValue(null);
            Assert.AreEqual(supported, module);
            bool simulatorOn = (bool)ios.GetMethod("ValidateSimulator").Invoke(null, null);
            bool deviceOn = (bool)ios.GetMethod("ValidateDevice").Invoke(null, null);
            Assert.AreEqual(module, simulatorOn);
            Assert.AreEqual(module, deviceOn);
            string simulator = (string)ios.GetField("SimulatorMenu").GetRawConstantValue();
            string device = (string)ios.GetField("DeviceMenu").GetRawConstantValue();
            Assert.AreEqual(module, Menu.GetEnabled(simulator));
            Assert.AreEqual(module, Menu.GetEnabled(device));
        }

        [Test]
        public void PlayerSettings_TargetsIphoneOnly()
        {
            Assert.AreEqual(iOSTargetDevice.iPhoneOnly, PlayerSettings.iOS.targetDevice);
        }

        [Test]
        public void ExportCode_DoesNotReenableIpad()
        {
            string editor = Path.GetFullPath(Path.Combine(Application.dataPath, "Scripts", "Editor"));
            foreach (string name in new[] { "IosExport.cs", "IosXcodePostProcess.cs" })
            {
                string text = File.ReadAllText(Path.Combine(editor, name));
                StringAssert.DoesNotContain("targetDevice", text, name);
                StringAssert.DoesNotContain("iPhoneAndiPad", text, name);
                StringAssert.DoesNotContain("iPadOnly", text, name);
                StringAssert.DoesNotContain("TARGETED_DEVICE_FAMILY", text, name);
            }
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
