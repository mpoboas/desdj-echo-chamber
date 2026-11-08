using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoChamber
{
    /// <summary>
    /// The Central de Feeds screen (1600x900 panel shown on the room LCD): neighbour tiles, one focused phone,
    /// profile, activity log and the dock of posts to hand out. Pure view; the controller feeds it.
    /// </summary>
    public class CentralFeedsView
    {
        public VisualElement Root;
        public FeedPhoneView Phone;
        public readonly List<TileView> Tiles = new List<TileView>();

        public Action<int> OnFocus;
        public Action<FeedPost, int> OnDropOnViewer;
        public Action<FeedPost> OnDropTrash;
        public Action<float> OnSpeed;
        public bool DragEnabled = true;

        private VisualElement rail, center, profile, log, cards, trash, phoneHolder;
        private Label dayLabel, goalValue;
        private VisualElement barFill;
        private readonly List<Label> speedButtons = new List<Label>();
        private readonly List<float> speedValues = new List<float>();
        private readonly List<VisualElement> cardElems = new List<VisualElement>();
        private readonly List<FeedPost> cardPosts = new List<FeedPost>();
        private VisualElement ghost, dragSource;
        private FeedPost dragPost;
        private int hoverTile = -1;
        public bool Dragging { get { return dragPost != null; } }

        public void Build(VisualElement parent)
        {
            Root = new VisualElement();
            Root.AddToClassList("cf-root");
            parent.Add(Root);

            var top = new VisualElement();
            top.AddToClassList("cf-top");
            var brandBox = new VisualElement();
            var brand = new Label("CENTRAL DE FEEDS");
            brand.AddToClassList("cf-brand");
            dayLabel = new Label("Dia 1");
            dayLabel.AddToClassList("cf-day");
            brandBox.Add(brand);
            brandBox.Add(dayLabel);
            top.Add(brandBox);
            var goal = new VisualElement();
            goal.AddToClassList("cf-goal");
            var row = new VisualElement();
            row.AddToClassList("cf-goal-row");
            var gl = new Label("Tempo na app hoje");
            gl.AddToClassList("cf-goal-label");
            goalValue = new Label("");
            goalValue.AddToClassList("cf-goal-value");
            row.Add(gl);
            row.Add(goalValue);
            goal.Add(row);
            var bar = new VisualElement();
            bar.AddToClassList("cf-bar");
            barFill = new VisualElement();
            barFill.AddToClassList("cf-bar-fill");
            bar.Add(barFill);
            goal.Add(bar);
            top.Add(goal);
            var speed = new VisualElement();
            speed.AddToClassList("cf-speed");
            foreach (float s in new[] { 1f, 2f, 4f })
            {
                var b = new Label(s + "x");
                b.AddToClassList("cf-speed-btn");
                speedButtons.Add(b);
                speedValues.Add(s);
                speed.Add(b);
            }
            SetSpeedHighlight(1f);
            top.Add(speed);
            Root.Add(top);

            var body = new VisualElement();
            body.AddToClassList("cf-body");
            Root.Add(body);
            rail = new VisualElement();
            rail.AddToClassList("cf-rail");
            body.Add(rail);
            center = new VisualElement();
            center.AddToClassList("cf-center");
            body.Add(center);
            Phone = new FeedPhoneView();
            phoneHolder = Phone.Root;
            center.Add(Phone.Root);
            var right = new VisualElement();
            right.AddToClassList("cf-right");
            body.Add(right);

            profile = new VisualElement();
            profile.AddToClassList("cf-profile");
            right.Add(profile);
            log = new VisualElement();
            log.AddToClassList("cf-log");
            var lt = new Label("O QUE ESTÁ A ACONTECER");
            lt.AddToClassList("cf-log-title");
            log.Add(lt);
            right.Add(log);

            var dock = new VisualElement();
            dock.AddToClassList("cf-dock");
            var left = new VisualElement();
            left.AddToClassList("cf-dock-left");
            var dt = new Label("POSTS PARA ENTREGAR");
            dt.AddToClassList("cf-dock-title");
            left.Add(dt);
            cards = new VisualElement();
            cards.AddToClassList("cf-cards");
            left.Add(cards);
            dock.Add(left);
            trash = new VisualElement();
            trash.AddToClassList("cf-trash");
            var tt = new Label("LIXO");
            tt.AddToClassList("cf-trash-text");
            trash.Add(tt);
            dock.Add(trash);
            right.Add(dock);
        }

        public void SetSpeedHighlight(float s)
        {
            for (int i = 0; i < speedButtons.Count; i++)
                speedButtons[i].EnableInClassList("cf-speed-btn--on", Mathf.Approximately(speedValues[i], s));
        }

        public TileView AddTile(string name, Color color)
        {
            int index = Tiles.Count;
            var tile = new TileView(name, color);
            Tiles.Add(tile);
            rail.Add(tile.Root);
            return tile;
        }

        public void SetDay(int day)
        {
            dayLabel.text = "Dia " + day;
        }

        public void SetTotal(float minutes, float goal)
        {
            goalValue.text = "";
            barFill.style.width = Length.Percent(Mathf.Clamp01(minutes / Mathf.Max(0.01f, goal)) * 100f);
            barFill.EnableInClassList("cf-bar-fill--met", minutes >= goal);
        }

        public static string Fmt(float minutes)
        {
            int s = Mathf.Max(0, Mathf.FloorToInt(minutes * 60f));
            return (s / 60) + ":" + (s % 60).ToString("00");
        }

        public void SetProfile(FeedProfile p, Color color)
        {
            profile.Clear();
            var av = new VisualElement();
            av.AddToClassList("cf-p-avatar");
            av.style.backgroundColor = color;
            var at = new Label(p.key.Substring(0, 1));
            at.AddToClassList("cf-p-avatar-text");
            av.Add(at);
            profile.Add(av);
            var box = new VisualElement();
            var nm = new Label(p.name);
            nm.AddToClassList("cf-p-name");
            var bio = new Label(p.bio);
            bio.AddToClassList("cf-p-bio");
            var chips = new VisualElement();
            chips.AddToClassList("cf-chips");
            foreach (var i in p.interests)
            {
                var c = new Label(FeedLabels.Theme(i));
                c.AddToClassList("cf-chip");
                chips.Add(c);
            }
            box.Add(nm);
            box.Add(bio);
            box.Add(chips);
            profile.Add(box);
        }

        public void ClearLog()
        {
            for (int i = log.childCount - 1; i >= 1; i--) log.RemoveAt(i);
        }

        public void AddLog(Color who, string name, string text, string time)
        {
            var ev = new VisualElement();
            ev.AddToClassList("cf-ev");
            var dot = new VisualElement();
            dot.AddToClassList("cf-ev-dot");
            dot.style.backgroundColor = who;
            ev.Add(dot);
            var t = new Label(name + " " + text);
            t.AddToClassList("cf-ev-text");
            ev.Add(t);
            var tm = new Label(time);
            tm.AddToClassList("cf-ev-time");
            ev.Add(tm);
            log.Add(ev);
            while (log.childCount > 7) log.RemoveAt(1);
        }

        public void RenderDock(IReadOnlyList<FeedPost> posts)
        {
            cards.Clear();
            cardElems.Clear();
            cardPosts.Clear();
            if (posts.Count == 0)
            {
                var e = new Label("à espera do próximo post...");
                e.AddToClassList("cf-empty");
                cards.Add(e);
                return;
            }
            for (int i = 0; i < posts.Count && i < 4; i++)
            {
                var post = posts[i];
                var card = MakeCard(post);
                cards.Add(card);
                cardElems.Add(card);
                cardPosts.Add(post);
            }
        }

        private static VisualElement MakeCard(FeedPost post)
        {
            var card = new VisualElement();
            card.AddToClassList("cf-card");
            var a = new Label(post.author);
            a.AddToClassList("cf-card-author");
            var t = new Label(post.text);
            t.AddToClassList("cf-card-text");
            card.Add(a);
            card.Add(t);
            return card;
        }

        public int TileAt(Vector2 world)
        {
            for (int i = 0; i < Tiles.Count; i++)
                if (!Tiles[i].Root.ClassListContains("cf-tile--hidden") && Tiles[i].Root.worldBound.Contains(world)) return i;
            return -1;
        }

        private void UpdateDrop(Vector2 world)
        {
            ClearDrop();
            int t = TileAt(world);
            if (t >= 0) Tiles[t].SetDrop(true);
            else if (phoneHolder.worldBound.Contains(world)) phoneHolder.AddToClassList("cf-phone--drop");
            else if (trash.worldBound.Contains(world)) trash.AddToClassList("cf-trash--drop");
        }

        private void ClearDrop()
        {
            foreach (var t in Tiles) t.SetDrop(false);
            phoneHolder.RemoveFromClassList("cf-phone--drop");
            trash.RemoveFromClassList("cf-trash--drop");
        }


        // ---- polled pointer API (the controller maps the real mouse to panel space) ----

        public int SpeedAt(Vector2 world)
        {
            for (int i = 0; i < speedButtons.Count; i++)
                if (speedButtons[i].worldBound.Contains(world)) return i;
            return -1;
        }

        public void ClickSpeed(int i)
        {
            SetSpeedHighlight(speedValues[i]);
            if (OnSpeed != null) OnSpeed(speedValues[i]);
        }

        public void Hover(Vector2 world, bool valid)
        {
            int t = valid && !Dragging ? TileAt(world) : -1;
            if (t == hoverTile) return;
            if (hoverTile >= 0 && hoverTile < Tiles.Count) Tiles[hoverTile].Root.RemoveFromClassList("cf-tile--hover");
            hoverTile = t;
            if (t >= 0) Tiles[t].Root.AddToClassList("cf-tile--hover");
        }

        public bool TryBeginDrag(Vector2 world)
        {
            if (!DragEnabled || Dragging) return false;
            for (int i = 0; i < cardElems.Count; i++)
            {
                if (!cardElems[i].worldBound.Contains(world)) continue;
                dragPost = cardPosts[i];
                dragSource = cardElems[i];
                ghost = MakeCard(dragPost);
                ghost.AddToClassList("cf-card--ghost");
                ghost.pickingMode = PickingMode.Ignore;
                foreach (var c in ghost.Children()) c.pickingMode = PickingMode.Ignore;
                Root.Add(ghost);
                dragSource.AddToClassList("cf-card--gone");
                UpdateDrag(world);
                return true;
            }
            return false;
        }

        public void UpdateDrag(Vector2 world)
        {
            if (!Dragging) return;
            var local = Root.WorldToLocal(world);
            ghost.style.left = local.x - 120f;
            ghost.style.top = local.y - 40f;
            UpdateDrop(world);
        }

        public void EndDrag(Vector2 world, bool valid)
        {
            if (!Dragging) return;
            var post = dragPost;
            ghost.RemoveFromHierarchy();
            dragSource.RemoveFromClassList("cf-card--gone");
            ghost = null;
            dragSource = null;
            dragPost = null;
            ClearDrop();
            if (valid) HandleDrop(post, world);
        }

        public int FocusedIndex = -1;

        private void HandleDrop(FeedPost post, Vector2 world)
        {
            int t = TileAt(world);
            if (t >= 0) { if (OnDropOnViewer != null) OnDropOnViewer(post, t); return; }
            if (phoneHolder.worldBound.Contains(world) && FocusedIndex >= 0) { if (OnDropOnViewer != null) OnDropOnViewer(post, FocusedIndex); return; }
            if (trash.worldBound.Contains(world) && OnDropTrash != null) OnDropTrash(post);
        }
    }
}
