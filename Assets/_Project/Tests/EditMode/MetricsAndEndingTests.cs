using NUnit.Framework;

namespace EchoChamber.Tests
{
    public class MetricsAndEndingTests
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

        [Test]
        public void EachOfTheFiveEndingsIsReachable()
        {
            var t = TestContent.Tuning();
            Assert.AreEqual(EndingId.Sleep, EndingResolver.Resolve(t, 0.6f, 0.9f, 0.9f, 100, 260));
            Assert.AreEqual(EndingId.Locked, EndingResolver.Resolve(t, 0.3f, 0.7f, 0.9f, 100, 260));
            Assert.AreEqual(EndingId.Alive, EndingResolver.Resolve(t, 0.3f, 0.3f, 0.6f, 200, 260));
            Assert.AreEqual(EndingId.Balance, EndingResolver.Resolve(t, 0.3f, 0.3f, 0.6f, 300, 260));
            Assert.AreEqual(EndingId.Balance, EndingResolver.Resolve(t, 0.3f, 0.3f, 0.52f, 100, 260));
            Assert.AreEqual(EndingId.Gentry, EndingResolver.Resolve(t, 0.3f, 0.3f, 0.4f, 300, 260));
            // Balance also needs the neighbourhood to be understood; Alive does not.
            Assert.AreEqual(EndingId.Gentry, EndingResolver.Resolve(t, 0.3f, 0.3f, 0.6f, 300, 260, 0.1f));
            Assert.AreEqual(EndingId.Gentry, EndingResolver.Resolve(t, 0.3f, 0.3f, 0.52f, 100, 260, 0.1f));
            Assert.AreEqual(EndingId.Balance, EndingResolver.Resolve(t, 0.3f, 0.3f, 0.6f, 300, 260, t.endingBalanceUnderstanding));
            Assert.AreEqual(EndingId.Alive, EndingResolver.Resolve(t, 0.3f, 0.3f, 0.6f, 200, 260, 0f));
        }

        [Test]
        public void EndingOrderFollowsTheSpec()
        {
            var t = TestContent.Tuning();
            // apathy beats tension
            Assert.AreEqual(EndingId.Sleep, EndingResolver.Resolve(t, 0.56f, 0.8f, 0.2f, 0, 260));
            // tension beats cohesion
            Assert.AreEqual(EndingId.Locked, EndingResolver.Resolve(t, 0.1f, 0.66f, 0.9f, 0, 260));
        }
    }
}
