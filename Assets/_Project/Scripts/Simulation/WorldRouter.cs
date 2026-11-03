using System;

namespace EchoChamber
{
    /// <summary>Waypoint routing in the logical world: doors, the walkway, and crosswalks (prototype routeTo).</summary>
    public class WorldRouter
    {
        private readonly TuningData t;
        private readonly ContentSet content;
        private readonly SimRandom rng;

        public WorldRouter(TuningData tuning, ContentSet content, SimRandom rng)
        {
            t = tuning;
            this.content = content;
            this.rng = rng;
        }

        public PathPoint ZonePoint(PlaceId id)
        {
            PlaceData p = content.FindPlace(id);
            float[] r = p.HasZone ? p.zone : p.rect;
            float m = p.HasZone ? t.zoneMarginWithZone : t.zoneMarginNoZone;
            return new PathPoint(rng.Range(r[0] + m, r[0] + r[2] - m), rng.Range(r[1] + m, r[1] + r[3] - m));
        }

        public void RouteTo(Neighbour n, PlaceId key)
        {
            n.Path.Clear();
            PlaceData cur = n.Place.HasValue ? content.FindPlace(n.Place.Value) : null;
            float sx = n.X;
            if (cur != null && !cur.open)
            {
                n.Path.Add(new PathPoint(cur.door[0], cur.door[1]));
                sx = cur.door[0];
            }
            if (!n.Place.HasValue || n.Place.Value != key)
            {
                PlaceData d = content.FindPlace(key);
                bool fromS = cur != null ? cur.open : n.Y > t.streetY;
                bool toS = d.open;
                if (fromS == toS)
                {
                    n.Path.Add(new PathPoint(sx, t.walkY));
                    n.Path.Add(new PathPoint(d.door[0], t.walkY));
                    n.Path.Add(new PathPoint(d.door[0], d.door[1]));
                }
                else
                {
                    float tx = toS ? n.X : d.door[0];
                    float mid = (sx + tx) / 2f;
                    float cw = Math.Abs(mid - t.crosswalkLeftX) < Math.Abs(mid - t.crosswalkRightX) ? t.crosswalkLeftX : t.crosswalkRightX;
                    if (fromS)
                    {
                        n.Path.Add(new PathPoint(cw, t.southY));
                        n.Path.Add(new PathPoint(cw, t.walkY, true));
                        n.Path.Add(new PathPoint(d.door[0], t.walkY));
                        n.Path.Add(new PathPoint(d.door[0], d.door[1]));
                    }
                    else
                    {
                        n.Path.Add(new PathPoint(sx, t.walkY));
                        n.Path.Add(new PathPoint(cw, t.walkY));
                        n.Path.Add(new PathPoint(cw, t.southY, true));
                    }
                }
            }
            n.Path.Add(ZonePoint(key));
            n.Place = null;
            n.Target = key;
        }
    }
}
