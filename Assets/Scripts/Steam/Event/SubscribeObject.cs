using System;
using Mirror;

namespace Steam.Event
{
    public abstract class SubscribeObject : NetworkBehaviour
    {
        public abstract void Subscribe(ref Action action);
    }
}