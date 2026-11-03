using UnityEditor;

namespace EchoChamber.Tests
{
    /// <summary>Loads the imported content assets and builds simulations for tests.</summary>
    public static class TestContent
    {
        public static ContentSet Content()
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameContent>("Assets/_Project/Data/GameContent.asset");
            if (asset == null) throw new System.InvalidOperationException("GameContent.asset missing: run Echo Chamber/Import Content");
            return asset.content;
        }

        public static TuningData Tuning() => new TuningData();

        public static GameSimulation Sim(int seed = 1) => new GameSimulation(Content(), Tuning(), seed);

        public static GameSimulation Sim(SimRandom rng) => new GameSimulation(Content(), Tuning(), rng);
    }

    /// <summary>Random source that always returns the same value.</summary>
    public class ConstantRandom : SimRandom
    {
        private readonly float v;

        public ConstantRandom(float value)
        {
            v = value;
        }

        public override float Value() => v;
    }
}
