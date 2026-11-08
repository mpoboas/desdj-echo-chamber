using System;

namespace EchoChamber
{
    /// <summary>All balance numbers of the v2 feed system. Values live in data, not in logic.</summary>
    [Serializable]
    public class FeedTuning
    {
        // Day structure (placeholder values, calibrate with playtest)
        public int postsPerDay = 5;
        public int maxNewPostsPerDay = 3;
        public float[] dayGoals = { 7f, 11f, 15f, 19f, 24f };
        public int warningsToBeFired = 3;

        // Timeline of a day (seconds of game time)
        public float dayLength = 90f;
        public float[] postTimes = { 3f, 20f, 38f, 56f, 72f };

        // Viewer behaviour
        public float appTimeMultiplier = 1.2f;
        public float hookedRateMultiplier = 1.7f;
        public float swipeSeconds = 0.4f;
        public float interruptSeconds = 1.8f;
        public float fillerChance = 0.35f;
        public float fatiguePerBoring = 0.25f;
        public float fatigueBase = 0.05f;
        public float hotRestores = 0.08f;
        public float restThreshold = 0.25f;
        public float restMin = 14f;
        public float restMax = 20f;
        public float energyAfterRest = 0.55f;
        public float minePostEnergy = 0.3f;
        public float minePostEnergyByInterest = 0.35f;
        public int hookVideosGood = 4;
        public int hookVideosOther = 1;
        public float hookInterest = 0.7f;
        public float typeSecondsPerChar = 0.09f;
        public float readCommentsMin = 1.8f;
        public float readCommentsMax = 2.8f;

        // Engagement (legacy per-post model, minutes of app time per post)
        public float baseMinutes = 1f;
        public float matchMultiplier = 2.5f;
        public float inflamedMultiplier = 2.2f;
        public float repeatBonusPerSeen = 0.15f;
        public float repeatBonusCap = 0.6f;
        public float jitter = 0.1f;
        public float discardPenaltyMinutes = 1f;

        // Reaction thresholds on engagement
        public float dwellThreshold = 2f;
        public float likeThreshold = 4f;

        // Hidden effects (per exposure)
        public float inflamedAggression = 0.12f;
        public float matchBubble = 0.05f;
        public float inflamedBubble = 0.08f;
        public float passiveIsolation = 0.06f;
        public float bridgeBubbleDrop = 0.12f;
        public float bridgeIsolationDrop = 0.08f;
        public float mentionedStress = 0.1f;
        public float calmAggressionDrop = 0.02f;

        // Visible signal thresholds
        public float stayHomeIsolation = 0.5f;
        public float argueAggression = 0.6f;
        public float avoidBubble = 0.5f;
        public float headDownApathy = 0.5f;
        public float healthyBubbleMax = 0.25f;
    }
}
