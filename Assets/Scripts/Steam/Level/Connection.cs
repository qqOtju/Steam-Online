using System;
using UnityAtoms.BaseAtoms;
using UnityEngine.Events;

namespace Steam.Level
{
    [Serializable]
    public struct Connection
    {
        public BoolEvent[] triggers;
        public UnityEvent events;
        public int triggersCount;
        public int currentTriggers;
    }
}