namespace EchoChamber
{
    /// <summary>Picks the final ending, in the prototype order (PrototypeSpec section 6).</summary>
    public static class EndingResolver
    {
        public static EndingId Resolve(TuningData t, float avgApat, float tension, float cohesion, float lastDayRevenue, float lastDayTarget, float understanding = 1f)
        {
            if (avgApat > t.endingSleepApat) return EndingId.Sleep;
            if (tension > t.endingLockedTension) return EndingId.Locked;
            if (cohesion >= t.endingAliveCohesion && lastDayRevenue < lastDayTarget) return EndingId.Alive;
            // The rare balance also needs the player to have understood the neighbourhood.
            if (cohesion >= t.endingBalanceCohesion && understanding >= t.endingBalanceUnderstanding) return EndingId.Balance;
            return EndingId.Gentry;
        }
    }
}
