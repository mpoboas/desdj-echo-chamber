namespace EchoChamber
{
    /// <summary>Picks the final ending, in the prototype order (PrototypeSpec section 6).</summary>
    public static class EndingResolver
    {
        public static EndingId Resolve(TuningData t, float avgApat, float tension, float cohesion, float lastDayRevenue, float lastDayTarget)
        {
            if (avgApat > t.endingSleepApat) return EndingId.Sleep;
            if (tension > t.endingLockedTension) return EndingId.Locked;
            if (cohesion >= t.endingAliveCohesion && lastDayRevenue < lastDayTarget) return EndingId.Alive;
            if (cohesion >= t.endingBalanceCohesion) return EndingId.Balance;
            return EndingId.Gentry;
        }
    }
}
