using System;
using UnityEngine;
using UnityEngine.Events;

namespace Steam.Event
{
    [Serializable]
    public struct Connection
    {
        [field: SerializeField] public RaiseObject Event { get; private set; }
        [field: SerializeField] public SubscribeObject[] Subscribers { get; private set; }
        [SerializeField] public UnityEvent SubscribeEvent;
    }
}