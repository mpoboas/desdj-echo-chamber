using UnityEngine;

namespace EchoChamber
{
    /// <summary>Pneumatic tube: a capsule shoots up when a slip enters.</summary>
    public class TubeView : MonoBehaviour
    {
        public Transform capsule;
        public float bottomY = 1.08f;
        public float rise = 1.35f;

        private float anim;

        public void Ping()
        {
            anim = 1f;
        }

        private void Update()
        {
            if (capsule == null) return;
            if (anim > 0f) anim = Mathf.Max(0f, anim - Time.deltaTime * 1.4f);
            capsule.gameObject.SetActive(anim > 0f);
            Vector3 p = capsule.position;
            p.y = bottomY + (1f - anim) * rise;
            capsule.position = p;
        }
    }
}
