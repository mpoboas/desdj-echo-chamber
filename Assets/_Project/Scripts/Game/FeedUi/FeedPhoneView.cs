using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoChamber
{
    /// <summary>The focused phone: video, overlay text, like and comment counters, comment sheet. Bound to one viewer at a time.</summary>
    public class FeedPhoneView
    {
        public VisualElement Root;
        private readonly VisualElement screen;
        private readonly VisualElement video;
        private readonly VisualElement overlay;
        private readonly Label avatarText;
        private readonly VisualElement avatar;
        private readonly Label likesLabel, commentsLabel, who, caption, music;
        private readonly IconElement heart, heartPop;
        private readonly VisualElement progress;
        private readonly VisualElement sheet, sheetList, asleep;
        private readonly Label field, sendLabel;
        private IVisualElementScheduledItem popOff;
        private float shake;

        private static readonly Color Pink = new Color(1f, 0.231f, 0.42f);

        public FeedPhoneView()
        {
            Root = new VisualElement();
            Root.AddToClassList("cf-phone");
            screen = new VisualElement();
            screen.AddToClassList("cf-screen");
            Root.Add(screen);

            video = Abs("cf-video");
            screen.Add(video);
            var shade = Abs("cf-shade");
            shade.style.backgroundImage = new StyleBackground(MakeShade());
            shade.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(BackgroundSizeType.Cover));
            screen.Add(shade);

            overlay = new VisualElement { pickingMode = PickingMode.Ignore };
            overlay.AddToClassList("cf-overlay");
            screen.Add(overlay);

            var tabs = Abs("cf-tabs");
            var t1 = new Label("A seguir"); t1.AddToClassList("cf-tab");
            var t2 = new Label("Para ti"); t2.AddToClassList("cf-tab"); t2.AddToClassList("cf-tab--on");
            tabs.Add(t1); tabs.Add(t2);
            screen.Add(tabs);
            var notch = Abs("cf-notch");
            screen.Add(notch);

            var side = Abs("cf-side");
            avatar = new VisualElement();
            avatar.AddToClassList("cf-avatar");
            avatarText = new Label("A");
            avatarText.AddToClassList("cf-avatar-text");
            avatar.Add(avatarText);
            side.Add(avatar);
            heart = new IconElement(IconElement.Kind.Heart, 36);
            likesLabel = new Label("0");
            likesLabel.AddToClassList("cf-side-count");
            side.Add(Btn(heart, likesLabel));
            var com = new IconElement(IconElement.Kind.Comment, 34);
            commentsLabel = new Label("0");
            commentsLabel.AddToClassList("cf-side-count");
            side.Add(Btn(com, commentsLabel));
            var share = new IconElement(IconElement.Kind.Share, 34);
            var shareLabel = new Label("partilhar");
            shareLabel.AddToClassList("cf-side-count");
            side.Add(Btn(share, shareLabel));
            screen.Add(side);

            var bottom = Abs("cf-bottom");
            who = new Label(); who.AddToClassList("cf-who");
            caption = new Label(); caption.AddToClassList("cf-cap");
            music = new Label(); music.AddToClassList("cf-music");
            bottom.Add(who); bottom.Add(caption); bottom.Add(music);
            screen.Add(bottom);

            progress = new VisualElement { pickingMode = PickingMode.Ignore };
            progress.AddToClassList("cf-progress");
            screen.Add(progress);

            heartPop = new IconElement(IconElement.Kind.Heart, 96);
            heartPop.AddToClassList("cf-heart-pop");
            heartPop.SetColor(Pink);
            screen.Add(heartPop);

            sheet = new VisualElement();
            sheet.AddToClassList("cf-sheet");
            var head = new Label("Comentários");
            head.AddToClassList("cf-sheet-head");
            sheet.Add(head);
            sheetList = new VisualElement();
            sheetList.AddToClassList("cf-sheet-list");
            sheet.Add(sheetList);
            var input = new VisualElement();
            input.AddToClassList("cf-sheet-input");
            var fieldBox = new VisualElement();
            fieldBox.AddToClassList("cf-field");
            field = new Label();
            field.AddToClassList("cf-field-text");
            fieldBox.Add(field);
            input.Add(fieldBox);
            sendLabel = new Label("Enviar");
            sendLabel.AddToClassList("cf-send");
            input.Add(sendLabel);
            sheet.Add(input);
            screen.Add(sheet);

            asleep = Abs("cf-asleep");
            var at = new Label("pousou o telemóvel");
            at.AddToClassList("cf-asleep-text");
            asleep.Add(at);
            screen.Add(asleep);
        }

        private static VisualElement Abs(string cls)
        {
            var e = new VisualElement { pickingMode = PickingMode.Ignore };
            e.AddToClassList(cls);
            return e;
        }

        private static VisualElement Btn(VisualElement icon, Label count)
        {
            var b = new VisualElement { pickingMode = PickingMode.Ignore };
            b.AddToClassList("cf-side-btn");
            b.Add(icon);
            b.Add(count);
            return b;
        }

        private static Texture2D MakeShade()
        {
            var tex = new Texture2D(1, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (int y = 0; y < 64; y++)
            {
                float v = y / 63f; // 0 = bottom
                float a = Mathf.Clamp01((0.45f - v) / 0.45f) * 0.72f + Mathf.Clamp01((v - 0.84f) / 0.16f) * 0.35f;
                tex.SetPixel(0, y, new Color(0f, 0f, 0f, a));
            }
            tex.Apply();
            return tex;
        }

        public void SetVideo(RenderTexture rt)
        {
            video.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(rt));
            video.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(BackgroundSizeType.Cover));
        }

        public void SetPost(FeedPost post, bool hot, int likes, int comments, Texture2D overlayTexture)
        {
            overlay.style.backgroundImage = overlayTexture != null ? new StyleBackground(overlayTexture) : new StyleBackground(StyleKeyword.None);
            string creator = post.author ?? "";
            string handle = creator.StartsWith("@") ? creator : "@" + System.Text.RegularExpressions.Regex.Replace(creator.ToLowerInvariant(), @"\W+", "");
            who.text = handle;
            caption.text = post.text;
            music.text = "som original · " + creator;
            avatarText.text = creator.Length > 0 ? (creator[0] == '@' && creator.Length > 1 ? creator.Substring(1, 1).ToUpperInvariant() : creator.Substring(0, 1)) : "?";
            var cols = new[] { "#ffd166", "#9be7ff", "#c9b6ff", "#ffadad" };
            ColorUtility.TryParseHtmlString(cols[Mathf.Abs(creator.GetHashCode()) % cols.Length], out var c);
            avatar.style.backgroundColor = c;
            SetCounts(likes, comments);
            shake = hot ? 1f : 0f;
        }

        public void SetCounts(int likes, int comments)
        {
            likesLabel.text = Abbr(likes);
            commentsLabel.text = Abbr(comments);
        }

        private static string Abbr(int n)
        {
            return n >= 1000 ? (n / 1000f).ToString("0.0").Replace(".0", "") + " mil" : n.ToString();
        }

        public void SetLiked(bool liked)
        {
            heart.SetColor(liked ? Pink : Color.white);
        }

        public void PopHeart()
        {
            heartPop.AddToClassList("cf-heart-pop--on");
            popOff?.Pause();
            popOff = heartPop.schedule.Execute(() => heartPop.RemoveFromClassList("cf-heart-pop--on")).StartingIn(700);
        }

        public void SetProgress(float p)
        {
            progress.style.width = Length.Percent(p * 100f);
        }

        public void SetSheet(bool open) { sheet.EnableInClassList("cf-sheet--open", open); }
        public void SetAsleep(bool on) { asleep.EnableInClassList("cf-asleep--on", on); }

        public void SetTyping(string typed)
        {
            bool empty = string.IsNullOrEmpty(typed);
            field.text = empty ? "Adicionar comentário..." : typed + "|";
            field.EnableInClassList("cf-field-text--empty", empty);
            sendLabel.style.opacity = empty ? 0.35f : 1f;
        }

        public void SetComments(List<ViewerComment> list)
        {
            sheetList.Clear();
            foreach (var c in list) sheetList.Add(MakeComment(c));
        }

        public void AddComment(ViewerComment c)
        {
            sheetList.Add(MakeComment(c));
            while (sheetList.childCount > 8) sheetList.RemoveAt(0);
        }

        private static VisualElement MakeComment(ViewerComment c)
        {
            var row = new VisualElement();
            row.AddToClassList("cf-comment");
            var av = new VisualElement();
            av.AddToClassList("cf-c-avatar");
            av.style.backgroundColor = c.color;
            var at = new Label(c.name.Length > 0 ? c.name.Substring(0, 1).ToUpperInvariant() : "?");
            at.AddToClassList("cf-c-av-text");
            av.Add(at);
            row.Add(av);
            var body = new VisualElement();
            body.AddToClassList("cf-c-body");
            var nm = new Label(c.name);
            nm.AddToClassList("cf-c-name");
            if (c.mine) nm.AddToClassList("cf-c-name--mine");
            var tx = new Label(c.text);
            tx.AddToClassList("cf-c-text");
            body.Add(nm);
            body.Add(tx);
            row.Add(body);
            return row;
        }

        /// <summary>Tiny jitter on inflamed clips, like a shaking caption.</summary>
        public void Tick(float time)
        {
            if (shake > 0f) overlay.style.translate = new Translate(Mathf.Sin(time * 38f) * 2.5f, 0f);
            else overlay.style.translate = new Translate(0f, 0f);
        }
    }

    public struct ViewerComment
    {
        public string name;
        public string text;
        public Color color;
        public bool mine;
    }
}
