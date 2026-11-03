namespace EchoChamber
{
    /// <summary>Something that happened in the neighbourhood. Text lives in the presentation layer.</summary>
    public class SimEvent
    {
        public SimEventKind Kind;
        public bool Good;
        public int Day;
        public int Turn;
    }
}
