using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>Everything the newspaper reports about one turn.</summary>
    public class TurnLog
    {
        public readonly List<TurnPostEntry> Posts = new List<TurnPostEntry>();
        public readonly List<SimEvent> Events = new List<SimEvent>();
        public float StartCohesion;
        public float StartTension;
        public int StartRevenue;
    }
}
