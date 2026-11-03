using System.Collections.Generic;
using NUnit.Framework;

namespace EchoChamber.Tests
{
    public class PostDeliveryTests
    {
        private static PostData Post(PostCategory cat, float medo = 0, float emp = 0, float desc = 0, float cons = 0, float apat = 0, float value = 1f)
        {
            return new PostData
            {
                text = "x" + cat,
                category = cat,
                value = value,
                effects = new PostEffects { medo = medo, emp = emp, desc = desc, cons = cons, apat = apat }
            };
        }

        private static Delivery MakeDelivery(PostData p, DecisionMode m)
        {
            return new Delivery { Entry = new TurnPostEntry { Post = p, Mode = m } };
        }

        private static GameSimulation SimWhereEveryoneSees()
        {
            var sim = TestContent.Sim(new ConstantRandom(0f));
            foreach (var n in sim.Neighbours)
            {
                n.Stress = 0.2f; n.Pert = 0.5f; n.Desc = 0.1f; n.Apat = 0.2f; n.Compra = 0.3f; n.Phone = 0.9f;
                n.Action = ActionKind.Casa;
            }
            return sim;
        }

        [Test]
        public void ViewProbabilityMatchesTheSpecFormula()
        {
            var sim = TestContent.Sim();
            // 0.5 * (0.4 + 1.2*0.5) * 1 * 0.6 = 0.3
            Assert.AreEqual(0.3f, sim.Deliverer.ViewProbability(0.5f, 1f, 0.6f, false), 0.0001f);
            // amplify, on screen, clamped to 0.97
            Assert.AreEqual(0.97f, sim.Deliverer.ViewProbability(1f, 2.2f, 0.9f, true), 0.0001f);
            // hide
            Assert.AreEqual(0.5f * 1.0f * 0.1f * 0.6f, sim.Deliverer.ViewProbability(0.5f, 0.1f, 0.6f, false), 0.0001f);
        }

        [Test]
        public void DeliverChangesStatesLikeThePrototype()
        {
            var sim = SimWhereEveryoneSees();
            var n = sim.Neighbours[0];
            Assert.AreEqual(Trait.Solidary, n.Trait);
            float stress0 = n.Stress, pert0 = n.Pert, desc0 = n.Desc;
            var d = MakeDelivery(Post(PostCategory.Sens, medo: 2, desc: 1, emp: 1), DecisionMode.Normal);
            sim.Deliverer.Deliver(d, sim.Neighbours, sim.Weights, false);
            const float k = 0.07f;
            Assert.AreEqual(stress0 + (2f + 0.4f * 1f) * k, n.Stress, 0.0001f);
            Assert.AreEqual(desc0 + 1f * k, n.Desc, 0.0001f);
            // solidary: emp x1.4
            Assert.AreEqual(pert0 + (1.4f - 0.4f * 1f) * k, n.Pert, 0.0001f);
            Assert.AreEqual(14, d.Entry.Viewers);
        }

        [Test]
        public void TraitsScaleTheEffects()
        {
            var sim = SimWhereEveryoneSees();
            var anxious = sim.Neighbours[2];
            var sceptic = sim.Neighbours[1];
            Assert.AreEqual(Trait.Anxious, anxious.Trait);
            Assert.AreEqual(Trait.Sceptic, sceptic.Trait);
            var d = MakeDelivery(Post(PostCategory.Sens, medo: 2), DecisionMode.Normal);
            sim.Deliverer.Deliver(d, sim.Neighbours, sim.Weights, false);
            Assert.AreEqual(0.2f + 2f * 1.5f * 0.07f, anxious.Stress, 0.0001f);
            Assert.AreEqual(0.2f + 2f * 0.55f * 0.07f, sceptic.Stress, 0.0001f);
        }

