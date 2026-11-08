using System.Collections.Generic;

namespace EchoChamber
{
    public class RunResult
    {
        public string Policy;
        public int Seed;
        public int Revenue;
        public float Cohesion;
        public float Tension;
        public float Apathy;
        public int TurnsPlayed;
        public readonly List<SimEventKind> Events = new List<SimEventKind>();
    }
}
