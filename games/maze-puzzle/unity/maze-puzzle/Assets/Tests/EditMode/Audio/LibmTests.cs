using NUnit.Framework;

namespace OrbitalSalvage.AudioDsp.Tests
{
    public class LibmTests
    {
        [TestCase(0.0, 1.0)]
        [TestCase(1.0, 2.718281828459045)]
        [TestCase(-1.0, 0.36787944117144233)]
        [TestCase(10.0, 22026.465794806718)]
        [TestCase(-20.5, 1.2501528663867426e-9)]
        public void ExpMatchesReference(double x, double expected)
        {
            Assert.AreEqual(1.0, Libm.Exp(x) / expected, 4e-16);
        }

        [TestCase(1000.0, 6.907755278982137)]
        [TestCase(0.5, -0.6931471805599453)]
        [TestCase(3.0, 1.0986122886681098)]
        public void LogMatchesReference(double x, double expected)
        {
            Assert.AreEqual(expected, Libm.Log(x), 4e-16 * System.Math.Abs(expected));
        }

        [Test]
        public void Log10AndPowAreInverseAndExactOnPowersOfTen()
        {
            Assert.AreEqual(3.0, Libm.Log10(1000.0), 1e-15);
            Assert.AreEqual(10f, (float)Libm.Pow(10.0, 1.0));
            Assert.AreEqual(32.0, Libm.Pow(2.0, 5.0));
            Assert.AreEqual(0.5, Libm.Pow(2.0, -1.0));
            Assert.AreEqual(1.0, Libm.Pow(2.0, 0.0));
        }

        [TestCase(1.0, 0.8414709848078965)]
        [TestCase(0.5, 0.479425538604203)]
        [TestCase(3.0, 0.1411200080598672)]
        [TestCase(-7.0, -0.6569865987187891)]
        public void SinMatchesReference(double x, double expected)
        {
            Assert.AreEqual(expected, Libm.Sin(x), 4e-16);
        }

        [TestCase(1.0, 0.5403023058681398)]
        [TestCase(2.0, -0.4161468365471424)]
        [TestCase(6.0, 0.960170286650366)]
        public void CosMatchesReference(double x, double expected)
        {
            Assert.AreEqual(expected, Libm.Cos(x), 4e-16);
        }

        [TestCase(0.5, 0.5235987755982989)]
        [TestCase(0.9998, 1.5507959934465636)]
        [TestCase(-0.25, -0.25268025514207865)]
        [TestCase(1.0, 1.5707963267948966)]
        public void AsinMatchesReference(double x, double expected)
        {
            Assert.AreEqual(expected, Libm.Asin(x), 4e-15);
        }
    }
}
