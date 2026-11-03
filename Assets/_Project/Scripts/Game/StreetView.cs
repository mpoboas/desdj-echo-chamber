using UnityEngine;

namespace EchoChamber
{
    /// <summary>Syncs street props with the simulation: shutters, police car, square lights.</summary>
    public class StreetView : MonoBehaviour
    {
        public Transform merceariaShutter;
        public Transform livrariaShutter;
        public float shutterHeight = 0.5f;
        public float shutterTop = 0.82f;
        public Transform police;
        public GameObject policeBlue;
        public GameObject policeRed;
        public GameObject squareLights;

        private float merceariaClose = 0.02f;
        private float livrariaClose = 0.02f;

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Sim == null) return;
            var sim = gm.Sim;
            float dt = Time.deltaTime;

            merceariaClose = Mathf.Lerp(merceariaClose, sim.Rings.MerceariaOpen ? 0.02f : 1f, dt * 3f);
            livrariaClose = Mathf.Lerp(livrariaClose, sim.Rings.LivrariaOpen ? 0.02f : 1f, dt * 3f);
            SetShutter(merceariaShutter, merceariaClose);
            SetShutter(livrariaShutter, livrariaClose);

            if (police != null)
            {
                police.gameObject.SetActive(sim.Rings.PoliceOnStreet);
                if (sim.Rings.PoliceOnStreet)
                {
                    police.position = new Vector3(-sim.Rings.PoliceX, WorldMap.GroundY, WorldMap.Z(sim.Tuning.streetY) + 0.42f);
                    bool f = Mathf.FloorToInt(Time.time * 1000f / 180f) % 2 == 0;
                    if (policeBlue != null) policeBlue.SetActive(f);
                    if (policeRed != null) policeRed.SetActive(!f);
                }
            }

            if (squareLights != null)
                squareLights.SetActive(sim.Rings.FestaActive || sim.IsNightTurn);
        }

        private void SetShutter(Transform t, float k)
        {
            if (t == null) return;
            Vector3 s = t.localScale;
            s.y = shutterHeight * k;
            t.localScale = s;
            Vector3 p = t.localPosition;
            p.y = shutterTop - shutterHeight * k / 2f;
            t.localPosition = p;
        }
    }
}
