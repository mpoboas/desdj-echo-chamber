using System;
using System.Collections.Generic;

namespace EchoChamber
{
    [Serializable]
    public class DayData
    {
        public string name;
        public int target;
        public string boss;
        public string brief;
        public List<TurnData> turns = new List<TurnData>();
    }
}
