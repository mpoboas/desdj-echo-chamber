using System;
using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>All authored content the simulation needs. Plain data, no engine types.</summary>
    [Serializable]
    public class ContentSet
    {
        public List<PostData> pool = new List<PostData>();
        public List<PostData> scripted = new List<PostData>();
        public List<DayData> days = new List<DayData>();
        public List<NeighbourData> neighbours = new List<NeighbourData>();
        public List<PlaceData> places = new List<PlaceData>();

        public PostData FindScripted(string id)
        {
            foreach (var p in scripted)
                if (p.id == id) return p;
            return null;
        }

        public PlaceData FindPlace(PlaceId id)
        {
            foreach (var p in places)
                if (p.id == id) return p;
            return null;
        }
    }
}
