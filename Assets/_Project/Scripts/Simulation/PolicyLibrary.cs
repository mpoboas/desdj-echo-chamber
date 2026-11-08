using System.Collections.Generic;

namespace EchoChamber
{
    /// <summary>Named scripted players used by tests.</summary>
    public static class PolicyLibrary
    {
        private const DecisionMode N = DecisionMode.Normal;
        private const DecisionMode A = DecisionMode.Amplify;
        private const DecisionMode H = DecisionMode.Hide;

        public static AutoPolicy AllNormal() => AutoPolicy.All("all-normal", N);
        public static AutoPolicy AllAmplify() => AutoPolicy.All("all-amplify", A);
        public static AutoPolicy AllHide() => AutoPolicy.All("all-hide", H);
    }
}
