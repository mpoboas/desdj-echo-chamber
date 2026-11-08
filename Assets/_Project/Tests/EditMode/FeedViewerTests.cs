using System.Linq;
using NUnit.Framework;
using EchoChamber.Editor;

namespace EchoChamber.Tests
{
    public class FeedViewerTests
    {
        private FeedContent content;

        [SetUp]
        public void SetUp()
        {
            content = FeedContentImporter.Build();
        }

        private FeedCareer Career(int seed = 1, FeedTuning t = null)
        {
            return new FeedCareer(content, t ?? new FeedTuning(), new SimRandom(seed));
        }

        private static void Run(FeedDay d, float seconds, System.Action<ViewerEvent> onEvent = null)
        {
            d.Running = true;
            for (float t = 0f; t < seconds; t += 0.05f)
            {
                d.Step(0.05f);
                if (onEvent != null)
                    foreach (var v in d.Viewers)
                        foreach (var e in v.DrainEvents()) onEvent(e);
            }
        }

        [Test]
        public void ViewersWatchLikeAndComment()
        {
            var c = Career(4, new FeedTuning { dayLength = 900f });
            var kinds = new System.Collections.Generic.HashSet<ViewerEventKind>();
            Run(c.Current, 800f, e => kinds.Add(e.Kind));
            Assert.IsTrue(kinds.Contains(ViewerEventKind.VideoStarted));
            Assert.IsTrue(kinds.Contains(ViewerEventKind.Liked));
            Assert.IsTrue(kinds.Contains(ViewerEventKind.CommentsOpened));
            Assert.IsTrue(kinds.Contains(ViewerEventKind.CommentPosted));
        }

        [Test]
        public void SameSeedGivesSameDay()
        {
            float Total(int seed)
            {
                var c = Career(seed);
                Run(c.Current, 95f);
                return c.Current.Minutes;
            }
            Assert.AreEqual(Total(9), Total(9), 0.0001f);
        }

        [Test]
        public void IgnoredViewersGetTiredAndPutThePhoneDown()
        {
            var c = Career(2, new FeedTuning { dayLength = 300f });
            bool slept = false;
            Run(c.Current, 200f, e => { if (e.Kind == ViewerEventKind.FellAsleep) slept = true; });
            Assert.IsTrue(slept);
        }

        [Test]
        public void FeedingTheTiredBeatsIgnoringThem()
        {
            float Idle(int seed)
            {
                var c = Career(seed);
                Run(c.Current, 95f);
                return c.Current.Minutes;
            }
            float Fed(int seed)
            {
                var c = Career(seed);
                var d = c.Current;
                d.Running = true;
                for (float t = 0f; t < 95f; t += 0.05f)
                {
                    d.Step(0.05f);
                    if (d.Dock.Count > 0)
                    {
                        var post = d.Dock[0];
                        var best = d.Viewers.OrderByDescending(v => (v.Person.Profile.interests.Contains(post.theme) ? 1 : 0) + (v.Resting ? 2 : 0)).First();
                        d.Give(post, best);
                    }
                }
                return d.Minutes;
            }
            float idle = 0f, fed = 0f;
            for (int s = 1; s <= 6; s++) { idle += Idle(s); fed += Fed(s); }
            Assert.Greater(fed, idle * 1.4f);
        }

        [Test]
        public void OnlyRunningDaysCountAppTime()
        {
            var c = Career(1);
            c.Current.Running = false;
            for (int i = 0; i < 200; i++) c.Current.Step(0.1f);
            Assert.AreEqual(0f, c.Current.Minutes, 0.0001f);
        }
    }
}
