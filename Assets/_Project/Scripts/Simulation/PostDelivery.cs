using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>Delivers one published post to the neighbours (PrototypeSpec section 4).</summary>
    public class PostDelivery
    {
        private readonly TuningData t;
        private readonly SimRandom rng;
        private int reactionSeq;

        public PostDelivery(TuningData tuning, SimRandom rng)
        {
            t = tuning;
            this.rng = rng;
        }

        /// <summary>Unique id for a reaction created outside of a delivery.</summary>
        public int NextReactionId() => ++reactionSeq;

        public float ModeMultiplier(DecisionMode mode)
        {
            switch (mode)
            {
                case DecisionMode.Amplify: return t.ampMultiplier;
                case DecisionMode.Hide: return t.hideMultiplier;
                default: return t.normalMultiplier;
            }
        }

        /// <summary>Probability that a neighbour sees a post.</summary>
        public float ViewProbability(float weight01, float modeMultiplier, float phone, bool onScreen)
        {
            return SimMath.Clamp(t.viewBase * (t.viewWeightOffset + weight01 * t.viewWeightScale) * modeMultiplier * phone
                                 * (onScreen ? t.viewEcraBoost : 1f), 0f, t.viewMax);
        }

        /// <summary>Estimated revenue shown on the slip before stamping (assumes normal mode).</summary>
        public int EstimateRevenue(PostData post, float weight01, IList<Neighbour> all, bool adsNervous)
        {
            float ev = 0f;
            foreach (var n in all)
                ev += SimMath.Clamp(t.viewBase * (t.viewWeightOffset + weight01 * t.viewWeightScale) * n.Phone, 0f, t.viewMax);
            float r = ev * post.value * t.revenuePerView
                      + (post.category == PostCategory.Pub ? ev * t.pubViewBonus * (t.pubWeightOffset + weight01) : 0f);
            if (adsNervous) r *= t.nervousRevenueMultiplier;
            return (int)System.Math.Round(r, System.MidpointRounding.AwayFromZero);
        }

        public int Revenue(PostData post, DecisionMode mode, int viewers, float weight01, bool adsNervous)
        {
            float rev = mode == DecisionMode.Hide
                ? 0f
                : viewers * post.value * t.revenuePerView
                  + (post.category == PostCategory.Pub ? viewers * t.pubViewBonus * (t.pubWeightOffset + weight01) : 0f);
            if (adsNervous) rev *= t.nervousRevenueMultiplier;
            return (int)System.Math.Round(rev, System.MidpointRounding.AwayFromZero);
        }

        private ReactionKind Top(float medo, float raiva, float empatia, float compra, float apatia)
        {
            ReactionKind kind = ReactionKind.Indiferente;
            float best = t.reactionMin;
            if (medo > best) { best = medo; kind = ReactionKind.Medo; }
            if (raiva > best) { best = raiva; kind = ReactionKind.Raiva; }
            if (empatia > best) { best = empatia; kind = ReactionKind.Empatia; }
            if (compra > best) { best = compra; kind = ReactionKind.Compra; }
            if (apatia > best) { best = apatia; kind = ReactionKind.Apatia; }
            return kind;
        }

        /// <summary>Applies the post to every neighbour that sees it. Returns revenue.</summary>
        public int Deliver(Delivery d, IList<Neighbour> all, float[] weights, bool adsNervous)
        {
            PostData p = d.Entry.Post;
            DecisionMode mode = d.Entry.Mode;
            float w = weights[(int)p.category] / 100f;
            float mult = ModeMultiplier(mode);
            float k = t.effectK;

            float bStress = Metrics.AvgStress(all), bPert = Metrics.AvgPert(all), bDesc = Metrics.AvgDesc(all),
                  bApat = Metrics.AvgApat(all), bCompra = Metrics.AvgCompra(all);

            foreach (Neighbour n in all)
            {
                float pv = ViewProbability(w, mult, n.Phone, n.Action == ActionKind.Ecra);
                if (rng.Value() >= pv) continue;
                d.Viewers.Add(n);

                PostEffects e = p.effects;
                Trait tm = n.Trait;
                float medo = e.medo * (tm == Trait.Anxious ? t.anxiousMedo : tm == Trait.Sceptic ? t.scepticMedo : 1f);
                float desc = e.desc * (tm == Trait.Sceptic ? t.scepticDesc : 1f);
                float emp = e.emp * (tm == Trait.Solidary ? t.solidaryEmp : 1f);
                float cons = e.cons * (tm == Trait.Consumerist ? t.consumeristCons : 1f);
                float apat = e.apat * (tm == Trait.Addict ? t.addictApat : 1f);

                n.Stress = SimMath.Clamp01(n.Stress + (medo + desc * t.descToStress) * k);
                n.Desc = SimMath.Clamp01(n.Desc + desc * k);
                n.Pert = SimMath.Clamp01(n.Pert + (emp - desc * t.descToPert - apat * t.apatToPert) * k);
                n.Compra = SimMath.Clamp01(n.Compra + cons * k * t.compraBoost);
                n.Apat = SimMath.Clamp01(n.Apat + apat * k - emp * k * t.empToApat);
                if (p.livraria != 0f) n.LivAff = SimMath.Clamp01(n.LivAff + p.livraria * t.livAffPost);
                n.PostsSeen++;

                float empL = tm == Trait.Reader && p.livraria != 0f ? t.readerLivEmpMultiplier : 1f;
                float rawMedo = e.medo + e.desc * t.descToStress;
                float rawRaiva = e.desc * t.raivaFromDesc;
                float rawEmp = e.emp;
                float rawCons = e.cons;
                float rawApat = e.apat;
                float effMedo = medo + desc * t.descToStress;
                float effRaiva = desc * t.raivaFromDesc;
                float effEmp = emp * empL;

                ReactionKind kind = Top(effMedo, effRaiva, effEmp, cons, apat);
                ReactionKind baseKind = Top(rawMedo, rawRaiva, rawEmp, rawCons, rawApat);
                float effOfKind = kind == ReactionKind.Medo ? effMedo : kind == ReactionKind.Raiva ? effRaiva
                    : kind == ReactionKind.Empatia ? effEmp : kind == ReactionKind.Compra ? cons : kind == ReactionKind.Apatia ? apat : 0f;
                float rawOfKind = kind == ReactionKind.Medo ? rawMedo : kind == ReactionKind.Raiva ? rawRaiva
                    : kind == ReactionKind.Empatia ? rawEmp : kind == ReactionKind.Compra ? rawCons : kind == ReactionKind.Apatia ? rawApat : 0f;

                n.Reaction = new Reaction
                {
                    Id = ++reactionSeq,
                    Kind = kind,
                    Base = baseKind,
                    Strong = kind != ReactionKind.Indiferente && effOfKind > rawOfKind * t.reactionStrongFactor,
                    Category = p.category,
                    TimeLeft = t.reactionDuration
                };
                if (kind == ReactionKind.Empatia && empL > 1f) n.LivAff = SimMath.Clamp01(n.LivAff + t.livAffReaderBonus);
                n.IconCategory = p.category;
                n.IconTime = t.iconDuration;
                if (!n.IsMoving) n.Dwell = System.Math.Min(n.Dwell, rng.Range(t.dwellAfterPost));
            }

            int rev = Revenue(p, mode, d.Viewers.Count, w, adsNervous);

            d.Entry.Viewers = d.Viewers.Count;
            d.Entry.Revenue = rev;
            foreach (var v in d.Viewers) d.Entry.ViewerNames.Add(v.Name);
            d.Entry.DeltaMedo = Metrics.AvgStress(all) - bStress;
            d.Entry.DeltaEmp = Metrics.AvgPert(all) - bPert;
            d.Entry.DeltaDesc = Metrics.AvgDesc(all) - bDesc;
            d.Entry.DeltaApat = Metrics.AvgApat(all) - bApat;
            d.Entry.DeltaCons = Metrics.AvgCompra(all) - bCompra;
            d.Entry.Delivered = true;
            d.Started = true;
            return rev;
        }
    }
}
