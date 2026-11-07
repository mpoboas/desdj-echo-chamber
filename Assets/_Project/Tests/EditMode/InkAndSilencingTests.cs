using System.Collections.Generic;
using NUnit.Framework;

namespace EchoChamber.Tests
{
    public class InkAndSilencingTests
    {
        private static List<KeyValuePair<PostData, DecisionMode>> All(GameSimulation sim, DecisionMode mode)
        {
            var list = new List<KeyValuePair<PostData, DecisionMode>>();
            foreach (var p in sim.TurnPosts) list.Add(new KeyValuePair<PostData, DecisionMode>(p, mode));
            return list;
        }

        private static Neighbour AuthorNeighbour(GameSimulation sim, PostData post)
        {
            return sim.Neighbours.Find(n => n.Name == sim.AuthorOf(post));
        }

        [Test]
        public void InkStartsFullEveryTurnAndIsSpent()
        {
            var sim = TestContent.Sim();
            sim.StartGame();
            Assert.AreEqual(sim.Tuning.hidesPerTurn, sim.InkLeft);
            Assert.IsTrue(sim.CanHide(sim.TurnPosts[0]));
            for (int i = 0; i < sim.Tuning.hidesPerTurn; i++) sim.SpendInk();
            Assert.AreEqual(0, sim.InkLeft);
            Assert.IsFalse(sim.CanHide(sim.TurnPosts[0]));
            sim.SpendInk();
            Assert.AreEqual(0, sim.InkLeft);
        }

        [Test]
        public void PublishingCapsHidesAtTheInkBudget()
        {
            var sim = TestContent.Sim();
            sim.StartGame();
            Assert.Greater(sim.TurnPosts.Count, sim.Tuning.hidesPerTurn);
            sim.Publish(All(sim, DecisionMode.Hide));
            int hidden = sim.Log.Posts.FindAll(e => e.Mode == DecisionMode.Hide).Count;
            Assert.AreEqual(sim.Tuning.hidesPerTurn, hidden);
        }

        [Test]
        public void EveryPostHasAnAuthorThatStaysTheSameInTheTurn()
        {
            var sim = TestContent.Sim(4);
            sim.StartGame();
            sim.Pressure = 1;
            sim.StartTurn();
            foreach (var p in sim.TurnPosts)
            {
                string a = sim.AuthorOf(p);
                Assert.IsFalse(string.IsNullOrEmpty(a));
                Assert.AreEqual(a, sim.AuthorOf(p));
            }
        }

        [Test]
        public void AuthorsAreDeterministicForTheSameSeed()
        {
            var a = TestContent.Sim(9);
            var b = TestContent.Sim(9);
            a.Pressure = b.Pressure = 1;
            a.StartGame();
            b.StartGame();
            for (int i = 0; i < a.TurnPosts.Count; i++) Assert.AreEqual(a.AuthorOf(a.TurnPosts[i]), b.AuthorOf(b.TurnPosts[i]));
        }

        [Test]
        public void HidingAPostMakesItsAuthorMoreDistrustfulAndLessBelonging()
        {
            var tuning = TestContent.Tuning();
            tuning.hiddenAuthorDesc = 0.6f;
            tuning.hiddenAuthorPert = 0.3f;

            float DescAfter(DecisionMode mode, out float pert)
            {
                var sim = new GameSimulation(TestContent.Content(), tuning, 3);
                sim.StartGame();
                PostData target = sim.TurnPosts.Find(p => AuthorNeighbour(sim, p) != null);
                Assert.NotNull(target, "a post written by a neighbour");
                string name = sim.AuthorOf(target);
                var list = new List<KeyValuePair<PostData, DecisionMode>>();
                foreach (var p in sim.TurnPosts) list.Add(new KeyValuePair<PostData, DecisionMode>(p, p == target ? mode : DecisionMode.Normal));
                sim.Publish(list);
                while (sim.Phase == GamePhase.Simulate) sim.Tick(0.1f);
                Neighbour n = sim.Neighbours.Find(x => x.Name == name);
                pert = n.Pert;
                return n.Desc;
            }

            float descHide = DescAfter(DecisionMode.Hide, out float pertHide);
            float descNormal = DescAfter(DecisionMode.Normal, out float pertNormal);
            Assert.Greater(descHide, descNormal + 0.1f);
            Assert.Less(pertHide, pertNormal - 0.05f);
        }
    }
}
