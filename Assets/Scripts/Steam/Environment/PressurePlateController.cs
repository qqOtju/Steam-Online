using Mirror;
using Steam.Event;
using UnityEngine;

namespace Steam.Environment
{
    [SelectionBase]
    [RequireComponent(typeof(Collider))]
    public class PressurePlateController : RaiseObject
    {
        [SerializeField] private bool _singleUse;
        private bool _used;
        
        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            if(_singleUse)
            {
                if (_used || !other.CompareTag("Player")) return;
                _used = true;
                Raise();
            }
            else if (other.CompareTag("Player"))
            {
                _used = true;
                Raise();
            }
        }
    }
}