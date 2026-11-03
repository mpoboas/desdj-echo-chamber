using System;

namespace EchoChamber
{
    /// <summary>Logical place in the 1000x640 prototype world.</summary>
    [Serializable]
    public class PlaceData
    {
        public PlaceId id;
        public string shortName;
        public string displayName;
        /// <summary>x, y, w, h</summary>
        public float[] rect = new float[4];
        public float[] door = new float[2];
        /// <summary>x, y, w, h. Empty when the place has no dedicated zone.</summary>
        public float[] zone = new float[0];
        public bool indoor;
        public bool open;
        public float height;

        public bool HasZone => zone != null && zone.Length == 4;
    }
}
