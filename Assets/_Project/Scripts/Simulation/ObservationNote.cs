namespace EchoChamber
{
    /// <summary>One entry of the player's notebook about a neighbour.</summary>
    public class ObservationNote
    {
        public int ReactionId;
        public PostCategory Category;
        public ReactionKind Kind;
        public bool Strong;
        public ReactionKind Base;
    }
}
