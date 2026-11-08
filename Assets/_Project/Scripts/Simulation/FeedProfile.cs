using System;
using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>Neighbour as the v2 feed system sees them: public interests plus hidden starting traits (0..1).</summary>
    [Serializable]
    public class FeedProfile
    {
        public int id;
        public string key;
        public string name;
        public string bio;
        public string color = "#FFFFFF";
        public PlaceId home;
        public int startDay = 1;
        public List<FeedTheme> interests = new List<FeedTheme>();
        public float aggression;
        public float openness;
        public float social;
    }
}
