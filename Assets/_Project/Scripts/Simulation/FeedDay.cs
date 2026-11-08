using System;
using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>
    /// One game day on the timeline: viewers scroll, posts reach the dock at fixed times, the player hands them out,
    /// and at the end the boss compares the total app time with the goal. Pure C#.
    /// </summary>
    public class FeedDay
    {
        private readonly FeedContent content;
        private readonly FeedTuning t;
        private readonly SimRandom rng;
        private readonly List<PlannedPost> plan = new List<PlannedPost>();
        private readonly List<FeedPost> dock = new List<FeedPost>();
        private readonly List<FeedPost> given = new List<FeedPost>();

        private struct PlannedPost
        {
            public float at;
            public FeedPost post;
        }

        public int Day { get; private set; }
        public FeedSystem System { get; private set; }
        public IReadOnlyList<FeedViewer> Viewers { get; private set; }
        public float Time { get; private set; }
        public bool Running { get; set; }
        public float Goal { get; private set; }
        public IReadOnlyList<FeedPost> Dock { get { return dock; } }
        public IReadOnlyList<FeedPost> Given { get { return given; } }
        public int PlannedCount { get { return plan.Count + dock.Count + given.Count; } }
        public bool DockChanged { get; set; }

        public float Minutes
        {
            get
            {
                float s = 0f;
                foreach (var v in Viewers) s += v.AppSeconds;
                return s / 60f - penalty;
            }
        }

        private float penalty;
        public bool GoalMet { get { return Minutes >= Goal; } }
        public bool Finished { get { return Time >= t.dayLength; } }

        public FeedDay(FeedContent content, FeedTuning tuning, SimRandom random, int day, FeedSystem system,
                       IReadOnlyList<FeedViewer> viewers, ICollection<int> usedPostIds)
        {
            this.content = content;
            t = tuning;
            rng = random;
            Day = day;
            System = system;
            Viewers = viewers;
            Goal = tuning.dayGoals[Math.Min(day, tuning.dayGoals.Length) - 1];
            foreach (var v in viewers) v.ResetForDay();
            Plan(usedPostIds);
        }

        private void Plan(ICollection<int> used)
        {
            var fresh = new List<FeedPost>();
            var older = new List<FeedPost>();
            foreach (var p in content.posts)
            {
                if (p.filler || p.minDay > Day) continue;
                if (used != null && used.Contains(p.id)) continue;
                (p.minDay == Day ? fresh : older).Add(p);
            }
            Shuffle(fresh);
            Shuffle(older);
            var q = new List<FeedPost>();
            for (int i = 0; i < fresh.Count && i < t.maxNewPostsPerDay && q.Count < t.postsPerDay; i++) q.Add(fresh[i]);
            foreach (var p in older) if (q.Count < t.postsPerDay) q.Add(p);
            for (int i = t.maxNewPostsPerDay; i < fresh.Count && q.Count < t.postsPerDay; i++) q.Add(fresh[i]);
            Shuffle(q);
            for (int i = 0; i < q.Count && i < t.postTimes.Length; i++) plan.Add(new PlannedPost { at = t.postTimes[i], post = q[i] });
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Index(i + 1);
                T tmp = list[i]; list[i] = list[j]; list[j] = tmp;
            }
        }

        public FeedViewer ViewerFor(FeedPerson p)
        {
            foreach (var v in Viewers) if (v.Person == p) return v;
            return null;
        }

        /// <summary>Advances the day. Viewers keep finishing their current video after the day stops.</summary>
        public void Step(float dt)
        {
            if (Running)
            {
                Time += dt;
                while (plan.Count > 0 && plan[0].at <= Time)
                {
                    dock.Add(plan[0].post);
                    plan.RemoveAt(0);
                    DockChanged = true;
                }
                if (Time >= t.dayLength) Running = false;
            }
            foreach (var v in Viewers) v.Step(dt, Running, PickDefault);
        }

        public bool Give(FeedPost post, FeedViewer viewer)
        {
            if (!Running || !dock.Remove(post)) return false;
            given.Add(post);
            viewer.Enqueue(post);
            DockChanged = true;
            return true;
        }

        public bool Discard(FeedPost post)
        {
            if (!Running || !dock.Remove(post)) return false;
            given.Add(post);
            penalty += t.discardPenaltyMinutes / 3f;
            DockChanged = true;
            return true;
        }

        /// <summary>Default algorithm: favours what the person likes and inflamed content.</summary>
        private FeedPost PickDefault(FeedViewer v)
        {
            bool filler = rng.Value() < t.fillerChance;
            var pool = new List<FeedPost>();
            foreach (var p in content.posts)
            {
                if (p.minDay > Day) continue;
                if (p.filler == filler || (!filler && !p.filler)) pool.Add(p);
            }
            if (pool.Count == 0) pool = content.posts;
            var weights = new float[pool.Count];
            float total = 0f;
            for (int i = 0; i < pool.Count; i++)
            {
                var p = pool[i];
                float w = (v.Person.Profile.interests.Contains(p.theme) ? 1f : 0.25f) + (p.inflamed ? 0.9f : 0f) + (p == v.Current ? -1f : 0f);
                weights[i] = Math.Max(0.02f, w);
                total += weights[i];
            }
            float x = rng.Value() * total;
            for (int i = 0; i < pool.Count; i++)
            {
                x -= weights[i];
                if (x <= 0f) return pool[i];
            }
            return pool[pool.Count - 1];
        }
    }
}
