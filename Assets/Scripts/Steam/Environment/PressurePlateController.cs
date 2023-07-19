using Mirror;
using Steam.Event;
using UnityEngine;

namespace Steam.Environment
{
    [SelectionBase]
    [RequireComponent(typeof(Collider))]
    public class PressurePlateController : RaiseObject
    {
        [Server]
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                Raise();
        }
    }
}