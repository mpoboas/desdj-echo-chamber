using UnityEngine;

namespace EchoChamber
{
    /// <summary>Marks a collider in the room (or a neighbour in the street) as interactive.</summary>
    public class RoomClickable : MonoBehaviour
    {
        public ClickAction action = ClickAction.None;
        public string tooltip;
        public ViewId view;
        public PostCategory category;
        public int neighbourIndex;
    }
}
