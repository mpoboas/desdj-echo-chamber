using UnityEngine;
using UnityEngine.UI;

namespace EchoChamber
{
    /// <summary>Top-left pills (turn, revenue, cohesion, tension, perceived neighbourhood), key legend and back button.</summary>
    public class HudView : MonoBehaviour
    {
        public static HudView Instance { get; private set; }

        private class Pill
        {
            public Text label;
            public Text value;
            public RectTransform bar;
        }

        private Pill day, rev, coe, ten, know;
        private Button back;
        private Text muteLabel;
        private GameObject root;
        private float timer;

        private Pill MakePill(Transform parent, string label, bool withBar, Color barColor)
        {
            var bg = UiFactory.Image(parent, UiPalette.Panel, "Pill");
            UiFactory.VLayout(bg.gameObject, 2f, new RectOffset(16, 16, 10, 12));
            var p = new Pill();
            p.label = UiFactory.Text(bg.transform, label.ToUpperInvariant(), 17, UiPalette.Muted, TextAnchor.UpperLeft, FontStyle.Bold, "L");
            p.value = UiFactory.Text(bg.transform, "-", 26, UiPalette.Paper, TextAnchor.UpperLeft, FontStyle.Bold, "V");
            if (withBar)
            {
                var track = UiFactory.Image(bg.transform, new Color(1f, 1f, 1f, 0.12f), "Bar");
                var le = track.gameObject.AddComponent<LayoutElement>();
                le.minHeight = 8f;
                le.preferredHeight = 8f;
                le.minWidth = 150f;
                var fill = UiFactory.Image(track.transform, barColor, "Fill");
                fill.rectTransform.anchorMin = Vector2.zero;
                fill.rectTransform.anchorMax = new Vector2(0f, 1f);
                fill.rectTransform.pivot = new Vector2(0f, 0.5f);
                fill.rectTransform.offsetMin = Vector2.zero;
                fill.rectTransform.offsetMax = Vector2.zero;
                p.bar = fill.rectTransform;
            }
            return p;
        }

        private static void SetBar(Pill p, float v)
        {
            if (p.bar == null) return;
            p.bar.anchorMax = new Vector2(Mathf.Clamp01(v), 1f);
        }

        private void Awake()
        {
            Instance = this;
            var canvas = UiFactory.CreateCanvas("HudCanvas", transform, 10, new Vector2(1920, 1080));
            root = canvas.gameObject;

            var row = UiFactory.Rect("Pills", canvas.transform);
            UiFactory.Anchor(row, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -16f), new Vector2(1200f, 100f));
            var h = UiFactory.HLayout(row.gameObject, 10f, new RectOffset(0, 0, 0, 0));
            h.childAlignment = TextAnchor.UpperLeft;
            h.childForceExpandHeight = false;
            day = MakePill(row, "Dia 1 · Manhã", false, Color.white);
            rev = MakePill(row, "Receita / meta do chefe", true, UiPalette.Amber);
            coe = MakePill(row, "Coesão", true, UiPalette.Blue);
            ten = MakePill(row, "Tensão", true, UiPalette.Pink);
            know = MakePill(row, "Bairro percebido", true, UiPalette.Green);

            var keys = UiFactory.Image(canvas.transform, UiPalette.Panel, "Keys");
            UiFactory.Anchor(keys.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 16f), new Vector2(430f, 74f));
            var kt = UiFactory.Text(keys.transform, "<b>A</b> amplificar · <b>O</b> ocultar · <b>N</b> normal\n<b>Espaço</b> alavanca · <b>Esc</b> voltar", 19, UiPalette.Muted, TextAnchor.MiddleLeft, FontStyle.Normal, "KeysText");
            UiFactory.Stretch(kt.rectTransform, 14, 4, 10, 4);

            var corner = UiFactory.Rect("Corner", canvas.transform);
            UiFactory.Anchor(corner, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -16f), new Vector2(520f, 64f));
            var ch = UiFactory.HLayout(corner.gameObject, 10f, new RectOffset(0, 0, 0, 0));
            ch.childAlignment = TextAnchor.UpperRight;
            ch.childForceExpandWidth = false;
            back = UiFactory.Button(corner, "Voltar à mesa", UiPalette.Panel, UiPalette.Paper, 24, () => RoomEvents.RaiseViewRequested(ViewId.Home), "Back");
            back.gameObject.AddComponent<LayoutElement>().minWidth = 220f;
            var mute = UiFactory.Button(corner, "Som: ligado", UiPalette.Panel, UiPalette.Paper, 24, ToggleMute, "Mute");
            mute.gameObject.AddComponent<LayoutElement>().minWidth = 200f;
            muteLabel = mute.GetComponentInChildren<Text>();
            back.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            RoomEvents.ViewChanged += OnView;
        }

        private void OnDisable()
        {
            RoomEvents.ViewChanged -= OnView;
        }

        private void OnView(ViewId v)
        {
            back.gameObject.SetActive(v != ViewId.Home);
        }

        private void ToggleMute()
        {
            GameAudio.Muted = !GameAudio.Muted;
            AudioListener.volume = GameAudio.Muted ? 0f : 1f;
            muteLabel.text = "Som: " + (GameAudio.Muted ? "desligado" : "ligado");
        }

        public void SetVisible(bool v)
        {
            if (root != null) root.SetActive(v);
        }

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = 0.05f;
            var gm = GameManager.Instance;
            if (gm == null || gm.Sim == null) return;
            var sim = gm.Sim;
            int di = Mathf.Clamp(sim.Day, 0, sim.Content.days.Count - 1);
            var d = sim.Content.days[di];
            day.label.text = (d.name + " · " + Texts.TurnNames[Mathf.Clamp(sim.Turn, 0, 2)]).ToUpperInvariant();
            day.value.text = TurnManager.Instance != null ? TurnManager.Instance.PhaseText : "-";
            rev.value.text = $"{sim.RevenueDay} / {d.target} €";
            SetBar(rev, Mathf.Clamp01((float)sim.RevenueDay / d.target));
            coe.value.text = Mathf.RoundToInt(sim.Metrics.Cohesion * 100f) + "%";
            SetBar(coe, sim.Metrics.Cohesion);
            ten.value.text = Mathf.RoundToInt(sim.Metrics.Tension * 100f) + "%";
            ten.value.color = sim.Metrics.Tension > 0.6f ? UiPalette.Pink : UiPalette.Paper;
            SetBar(ten, sim.Metrics.Tension);
            float u = sim.Understanding;
            know.value.text = Mathf.RoundToInt(u * 100f) + "%";
            SetBar(know, u);
        }
    }
}
