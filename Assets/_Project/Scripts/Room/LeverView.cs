using UnityEngine;

namespace EchoChamber
{
    /// <summary>The publish lever: swings forward when pulled.</summary>
    public class LeverView : MonoBehaviour
    {
        public Transform arm;
        public float restAngle = -0.5f;

        private float anim;

        public void Pull()
        {
            anim = 1f;
        }

        private void Update()
        {
            if (arm == null) return;
            if (anim > 0f) anim = Mathf.Max(0f, anim - Time.deltaTime * 1.4f);
            float a = restAngle + Mathf.Sin((1f - anim) * Mathf.PI) * 1.0f;
            arm.localRotation = Quaternion.Euler(a * Mathf.Rad2Deg, 0f, 0f);
        }
    }
}
