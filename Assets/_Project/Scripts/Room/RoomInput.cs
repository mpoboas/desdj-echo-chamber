using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace EchoChamber
{
    /// <summary>Mouse picking, hover tooltips and keyboard shortcuts for the control room and the street.</summary>
    public class RoomInput : MonoBehaviour
    {
        public Camera cam;
        public LayerMask mask = ~0;

        public static RoomInput Instance { get; private set; }
        public RoomClickable Hovered { get; private set; }
        public bool InputBlocked { get; set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null || Mouse.current == null) return;
            Vector2 pos = Mouse.current.position.ReadValue();

            Hovered = InputBlocked ? null : Pick(pos);
            var tip = TooltipView.Instance;
            if (tip != null)
            {
                if (Hovered != null && !string.IsNullOrEmpty(Hovered.tooltip)) tip.Show(Hovered.tooltip, pos);
                else tip.Hide();
            }

            if (InputBlocked) return;
            if (Mouse.current.leftButton.wasPressedThisFrame && !PointerOverUi())
            {
                if (Hovered != null) Dispatch(Hovered);
            }

            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.escapeKey.wasPressedThisFrame) RoomEvents.RaiseViewRequested(ViewId.Home);
        }

        private static bool PointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private RoomClickable Pick(Vector2 screenPos)
        {
            if (PointerOverUi()) return null;
            Ray ray = cam.ScreenPointToRay(screenPos);
            var hits = Physics.RaycastAll(ray, 60f, mask, QueryTriggerInteraction.Collide);
            if (hits.Length == 0) return null;
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var h in hits)
            {
                var c = h.collider.GetComponentInParent<RoomClickable>();
                if (c != null) return c;
                // opaque non-interactive geometry blocks what is behind it
                if (!h.collider.isTrigger) return null;
            }
            return null;
        }

        private void Dispatch(RoomClickable c)
        {
            switch (c.action)
            {
                case ClickAction.Zoom:
                    RoomEvents.RaiseViewRequested(c.view);
                    break;
                default:
                    RoomEvents.RaiseClicked(c.action, c);
                    break;
            }
        }
    }
}
