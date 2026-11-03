using System.Collections.Generic;

namespace EchoChamber
{
    public class RunResult
    {
        public string Policy;
        public int Seed;
        public EndingId Ending;
        public int[] RevenueByDay;
        public float Cohesion;
        public float Tension;
        public float Apathy;
        public int TurnsPlayed;
        public int GoalsAchieved;
        public int GoalsTotal;
        public readonly List<KeyValuePair<float, float>> History = new List<KeyValuePair<float, float>>();
        public readonly List<SimEventKind> Events = new List<SimEventKind>();
    }
}
