using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EchoChamber
{
    /// <summary>Stack of short captions at the bottom of the screen (boss phone calls, radio, hints).</summary>
    public class SubtitleView : MonoBehaviour
    {
        public enum Style
        {
            Normal,
            Radio,
            Hint,
            Alarm
        }

        public static SubtitleView Instance { get; private set; }

        private class Item
        {
            public GameObject go;
            public float until;
        }

        private readonly List<Item> items = new List<Item>();
        private RectTransform stack;

        private void Awake()
        {
            Instance = this;
            var canvas = UiFactory.CreateCanvas("SubtitleCanvas", transform, 20, new Vector2(1920, 1080));
            stack = UiFactory.Rect("Stack", canvas.transform);
            UiFactory.Anchor(stack, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(980f, 10f));
            var v = UiFactory.VLayout(stack.gameObject, 8f, new RectOffset(0, 0, 0, 0));
            v.childAlignment = TextAnchor.LowerCenter;
            v.reverseArrangement = false;
        }

        public static void Say(string who, string text, Style style = Style.Normal, float seconds = 5.2f)
        {
            if (Instance != null) Instance.Add(who, text, style, seconds);
        }

        private void Add(string who, string text, Style style, float seconds)
        {
            var bg = UiFactory.Image(stack, UiPalette.Panel, "Subtitle");
            var v = UiFactory.VLayout(bg.gameObject, 2f, new RectOffset(18, 18, 10, 12));
            v.childAlignment = TextAnchor.UpperCenter;
            Color whoCol = style == Style.Radio ? UiPalette.Blue : style == Style.Hint ? UiPalette.Green : style == Style.Alarm ? UiPalette.Pink : UiPalette.Amber;
            var w = UiFactory.Text(bg.transform, who.ToUpperInvariant(), 17, whoCol, TextAnchor.UpperCenter, FontStyle.Bold, "Who");
            var t = UiFactory.Text(bg.transform, text.Replace("<b>", "<color=#FFB547><b>").Replace("</b>", "</b></color>"), 26, UiPalette.Paper, TextAnchor.UpperCenter, FontStyle.Normal, "Body");
            items.Add(new Item { go = bg.gameObject, until = Time.unscaledTime + seconds });
            while (items.Count > 3)
            {
                Destroy(items[0].go);
                items.RemoveAt(0);
            }
        }

        private void Update()
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (Time.unscaledTime >= items[i].until)
                {
                    Destroy(items[i].go);
                    items.RemoveAt(i);
                }
            }
        }
    }
}
