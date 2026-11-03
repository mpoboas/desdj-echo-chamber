using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace EchoChamber
{
    /// <summary>Full-screen paper cards: intro, newspaper, end of day, final report, menu. Built in code per use.</summary>
    public class OverlayView : MonoBehaviour
    {
        public static OverlayView Instance { get; private set; }
        public bool IsOpen { get; private set; }

        private Canvas canvas;
        private RectTransform paper;
        private RectTransform content;
        private ScrollRect scroll;
        private Action primary;

        private void Awake()
        {
            Instance = this;
            canvas = UiFactory.CreateCanvas("OverlayCanvas", transform, 40, new Vector2(1920, 1080));
            var dim = UiFactory.Image(canvas.transform, new Color(10f / 255f, 12f / 255f, 22f / 255f, 0.6f), "Dim");
            dim.raycastTarget = true;
            UiFactory.Stretch(dim.rectTransform);

            var p = UiFactory.Image(dim.transform, UiPalette.Paper, "Paper");
            paper = p.rectTransform;
            paper.anchorMin = paper.anchorMax = new Vector2(0.5f, 0.5f);
            paper.pivot = new Vector2(0.5f, 0.5f);
            paper.sizeDelta = new Vector2(1180f, 960f);
            paper.localRotation = Quaternion.Euler(0f, 0f, 0.4f);

            var viewport = UiFactory.Image(paper, new Color(1f, 1f, 1f, 0.004f), "Viewport");
            UiFactory.Stretch(viewport.rectTransform, 0, 0, 0, 0);
            viewport.gameObject.AddComponent<RectMask2D>();
            viewport.raycastTarget = true;

            var c = UiFactory.Rect("Content", viewport.transform);
            c.anchorMin = new Vector2(0f, 1f);
            c.anchorMax = new Vector2(1f, 1f);
            c.pivot = new Vector2(0.5f, 1f);
            c.anchoredPosition = Vector2.zero;
            c.sizeDelta = Vector2.zero;
            var v = UiFactory.VLayout(c.gameObject, 8f, new RectOffset(48, 48, 30, 30));
            content = c;

            scroll = paper.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport.rectTransform;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;

            dim.gameObject.SetActive(false);
        }

        private GameObject Dim => canvas.transform.GetChild(0).gameObject;

        /// <summary>Opens a fresh paper card and returns a builder for its content.</summary>
        public PaperCard Open(float width = 1180f)
        {
            for (int i = content.childCount - 1; i >= 0; i--) Destroy(content.GetChild(i).gameObject);
            paper.sizeDelta = new Vector2(width, Mathf.Min(960f, 1000f));
            Dim.SetActive(true);
            IsOpen = true;
            primary = null;
            if (RoomInput.Instance != null) RoomInput.Instance.InputBlocked = true;
            if (TooltipView.Instance != null) TooltipView.Instance.Hide();
            scroll.verticalNormalizedPosition = 1f;
            return new PaperCard(this, content);
        }

        public void InvokePrimary()
        {
            primary?.Invoke();
        }

        public void SetPrimary(Action a)
        {
            primary = a;
        }

        public void Close()
        {
            Dim.SetActive(false);
            IsOpen = false;
            primary = null;
            if (RoomInput.Instance != null) RoomInput.Instance.InputBlocked = false;
        }

        /// <summary>Shrinks the paper to fit its content (min 360, max 960).</summary>
        public void Fit()
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            float h = Mathf.Clamp(LayoutUtility.GetPreferredHeight(content), 360f, 960f);
            paper.sizeDelta = new Vector2(paper.sizeDelta.x, h);
        }

        private void Update()
        {
            if (!IsOpen || Keyboard.current == null) return;
            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
                primary?.Invoke();
        }
    }

    /// <summary>Fluent builder for the contents of a paper card.</summary>
    public class PaperCard
    {
        private readonly OverlayView view;
        private readonly Transform parent;
        public Transform Root => parent;

        public PaperCard(OverlayView v, Transform p)
        {
            view = v;
            parent = p;
        }

        public Text Eyebrow(string text, Transform into = null)
        {
            return UiFactory.Text(into ?? parent, text.ToUpperInvariant(), 20, UiPalette.Claret, TextAnchor.UpperLeft, FontStyle.Bold, "Eyebrow");
        }

        public Text Title(string text, int size = 54, Transform into = null)
        {
            return UiFactory.Text(into ?? parent, text, size, UiPalette.Ink, TextAnchor.UpperLeft, FontStyle.Bold, "Title");
        }

        public Text Para(string text, int size = 28, Transform into = null, Color? color = null)
        {
            var t = UiFactory.Text(into ?? parent, Rich(text), size, color ?? UiPalette.Ink, TextAnchor.UpperLeft, FontStyle.Normal, "Para");
            t.lineSpacing = 1.1f;
            return t;
        }

        public Text Small(string text, Transform into = null)
        {
            return UiFactory.Text(into ?? parent, Rich(text), 22, ReactionStyle.Hex("#4A4536"), TextAnchor.UpperLeft, FontStyle.Normal, "Small");
        }

        public Text Key(string text, Transform into = null)
        {
            var t = UiFactory.Text(into ?? parent, text.ToUpperInvariant(), 18, ReactionStyle.Hex("#6B6455"), TextAnchor.UpperLeft, FontStyle.Bold, "Key");
            return t;
        }

        public void Rule(Transform into = null)
        {
            var r = UiFactory.Image(into ?? parent, UiPalette.Rule, "Rule");
            var le = r.gameObject.AddComponent<LayoutElement>();
            le.minHeight = 2f;
            le.preferredHeight = 2f;
        }

        public Transform Space(float h, Transform into = null)
        {
            var r = UiFactory.Rect("Space", into ?? parent);
            var le = r.gameObject.AddComponent<LayoutElement>();
            le.minHeight = h;
            le.preferredHeight = h;
            return r;
        }

        public void Stat(string label, string value, Color? valueColor = null, Transform into = null)
        {
            var row = UiFactory.Rect("Stat", into ?? parent);
            var h = UiFactory.HLayout(row.gameObject, 8f, new RectOffset(0, 0, 3, 3));
            h.childForceExpandWidth = true;
            var l = UiFactory.Text(row, label, 23, UiPalette.Ink, TextAnchor.MiddleLeft, FontStyle.Normal, "Label");
            var v = UiFactory.Text(row, value, 23, valueColor ?? UiPalette.Ink, TextAnchor.MiddleRight, FontStyle.Bold, "Value");
            l.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 30);
            var le = l.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 3f;
            var lv = v.gameObject.AddComponent<LayoutElement>();
            lv.flexibleWidth = 1.2f;
            var line = UiFactory.Image(row, UiPalette.Rule, "Line");
            line.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var lr = line.rectTransform;
            lr.anchorMin = new Vector2(0f, 0f);
            lr.anchorMax = new Vector2(1f, 0f);
            lr.sizeDelta = new Vector2(0f, 1f);
            lr.anchoredPosition = Vector2.zero;
        }

        public Transform Panel(Color color, RectOffset padding, Transform into = null)
        {
            var img = UiFactory.Image(into ?? parent, color, "Panel");
            UiFactory.VLayout(img.gameObject, 4f, padding);
            return img.transform;
        }

        public Transform Columns(float leftWeight, float rightWeight, out Transform left, out Transform right)
        {
            var row = UiFactory.Rect("Columns", parent);
            var h = UiFactory.HLayout(row.gameObject, 36f, new RectOffset(0, 0, 4, 4));
            h.childForceExpandWidth = true;
            h.childAlignment = TextAnchor.UpperLeft;
            var l = UiFactory.Rect("Left", row);
            UiFactory.VLayout(l.gameObject, 8f, new RectOffset(0, 0, 0, 0), false);
            l.gameObject.AddComponent<LayoutElement>().flexibleWidth = leftWeight;
            var r = UiFactory.Rect("Right", row);
            UiFactory.VLayout(r.gameObject, 8f, new RectOffset(0, 0, 0, 0), false);
            r.gameObject.AddComponent<LayoutElement>().flexibleWidth = rightWeight;
            left = l;
            right = r;
            return row;
        }

        public Button Button(string label, Action onClick, bool primaryAction = true)
        {
            var b = UiFactory.Button(parent, label, UiPalette.Ink, UiPalette.Paper, 28, () =>
            {
                GameAudio.Click();
                onClick?.Invoke();
            }, "Button");
            var le = b.GetComponent<LayoutElement>();
            le.minHeight = 64f;
            if (primaryAction) view.SetPrimary(() => onClick?.Invoke());
            return b;
        }

        public void Done()
        {
            view.Fit();
        }

        public static string Rich(string t)
        {
            return t.Replace("<b>", "<b>").Replace("</b>", "</b>");
        }
    }
}