        [Test]
        public void StatesAreClamped()
        {
            var sim = SimWhereEveryoneSees();
            foreach (var n in sim.Neighbours) n.Stress = 0.99f;
            var d = MakeDelivery(Post(PostCategory.Sens, medo: 4), DecisionMode.Amplify);
            sim.Deliverer.Deliver(d, sim.Neighbours, sim.Weights, false);
            foreach (var n in sim.Neighbours) Assert.LessOrEqual(n.Stress, 1f);
        }

        [Test]
        public void HideShowsPostToFarFewerNeighboursThanAmplify()
        {
            int amp = 0, hide = 0;
            for (int seed = 1; seed <= 30; seed++)
            {
                var s1 = TestContent.Sim(seed);
                var s2 = TestContent.Sim(seed);
                var p = Post(PostCategory.Ent, apat: 2);
                var a = MakeDelivery(p, DecisionMode.Amplify);
                var h = MakeDelivery(p, DecisionMode.Hide);
                s1.Deliverer.Deliver(a, s1.Neighbours, s1.Weights, false);
                s2.Deliverer.Deliver(h, s2.Neighbours, s2.Weights, false);
                amp += a.Entry.Viewers;
                hide += h.Entry.Viewers;
            }
            Assert.Greater(amp, hide * 3);
        }

        [Test]
        public void HiddenPostsEarnNothing()
        {
            var sim = SimWhereEveryoneSees();
            var d = MakeDelivery(Post(PostCategory.Sens, medo: 1, value: 2f), DecisionMode.Hide);
            int rev = sim.Deliverer.Deliver(d, sim.Neighbours, sim.Weights, false);
            Assert.AreEqual(0, rev);
        }

        [Test]
        public void RevenueFollowsTheFormula()
        {
            var sim = TestContent.Sim();
            var post = Post(PostCategory.Sens, value: 1.5f);
            // 10 viewers * 1.5 * 1.6 = 24
            Assert.AreEqual(24, sim.Deliverer.Revenue(post, DecisionMode.Normal, 10, 0.5f, false));
            Assert.AreEqual(12, sim.Deliverer.Revenue(post, DecisionMode.Normal, 10, 0.5f, true));
            var ad = Post(PostCategory.Pub, value: 0.7f);
            // 10*0.7*1.6 = 11.2 ; + 10*2*(0.5+0.5) = 20 => 31.2 => 31
            Assert.AreEqual(31, sim.Deliverer.Revenue(ad, DecisionMode.Normal, 10, 0.5f, false));
        }

        [Test]
        public void ReactionIsTheStrongestEffectAboveThreshold()
        {
            var sim = SimWhereEveryoneSees();
            var d = MakeDelivery(Post(PostCategory.Sens, medo: 3, desc: 1), DecisionMode.Normal);
            sim.Deliverer.Deliver(d, sim.Neighbours, sim.Weights, false);
            var anxious = sim.Neighbours[2];
            Assert.AreEqual(ReactionKind.Medo, anxious.Reaction.Kind);
            Assert.IsTrue(anxious.Reaction.Strong); // 1.5x raw crosses the 1.2x factor
            Assert.AreEqual(9f, anxious.Reaction.TimeLeft);

            var weak = MakeDelivery(Post(PostCategory.Loc, emp: 1), DecisionMode.Normal);
            sim.Deliverer.Deliver(weak, sim.Neighbours, sim.Weights, false);
            Assert.AreEqual(ReactionKind.Indiferente, sim.Neighbours[2].Reaction.Kind);
        }

        [Test]
        public void EstimatedRevenueUsesExpectedViewers()
        {
            var sim = TestContent.Sim();
            var p = Post(PostCategory.Ent, apat: 2, value: 1.3f);
            float ev = 0f;
            foreach (var n in sim.Neighbours) ev += SimMath.Clamp(0.5f * (0.4f + 0.5f * 1.2f) * n.Phone, 0f, 0.97f);
            int expected = (int)System.Math.Round(ev * 1.3f * 1.6f, System.MidpointRounding.AwayFromZero);
            Assert.AreEqual(expected, sim.EstimateRevenue(p));
        }
    }
}
