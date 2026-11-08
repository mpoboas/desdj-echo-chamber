using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>
    /// v2 core: the player gives one post to one neighbour (or discards it). Pure C#, deterministic via SimRandom.
    /// Engagement is what the company sees. Hidden effects (bubble, aggression, isolation) only leave through Signal().
    /// </summary>
    public class FeedSystem
    {
        private readonly FeedTuning t;
        private readonly SimRandom rng;
        private readonly List<FeedPerson> people = new List<FeedPerson>();

        public IReadOnlyList<FeedPerson> People { get { return people; } }
        public float TotalMinutes { get; private set; }

        public FeedSystem(FeedContent content, FeedTuning tuning, SimRandom random, int day)
        {
            t = tuning;
            rng = random;
            this.content = content;
            EnsurePeople(day);
        }

        private readonly FeedContent content;

        /// <summary>Adds the neighbours that join on or before this day. Hidden state of existing people is kept.</summary>
        public void EnsurePeople(int day)
        {
            foreach (var p in content.profiles)
            {
                if (p.startDay > day) continue;
                bool have = false;
                foreach (var q in people) if (q.Profile == p) { have = true; break; }
                if (!have) people.Add(new FeedPerson(p));
            }
        }

        /// <summary>Hidden effects of someone having watched this post (no engagement accounting).</summary>
        public void ApplyExposure(FeedPost post, FeedPerson who)
        {
            ApplyEffects(post, who, Matches(post, who));
        }

        public FeedPerson Find(string name)
        {
            foreach (var p in people)
                if (p.Profile.name == name || p.Profile.name.EndsWith(" " + name)) return p;
            return null;
        }

        public bool Matches(FeedPost post, FeedPerson who)
        {
            return who.Profile.interests.Contains(post.theme);
        }

        public FeedResult Give(FeedPost post, FeedPerson who)
        {
            bool match = Matches(post, who);
            float m = t.baseMinutes;
            if (match) m *= t.matchMultiplier;
            if (post.inflamed) m *= t.inflamedMultiplier;
            float repeat = t.repeatBonusPerSeen * who.Seen(post.theme);
            if (repeat > t.repeatBonusCap) repeat = t.repeatBonusCap;
            m *= 1f + repeat;
            m *= 1f + rng.Range(-t.jitter, t.jitter);

            who.EngagementMinutes += m;
            TotalMinutes += m;
            who.PostsSeen++;
            who.SeenByTheme[post.theme] = who.Seen(post.theme) + 1;

            ApplyEffects(post, who, match);

            var reaction = m >= t.likeThreshold ? FeedReaction.Like
                         : m >= t.dwellThreshold ? FeedReaction.Dwell : FeedReaction.Pass;
            who.LastReaction = reaction;
            return new FeedResult { Person = who, Post = post, Minutes = m, Reaction = reaction };
        }

        public FeedResult Discard(FeedPost post)
        {
            TotalMinutes -= t.discardPenaltyMinutes;
            return new FeedResult { Post = post, Minutes = -t.discardPenaltyMinutes, Discarded = true };
        }

        private void ApplyEffects(FeedPost post, FeedPerson who, bool match)
        {
            if (post.inflamed)
            {
                who.Aggression += t.inflamedAggression * (1f - 0.5f * who.Profile.openness);
                who.Bubble += t.inflamedBubble;
            }
            else
            {
                who.Aggression -= t.calmAggressionDrop;
                if (match && post.role != FeedRole.Ponte) who.Bubble += t.matchBubble;
            }

            if (post.theme == FeedTheme.Meme || post.theme == FeedTheme.Desporto)
            {
                who.Isolation += t.passiveIsolation;
                who.Apathy += t.passiveIsolation;
            }

            if (post.role == FeedRole.Ponte)
            {
                who.Bubble -= t.bridgeBubbleDrop;
                who.Isolation -= t.bridgeIsolationDrop;
                who.Apathy -= t.bridgeIsolationDrop;
            }

            foreach (var name in post.mentions)
            {
                var other = Find(name);
                if (other != null && other != who) other.Stress += t.mentionedStress;
            }

            Clamp(who);
        }

        private static void Clamp(FeedPerson p)
        {
            p.Aggression = Clamp01(p.Aggression);
            p.Bubble = Clamp01(p.Bubble);
            p.Isolation = Clamp01(p.Isolation);
            p.Apathy = Clamp01(p.Apathy);
            p.Stress = Clamp01(p.Stress);
        }

        private static float Clamp01(float v)
        {
            return v < 0f ? 0f : v > 1f ? 1f : v;
        }

        /// <summary>The one visible behaviour that best describes this person right now.</summary>
        public FeedSignal Signal(FeedPerson p)
        {
            if (p.Aggression >= t.argueAggression) return FeedSignal.ArguesInSquare;
            if (p.Isolation >= t.stayHomeIsolation) return FeedSignal.StaysHome;
            if (p.Apathy >= t.headDownApathy) return FeedSignal.HeadDown;
            if (p.Bubble >= t.avoidBubble) return FeedSignal.AvoidsNeighbour;
            if (p.Bubble <= t.healthyBubbleMax && p.Profile.social >= 0.6f) return FeedSignal.VisitsAndSmiles;
            return FeedSignal.GoesToCafe;
        }
    }
}
