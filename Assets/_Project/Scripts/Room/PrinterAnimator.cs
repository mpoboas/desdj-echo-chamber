using UnityEngine;

namespace EchoChamber
{
    /// <summary>Printer carriage and platen knob motion while a slip is being printed.</summary>
    public class PrinterAnimator : MonoBehaviour
    {
        public Transform carriage;
        public Transform platenKnob;

        public bool Printing { get; set; }
        private float t;
        private Vector3 carriageHome;

        private void Awake()
        {
            if (carriage != null) carriageHome = carriage.localPosition;
        }

        private void Update()
        {
            if (carriage == null) return;
            if (Printing)
            {
                t += Time.deltaTime;
                Vector3 p = carriageHome;
                p.x += Mathf.Sin(t * 18f) * 0.2f;
                carriage.localPosition = p;
                if (platenKnob != null) platenKnob.Rotate(Vector3.right, Time.deltaTime * 7f * Mathf.Rad2Deg, Space.Self);
            }
            else
            {
                carriage.localPosition = carriageHome;
            }
        }
    }
}
