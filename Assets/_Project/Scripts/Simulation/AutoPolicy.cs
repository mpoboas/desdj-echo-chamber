using System;

namespace EchoChamber
{
    /// <summary>Scripted player for headless runs: fixed mixer weights and a stamp per post category.</summary>
    public class AutoPolicy
    {
        public string Name = "policy";
        public float[] Weights = { 50f, 50f, 50f, 50f, 50f };
        public DecisionMode[] ModeByCategory =
        {
            DecisionMode.Normal, DecisionMode.Normal, DecisionMode.Normal, DecisionMode.Normal, DecisionMode.Normal
        };
        /// <summary>Optional override: decides per post.</summary>
        public Func<PostData, DecisionMode> Custom;

        public DecisionMode Decide(PostData post)
        {
            return Custom != null ? Custom(post) : ModeByCategory[(int)post.category];
        }

        public static AutoPolicy All(string name, DecisionMode mode)
        {
            var p = new AutoPolicy { Name = name };
            for (int i = 0; i < p.ModeByCategory.Length; i++) p.ModeByCategory[i] = mode;
            return p;
        }
    }
}
