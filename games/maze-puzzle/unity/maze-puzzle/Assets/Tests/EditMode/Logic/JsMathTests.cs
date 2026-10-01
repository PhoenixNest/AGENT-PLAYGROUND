using System;
using System.Text;
using NUnit.Framework;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Logic.Tests
{
    public class JsMathTests
    {
        private static JsonValue _doc;

        private static JsonValue Doc
        {
            get
            {
                if (_doc == null) _doc = Fixtures.Load("jsmath-vectors.json");
                return _doc;
            }
        }

        private static void RunUnary(string name, Func<double, double> fn)
        {
            JsonValue list = Doc["unary"][name];
            int bad = 0;
            var report = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                string[] p = list[i].String.Split(':');
                double x = Fixtures.D(p[0]);
                double actual = fn(x);
                bool ok = p[1] == "nan" ? double.IsNaN(actual) : Fixtures.SameBits(Fixtures.D(p[1]), actual);
                if (!ok)
                {
                    bad++;
                    if (bad <= 5)
                    {
                        report.Append(name).Append("(").Append(p[0]).Append(") expected ").Append(p[1]).Append(" actual ").Append(Fixtures.Hex(actual)).Append('\n');
                    }
                }
            }
            Assert.AreEqual(0, bad, name + ": " + bad + "/" + list.Count + " vectors differ\n" + report);
        }

        private static void RunBinary(string name, Func<double, double, double> fn)
        {
            JsonValue list = Doc["binary"][name];
            int bad = 0;
            var report = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                string[] p = list[i].String.Split(':');
                double a = Fixtures.D(p[0]);
                double b = Fixtures.D(p[1]);
                double actual = fn(a, b);
                bool ok = p[2] == "nan" ? double.IsNaN(actual) : Fixtures.SameBits(Fixtures.D(p[2]), actual);
                if (!ok)
                {
                    bad++;
                    if (bad <= 5)
                    {
                        report.Append(name).Append("(").Append(p[0]).Append(", ").Append(p[1]).Append(") expected ").Append(p[2]).Append(" actual ").Append(Fixtures.Hex(actual)).Append('\n');
                    }
                }
            }
            Assert.AreEqual(0, bad, name + ": " + bad + "/" + list.Count + " vectors differ\n" + report);
        }

        [Test]
        public void Sin_BitExact()
        {
            RunUnary("sin", JsMath.Sin);
        }

        [Test]
        public void Cos_BitExact()
        {
            RunUnary("cos", JsMath.Cos);
        }

        [Test]
        public void Atan_BitExact()
        {
            RunUnary("atan", JsMath.Atan);
        }

        [Test]
        public void Atan2_BitExact()
        {
            RunBinary("atan2", JsMath.Atan2);
        }

        [Test]
        public void Pow_ParityArguments_BitExact()
        {
            JsonValue list = Doc["binary"]["pow"];
            int checkedCount = 0;
            for (int i = 0; i < list.Count; i++)
            {
                string[] p = list[i].String.Split(':');
                double a = Fixtures.D(p[0]);
                double b = Fixtures.D(p[1]);
                if (a != 0.94 || (b != 0.016 * 60 && b != 0.96)) continue;
                checkedCount++;
                Assert.IsTrue(Fixtures.SameBits(Fixtures.D(p[2]), JsMath.Pow(a, b)), "pow(0.94, " + p[1] + ")");
            }
            Assert.Greater(checkedCount, 0);
        }

        [Test]
        public void Pow_General_WithinOneUlp()
        {
            JsonValue list = Doc["binary"]["pow"];
            int exact = 0;
            int total = 0;
            for (int i = 0; i < list.Count; i++)
            {
                string[] p = list[i].String.Split(':');
                double expected = p[2] == "nan" ? double.NaN : Fixtures.D(p[2]);
                double actual = JsMath.Pow(Fixtures.D(p[0]), Fixtures.D(p[1]));
                if (double.IsNaN(expected)) { Assert.IsTrue(double.IsNaN(actual), "pow(" + p[0] + "," + p[1] + ") expected NaN"); continue; }
                total++;
                if (Fixtures.SameBits(expected, actual)) { exact++; continue; }
                Assert.LessOrEqual(Fixtures.UlpDistance(expected, actual), 1, "pow(" + p[0] + "," + p[1] + ")");
            }
            Assert.GreaterOrEqual((double)exact / total, 0.995, "pow exact-match rate");
        }

        [Test]
        public void Hypot_BitExact()
        {
            RunBinary("hypot", (a, b) => JsMath.Hypot(a, b));
        }

        [Test]
        public void HypotVariadic_MatchesTwoArgForm()
        {
            JsonValue list = Doc["binary"]["hypot"];
            for (int i = 0; i < list.Count; i++)
            {
                string[] p = list[i].String.Split(':');
                double a = Fixtures.D(p[0]);
                double b = Fixtures.D(p[1]);
                Assert.IsTrue(Fixtures.SameBits(JsMath.Hypot(a, b), JsMath.Hypot(new[] { a, b })), "hypot(" + p[0] + "," + p[1] + ")");
            }
        }

        [Test]
        public void Sqrt_BitExact()
        {
            RunUnary("sqrt", JsMath.Sqrt);
        }

        [Test]
        public void Floor_BitExact()
        {
            RunUnary("floor", JsMath.Floor);
        }

        [Test]
        public void Ceil_BitExact()
        {
            RunUnary("ceil", JsMath.Ceil);
        }

        [Test]
        public void Trunc_BitExact()
        {
            RunUnary("trunc", JsMath.Trunc);
        }

        [Test]
        public void Round_BitExact()
        {
            RunUnary("round", JsMath.Round);
        }

        [Test]
        public void Sign_BitExact()
        {
            RunUnary("sign", JsMath.Sign);
        }

        [Test]
        public void Abs_BitExact()
        {
            RunUnary("abs", JsMath.Abs);
        }

        [Test]
        public void Min_BitExact()
        {
            RunBinary("min", JsMath.Min);
        }

        [Test]
        public void Max_BitExact()
        {
            RunBinary("max", JsMath.Max);
        }

        [Test]
        public void Mod_BitExact()
        {
            RunBinary("mod", JsMath.Mod);
        }

        [Test]
        public void ToInt32_Exact()
        {
            RunUnary("toInt32", x => (double)JsMath.ToInt32(x));
        }

        [Test]
        public void ToUint32_Exact()
        {
            RunUnary("toUint32", x => (double)JsMath.ToUint32(x));
        }

        [Test]
        public void Imul_Exact()
        {
            JsonValue list = Doc["imul"];
            for (int i = 0; i < list.Count; i++)
            {
                string[] p = list[i].String.Split(':');
                int a = int.Parse(p[0]);
                int b = int.Parse(p[1]);
                Assert.AreEqual(int.Parse(p[2]), JsMath.Imul(a, b), "imul(" + p[0] + "," + p[1] + ")");
            }
        }

        [Test]
        public void NumberToString_MatchesV8()
        {
            JsonValue list = Doc["numberToString"];
            int bad = 0;
            var report = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                string s = list[i].String;
                int colon = s.IndexOf(':');
                double x = Fixtures.D(s.Substring(0, colon));
                string expected = s.Substring(colon + 1);
                string actual = JsNumber.ToString(x);
                if (actual != expected)
                {
                    bad++;
                    if (bad <= 5) report.Append(s.Substring(0, colon)).Append(" expected ").Append(expected).Append(" actual ").Append(actual).Append('\n');
                }
            }
            Assert.AreEqual(0, bad, "numberToString: " + bad + "/" + list.Count + " differ\n" + report);
        }

        [Test]
        public void JsRandom_Mulberry32_FirstValuesMatchReference()
        {
            JsonValue rng = Fixtures.Load("rng-reference.json");
            var r = new JsRandom(JsRandom.DefaultSeed);
            JsonValue values = rng["values"];
            for (int i = 0; i < values.Count; i++)
            {
                Assert.AreEqual(values[i].String, Fixtures.Hex(r.NextDouble()), "draw " + i);
            }
            Assert.AreEqual(values.Count, r.CallCount);
        }
    }
}
