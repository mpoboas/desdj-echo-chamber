using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoChamber
{
    /// <summary>What the player has learned: noted reactions, discovered traits and relations, "perceived neighbourhood".</summary>
    public class DiscoveryModel
    {
        private readonly TuningData t;
        private readonly HashSet<string> relations = new HashSet<string>();

        /// <summary>Category -> reaction kind -> times the player noted it.</summary>
        public readonly Dictionary<PostCategory, Dictionary<ReactionKind, int>> CategoryObservations =
            new Dictionary<PostCategory, Dictionary<ReactionKind, int>>();

        public DiscoveryModel(TuningData tuning)
        {
            t = tuning;
        }

        public int RelationCount => relations.Count;

        public static string RelationKey(int a, int b)
        {
            return a < b ? a + "-" + b : b + "-" + a;
        }

        public bool HasRelation(int a, int b)
        {
            return relations.Contains(RelationKey(a, b));
        }

        public IEnumerable<string> RelationKeys => relations;

        public static ReactionKind SignatureOf(Trait trait)
        {
            switch (trait)
            {
                case Trait.Anxious: return ReactionKind.Medo;
                case Trait.Sceptic: return ReactionKind.Indiferente;
                case Trait.Addict: return ReactionKind.Apatia;
                case Trait.Solidary: return ReactionKind.Empatia;
                case Trait.Consumerist: return ReactionKind.Compra;
                case Trait.Reader: return ReactionKind.Empatia;
                default: return ReactionKind.Indiferente;
            }
        }

        public ObserveResult Observe(Neighbour n, IList<Neighbour> all)
        {
            var result = new ObserveResult();
            Reaction r = n.Reaction;
            if (r == null || r.TimeLeft <= 0f || n.Notes.Any(x => x.ReactionId == r.Id))
                return result;

            n.Notes.Add(new ObservationNote { ReactionId = r.Id, Category = r.Category, Kind = r.Kind, Strong = r.Strong, Base = r.Base });
            if (!CategoryObservations.TryGetValue(r.Category, out var co))
            {
                co = new Dictionary<ReactionKind, int>();
                CategoryObservations[r.Category] = co;
            }
            co.TryGetValue(r.Kind, out int count);
            co[r.Kind] = count + 1;
            result.Noted = true;
            result.Reaction = r;

            if (!n.TraitKnown)
            {
                ReactionKind sig = SignatureOf(n.Trait);
                bool hit = n.Notes.Any(x => sig == ReactionKind.Indiferente
                    ? (x.Kind == ReactionKind.Indiferente && x.Base != ReactionKind.Indiferente)
                    : (x.Kind == sig && x.Strong));
                if (hit || n.Notes.Count >= 3)
                {
                    n.TraitKnown = true;
                    result.TraitDiscovered = true;
                }
            }

            foreach (Neighbour m in all)
            {
                if (m == n) continue;
                string key = RelationKey(n.Id, m.Id);
                if (relations.Contains(key)) continue;
                n.Links.TryGetValue(m.Id, out float l1);
                m.Links.TryGetValue(n.Id, out float l2);
                float dx = m.X - n.X, dy = m.Y - n.Y;
                if (l1 + l2 > t.relationTime && Math.Sqrt(dx * dx + dy * dy) < t.relationRadius)
                {
                    relations.Add(key);
                    result.NewRelations.Add(m);
                }
            }
            return result;
        }

        /// <summary>Share of the neighbourhood the player has figured out, 0 to 1. Does not affect the ending.</summary>
        public float Understanding(IList<Neighbour> all)
        {
            int traits = all.Count(n => n.TraitKnown);
            int rels = Math.Min(t.relationCap, relations.Count);
            return (float)(traits + rels) / (all.Count + t.relationCap);
        }

        /// <summary>Top 3 reactions noted for a category, or null when fewer than 2 observations.</summary>
        public List<KeyValuePair<ReactionKind, int>> TopPattern(PostCategory cat)
        {
            if (!CategoryObservations.TryGetValue(cat, out var o)) return null;
            int total = o.Values.Sum();
            if (total < 2) return null;
            return o.OrderByDescending(kv => kv.Value).Take(3).ToList();
        }
    }
}
