using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace EchoChamber
{
    /// <summary>
    /// Runs the v2 game: owns the FeedCareer, feeds the Central de Feeds panel (shown on the room LCD),
    /// plays each viewer's 2D clip and turns viewer events into what the player sees.
    /// </summary>
    public class FeedGameController : MonoBehaviour
    {
        public FeedContentAsset content;
        public FeedClipCatalog clips;
        public UIDocument document;
        public StyleSheet sheet;
        public PanelSettings panelSettings;
        public RenderTexture screenTexture;
        public Camera cam;
        public Collider screenCollider;
        [Tooltip("0 = random seed")] public int seed;
        [Range(0.5f, 8f)] public float timeScale = 1f;
        public bool autoStartDay = false;
        public UIDocument hudDocument;
        public StyleSheet hudSheet;
        public FeedTuning tuning = new FeedTuning();

        public FeedCareer Career { get; private set; }
        public CentralFeedsView View { get; private set; }
        public FeedHud Hud { get; private set; }

        private class Pres
        {
            public FeedViewer viewer;
            public TileView tile;
            public VideoPlayer player;
            public RenderTexture rt;
            public int likes, comments;
            public readonly List<ViewerComment> list = new List<ViewerComment>();
            public Color color;
            public float pendingTime;
        }

        private readonly List<Pres> order = new List<Pres>();
        private readonly Dictionary<FeedViewer, Pres> map = new Dictionary<FeedViewer, Pres>();
        private Pres focus;
        private System.Random vis;
        private Font osFont;
        private Vector2 lastDown = new Vector2(-1, -1), lastUp = new Vector2(-1, -1);

        public Vector2 DebugMap(Vector2 screen) { return ScreenToPanel(screen); }

        private void Start()
        {
            if (cam == null) cam = Camera.main;
            vis = new System.Random(seed != 0 ? seed + 1 : System.Environment.TickCount);
            var root = document.rootVisualElement;
            root.Clear();
            if (sheet != null) root.styleSheets.Add(sheet);
            View = new CentralFeedsView();
            View.Build(root);
            View.OnFocus = i => Focus(i);
            osFont = Font.CreateDynamicFontFromOSFont(new[] { "SF Pro Text", "Helvetica Neue", "Arial" }, 16);
            if (osFont != null) View.Root.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(osFont));
            View.OnSpeed = s => timeScale = s;
            View.OnDropOnViewer = (post, i) => Give(post, i);
            View.OnDropTrash = Discard;
            if (panelSettings != null) panelSettings.SetScreenToPanelSpaceFunction(ScreenToPanel);
            if (hudDocument != null)
            {
                var hr = hudDocument.rootVisualElement;
                hr.Clear();
                if (hudSheet != null) hr.styleSheets.Add(hudSheet);
                if (osFont != null) hr.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(osFont));
                Hud = new FeedHud();
                Hud.Build(hr);
            }
            RoomEvents.Clicked += OnRoomClicked;
            StartCareer();
        }

        private void OnRoomClicked(ClickAction a, RoomClickable c)
        {
            if (a == ClickAction.Phone && Hud != null) Hud.AnswerRing();
        }

        private void OnDestroy()
        {
            RoomEvents.Clicked -= OnRoomClicked;
            if (panelSettings != null) panelSettings.SetScreenToPanelSpaceFunction(null);
            foreach (var p in order) if (p.rt != null) p.rt.Release();
        }

        private Vector2 ScreenToPanel(Vector2 sp)
        {
            var invalid = new Vector2(float.NaN, float.NaN);
            if (cam == null || screenCollider == null || screenTexture == null) return invalid;
            var ray = cam.ScreenPointToRay(sp);
            if (!screenCollider.Raycast(ray, out var hit, 20f)) return invalid;
            // the screen is a unit quad scaled to the LCD size: local x,y in [-0.5, 0.5] map to the texture
            var local = screenCollider.transform.InverseTransformPoint(hit.point);
            float u = Mathf.Clamp01(local.x + 0.5f), v = Mathf.Clamp01(local.y + 0.5f);
            float k = panelSettings != null && panelSettings.scale > 0f ? panelSettings.scale : 1f;
            return new Vector2(u * screenTexture.width / k, (1f - v) * screenTexture.height / k);
        }

        public void StartCareer()
        {
            int s = seed != 0 ? seed : System.Environment.TickCount;
            Career = new FeedCareer(content.content, tuning, new SimRandom(s));
            SyncViewers();
            View.SetDay(Career.Day);
            if (focus == null && order.Count > 0) Focus(0);
            if (autoStartDay) Career.Current.Running = true;
            View.RenderDock(Career.Current.Dock);
            if (!autoStartDay && Hud != null) StartCoroutine(DayFlow());
        }

        private void SyncViewers()
        {
            foreach (var v in Career.Viewers)
            {
                if (map.ContainsKey(v)) continue;
                var prof = v.Person.Profile;
                ColorUtility.TryParseHtmlString(prof.color, out var col);
                var p = new Pres { viewer = v, color = col };
                p.tile = View.AddTile(prof.name, col);
                p.tile.Show();
                p.rt = new RenderTexture(540, 1140, 0, RenderTextureFormat.ARGB32) { name = "Feed_" + prof.key };
                p.rt.Create();
                var go = new GameObject("Video_" + prof.key);
                go.transform.SetParent(transform, false);
                p.player = go.AddComponent<VideoPlayer>();
                p.player.playOnAwake = false;
                p.player.isLooping = true;
                p.player.renderMode = VideoRenderMode.RenderTexture;
                p.player.targetTexture = p.rt;
                p.player.audioOutputMode = VideoAudioOutputMode.None;
                p.player.skipOnDrop = true;
                var captured = p;
                p.player.prepareCompleted += pl =>
                {
                    if (captured.pendingTime > 0f && captured.pendingTime < (float)pl.length) pl.time = captured.pendingTime;
                    pl.Play();
                };
                p.tile.SetVideo(p.rt);
                map[v] = p;
                order.Add(p);
            }
        }

        public void Focus(int index)
        {
            if (index < 0 || index >= order.Count) return;
            var p = order[index];
            if (focus != null) focus.tile.SetSelected(false);
            focus = p;
            View.FocusedIndex = index;
            p.tile.SetSelected(true);
            View.SetProfile(p.viewer.Person.Profile, p.color);
            View.Phone.SetVideo(p.rt);
            BindPhone(p);
        }

        private void BindPhone(Pres p)
        {
            var v = p.viewer;
            if (v.Current != null) View.Phone.SetPost(v.Current, v.Current.inflamed, p.likes, p.comments, clips != null ? clips.Overlay(v.Current.id) : null);
            View.Phone.SetLiked(v.Liked);
            View.Phone.SetComments(p.list);
            View.Phone.SetSheet(v.CommentsOpen);
            View.Phone.SetAsleep(v.Resting);
            View.Phone.SetTyping(v.TypedText);
        }

        private int pendingFocus = -1;

        /// <summary>Maps the real mouse onto the LCD panel and drives clicks, hover and drag. UI Toolkit render-texture panels get no pointer events from the EventSystem, so this is done by hand.</summary>
        private void PollMouse()
        {
            var m = Mouse.current;
            if (m == null || cam == null || View == null) return;
            if (Hud != null && m.leftButton.wasPressedThisFrame)
            {
                var sp0 = m.position.ReadValue();
                var r = Hud.Root.resolvedStyle;
                var hp = new Vector2(sp0.x / cam.pixelWidth * r.width, (1f - sp0.y / cam.pixelHeight) * r.height);
                if (Hud.TryClick(hp)) return;
            }
            bool zoomed = CameraRig.Instance != null && CameraRig.Instance.Current == ViewId.Crt;
            if (!zoomed)
            {
                if (View.Dragging) View.EndDrag(Vector2.zero, false);
                View.Hover(Vector2.zero, false);
                return;
            }
            var pp = ScreenToPanel(m.position.ReadValue());
            bool valid = !float.IsNaN(pp.x);
            View.Hover(pp, valid);
            if (m.leftButton.wasPressedThisFrame && valid)
            {
                int sp = View.SpeedAt(pp);
                int tile = View.TileAt(pp);
                if (sp >= 0) View.ClickSpeed(sp);
                else if (tile >= 0) Focus(tile);
                else View.TryBeginDrag(pp);
            }
            if (View.Dragging)
            {
                if (valid) View.UpdateDrag(pp);
                if (m.leftButton.wasReleasedThisFrame || !m.leftButton.isPressed) View.EndDrag(pp, valid);
            }
        }

        private void Update()
        {
            if (Career == null) return;
            PollMouse();
            if (Hud != null)
            {
                if (Hud.Ringing && PhoneView.Instance != null) PhoneView.Instance.Ring(0.2f);
                if (RoomInput.Instance != null) RoomInput.Instance.InputBlocked = Hud.Modal;
            }
            float dt = Mathf.Min(Time.deltaTime, 0.1f) * timeScale;
            int steps = Mathf.Max(1, Mathf.CeilToInt(dt / 0.05f));
            for (int i = 0; i < steps; i++)
            {
                Career.Current.Step(dt / steps);
                foreach (var p in order)
                    foreach (var e in p.viewer.DrainEvents()) Handle(p, e);
            }
            var day = Career.Current;
            if (day.DockChanged)
            {
                day.DockChanged = false;
                View.RenderDock(day.Dock);
            }
            View.SetTotal(day.Minutes, day.Goal);
            if (focus != null)
            {
                View.Phone.SetProgress(focus.viewer.Progress);
                View.Phone.SetTyping(focus.viewer.TypedText);
                View.Phone.Tick(Time.time);
            }
        }

        private string ClockText()
        {
            float h = 9f + Career.Current.Time / Mathf.Max(1f, tuning.dayLength) * 8f;
            int hh = Mathf.FloorToInt(h);
            int mm = Mathf.FloorToInt((h - hh) * 60f);
            return hh + ":" + mm.ToString("00");
        }

        private void Log(Pres p, string text)
        {
            View.AddLog(p.color, p.viewer.Person.Profile.key, text, ClockText());
        }

        private static string Short(FeedPost post)
        {
            return post.text.Length > 40 ? post.text.Substring(0, 40) + "..." : post.text;
        }

        private void Handle(Pres p, ViewerEvent e)
        {
            bool f = p == focus;
            switch (e.Kind)
            {
                case ViewerEventKind.VideoStarted:
                case ViewerEventKind.Skipped:
                    PlayClip(p, e.Post);
                    p.likes = vis.Next(200, 9200);
                    p.comments = vis.Next(5, 400);
                    SeedComments(p, e.Post);
                    p.tile.SetStatus(e.Kind == ViewerEventKind.Skipped ? "passou à frente" : "a ver: " + Short(e.Post));
                    if (e.Mine)
                    {
                        Log(p, "está a ver o teu post");
                        p.tile.Pulse(new Color(1f, 0.71f, 0.28f));
                    }
                    if (f) BindPhone(p);
                    break;
                case ViewerEventKind.Liked:
                    p.likes++;
                    p.tile.Pulse(new Color(1f, 0.23f, 0.42f));
                    if (e.Post != null && !e.Post.filler) Log(p, "gostou de «" + Short(e.Post) + "»");
                    if (f)
                    {
                        View.Phone.SetLiked(true);
                        View.Phone.PopHeart();
                        View.Phone.SetCounts(p.likes, p.comments);
                    }
                    break;
                case ViewerEventKind.CommentsOpened:
                    p.tile.SetStatus("a ler os comentários");
                    if (f) View.Phone.SetSheet(true);
                    break;
                case ViewerEventKind.CommentPosted:
                    p.comments++;
                    var c = new ViewerComment { name = p.viewer.Person.Profile.key.ToLowerInvariant(), text = e.Text, color = p.color, mine = true };
                    p.list.Add(c);
                    if (p.list.Count > 8) p.list.RemoveAt(0);
                    bool angry = p.viewer.Person.Aggression > 0.6f && e.Post.inflamed;
                    p.tile.Pulse(angry ? new Color(1f, 0.35f, 0.24f) : new Color(0.36f, 0.55f, 1f));
                    p.tile.SetStatus("comentou: \"" + e.Text + "\"");
                    Log(p, "comentou: \"" + e.Text + "\"");
                    if (f)
                    {
                        View.Phone.AddComment(c);
                        View.Phone.SetCounts(p.likes, p.comments);
                    }
                    break;
                case ViewerEventKind.CommentsClosed:
                    if (f) View.Phone.SetSheet(false);
                    break;
                case ViewerEventKind.FellAsleep:
                    p.tile.SetAsleep(true);
                    p.tile.SetStatus("pousou o telemóvel");
                    Log(p, "pousou o telemóvel");
                    if (f) View.Phone.SetAsleep(true);
                    break;
                case ViewerEventKind.Woke:
                    p.tile.SetAsleep(false);
                    if (f) View.Phone.SetAsleep(false);
                    break;
            }
        }

        private void PlayClip(Pres p, FeedPost post)
        {
            var clip = clips != null ? clips.Find(post.clip) : null;
            if (clip == null) return;
            p.pendingTime = (float)(vis.NextDouble() * Mathf.Max(0.1f, (float)clip.length - 1.5f));
            p.player.Stop();
            p.player.clip = clip;
            p.player.Prepare();
        }

        private void SeedComments(Pres p, FeedPost post)
        {
            p.list.Clear();
            var names = content.content.Pool("names");
            var pool = content.content.Pool(post.inflamed ? "generic.hot" : "generic.calm");
            if (names == null || pool == null) return;
            var cols = new[] { "#ffd166", "#9be7ff", "#c9b6ff", "#ffadad", "#b5e48c" };
            int n = vis.Next(3, 6);
            for (int i = 0; i < n; i++)
            {
                ColorUtility.TryParseHtmlString(cols[vis.Next(cols.Length)], out var col);
                p.list.Add(new ViewerComment { name = names[vis.Next(names.Count)], text = pool[vis.Next(pool.Count)], color = col });
            }
        }


        private IEnumerator DayFlow()
        {
            var dlg = content.content.dialogue;
            while (Career != null)
            {
                var day = Career.Current;
                day.Running = false;
                SyncViewers();
                View.SetDay(Career.Day);
                if (focus == null && order.Count > 0) Focus(0);
                View.RenderDock(day.Dock);
                View.ClearLog();
                var dd = dlg.Day(Career.Day);
                string goal = Mathf.RoundToInt(day.Goal).ToString();
                bool done = false;
                Hud.ShowDayCard(dd != null ? dd.title : "Dia " + Career.Day, dd != null ? dd.subtitle : "", 2f, () => done = true);
                yield return new WaitUntil(() => done);
                if (dd != null && dd.pre.Count > 0)
                {
                    done = false;
                    Hud.ShowDialogue(dd.pre, goal, () => done = true);
                    yield return new WaitUntil(() => done);
                }
                done = false;
                Hud.ShowRing(() => done = true);
                yield return new WaitUntil(() => done);
                if (dd != null)
                {
                    done = false;
                    Hud.ShowDialogue(dd.call, goal, () => done = true);
                    yield return new WaitUntil(() => done);
                }
                day.Running = true;
                while (day.Running) yield return null;
                yield return new WaitForSeconds(2.5f);
                bool met = day.GoalMet;
                done = false;
                Hud.ShowRing(() => done = true);
                yield return new WaitUntil(() => done);
                done = false;
                Hud.ShowDialogue(met ? dlg.met : dlg.missed, goal, () => done = true);
                yield return new WaitUntil(() => done);
                Career.EndDay();
                if (Career.Fired)
                {
                    done = false;
                    Hud.ShowDialogue(dlg.fired, goal, () => done = true);
                    yield return new WaitUntil(() => done);
                }
                bool last = Career.Fired || Career.Finished;
                string title = met ? "Meta cumprida" : "Meta falhada";
                string body = met ? "O Dr. Valente ficou satisfeito." : "Aviso " + Career.Warnings + " de " + tuning.warningsToBeFired + ".";
                string button = last ? "Recomeçar" : "Dia " + Career.Day;
                if (Career.Fired) { title = "Despedido"; body = "Falhaste as metas. O bairro continua lá fora."; }
                else if (Career.Finished) { title = "Fim da demonstração"; body = "Os cinco dias estão feitos. Os finais ainda não existem."; }
                done = false;
                Hud.ShowSummary(title, body, Career.Warnings, tuning.warningsToBeFired, button, () => done = true);
                yield return new WaitUntil(() => done);
                if (last)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
                    yield break;
                }
            }
        }

        public string DebugInfo()
        {
            var sb = new System.Text.StringBuilder();
            var root = document.rootVisualElement;
            var log = root.Q<VisualElement>(className: "cf-log");
            sb.Append("log bg=" + log.resolvedStyle.backgroundColor + " ");
            var vid = root.Q<VisualElement>(className: "cf-video");
            sb.Append("video rt=" + (vid.resolvedStyle.backgroundImage.renderTexture != null) + " " + vid.resolvedStyle.width + "x" + vid.resolvedStyle.height + " | ");
            foreach (var p in order)
                sb.Append(p.viewer.Person.Profile.key + ":playing=" + p.player.isPlaying + ",prep=" + p.player.isPrepared + ",clip=" + (p.player.clip != null ? p.player.clip.name : "null") + ",t=" + p.player.time.ToString("0.0") + " ");
            sb.Append(" | down=" + lastDown + " up=" + lastUp + " dockCount=" + Career.Current.Dock.Count);
            return sb.ToString();
        }

        private void Give(FeedPost post, int index)
        {
            if (index < 0 || index >= order.Count) return;
            var p = order[index];
            if (Career.Current.Give(post, p.viewer))
            {
                p.tile.SetStatus("o teu post vem a seguir");
                p.tile.Pulse(new Color(1f, 0.71f, 0.28f));
                Log(p, "vai receber o teu post");
            }
            View.RenderDock(Career.Current.Dock);
        }

        private void Discard(FeedPost post)
        {
            Career.Current.Discard(post);
            View.RenderDock(Career.Current.Dock);
        }
    }
}
