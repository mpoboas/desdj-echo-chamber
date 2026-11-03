using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>A scheduled delivery of one published post during the simulate phase.</summary>
    public class Delivery
    {
        public TurnPostEntry Entry;
        public float At;
        public bool Started;
        public float Time;
        public readonly List<Neighbour> Viewers = new List<Neighbour>();
    }
}
