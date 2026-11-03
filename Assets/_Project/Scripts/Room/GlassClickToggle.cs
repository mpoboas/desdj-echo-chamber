using UnityEngine;

namespace EchoChamber
{
    /// <summary>The window glass is clickable to zoom in, but must not block clicks on neighbours once zoomed.</summary>
    public class GlassClickToggle : MonoBehaviour
    {
        public Collider glass;

        private void OnEnable()
        {
            RoomEvents.ViewChanged += OnView;
        }

        private void OnDisable()
        {
            RoomEvents.ViewChanged -= OnView;
        }

        private void OnView(ViewId v)
        {
            if (glass != null) glass.enabled = v != ViewId.Window;
        }
    }
}
