using System;
using System.Collections.Generic;

namespace EchoChamber
{
    [Serializable]
    public class FeedContent
    {
        public List<FeedPost> posts = new List<FeedPost>();
        public List<FeedProfile> profiles = new List<FeedProfile>();
        public List<FeedCommentPool> commentPools = new List<FeedCommentPool>();
        public FeedDialogue dialogue = new FeedDialogue();

        public List<string> Pool(string key)
        {
            foreach (var p in commentPools)
                if (p.key == key) return p.lines;
            return null;
        }

        public FeedProfile FindProfile(string name)
        {
            foreach (var p in profiles)
                if (p.name == name || p.name.EndsWith(" " + name)) return p;
            return null;
        }
    }
}
