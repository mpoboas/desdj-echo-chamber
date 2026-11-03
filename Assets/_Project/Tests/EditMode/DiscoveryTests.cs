using NUnit.Framework;

namespace EchoChamber.Tests
{
    public class DiscoveryTests
    {
        private static void GiveReaction(Neighbour n, int id, ReactionKind kind, bool strong, ReactionKind baseKind = ReactionKind.Medo)
        {
            n.Reaction = new Reaction { Id = id, Kind = kind, Base = baseKind, Strong = strong, Category = PostCategory.Sens, TimeLeft = 5f };
        }

        [Test]
        public void NothingToNoteWithoutAnActiveReaction()
        {
            var sim = TestContent.Sim();
            var r = sim.Observe(sim.Neighbours[0]);
            Assert.IsFalse(r.Noted);
        }

        [Test]
        public void ReactionCanOnlyBeNotedOnce()
        {
            var sim = TestContent.Sim();
            var n = sim.Neighbours[0];
            GiveReaction(n, 1, ReactionKind.Medo, false);
            Assert.IsTrue(sim.Observe(n).Noted);
            Assert.IsFalse(sim.Observe(n).Noted);
            Assert.AreEqual(1, n.Notes.Count);
        }

        [Test]
        public void ExpiredReactionCannotBeNoted()
        {
            var sim = TestContent.Sim();
            var n = sim.Neighbours[0];
            GiveReaction(n, 1, ReactionKind.Medo, false);
            n.Reaction.TimeLeft = 0f;
            Assert.IsFalse(sim.Observe(n).Noted);
        }

        [Test]
        public void StrongSignatureReactionRevealsTheTrait()
        {
            var sim = TestContent.Sim();
            var anxious = sim.Neighbours[2];
            GiveReaction(anxious, 1, ReactionKind.Medo, true);
            var r = sim.Observe(anxious);
            Assert.IsTrue(r.TraitDiscovered);
            Assert.IsTrue(anxious.TraitKnown);
        }

        [Test]
        public void WeakReactionDoesNotRevealButThreeNotesDo()
        {
            var sim = TestContent.Sim();
            var n = sim.Neighbours[2];
            GiveReaction(n, 1, ReactionKind.Raiva, false);
            Assert.IsFalse(sim.Observe(n).TraitDiscovered);
            GiveReaction(n, 2, ReactionKind.Raiva, false);
            Assert.IsFalse(sim.Observe(n).TraitDiscovered);
            GiveReaction(n, 3, ReactionKind.Raiva, false);
            Assert.IsTrue(sim.Observe(n).TraitDiscovered);
        }

        [Test]
        public void ScepticsAreRevealedByIndifferenceToAlarmingPosts()
        {
            var sim = TestContent.Sim();
            var sceptic = sim.Neighbours[1];
            GiveReaction(sceptic, 1, ReactionKind.Indiferente, false, ReactionKind.Medo);
            Assert.IsTrue(sim.Observe(sceptic).TraitDiscovered);
        }

        [Test]
        public void RelationNeedsTimeTogetherAndProximity()
        {
            var sim = TestContent.Sim();
            var a = sim.Neighbours[0];
            var b = sim.Neighbours[1];
            a.X = 100; a.Y = 100; b.X = 200; b.Y = 100;
            a.Links[b.Id] = 3f; b.Links[a.Id] = 3f;
            GiveReaction(a, 1, ReactionKind.Medo, false);
            var r = sim.Observe(a);
            Assert.IsTrue(r.NewRelations.Contains(b));
            Assert.IsTrue(sim.Discovery.HasRelation(a.Id, b.Id));
        }

        [Test]
        public void NoRelationWhenTooFarApart()
        {
            var sim = TestContent.Sim();
            var a = sim.Neighbours[0];
            var b = sim.Neighbours[1];
            a.X = 100; a.Y = 100; b.X = 300; b.Y = 100;
            a.Links[b.Id] = 10f;
            GiveReaction(a, 1, ReactionKind.Medo, false);
            Assert.AreEqual(0, sim.Observe(a).NewRelations.Count);
        }

        [Test]
        public void UnderstandingStartsAtZeroAndCapsRelationsAtEight()
        {
            var sim = TestContent.Sim();
            Assert.AreEqual(0f, sim.Understanding);
            foreach (var n in sim.Neighbours) n.TraitKnown = true;
            Assert.AreEqual(14f / 22f, sim.Understanding, 0.0001f);
        }

        [Test]
        public void PatternShowsTopThreeReactionsAfterTwoObservations()
        {
            var sim = TestContent.Sim();
            Assert.IsNull(sim.Discovery.TopPattern(PostCategory.Sens));
            var n = sim.Neighbours[0];
            GiveReaction(n, 1, ReactionKind.Medo, false); sim.Observe(n);
            Assert.IsNull(sim.Discovery.TopPattern(PostCategory.Sens));
            GiveReaction(n, 2, ReactionKind.Medo, false); sim.Observe(n);
            var top = sim.Discovery.TopPattern(PostCategory.Sens);
            Assert.AreEqual(ReactionKind.Medo, top[0].Key);
            Assert.AreEqual(2, top[0].Value);
        }
    }
}
