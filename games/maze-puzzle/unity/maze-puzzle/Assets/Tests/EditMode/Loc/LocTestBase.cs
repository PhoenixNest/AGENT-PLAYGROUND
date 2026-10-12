using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace OrbitalSalvage.Loc.Tests
{
    public abstract class LocTestBase
    {
        [SetUp]
        public void ResetLocBefore()
        {
            Loc.Reset(null);
        }

        [TearDown]
        public void ResetLocAfter()
        {
            Loc.Reset(null);
        }
    }

    public sealed class MemoryTables : ILocTableLoader
    {
        public readonly Dictionary<string, string> Files = new Dictionary<string, string>(StringComparer.Ordinal);

        public bool TryLoad(string code, out string json)
        {
            return Files.TryGetValue(code, out json);
        }

        public void Add(string code, string specBody, string strBody)
        {
            string name = code;
            LanguageInfo info;
            if (LanguageRegistry.TryGet(code, out info)) name = info.Endonym;
            Files[code] = "{\"meta\":{\"code\":\"" + code + "\",\"name\":\"" + name + "\",\"version\":1,\"source\":\"en\"},\"spec\":{"
                + (specBody ?? "") + "},\"str\":{" + (strBody ?? "") + "}}";
        }
    }
}
