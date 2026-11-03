using UnityEngine;

namespace EchoChamber
{
    /// <summary>Rotary phone handset that rattles while ringing.</summary>
    public class PhoneView : MonoBehaviour
    {
        public Transform handset;

        public static PhoneView Instance { get; private set; }

        private float ring;
        private Vector3 home;

        private void Awake()
        {
            Instance = this;
            if (handset != null) home = handset.localPosition;
        }

        public void Ring(float seconds)
        {
            ring = seconds;
        }

        private void Update()
        {
            if (handset == null) return;
            if (ring > 0f)
            {
                ring -= Time.deltaTime;
                Vector3 p = home;
                p.y += Mathf.Abs(Mathf.Sin(Time.time * 33f)) * 0.02f;
                handset.localPosition = p;
                handset.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 40f) * 0.08f * Mathf.Rad2Deg);
            }
            else
            {
                handset.localPosition = home;
                handset.localRotation = Quaternion.identity;
            }
        }
    }
}
