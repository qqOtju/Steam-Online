using System;
using UnityEngine;

namespace Steam.Event
{
    [Serializable]
    public struct Connection
    {
        [field: SerializeField] public RaiseObject Event { get; private set; }
        [field: SerializeField] public SubscribeObject[] Subscribers { get; private set; }
    }
}