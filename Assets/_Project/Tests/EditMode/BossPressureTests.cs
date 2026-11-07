using System.Collections.Generic;
using NUnit.Framework;

namespace EchoChamber.Tests
{
    public class BossPressureTests
    {
        private static void PlayTurnHidingEverything(GameSimulation sim)
        {
            var decisions = new List<KeyValuePair<PostData, DecisionMode>>();
            foreach (var p in sim.TurnPosts) decisions.Add(new KeyValuePair<PostData, DecisionMode>(p, DecisionMode.Hide));
            sim.Publish(decisions);
            while (sim.Phase == GamePhase.Simulate) sim.Tick(0.1f);
        }

        /// <summary>Plays one whole day hiding every post, so the revenue goal is missed, and opens the next day.</summary>
        private static void FailDayAndOpenNext(GameSimulation sim)
        {
            for (int turn = 0; turn < sim.Tuning.turnsPerDay; turn++)
            {
                PlayTurnHidingEverything(sim);
                sim.ContinueAfterReport();
            }
            Assert.AreEqual(GamePhase.DayEnd, sim.Phase);
            sim.BeginNextDay();
        }

        [Test]
        public void NoPressureAndNoImposedPostsAtTheStart()
        {
            var sim = TestContent.Sim();
            sim.StartGame();
            Assert.AreEqual(0, sim.Pressure);
            Assert.AreEqual(0, sim.ImposedPosts.Count);
        }

        [Test]
        public void FailingTheRevenueGoalRaisesPressureAndAddsOneImposedPostPerTurn()
        {
            var sim = TestContent.Sim();
            sim.StartGame();
            int scripted = sim.TurnPosts.Count;
            FailDayAndOpenNext(sim);

            Assert.AreEqual(1, sim.Pressure);
            for (int turn = 0; turn < sim.Tuning.turnsPerDay; turn++)
            {
                Assert.AreEqual(sim.Tuning.imposedPerLevel, sim.ImposedPosts.Count, "turn " + turn);
                foreach (var p in sim.ImposedPosts)
                {
                    Assert.IsTrue(sim.TurnPosts.Contains(p));
                    Assert.IsTrue(p.category == PostCategory.Sens || p.category == PostCategory.Pub);
                    Assert.GreaterOrEqual(p.value, sim.Tuning.imposedMinValue);
                }
                PlayTurnHidingEverything(sim);
                if (turn < sim.Tuning.turnsPerDay - 1) sim.ContinueAfterReport();
            }
            Assert.Greater(scripted, 0);
        }

        [Test]
        public void ImposedPostsCannotBeHidden()
        {
            var sim = TestContent.Sim();
            sim.StartGame();
            FailDayAndOpenNext(sim);
            PlayTurnHidingEverything(sim);

            int imposed = 0;
            foreach (var e in sim.Log.Posts)
            {
                if (e.Imposed)
                {
                    imposed++;
                    Assert.AreEqual(DecisionMode.Normal, e.Mode);
                }
            }
            Assert.AreEqual(sim.Tuning.imposedPerLevel, imposed);
        }

        [Test]
        public void PressureGrowsEachFailedDayAndIsCapped()
        {
            var sim = TestContent.Sim();
            sim.StartGame();
            FailDayAndOpenNext(sim);
            Assert.AreEqual(1, sim.Pressure);
            Assert.AreEqual(1 * sim.Tuning.imposedPerLevel, sim.ImposedPosts.Count);
            FailDayAndOpenNext(sim);
            Assert.AreEqual(sim.Tuning.pressureMax, sim.Pressure);
            Assert.AreEqual(sim.Tuning.pressureMax * sim.Tuning.imposedPerLevel, sim.ImposedPosts.Count);
        }

        [Test]
        public void MeetingTheGoalResetsPressure()
        {
            var sim = TestContent.Sim();
            sim.StartGame();
            sim.Pressure = sim.Tuning.pressureMax;
            sim.RevenueDay = sim.CurrentDay.target;
            sim.CloseDay();
            Assert.AreEqual(0, sim.Pressure);
        }

        [Test]
        public void ImposedPostsAreNeverDuplicatedInATurn()
        {
            var tuning = TestContent.Tuning();
            tuning.imposedPerLevel = 3;
            for (int seed = 1; seed <= 20; seed++)
            {
                var sim = new GameSimulation(TestContent.Content(), tuning, seed);
                sim.StartGame();
                sim.Pressure = 2;
                sim.StartTurn();
                var seen = new HashSet<PostData>();
                foreach (var p in sim.ImposedPosts) Assert.IsTrue(seen.Add(p));
                Assert.AreEqual(6, sim.ImposedPosts.Count);
            }
        }
    }
}
