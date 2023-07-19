using System;
using Mirror;

namespace Steam.Event
{
    public abstract class RaiseObject : NetworkBehaviour
    {
        public Action Event;

        protected void Raise() =>
            Event.Invoke();
    }
}