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

            var opts = new List<PostData>();
            foreach (var p in content.pool)
                if (p.category == cat && !recent.Contains(p.text)) opts.Add(p);
            if (opts.Count == 0)
                foreach (var p in content.pool)
                    if (p.category == cat) opts.Add(p);

            PostData chosen = opts[rng.Index(opts.Count)];
            recent.Add(chosen.text);
            if (recent.Count > t.pickRecentMemory) recent.RemoveAt(0);
            return chosen;
        }
    }
}
