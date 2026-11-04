using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.TestTools;

namespace OrbitalSalvage.Viewer.Tests
{
    /// <summary>
    /// Synthetic pbxproj fixtures only. They are modelled on the usual Unity Xcode layout
    /// (PBXNativeTarget, PBXAggregateTarget, XCConfigurationList, XCBuildConfiguration).
    /// They are not a project Unity generated, and this machine cannot export one.
    /// </summary>
    public class IosGameAssemblyBundleIdTests
    {
        const string NewId = "com.unity3d.gameassembly";
        const string OldId = "com.example.gameassembly";

        [Test]
        public void Replace_SetsGameAssemblyDebugAndRelease_AndLeavesOtherTargetsByteIdentical()
        {
            string original = BaseFixture();
            string updated = Apply(original, out object result);
            Assert.AreEqual("Updated", Kind(result));
            Assert.AreEqual(2, UpdatedCount(result));
            Assert.AreEqual(original.Replace(OldId, NewId), updated);
            Assert.AreEqual(Slice(original, "Unity-iPhone"), Slice(updated, "Unity-iPhone"));
            Assert.AreEqual(Slice(original, "UnityFramework"), Slice(updated, "UnityFramework"));
            Assert.AreEqual(Slice(original, "Aggregate"), Slice(updated, "Aggregate"));
            StringAssert.Contains("PRODUCT_BUNDLE_IDENTIFIER = " + NewId + ";", updated);
            StringAssert.Contains("PRODUCT_BUNDLE_IDENTIFIER = \"" + NewId + "\";", updated);
            StringAssert.Contains("PRODUCT_BUNDLE_IDENTIFIER = io.dev.maze;", updated);
            StringAssert.Contains("PRODUCT_BUNDLE_IDENTIFIER = com.unity3d.framework;", updated);
        }

        [Test]
        public void Replace_IsIdempotent()
        {
            string once = Apply(BaseFixture(), out object first);
            string twice = Apply(once, out object second);
            Assert.AreEqual("Updated", Kind(first));
            Assert.AreSame(once, twice);
            Assert.AreEqual("Unchanged", Kind(second));
            Assert.AreEqual(0, UpdatedCount(second));
        }

        [Test]
        public void Insert_AddsProductBundleIdentifierWhenMissing()
        {
            string stripped = StripGameAssemblyBundleLines(BaseFixture());
            StringAssert.DoesNotContain("PRODUCT_BUNDLE_IDENTIFIER = " + OldId, stripped);
            string updated = Apply(stripped, out object result);
            Assert.AreEqual("Updated", Kind(result));
            Assert.AreEqual(2, UpdatedCount(result));
            Assert.AreEqual(stripped, updated.Replace("\n\t\t\t\tPRODUCT_BUNDLE_IDENTIFIER = " + NewId + ";", ""));
            Assert.AreEqual(Slice(stripped, "Unity-iPhone"), Slice(updated, "Unity-iPhone"));
            Assert.AreEqual(Slice(stripped, "UnityFramework"), Slice(updated, "UnityFramework"));
            Assert.AreEqual(2, CountOf(updated, "PRODUCT_BUNDLE_IDENTIFIER = " + NewId + ";"));
            StringAssert.Contains("PRODUCT_BUNDLE_IDENTIFIER = io.dev.maze;", updated);
            StringAssert.Contains("PRODUCT_BUNDLE_IDENTIFIER = com.unity3d.framework;", updated);
        }

