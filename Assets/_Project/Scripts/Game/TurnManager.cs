using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace EchoChamber
{
    /// <summary>
    /// Presentation side of the turn cycle: Decide (printer, slips, stamps) -> Publish (lever) -> Simulate -> Newspaper.
    /// The rules live in GameSimulation; this class turns them into physical objects and overlays.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }

        public Transform printerSlot;
        public Transform printerRoot;
        public Transform standPose;
        public Transform tubeMouth;
        public Transform trayPoint;
        public Transform slipParent;
        public PrinterAnimator printer;
        public StampView stamps;
        public LeverView lever;
        public TubeView tube;
        public Material paperEdge;

        public string PhaseText { get; private set; } = "-";

        private GameSimulation sim;
        private readonly List<SlipView> slips = new List<SlipView>();
        private readonly Queue<PostData> printQueue = new Queue<PostData>();
        private readonly List<KeyValuePair<PostData, DecisionMode>> decided = new List<KeyValuePair<PostData, DecisionMode>>();
        private SlipView printing;
        private SlipView onStand;
        private float printWait;
        private float printTime;
        private int printLine;
        private bool torn;
        private DecisionMode? pendingAct;
        private int turnKey = -1;
        private bool introShown, reportShown, dayEndShown, endShown, simStarted;
        private int tickPrev = 99;
        private bool firstDecisionHinted;

        private const float PrintDuration = 1.7f;
        private const float ExitTilt = 0.3f;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            RoomEvents.Clicked += OnClicked;
        }

        private void OnDisable()
        {
            RoomEvents.Clicked -= OnClicked;
        }

        private void Start()
        {
            sim = GameManager.Instance.Sim;
            sim.EventRaised += OnSimEvent;
        }

        private int Key => sim.Day * 10 + sim.Turn;

        // --------------------------------------------------------------- update

        private void Update()
        {
            if (sim == null) return;
            switch (sim.Phase)
            {
                case GamePhase.Intro:
                    PhaseText = "-";
                    if (!introShown)
                    {
                        introShown = true;
                        if (GameFlow.Instance != null) GameFlow.Instance.ShowStartScreen();
                        else ShowIntroCard();
                    }
                    break;
                case GamePhase.Decide:
                    if (turnKey != Key) BeginTurn();
                    UpdateDecide(Mathf.Min(Time.deltaTime, 0.05f) * GameManager.Instance.timeScale);
                    break;
                case GamePhase.Simulate:
                    PhaseText = "A observar";
                    if (!simStarted) OnSimulateStarted();
                    break;
                case GamePhase.Report:
                    PhaseText = "-";
                    if (!reportShown)
                    {
                        reportShown = true;
                        ShowReport();
                    }
                    break;
                case GamePhase.DayEnd:
                    PhaseText = "-";
                    if (!dayEndShown)
                    {
                        dayEndShown = true;
                        ShowDayEnd();
                    }
                    break;
                case GamePhase.End:
                    PhaseText = "-";
                    if (!endShown)
                    {
                        endShown = true;
                        if (GameFlow.Instance != null) GameFlow.Instance.ShowEnding();
                    }
                    break;
            }
        }

        public void ResetForNewGame()
        {
            if (sim != null) sim.EventRaised -= OnSimEvent;
            sim = GameManager.Instance.Sim;
            sim.EventRaised += OnSimEvent;
            ClearSlips();
            turnKey = -1;
            introShown = reportShown = dayEndShown = endShown = simStarted = false;
            firstDecisionHinted = false;
            pendingAct = null;
        }

        // --------------------------------------------------------------- decide phase

        private void ClearSlips()
        {
            foreach (var s in slips) if (s != null) Destroy(s.gameObject);
            slips.Clear();
            printQueue.Clear();
            decided.Clear();
            printing = null;
            onStand = null;
            torn = false;
            if (printer != null) printer.Printing = false;
        }

        private void BeginTurn()
        {
            turnKey = Key;
            reportShown = false;
            dayEndShown = false;
            simStarted = false;
            ClearSlips();
            foreach (var p in sim.TurnPosts) printQueue.Enqueue(p);
            printWait = 0.6f;
            sim.DecideClockRunning = false;
            pendingAct = null;
            tickPrev = 99;
            RoomEvents.RaiseViewRequested(ViewId.Home);

            if (sim.Turn == 0)
            {
                string boss = sim.CurrentDay.boss;
                StartCoroutine(After(0.6f, () =>
                {
                    GameAudio.Ring();
                    if (PhoneView.Instance != null) PhoneView.Instance.Ring(1.2f);
                    StartCoroutine(After(0.7f, () => SubtitleView.Say("Telefone · Dr. Valente", boss, SubtitleView.Style.Normal, 8.5f)));
                }));
            }
            if (sim.IsFirstTurnEver)
            {
                StartCoroutine(After(5.2f, () => SubtitleView.Say("Dica",
                    "Os posts saem da impressora para o suporte à tua frente. Lê e carimba: <b>AMPLIFICAR</b> (A), <b>OCULTAR</b> (O) ou atira para o cesto <b>NORMAL</b> (N).",
                    SubtitleView.Style.Hint, 9f)));
            }
        }

        private System.Collections.IEnumerator After(float seconds, System.Action a)
        {
            yield return new WaitForSeconds(seconds);
            a();
        }

        private void UpdateDecide(float dt)
        {
            bool undecided = slips.Exists(s => !s.Decided);
            PhaseText = (printQueue.Count > 0 || printing != null || undecided) ? "A decidir" : "Puxa a alavanca";

            if (printQueue.Count > 0 && printing == null && onStand == null)
            {
                printWait -= dt;
                if (printWait <= 0f)
                {
                    StartPrint();
                    printWait = 0.35f;
                }
            }
            StepPrinting(dt);

            if (!sim.DecideClockRunning && slips.Count > 0) sim.DecideClockRunning = true;
            float left = sim.DecideMax - sim.DecideTime;
            if (left < 8f && left > 0f && Mathf.CeilToInt(left) != tickPrev)
            {
                tickPrev = Mathf.CeilToInt(left);
                GameAudio.Tick();
            }
            if (left <= 0f) PullLever();
        }

        private void PrintGeometry(out Vector3 dir, out Vector3 slot, out Quaternion rot)
        {
            Quaternion pq = printerRoot.rotation;
            dir = (pq * new Vector3(0f, Mathf.Cos(ExitTilt), -Mathf.Sin(ExitTilt))).normalized;
            slot = printerSlot.position;
            rot = pq * Quaternion.Euler((Mathf.PI / 2f - ExitTilt) * Mathf.Rad2Deg, 0f, 0f);
        }

        private string PatternText(PostCategory cat)
        {
            var top = sim.Discovery.TopPattern(cat);
            if (top == null) return null;
            var sb = new StringBuilder();
            for (int i = 0; i < top.Count; i++)
            {
                if (i > 0) sb.Append(" · ");
                sb.Append(ReactionStyle.Short(top[i].Key)).Append(" ×").Append(top[i].Value);
            }
            return sb.ToString();
        }

        private void StartPrint()
        {
            var post = printQueue.Dequeue();
            string author = !string.IsNullOrEmpty(post.author) ? post.author : sim.Neighbours[Random.Range(0, sim.Neighbours.Count)].Name;
            var slip = SlipView.Create(post, author, paperEdge, slipParent);
            slips.Add(slip);
            printing = slip;
            slip.SetEstimates(sim.EstimateRevenue(post), PatternText(post.category));
            PrintGeometry(out var dir, out var slot, out var rot);
            slip.transform.rotation = rot;
            slip.transform.position = slot - dir * 0.18f;
            slip.SetReveal(0f);
            printTime = 0f;
            printLine = -1;
            torn = false;
            printer.Printing = true;
        }

        private static float Ease(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        private void StepPrinting(float dt)
        {
            var s = printing;
            if (s == null) return;
            printTime += dt;
            PrintGeometry(out var dir, out var slot, out var rot);
            float x = Mathf.Min(1f, printTime / PrintDuration);
            int line = Mathf.Min(SlipView.PrintLines, Mathf.FloorToInt(x * SlipView.PrintLines));
            float frac = x * SlipView.PrintLines - line;
            float prog = x >= 1f ? 1f : (line + Ease(Mathf.Clamp01(frac / 0.35f))) / SlipView.PrintLines;
            if (!torn) s.transform.position = slot + dir * (-0.18f + 0.36f * prog);
            if (line != printLine && x < 1f)
            {
                printLine = line;
                s.SetReveal((line + 1f) / SlipView.PrintLines);
                GameAudio.Dots();
            }
            if (printTime >= PrintDuration + 0.3f && !torn)
            {
                torn = true;
                GameAudio.Tear();
                s.SetReveal(1f);
                printer.Printing = false;
                printing = null;
                onStand = s;
                s.SetEstimates(sim.EstimateRevenue(s.Post), PatternText(s.Post.category));
                Vector3 up = s.transform.position + dir * 0.04f;
                Quaternion twist = s.transform.rotation * Quaternion.Euler(0f, -0.08f * Mathf.Rad2Deg, 0f);
                s.MoveTo(up, twist, 0.12f, 0f, -1f, () =>
                    s.MoveTo(standPose.position, standPose.rotation, 0.5f, 0.12f, -1f, () =>
                    {
                        onStand = s;
                        if (pendingAct.HasValue)
                        {
                            var m = pendingAct.Value;
                            pendingAct = null;
                            Act(m);
                        }
                    }));
            }
        }

        private bool SlipReady => onStand != null && !onStand.Moving && !onStand.Decided;

        // Debug / QA bot hooks
        public bool DebugReadyForAct => sim != null && sim.Phase == GamePhase.Decide && SlipReady && !stamps.Busy;
        public bool DebugReadyForLever => sim != null && sim.Phase == GamePhase.Decide && printQueue.Count == 0 && printing == null && onStand == null && !stamps.Busy && slips.Count > 0;
        public void DebugAct(DecisionMode m) => Act(m);
        public void DebugPull() => PullLever();

        // --------------------------------------------------------------- player actions

        private void OnClicked(ClickAction action, RoomClickable c)
        {
            if (sim == null) return;
            if (OverlayView.Instance != null && OverlayView.Instance.IsOpen) return;
            switch (action)
            {
                case ClickAction.Amplify: Act(DecisionMode.Amplify); break;
                case ClickAction.Hide: Act(DecisionMode.Hide); break;
                case ClickAction.Normal: Act(DecisionMode.Normal); break;
                case ClickAction.Lever: PullLever(); break;
                case ClickAction.Slip:
                    if (CameraRig.Instance != null && CameraRig.Instance.Current != ViewId.Stand) RoomEvents.RaiseViewRequested(ViewId.Stand);
                    else SubtitleView.Say("Talão", "Escolhe um carimbo: <b>AMPLIFICAR</b> (A), <b>OCULTAR</b> (O), ou o cesto <b>NORMAL</b> (N).", SubtitleView.Style.Hint, 3f);
                    break;
                case ClickAction.Phone:
                {
                    var d = sim.CurrentDay;
                    string msg = sim.RevenueDay >= d.target ? "Os números estão bons. Continua assim." : $"Faltam {d.target - sim.RevenueDay} euros para a meta de hoje.";
                    SubtitleView.Say("Telefone · Dr. Valente", msg, SubtitleView.Style.Normal, 4f);
                    break;
                }
                case ClickAction.Radio:
                {
                    float t = sim.Metrics.Tension, co = sim.Metrics.Cohesion;
                    string mood = t > 0.6f ? "O ambiente está pesado." : co > 0.55f ? "Os vizinhos andam bem-dispostos." : "Dia normal no bairro.";
                    SubtitleView.Say("Rádio Bairro FM", $"Coesão {Mathf.RoundToInt(co * 100f)}%, tensão {Mathf.RoundToInt(t * 100f)}%. {mood}", SubtitleView.Style.Radio, 4f);
                    break;
                }
                case ClickAction.Neighbour:
                    if (c != null && ObservationController.Instance != null) ObservationController.Instance.Observe(c.neighbourIndex);
                    break;
            }
        }

        private void Act(DecisionMode mode)
        {
            if (sim.Phase != GamePhase.Decide) return;
            var s = onStand;
            if (!stamps.Busy && ((s != null && s.Moving) || (s == null && printing != null)))
            {
                pendingAct = mode;
                return;
            }
            if (s == null || s.Moving || s.Decided || stamps.Busy) return;

            if (mode == DecisionMode.Normal)
            {
                s.SetMark(DecisionMode.Normal);
                FinishSlip(s, true);
                GameAudio.Click();
                return;
            }
            stamps.Play(mode, standPose.position, () =>
            {
                GameAudio.Stamp();
                s.SetMark(mode);
                if (CameraRig.Instance != null) CameraRig.Instance.Shake(0.12f);
            }, () => FinishSlip(s, false));
        }

        private void FinishSlip(SlipView s, bool toTray)
        {
            s.Decided = true;
            onStand = null;
            decided.Add(new KeyValuePair<PostData, DecisionMode>(s.Post, s.Mark ?? DecisionMode.Normal));
            if (toTray)
            {
                Quaternion q = Quaternion.Euler(0f, Random.Range(-30f, 10f), 0f);
                s.MoveTo(trayPoint.position + Vector3.up * (decided.Count * 0.005f), q, 0.45f, 0.15f, -1f, null);
                StartCoroutine(After(0.7f, () => SendToTube(s)));
            }
            else
            {
                SendToTube(s);
            }
            if (sim.IsFirstTurnEver && decided.Count == 1 && !firstDecisionHinted)
            {
                firstDecisionHinted = true;
                StartCoroutine(After(0.4f, () => SubtitleView.Say("Dica",
                    "Cada post decidido segue pelo tubo. Ainda não está publicado: só sai quando puxares a <b>alavanca</b>. Até lá podes mexer nos faders da mesa de mistura.",
                    SubtitleView.Style.Hint, 8.5f)));
            }
        }

        private void SendToTube(SlipView s)
        {
            if (s == null) return;
            Quaternion q = Quaternion.Euler(90f, 0f, -0.3f * Mathf.Rad2Deg);
            s.MoveTo(tubeMouth.position + Vector3.up * 0.04f, q, 0.55f, 0.25f, 0.3f, () =>
            {
                s.gameObject.SetActive(false);
                GameAudio.Tube();
                if (tube != null) tube.Ping();
            });
        }

        public void PullLever()
        {
            if (sim.Phase != GamePhase.Decide || stamps.Busy) return;
            if ((printQueue.Count > 0 || printing != null) && sim.DecideTime < sim.DecideMax)
            {
                SubtitleView.Say("Mesa", "A impressora ainda tem posts deste turno para imprimir.", SubtitleView.Style.Hint, 2.5f);
                return;
            }
            if (printing != null)
            {
                printing.SetReveal(1f);
                printer.Printing = false;
                var p = printing;
                printing = null;
                if (!torn) p.transform.position = standPose.position;
            }
            GameAudio.Lever();
            lever.Pull();

            foreach (var s in slips)
            {
                if (s.Decided) continue;
                s.SetMark(DecisionMode.Auto);
                s.Decided = true;
                decided.Add(new KeyValuePair<PostData, DecisionMode>(s.Post, DecisionMode.Auto));
                SendToTube(s);
            }
            while (printQueue.Count > 0) decided.Add(new KeyValuePair<PostData, DecisionMode>(printQueue.Dequeue(), DecisionMode.Auto));
            onStand = null;
            pendingAct = null;
            sim.Publish(decided);
        }

        // --------------------------------------------------------------- simulate phase

        private void OnSimulateStarted()
        {
            simStarted = true;
            if (sim.IsFirstTurnEver)
            {
                StartCoroutine(After(0.3f, () => SubtitleView.Say("Dica",
                    "Agora observas. O monitor mostra quem recebe cada post, mas a app não sabe o que as pessoas sentem. <b>Clica na janela</b> para ver a rua.",
                    SubtitleView.Style.Hint, 8f)));
                StartCoroutine(After(8.8f, () => SubtitleView.Say("Dica",
                    "Quando um vizinho reage a um post aparece-lhe um símbolo por cima. <b>Clica nele</b> para anotar no caderno. É assim que descobres como cada um funciona.",
                    SubtitleView.Style.Hint, 9f)));
            }
        }

        private void OnSimEvent(SimEvent e)
        {
            if (e.Kind == SimEventKind.AdsNervous)
            {
                SubtitleView.Say("Telefone · Dr. Valente", Texts.EventText(e.Kind), SubtitleView.Style.Alarm, 5f);
                GameAudio.Alarm();
                return;
            }
            SubtitleView.Say("Rádio Bairro FM", Texts.RadioText(e.Kind), e.Good ? SubtitleView.Style.Radio : SubtitleView.Style.Alarm, 6f);
            if (e.Good) GameAudio.Good(); else GameAudio.Alarm();
        }

        // --------------------------------------------------------------- overlays

        public void ShowIntroCard()
        {
            var ov = OverlayView.Instance;
            var card = ov.Open(1180f);
            card.Eyebrow("Invisible Systems · Echo Chamber");
            card.Title("Bem-vindo à cave da Vizinhança.");
            card.Para(sim.Content.days[0].brief);
            card.Para("• <b>Decidir:</b> os posts saem da impressora. Carimba <b>AMPLIFICAR</b> ou <b>OCULTAR</b>, ou atira para o cesto <b>NORMAL</b>. A mesa de mistura define o alcance de cada categoria.", 26);
            card.Para("• <b>Publicar:</b> puxa a alavanca. O tempo corre e observas no monitor e pela janela o que os vizinhos fazem com o que viram.", 26);
            card.Para("• <b>Perceber:</b> a app não sabe o que as pessoas sentem, só o que fazem. Pela janela, clica nos vizinhos quando reagem a um post e anota. O quadro de cortiça junta o que descobres.", 26);
            card.Para("• O jornal conta o que aconteceu, não porquê. As causas descobres tu.", 26);
            card.Para("• 3 dias, 3 turnos cada. Garante a receita do chefe sem partir o bairro. Os anéis no monitor enchem e, quando completam, há crise.", 26);
            card.Small("Clica no monitor, no quadro ou na janela para ver de perto.");
            card.Button("Entrar na sala", () =>
            {
                ov.Close();
                sim.StartGame();
            });
            card.Done();
        }

        private static string Pct(float v) => Mathf.RoundToInt(v * 100f) + "%";

        private void ShowReport()
        {
            var ov = OverlayView.Instance;
            var log = sim.Log;
            var day = sim.CurrentDay;
            var card = ov.Open(1240f);

            var mast = UiFactory.Rect("Mast", card.Root);
            var mh = UiFactory.HLayout(mast.gameObject, 10f, new RectOffset(0, 0, 0, 8));
            mh.childForceExpandWidth = true;
            mh.childAlignment = TextAnchor.LowerLeft;
            var mt = UiFactory.Text(mast, "O Jornal do Bairro", 50, UiPalette.Ink, TextAnchor.LowerLeft, FontStyle.Bold, "Mast");
            mt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 2f;
            var ms = UiFactory.Text(mast, (day.name + " · edição da " + Texts.TurnNames[sim.Turn].ToLowerInvariant()).ToUpperInvariant(), 19, UiPalette.Ink, TextAnchor.LowerRight, FontStyle.Bold, "Edition");
            ms.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1.4f;
            card.Rule();

            string headline;
            if (log.Events.Count > 0)
            {
                string t = Texts.EventText(log.Events[0].Kind);
                int dot = t.IndexOf('.');
                headline = dot > 0 ? t.Substring(0, dot) : t;
            }
            else headline = log.Posts.Exists(e => e.Mode == DecisionMode.Amplify) ? "O bairro passou o turno colado ao telemóvel" : "Turno calmo no bairro";
            card.Title(headline, 44);

            card.Columns(1.4f, 1f, out var left, out var right);
            card.Key("O que publicaste e o que aconteceu", left);
            foreach (var e in log.Posts)
            {
                var row = UiFactory.Rect("Chain", left);
                UiFactory.VLayout(row.gameObject, 2f, new RectOffset(0, 0, 4, 6));
                string who = e.ViewerNames.Count == 0 ? "ninguém"
                    : string.Join(", ", e.ViewerNames.GetRange(0, Mathf.Min(3, e.ViewerNames.Count))) + (e.ViewerNames.Count > 3 ? $" e mais {e.ViewerNames.Count - 3}" : "");
                string col = "#" + ColorUtility.ToHtmlStringRGB(ReactionStyle.CategoryColor(e.Post.category));
                string rev = e.Revenue > 0 ? $" · +{e.Revenue} €" : "";
                var line = UiFactory.Text(row, $"<b>[{Texts.ModeLabel(e.Mode)}]</b> <color={col}>●</color> <i>\"{e.Post.text}\"</i> → {e.Viewers} vizinhos{rev}", 24, UiPalette.Ink, TextAnchor.UpperLeft, FontStyle.Normal, "ChainText");
                var fx = UiFactory.Text(row, "viram: " + who, 20, ReactionStyle.Hex("#4A4536"), TextAnchor.UpperLeft, FontStyle.Normal, "Who");
            }

            card.Key("Acontecimentos", right);
            if (log.Events.Count == 0) card.Para("Nada de especial a relatar. Os anéis continuam a encher em silêncio.", 24, right);
            foreach (var ev in log.Events)
                card.Para("<b>" + Texts.EventText(ev.Kind) + "</b>", 24, right, ev.Good ? UiPalette.Good : UiPalette.Claret);
            card.Key("Balanço do turno", right);
            float dC = sim.Metrics.Cohesion - log.StartCohesion, dT = sim.Metrics.Tension - log.StartTension;
            int rev2 = sim.RevenueDay - log.StartRevenue;
            card.Stat("Receita", $"+{rev2} € ({sim.RevenueDay}/{day.target} €)", null, right);
            card.Stat("Coesão", $"{Pct(sim.Metrics.Cohesion)} ({(dC >= 0 ? "+" : "")}{Mathf.RoundToInt(dC * 100f)})", dC >= 0 ? UiPalette.Good : UiPalette.Claret, right);
            card.Stat("Tensão", $"{Pct(sim.Metrics.Tension)} ({(dT >= 0 ? "+" : "")}{Mathf.RoundToInt(dT * 100f)})", dT <= 0 ? UiPalette.Good : UiPalette.Claret, right);

            var disc = card.Panel(ReactionStyle.Hex("#E5DCC6"), new RectOffset(18, 18, 12, 14));
            var reveals = new List<string>();
            foreach (var r in log.Reveals)
                reveals.Add(r.Kind == RevealKind.Trait ? $"{r.A.Name}: {r.A.Data.traitLabel}" : $"{r.A.Name} e {r.B.Name} andam juntos");
            string notes = log.Notes > 0 ? $"anotaste {log.Notes} reaç{(log.Notes > 1 ? "ões" : "ão")} neste turno"
                : "não anotaste nada neste turno. Sem observar, o bairro continua uma caixa negra";
            string found = reveals.Count > 0 ? " Descobriste: " + string.Join("; ", reveals) + "." : "";
            card.Para($"<b>O teu caderno:</b> {notes}.{found} Bairro percebido: <b>{Pct(sim.Understanding)}</b>.", 23, disc);

            bool last = sim.IsLastTurnOfDay;
            card.Button(last ? "Fechar o dia" : "Próximo turno: " + Texts.TurnNames[sim.Turn + 1], () =>
            {
                ov.Close();
                reportShown = false;
                sim.ContinueAfterReport();
            });
            card.Done();
        }

        private void ShowDayEnd()
        {
            var ov = OverlayView.Instance;
            var day = sim.CurrentDay;
            var goals = sim.DayResults[sim.DayResults.Count - 1];
            var next = sim.Content.days[Mathf.Min(sim.Day + 1, sim.Content.days.Count - 1)];
            var card = ov.Open(1100f);
            string dayShort = day.name.Contains("·") ? day.name.Substring(0, day.name.IndexOf('·')).Trim().ToLowerInvariant() : day.name.ToLowerInvariant();
            card.Eyebrow("Fim do " + dayShort);
            card.Title(sim.RevenueDay >= day.target ? "O chefe está satisfeito." : "O chefe não atende o telefone.", 50);
            foreach (var g in goals)
            {
                string label = g.Kind == DayGoalKind.Revenue ? $"Meta do chefe: {day.target} €"
                    : g.Kind == DayGoalKind.LivrariaOpen ? "Letra Viva aberta" : "Tensão abaixo de 70%";
                card.Stat(label, g.Achieved ? "cumprido" : "falhado", g.Achieved ? UiPalette.Good : UiPalette.Claret);
            }
            card.Para("Os vizinhos dormem sobre o que viram hoje. Acordam um pouco mais calmos, mas a desconfiança e os hábitos ficam.", 26);
            card.Space(6f);
            card.Eyebrow("A seguir");
            card.Title(next.name, 42);
            card.Para(next.brief, 26);
            card.Button("Abrir a sala", () =>
            {
                ov.Close();
                dayEndShown = false;
                sim.BeginNextDay();
            });
            card.Done();
        }
    }
}
