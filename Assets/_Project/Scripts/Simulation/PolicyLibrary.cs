using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>Named scripted players used by tests and the balance report.</summary>
    public static class PolicyLibrary
    {
        private static AutoPolicy Make(string name, DecisionMode[] modes, float[] weights)
        {
            return new AutoPolicy { Name = name, ModeByCategory = modes, Weights = weights };
        }

        // Category order: Sens, Com, Ent, Pub, Loc
        private const DecisionMode N = DecisionMode.Normal;
        private const DecisionMode A = DecisionMode.Amplify;
        private const DecisionMode H = DecisionMode.Hide;

        public static AutoPolicy AllNormal() => AutoPolicy.All("all-normal", N);
        public static AutoPolicy AllAmplify() => AutoPolicy.All("all-amplify", A);
        public static AutoPolicy AllHide() => AutoPolicy.All("all-hide", H);

        /// <summary>Entertainment boosted, everything else muted: apathy wins.</summary>
        public static AutoPolicy TargetSleep() => Make("target-sleep", new[] { N, H, A, N, A }, new[] { 50f, 90f, 50f, 90f, 90f });

        /// <summary>Fear boosted: tension runs away.</summary>
        public static AutoPolicy TargetLocked() => Make("target-locked", new[] { A, N, N, H, H }, new[] { 90f, 90f, 50f, 90f, 50f });

        /// <summary>Hide everything: quiet, cohesive, no money.</summary>
        public static AutoPolicy TargetAlive() => Make("target-alive", new[] { H, H, H, H, H }, new[] { 50f, 50f, 50f, 50f, 50f });

        /// <summary>Community and ads boosted, fear muted.</summary>
        public static AutoPolicy TargetBalance() => Make("target-balance", new[] { A, A, A, A, H }, new[] { 10f, 90f, 90f, 90f, 90f });

        /// <summary>Same as target-balance but never notes a reaction, so it never understands the neighbourhood.</summary>
        public static AutoPolicy TargetBalanceNoNotes()
        {
            var p = TargetBalance();
            p.Name = "target-balance-no-notes";
            p.NotesPerTurn = 0;
            return p;
        }

        /// <summary>
        /// Upper bound for a player who reads every post: decides by the real effects of the post, not by its category.
        /// Used to check that the category label alone is no longer enough.
        /// </summary>
        public static AutoPolicy OraclePerPost()
        {
            var p = Make("oracle-per-post", new[] { N, N, N, N, N }, new[] { 90f, 90f, 90f, 90f, 90f });
            p.Custom = post =>
            {
                PostEffects e = post.effects;
                // Net effect on the neighbourhood: belonging up, distrust, fear and apathy down.
                float net = e.emp - e.desc - e.medo - 0.5f * e.apat;
                if (net >= 1f) return A;
                if (net <= -5f) return H;
                // Money matters too: amplify anything that is not clearly harmful.
                if (net >= -3f && post.value >= 0.3f) return A;
                return N;
            };
            return p;
        }

        public static AutoPolicy TargetGentry() => Make("target-gentry", new[] { N, H, H, A, N }, new[] { 90f, 50f, 50f, 90f, 10f });

        public static List<AutoPolicy> All()
        {
            return new List<AutoPolicy>
            {
                AllNormal(), AllAmplify(), AllHide(),
                TargetSleep(), TargetLocked(), TargetAlive(), TargetBalance(), TargetBalanceNoNotes(), OraclePerPost(), TargetGentry()
            };
        }

        public static AutoPolicy For(EndingId ending)
        {
            switch (ending)
            {
                case EndingId.Sleep: return TargetSleep();
                case EndingId.Locked: return TargetLocked();
                case EndingId.Alive: return TargetAlive();
                case EndingId.Balance: return TargetBalance();
                default: return TargetGentry();
            }
        }
    }
}
