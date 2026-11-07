using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>Weighted random post choice that reacts to the neighbourhood mood (PrototypeSpec section 2).</summary>
    public class PostPicker
    {
        private readonly TuningData t;
        private readonly ContentSet content;
        private readonly SimRandom rng;
        private readonly List<string> recent = new List<string>();

        public PostPicker(TuningData tuning, ContentSet content, SimRandom rng)
        {
            t = tuning;
            this.content = content;
            this.rng = rng;
        }

        public float[] Weights(float stress, float desc, float pert, float apat)
        {
            var w = new float[5];
            w[(int)PostCategory.Sens] = t.pickSensBase + stress * t.pickSensStress + desc * t.pickSensDesc;
            w[(int)PostCategory.Com] = t.pickComBase + pert * t.pickComPert;
            w[(int)PostCategory.Ent] = t.pickEntBase + apat * t.pickEntApat;
            w[(int)PostCategory.Pub] = t.pickPub;
            w[(int)PostCategory.Loc] = t.pickLoc;
            return w;
        }

        /// <summary>
        /// Picks a high-value sensationalism or advertising post for the boss to impose.
        /// Skips posts already in the turn. Falls back to any high-value sensationalism post.
        /// </summary>
        public PostData PickImposed(ICollection<PostData> alreadyInTurn)
        {
            PostCategory cat = rng.Value() < t.imposedSensChance ? PostCategory.Sens : PostCategory.Pub;
            var opts = ImposedOptions(cat, alreadyInTurn);
            if (opts.Count == 0) opts = ImposedOptions(cat == PostCategory.Sens ? PostCategory.Pub : PostCategory.Sens, alreadyInTurn);
            if (opts.Count == 0) opts = ImposedOptions(PostCategory.Sens, null);
            return opts[rng.Index(opts.Count)];
        }

        private List<PostData> ImposedOptions(PostCategory cat, ICollection<PostData> exclude)
        {
            var opts = new List<PostData>();
            foreach (var p in content.pool)
                if (p.category == cat && !p.dilemma && p.value >= t.imposedMinValue && (exclude == null || !exclude.Contains(p))) opts.Add(p);
            return opts;
        }

        private bool HasDilemma(PostCategory cat)
        {
            foreach (var p in content.pool)
                if (p.category == cat && p.dilemma) return true;
            return false;
        }

        public PostData Pick(IList<Neighbour> all)
        {
            float[] w = Weights(Metrics.AvgStress(all), Metrics.AvgDesc(all), Metrics.AvgPert(all), Metrics.AvgApat(all));
            float tot = 0f;
            for (int i = 0; i < w.Length; i++) tot += w[i];
            float r = rng.Value() * tot;
            PostCategory cat = PostCategory.Loc;
            for (int i = 0; i < w.Length; i++)
            {
                r -= w[i];
                if (r <= 0f)
                {
                    cat = (PostCategory)i;
                    break;
                }
            }

            // Dilemma posts are only rolled for when the category has some, so content without them plays as before.
            bool wantDilemma = HasDilemma(cat) && rng.Value() < t.dilemmaChance;
            var opts = new List<PostData>();
            foreach (var p in content.pool)
                if (p.category == cat && p.dilemma == wantDilemma && !recent.Contains(p.text)) opts.Add(p);
            if (opts.Count == 0)
                foreach (var p in content.pool)
                    if (p.category == cat && p.dilemma == wantDilemma) opts.Add(p);

            PostData chosen = opts[rng.Index(opts.Count)];
            recent.Add(chosen.text);
            if (recent.Count > t.pickRecentMemory) recent.RemoveAt(0);
            return chosen;
        }
    }
}
