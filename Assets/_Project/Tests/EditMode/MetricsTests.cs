using NUnit.Framework;

namespace EchoChamber.Tests
{
    public class MetricsTests
    {
        [Test]
        public void TensionFormula()
        {
            var m = new Metrics(TestContent.Tuning());
            Assert.AreEqual(0.65f * 0.4f + 0.35f * 0.2f, m.ComputeTension(0.4f, 0.2f, false, false), 0.0001f);
            Assert.AreEqual(0.65f * 0.4f + 0.35f * 0.2f + 0.12f + 0.04f, m.ComputeTension(0.4f, 0.2f, true, true), 0.0001f);
            Assert.AreEqual(1f, m.ComputeTension(1f, 1f, true, true));
        }

        [Test]
        public void CohesionFormula()
        {
            var m = new Metrics(TestContent.Tuning());
            Assert.AreEqual(0.5f * (1f - 0.1f * 0.6f) * 1.25f - 0.2f * 0.2f, m.ComputeCohesion(0.5f, 0.1f, 0.2f), 0.0001f);
            Assert.AreEqual(0f, m.ComputeCohesion(0f, 1f, 1f));
        }

        [Test]
        public void InitialValues()
        {
            var m = new Metrics(TestContent.Tuning());
            Assert.AreEqual(0.5f, m.Cohesion);
            Assert.AreEqual(0.2f, m.Tension);
        }
    }
}
