using UnityEngine;

namespace EchoChamber
{
    /// <summary>Posts, days, neighbours, places and endings, imported from docs/data/*.json.</summary>
    [CreateAssetMenu(menuName = "Echo Chamber/Game Content", fileName = "GameContent")]
    public class GameContent : ScriptableObject
    {
        public ContentSet content = new ContentSet();
    }
}
