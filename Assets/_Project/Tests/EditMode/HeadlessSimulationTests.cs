using NUnit.Framework;

namespace EchoChamber.Tests
{
    public class HeadlessSimulationTests
    {
        [Test]
        public void OneDayRunPlaysThreeTurns()
        {
            var r = HeadlessRunner.Run(TestContent.Content(), TestContent.Tuning(), PolicyLibrary.AllNormal(), 1);
            Assert.AreEqual(3, r.TurnsPlayed);
        }

        [Test]
        public void SameSeedGivesTheSameRun()
        {
            var a = HeadlessRunner.Run(TestContent.Content(), TestContent.Tuning(), PolicyLibrary.AllAmplify(), 5);
            var b = HeadlessRunner.Run(TestContent.Content(), TestContent.Tuning(), PolicyLibrary.AllAmplify(), 5);
            Assert.AreEqual(a.Revenue, b.Revenue);
            Assert.AreEqual(a.Cohesion, b.Cohesion);
            Assert.AreEqual(a.Tension, b.Tension);
        }

        [Test]
        public void HidingEverythingEarnsNothing()
        {
            var r = HeadlessRunner.Run(TestContent.Content(), TestContent.Tuning(), PolicyLibrary.AllHide(), 3);
            Assert.AreEqual(0, r.Revenue);
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
                revAmp += a.Revenue;
                revNormal += n.Revenue;
                tenAmp += a.Tension;
                tenNormal += n.Tension;
            }
            Assert.Greater(revAmp, revNormal);
            Assert.Greater(tenAmp, tenNormal);
        }

        [Test]
        public void MetricsStayInRangeAtTheEndOfARun()
        {
            var r = HeadlessRunner.Run(TestContent.Content(), TestContent.Tuning(), PolicyLibrary.AllAmplify(), 2);
            Assert.That(r.Cohesion, Is.InRange(0f, 1f));
            Assert.That(r.Tension, Is.InRange(0f, 1f));
        }

        [Test]
        public void EveryPostedDeliveryReachesAtLeastOneNeighbour()
        {
            var sim = TestContent.Sim(7);
            sim.StartGame();
            var decisions = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<PostData, DecisionMode>>();
            foreach (var p in sim.TurnPosts)
                decisions.Add(new System.Collections.Generic.KeyValuePair<PostData, DecisionMode>(p, DecisionMode.Amplify));
            sim.Publish(decisions);
            while (sim.Phase == GamePhase.Simulate) sim.Tick(0.05f);
            foreach (var e in sim.Log.Posts) Assert.Greater(e.Viewers, 0, e.Post.text);
        }
    }
}
