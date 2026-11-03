using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EchoChamber
{
    /// <summary>
    /// The CRT monitor: an orthographic camera renders a uGUI map of the neighbourhood into a RenderTexture.
    /// Shows places, crisis rings, neighbours as neutral dots, who received which post, cohesion and tension.
    /// Never shows hidden neighbour states (pillar: see only what people do).
    /// </summary>
    public class CRTMonitor : MonoBehaviour
    {
        public Renderer screen;
        public Sprite circle;
        public Sprite ring;
        public Sprite diamond;
        public Sprite triangle;
        public Sprite star;
        public Sprite house;
        public Sprite scanlines;
        public Sprite vignette;
        public Vector3 offscreenOrigin = new Vector3(2000f, 2000f, 2000f);

        private const int W = 640, H = 480;
        private static readonly Color Bg = ReactionStyle.Hex("#0F1A2E");

        private RenderTexture rt;
        private Camera crtCam;
        private RectTransform root;
        private Text topLeft, topLeft2, topRight;
        private readonly Dictionary<PlaceId, Image> placeIcon = new Dictionary<PlaceId, Image>();
        private readonly Dictionary<PlaceId, Text> placeLabel = new Dictionary<PlaceId, Text>();
        private readonly Dictionary<PlaceId, Image> ringFill = new Dictionary<PlaceId, Image>();
        private readonly Dictionary<PlaceId, Text> ringLabel = new Dictionary<PlaceId, Text>();
        private readonly List<Image> dots = new List<Image>();
        private readonly List<Image> linkPool = new List<Image>();
        private readonly List<Image> linePool = new List<Image>();
        private Image antenna, antennaPulse;
        private float timer;

        private static float Mx(float lx) => 30f + lx / 1000f * 580f;
        private static float My(float ly) => 70f + ly / 640f * 380f;

        private struct RingDef
        {
            public string label;
            public Color col;
            public bool inverse;
            public bool good;
        }

        private static readonly Dictionary<PlaceId, RingDef> Rings = new Dictionary<PlaceId, RingDef>
        {
            { PlaceId.Mercearia, new RingDef { label = "MEDO", col = ReactionStyle.Hex("#FF4F8B") } },
            { PlaceId.Junta, new RingDef { label = "REVOLTA", col = ReactionStyle.Hex("#FF4F8B") } },
            { PlaceId.Livraria, new RingDef { label = "VIDA", col = ReactionStyle.Hex("#2FB37E"), inverse = true } },
            { PlaceId.Praca, new RingDef { label = "COMUNIDADE", col = ReactionStyle.Hex("#4C7DFF"), good = true } },
        };

        private Sprite ShapeFor(PlaceId id)
        {
            switch (id)
            {
                case PlaceId.Cafe: return circle;
                case PlaceId.Mercearia: return UiFactory.White();
                case PlaceId.CasasN:
                case PlaceId.CasasS: return house;
                case PlaceId.Junta: return diamond;
                case PlaceId.Livraria: return triangle;
                default: return star;
            }
        }

        private static string ShortName(PlaceId id)
        {
            switch (id)
            {
                case PlaceId.Cafe: return "CAFÉ";
                case PlaceId.Mercearia: return "MERCEARIA";
                case PlaceId.CasasN: return "CASAS";
                case PlaceId.Junta: return "JUNTA";
                case PlaceId.Livraria: return "LIVRARIA";
                case PlaceId.CasasS: return "PRÉDIO";
                default: return "PRAÇA";
            }
        }

        private void Start()
        {
            Build();
        }

        private void OnDestroy()
        {
            if (rt != null) rt.Release();
        }

        private RectTransform Place(Transform parent, string name, float x, float y, float w, float h)
        {
            var r = UiFactory.Rect(name, parent);
            r.anchorMin = r.anchorMax = new Vector2(0f, 1f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(x, -y);
            r.sizeDelta = new Vector2(w, h);
            return r;
        }

        private Image Img(Transform parent, string name, float x, float y, float w, float h, Color c, Sprite s = null)
        {
            var im = UiFactory.Image(parent, c, name, s);
            var r = im.rectTransform;
            r.anchorMin = r.anchorMax = new Vector2(0f, 1f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(x, -y);
            r.sizeDelta = new Vector2(w, h);
            return im;
        }

        private Text Txt(Transform parent, string name, string text, int size, Color c, TextAnchor a, float x, float y, float w = 300f)
        {
            var t = UiFactory.Text(parent, text, size, c, a, FontStyle.Bold, name);
            var r = t.rectTransform;
            r.anchorMin = r.anchorMax = new Vector2(0f, 1f);
            r.pivot = a == TextAnchor.MiddleLeft || a == TextAnchor.UpperLeft ? new Vector2(0f, 0.5f)
                : a == TextAnchor.MiddleRight || a == TextAnchor.UpperRight ? new Vector2(1f, 0.5f) : new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(x, -y);
            r.sizeDelta = new Vector2(w, size + 6);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private void Build()
        {
            var gm = GameManager.Instance;
            var content = gm.Content;

            rt = new RenderTexture(W, H, 16, RenderTextureFormat.ARGB32) { name = "CRT" };
            rt.Create();

            var camGo = new GameObject("CRTCamera");
            camGo.transform.SetParent(transform, false);
            camGo.transform.position = offscreenOrigin;
            crtCam = camGo.AddComponent<Camera>();
            crtCam.orthographic = true;
            crtCam.orthographicSize = H / 2f;
            crtCam.clearFlags = CameraClearFlags.SolidColor;
            crtCam.backgroundColor = Bg;
            crtCam.cullingMask = 1 << 5;
            crtCam.targetTexture = rt;
            crtCam.nearClipPlane = 0.1f;
            crtCam.farClipPlane = 10f;
            crtCam.depth = -50;
            crtCam.allowHDR = false;
            crtCam.allowMSAA = false;

            var cgo = new GameObject("CRTCanvas");
            cgo.transform.SetParent(transform, false);
            cgo.layer = 5;
            var canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = crtCam;
            canvas.planeDistance = 5f;
            var scaler = cgo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;
            var rootRect = cgo.GetComponent<RectTransform>();
            root = UiFactory.Rect("Root", cgo.transform);
            UiFactory.Stretch(root);
            SetLayerRecursive(cgo, 5);

            Img(root, "Bg", W / 2f, H / 2f, W, H, Bg);

            // streets
            Color street = ReactionStyle.Hex("#22355A");
            Img(root, "StreetH", (Mx(0) + Mx(1000)) / 2f, My(318f), Mx(1000) - Mx(0), 22f, street);
            Img(root, "StreetV", Mx(505f), (My(20f) + My(318f)) / 2f, 22f, My(318f) - My(20f), street);

            // contagion link pool
            for (int i = 0; i < 40; i++)
            {
                var l = Img(root, "Link", 0, 0, 1, 1.5f, new Color(0.945f, 0.925f, 0.878f, 0.25f));
                l.gameObject.SetActive(false);
                linkPool.Add(l);
            }

            // places and rings
            foreach (var p in content.places)
            {
                float cx = Mx(p.rect[0] + p.rect[2] / 2f), cy = My(p.rect[1] + p.rect[3] / 2f);
                bool isHome = p.id == PlaceId.CasasN || p.id == PlaceId.CasasS;
                float r = isHome ? 11f : 15f;
                if (Rings.TryGetValue(p.id, out var rd))
                {
                    Img(root, "RingTrack", cx, cy, 64f, 64f, new Color(1f, 1f, 1f, 0.08f), ring);
                    var fill = Img(root, "RingFill_" + p.id, cx, cy, 64f, 64f, rd.col, ring);
                    fill.type = Image.Type.Filled;
                    fill.fillMethod = Image.FillMethod.Radial360;
                    fill.fillOrigin = 2;
                    fill.fillClockwise = true;
                    ringFill[p.id] = fill;
                    ringLabel[p.id] = Txt(root, "RingLabel", rd.label, 12, rd.col, TextAnchor.MiddleCenter, cx, cy - 40f);
                }
                var icon = Img(root, "Place_" + p.id, cx, cy, r * 2.4f, r * 2.4f, ReactionStyle.Hex("#F1ECE0"), ShapeFor(p.id));
                placeIcon[p.id] = icon;
                placeLabel[p.id] = Txt(root, "PlaceLabel", ShortName(p.id), 14, ReactionStyle.Hex("#C9CEDD"), TextAnchor.MiddleCenter, cx, cy + (rd.label != null ? 50f : 30f));
            }

            // antenna
            float ax = Mx(505f), ay = My(318f);
            antennaPulse = Img(root, "AntennaPulse", ax, ay, 28f, 28f, new Color(1f, 0.71f, 0.28f, 0.5f), ring);
            antenna = Img(root, "Antenna", ax, ay, 18f, 18f, UiPalette.Amber, circle);

            // delivery line pool
            for (int i = 0; i < 160; i++)
            {
                var l = Img(root, "Line", 0, 0, 1, 3f, Color.white);
                l.gameObject.SetActive(false);
                linePool.Add(l);
            }

            // neighbour dots
            for (int i = 0; i < content.neighbours.Count; i++)
                dots.Add(Img(root, "Dot_" + i, 0, 0, 14f, 14f, ReactionStyle.Hex("#D9DEEA"), circle));

            // header
            Img(root, "Header", W / 2f, 26f, W, 52f, ReactionStyle.Hex("#152440"));
            topLeft = Txt(root, "Title", "", 18, UiPalette.Amber, TextAnchor.MiddleLeft, 16f, 22f);
            topLeft2 = Txt(root, "Phase", "", 18, UiPalette.Muted, TextAnchor.MiddleLeft, 16f, 42f);
            topRight = Txt(root, "Metrics", "", 18, UiPalette.Paper, TextAnchor.MiddleRight, W - 16f, 32f, 400f);

            // legend
            Txt(root, "Legend1", "o vizinho (a app não sabe o que sentem)", 12, ReactionStyle.Hex("#D9DEEA"), TextAnchor.MiddleLeft, 16f, H - 12f, 340f);
            Txt(root, "Legend2", "- juntos   anel cheio = crise", 12, UiPalette.Muted, TextAnchor.MiddleLeft, 380f, H - 12f, 250f);

            // CRT finish
            var scan = Img(root, "Scanlines", W / 2f, H / 2f, W, H, new Color(1f, 1f, 1f, 1f), scanlines);
            scan.type = Image.Type.Tiled;
            var vig = Img(root, "Vignette", W / 2f, H / 2f, W, H, Color.white, vignette);

            if (screen != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                mat.SetTexture("_BaseMap", rt);
                mat.mainTexture = rt;
                screen.sharedMaterial = mat;
            }
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform t in go.transform) SetLayerRecursive(t.gameObject, layer);
        }

        private static void SetLine(Image img, Vector2 a, Vector2 b, float width, Color c)
        {
            var r = img.rectTransform;
            Vector2 d = b - a;
            float len = d.magnitude;
            img.gameObject.SetActive(len > 0.5f);
            r.anchoredPosition = new Vector2((a.x + b.x) / 2f, -(a.y + b.y) / 2f);
            r.sizeDelta = new Vector2(len, width);
            r.localRotation = Quaternion.Euler(0f, 0f, -Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            img.color = c;
        }

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer > 0f || root == null) return;
            timer = 1f / 20f;
            Redraw();
        }

        private static readonly string[] TurnNames = { "MANHÃ", "TARDE", "NOITE" };

        private void Redraw()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Sim == null) return;
            var sim = gm.Sim;
            var content = gm.Content;

            string phaseTxt = sim.Phase == GamePhase.Simulate ? $"SIMULAÇÃO · {Mathf.Max(0, Mathf.CeilToInt(sim.Tuning.simLength - sim.SimTime))}s"
                : sim.Phase == GamePhase.Decide ? "A AGUARDAR DECISÕES" : "EM PAUSA";
            string dayName = content.days[Mathf.Clamp(sim.Day, 0, content.days.Count - 1)].name;
            int dot = dayName.IndexOf('·');
            string dayShort = (dot > 0 ? dayName.Substring(0, dot).Trim() : dayName).ToUpperInvariant();
            topLeft.text = $"{dayShort} · {TurnNames[Mathf.Clamp(sim.Turn, 0, 2)]}";
            topLeft2.text = phaseTxt;
            topLeft2.color = sim.Phase == GamePhase.Simulate ? ReactionStyle.Hex("#3DFF8A") : UiPalette.Muted;
            topRight.text = $"COESÃO {Mathf.RoundToInt(sim.Metrics.Cohesion * 100f)}%  TENSÃO {Mathf.RoundToInt(sim.Metrics.Tension * 100f)}%";

            // places and rings
            foreach (var p in content.places)
            {
                bool closed = (p.id == PlaceId.Mercearia && !sim.Rings.MerceariaOpen) || (p.id == PlaceId.Livraria && !sim.Rings.LivrariaOpen);
                placeIcon[p.id].color = closed ? ReactionStyle.Hex("#3A4560") : ReactionStyle.Hex("#F1ECE0");
                var lab = placeLabel[p.id];
                lab.text = closed ? "FECHADO" : ShortName(p.id);
                lab.color = closed ? ReactionStyle.Hex("#FF4F8B") : ReactionStyle.Hex("#C9CEDD");

                if (Rings.TryGetValue(p.id, out var rd))
                {
                    bool visible = !(p.id == PlaceId.Livraria && sim.Day < 1);
                    ringFill[p.id].gameObject.SetActive(visible);
                    ringLabel[p.id].gameObject.SetActive(visible);
                    if (!visible) continue;
                    float v = p.id == PlaceId.Mercearia ? sim.Rings.Mercearia : p.id == PlaceId.Junta ? sim.Rings.Junta
                        : p.id == PlaceId.Livraria ? sim.Rings.Livraria : sim.Rings.Praca;
                    bool warn = rd.inverse ? v < 0.25f : (!rd.good && v > 0.75f);
                    bool blink = Mathf.FloorToInt(Time.time * 1000f / 220f) % 2 == 1;
                    ringFill[p.id].fillAmount = v;
                    ringFill[p.id].color = warn && blink ? Color.white : rd.col;
                }
            }

            // contagion links between neighbours that are close at the same place
            int li = 0;
            var ns = sim.Neighbours;
            for (int i = 0; i < ns.Count; i++)
                for (int j = i + 1; j < ns.Count; j++)
                {
                    var a = ns[i];
                    var b = ns[j];
                    if (!sim.Contagion.CanInteract(a, b)) continue;
                    if (li >= linkPool.Count) break;
                    SetLine(linkPool[li++], new Vector2(Mx(a.X), My(a.Y)), new Vector2(Mx(b.X), My(b.Y)), 1.5f, new Color(0.945f, 0.925f, 0.878f, 0.25f));
                }
            for (; li < linkPool.Count; li++) linkPool[li].gameObject.SetActive(false);

            // antenna pulse
            float pulse = 14f + (Time.time * 1000f / 40f) % 16f;
            antennaPulse.rectTransform.sizeDelta = new Vector2(pulse * 2f, pulse * 2f);

            // deliveries (Mini Metro style lines)
            int ci = 0;
            float ax = Mx(505f), ay = My(318f);
            foreach (var d in sim.Deliveries)
            {
                if (!d.Started) continue;
                float k = Mathf.Clamp01(d.Time / 0.8f);
                float alpha = Mathf.Clamp01(2.4f - d.Time);
                if (alpha <= 0f) continue;
                Color col = ReactionStyle.CategoryColor(d.Entry.Post.category);
                col.a = alpha;
                float w = d.Entry.Mode == DecisionMode.Amplify ? 6f : 3.5f;
                foreach (var n in d.Viewers)
                {
                    if (ci + 1 >= linePool.Count) break;
                    float tx = Mx(n.X), ty = My(n.Y);
                    float ex = ax + (tx - ax) * k, ey = ay + (ty - ay) * k;
                    SetLine(linePool[ci++], new Vector2(ax, ay), new Vector2(ex, ay), w, col);
                    SetLine(linePool[ci++], new Vector2(ex, ay), new Vector2(ex, ey), w, col);
                }
            }
            for (; ci < linePool.Count; ci++) linePool[ci].gameObject.SetActive(false);

            // neighbours as neutral dots
            for (int i = 0; i < ns.Count; i++)
            {
                var n = ns[i];
                bool inside = !n.IsMoving && n.Place.HasValue && content.FindPlace(n.Place.Value).indoor;
                float r = inside ? 10f : 14f;
                dots[i].rectTransform.anchoredPosition = new Vector2(Mx(n.X), -My(n.Y));
                dots[i].rectTransform.sizeDelta = new Vector2(r, r);
                var c = ReactionStyle.Hex("#D9DEEA");
                c.a = inside ? 0.45f : 1f;
                dots[i].color = c;
            }
        }
    }
}
