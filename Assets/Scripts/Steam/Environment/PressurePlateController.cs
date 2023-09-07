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
        [SerializeField] private bool _oneTimeStep;

        private int _playersCount;

        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            if(!other.CompareTag("Player")) return;
            _playersCount++;
            if(_playersCount >= 1) return;
            _triggerEvent.Raise(true);
            _particle.Play();
        }

        [ServerCallback]
        private void OnTriggerExit(Collider other)
        {
            if(_oneTimeStep || !other.CompareTag("Player")) return;
            _playersCount--;
            if(_playersCount <= 0)
                _triggerEvent.Raise(false);
        }
    }
}