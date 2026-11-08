using System;
using System.Collections.Generic;

namespace EchoChamber
{
    [Serializable]
    public class FeedLine
    {
        public string speaker;
        public string text;
    }

    [Serializable]
    public class FeedDialogueDay
    {
        public int day;
        public string title;
        public string subtitle;
        public List<FeedLine> pre = new List<FeedLine>();
        public List<FeedLine> call = new List<FeedLine>();
    }

    /// <summary>Phone calls and day cards. {goal} in a line is replaced by the day goal in minutes.</summary>
    [Serializable]
    public class FeedDialogue
    {
        public List<FeedDialogueDay> days = new List<FeedDialogueDay>();
        public List<FeedLine> met = new List<FeedLine>();
        public List<FeedLine> missed = new List<FeedLine>();
        public List<FeedLine> fired = new List<FeedLine>();

        public FeedDialogueDay Day(int day)
        {
            foreach (var d in days)
                if (d.day == day) return d;
            return null;
        }
    }
}
