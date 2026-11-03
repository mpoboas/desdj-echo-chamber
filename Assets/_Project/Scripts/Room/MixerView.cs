using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoChamber
{
    /// <summary>The mixing desk: five draggable faders that set the reach of each post category (0 to 100).</summary>
    public class MixerView : MonoBehaviour
    {
        public static bool Dragging { get; private set; }

        public Transform[] knobs = new Transform[5];
        public TextMesh[] valueLabels = new TextMesh[5];
        public float zAtZero = 0.1f;
        public float zAtFull = -0.1f;
        public float surfaceWorldY = -0.1f;
        public Camera cam;

        private PostCategory? drag;

        private void OnEnable()
        {
            RoomEvents.Clicked += OnClicked;
        }

        private void OnDisable()
        {
            RoomEvents.Clicked -= OnClicked;
            Dragging = false;
        }

        private void OnClicked(ClickAction action, RoomClickable c)
        {
            if (action != ClickAction.Fader || c == null) return;
            var sim = GameManager.Instance.Sim;
            if (sim.Phase == GamePhase.Simulate)
            {
                SubtitleView.Say("Mesa", "Os faders estão bloqueados durante a simulação.", SubtitleView.Style.Hint, 2.5f);
                return;
            }
            drag = c.category;
            Dragging = true;
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Sim == null) return;
            var sim = gm.Sim;
            if (cam == null) cam = Camera.main;

            if (drag.HasValue && Mouse.current != null)
            {
                if (Mouse.current.leftButton.isPressed)
                {
                    Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
                    var plane = new Plane(Vector3.up, new Vector3(0f, surfaceWorldY, 0f));
                    if (plane.Raycast(ray, out float enter))
                    {
                        Vector3 local = transform.InverseTransformPoint(ray.GetPoint(enter));
                        float v = Mathf.Clamp01((local.z - zAtZero) / (zAtFull - zAtZero));
                        sim.Weights[(int)drag.Value] = Mathf.Round(v * 20f) * 5f;
                    }
                    if (TooltipView.Instance != null)
                        TooltipView.Instance.Show($"{ReactionStyle.CategoryLabel(drag.Value)}: {sim.Weights[(int)drag.Value]:0}%", Mouse.current.position.ReadValue());
                }
                else
                {
                    drag = null;
                    Dragging = false;
                }
            }

            for (int i = 0; i < knobs.Length; i++)
            {
                if (knobs[i] == null) continue;
                Vector3 p = knobs[i].localPosition;
                p.z = Mathf.Lerp(zAtZero, zAtFull, sim.Weights[i] / 100f);
                knobs[i].localPosition = p;
                if (valueLabels[i] != null) valueLabels[i].text = sim.Weights[i].ToString("0") + "%";
            }
        }
    }
}
