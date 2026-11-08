using UnityEngine;
using UnityEngine.UIElements;

namespace EchoChamber
{
    /// <summary>Vector icon (heart, comment, share) drawn with Painter2D.</summary>
    public class IconElement : VisualElement
    {
        public enum Kind { Heart, Comment, Share }

        private readonly Kind kind;
        private Color color = Color.white;

        public IconElement(Kind kind, float size)
        {
            this.kind = kind;
            style.width = size;
            style.height = size;
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }

        public void SetColor(Color c)
        {
            color = c;
            MarkDirtyRepaint();
        }

        private void Draw(MeshGenerationContext ctx)
        {
            var p = ctx.painter2D;
            float s = Mathf.Min(contentRect.width, contentRect.height);
            if (s <= 0f) return;
            p.fillColor = color;
            p.BeginPath();
            switch (kind)
            {
                case Kind.Heart:
                    for (int i = 0; i <= 64; i++)
                    {
                        float t = i / 64f * Mathf.PI * 2f;
                        float x = 16f * Mathf.Pow(Mathf.Sin(t), 3f);
                        float y = 13f * Mathf.Cos(t) - 5f * Mathf.Cos(2f * t) - 2f * Mathf.Cos(3f * t) - Mathf.Cos(4f * t);
                        var pt = new Vector2(s * (0.5f + x / 36f), s * (0.46f - y / 36f));
                        if (i == 0) p.MoveTo(pt); else p.LineTo(pt);
                    }
                    break;
                case Kind.Comment:
                    {
                        float l = 0.08f * s, r = 0.92f * s, tp = 0.1f * s, bt = 0.74f * s, rad = 0.16f * s;
                        AddRoundRect(p, l, tp, r, bt, rad);
                        p.ClosePath();
                        p.Fill();
                        p.BeginPath();
                        p.MoveTo(new Vector2(0.28f * s, 0.7f * s));
                        p.LineTo(new Vector2(0.24f * s, 0.94f * s));
                        p.LineTo(new Vector2(0.5f * s, 0.72f * s));
                        break;
                    }
                case Kind.Share:
                    p.MoveTo(new Vector2(0.58f * s, 0.12f * s));
                    p.LineTo(new Vector2(0.96f * s, 0.48f * s));
                    p.LineTo(new Vector2(0.58f * s, 0.84f * s));
                    p.LineTo(new Vector2(0.58f * s, 0.62f * s));
                    p.LineTo(new Vector2(0.1f * s, 0.88f * s));
                    p.LineTo(new Vector2(0.24f * s, 0.46f * s));
                    p.LineTo(new Vector2(0.58f * s, 0.34f * s));
                    break;
            }
            p.ClosePath();
            p.Fill();
        }

        private static void AddRoundRect(Painter2D p, float l, float t, float r, float b, float rad)
        {
            const int seg = 6;
            Vector2[] centers = { new Vector2(r - rad, t + rad), new Vector2(r - rad, b - rad), new Vector2(l + rad, b - rad), new Vector2(l + rad, t + rad) };
            float[] start = { -90f, 0f, 90f, 180f };
            bool first = true;
            for (int c = 0; c < 4; c++)
                for (int i = 0; i <= seg; i++)
                {
                    float a = (start[c] + 90f * i / seg) * Mathf.Deg2Rad;
                    var pt = centers[c] + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * rad;
                    if (first) { p.MoveTo(pt); first = false; } else p.LineTo(pt);
                }
        }
    }
}
