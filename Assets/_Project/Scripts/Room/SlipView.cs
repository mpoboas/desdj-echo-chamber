using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EchoChamber
{
    /// <summary>
    /// A printed post slip: a thin paper box with a world-space canvas on top. Handles the print reveal
    /// (blank paper cover shrinking line by line), the stamp mark, and move animations.
    /// </summary>
    public class SlipView : MonoBehaviour
    {
        public const float CanvasW = 640f;
        public const float CanvasH = 460f;
        public const float PaperW = 0.5f;
        public const float PaperD = 0.36f;
        public const int PrintLines = 12;

        private static int seq;

        public PostData Post { get; private set; }
        public string Author { get; private set; }
        public int Id { get; private set; }
        public DecisionMode? Mark { get; private set; }
        public bool Decided;
        public bool Imposed { get; private set; }
        private RectTransform canvasRt;
        private Text ink;
        public bool Moving { get; private set; }

        private RectTransform cover;
        private Text footer;
        private Text revenue;
        private RectTransform markRoot;
        private Text markText;
        private Image markBorder;

        public static SlipView Create(PostData post, string author, Material paperEdge, Transform parent)
        {
            var go = new GameObject("Slip");
            go.transform.SetParent(parent, false);
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = "Paper";
            box.transform.SetParent(go.transform, false);
            box.transform.localScale = new Vector3(PaperW, 0.003f, PaperD);
            box.GetComponent<Renderer>().sharedMaterial = paperEdge;
            UnityEngine.Object.Destroy(box.GetComponent<Collider>());
            var col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(PaperW, 0.02f, PaperD);
            var rc = go.AddComponent<RoomClickable>();
            rc.action = ClickAction.Slip;
            rc.tooltip = "Post: clica para ler de perto";

            var s = go.AddComponent<SlipView>();
            s.Post = post;
            s.Author = author;
            s.Id = ++seq;
            s.BuildCanvas(go.transform);
            return s;
        }

        private void BuildCanvas(Transform parent)
        {
            var cgo = new GameObject("SlipCanvas", typeof(RectTransform));
            cgo.transform.SetParent(parent, false);
            cgo.transform.localPosition = new Vector3(0f, 0.0018f, 0f);
            cgo.transform.localRotation = Quaternion.Euler(90f, 180f, 0f);
            cgo.transform.localScale = Vector3.one * (PaperW / CanvasW);
            var canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = cgo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(CanvasW, CanvasH);
            rt.pivot = new Vector2(0.5f, 0.5f);
            canvasRt = rt;

            Color paper = ReactionStyle.Hex("#F6F1E4");
            var bg = UiFactory.Image(rt, paper, "Paper");
            UiFactory.Stretch(bg.rectTransform);

            var strip = ReactionStyle.Hex("#EDE6D4");
            var l = UiFactory.Image(rt, strip, "StripL");
            Place(l.rectTransform, 0f, 0f, 26f, CanvasH);
            var r = UiFactory.Image(rt, strip, "StripR");
            Place(r.rectTransform, CanvasW - 26f, 0f, 26f, CanvasH);
            Color hole = ReactionStyle.Hex("#C9BFA8");
            for (float y = 14f; y < CanvasH; y += 28f)
            {
                var hl = UiFactory.Image(rt, hole, "Hole");
                Place(hl.rectTransform, 8f, y - 5f, 10f, 10f);
                var hr = UiFactory.Image(rt, hole, "Hole");
                Place(hr.rectTransform, CanvasW - 18f, y - 5f, 10f, 10f);
            }

            const float L = 36f, R = CanvasW - 36f;
            var head = UiFactory.Image(rt, ReactionStyle.CategoryColor(Post.category), "Header");
            Place(head.rectTransform, L, 8f, R - L, 40f);
            var cat = UiFactory.Text(head.transform, ReactionStyle.CategoryLabel(Post.category).ToUpperInvariant(), 22, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold, "Cat");
            UiFactory.Stretch(cat.rectTransform, 12, 0, 12, 0);
            var idt = UiFactory.Text(head.transform, "POST #" + Id.ToString("000"), 22, Color.white, TextAnchor.MiddleRight, FontStyle.Bold, "Id");
            UiFactory.Stretch(idt.rectTransform, 12, 0, 12, 0);

            var by = UiFactory.Text(rt, "de: " + Author, 22, ReactionStyle.Hex("#5A5446"), TextAnchor.MiddleLeft, FontStyle.Bold, "Author");
            Place(by.rectTransform, L + 4f, 66f, 260f, 34f);
            revenue = UiFactory.Text(rt, "", 20, ReactionStyle.Hex("#1F7A55"), TextAnchor.MiddleRight, FontStyle.Bold, "Revenue");
            Place(revenue.rectTransform, L + 200f, 66f, R - L - 204f, 34f);

            var body = UiFactory.Text(rt, Post.text, 34, ReactionStyle.Hex("#262A38"), TextAnchor.UpperLeft, FontStyle.Normal, "Body");
            body.lineSpacing = 1.05f;
            body.verticalOverflow = VerticalWrapMode.Truncate;
            Place(body.rectTransform, L + 4f, 108f, R - L - 8f, 230f);

            var rule = UiFactory.Image(rt, ReactionStyle.Hex("#C9BFA8"), "Rule");
            Place(rule.rectTransform, L, CanvasH - 62f, R - L, 2f);
            footer = UiFactory.Text(rt, "", 19, ReactionStyle.Hex("#8A8270"), TextAnchor.MiddleLeft, FontStyle.Bold, "Footer");
            Place(footer.rectTransform, L + 4f, CanvasH - 54f, R - L - 8f, 40f);

            // mark (stamp impression), hidden until stamped
            markRoot = UiFactory.Rect("Mark", rt);
            markRoot.anchorMin = markRoot.anchorMax = new Vector2(0f, 1f);
            markRoot.pivot = new Vector2(0.5f, 0.5f);
            markRoot.anchoredPosition = new Vector2(CanvasW * 0.66f, -CanvasH * 0.5f);
            markRoot.sizeDelta = new Vector2(260f, 88f);
            markRoot.localRotation = Quaternion.Euler(0f, 0f, 0.18f * Mathf.Rad2Deg);
            markBorder = UiFactory.Image(markRoot, Color.white, "Border");
            UiFactory.Stretch(markBorder.rectTransform);
            var inner = UiFactory.Image(markBorder.transform, paper, "Inner");
            UiFactory.Stretch(inner.rectTransform, 7, 7, 7, 7);
            markText = UiFactory.Text(markRoot, "", 36, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold, "MarkText");
            UiFactory.Stretch(markText.rectTransform);
            markRoot.gameObject.SetActive(false);

            // cover for the print reveal (blank paper below the print head)
            var cv = UiFactory.Image(rt, paper, "Cover");
            cover = cv.rectTransform;
            cover.anchorMin = cover.anchorMax = new Vector2(0f, 1f);
            cover.pivot = new Vector2(0f, 1f);
            SetReveal(1f);
        }

        private static void Place(RectTransform r, float x, float y, float w, float h)
        {
            r.anchorMin = r.anchorMax = new Vector2(0f, 1f);
            r.pivot = new Vector2(0f, 1f);
            r.anchoredPosition = new Vector2(x, -y);
            r.sizeDelta = new Vector2(w, h);
        }

        /// <summary>Shows how many OCULTAR stamps are left this turn.</summary>
        public void SetInk(int left)
        {
            if (ink == null)
            {
                ink = UiFactory.Text(canvasRt, "", 20, ReactionStyle.Hex("#8A8270"), TextAnchor.MiddleRight, FontStyle.Bold, "Ink");
                Place(ink.rectTransform, 40f, 352f, CanvasW - 80f, 34f);
            }
            ink.text = Imposed ? "imposto: não se oculta" : left > 0 ? $"tinta OCULTAR: {left}" : "sem tinta OCULTAR";
            ink.color = Imposed || left <= 0 ? ReactionStyle.Hex("#C8344F") : ReactionStyle.Hex("#8A8270");
        }

        /// <summary>Marks the slip as imposed by the boss (stamp OCULTAR is not allowed).</summary>
        public void SetImposed()
        {
            Imposed = true;
            // sits inside the coloured header, between the category label and the post number
            var tag = UiFactory.Image(canvasRt, ReactionStyle.Hex("#1A1A22"), "ImposedTag");
            Place(tag.rectTransform, 278f, 12f, 150f, 32f);
            var t = UiFactory.Text(tag.transform, "IMPOSTO", 20, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold, "ImposedText");
            UiFactory.Stretch(t.rectTransform);
        }

        public void SetEstimates(int est, string patternOrNull)
        {
            revenue.text = $"rende ≈ {est} € · ampl. ≈ {Mathf.RoundToInt(est * 2.2f)} €";
            footer.text = patternOrNull != null ? "observado: " + patternOrNull : "efeito no bairro: ? (observa quem o lê)";
            footer.color = patternOrNull != null ? UiPalette.Claret : ReactionStyle.Hex("#8A8270");
        }

        /// <summary>0 = blank paper, 1 = fully printed.</summary>
        public void SetReveal(float reveal)
        {
            float y = Mathf.Floor(CanvasH * Mathf.Clamp01(reveal));
            cover.anchoredPosition = new Vector2(27f, -y);
            cover.sizeDelta = new Vector2(CanvasW - 54f, CanvasH - y);
            cover.gameObject.SetActive(reveal < 1f);
        }

        public void SetMark(DecisionMode mode)
        {
            Mark = mode;
            Color col;
            string txt;
            switch (mode)
            {
                case DecisionMode.Amplify: col = ReactionStyle.Hex("#1F9D6B"); txt = "AMPLIFICAR"; break;
                case DecisionMode.Hide: col = ReactionStyle.Hex("#C8344F"); txt = "OCULTAR"; break;
                case DecisionMode.Normal: col = ReactionStyle.Hex("#5B6478"); txt = "NORMAL"; break;
                default: col = ReactionStyle.Hex("#8A8270"); txt = "AUTO"; break;
            }
            markBorder.color = col;
            markText.color = col;
            markText.text = txt;
            markRoot.gameObject.SetActive(true);
        }

        private static float Ease(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        public void MoveTo(Vector3 pos, Quaternion rot, float duration, float arc, float scaleTo, Action done)
        {
            StopAllCoroutines();
            StartCoroutine(MoveRoutine(pos, rot, duration, arc, scaleTo, done));
        }

        private IEnumerator MoveRoutine(Vector3 pos, Quaternion rot, float duration, float arc, float scaleTo, Action done)
        {
            Moving = true;
            Vector3 p0 = transform.position;
            Quaternion q0 = transform.rotation;
            float s0 = transform.localScale.x;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, duration);
                float k = Ease(Mathf.Clamp01(t));
                Vector3 p = Vector3.Lerp(p0, pos, k);
                p.y += Mathf.Sin(k * Mathf.PI) * arc;
                transform.position = p;
                transform.rotation = Quaternion.Slerp(q0, rot, k);
                if (scaleTo >= 0f) transform.localScale = Vector3.one * Mathf.Lerp(s0, scaleTo, k);
                yield return null;
            }
            Moving = false;
            done?.Invoke();
        }
    }
}
