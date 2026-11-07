using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EchoChamber
{
    /// <summary>Main menu, how-to-play card, final impact report with chart, and restart.</summary>
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
            card.Para("• Durante a simulação, clica nos vizinhos que mostram um símbolo por cima para anotar a reação no caderno e no quadro de cortiça.", 26);
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

        // ---------------------------------------------------------------- ending

        public void ShowEnding()
        {
            var ov = OverlayView.Instance;
            var sim = GameManager.Instance.Sim;
            var content = sim.Content;
            var ending = content.FindEnding(sim.FinalEnding ?? EndingId.Gentry);
            var card = ov.Open(1240f);

            card.Eyebrow("Relatório de impacto social");
            card.Title(ending.headline, 50);
            card.Para(ending.text, 25);
            card.Para("<i>" + Texts.Epilogue(sim.Understanding, sim.Tuning, sim.BalanceMissedForUnderstanding) + "</i>", 24);
            BuildChart(card.Root, sim);
            for (int i = 0; i < sim.DayResults.Count; i++)
            {
                var goals = sim.DayResults[i];
                int ok = goals.Count(g => g.Achieved);
                card.Stat(content.days[i].name, $"{ok}/{goals.Count} objetivos · {sim.RevenueByDay[i]} €");
            }
            float apat = Metrics.AvgApat(sim.Neighbours);
            card.Stat("Coesão / Tensão / Apatia finais",
                $"{Mathf.RoundToInt(sim.Metrics.Cohesion * 100f)}% / {Mathf.RoundToInt(sim.Metrics.Tension * 100f)}% / {Mathf.RoundToInt(apat * 100f)}%");
            card.Stat("Quanto do bairro percebeste", Mathf.RoundToInt(sim.Understanding * 100f) + "%", UiPalette.Good);

            var known = sim.Neighbours.Where(n => n.TraitKnown).ToList();
            var unknown = sim.Neighbours.Where(n => !n.TraitKnown).ToList();
            string traits = known.Count > 0
                ? "Traços descobertos: " + string.Join(", ", known.Select(n => $"{n.Name} ({n.Data.traitLabel})")) + "."
                : "Não chegaste a perceber ninguém. Decidiste tudo às cegas.";
            if (unknown.Count > 0) traits += " Ficaram por perceber: " + string.Join(", ", unknown.Select(n => n.Name)) + ".";
            card.Para(traits, 22);

            card.Button("Jogar de novo", () =>
            {
                ov.Close();
                Restart();
            });
            card.Button("Voltar ao menu", () =>
            {
                ov.Close();
                Restart();
            }, false);
            card.Done();
        }

        private static void BuildChart(Transform parent, GameSimulation sim)
        {
            const float W = 1100f, H = 240f, padL = 70f, padR = 20f, padT = 30f, padB = 40f;
            var holder = UiFactory.Image(parent, Color.white, "Chart");
            var le = holder.gameObject.AddComponent<LayoutElement>();
            le.minHeight = H;
            le.preferredHeight = H;
            var rt = holder.rectTransform;

            foreach (float v in new[] { 0f, 0.5f, 1f })
            {
                float y = padT + (1f - v) * (H - padT - padB);
                var grid = UiFactory.Image(rt, UiPalette.Rule, "Grid");
                Anchor(grid.rectTransform, padL, y, W - padL - padR, 2f);
                var lab = UiFactory.Text(rt, Mathf.RoundToInt(v * 100f) + "%", 20, ReactionStyle.Hex("#6B6455"), TextAnchor.MiddleRight, FontStyle.Bold, "GridLabel");
                Anchor(lab.rectTransform, 4f, y - 12f, padL - 12f, 24f);
            }

            var hist = sim.History;
            if (hist.Count >= 2)
            {
                int step = Mathf.Max(1, hist.Count / 80);
                var pts = new List<KeyValuePair<float, float>>();
                for (int i = 0; i < hist.Count; i += step) pts.Add(hist[i]);
                pts.Add(hist[hist.Count - 1]);
                float chartW = W - padL - padR, chartH = H - padT - padB;
                for (int series = 0; series < 2; series++)
                {
                    Color c = series == 0 ? UiPalette.Blue : UiPalette.Pink;
                    for (int i = 1; i < pts.Count; i++)
                    {
                        float v0 = series == 0 ? pts[i - 1].Key : pts[i - 1].Value;
                        float v1 = series == 0 ? pts[i].Key : pts[i].Value;
                        Vector2 a = new Vector2(padL + (i - 1f) / (pts.Count - 1f) * chartW, padT + (1f - v0) * chartH);
                        Vector2 b = new Vector2(padL + i / (pts.Count - 1f) * chartW, padT + (1f - v1) * chartH);
                        var seg = UiFactory.Image(rt, c, "Seg");
                        var r = seg.rectTransform;
                        r.anchorMin = r.anchorMax = new Vector2(0f, 1f);
                        r.pivot = new Vector2(0.5f, 0.5f);
                        Vector2 d = b - a;
                        r.anchoredPosition = new Vector2((a.x + b.x) / 2f, -(a.y + b.y) / 2f);
                        r.sizeDelta = new Vector2(d.magnitude + 1.5f, 5f);
                        r.localRotation = Quaternion.Euler(0f, 0f, -Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
                    }
                }
            }

            var l1 = UiFactory.Text(rt, "coesão", 22, UiPalette.Blue, TextAnchor.MiddleLeft, FontStyle.Bold, "LegendBlue");
            Anchor(l1.rectTransform, W - 280f, 4f, 120f, 26f);
            var l2 = UiFactory.Text(rt, "tensão", 22, UiPalette.Pink, TextAnchor.MiddleLeft, FontStyle.Bold, "LegendPink");
            Anchor(l2.rectTransform, W - 150f, 4f, 120f, 26f);
            var foot = UiFactory.Text(rt, "turno a turno, 3 dias", 20, ReactionStyle.Hex("#6B6455"), TextAnchor.MiddleLeft, FontStyle.Bold, "Foot");
            Anchor(foot.rectTransform, padL, H - 34f, 400f, 26f);
        }

        private static void Anchor(RectTransform r, float x, float y, float w, float h)
        {
            r.anchorMin = r.anchorMax = new Vector2(0f, 1f);
            r.pivot = new Vector2(0f, 0f);
            r.anchoredPosition = new Vector2(x, -(y + h));
            r.sizeDelta = new Vector2(w, h);
        }
    }
}