        [Test]
        public void Insert_QuotesWhenEverySiblingValueIsQuoted()
        {
            string text = Mini(Blocks(
                TargetBlock(Id('A', 1), "GameAssembly", "PBXNativeTarget", Id('B', 1), "GameAssembly", "GameAssembly", "com.apple.product-type.library.static"),
                ListBlock(Id('B', 1), "Build configuration list for PBXNativeTarget \"GameAssembly\"", Id('C', 1), Id('C', 2)),
                QuotedConfig(Id('C', 1), "Debug"),
                QuotedConfig(Id('C', 2), "Release")));
            string updated = Apply(text, out object result);
            Assert.AreEqual("Updated", Kind(result));
            Assert.AreEqual(2, UpdatedCount(result));
            StringAssert.Contains("PRODUCT_BUNDLE_IDENTIFIER = \"" + NewId + "\";", updated);
            Assert.IsFalse(updated.Contains("PRODUCT_BUNDLE_IDENTIFIER = " + NewId + ";"));
        }

        [Test]
        public void Crlf_LineEndingsArePreserved()
        {
            string crlf = BaseFixture().Replace("\n", "\r\n");
            string updated = Apply(crlf, out object result);
            Assert.AreEqual("Updated", Kind(result));
            Assert.AreEqual(crlf.Replace(OldId, NewId), updated);
            Assert.IsFalse(HasBareLf(updated));

            string stripped = StripGameAssemblyBundleLines(BaseFixture()).Replace("\n", "\r\n");
            string inserted = Apply(stripped, out object insertResult);
            Assert.AreEqual("Updated", Kind(insertResult));
            Assert.AreEqual(stripped, inserted.Replace("\r\n\t\t\t\tPRODUCT_BUNDLE_IDENTIFIER = " + NewId + ";", ""));
            Assert.IsFalse(HasBareLf(inserted));
        }

        [Test]
        public void NotFound_WhenGameAssemblyTargetIsAbsent_ReturnsOriginalAndListsNativeNames()
        {
            string text = WithoutGameAssemblyTarget(BaseFixture());
            Assert.IsFalse(text.Contains("name = GameAssembly;"));
            string updated = Apply(text, out object result);
            Assert.AreSame(text, updated);
            Assert.AreEqual("NotFound", Kind(result));
            CollectionAssert.AreEqual(new[] { "Unity-iPhone", "UnityFramework" }, Names(result));
        }

        [Test]
        public void NotFound_WhenNameIsOnlyInAComment()
        {
            string text = BaseFixture()
                .Replace("name = GameAssembly;", "name = Il2CppOutput;")
                .Replace("productName = GameAssembly;", "productName = Il2CppOutput;");
            StringAssert.Contains("/* GameAssembly */", text);
            StringAssert.Contains("PBXNativeTarget \"GameAssembly\"", text);
            string updated = Apply(text, out object result);
            Assert.AreSame(text, updated);
            Assert.AreEqual("NotFound", Kind(result));
            CollectionAssert.AreEqual(new[] { "Unity-iPhone", "UnityFramework", "Il2CppOutput" }, Names(result));
        }

        [Test]
        public void NotFound_WhenOnlyAggregateTargetIsNamedGameAssembly()
        {
            string text = BaseFixture()
                .Replace("name = GameAssembly;", "name = Il2CppOutput;")
                .Replace("productName = GameAssembly;", "productName = Il2CppOutput;")
                .Replace("name = Aggregate;", "name = GameAssembly;")
                .Replace("productName = Aggregate;", "productName = GameAssembly;");
            string updated = Apply(text, out object result);
            Assert.AreSame(text, updated);
            Assert.AreEqual("NotFound", Kind(result));
            CollectionAssert.AreEqual(new[] { "Unity-iPhone", "UnityFramework", "Il2CppOutput" }, Names(result));
            StringAssert.Contains("PRODUCT_BUNDLE_IDENTIFIER = com.example.aggregate;", updated);
        }

