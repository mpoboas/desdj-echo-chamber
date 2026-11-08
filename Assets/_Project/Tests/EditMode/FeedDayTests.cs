using System.Linq;
using NUnit.Framework;
using EchoChamber.Editor;

namespace EchoChamber.Tests
{
    public class FeedDayTests
    {
        private FeedContent content;

        [SetUp]
        public void SetUp()
        {
            content = FeedContentImporter.Build();
        }

        private static void Run(FeedCareer c, float seconds, System.Action<FeedDay> each = null)
        {
            c.Current.Running = true;
            for (float t = 0f; t < seconds; t += 0.1f)
            {
                c.Current.Step(0.1f);
                if (each != null) each(c.Current);
            }
        }

        [Test]
        public void DayOneOnlyReleasesBasePosts()
        {
            var c = new FeedCareer(content, new FeedTuning(), new SimRandom(3));
            var seen = new System.Collections.Generic.List<FeedPost>();
            Run(c, 90f, d => { foreach (var p in d.Dock) if (!seen.Contains(p)) seen.Add(p); });
            Assert.AreEqual(5, seen.Count);
            Assert.IsTrue(seen.All(p => p.role == FeedRole.Base && !p.inflamed && !p.filler));
        }

        [Test]
        public void DayTwoBringsInflamedPosts()
        {
            var c = new FeedCareer(content, new FeedTuning { dayGoals = new[] { 0f, 0f, 0f } }, new SimRandom(3));
            Run(c, 95f);
            c.EndDay();
            var seen = new System.Collections.Generic.List<FeedPost>();
            Run(c, 90f, d => { foreach (var p in d.Dock) if (!seen.Contains(p)) seen.Add(p); });
            Assert.IsTrue(seen.Any(p => p.inflamed));
        }

        [Test]
        public void PostsAreReleasedAtTheirTimesAndNeverRepeat()
        {
            var c = new FeedCareer(content, new FeedTuning(), new SimRandom(5));
            var seen = new System.Collections.Generic.List<int>();
            Run(c, 95f, d => { foreach (var p in d.Dock) if (!seen.Contains(p.id)) seen.Add(p.id); });
            Assert.AreEqual(seen.Count, seen.Distinct().Count());
            Assert.IsTrue(c.Current.Finished);
        }

        [Test]
        public void NobodyScrollsBeforeTheDayRuns()
        {
            var c = new FeedCareer(content, new FeedTuning(), new SimRandom(1));
            for (int i = 0; i < 100; i++) c.Current.Step(0.1f);
            Assert.AreEqual(0f, c.Current.Minutes, 0.0001f);
        }

        [Test]
        public void GivingAPostReachesTheViewer()
        {
            var c = new FeedCareer(content, new FeedTuning(), new SimRandom(1));
            Run(c, 5f);
            var post = c.Current.Dock[0];
            var rosa = c.Current.ViewerFor(c.System.Find("Rosa"));
            Assert.IsTrue(c.Current.Give(post, rosa));
            Assert.AreEqual(0, c.Current.Dock.Count);
            Assert.AreEqual(1, c.Current.Given.Count);
        }

        [Test]
        public void FailingGoalGivesWarningAndThreeFire()
        {
            var c = new FeedCareer(content, new FeedTuning { dayGoals = new[] { 999f, 999f, 999f, 999f } }, new SimRandom(1));
            for (int i = 0; i < 3; i++)
            {
                Run(c, 95f);
                Assert.IsFalse(c.EndDay());
            }
            Assert.AreEqual(3, c.Warnings);
            Assert.IsTrue(c.Fired);
            Assert.IsTrue(c.Finished);
        }

        [Test]
        public void MetGoalGivesNoWarning()
        {
            var c = new FeedCareer(content, new FeedTuning { dayGoals = new[] { 0.1f, 0.1f } }, new SimRandom(1));
            Run(c, 95f);
            Assert.IsTrue(c.EndDay());
            Assert.AreEqual(0, c.Warnings);
        }

        [Test]
        public void CareerEndsAfterLastDay()
        {
            var c = new FeedCareer(content, new FeedTuning { dayGoals = new[] { 0f, 0f } }, new SimRandom(1));
            c.EndDay();
            Assert.IsFalse(c.Finished);
            c.EndDay();
            Assert.IsTrue(c.Finished);
        }

        [Test]
        public void HiddenStateSurvivesTheNightAndNewPeopleJoin()
        {
            var c = new FeedCareer(content, new FeedTuning { dayGoals = new[] { 0f, 0f } }, new SimRandom(2));
            var armando = c.System.Find("Armando");
            armando.Aggression = 0.9f;
            Run(c, 95f);
            c.EndDay();
            Assert.Greater(c.System.Find("Armando").Aggression, 0.6f);
            Assert.IsNotNull(c.System.Find("Leonor"));
            Assert.AreEqual(4, c.Viewers.Count);
        }
    }
}
