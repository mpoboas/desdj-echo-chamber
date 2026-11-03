using System;
using System.Collections.Generic;

namespace EchoChamber
{
    [Serializable]
    public class TurnData
    {
        public List<TurnEntry> entries = new List<TurnEntry>();
    }
}
