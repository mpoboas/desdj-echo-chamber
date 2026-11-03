using UnityEngine;
using UnityEngine.UI;

namespace EchoChamber
{
    /// <summary>Paper-coloured tooltip that follows the cursor.</summary>
    public class TooltipView : MonoBehaviour
    {
        public static TooltipView Instance { get; private set; }

        private RectTransform box;
        private Text label;
        private Canvas canvas;

        private void Awake()
        {
            Instance = this;
            canvas = UiFactory.CreateCanvas("TooltipCanvas", transform, 50, new Vector2(1920, 1080));
            var bg = UiFactory.Image(canvas.transform, UiPalette.Paper, "Tooltip");
            box = bg.rectTransform;
            box.pivot = new Vector2(0f, 1f);
            box.anchorMin = box.anchorMax = Vector2.zero;
            label = UiFactory.Text(box, "", 22, UiPalette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            UiFactory.Stretch(label.rectTransform, 12, 6, 12, 6);
            Hide();
        }

        public void Show(string text, Vector2 screenPos)
        {
            if (label.text != text)
            {
                label.text = text;
                float w = label.preferredWidth + 24f;
                box.sizeDelta = new Vector2(w, 40f);
            }
            box.gameObject.SetActive(true);
            // convert the screen position into canvas space (scaler aware)
            float s = canvas.scaleFactor;
            box.anchoredPosition = new Vector2(screenPos.x / s + 14f, screenPos.y / s - 14f);
        }

        public void Hide()
        {
            if (box != null) box.gameObject.SetActive(false);
        }
    }
}
