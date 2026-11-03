using UnityEngine;

namespace EchoChamber
{
    /// <summary>Digital clock on top of the CRT: decision countdown, "AO VIVO" during the simulation.</summary>
    public class ClockView : MonoBehaviour
    {
        public TextMesh main;
        public TextMesh sub;

        private static readonly string[] TurnNames = { "MANHÃ", "TARDE", "NOITE" };

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Sim == null || main == null) return;
            var sim = gm.Sim;
            string txt = "--:--";
            Color col = ReactionStyle.Hex("#FF6B4A");
            if (sim.Phase == GamePhase.Decide)
            {
                if (float.IsPositiveInfinity(sim.DecideMax)) txt = "LIVRE";
                else
                {
                    int s = Mathf.Max(0, Mathf.CeilToInt(sim.DecideMax - sim.DecideTime));
                    txt = "0:" + s.ToString("00");
                    if (s <= 8 && Mathf.FloorToInt(Time.time * 1000f / 300f) % 2 == 1) col = UiPalette.Amber;
                }
            }
            else if (sim.Phase == GamePhase.Simulate)
            {
                txt = "AO VIVO";
                col = ReactionStyle.Hex("#3DFF8A");
            }
            main.text = txt;
            main.color = col;
            if (sub != null) sub.text = sim.Phase == GamePhase.Decide ? "TEMPO PARA DECIDIR" : TurnNames[Mathf.Clamp(sim.Turn, 0, 2)];
        }
    }
}
