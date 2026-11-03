using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>Close neighbours at the same place pull each other's stress, distrust and belonging together.</summary>
    public class ContagionSystem
    {
        private readonly TuningData t;

        public ContagionSystem(TuningData tuning)
        {
            t = tuning;
        }

        public static bool IsHomePlace(PlaceId id)
        {
            return id == PlaceId.CasasN || id == PlaceId.CasasS;
        }

        public bool CanInteract(Neighbour a, Neighbour b)
        {
            if (!a.Place.HasValue || !b.Place.HasValue || a.Place.Value != b.Place.Value) return false;
            if (a.Action == ActionKind.Ecra || b.Action == ActionKind.Ecra) return false;
            if (IsHomePlace(a.Place.Value)) return false;
            float dx = a.X - b.X, dy = a.Y - b.Y;
            return System.Math.Sqrt(dx * dx + dy * dy) <= t.contagionRadius;
        }

        public void Step(IList<Neighbour> all, float dt)
        {
            for (int i = 0; i < all.Count; i++)
            {
                for (int j = i + 1; j < all.Count; j++)
                {
                    Neighbour a = all[i], b = all[j];
                    if (!CanInteract(a, b)) continue;
                    float k = t.contagionRate * dt;
                    float m = (b.Stress - a.Stress) * k;
                    a.Stress = SimMath.Clamp01(a.Stress + m);
                    b.Stress = SimMath.Clamp01(b.Stress - m);
                    m = (b.Desc - a.Desc) * k;
                    a.Desc = SimMath.Clamp01(a.Desc + m);
                    b.Desc = SimMath.Clamp01(b.Desc - m);
                    m = (b.Pert - a.Pert) * k;
                    a.Pert = SimMath.Clamp01(a.Pert + m);
                    b.Pert = SimMath.Clamp01(b.Pert - m);
                    a.Links.TryGetValue(b.Id, out float l);
                    a.Links[b.Id] = l + dt;
                }
            }
        }
    }
}
