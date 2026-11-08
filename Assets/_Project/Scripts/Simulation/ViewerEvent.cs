namespace EchoChamber
{
    public struct ViewerEvent
    {
        public FeedViewer Viewer;
        public ViewerEventKind Kind;
        public FeedPost Post;
        /// <summary>Comment text for CommentPosted.</summary>
        public string Text;
        /// <summary>True when the post was handed over by the player.</summary>
        public bool Mine;
    }
}
