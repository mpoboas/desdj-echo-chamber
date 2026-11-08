using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>Runs the days in order, keeps neighbours' hidden state across days, warnings and used posts.</summary>
    public class FeedCareer
    {
        private readonly FeedContent content;
        private readonly FeedTuning t;
        private readonly SimRandom rng;
        private readonly HashSet<int> used = new HashSet<int>();
        private readonly List<FeedViewer> viewers = new List<FeedViewer>();

        public int Day { get; private set; }
        public int Warnings { get; private set; }
        public bool Fired { get { return Warnings >= t.warningsToBeFired; } }
        public bool Finished { get { return Fired || Day > t.dayGoals.Length; } }
        public FeedDay Current { get; private set; }
        public FeedSystem System { get; private set; }
        public IReadOnlyList<FeedViewer> Viewers { get { return viewers; } }

        public FeedCareer(FeedContent content, FeedTuning tuning, SimRandom random)
        {
            this.content = content;
            t = tuning;
            rng = random;
            Day = 1;
            System = new FeedSystem(content, tuning, random, 1);
            StartDay();
        }

        private void StartDay()
        {
            System.EnsurePeople(Day);
            foreach (var p in System.People)
            {
                bool have = false;
                foreach (var v in viewers) if (v.Person == p) { have = true; break; }
                if (!have) viewers.Add(new FeedViewer(p, content, System, t, rng, 0.2f + rng.Value() * 1.0f));
            }
            Current = new FeedDay(content, t, rng, Day, System, viewers, used);
        }

        /// <summary>Closes the current day. Returns true if the goal was met (no warning).</summary>
        public bool EndDay()
        {
            foreach (var p in Current.Given) used.Add(p.id);
            bool met = Current.GoalMet;
            if (!met) Warnings++;
            Day++;
            if (!Finished) StartDay();
            return met;
        }
    }
}
