namespace EchoChamber
{
    public struct PathPoint
    {
        public float X;
        public float Y;
        /// <summary>True when this waypoint is a crosswalk crossing that waits for the pedestrian light.</summary>
        public bool Cross;

        public PathPoint(float x, float y, bool cross = false)
        {
            X = x;
            Y = y;
            Cross = cross;
        }
    }
}
