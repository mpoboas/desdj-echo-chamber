using NUnit.Framework;

namespace EchoChamber.Tests
{
    public class DilemmaPostTests
    {
        private static ContentSet PoolWithDilemmas()
        {
            var c = new ContentSet();
            foreach (PostCategory cat in System.Enum.GetValues(typeof(PostCategory)))
            {
                c.pool.Add(new PostData { category = cat, text = cat + " normal 1", value = 1.5f });
                c.pool.Add(new PostData { category = cat, text = cat + " normal 2", value = 1.5f });
                c.pool.Add(new PostData { category = cat, text = cat + " dilemma 1", value = 1.5f, dilemma = true });
                c.pool.Add(new PostData { category = cat, text = cat + " dilemma 2", value = 1.5f, dilemma = true });
            }
            return c;
        }

        private static PostPicker Picker(TuningData t, ContentSet c, int seed, out System.Collections.Generic.List<Neighbour> ns)
        {
            var sim = TestContent.Sim(seed);
            ns = sim.Neighbours;
            return new PostPicker(t, c, new SimRandom(seed));
        }

        [Test]
        public void ChanceOneAlwaysPicksDilemmas()
        {
            var t = TestContent.Tuning();
            t.dilemmaChance = 1f;
            var picker = Picker(t, PoolWithDilemmas(), 1, out var ns);
            for (int i = 0; i < 40; i++) Assert.IsTrue(picker.Pick(ns).dilemma);
        }

        [Test]
        public void ChanceZeroNeverPicksDilemmas()
        {
            var t = TestContent.Tuning();
            t.dilemmaChance = 0f;
            var picker = Picker(t, PoolWithDilemmas(), 2, out var ns);
            for (int i = 0; i < 40; i++) Assert.IsFalse(picker.Pick(ns).dilemma);
        }

        [Test]
        public void ChanceHalfMixesBoth()
        {
            var t = TestContent.Tuning();
            t.dilemmaChance = 0.5f;
            var picker = Picker(t, PoolWithDilemmas(), 3, out var ns);
            int d = 0;
            for (int i = 0; i < 200; i++) if (picker.Pick(ns).dilemma) d++;
            Assert.That(d, Is.InRange(60, 140));
        }

        [Test]
        public void ImposedPostsAreNeverDilemmas()
        {
            var t = TestContent.Tuning();
            var picker = Picker(t, PoolWithDilemmas(), 4, out _);
            for (int i = 0; i < 40; i++) Assert.IsFalse(picker.PickImposed(new System.Collections.Generic.List<PostData>()).dilemma);
        }

        [Test]
        public void ReadingEveryPostBeatsTheCategoryRule()
        {
            var c = TestContent.Content();
            var t = TestContent.Tuning();
            const int seeds = 40;
            int byCategory = 0, perPost = 0;
            for (int s = 1; s <= seeds; s++)
            {
                if (HeadlessRunner.Run(c, t, PolicyLibrary.TargetBalance(), s).Ending == EndingId.Balance) byCategory++;
                if (HeadlessRunner.Run(c, t, PolicyLibrary.OraclePerPost(), s).Ending == EndingId.Balance) perPost++;
            }
            Assert.GreaterOrEqual(perPost, byCategory + seeds / 10, $"per post {perPost} vs by category {byCategory}");
            Assert.Greater(byCategory, 0, "the category rule must still be able to win");
        }

        [Test]
        public void ContentWithoutDilemmasIgnoresTheChance()
        {
            var c = PoolWithDilemmas();
            c.pool.RemoveAll(p => p.dilemma);
            var t = TestContent.Tuning();
            t.dilemmaChance = 1f;
            var picker = Picker(t, c, 5, out var ns);
            for (int i = 0; i < 30; i++) Assert.IsFalse(picker.Pick(ns).dilemma);
        }
    }
}
