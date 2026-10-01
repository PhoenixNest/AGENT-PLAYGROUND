using System.Collections.Generic;
using NUnit.Framework;

namespace OrbitalSalvage.Render.Tests
{
    public class GeometryTests
    {
        [Test]
        public void CssColor_ParsesHexRgbAndNamed()
        {
            Assert.IsTrue(CssColor.TryParse("#f80", out var a));
            Assert.AreEqual(1f, a.r, 1e-6);
            Assert.AreEqual(0x88 / 255f, a.g, 1e-6);
            Assert.IsTrue(CssColor.TryParse("rgba(10, 20, 30, 0.5)", out var b));
            Assert.AreEqual(10 / 255f, b.r, 1e-4);
            Assert.AreEqual(0.5f, b.a, 1e-6);
            Assert.IsTrue(CssColor.TryParse("transparent", out var c));
            Assert.AreEqual(0f, c.a);
            Assert.IsFalse(CssColor.TryParse("not-a-colour", out _));
        }

        [Test]
        public void Matrix_InverseRoundTrips()
        {
            var m = new Matrix2D(2, 0.5, -0.25, 3, 10, -4);
            var p = m.Inverse().Apply(m.Apply(7, 9));
            Assert.AreEqual(7, p.x, 1e-9);
            Assert.AreEqual(9, p.y, 1e-9);
        }

        [Test]
        public void MiniJson_ParsesNestedStructures()
        {
            var o = (Dictionary<string, object>)MiniJson.Parse("{\"a\":[1,2.5,\"x\"],\"b\":{\"c\":true,\"d\":null}}");
            var list = (List<object>)o["a"];
            Assert.AreEqual(2.5, (double)list[1], 1e-12);
            Assert.AreEqual("x", list[2]);
            Assert.AreEqual(true, ((Dictionary<string, object>)o["b"])["c"]);
        }

        [Test]
        public void Path_ArcFlattensWithinTolerance()
        {
            var path = new PathData();
            path.Arc(Matrix2D.Identity, 50, 50, 40, 0, System.Math.PI * 2, false);
            Assert.GreaterOrEqual(path.Count, 1);
            foreach (var p in path[0].pts)
                Assert.AreEqual(40, System.Math.Sqrt((p.x - 50) * (p.x - 50) + (p.y - 50) * (p.y - 50)), PathData.FlattenTolerance * 1.5);
        }

        [Test]
        public void Stroker_ButtLineProducesRectangleOfRequestedWidth()
        {
            var st = new Stroker { halfWidth = 2, cap = LineCapStyle.Butt, join = LineJoinStyle.Miter };
            var set = new PolygonSet();
            st.Stroke(new List<Vec2d> { new Vec2d(0, 0), new Vec2d(10, 0) }, false, set, 1e-6);
            double minY = double.MaxValue, maxY = double.MinValue, minX = double.MaxValue, maxX = double.MinValue;
            foreach (var p in set.pts)
            {
                minY = System.Math.Min(minY, p.y); maxY = System.Math.Max(maxY, p.y);
                minX = System.Math.Min(minX, p.x); maxX = System.Math.Max(maxX, p.x);
            }
            Assert.AreEqual(-2, minY, 1e-9);
            Assert.AreEqual(2, maxY, 1e-9);
            Assert.AreEqual(0, minX, 1e-9);
            Assert.AreEqual(10, maxX, 1e-9);
        }

        [Test]
        public void Stroker_ZeroLengthWithoutAllowDotDrawsNothing()
        {
            var st = new Stroker { halfWidth = 2, cap = LineCapStyle.Round, join = LineJoinStyle.Round };
            var set = new PolygonSet();
            st.Stroke(new List<Vec2d> { new Vec2d(5, 5), new Vec2d(5, 5) }, false, set, 1e-6);
            Assert.AreEqual(0, set.pts.Count);
        }

        [Test]
        public void Dasher_SplitsLineIntoExpectedSegments()
        {
            var result = new List<List<Vec2d>>();
            Dasher.Dash(new List<Vec2d> { new Vec2d(0, 0), new Vec2d(10, 0) }, false, new[] { 2.0, 3.0 }, 0, result, new Stack<List<Vec2d>>(), 1e-9);
            Assert.GreaterOrEqual(result.Count, 2);
            Assert.AreEqual(2, result[0][result[0].Count - 1].x, 1e-9);
            Assert.AreEqual(5, result[1][0].x, 1e-9);
            Assert.AreEqual(7, result[1][result[1].Count - 1].x, 1e-9);
        }
    }
}
