using UnityEngine;

namespace EchoChamber
{
    /// <summary>All simulation numbers, editable in the inspector. Defaults are the prototype values.</summary>
    [CreateAssetMenu(menuName = "Echo Chamber/Tuning Config", fileName = "TuningConfig")]
    public class TuningConfig : ScriptableObject
    {
        public TuningData data = new TuningData();
    }
}
