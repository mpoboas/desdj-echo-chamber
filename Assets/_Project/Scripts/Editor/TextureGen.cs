using System.IO;
using UnityEditor;
using UnityEngine;

namespace EchoChamber.Editor
{
    /// <summary>Procedural textures and sprites for the room (wallpaper, wood, floor) and the CRT overlays.</summary>
    public static class TextureGen
    {
        public const string Dir = "Assets/_Project/Art/Generated";

        private static Color C(string hex) => SceneBuilderUtil.Hex(hex);

        private static Texture2D Save(Texture2D tex, string name, bool sprite = false, int ppu = 100, bool repeat = true, bool sRGB = true)
        {
            Directory.CreateDirectory(Dir);
            string path = $"{Dir}/{name}.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.sRGBTexture = sRGB;
            imp.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            imp.mipmapEnabled = !sprite;
            imp.filterMode = sprite ? FilterMode.Bilinear : FilterMode.Trilinear;
            imp.anisoLevel = 8;
            if (sprite)
            {
                imp.textureType = TextureImporterType.Sprite;
                imp.spritePixelsPerUnit = ppu;
                imp.alphaIsTransparency = true;
            }
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Texture2D Make(int w, int h, System.Func<int, int, Color> f)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var px = new Color[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++) px[y * w + x] = f(x, y);
            t.SetPixels(px);
            t.Apply();
            return t;
        }

        private static float Grain(int x, int y, float fx, float fy, float seed)
        {
            return Mathf.PerlinNoise(x * fx + seed, y * fy + seed * 1.7f);
        }

        private static Texture2D Wood(int w, int h, string baseHex, float dark, bool vertical, float seed, System.Action<Color[], int, int> post = null)
        {
            Color b = C(baseHex);
            var t = Make(w, h, (x, y) =>
            {
                float g = vertical ? Grain(x, y, 0.12f, 0.004f, seed) : Grain(x, y, 0.004f, 0.12f, seed);
                float g2 = vertical ? Grain(x, y, 0.5f, 0.02f, seed + 9) : Grain(x, y, 0.02f, 0.5f, seed + 9);
                float k = 1f - dark * (0.65f * g + 0.35f * g2);
                return new Color(b.r * k, b.g * k, b.b * k, 1f);
            });
            if (post != null)
            {
                var px = t.GetPixels();
                post(px, w, h);
                t.SetPixels(px);
                t.Apply();
            }
            return t;
        }

        public static void GenerateAll()
        {
            // wallpaper: vertical stripes on terracotta
            Save(Make(256, 256, (x, y) =>
            {
                Color c = C("#A65A3F");
                int m = x % 32;
                if (m < 14) c = Color.Lerp(c, new Color(1f, 0.86f, 0.7f), 0.07f);
                else if (m >= 15 && m < 17) c = Color.Lerp(c, new Color(0.24f, 0.08f, 0.04f), 0.08f);
                float n = Grain(x, y, 0.9f, 0.9f, 3f) * 0.12f;
                return new Color(c.r * (1f - n), c.g * (1f - n), c.b * (1f - n), 1f);
            }), "tex_wallpaper");

            Save(Wood(512, 256, "#5E3B28", 0.55f, true, 1f, (px, w, h) =>
            {
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x += 64)
                        for (int d = 0; d < 3; d++) px[y * w + Mathf.Min(w - 1, x + d)] = new Color(0.08f, 0.03f, 0.01f, 1f);
            }), "tex_wainscot");

            Save(Wood(512, 512, "#6B4631", 0.6f, false, 5f, (px, w, h) =>
            {
                for (int y = 0; y < h; y += 64)
                {
                    for (int x = 0; x < w; x++) for (int d = 0; d < 3; d++) px[Mathf.Min(h - 1, y + d) * w + x] = new Color(0.06f, 0.02f, 0.01f, 1f);
                    int off = (y / 64 % 2) * 180;
                    for (int x = off; x < w; x += 360)
                        for (int yy = y; yy < Mathf.Min(h, y + 64); yy++) px[yy * w + x] = new Color(0.06f, 0.02f, 0.01f, 1f);
                }
            }), "tex_floor");

            Save(Wood(1024, 512, "#A46B42", 0.5f, false, 11f), "tex_desk");
            Save(Wood(256, 256, "#5B3A27", 0.45f, true, 21f), "tex_deskdark");
            Save(Make(256, 256, (x, y) =>
            {
                Color c = C("#3E2C22");
                float n = Grain(x, y, 0.5f, 0.5f, 7f) * 0.25f;
                return new Color(c.r * (1f - n), c.g * (1f - n), c.b * (1f - n), 1f);
            }), "tex_ceiling");

            Save(Make(512, 320, (x, y) =>
            {
                Color c = C("#2F6E64");
                Color line = new Color(0.86f, 1f, 0.94f);
                if (x % 16 == 0 || y % 16 == 0) c = Color.Lerp(c, line, 0.18f);
                if (x % 80 == 0 || y % 80 == 0) c = Color.Lerp(c, line, 0.35f);
                if (x < 5 || y < 5 || x > 506 || y > 314) c = Color.Lerp(c, line, 0.5f);
                return c;
            }), "tex_mat");

