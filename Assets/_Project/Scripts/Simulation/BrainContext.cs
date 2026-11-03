namespace EchoChamber
{
    /// <summary>World facts the utility brain needs to score actions.</summary>
    public struct BrainContext
    {
        public bool IsNight;
        public bool MerceariaOpen;
        public bool LivrariaOpen;
        public bool PoliceOnStreet;
        public bool FestaActive;
    }
}
