using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>Runtime state of one neighbour in the v2 feed system. Hidden values are never shown as numbers.</summary>
    public class FeedPerson
    {
        public FeedProfile Profile;
        public float Aggression;
        public float Bubble;
        public float Isolation;
        public float Apathy;
        public float Stress;
        public float EngagementMinutes;
        public int PostsSeen;
        public readonly Dictionary<FeedTheme, int> SeenByTheme = new Dictionary<FeedTheme, int>();
        public FeedReaction LastReaction;

        public FeedPerson(FeedProfile profile)
        {
            Profile = profile;
            Aggression = profile.aggression;
        }

        public int Seen(FeedTheme t)
        {
            int n;
            return SeenByTheme.TryGetValue(t, out n) ? n : 0;
        }
    }
}
