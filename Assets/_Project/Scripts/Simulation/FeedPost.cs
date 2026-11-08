using System;
using System.Collections.Generic;

namespace EchoChamber
{
    [Serializable]
    public class FeedPost
    {
        public int id;
        public string author;
        public string text;
        public FeedTheme theme;
        public bool inflamed;
        public FeedRole role;
        /// <summary>Names of the neighbours whose public interests this post matches.</summary>
        public List<string> matches = new List<string>();
        /// <summary>Names of the neighbours the post cites or attacks.</summary>
        public List<string> mentions = new List<string>();
        public int minDay = 1;
        /// <summary>Short text burned over the video (what the clip says).</summary>
        public string overlay;
        /// <summary>Name of the 2D clip that plays behind it (feed_&lt;clip&gt;.mp4).</summary>
        public string clip;
        /// <summary>Background post of the default algorithm, never offered to the player.</summary>
        public bool filler;
    }
}
