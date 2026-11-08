using NUnit.Framework;
using EchoChamber.Editor;

namespace EchoChamber.Tests
{
    public class FeedSystemTests
    {
        private FeedContent content;

        [SetUp]
        public void SetUp()
        {
            content = FeedContentImporter.Build();
        }

        private FeedSystem Sys(int day = 5, float jitter = 0f)
        {
            var tuning = new FeedTuning { jitter = jitter };
            return new FeedSystem(content, tuning, new SimRandom(1), day);
        }

        private FeedPost Post(int id)
        {
            return content.posts.Find(p => p.id == id);
        }

        [Test]
        public void ContentHas39PostsAnd5Profiles()
        {
            Assert.AreEqual(39, content.posts.FindAll(p => !p.filler).Count);
            Assert.AreEqual(8, content.posts.FindAll(p => p.filler).Count);
            Assert.AreEqual(5, content.profiles.Count);
        }

        [Test]
        public void PeopleAppearByStartDay()
        {
            Assert.AreEqual(3, Sys(1).People.Count);
            Assert.AreEqual(4, Sys(2).People.Count);
            Assert.AreEqual(5, Sys(4).People.Count);
        }

        [Test]
        public void MatchingPostBeatsNonMatchingCalmPost()
        {
            var s = Sys();
            var rosa = s.Find("Rosa");
            float match = s.Give(Post(1), rosa).Minutes;
            var s2 = Sys();
            float off = s2.Give(Post(2), s2.Find("Rosa")).Minutes;
            Assert.Greater(match, off);
        }

        [Test]
        public void InflamedBeatsCalmForSameMatch()
        {
            var s = Sys();
            float calm = s.Give(Post(4), s.Find("Armando")).Minutes;
            var s2 = Sys();
            float hot = s2.Give(Post(7), s2.Find("Armando")).Minutes;
            Assert.Greater(hot, calm);
        }

        [Test]
        public void InflamedEvenOffInterestBeatsCalmOffInterest()
        {
            var s = Sys();
            float calm = s.Give(Post(12), s.Find("Armando")).Minutes;
            var s2 = Sys();
            float hot = s2.Give(Post(21), s2.Find("Rosa")).Minutes;
            Assert.Greater(hot, calm);
        }

        [Test]
        public void InflamedPostsRaiseAggressionAndBubble()
        {
            var s = Sys();
            var a = s.Find("Armando");
            float ag = a.Aggression, bb = a.Bubble;
            s.Give(Post(7), a);
            Assert.Greater(a.Aggression, ag);
            Assert.Greater(a.Bubble, bb);
        }

        [Test]
        public void BridgePostLowersBubbleAndIsolation()
        {
            var s = Sys();
            var a = s.Find("Armando");
            a.Bubble = 0.6f;
            a.Isolation = 0.5f;
            s.Give(Post(36), a);
            Assert.Less(a.Bubble, 0.6f);
            Assert.Less(a.Isolation, 0.5f);
        }

        [Test]
        public void MentionedNeighbourGetsStressed()
        {
            var s = Sys();
            var rosa = s.Find("Rosa");
            s.Give(Post(26), s.Find("Armando"));
            Assert.Greater(rosa.Stress, 0f);
        }

        [Test]
        public void RepeatedThemePaysMore()
        {
            var s = Sys();
            var t = s.Find("Tiago");
            float first = s.Give(Post(2), t).Minutes;
            float second = s.Give(Post(29), t).Minutes;
            Assert.Greater(second, first);
        }

        [Test]
        public void DiscardCostsEngagement()
        {
            var s = Sys();
            var r = s.Discard(Post(1));
            Assert.IsTrue(r.Discarded);
            Assert.Less(s.TotalMinutes, 0f);
        }

        [Test]
        public void SameSeedGivesSameResult()
        {
            float Run()
            {
                var s = new FeedSystem(content, new FeedTuning(), new SimRandom(7), 5);
                s.Give(Post(7), s.Find("Armando"));
                s.Give(Post(2), s.Find("Tiago"));
                return s.TotalMinutes;
            }
            Assert.AreEqual(Run(), Run());
        }

        [Test]
        public void MemesSlowlyIsolateAndShowAsStayingHome()
        {
            var s = Sys();
            var t = s.Find("Tiago");
            for (int i = 0; i < 12; i++) s.Give(Post(2), t);
            Assert.AreEqual(FeedSignal.StaysHome, s.Signal(t));
        }

        [Test]
        public void InflamedStreakShowsAsArguing()
        {
            var s = Sys();
            var a = s.Find("Armando");
            for (int i = 0; i < 6; i++) s.Give(Post(7), a);
            Assert.AreEqual(FeedSignal.ArguesInSquare, s.Signal(a));
        }
    }
}
