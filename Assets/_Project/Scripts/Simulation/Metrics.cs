using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>Neighbourhood averages, tension and cohesion (PrototypeSpec section 5).</summary>
    public class Metrics
    {
        private readonly TuningData t;

        public float Cohesion;
        public float Tension;

        public Metrics(TuningData tuning)
        {
            t = tuning;
            Cohesion = t.initialCohesion;
            Tension = t.initialTension;
        }

        public static float Average(IList<Neighbour> all, System.Func<Neighbour, float> selector)
        {
            if (all.Count == 0) return 0f;
            float s = 0f;
            for (int i = 0; i < all.Count; i++) s += selector(all[i]);
            return s / all.Count;
        }

        public static float AvgStress(IList<Neighbour> all) => Average(all, n => n.Stress);
        public static float AvgDesc(IList<Neighbour> all) => Average(all, n => n.Desc);
        public static float AvgPert(IList<Neighbour> all) => Average(all, n => n.Pert);
        public static float AvgApat(IList<Neighbour> all) => Average(all, n => n.Apat);
        public static float AvgCompra(IList<Neighbour> all) => Average(all, n => n.Compra);

        public float ComputeTension(float stress, float desc, bool riot, bool police)
        {
            return SimMath.Clamp01(stress * t.tensionStress + desc * t.tensionDesc
                                   + (riot ? t.tensionRiot : 0f) + (police ? t.tensionPolice : 0f));
        }

        public float ComputeCohesion(float pert, float desc, float apat)
        {
            return SimMath.Clamp01(pert * (1f - desc * t.cohesionDescPenalty) * t.cohesionScale - apat * t.cohesionApatPenalty);
        }

        public void Update(IList<Neighbour> all, bool riot, bool police)
        {
            Tension = ComputeTension(AvgStress(all), AvgDesc(all), riot, police);
            Cohesion = ComputeCohesion(AvgPert(all), AvgDesc(all), AvgApat(all));
        }
    }
}
