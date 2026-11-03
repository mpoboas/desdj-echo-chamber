using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace EchoChamber
{
    /// <summary>Small helpers to build uGUI hierarchies in code with the built-in dynamic font.</summary>
    public static class UiFactory
    {
        private static Font font;
        private static Sprite white;

        public static Font Font
        {
            get
            {
                if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return font;
            }
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        public static Canvas CreateCanvas(string name, Transform parent, int sortOrder, Vector2 reference)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = reference;
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        public static void Stretch(RectTransform r, float l = 0, float b = 0, float rt = 0, float t = 0)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(l, b);
            r.offsetMax = new Vector2(-rt, -t);
        }

        public static void Anchor(RectTransform r, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            r.anchorMin = anchor;
            r.anchorMax = anchor;
            r.pivot = pivot;
            r.anchoredPosition = pos;
            r.sizeDelta = size;
        }

        public static Image Image(Transform parent, Color color, string name = "Image", Sprite sprite = null)
        {
            var r = Rect(name, parent);
            var img = r.gameObject.AddComponent<Image>();
            img.color = color;
            img.sprite = sprite;
            img.raycastTarget = false;
            return img;
        }

        public static Text Text(Transform parent, string text, int size, Color color, TextAnchor anchor = TextAnchor.UpperLeft,
                                FontStyle style = FontStyle.Normal, string name = "Text")
        {
            var r = Rect(name, parent);
            var t = r.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = anchor;
            t.fontStyle = style;
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        public static Button Button(Transform parent, string label, Color bg, Color fg, int size, System.Action onClick, string name = "Button")
        {
            var img = Image(parent, bg, name);
            img.raycastTarget = true;
            var btn = img.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.85f, 0.9f);
            colors.pressedColor = new Color(0.8f, 0.7f, 0.75f);
            btn.colors = colors;
            btn.targetGraphic = img;
            var t = Text(img.transform, label, size, fg, TextAnchor.MiddleCenter, FontStyle.Bold, "Label");
            Stretch(t.rectTransform, 12, 6, 12, 6);
            var fit = img.gameObject.AddComponent<LayoutElement>();
            fit.minHeight = size * 2.2f;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }

        public static VerticalLayoutGroup VLayout(GameObject go, float spacing, RectOffset padding, bool fitHeight = true)
        {
            var v = go.AddComponent<VerticalLayoutGroup>();
            v.spacing = spacing;
            v.padding = padding;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            if (fitHeight)
            {
                var f = go.AddComponent<ContentSizeFitter>();
                f.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            return v;
        }

        public static HorizontalLayoutGroup HLayout(GameObject go, float spacing, RectOffset padding)
        {
            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.spacing = spacing;
            h.padding = padding;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;
            return h;
        }

        public static Sprite White()
        {
            if (white != null) return white;
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            for (int i = 0; i < 4; i++) tex.SetPixel(i % 2, i / 2, Color.white);
            tex.Apply();
            white = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 100f);
            return white;
        }
    }
}
