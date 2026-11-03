using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>A post published in a turn, with the decision and (after delivery) its outcome.</summary>
    public class TurnPostEntry
    {
        public PostData Post;
        public DecisionMode Mode;
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
