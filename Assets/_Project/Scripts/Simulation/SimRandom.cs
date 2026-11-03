using System;

namespace EchoChamber
{
    /// <summary>All simulation randomness goes through here. Seedable; tests can subclass it.</summary>
    public class SimRandom
    {
        private readonly Random rng;

        public SimRandom(int seed)
        {
            rng = new Random(seed);
        }

        protected SimRandom()
        {
        }

        /// <summary>Uniform value in [0, 1).</summary>
        public virtual float Value()
        {
            return rng == null ? 0f : (float)rng.NextDouble();
        }

        public float Range(float min, float max)
        {
            return min + Value() * (max - min);
        }

        public float Range(FloatRange r)
        {
            return Range(r.min, r.max);
        }

        public int Index(int count)
        {
            int i = (int)(Value() * count);
            return i >= count ? count - 1 : i;
        }
    }
}
