using System;
using UnityEngine;

namespace Steam.Event
{
    public abstract class RaiseObject : MonoBehaviour
    {
        public Action Event;

        protected void Raise() =>
            Event.Invoke();
    }
}