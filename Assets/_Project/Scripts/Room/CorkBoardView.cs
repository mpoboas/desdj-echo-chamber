using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EchoChamber
{
    /// <summary>
    /// The cork board: 14 neighbour cards that fill up with what the player has noted. Starts empty
    /// ("traço: ?", "sem observações"), shows discovered traits, reaction chips and red strings for relations.
    /// </summary>
    public class CorkBoardView : MonoBehaviour
    {
        public Sprite disc;
        public float worldWidth = 1.12f;

        private const float W = 1100f, H = 760f, CW = 200f, CH = 200f;
        private RectTransform root;
        private Text percent;
        private int lastVersion = -1;
        private float timer;

        private void OnEnable()
        {
            GameManager.SimulationCreated += ResetBoard;
        }

        private void OnDisable()
        {
            GameManager.SimulationCreated -= ResetBoard;
        }

        private void ResetBoard()
        {
            lastVersion = -1;
        }

        private void Start()
        {
            var go = new GameObject("BoardCanvas", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0f, 0.003f);
            go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            go.transform.localScale = Vector3.one * (worldWidth / W);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            root = go.GetComponent<RectTransform>();
            root.sizeDelta = new Vector2(W, H);
            root.pivot = new Vector2(0.5f, 0.5f);
        }

        private static Vector2 CardPos(int i)
        {
            return new Vector2(40f + (i % 5) * 210f, 72f + (i / 5) * 228f);
        }

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer > 0f || root == null) return;
            timer = 0.4f;
            var gm = GameManager.Instance;
            if (gm == null || gm.Sim == null) return;
            if (gm.Sim.Discovery.Version == lastVersion) return;
            lastVersion = gm.Sim.Discovery.Version;
            Rebuild(gm.Sim);
        }

        private void Place(RectTransform r, float x, float y, float w, float h)
        {
            r.anchorMin = r.anchorMax = new Vector2(0f, 1f);
            r.pivot = new Vector2(0f, 1f);
            r.anchoredPosition = new Vector2(x, -y);
            r.sizeDelta = new Vector2(w, h);
        }

        private void Rebuild(GameSimulation sim)
        {
            for (int i = root.childCount - 1; i >= 0; i--) Destroy(root.GetChild(i).gameObject);

            var title = UiFactory.Text(root, "O QUE SEI DO BAIRRO", 30, UiPalette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold, "Title");
            Place(title.rectTransform, 30f, 20f, 600f, 40f);
            percent = UiFactory.Text(root, Mathf.RoundToInt(sim.Understanding * 100f) + "% percebido", 26, UiPalette.Ink, TextAnchor.MiddleRight, FontStyle.Bold, "Percent");
            Place(percent.rectTransform, W - 430f, 20f, 400f, 40f);

            // red strings between related neighbours (drawn first, under the cards)
            var ns = sim.Neighbours;
            foreach (string key in sim.Discovery.RelationKeys)
            {
                var parts = key.Split('-');
                int a = int.Parse(parts[0]), b = int.Parse(parts[1]);
                Vector2 pa = CardPos(a) + new Vector2(CW / 2f, 10f);
                Vector2 pb = CardPos(b) + new Vector2(CW / 2f, 10f);
                var line = UiFactory.Image(root, new Color(0.745f, 0.118f, 0.176f, 0.85f), "String");
                var r = line.rectTransform;
                r.anchorMin = r.anchorMax = new Vector2(0f, 1f);
                r.pivot = new Vector2(0.5f, 0.5f);
                Vector2 d = pb - pa;
                r.anchoredPosition = new Vector2((pa.x + pb.x) / 2f, -(pa.y + pb.y) / 2f);
                r.sizeDelta = new Vector2(d.magnitude, 4f);
                r.localRotation = Quaternion.Euler(0f, 0f, -Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            }

            for (int i = 0; i < ns.Count; i++) BuildCard(ns[i], i);

            var foot = UiFactory.Text(root, "clica nos vizinhos pela janela quando reagem a um post · cor = categoria · símbolo = reação · fio = andam juntos", 15, UiPalette.Ink, TextAnchor.MiddleLeft, FontStyle.Normal, "Legend");
            Place(foot.rectTransform, 30f, H - 34f, W - 60f, 24f);
        }

        private void BuildCard(Neighbour n, int i)
        {
            Vector2 p = CardPos(i);
            var card = UiFactory.Image(root, ReactionStyle.Hex("#F7F3EA"), "Card_" + n.Name);
            Place(card.rectTransform, p.x, p.y, CW, CH);
            card.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            card.rectTransform.anchoredPosition = new Vector2(p.x + CW / 2f, -(p.y + CH / 2f));
            card.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -((i * 37) % 7 - 3) * 0.012f * Mathf.Rad2Deg);
            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.35f);
            shadow.effectDistance = new Vector2(3f, -4f);

            Color col = ReactionStyle.Hex(n.Data.color);
            var head = UiFactory.Image(card.transform, col, "Portrait");
            Place(head.rectTransform, 12f, 12f, CW - 24f, 80f);
            var face = UiFactory.Image(card.transform, ReactionStyle.Hex("#F0C9A0"), "Face", disc);
            face.rectTransform.anchorMin = face.rectTransform.anchorMax = new Vector2(0f, 1f);
            face.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            face.rectTransform.anchoredPosition = new Vector2(CW / 2f, -46f);
            face.rectTransform.sizeDelta = new Vector2(40f, 40f);
            var shoulders = UiFactory.Image(card.transform, col, "Shoulders");
            Place(shoulders.rectTransform, CW / 2f - 28f, 68f, 56f, 24f);
            shoulders.color = Color.Lerp(col, Color.black, 0.15f);

            var nm = UiFactory.Text(card.transform, n.Name, 22, UiPalette.Ink, TextAnchor.UpperLeft, FontStyle.Bold, "Name");
            Place(nm.rectTransform, 14f, 98f, CW - 20f, 26f);
            if (n.TraitKnown)
            {
                var tr = UiFactory.Text(card.transform, n.Data.traitLabel.ToUpperInvariant(), 15, UiPalette.Claret, TextAnchor.UpperLeft, FontStyle.Bold, "Trait");
                Place(tr.rectTransform, 14f, 124f, CW - 20f, 20f);
                var td = UiFactory.Text(card.transform, Texts.TraitDescription(n.Trait, n.Data.traitLabel), 12, ReactionStyle.Hex("#5A5446"), TextAnchor.UpperLeft, FontStyle.Normal, "TraitDesc");
                Place(td.rectTransform, 14f, 142f, CW - 24f, 30f);
            }
            else
            {
                var tr = UiFactory.Text(card.transform, "traço: ?", 15, ReactionStyle.Hex("#9A917E"), TextAnchor.UpperLeft, FontStyle.Bold, "Trait");
                Place(tr.rectTransform, 14f, 124f, CW - 20f, 20f);
            }

            int start = Mathf.Max(0, n.Notes.Count - 6);
            for (int k = start; k < n.Notes.Count; k++)
            {
                var note = n.Notes[k];
                int c = k - start;
                var chip = UiFactory.Image(card.transform, ReactionStyle.CategoryColor(note.Category), "Chip");
                Place(chip.rectTransform, 14f + c * 30f, 170f, 26f, 20f);
                var g = UiFactory.Text(chip.transform, ReactionStyle.Glyph(note.Kind) + (note.Strong ? "!" : ""), 15, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold, "Glyph");
                UiFactory.Stretch(g.rectTransform);
                g.horizontalOverflow = HorizontalWrapMode.Overflow;
            }
            if (n.Notes.Count == 0)
            {
                var none = UiFactory.Text(card.transform, "sem observações", 13, ReactionStyle.Hex("#B5AC98"), TextAnchor.UpperLeft, FontStyle.Bold, "None");
                Place(none.rectTransform, 14f, 172f, CW - 20f, 20f);
            }

            var last = n.Notes.Count > 0 ? n.Notes[n.Notes.Count - 1] : null;
            var pin = UiFactory.Image(card.transform, last != null ? ReactionStyle.Color(last.Kind) : ReactionStyle.Hex("#9A917E"), "Pin", disc);
            pin.rectTransform.anchorMin = pin.rectTransform.anchorMax = new Vector2(0f, 1f);
            pin.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            pin.rectTransform.anchoredPosition = new Vector2(CW / 2f, -6f);
            pin.rectTransform.sizeDelta = new Vector2(24f, 24f);
        }
    }
}
