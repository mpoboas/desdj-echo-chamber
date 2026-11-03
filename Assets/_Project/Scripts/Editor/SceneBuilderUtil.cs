using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EchoChamber.Editor
{
    /// <summary>Helpers shared by the scene builders. Positions are given in prototype (three.js) coordinates and mirrored on X.</summary>
    public static class SceneBuilderUtil
    {
        public const string MaterialDir = "Assets/_Project/Art/Materials";
        public const string ModelDir = "Assets/_Project/Art/Models";
        public const string RoomDir = "Assets/_Project/Art/Room";

        private static readonly Dictionary<string, Material> matCache = new Dictionary<string, Material>();

        public static Vector3 P(float x, float y, float z) => new Vector3(-x, y, z);

        public static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;
        }

        public static void ResetCache() => matCache.Clear();

        /// <summary>URP Lit material saved as an asset (re-used by name).</summary>
        public static Material Mat(string hex, float rough = 0.85f, float metal = 0f, string emissiveHex = null, float emissive = 1f, float alpha = 1f)
        {
            string key = $"lit_{hex.TrimStart('#')}_{(int)(rough * 100)}_{(int)(metal * 100)}_{emissiveHex}_{(int)(emissive * 100)}_{(int)(alpha * 100)}";
            if (matCache.TryGetValue(key, out var cached) && cached != null) return cached;
            Directory.CreateDirectory(MaterialDir);
            string path = $"{MaterialDir}/{key}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(m, path);
            }
            Color c = Hex(hex);
            c.a = alpha;
            m.SetColor("_BaseColor", c);
            m.SetFloat("_Smoothness", 1f - rough);
            m.SetFloat("_Metallic", metal);
            if (alpha < 1f)
            {
                m.SetFloat("_Surface", 1f);
                m.SetFloat("_Blend", 0f);
                m.SetOverrideTag("RenderType", "Transparent");
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            if (emissiveHex != null)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", Hex(emissiveHex) * emissive);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }
            EditorUtility.SetDirty(m);
            matCache[key] = m;
            return m;
        }

        public static Material Unlit(string hex, float alpha = 1f)
        {
            string key = $"unlit_{hex.TrimStart('#')}_{(int)(alpha * 100)}";
            if (matCache.TryGetValue(key, out var cached) && cached != null) return cached;
            Directory.CreateDirectory(MaterialDir);
            string path = $"{MaterialDir}/{key}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                AssetDatabase.CreateAsset(m, path);
            }
            Color c = Hex(hex);
            c.a = alpha;
            m.SetColor("_BaseColor", c);
            if (alpha < 1f)
            {
                m.SetFloat("_Surface", 1f);
                m.SetOverrideTag("RenderType", "Transparent");
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            EditorUtility.SetDirty(m);
            matCache[key] = m;
            return m;
        }

        private static void Strip(GameObject go, bool keepCollider)
        {
            if (keepCollider) return;
            var c = go.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);
        }

        public static GameObject Box(string name, Vector3 size, Material m, Vector3 localPos, Transform parent, bool shadow = true, bool collider = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = m;
            go.GetComponent<Renderer>().shadowCastingMode = shadow ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            Strip(go, collider);
            return go;
        }

        public static GameObject Cyl(string name, float radiusTop, float radiusBottom, float height, Material m, Vector3 localPos, Transform parent, bool shadow = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            float r = (radiusTop + radiusBottom);
            go.transform.localScale = new Vector3(r, height / 2f, r);
            go.GetComponent<Renderer>().sharedMaterial = m;
            go.GetComponent<Renderer>().shadowCastingMode = shadow ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            Strip(go, false);
            return go;
        }

        public static GameObject Sphere(string name, float radius, Material m, Vector3 localPos, Transform parent, bool shadow = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one * radius * 2f;
            go.GetComponent<Renderer>().sharedMaterial = m;
            go.GetComponent<Renderer>().shadowCastingMode = shadow ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            Strip(go, false);
            return go;
        }

        /// <summary>Quad that faces +Z (towards the player camera).</summary>
        public static GameObject Quad(string name, Vector2 size, Material m, Vector3 localPos, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            go.GetComponent<Renderer>().sharedMaterial = m;
            go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Strip(go, false);
            return go;
        }

        public static Font DefaultFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        /// <summary>Flat sign with text (TextMesh) facing +Z.</summary>
        public static GameObject Label(string name, string text, Vector2 size, string bgHex, string fgHex, Vector3 localPos, Transform parent, float textScale = 0.5f)
        {
            var bg = Quad(name, size, Unlit(bgHex), localPos, parent);
            var t = new GameObject("Text");
            t.transform.SetParent(bg.transform, false);
            t.transform.localPosition = new Vector3(0f, 0f, 0.01f);
            // Quad is scaled by size; counter-scale so the text keeps its proportions.
            t.transform.localScale = new Vector3(1f / size.x, 1f / size.y, 1f);
            var tm = t.AddComponent<TextMesh>();
            tm.text = text;
            tm.font = DefaultFont();
            tm.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Hex(fgHex);
            tm.fontSize = 64;
            // 64pt LegacyRuntime: about 0.041 wide per char and 0.0715 tall at characterSize 0.01
            tm.characterSize = 0.01f * Mathf.Min(0.9f * size.x / (0.041f * Mathf.Max(1, text.Length)), textScale * size.y / 0.0715f);
            return bg;
        }

        public class InstOptions
        {
            public float scale;          // uniform scale; 0 = use height/length
            public float height;
            public float length;
            public float scaleX = 1f;
            public float scaleZ = 1f;
            public bool center = true;
            public float rotY;           // prototype radians
            public string tint;
            public bool shadow = true;
            public Vector3 pos;          // prototype coordinates
        }

        private static readonly Dictionary<string, GameObject> modelCache = new Dictionary<string, GameObject>();

        public static GameObject LoadModel(string name)
        {
            if (modelCache.TryGetValue(name, out var m) && m != null) return m;
            string[] dirs = { ModelDir, RoomDir };
            foreach (var d in dirs)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>($"{d}/{name}.glb");
                if (go != null)
                {
                    modelCache[name] = go;
                    return go;
                }
            }
            Debug.LogWarning("[EchoChamber] Missing model " + name);
            return null;
        }

        private static Bounds RenderBounds(GameObject go)
        {
            bool first = true;
            Bounds b = new Bounds();
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                if (r is SkinnedMeshRenderer smr)
                {
                    // bounds of skinned meshes are unreliable; use localBounds transformed
                    var lb = smr.localBounds;
                    var wb = new Bounds(smr.transform.TransformPoint(lb.center), Vector3.zero);
                    foreach (var corner in new[]
                    {
                        new Vector3(-1,-1,-1), new Vector3(1,-1,-1), new Vector3(-1,1,-1), new Vector3(1,1,-1),
                        new Vector3(-1,-1,1), new Vector3(1,-1,1), new Vector3(-1,1,1), new Vector3(1,1,1)
                    })
                        wb.Encapsulate(smr.transform.TransformPoint(lb.center + Vector3.Scale(lb.extents, corner)));
                    if (first) { b = wb; first = false; } else b.Encapsulate(wb);
                }
                else
                {
                    if (first) { b = r.bounds; first = false; } else b.Encapsulate(r.bounds);
                }
            }
            return b;
        }

        /// <summary>
        /// Instantiates a GLB normalised like the prototype: scaled, centred on X/Z (optional), sitting on y=0,
        /// then placed at o.pos (prototype coordinates) with rotation o.rotY.
        /// </summary>
        public static GameObject Inst(string model, Transform parent, InstOptions o)
        {
            var src = LoadModel(model);
            if (src == null) return null;
            var wrapper = new GameObject("inst_" + model);
            var m = (GameObject)PrefabUtility.InstantiatePrefab(src, wrapper.transform);
            m.transform.localPosition = Vector3.zero;
            m.transform.localRotation = Quaternion.identity;
            m.transform.localScale = Vector3.one;

            if (o.scale > 0f) m.transform.localScale = Vector3.one * o.scale;
            else if (o.height > 0f || o.length > 0f)
            {
                Bounds sb = RenderBounds(wrapper);
                float s = o.height > 0f ? o.height / sb.size.y : o.length / Mathf.Max(sb.size.x, sb.size.z);
                m.transform.localScale = Vector3.one * s;
            }
            var ls = m.transform.localScale;
            ls.x *= o.scaleX;
            ls.z *= o.scaleZ;
            m.transform.localScale = ls;

            Bounds b = RenderBounds(wrapper);
            m.transform.localPosition = new Vector3(o.center ? -b.center.x : 0f, -b.min.y, o.center ? -b.center.z : 0f);

            foreach (var r in wrapper.GetComponentsInChildren<Renderer>())
            {
                r.shadowCastingMode = o.shadow ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = true;
            }
            if (!string.IsNullOrEmpty(o.tint)) Tint(wrapper, Hex(o.tint));

            wrapper.transform.SetParent(parent, false);
            wrapper.transform.localPosition = P(o.pos.x, o.pos.y, o.pos.z);
            wrapper.transform.localRotation = Quaternion.Euler(0f, -o.rotY * Mathf.Rad2Deg, 0f);
            return wrapper;
        }

        public static void Tint(GameObject go, Color tint)
        {
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                var block = new MaterialPropertyBlock();
                r.GetPropertyBlock(block);
                block.SetColor("baseColorFactor", tint);
                r.SetPropertyBlock(block);
            }
        }

        /// <summary>Adds a tiny circle sprite asset used for reaction icons and shadows.</summary>
        public static Sprite CircleSprite()
        {
            const string path = "Assets/_Project/Art/Generated/circle.png";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            const int n = 128;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = x - n / 2f + 0.5f, dy = y - n / 2f + 0.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01((n / 2f - 2f - d) / 2f);
                    float ring = d > n / 2f - 12f ? 1f : 0f;
                    Color c = ring > 0f ? Color.white : new Color(0.97f, 0.97f, 0.97f);
                    c.a = a;
                    tex.SetPixel(x, y, c);
                }
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = n;
            imp.alphaIsTransparency = true;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
