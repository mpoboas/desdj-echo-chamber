using UnityEngine;

namespace EchoChamber
{
    /// <summary>Traffic light head or pedestrian signal. Colour is driven by the simulation traffic cycle.</summary>
    public class SignalLamp : MonoBehaviour
    {
        public enum Kind
        {
            Car,
            Pedestrian
        }

        public Kind kind;
        public Renderer red;
        public Renderer yellow;
        public Renderer green;
        public Renderer pedWalk;
        public Renderer pedStop;

        private static readonly Color Red = new Color(1f, 0.23f, 0.23f);
        private static readonly Color Yellow = new Color(1f, 0.71f, 0.28f);
        private static readonly Color Green = new Color(0.24f, 1f, 0.54f);
        private MaterialPropertyBlock block;

        private void Set(Renderer r, Color c, bool on)
        {
            if (r == null) return;
            if (block == null) block = new MaterialPropertyBlock();
            Color col = c * (on ? 1f : 0.16f);
            col.a = 1f;
            r.GetPropertyBlock(block);
            block.SetColor("_BaseColor", col);
            block.SetColor("_Color", col);
            r.SetPropertyBlock(block);
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Sim == null) return;
            var traffic = gm.Sim.Traffic;
            if (kind == Kind.Car)
            {
                var c = traffic.Car;
                Set(red, Red, c == CarLight.Red);
                Set(yellow, Yellow, c == CarLight.Yellow);
                Set(green, Green, c == CarLight.Green);
            }
            else
            {
                var p = traffic.Ped;
                bool blinkOn = Mathf.FloorToInt(Time.time * 1000f / 300f) % 2 == 0;
                bool walkOn = p == PedLight.Walk || (p == PedLight.Blink && blinkOn);
                Set(pedWalk, Green, walkOn);
                Set(pedStop, Red, p == PedLight.Stop);
            }
        }
    }
}
