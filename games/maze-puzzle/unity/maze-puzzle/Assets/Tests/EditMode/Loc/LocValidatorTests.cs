using System.Collections.Generic;
using NUnit.Framework;

namespace OrbitalSalvage.Loc.Tests
{
    public class LocValidatorTests : LocTestBase
    {
        [Test]
        public void Validator_ReportsFindings()
        {
            var english = new LocTable();
            english.Code = "en";
            english.Spec["Hello"] = "Hello";
            english.Spec["lost"] = "Lost";
            english.Spec["gone"] = "{0} {1}";
            english.Str["ok"] = "OK";
            english.Str["bad"] = "ab";

            var german = new LocTable();
            german.Code = "de";
            german.Spec["Hello"] = "12345678";
            german.Spec["gone"] = "{0}";
            german.Spec["extra"] = "x";
            german.Spec["Boom"] = "1234567890123";
            english.Spec["Boom"] = "Hello";
            german.Str["ok"] = " OK";
            german.Str["bad"] = "a\nb";

            List<LocFinding> findings = LocValidator.Validate(english, german);
            Assert.IsTrue(Has(findings, LocSeverity.Warning, "length", "spec:Hello"));
            Assert.IsTrue(Has(findings, LocSeverity.Error, "length", "spec:Boom"));
            Assert.IsTrue(Has(findings, LocSeverity.Error, "missing", "spec:lost"));
            Assert.IsTrue(Has(findings, LocSeverity.Error, "placeholder", "spec:gone"));
            Assert.IsTrue(Has(findings, LocSeverity.Error, "extra", "spec:extra"));
            Assert.IsTrue(Has(findings, LocSeverity.Warning, "whitespace", "str:ok"));
            Assert.IsTrue(Has(findings, LocSeverity.Error, "control", "str:bad"));

            string line = Find(findings, "spec:lost").ToString();
            StringAssert.StartsWith("error\tmissing\tspec:lost\t", line);

            var zhEnglish = new LocTable();
            zhEnglish.Spec["Hi"] = "Hi";
            var zh = new LocTable();
            zh.Code = "zh-Hans";
            zh.Spec["Hi"] = new string('字', 40);
            List<LocFinding> cjk = LocValidator.Validate(zhEnglish, zh);
            for (int i = 0; i < cjk.Count; i++) Assert.AreNotEqual("length", cjk[i].Kind);
        }

        static bool Has(List<LocFinding> findings, LocSeverity severity, string kind, string key)
        {
            LocFinding finding = Find(findings, key);
            if (finding == null || finding.Kind != kind) 
            {
                for (int i = 0; i < findings.Count; i++)
                {
                    if (findings[i].Key == key && findings[i].Kind == kind && findings[i].Severity == severity) return true;
                }
                return false;
            }
            return finding.Severity == severity && finding.Kind == kind;
        }

        static LocFinding Find(List<LocFinding> findings, string key)
        {
            for (int i = 0; i < findings.Count; i++)
            {
                if (findings[i].Key == key) return findings[i];
            }
            return null;
        }
    }
}
