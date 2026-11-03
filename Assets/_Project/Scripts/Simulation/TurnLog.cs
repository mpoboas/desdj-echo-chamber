using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>Everything the newspaper reports about one turn. Never contains causes.</summary>
    public class TurnLog
    {
        public readonly List<TurnPostEntry> Posts = new List<TurnPostEntry>();
        public readonly List<SimEvent> Events = new List<SimEvent>();
        public readonly List<Reveal> Reveals = new List<Reveal>();
        public int Notes;
        public float StartCohesion;
        public float StartTension;
        public int StartRevenue;
    }
}
