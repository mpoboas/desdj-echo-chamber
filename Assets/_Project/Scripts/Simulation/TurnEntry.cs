using System;

namespace EchoChamber
{
    /// <summary>One item of a turn script: a scripted post, the grocery ad, or N random posts.</summary>
    [Serializable]
    public class TurnEntry
    {
        public TurnEntryKind kind;
        public string postId;
        public int count = 1;
    }
}
