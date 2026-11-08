using UnityEngine;
using UnityEngine.UIElements;

namespace EchoChamber
{
    /// <summary>A neighbour card in the left rail: mirrors their video, pulses when something happens.</summary>
    public class TileView
    {
        public VisualElement Root;
        public VisualElement Mini;
        private readonly VisualElement shade;
        private readonly Label status;
        private readonly Label time;
        private readonly VisualElement ring;
        private IVisualElementScheduledItem ringOff;

        public TileView(string name, Color color)
        {
            Root = new VisualElement();
            Root.AddToClassList("cf-tile");
            Root.AddToClassList("cf-tile--hidden");
            Mini = new VisualElement();
            Mini.AddToClassList("cf-mini");
            shade = new VisualElement { pickingMode = PickingMode.Ignore };
            shade.AddToClassList("cf-mini-shade");
            Mini.Add(shade);
            Root.Add(Mini);
            var meta = new VisualElement();
            meta.AddToClassList("cf-meta");
            var row = new VisualElement();
            row.AddToClassList("cf-name-row");
            var dot = new VisualElement();
            dot.AddToClassList("cf-dot");
            dot.style.backgroundColor = color;
            row.Add(dot);
            var nm = new Label(name);
            nm.AddToClassList("cf-name");
            row.Add(nm);
            meta.Add(row);
            status = new Label("a ligar...");
            status.AddToClassList("cf-status");
            meta.Add(status);
            time = new Label("0:00");
            time.AddToClassList("cf-time");
            time.style.display = DisplayStyle.None;
            meta.Add(time);
            Root.Add(meta);
            ring = new VisualElement { pickingMode = PickingMode.Ignore };
            ring.AddToClassList("cf-ring");
            Root.Add(ring);
            foreach (var c in Root.Children()) c.pickingMode = c == Root ? PickingMode.Position : c.pickingMode;
        }

        public void Show() { Root.RemoveFromClassList("cf-tile--hidden"); }
        public void SetStatus(string s) { status.text = s; }
        public void SetTime(string s) { time.text = s; }
        public void SetSelected(bool on) { Root.EnableInClassList("cf-tile--sel", on); }
        public void SetDrop(bool on) { Root.EnableInClassList("cf-tile--drop", on); }
        public void SetAsleep(bool on) { shade.EnableInClassList("cf-mini-shade--asleep", on); }

        public void SetVideo(RenderTexture rt)
        {
            Mini.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(rt));
            Mini.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(BackgroundSizeType.Cover));
        }

        public void Pulse(Color c)
        {
            ring.style.borderTopColor = ring.style.borderBottomColor = ring.style.borderLeftColor = ring.style.borderRightColor = c;
            ring.AddToClassList("cf-ring--flash");
            ringOff?.Pause();
            ringOff = ring.schedule.Execute(() => ring.RemoveFromClassList("cf-ring--flash")).StartingIn(60);
        }
    }
}
