using UnityEngine;

namespace EchoChamber
{
    /// <summary>Sticky note on the CRT with the boss target for today.</summary>
    public class StickyNoteView : MonoBehaviour
    {
        public TextMesh text;

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Sim == null || text == null) return;
            var sim = gm.Sim;
            int day = Mathf.Clamp(sim.Day, 0, sim.Content.days.Count - 1);
            text.text = "meta:\n" + sim.Content.days[day].target + " €";
        }
    }
}