        [Test]
        public void Name_WinsOverAnotherTargetsProductName()
        {
            string text = Mini(Blocks(
                TargetBlock(Id('A', 1), "GameAssembly", "PBXNativeTarget", Id('B', 1), "GameAssembly", "SomethingElse", "com.apple.product-type.library.static"),
                TargetBlock(Id('A', 2), "Other", "PBXNativeTarget", Id('B', 2), "Other", "GameAssembly", "com.apple.product-type.library.static"),
                ListBlock(Id('B', 1), "list A", Id('C', 1), Id('C', 2)),
                ListBlock(Id('B', 2), "list B", Id('C', 3), Id('C', 4)),
                PlainConfig(Id('C', 1), "Debug", "com.example.from-name"),
                PlainConfig(Id('C', 2), "Release", "com.example.from-name"),
                PlainConfig(Id('C', 3), "Debug", "com.example.from-product"),
                PlainConfig(Id('C', 4), "Release", "com.example.from-product")));
            string updated = Apply(text, out object result);
            Assert.AreEqual("Updated", Kind(result));
            Assert.AreEqual(text.Replace("com.example.from-name", NewId), updated);
            StringAssert.Contains("PRODUCT_BUNDLE_IDENTIFIER = com.example.from-product;", updated);
        }

        [Test]
        public void ProductName_IsUsedWhenNoNativeNameMatches()
        {
            string text = Mini(Blocks(
                TargetBlock(Id('A', 1), "Lib", "PBXNativeTarget", Id('B', 1), "GameAssemblyLib", "GameAssembly", "com.apple.product-type.library.static"),
                ListBlock(Id('B', 1), "list", Id('C', 1), Id('C', 2)),
                PlainConfig(Id('C', 1), "Debug", OldId),
                PlainConfig(Id('C', 2), "Release", OldId)));
            string updated = Apply(text, out object result);
            Assert.AreEqual("Updated", Kind(result));
            Assert.AreEqual(2, UpdatedCount(result));
            Assert.AreEqual(text.Replace(OldId, NewId), updated);
        }

        [Test]
        public void CaseSensitive_LowercaseNameIsNotAMatch()
        {
            string text = Mini(Blocks(
                TargetBlock(Id('A', 1), "gameassembly", "PBXNativeTarget", Id('B', 1), "gameassembly", "gameassembly", "com.apple.product-type.library.static"),
                ListBlock(Id('B', 1), "list", Id('C', 1), Id('C', 2)),
                PlainConfig(Id('C', 1), "Debug", OldId),
                PlainConfig(Id('C', 2), "Release", OldId)));
            string updated = Apply(text, out object result);
            Assert.AreSame(text, updated);
            Assert.AreEqual("NotFound", Kind(result));
            CollectionAssert.AreEqual(new[] { "gameassembly" }, Names(result));
        }

        [Test]
        public void Ambiguous_TwoNativeTargetsNamedGameAssembly_DoesNotEdit()
        {
            string text = Mini(Blocks(
                TargetBlock(Id('A', 1), "GameAssembly", "PBXNativeTarget", Id('B', 1), "GameAssembly", "GameAssembly", "com.apple.product-type.library.static"),
                TargetBlock(Id('A', 2), "GameAssembly", "PBXNativeTarget", Id('B', 2), "GameAssembly", "GameAssembly", "com.apple.product-type.library.static"),
                ListBlock(Id('B', 1), "list A", Id('C', 1), Id('C', 2)),
                ListBlock(Id('B', 2), "list B", Id('C', 3), Id('C', 4)),
                PlainConfig(Id('C', 1), "Debug", "com.example.one"),
                PlainConfig(Id('C', 2), "Release", "com.example.one"),
                PlainConfig(Id('C', 3), "Debug", "com.example.two"),
                PlainConfig(Id('C', 4), "Release", "com.example.two")));
            string updated = Apply(text, out object result);
            Assert.AreSame(text, updated);
            Assert.AreEqual("Ambiguous", Kind(result));
            StringAssert.Contains("com.example.one", updated);
            StringAssert.Contains("com.example.two", updated);
        }

