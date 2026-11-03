using System.Collections.Generic;

namespace EchoChamber
{
    public class ObserveResult
    {
        /// <summary>False when there was nothing new to note (no active reaction or already noted).</summary>
        public bool Noted;
        public Reaction Reaction;
        public bool TraitDiscovered;
        public readonly List<Neighbour> NewRelations = new List<Neighbour>();
    }
}
