using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace EchoChamber.Tests
{
    public class HeadlessSimulationTests
    {
        private const int Seeds = 40;

        [Test]
        public void ThreeDayRunPlaysNineTurns()
        {
            var r = HeadlessRunner.Run(TestContent.Content(), TestContent.Tuning(), PolicyLibrary.AllNormal(), 1);
            Assert.AreEqual(9, r.TurnsPlayed);
            Assert.AreEqual(3, r.RevenueByDay.Length);
            Assert.Greater(r.History.Count, 20);
            Assert.GreaterOrEqual(r.GoalsTotal, 7); // 2 + 3 + 2
        }

        [Test]
        public void SameSeedGivesTheSameRun()
        {
            var a = HeadlessRunner.Run(TestContent.Content(), TestContent.Tuning(), PolicyLibrary.AllAmplify(), 5);
            var b = HeadlessRunner.Run(TestContent.Content(), TestContent.Tuning(), PolicyLibrary.AllAmplify(), 5);
            Assert.AreEqual(a.Ending, b.Ending);
            CollectionAssert.AreEqual(a.RevenueByDay, b.RevenueByDay);
            Assert.AreEqual(a.Cohesion, b.Cohesion);
            Assert.AreEqual(a.Tension, b.Tension);
        }

        private static EndingId Mode(Dictionary<EndingId, int> counts)
        {
            EndingId best = EndingId.Gentry;
            int n = -1;
            foreach (var kv in counts)
                if (kv.Value > n) { n = kv.Value; best = kv.Key; }
            return best;
        }

        [Test]
        public void ExtremePoliciesLeadToDifferentEndings()
        {
            var c = TestContent.Content();
            var t = TestContent.Tuning();
            var amp = Mode(HeadlessRunner.Sweep(c, t, PolicyLibrary.AllAmplify(), Seeds));
            var hide = Mode(HeadlessRunner.Sweep(c, t, PolicyLibrary.AllHide(), Seeds));
            var normal = Mode(HeadlessRunner.Sweep(c, t, PolicyLibrary.AllNormal(), Seeds));
            Assert.AreNotEqual(amp, hide, "amplify-all vs hide-all");
            Assert.AreNotEqual(amp, normal, "amplify-all vs normal-all");
        }

        [TestCase(EndingId.Sleep)]
        [TestCase(EndingId.Locked)]
        [TestCase(EndingId.Alive)]
        [TestCase(EndingId.Balance)]
        [TestCase(EndingId.Gentry)]
        public void EveryEndingIsReachableWithAScriptedPlayer(EndingId ending)
        {
            var counts = HeadlessRunner.Sweep(TestContent.Content(), TestContent.Tuning(), PolicyLibrary.For(ending), Seeds);
            counts.TryGetValue(ending, out int hits);
            Assert.GreaterOrEqual(hits, 5, ending + " reached only " + hits + "/" + Seeds + " times");
        }

        [Test]
        public void HidingEverythingEarnsNothing()
        {
            var r = HeadlessRunner.Run(TestContent.Content(), TestContent.Tuning(), PolicyLibrary.AllHide(), 3);
            Assert.AreEqual(0, r.RevenueByDay[0] + r.RevenueByDay[1] + r.RevenueByDay[2]);
        }

        [Test]
        public void AmplifyingEverythingEarnsMoreThanNormalButCostsTheNeighbourhood()
        {
            long revAmp = 0, revNormal = 0;
            float tenAmp = 0, tenNormal = 0;
            for (int s = 1; s <= 10; s++)
            {
                var a = HeadlessRunner.Run(TestContent.Content(), TestContent.Tuning(), PolicyLibrary.AllAmplify(), s);
                var n = HeadlessRunner.Run(TestContent.Content(), TestContent.Tuning(), PolicyLibrary.AllNormal(), s);
                revAmp += a.RevenueByDay[0] + a.RevenueByDay[1] + a.RevenueByDay[2];
                revNormal += n.RevenueByDay[0] + n.RevenueByDay[1] + n.RevenueByDay[2];
                tenAmp += a.Tension;
                tenNormal += n.Tension;
            }
            Assert.Greater(revAmp, revNormal);
            Assert.Greater(tenAmp, tenNormal);
        }

        [Test]
        public void MetricsStayInRangeThroughoutARun()
        {
            var r = HeadlessRunner.Run(TestContent.Content(), TestContent.Tuning(), PolicyLibrary.AllAmplify(), 2);
            foreach (var h in r.History)
            {
                Assert.That(h.Key, Is.InRange(0f, 1f));
                Assert.That(h.Value, Is.InRange(0f, 1f));
            }
        }
    }
}
