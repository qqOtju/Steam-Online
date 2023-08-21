using Mirror;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Steam.Environment
{
    [SelectionBase]
    [RequireComponent(typeof(Collider))]
    public class PressurePlateController : MonoBehaviour
    {
        [SerializeField] private BoolEvent _triggerEvent;
        [SerializeField] private ParticleSystem _particle;

        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            if(!other.CompareTag("Player")) return;
            _triggerEvent.Raise(true);
            _particle.Play();
        }

        [ServerCallback]
        private void OnTriggerExit(Collider other)
        {
            if(!other.CompareTag("Player")) return;
            _triggerEvent.Raise(false);
        }
    }
}