        [Test]
        public void Constants_StayNextToTheAppBundleId()
        {
            var ios = Find("OrbitalSalvage.Editor.IosExport");
            Assert.AreEqual("io.dev.maze", ios.GetField("BundleId").GetRawConstantValue());
            Assert.AreEqual(NewId, ios.GetField("GameAssemblyBundleId").GetRawConstantValue());
            Assert.AreEqual("GameAssembly", ios.GetField("GameAssemblyTargetName").GetRawConstantValue());
            Assert.AreEqual("15.0", ios.GetField("MinimumIosVersion").GetRawConstantValue());
        }

        [Test]
        public void PostProcess_IsALateBuildCallback()
        {
            var type = Find("OrbitalSalvage.Editor.IosXcodePostProcess");
            MethodInfo found = null;
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                object[] attrs = method.GetCustomAttributes(typeof(PostProcessBuildAttribute), false);
                if (attrs.Length == 0) continue;
                found = method;
                Assert.AreEqual(100, CallbackOrder(attrs[0]));
            }
            Assert.IsNotNull(found);
            Assert.AreEqual("OnPostProcessBuild", found.Name);
        }

        [Test]
        public void PostProcess_MissingFolderOrFile_DoesNotThrow()
        {
            string missing = Path.Combine(Path.GetTempPath(), "ios-ga-missing-" + Guid.NewGuid().ToString("N"));
            LogAssert.Expect(LogType.Warning, "[IosExport] project.pbxproj not found; no change made.");
            InvokePost(BuildTarget.iOS, missing);
            LogAssert.Expect(LogType.Warning, "[IosExport] project.pbxproj not found; no change made.");
            InvokePost(BuildTarget.iOS, null);

            string dir = Path.Combine(Path.GetTempPath(), "ios-ga-empty-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                LogAssert.Expect(LogType.Warning, "[IosExport] project.pbxproj not found; no change made.");
                InvokePost(BuildTarget.iOS, dir);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [Test]
        public void PostProcess_WritesUtf8WithoutBom_AndDoesNotRewriteWhenUnchanged()
        {
            string dir = Path.Combine(Path.GetTempPath(), "ios-ga-write-" + Guid.NewGuid().ToString("N"));
            string proj = Path.Combine(dir, "Unity-iPhone.xcodeproj");
            string pbx = Path.Combine(proj, "project.pbxproj");
            Directory.CreateDirectory(proj);
            try
            {
                string crlf = BaseFixture().Replace("\n", "\r\n");
                File.WriteAllText(pbx, crlf, new UTF8Encoding(false));
                LogAssert.Expect(LogType.Log, "[IosExport] GameAssembly bundle id set to com.unity3d.gameassembly in 2 configuration(s)");
                InvokePost(BuildTarget.iOS, dir);
                byte[] bytes = File.ReadAllBytes(pbx);
                Assert.IsFalse(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
                string text = new UTF8Encoding(false).GetString(bytes);
                Assert.AreEqual(crlf.Replace(OldId, NewId), text);
                Assert.IsFalse(HasBareLf(text));

                File.SetAttributes(pbx, FileAttributes.ReadOnly);
                LogAssert.Expect(LogType.Log, "[IosExport] GameAssembly bundle id already com.unity3d.gameassembly. No change made.");
                InvokePost(BuildTarget.iOS, dir);
                Assert.AreEqual(text, new UTF8Encoding(false).GetString(File.ReadAllBytes(pbx)));
            }
            finally
            {
                if (File.Exists(pbx)) File.SetAttributes(pbx, FileAttributes.Normal);
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [Test]
        public void PostProcess_SkipsNonIosBuilds()
        {
            string dir = Path.Combine(Path.GetTempPath(), "ios-ga-win-" + Guid.NewGuid().ToString("N"));
            string proj = Path.Combine(dir, "Unity-iPhone.xcodeproj");
            string pbx = Path.Combine(proj, "project.pbxproj");
            Directory.CreateDirectory(proj);
            try
            {
                string original = BaseFixture();
                File.WriteAllText(pbx, original, new UTF8Encoding(false));
                InvokePost(BuildTarget.StandaloneWindows64, dir);
                Assert.AreEqual(original, File.ReadAllText(pbx));
            }
            finally
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [Test]
        public void PostProcess_UsesTheOnlyXcodeprojWhenTheNameIsNotUnityIPhone()
        {
            string dir = Path.Combine(Path.GetTempPath(), "ios-ga-one-" + Guid.NewGuid().ToString("N"));
            string proj = Path.Combine(dir, "CustomExport.xcodeproj");
            string pbx = Path.Combine(proj, "project.pbxproj");
            Directory.CreateDirectory(proj);
            try
            {
                File.WriteAllText(pbx, BaseFixture(), new UTF8Encoding(false));
                LogAssert.Expect(LogType.Log, "[IosExport] GameAssembly bundle id set to com.unity3d.gameassembly in 2 configuration(s)");
                InvokePost(BuildTarget.iOS, dir);
                StringAssert.Contains(NewId, File.ReadAllText(pbx));
            }
            finally
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [Test]
        public void PostProcess_PrefersUnityIPhone_AndLeavesASecondProjectUntouched()
        {
            string dir = Path.Combine(Path.GetTempPath(), "ios-ga-two-" + Guid.NewGuid().ToString("N"));
            string phone = Path.Combine(dir, "Unity-iPhone.xcodeproj", "project.pbxproj");
            string other = Path.Combine(dir, "Other.xcodeproj", "project.pbxproj");
            Directory.CreateDirectory(Path.GetDirectoryName(phone));
            Directory.CreateDirectory(Path.GetDirectoryName(other));
            try
            {
                string original = BaseFixture();
                File.WriteAllText(phone, original, new UTF8Encoding(false));
                File.WriteAllText(other, original, new UTF8Encoding(false));
                LogAssert.Expect(LogType.Log, "[IosExport] GameAssembly bundle id set to com.unity3d.gameassembly in 2 configuration(s)");
                InvokePost(BuildTarget.iOS, dir);
                Assert.AreEqual(original.Replace(OldId, NewId), File.ReadAllText(phone));
                Assert.AreEqual(original, File.ReadAllText(other));
            }
            finally
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [Test]
        public void PostProcess_TwoUnnamedProjects_DoesNotThrowOrEdit()
        {
            string dir = Path.Combine(Path.GetTempPath(), "ios-ga-ambiguous-" + Guid.NewGuid().ToString("N"));
            string a = Path.Combine(dir, "A.xcodeproj", "project.pbxproj");
            string b = Path.Combine(dir, "B.xcodeproj", "project.pbxproj");
            Directory.CreateDirectory(Path.GetDirectoryName(a));
            Directory.CreateDirectory(Path.GetDirectoryName(b));
            try
            {
                string original = BaseFixture();
                File.WriteAllText(a, original, new UTF8Encoding(false));
                File.WriteAllText(b, original, new UTF8Encoding(false));
                LogAssert.Expect(LogType.Warning, "[IosExport] project.pbxproj not found; no change made.");
                InvokePost(BuildTarget.iOS, dir);
                Assert.AreEqual(original, File.ReadAllText(a));
                Assert.AreEqual(original, File.ReadAllText(b));
            }
            finally
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        static string Apply(string text, out object result)
        {
            var type = Find("OrbitalSalvage.Editor.PbxprojBundleId");
            var method = type.GetMethod("SetTargetBundleId", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method, "SetTargetBundleId");
            var args = new object[] { text, "GameAssembly", NewId, null };
            try
            {
                var updated = (string)method.Invoke(null, args);
                result = args[3];
                return updated;
            }
            catch (TargetInvocationException ex)
            {
                Assert.Fail(ex.InnerException == null ? ex.ToString() : ex.InnerException.ToString());
                result = null;
                return null;
            }
        }

        static void InvokePost(BuildTarget target, string path)
        {
            var type = Find("OrbitalSalvage.Editor.IosXcodePostProcess");
            var method = type.GetMethod("OnPostProcessBuild", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "OnPostProcessBuild");
            try
            {
                method.Invoke(null, new object[] { target, path });
            }
            catch (TargetInvocationException ex)
            {
                Assert.Fail(ex.InnerException == null ? ex.ToString() : ex.InnerException.ToString());
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

        static string Kind(object result)
        {
            return result.GetType().GetProperty("Kind").GetValue(result).ToString();
        }

        static int UpdatedCount(object result)
        {
            return (int)result.GetType().GetProperty("ConfigurationsUpdated").GetValue(result);
        }

        static string[] Names(object result)
        {
            return (string[])result.GetType().GetProperty("NativeTargetNames").GetValue(result);
        }

        static int CallbackOrder(object attr)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var prop = attr.GetType().GetProperty("callbackOrder", flags);
            if (prop == null && attr.GetType().BaseType != null)
                prop = attr.GetType().BaseType.GetProperty("callbackOrder", flags);
            if (prop != null) return (int)prop.GetValue(attr, null);
            var field = attr.GetType().GetField("m_CallbackOrder", flags);
            if (field == null && attr.GetType().BaseType != null)
                field = attr.GetType().BaseType.GetField("m_CallbackOrder", flags);
            Assert.IsNotNull(field, "callback order");
            return (int)field.GetValue(attr);
        }

        static string Slice(string text, string name)
        {
            string start = "/* SLICE " + name + " */";
            string end = "/* /SLICE " + name + " */";
            int i = text.IndexOf(start, StringComparison.Ordinal);
            int j = text.IndexOf(end, StringComparison.Ordinal);
            Assert.GreaterOrEqual(i, 0, name);
            Assert.Greater(j, i, name);
            return text.Substring(i, j + end.Length - i);
        }

        static string StripGameAssemblyBundleLines(string text)
        {
            return text
                .Replace("\t\t\t\tPRODUCT_BUNDLE_IDENTIFIER = " + OldId + ";\n", "")
                .Replace("\t\t\t\tPRODUCT_BUNDLE_IDENTIFIER = \"" + OldId + "\";\n", "");
        }

        static string WithoutGameAssemblyTarget(string text)
        {
            string start = "/* TARGET GameAssembly */";
            string end = "/* /TARGET GameAssembly */";
            int i = text.IndexOf(start, StringComparison.Ordinal);
            int j = text.IndexOf(end, StringComparison.Ordinal);
            Assert.GreaterOrEqual(i, 0);
            Assert.Greater(j, i);
            return text.Substring(0, i) + text.Substring(j + end.Length);
        }

        static int CountOf(string text, string needle)
        {
            int count = 0;
            int i = 0;
            while (true)
            {
                i = text.IndexOf(needle, i, StringComparison.Ordinal);
                if (i < 0) return count;
                count++;
                i += needle.Length;
            }
        }

        static bool HasBareLf(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != '\n') continue;
                if (i == 0 || text[i - 1] != '\r') return true;
            }
            return false;
        }

        static string Id(char prefix, int n)
        {
            return prefix + n.ToString("D23");
        }

        static string BaseFixture()
        {
            var lines = new List<string>();
            lines.Add("// !$*UTF8*$!");
            lines.Add("// SYNTHETIC pbxproj. Not a Unity-generated Xcode project.");
            lines.Add("// Modelled on PBXNativeTarget, PBXAggregateTarget, XCConfigurationList, and XCBuildConfiguration.");
            lines.Add("{");
            lines.Add("\tarchiveVersion = 1;");
            lines.Add("\tclasses = {");
            lines.Add("\t};");
            lines.Add("\tobjectVersion = 56;");
            lines.Add("\tobjects = {");
            lines.Add("");
            lines.Add("/* Begin PBXNativeTarget section */");
            lines.Add(TargetBlock(Id('A', 1), "Unity-iPhone", "PBXNativeTarget", Id('B', 1), "Unity-iPhone", "Unity-iPhone", "com.apple.product-type.application"));
            lines.Add(TargetBlock(Id('A', 2), "UnityFramework", "PBXNativeTarget", Id('B', 2), "UnityFramework", "UnityFramework", "com.apple.product-type.framework"));
            lines.Add("/* TARGET GameAssembly */");
            lines.Add(TargetBlock(Id('A', 3), "GameAssembly", "PBXNativeTarget", Id('B', 3), "GameAssembly", "GameAssembly", "com.apple.product-type.library.static"));
            lines.Add("/* /TARGET GameAssembly */");
            lines.Add("/* End PBXNativeTarget section */");
            lines.Add("");
            lines.Add("/* Begin PBXAggregateTarget section */");
            lines.Add(TargetBlock(Id('A', 4), "Aggregate", "PBXAggregateTarget", Id('B', 4), "Aggregate", "Aggregate", null));
            lines.Add("/* End PBXAggregateTarget section */");
            lines.Add("");
            lines.Add(ShellBlock(Id('E', 1)));
            lines.Add("");
            lines.Add("/* Begin XCConfigurationList section */");
            lines.Add(ListBlock(Id('B', 1), "Build configuration list for PBXNativeTarget \"Unity-iPhone\"", Id('C', 1), Id('C', 2)));
            lines.Add(ListBlock(Id('B', 2), "Build configuration list for PBXNativeTarget \"UnityFramework\"", Id('C', 3), Id('C', 4)));
            lines.Add(ListBlock(Id('B', 3), "Build configuration list for PBXNativeTarget \"GameAssembly\"", Id('C', 5), Id('C', 6)));
            lines.Add(ListBlock(Id('B', 4), "Build configuration list for PBXAggregateTarget \"Aggregate\"", Id('C', 7), Id('C', 8)));
            lines.Add("/* End XCConfigurationList section */");
            lines.Add("");
            lines.Add("/* SLICE Unity-iPhone */");
            lines.Add(PlainConfig(Id('C', 1), "Debug", "io.dev.maze", "PRODUCT_NAME = \"$(PRODUCT_NAME_APP)\";"));
            lines.Add(PlainConfig(Id('C', 2), "Release", "io.dev.maze", "PRODUCT_NAME = \"$(PRODUCT_NAME_APP)\";"));
            lines.Add("/* /SLICE Unity-iPhone */");
            lines.Add("/* SLICE UnityFramework */");
            lines.Add(PlainConfig(Id('C', 3), "Debug", "com.unity3d.framework", "PRODUCT_NAME = UnityFramework;"));
            lines.Add(PlainConfig(Id('C', 4), "Release", "com.unity3d.framework", "PRODUCT_NAME = UnityFramework;"));
            lines.Add("/* /SLICE UnityFramework */");
            lines.Add("/* SLICE GameAssembly */");
            lines.Add(PlainConfig(Id('C', 5), "Debug", OldId, "PRODUCT_NAME = GameAssembly;"));
            lines.Add(QuotedBundleConfig(Id('C', 6), "Release", OldId));
            lines.Add("/* /SLICE GameAssembly */");
            lines.Add("/* SLICE Aggregate */");
            lines.Add(PlainConfig(Id('C', 7), "Debug", "com.example.aggregate", "PRODUCT_NAME = Aggregate;"));
            lines.Add(PlainConfig(Id('C', 8), "Release", "com.example.aggregate", "PRODUCT_NAME = Aggregate;"));
            lines.Add("/* /SLICE Aggregate */");
            lines.Add("\t};");
            lines.Add("\trootObject = " + Id('D', 1) + " /* Project object */;");
            lines.Add("}");
            return string.Join("\n", lines) + "\n";
        }

        static string Blocks(params string[] parts)
        {
            return string.Join("\n", parts);
        }

        static string Mini(string objectsInner)
        {
            return "// SYNTHETIC pbxproj. Not a Unity-generated Xcode project.\n{\n\tarchiveVersion = 1;\n\tclasses = {\n\t};\n\tobjects = {\n" +
                objectsInner + "\n\t};\n\trootObject = " + Id('D', 1) + " /* Project object */;\n}\n";
        }

        static string TargetBlock(string id, string comment, string isa, string listId, string name, string product, string productType)
        {
            var lines = new List<string>();
            lines.Add("\t\t" + id + " /* " + comment + " */ = {");
            lines.Add("\t\t\tisa = " + isa + ";");
            lines.Add("\t\t\tbuildConfigurationList = " + listId + " /* Build configuration list for " + isa + " \"" + comment + "\" */;");
            lines.Add("\t\t\tname = " + name + ";");
            lines.Add("\t\t\tproductName = " + product + ";");
            if (productType != null)
                lines.Add("\t\t\tproductType = \"" + productType + "\";");
            lines.Add("\t\t};");
            return string.Join("\n", lines);
        }

        static string ListBlock(string id, string comment, string debugId, string releaseId)
        {
            return string.Join("\n", new[]
            {
                "\t\t" + id + " /* " + comment + " */ = {",
                "\t\t\tisa = XCConfigurationList;",
                "\t\t\tbuildConfigurations = (",
                "\t\t\t\t" + debugId + " /* Debug */,",
                "\t\t\t\t" + releaseId + " /* Release */,",
                "\t\t\t);",
                "\t\t\tdefaultConfigurationIsVisible = 0;",
                "\t\t\tdefaultConfigurationName = Release;",
                "\t\t};"
            });
        }

        static string PlainConfig(string id, string name, string bundleId)
        {
            return PlainConfig(id, name, bundleId, "PRODUCT_NAME = " + name + ";");
        }

        static string PlainConfig(string id, string name, string bundleId, string productLine)
        {
            return string.Join("\n", new[]
            {
                "\t\t" + id + " /* " + name + " */ = {",
                "\t\t\tisa = XCBuildConfiguration;",
                "\t\t\tbuildSettings = {",
                "\t\t\t\tPRODUCT_BUNDLE_IDENTIFIER = " + bundleId + ";",
                "\t\t\t\t" + productLine,
                "\t\t\t};",
                "\t\t\tname = " + name + ";",
                "\t\t};"
            });
        }

        static string QuotedBundleConfig(string id, string name, string bundleId)
        {
            return string.Join("\n", new[]
            {
                "\t\t" + id + " /* " + name + " */ = {",
                "\t\t\tisa = XCBuildConfiguration;",
                "\t\t\tbuildSettings = {",
                "\t\t\t\tPRODUCT_BUNDLE_IDENTIFIER = \"" + bundleId + "\";",
                "\t\t\t\tPRODUCT_NAME = " + name + ";",
                "\t\t\t};",
                "\t\t\tname = " + name + ";",
                "\t\t};"
            });
        }

        static string QuotedConfig(string id, string name)
        {
            return string.Join("\n", new[]
            {
                "\t\t" + id + " /* " + name + " */ = {",
                "\t\t\tisa = XCBuildConfiguration;",
                "\t\t\tbuildSettings = {",
                "\t\t\t\tCODE_SIGN_STYLE = \"Automatic\";",
                "\t\t\t\tPRODUCT_NAME = \"" + name + "\";",
                "\t\t\t};",
                "\t\t\tname = " + name + ";",
                "\t\t};"
            });
        }

        static string ShellBlock(string id)
        {
            return string.Join("\n", new[]
            {
                "\t\t" + id + " /* Run Script */ = {",
                "\t\t\tisa = PBXShellScriptBuildPhase;",
                "\t\t\tname = \"Run Script\";",
                "\t\t\tshellScript = \"if [ -n \\\"$X\\\" ]; then echo \\\"/* GameAssembly */ { }\\\"; fi\";",
                "\t\t\tshowEnvVarsInLog = 0;",
                "\t\t};"
            });
        }
    }
}
