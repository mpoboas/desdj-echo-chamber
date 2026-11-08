using UnityEngine;

namespace EchoChamber
{
    /// <summary>v2 posts and neighbour profiles, imported from docs/data/feed_*.json.</summary>
    [CreateAssetMenu(menuName = "Echo Chamber/Feed Content", fileName = "FeedContent")]
    public class FeedContentAsset : ScriptableObject
    {
        public FeedContent content = new FeedContent();
    }
}
