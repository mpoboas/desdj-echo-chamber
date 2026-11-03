using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>Utility AI: scores every action for a neighbour and picks the best one (PrototypeSpec section 3).</summary>
    public class UtilityBrain
    {
        private readonly TuningData t;
        private readonly SimRandom rng;

        /// <summary>Last scores per action, kept for the debug panel only.</summary>
        public readonly Dictionary<ActionKind, float> LastScores = new Dictionary<ActionKind, float>();

        public UtilityBrain(TuningData tuning, SimRandom rng)
        {
            t = tuning;
            this.rng = rng;
        }

        /// <summary>Raw utility (no noise, no current-action penalty).</summary>
        public float Score(Neighbour n, ActionKind action, BrainContext c)
        {
            float night = c.IsNight ? t.nightBonus : 0f;
            switch (action)
            {
                case ActionKind.Casa:
                    return t.casaBase + n.Stress * t.casaStress + night + (n.IsAnxious ? n.Stress * t.casaAnxiousStress : 0f);
                case ActionKind.Cafe:
                    return t.cafeBase + n.Pert * t.cafePertW + (1f - n.Stress) * t.cafeCalm - n.Apat * t.cafeApat;
                case ActionKind.Mercearia:
                    return c.MerceariaOpen ? t.merceariaBase + n.Compra * t.merceariaCompra : t.utilityClosed;
                case ActionKind.Livraria:
                    return c.LivrariaOpen
                        ? t.livrariaBase + n.Pert * t.livrariaPert + n.LivAff * t.livrariaAff - n.Apat * t.livrariaApat
                        : t.utilityClosed;
                case ActionKind.Junta:
                    return t.juntaBase + n.Desc * t.juntaDescW + n.Stress * t.juntaStress - n.Apat * t.juntaApat;
                case ActionKind.Praca:
                    return t.pracaBase + (1f - n.Stress) * t.pracaCalm + n.Pert * t.pracaPertW
                           - (c.PoliceOnStreet ? t.pracaPolice : 0f) - night * t.pracaNight
                           + (c.FestaActive ? t.pracaFesta : 0f);
                case ActionKind.Ecra:
                    return t.ecraBase + n.Apat * t.ecraApatW + (n.IsAddict ? t.ecraAddict : 0f);
                default:
                    return t.utilityClosed;
            }
        }

        public ActionKind Choose(Neighbour n, BrainContext c)
        {
            ActionKind best = ActionKind.Casa;
            float bestScore = float.NegativeInfinity;
            // Same evaluation order as the prototype object, so ties resolve identically.
            foreach (ActionKind a in System.Enum.GetValues(typeof(ActionKind)))
            {
                float u = Score(n, a, c);
                if (a == n.Action) u -= t.currentActionPenalty;
                u += rng.Value() * t.utilityNoise;
                LastScores[a] = u;
                if (u > bestScore)
                {
                    bestScore = u;
                    best = a;
                }
            }
            return best;
        }
    }
}
