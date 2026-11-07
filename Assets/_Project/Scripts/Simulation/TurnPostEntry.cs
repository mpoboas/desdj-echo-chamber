using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>A post published in a turn, with the decision and (after delivery) its outcome.</summary>
    public class TurnPostEntry
    {
        public PostData Post;
        public DecisionMode Mode;
        /// <summary>Who wrote it: a neighbour's name or an entity such as the Junta.</summary>
        public string Author;
        /// <summary>Forced into the queue by the boss: cannot be hidden.</summary>
        public bool Imposed;
        public int Viewers;
        public int Revenue;
        public readonly List<string> ViewerNames = new List<string>();
        public float DeltaMedo;
        public float DeltaEmp;
        public float DeltaDesc;
        public float DeltaApat;
        public float DeltaCons;
        public bool Delivered;
    }
}
