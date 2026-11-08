using UnityEngine;

namespace EchoChamber
{
    /// <summary>Main menu, how-to-play card, end-of-day summary and restart.</summary>
    public class GameFlow : MonoBehaviour
    {
        public static GameFlow Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void ShowStartScreen()
        {
            ShowMenu();
        }

        public void ShowMenu()
        {
            var ov = OverlayView.Instance;
            var card = ov.Open(980f);
            card.Eyebrow("Invisible Systems · DESDJ · ISEP");
            card.Title("Echo Chamber", 84);
            card.Para("És o algoritmo da <b>Vizinhança</b>, a rede social de um bairro do Porto. Numa cave, decides que posts são amplificados ou ocultados e observas o que isso faz a 14 vizinhos.", 28);
            card.Para("O sistema invisível é o próprio bairro. Só o descobres a observar o que as pessoas fazem.", 28);
            card.Space(10f);
            card.Button("Jogar", () =>
            {
                ov.Close();
                TurnManager.Instance.ShowIntroCard();
            });
            card.Button("Como jogar", () => ShowHowTo(), false);
            card.Button("Sair", () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }, false);
            card.Space(6f);
            card.Small("Projeto da cadeira DESDJ. Modelos: Kenney, KayKit e pmndrs (licenças livres).");
            card.Done();
        }

        private void ShowHowTo()
        {
            var ov = OverlayView.Instance;
            var card = ov.Open(1100f);
            card.Eyebrow("Como jogar");
            card.Title("Uma cave, uma impressora, um bairro.", 46);
            card.Para("• <b>A</b> amplifica o post, <b>O</b> oculta, <b>N</b> envia para o cesto sem mexer. Podes também clicar nos carimbos e no cesto.", 26);
            card.Para("• <b>Espaço</b> puxa a alavanca e publica o turno. <b>Esc</b> volta à vista da mesa.", 26);
            card.Para("• Arrasta os faders da mesa de mistura para dar mais ou menos alcance a cada categoria.", 26);
            card.Para("• Durante a simulação, observa os vizinhos pela janela e no monitor. Quando reagem a um post aparece-lhes um símbolo por cima.", 26);
            card.Para("• Receita e bairro andam em sentidos opostos: o que rende tende a fazer mal à coesão. Se a tensão passar de 70%, a receita cai para metade.", 26);
            card.Button("Voltar", ShowMenu);
            card.Done();
        }

        public void Restart()
        {
            var gm = GameManager.Instance;
            gm.CreateSimulation();
            TurnManager.Instance.ResetForNewGame();
            RoomEvents.RaiseViewRequested(ViewId.Home);
        }

        // ---------------------------------------------------------------- end of day

        public void ShowEnd()
        {
            var ov = OverlayView.Instance;
            var sim = GameManager.Instance.Sim;
            var card = ov.Open(1100f);
            card.Eyebrow("Fim do dia");
            card.Title("O bairro vai dormir.", 56);
            card.Para("Três edições do jornal depois, é isto que a tua mesa deixou na vizinhança.", 26);
            card.Stat("Receita do dia", sim.RevenueDay + " €");
            card.Stat("Coesão", Mathf.RoundToInt(sim.Metrics.Cohesion * 100f) + "%");
            card.Stat("Tensão", Mathf.RoundToInt(sim.Metrics.Tension * 100f) + "%");
            card.Space(6f);
            card.Button("Jogar de novo", () =>
            {
                ov.Close();
                Restart();
            });
            card.Done();
        }
    }
}
