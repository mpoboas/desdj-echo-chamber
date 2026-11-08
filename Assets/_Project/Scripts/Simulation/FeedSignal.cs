namespace EchoChamber
{
    /// <summary>The six fixed, visible behaviours (GDD section 5). The only way hidden state reaches the player.</summary>
    public enum FeedSignal
    {
        None,
        StaysHome,
        HeadDown,
        ArguesInSquare,
        AvoidsNeighbour,
        VisitsAndSmiles,
        GoesToCafe
    }
}
