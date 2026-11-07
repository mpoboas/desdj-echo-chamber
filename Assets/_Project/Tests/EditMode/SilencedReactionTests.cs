using System.Collections.Generic;
using NUnit.Framework;

namespace EchoChamber.Tests
{
    public class SilencedReactionTests
    {
        private static GameSimulation PublishHidingOneNeighbourPost(out PostData target, out Neighbour author)
        {
            var sim = TestContent.Sim(3);
            sim.StartGame();
            PostData found = sim.TurnPosts.Find(p => sim.Neighbours.Exists(n => n.Name == sim.AuthorOf(p)));
            Assert.NotNull(found, "a post written by a neighbour");
            target = found;
            author = sim.Neighbours.Find(n => n.Name == sim.AuthorOf(found));
            PostData hidden = target;
            var list = new List<KeyValuePair<PostData, DecisionMode>>();
            foreach (var p in sim.TurnPosts) list.Add(new KeyValuePair<PostData, DecisionMode>(p, p == hidden ? DecisionMode.Hide : DecisionMode.Normal));
            sim.Publish(list);
            return sim;
        }

        [Test]
        public void TheAuthorOfAHiddenPostShowsASilencedReaction()
        {
            var sim = PublishHidingOneNeighbourPost(out PostData target, out Neighbour author);
            var entry = sim.Log.Posts.Find(e => e.Post == target);
            while (!entry.Delivered) sim.Tick(0.05f);

            Assert.NotNull(author.Reaction);
            Assert.AreEqual(ReactionKind.Silenciado, author.Reaction.Kind);
            Assert.AreEqual(target.category, author.Reaction.Category);
            Assert.Greater(author.Reaction.TimeLeft, 0f);
        }

        [Test]
        public void NotingASilencedAuthorDoesNotTouchTraitsOrCategoryPatterns()
        {
            var sim = TestContent.Sim();
            var n = sim.Neighbours[0];
            for (int i = 1; i <= 4; i++)
            {
                n.Reaction = new Reaction { Id = 100 + i, Kind = ReactionKind.Silenciado, Base = ReactionKind.Silenciado, Category = PostCategory.Sens, TimeLeft = 5f };
                var r = sim.Discovery.Observe(n, sim.Neighbours);
                Assert.IsTrue(r.Noted);
                Assert.IsFalse(r.TraitDiscovered);
            }
            Assert.IsFalse(n.TraitKnown);
            Assert.AreEqual(4, sim.Discovery.SilencedNoted);
            Assert.IsFalse(sim.Discovery.CategoryObservations.ContainsKey(PostCategory.Sens));
            Assert.AreEqual(0, sim.Discovery.RelationCount);
        }

        [Test]
        public void ASilencedReactionCanOnlyBeNotedOnce()
        {
            var sim = TestContent.Sim();
            var n = sim.Neighbours[0];
            n.Reaction = new Reaction { Id = 7, Kind = ReactionKind.Silenciado, Base = ReactionKind.Silenciado, Category = PostCategory.Com, TimeLeft = 5f };
            Assert.IsTrue(sim.Discovery.Observe(n, sim.Neighbours).Noted);
            Assert.IsFalse(sim.Discovery.Observe(n, sim.Neighbours).Noted);
        }

        [Test]
        public void SilencedReactionIdsNeverCollideWithOrdinaryOnes()
        {
            var sim = TestContent.Sim();
            int a = sim.Deliverer.NextReactionId();
            int b = sim.Deliverer.NextReactionId();
            Assert.AreNotEqual(a, b);
        }
    }
}
