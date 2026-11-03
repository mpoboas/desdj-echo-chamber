using System.Collections.Generic;
using UnityEngine;

namespace EchoChamber
{
    /// <summary>Ambient traffic that obeys the shared 16 s traffic light cycle (ported from the prototype).</summary>
    public class TrafficSystem : MonoBehaviour
    {
        [System.Serializable]
        public class Car
        {
            public Transform body;
            public int dir = 1;
            public float speed = 1.5f;
            [HideInInspector] public float x3;
        }

        public List<Car> cars = new List<Car>();
        public float[] crosswalkProtoX;

        private void Start()
        {
            foreach (var c in cars) c.x3 = -c.body.position.x;
            if (crosswalkProtoX == null || crosswalkProtoX.Length == 0)
            {
                var t = GameManager.Instance.Tuning;
                crosswalkProtoX = new[] { WorldMap.ProtoX(t.crosswalkLeftX), WorldMap.ProtoX(t.crosswalkRightX) };
            }
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Sim == null) return;
            var sim = gm.Sim;
            float mul = (sim.Phase == GamePhase.Simulate ? 1f : 0.35f) * (sim.IsNightTurn ? 0.7f : 1f);
            CarLight light = sim.Traffic.Car;
            float dt = Time.deltaTime;

            foreach (var c in cars)
            {
                float v = c.speed * mul * dt;
                float x = c.x3;
                if (light != CarLight.Green)
                {
                    foreach (float cw in crosswalkProtoX)
                    {
                        float stopX = cw - c.dir * 0.95f;
                        float ahead = (stopX - x) * c.dir;
                        if (ahead >= -0.02f && ahead < v + 0.05f) v = Mathf.Max(0f, ahead);
                        else if (ahead >= -0.02f && ahead < 1.2f) v = Mathf.Min(v, Mathf.Max(0f, ahead) * 0.08f + 0.004f);
                    }
                }
                foreach (var o in cars)
                {
                    if (o == c || o.dir != c.dir) continue;
                    float gap = (o.x3 - x) * c.dir;
                    if (gap > 0f && gap < 1.25f) v = Mathf.Min(v, Mathf.Max(0f, gap - 1.05f));
                }
                c.x3 += c.dir * v;
                if (c.x3 > 12f) c.x3 = -12f;
                if (c.x3 < -12f) c.x3 = 12f;
                Vector3 p = c.body.position;
                p.x = -c.x3;
                c.body.position = p;
            }
        }
    }
}
