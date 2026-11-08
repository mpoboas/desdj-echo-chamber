using System.Collections.Generic;
using UnityEngine;

namespace EchoChamber
{
    /// <summary>Development helper: plays turns automatically so the street runs without the control room.</summary>
    public class DebugAutoplay : MonoBehaviour
    {
        public DecisionMode mode = DecisionMode.Normal;

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Sim == null) return;
            var sim = gm.Sim;
            switch (sim.Phase)
            {
                case GamePhase.Intro:
                    sim.StartGame();
                    break;
                case GamePhase.Decide:
                    sim.DecideClockRunning = true;
                    if (sim.DecideTime > 2f)
                    {
                        var list = new List<KeyValuePair<PostData, DecisionMode>>();
                        foreach (var p in sim.TurnPosts) list.Add(new KeyValuePair<PostData, DecisionMode>(p, mode));
                        sim.Publish(list);
                    }
                    break;
                case GamePhase.Report:
                    sim.ContinueAfterReport();
                    break;
            }
        }
    }
}
