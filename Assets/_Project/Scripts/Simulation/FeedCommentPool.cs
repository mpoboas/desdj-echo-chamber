using System;
using System.Collections.Generic;

namespace EchoChamber
{
    [Serializable]
    public class FeedCommentPool
    {
        public string key;
        public List<string> lines = new List<string>();
    }
}
