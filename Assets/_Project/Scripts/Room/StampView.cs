using System;
using System.Collections;
using UnityEngine;

namespace EchoChamber
{
    /// <summary>The two stamps (amplify, hide). Plays the lift, press and return animation over the slip on the stand.</summary>
    public class StampView : MonoBehaviour
    {
        public Transform amplify;
        public Transform hide;

        public bool Busy { get; private set; }

        private Vector3 ampHome;
        private Vector3 hideHome;

        private void Awake()
        {
            if (amplify != null) ampHome = amplify.position;
            if (hide != null) hideHome = hide.position;
        }

        public void Play(DecisionMode mode, Vector3 slipWorldPos, Action onHit, Action onDone)
        {
            if (Busy) return;
            StartCoroutine(Run(mode, slipWorldPos, onHit, onDone));
        }

        private static float Ease(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        private IEnumerator Run(DecisionMode mode, Vector3 slipPos, Action onHit, Action onDone)
        {
            Busy = true;
            Transform st = mode == DecisionMode.Amplify ? amplify : hide;
            Vector3 home = mode == DecisionMode.Amplify ? ampHome : hideHome;
            Vector3 target = slipPos + new Vector3(-0.1f, 0.12f, 0f);
            const float t1 = 0.22f, t2 = 0.32f, t3 = 0.6f;
            float t = 0f;
            bool hit = false;
            while (t < t3)
            {
                t += Time.deltaTime;
                if (t < t1) st.position = Vector3.Lerp(home, target + Vector3.up * 0.15f, Ease(t / t1));
                else if (t < t2) st.position = Vector3.Lerp(target + Vector3.up * 0.15f, target + Vector3.down * 0.07f, Ease((t - t1) / (t2 - t1)));
                else if (!hit)
                {
                    hit = true;
                    onHit?.Invoke();
                }
                if (t >= t2 && t < t3) st.position = Vector3.Lerp(target + Vector3.down * 0.07f, home, Ease((t - t2) / (t3 - t2)));
                yield return null;
            }
            st.position = home;
            Busy = false;
            onDone?.Invoke();
        }
    }
}
