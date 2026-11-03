using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoChamber
{
    /// <summary>
    /// All game audio, synthesised in code (the Unity AI audio generator is not available in this account).
    /// One-shots mirror the prototype WebAudio effects; ambient loops react to the neighbourhood state
    /// (street rumble louder during the simulation, tension drone, warm pad with cohesion, crickets at night).
    /// </summary>
    public class ProceduralAudio : MonoBehaviour
    {
        public const int Rate = 22050;
        public float masterGain = 3.2f;

        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private readonly List<AudioSource> pool = new List<AudioSource>();
        private AudioSource street, drone, pad, crickets;
        private System.Random rng = new System.Random(77);

        public static ProceduralAudio Instance { get; private set; }

        private enum Wave
        {
            Sine,
            Square,
            Triangle,
            Saw
        }

        private void Awake()
        {
            Instance = this;
            BuildClips();
            for (int i = 0; i < 10; i++)
            {
                var s = gameObject.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.spatialBlend = 0f;
                pool.Add(s);
            }
            street = Loop("Street", BuildStreetLoop());
            drone = Loop("Drone", BuildDroneLoop());
            pad = Loop("Pad", BuildPadLoop());
            crickets = Loop("Crickets", BuildCricketLoop());
            GameAudio.Hook = PlayOneShot;
        }

        private void OnDestroy()
        {
            if (GameAudio.Hook == (Action<string>)PlayOneShot) GameAudio.Hook = null;
        }

        private AudioSource Loop(string name, AudioClip clip)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var s = go.AddComponent<AudioSource>();
            s.clip = clip;
            s.loop = true;
            s.volume = 0f;
            s.spatialBlend = 0f;
            s.Play();
            return s;
        }

        public void PlayOneShot(string id)
        {
            if (!clips.TryGetValue(id, out var clip)) return;
            foreach (var s in pool)
            {
                if (s.isPlaying) continue;
                s.clip = clip;
                s.volume = 1f;
                s.Play();
                return;
            }
            pool[0].PlayOneShot(clip);
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Sim == null) return;
            var sim = gm.Sim;
            float k = 1f - Mathf.Exp(-2f * Time.deltaTime);
            float simulating = sim.Phase == GamePhase.Simulate ? 1f : 0.35f;
            float tension = sim.Metrics.Tension;
            float cohesion = sim.Metrics.Cohesion;
            bool night = sim.IsNightTurn;
            street.volume = Mathf.Lerp(street.volume, 0.16f * simulating * (night ? 0.7f : 1f), k);
            drone.volume = Mathf.Lerp(drone.volume, Mathf.Clamp01(tension * tension) * 0.5f, k);
            pad.volume = Mathf.Lerp(pad.volume, Mathf.Clamp01((cohesion - 0.5f) * 2f) * 0.22f, k);
            crickets.volume = Mathf.Lerp(crickets.volume, night ? 0.05f : 0f, k);
        }

        // -------------------------------------------------------------- synthesis

        private class Mix
        {
            public readonly List<float> data = new List<float>();

            public void Add(float[] samples, float startSeconds)
            {
                int start = Mathf.RoundToInt(startSeconds * Rate);
                while (data.Count < start + samples.Length) data.Add(0f);
                for (int i = 0; i < samples.Length; i++) data[start + i] += samples[i];
            }
        }

        private static float WaveSample(Wave w, float phase)
        {
            float p = phase - Mathf.Floor(phase);
            switch (w)
            {
                case Wave.Square: return p < 0.5f ? 1f : -1f;
                case Wave.Triangle: return 4f * Mathf.Abs(p - 0.5f) - 1f;
                case Wave.Saw: return 2f * p - 1f;
                default: return Mathf.Sin(p * Mathf.PI * 2f);
            }
        }

        private static float[] Tone(float freq, float dur, Wave wave, float vol, float slide = 0f)
        {
            int n = Mathf.Max(1, Mathf.RoundToInt((dur + 0.02f) * Rate));
            var s = new float[n];
            double phase = 0;
            float f1 = Mathf.Max(30f, freq + slide);
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / Rate;
                float u = Mathf.Clamp01(t / dur);
                float f = slide != 0f ? freq * Mathf.Pow(f1 / freq, u) : freq;
                phase += f / Rate;
                float env = vol * Mathf.Pow(0.0001f / vol, u);
                if (t > dur) env = 0f;
                s[i] = WaveSample(wave, (float)phase) * env;
            }
            return s;
        }

        private float[] Noise(float dur, float vol, float freq)
        {
            int n = Mathf.Max(1, Mathf.RoundToInt(dur * Rate));
            var s = new float[n];
            // RBJ band-pass, Q = 1
            float w0 = 2f * Mathf.PI * freq / Rate;
            float alpha = Mathf.Sin(w0) / 2f;
            float b0 = alpha, b2 = -alpha, a0 = 1f + alpha, a1 = -2f * Mathf.Cos(w0), a2 = 1f - alpha;
            float x1 = 0, x2 = 0, y1 = 0, y2 = 0;
            for (int i = 0; i < n; i++)
            {
                float x = ((float)rng.NextDouble() * 2f - 1f) * (1f - (float)i / n);
                float y = (b0 * x + b2 * x2 - a1 * y1 - a2 * y2) / a0;
                x2 = x1; x1 = x; y2 = y1; y1 = y;
                s[i] = y * vol * 3f;
            }
            return s;
        }

        private AudioClip Make(string name, Mix mix)
        {
            var arr = mix.data.ToArray();
            for (int i = 0; i < arr.Length; i++) arr[i] = Mathf.Clamp(arr[i] * masterGain, -1f, 1f);
            var clip = AudioClip.Create(name, arr.Length, 1, Rate, false);
            clip.SetData(arr, 0);
            return clip;
        }

        private void BuildClips()
        {
            var m = new Mix();
            for (int i = 0; i < 4; i++)
            {
                m.Add(Noise(0.03f, 0.07f, 3200f + (float)rng.NextDouble() * 800f), i * 0.022f);
                m.Add(Tone(140f, 0.02f, Wave.Square, 0.015f), i * 0.022f);
            }
            clips["dots"] = Make("dots", m);

            m = new Mix();
            m.Add(Noise(0.14f, 0.09f, 2200f), 0f);
            m.Add(Noise(0.08f, 0.06f, 1400f), 0.06f);
            clips["tear"] = Make("tear", m);

            m = new Mix();
            m.Add(Tone(90f, 0.18f, Wave.Sine, 0.18f, -40f), 0f);
            m.Add(Noise(0.08f, 0.09f, 600f), 0f);
            clips["stamp"] = Make("stamp", m);

            m = new Mix();
            m.Add(Noise(0.5f, 0.06f, 900f), 0f);
            clips["tube"] = Make("tube", m);

            m = new Mix();
            m.Add(Tone(160f, 0.12f, Wave.Square, 0.05f), 0f);
            m.Add(Tone(110f, 0.2f, Wave.Square, 0.05f), 0.11f);
            clips["lever"] = Make("lever", m);

            m = new Mix();
            for (int i = 0; i < 3; i++)
            {
                m.Add(Tone(1250f, 0.08f, Wave.Square, 0.03f), i * 0.14f);
                m.Add(Tone(1000f, 0.08f, Wave.Square, 0.03f), i * 0.14f);
            }
            clips["ring"] = Make("ring", m);

            m = new Mix();
            m.Add(Tone(880f, 0.15f, Wave.Saw, 0.04f), 0f);
            m.Add(Tone(660f, 0.2f, Wave.Saw, 0.04f), 0.17f);
            clips["alarm"] = Make("alarm", m);

            m = new Mix();
            m.Add(Tone(523f, 0.12f, Wave.Triangle, 0.05f), 0f);
            m.Add(Tone(784f, 0.2f, Wave.Triangle, 0.05f), 0.12f);
            clips["good"] = Make("good", m);

            m = new Mix();
            m.Add(Tone(1800f, 0.03f, Wave.Square, 0.02f), 0f);
            clips["tick"] = Make("tick", m);

            m = new Mix();
            for (int i = 0; i < 6; i++) m.Add(Noise(0.04f, 0.05f, 3800f + (float)rng.NextDouble() * 1200f), i * 0.045f);
            clips["note"] = Make("note", m);

            m = new Mix();
            m.Add(Tone(700f, 0.06f, Wave.Sine, 0.03f, 300f), 0f);
            clips["deliver"] = Make("deliver", m);

            m = new Mix();
            m.Add(Tone(400f, 0.06f, Wave.Sine, 0.06f), 0f);
            clips["click"] = Make("click", m);
        }

        private static float[] CrossfadeLoop(float[] a, int fade)
        {
            // make the buffer loop seamlessly by blending its tail into its head
            int n = a.Length - fade;
            var o = new float[n];
            Array.Copy(a, 0, o, 0, n);
            for (int i = 0; i < fade; i++)
            {
                float t = (float)i / fade;
                o[i] = a[i] * t + a[n + i] * (1f - t);
            }
            return o;
        }

        private AudioClip BuildStreetLoop()
        {
            const int seconds = 6;
            int n = Rate * seconds + Rate / 2;
            var a = new float[n];
            float lp = 0f, lp2 = 0f;
            for (int i = 0; i < n; i++)
            {
                float x = (float)rng.NextDouble() * 2f - 1f;
                lp += (x - lp) * 0.04f;
                lp2 += (lp - lp2) * 0.08f;
                float swell = 0.65f + 0.35f * Mathf.Sin(i / (float)Rate * 0.9f);
                a[i] = lp2 * 5f * swell;
            }
            var o = CrossfadeLoop(a, Rate / 2);
            var clip = AudioClip.Create("street", o.Length, 1, Rate, false);
            clip.SetData(o, 0);
            return clip;
        }

        private AudioClip BuildDroneLoop()
        {
            int n = Rate * 4;
            var o = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / Rate;
                // frequencies chosen so each completes whole cycles in 4 s: 55 Hz and 58 Hz
                float v = Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.5f + Mathf.Sin(2f * Mathf.PI * 58f * t) * 0.5f;
                v += 0.25f * Mathf.Sin(2f * Mathf.PI * 110.5f * t);
                float trem = 0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * 0.5f * t);
                o[i] = v * 0.4f * trem;
            }
            var clip = AudioClip.Create("drone", o.Length, 1, Rate, false);
            clip.SetData(o, 0);
            return clip;
        }

        private AudioClip BuildPadLoop()
        {
            int n = Rate * 8;
            var o = new float[n];
            float[] freqs = { 220f, 277.5f, 330f, 440f };
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / Rate;
                float v = 0f;
                for (int k = 0; k < freqs.Length; k++)
                {
                    float f = Mathf.Round(freqs[k] * 8f) / 8f; // whole cycles over 8 s
                    v += Mathf.Sin(2f * Mathf.PI * f * t) * (k == 3 ? 0.15f : 0.3f);
                }
                float swell = 0.75f + 0.25f * Mathf.Sin(2f * Mathf.PI * t / 8f);
                o[i] = v * 0.35f * swell;
            }
            var clip = AudioClip.Create("pad", o.Length, 1, Rate, false);
            clip.SetData(o, 0);
            return clip;
        }

        private AudioClip BuildCricketLoop()
        {
            int n = Rate * 4;
            var o = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / Rate;
                float gate = Mathf.Sin(2f * Mathf.PI * 8f * t) > 0.4f ? 1f : 0f;
                float chirp = Mathf.Sin(2f * Mathf.PI * 4200f * t) * gate;
                float slow = Mathf.Sin(2f * Mathf.PI * 0.75f * t) > 0f ? 1f : 0.2f;
                o[i] = chirp * slow * 0.5f;
            }
            var clip = AudioClip.Create("crickets", o.Length, 1, Rate, false);
            clip.SetData(o, 0);
            return clip;
        }
    }
}
