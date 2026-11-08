using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace EchoChamber
{
    /// <summary>The 2D clips that play behind each post, found by FeedPost.clip.</summary>
    [CreateAssetMenu(menuName = "Echo Chamber/Feed Clip Catalog", fileName = "FeedClipCatalog")]
    public class FeedClipCatalog : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string name;
            public VideoClip clip;
        }

        [Serializable]
        public class OverlayEntry
        {
            public int postId;
            public Texture2D texture;
        }

        public List<Entry> entries = new List<Entry>();
        public List<OverlayEntry> overlays = new List<OverlayEntry>();

        /// <summary>Caption drawn over the clip (rendered from the HTML prototype, heavy font with outline).</summary>
        public Texture2D Overlay(int postId)
        {
            foreach (var o in overlays)
                if (o.postId == postId) return o.texture;
            return null;
        }

        public VideoClip Find(string name)
        {
            foreach (var e in entries)
                if (e.name == name) return e.clip;
            return entries.Count > 0 ? entries[0].clip : null;
        }
    }
}
