using System;
using UnityEngine;

namespace Steam.Event
{
    public abstract class SubscribeObject : MonoBehaviour
    {
        public abstract void Subscribe(ref Action action);
    }
}