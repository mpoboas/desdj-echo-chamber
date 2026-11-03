using UnityEngine;

namespace EchoChamber
{
    /// <summary>Menu, start screen, ending and restart. Filled out in M6; minimal start flow for M4.</summary>
    public class GameFlow : MonoBehaviour
    {
        public static GameFlow Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void ShowStartScreen()
        {
            TurnManager.Instance.ShowIntroCard();
        }

        public void ShowEnding()
        {
            var ov = OverlayView.Instance;
            var card = ov.Open(1100f);
            var sim = GameManager.Instance.Sim;
            card.Title(sim.FinalEnding.ToString());
            card.Button("Fechar", () => ov.Close());
            card.Done();
        }
    }
}
