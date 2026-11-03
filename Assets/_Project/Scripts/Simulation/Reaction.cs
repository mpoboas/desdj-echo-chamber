namespace EchoChamber
{
    /// <summary>The visible reaction a neighbour shows after seeing a post.</summary>
    public class Reaction
    {
        public int Id;
        public ReactionKind Kind;
        /// <summary>What the post would cause without personality.</summary>
        public ReactionKind Base;
        public bool Strong;
        public PostCategory Category;
        /// <summary>Seconds left in which the reaction is visible and can be noted.</summary>
        public float TimeLeft;
    }
}
