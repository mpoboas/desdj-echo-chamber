using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace EchoChamber.Editor
{
    /// <summary>Collects Assets/_Project/Art/Videos/feed_*.mp4 into the FeedClipCatalog asset.</summary>
    public static class FeedClipImporter
    {
        public const string Dir = "Assets/_Project/Art/Videos";
        public const string OverlayDir = "Assets/_Project/Art/Overlays";
        public const string AssetPath = "Assets/_Project/Data/FeedClipCatalog.asset";

        [MenuItem("Echo Chamber/Import Feed Clips")]
        public static void ImportAll()
        {
            AssetDatabase.Refresh();
            var cat = AssetDatabase.LoadAssetAtPath<FeedClipCatalog>(AssetPath);
            if (cat == null)
            {
                cat = ScriptableObject.CreateInstance<FeedClipCatalog>();
                AssetDatabase.CreateAsset(cat, AssetPath);
            }
            cat.entries.Clear();
            foreach (var path in Directory.GetFiles(Dir, "feed_*.mp4"))
            {
                string p = path.Replace('\\', '/');
                var clip = AssetDatabase.LoadAssetAtPath<VideoClip>(p);
                if (clip == null) { Debug.LogWarning("[EchoChamber] Not a VideoClip: " + p); continue; }
                cat.entries.Add(new FeedClipCatalog.Entry { name = Path.GetFileNameWithoutExtension(p).Substring(5), clip = clip });
            }
            cat.overlays.Clear();
            foreach (var path in Directory.GetFiles(OverlayDir, "overlay_*.png"))
            {
                string p = path.Replace('\\', '/');
                var imp = AssetImporter.GetAtPath(p) as TextureImporter;
                if (imp != null && (imp.mipmapEnabled || !imp.alphaIsTransparency || imp.maxTextureSize < 1024))
                {
                    imp.mipmapEnabled = false;
                    imp.alphaIsTransparency = true;
                    imp.maxTextureSize = 1024;
                    imp.textureCompression = TextureImporterCompression.Uncompressed;
                    imp.SaveAndReimport();
                }
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                string name = Path.GetFileNameWithoutExtension(p).Substring(8);
                if (tex != null && int.TryParse(name, out int id)) cat.overlays.Add(new FeedClipCatalog.OverlayEntry { postId = id, texture = tex });
            }
            EditorUtility.SetDirty(cat);
            AssetDatabase.SaveAssets();
            Debug.Log("[EchoChamber] Feed clip catalog: " + cat.entries.Count + " clips, " + cat.overlays.Count + " overlays.");
        }
    }
}
