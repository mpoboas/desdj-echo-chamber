using System;

namespace EchoChamber
{
    [Serializable]
    public class NeighbourData
    {
        public int id;
        public string name;
        public Trait trait;
        /// <summary>Gendered Portuguese label, e.g. "ansiosa".</summary>
        public string traitLabel;
        public string color = "#FFFFFF";
        public PlaceId home;
    }
}
