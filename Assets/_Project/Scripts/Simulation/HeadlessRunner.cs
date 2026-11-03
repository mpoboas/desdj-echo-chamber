using System;
using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>Runs the full 3-day game without a scene, driven by an AutoPolicy. Used by tests and balancing.</summary>
    public static class HeadlessRunner
    {
        public static RunResult Run(ContentSet content, TuningData tuning, AutoPolicy policy, int seed,
                                    float dt = 0.05f, float decideSecondsPerPost = 6f)
        {
            var sim = new GameSimulation(content, tuning, seed);
            var result = new RunResult { Policy = policy.Name, Seed = seed };
            sim.EventRaised += e => result.Events.Add(e.Kind);
            sim.StartGame();

            int guard = 0;
            while (sim.Phase != GamePhase.End)
            {
                if (++guard > 5_000_000) throw new InvalidOperationException("Headless run did not finish");
                switch (sim.Phase)
                {
                    case GamePhase.Decide:
                        RunDecide(sim, policy, dt, decideSecondsPerPost);
                        break;
                    case GamePhase.Simulate:
                        sim.Tick(dt);
                        break;
                    case GamePhase.Report:
                        result.TurnsPlayed++;
                        sim.ContinueAfterReport();
                        break;
                    case GamePhase.DayEnd:
                        sim.BeginNextDay();
                        break;
                }
            }

            result.Ending = sim.FinalEnding.Value;
            result.RevenueByDay = (int[])sim.RevenueByDay.Clone();
            result.Cohesion = sim.Metrics.Cohesion;
            result.Tension = sim.Metrics.Tension;
            result.Apathy = Metrics.AvgApat(sim.Neighbours);
            foreach (var day in sim.DayResults)
                foreach (var g in day)
                {
                    result.GoalsTotal++;
                    if (g.Achieved) result.GoalsAchieved++;
                }
            result.History.AddRange(sim.History);
            return result;
        }

        private static void RunDecide(GameSimulation sim, AutoPolicy policy, float dt, float perPost)
        {
            for (int i = 0; i < sim.Weights.Length; i++) sim.Weights[i] = policy.Weights[i];
            sim.DecideClockRunning = true;
            float wait = perPost * sim.TurnPosts.Count;
            for (float t = 0f; t < wait; t += dt) sim.Tick(dt);

            var decisions = new List<KeyValuePair<PostData, DecisionMode>>();
            foreach (var post in sim.TurnPosts)
                decisions.Add(new KeyValuePair<PostData, DecisionMode>(post, policy.Decide(post)));
            sim.Publish(decisions);
        }

        public static Dictionary<EndingId, int> Sweep(ContentSet content, TuningData tuning, AutoPolicy policy, int seeds)
        {
            var counts = new Dictionary<EndingId, int>();
            for (int s = 1; s <= seeds; s++)
            {
                var r = Run(content, tuning, policy, s);
                counts.TryGetValue(r.Ending, out int c);
                counts[r.Ending] = c + 1;
            }
            return counts;
        }
    }
}
