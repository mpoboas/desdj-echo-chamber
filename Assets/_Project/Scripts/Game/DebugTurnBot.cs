using UnityEngine;

namespace EchoChamber
{
    /// <summary>QA helper: plays the game by itself (stamps, lever, closes cards, notes reactions). Disabled by default.</summary>
    public class DebugTurnBot : MonoBehaviour
    {
        public DecisionMode[] cycle = { DecisionMode.Amplify, DecisionMode.Hide, DecisionMode.Normal };
        public bool observe = true;
        public bool closeOverlays = true;
        public bool stopAtEnd = true;
        public float actionDelay = 0.4f;

        private int next;
        private float wait;
        private float observeWait;

        private void Update()
        {
            var gm = GameManager.Instance;
            var tm = TurnManager.Instance;
            if (gm == null || gm.Sim == null || tm == null) return;
            var sim = gm.Sim;
            wait -= Time.deltaTime;
            if (wait > 0f) return;

            if (OverlayView.Instance != null && OverlayView.Instance.IsOpen)
            {
                if (closeOverlays && !(stopAtEnd && sim.Phase == GamePhase.End))
                {
                    OverlayView.Instance.InvokePrimary();
                    wait = 0.5f;
                }
                return;
            }

            if (sim.Phase == GamePhase.Decide)
            {
                if (tm.DebugReadyForAct)
                {
                    tm.DebugAct(cycle[next++ % cycle.Length]);
                    wait = actionDelay;
                }
                else if (tm.DebugReadyForLever)
                {
                    tm.DebugPull();
                    wait = 0.5f;
                }
            }
            else if (sim.Phase == GamePhase.Simulate && observe && ObservationController.Instance != null)
            {
                observeWait -= Time.deltaTime;
                if (observeWait <= 0f)
                {
                    observeWait = 0.7f;
                    for (int i = 0; i < sim.Neighbours.Count; i++)
                    {
                        var r = sim.Neighbours[i].Reaction;
                        if (r != null && r.TimeLeft > 0f)
                        {
                            ObservationController.Instance.Observe(i);
                            break;
                        }
                    }
                }
            }
        }
    }
}