            // cork
            Save(Make(256, 256, (x, y) =>
            {
                Color c = C("#B98B5E");
                float n = Grain(x, y, 0.35f, 0.35f, 2f) * 0.35f + Grain(x, y, 1.3f, 1.3f, 5f) * 0.2f;
                return new Color(c.r * (1f - n), c.g * (1f - n), c.b * (1f - n), 1f);
            }), "tex_cork");

            // paper (slip, newspaper)
            Save(Make(128, 128, (x, y) =>
            {
                float n = Grain(x, y, 1.1f, 1.1f, 4f) * 0.06f;
                return new Color(0.965f - n, 0.945f - n, 0.894f - n, 1f);
            }), "tex_paper");

            // CRT overlays
            Save(Make(2, 3, (x, y) => y == 0 ? new Color(0f, 0f, 0f, 0.16f) : new Color(0f, 0f, 0f, 0f)), "spr_scanlines", true, 1);
            Save(Make(128, 128, (x, y) =>
            {
                float dx = (x - 63.5f) / 64f, dy = (y - 63.5f) / 64f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01((d - 0.42f) / 0.62f);
                return new Color(0f, 0f, 0f, a * a * 0.6f);
            }), "spr_vignette", true, 100, false);

            // shapes (white, antialiased)
            Save(Shape(new[] { new Vector2(0, 1.25f), new Vector2(1.25f, 0), new Vector2(0, -1.25f), new Vector2(-1.25f, 0) }), "spr_diamond", true, 128, false);
            Save(Shape(new[] { new Vector2(0, 1.15f), new Vector2(1.1f, -0.8f), new Vector2(-1.1f, -0.8f) }), "spr_triangle", true, 128, false);
            Save(Shape(new[] { new Vector2(-1f, -0.8f), new Vector2(-1f, 0.2f), new Vector2(0, 1.1f), new Vector2(1f, 0.2f), new Vector2(1f, -0.8f) }), "spr_house", true, 128, false);
            var star = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float a = Mathf.PI / 2f + i * Mathf.PI / 5f;
                float r = i % 2 == 1 ? 0.55f : 1.25f;
                star[i] = new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r);
            }
            Save(Shape(star), "spr_star", true, 128, false);
            Save(Make(128, 128, (x, y) =>
            {
                float dx = (x - 63.5f) / 64f, dy = (y - 63.5f) / 64f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01((1f - d) * 64f) * Mathf.Clamp01((d - 0.74f) * 64f);
                return new Color(1f, 1f, 1f, a);
            }), "spr_ring", true, 128, false);
            Save(Make(128, 128, (x, y) =>
            {
                float dx = (x - 63.5f) / 64f, dy = (y - 63.5f) / 64f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                return new Color(1f, 1f, 1f, Mathf.Clamp01((1f - d) * 64f));
            }), "spr_disc", true, 128, false);
            AssetDatabase.SaveAssets();
        }

        private static Texture2D Shape(Vector2[] poly)
        {
            const int n = 128;
            return Make(n, n, (px, py) =>
            {
                int hit = 0;
                for (int sy = 0; sy < 2; sy++)
                    for (int sx = 0; sx < 2; sx++)
                    {
                        float u = ((px + 0.25f + sx * 0.5f) / n * 2f - 1f) * 1.3f;
                        float v = ((py + 0.25f + sy * 0.5f) / n * 2f - 1f) * 1.3f;
                        if (Inside(poly, new Vector2(u, v))) hit++;
                    }
                return new Color(1f, 1f, 1f, hit / 4f);
            });
        }

        private static bool Inside(Vector2[] p, Vector2 pt)
        {
            bool inside = false;
            for (int i = 0, j = p.Length - 1; i < p.Length; j = i++)
                if (((p[i].y > pt.y) != (p[j].y > pt.y)) && (pt.x < (p[j].x - p[i].x) * (pt.y - p[i].y) / (p[j].y - p[i].y) + p[i].x))
                    inside = !inside;
            return inside;
        }

        public static Texture2D Tex(string name) => AssetDatabase.LoadAssetAtPath<Texture2D>($"{Dir}/{name}.png");

        public static Sprite Spr(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{Dir}/{name}.png");

        public static Material TexMat(string texName, Vector2 tiling, float rough = 0.8f, string tint = "#FFFFFF")
        {
            string path = $"{SceneBuilderUtil.MaterialDir}/tex_{texName}_{tiling.x}_{tiling.y}_{(int)(rough * 100)}_{tint.TrimStart('#')}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                Directory.CreateDirectory(SceneBuilderUtil.MaterialDir);
                m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(m, path);
            }
            m.SetTexture("_BaseMap", Tex(texName));
            m.SetTextureScale("_BaseMap", tiling);
            m.SetColor("_BaseColor", SceneBuilderUtil.Hex(tint));
            m.SetFloat("_Smoothness", 1f - rough);
            m.SetFloat("_Metallic", 0f);
            EditorUtility.SetDirty(m);
            return m;
        }
    }
}
