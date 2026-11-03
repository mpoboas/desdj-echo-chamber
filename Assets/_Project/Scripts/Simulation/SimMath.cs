namespace EchoChamber
{
    public static class SimMath
    {
        public static float Clamp01(float x)
        {
            return x < 0f ? 0f : (x > 1f ? 1f : x);
        }

        public static float Clamp(float x, float a, float b)
        {
            return x < a ? a : (x > b ? b : x);
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }
}
