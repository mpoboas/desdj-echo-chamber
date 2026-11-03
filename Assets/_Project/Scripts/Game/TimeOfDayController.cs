using System.Collections.Generic;
using UnityEngine;

namespace EchoChamber
{
    /// <summary>Lighting by turn: morning clear, afternoon warm, night with street lamps on.</summary>
    public class TimeOfDayController : MonoBehaviour
    {
        [System.Serializable]
        public class Preset
        {
            public Color sky = Color.white;
            public Color sun = Color.white;
            public float sunIntensity = 1.2f;
            public Color ambient = Color.gray;
            public float windowIntensity = 0.7f;
            public Color windowColor = Color.white;
            public bool lampsOn;
            public float pendantIntensity = 0.45f;
            public float lampIntensity = 1.1f;
            public float shaftOpacity = 0.06f;
            public Color shaftColor = Color.white;
        }

        public Camera targetCamera;
        public Light sun;
        public Light windowLight;
        public Light pendantLight;
        public Light deskLamp;
        public List<Light> streetLights = new List<Light>();
        public List<Renderer> windowGlass = new List<Renderer>();
        public List<Renderer> lampHeads = new List<Renderer>();
        public List<Renderer> lightShafts = new List<Renderer>();
        public GameObject dust;
        public float blendSpeed = 2.5f;
        public List<Preset> presets = new List<Preset>();

        public float streetLightIntensity = 1.6f;

        private Preset current;
        private Color sky;
        private Color sunColor;
        private float sunI;
        private Color ambient;
        private float winI;
        private Color winC;
        private float pendI, lampI, lampsBlend;
        private int lastTurn = -1;
        private MaterialPropertyBlock block;

        public static TimeOfDayController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (presets.Count > 0) Snap(presets[0]);
        }

        private void Snap(Preset p)
        {
            current = p;
            sky = p.sky; sunColor = p.sun; sunI = p.sunIntensity; ambient = p.ambient;
            winI = p.windowIntensity; winC = p.windowColor; pendI = p.pendantIntensity; lampI = p.lampIntensity;
            lampsBlend = p.lampsOn ? 1f : 0f;
            Apply();
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Sim == null || presets.Count == 0) return;
            int turn = Mathf.Clamp(gm.Sim.Turn, 0, presets.Count - 1);
            var p = presets[turn];
            if (lastTurn != turn)
            {
                lastTurn = turn;
                current = p;
            }
            float k = 1f - Mathf.Exp(-blendSpeed * Time.deltaTime);
            sky = Color.Lerp(sky, p.sky, k);
            sunColor = Color.Lerp(sunColor, p.sun, k);
            sunI = Mathf.Lerp(sunI, p.sunIntensity, k);
            ambient = Color.Lerp(ambient, p.ambient, k);
            winI = Mathf.Lerp(winI, p.windowIntensity, k);
            winC = Color.Lerp(winC, p.windowColor, k);
            pendI = Mathf.Lerp(pendI, p.pendantIntensity, k);
            lampI = Mathf.Lerp(lampI, p.lampIntensity, k);
            lampsBlend = Mathf.Lerp(lampsBlend, p.lampsOn ? 1f : 0f, k);
            Apply();
        }

        private void Apply()
        {
            if (targetCamera != null) targetCamera.backgroundColor = sky;
            RenderSettings.fogColor = sky;
            RenderSettings.ambientLight = ambient;
            if (sun != null) { sun.color = sunColor; sun.intensity = sunI; }
            if (windowLight != null) { windowLight.intensity = winI; windowLight.color = winC; }
            if (pendantLight != null) pendantLight.intensity = pendI;
            if (deskLamp != null) deskLamp.intensity = lampI;
            foreach (var l in streetLights) if (l != null) l.intensity = streetLightIntensity * lampsBlend;

            if (block == null) block = new MaterialPropertyBlock();
            foreach (var r in windowGlass)
            {
                if (r == null) continue;
                r.GetPropertyBlock(block);
                block.SetColor("_EmissionColor", new Color(1f, 0.82f, 0.45f) * (0.9f * lampsBlend));
                r.SetPropertyBlock(block);
            }
            foreach (var r in lampHeads)
            {
                if (r == null) continue;
                r.GetPropertyBlock(block);
                Color c = Color.Lerp(new Color(0.33f, 0.27f, 0.2f), new Color(1f, 0.82f, 0.48f), lampsBlend);
                block.SetColor("_BaseColor", c);
                block.SetColor("_Color", c);
                r.SetPropertyBlock(block);
            }
            if (current != null)
            {
                foreach (var r in lightShafts)
                {
                    if (r == null) continue;
                    r.gameObject.SetActive(lampsBlend < 0.5f);
                    r.GetPropertyBlock(block);
                    Color c = current.shaftColor;
                    c.a = current.shaftOpacity;
                    block.SetColor("_BaseColor", c);
                    block.SetColor("_Color", c);
                    r.SetPropertyBlock(block);
                }
            }
            if (dust != null) dust.SetActive(lampsBlend < 0.5f);
        }
    }
}
