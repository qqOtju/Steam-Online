using Mirror;
using Steam.Interface;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Steam.Environment
{
    public class NewLaserController : NetworkBehaviour
    {
        [SerializeField] private FloatConstant _laserDamage;
        [SerializeField] private Transform _startTransform;
        [SerializeField] private GameObject _laserObj;
        [SerializeField] private float _timeBeforeDamage = 1f;

        private Transform _laserTransform;
        private Vector3 _startPos;
        private bool _damaging;
        private float _time;

        [ServerCallback]
        private void Awake()
        {
            _laserTransform = _laserObj.transform;
            _startPos = _startTransform.position;
        }

        [ServerCallback]
        private void FixedUpdate()
        {
            if(_damaging)
                _time += Time.deltaTime;
            if (Physics.Raycast(_startTransform.position, _startTransform.forward, out var hit))
            {
                var distance = Vector3.Distance(_startPos, hit.point) / 2;
                _laserTransform.position = Vector3.Lerp(_startPos, hit.point, 0.5f);
                var localScale = _laserTransform.localScale;
                localScale = new Vector3(localScale.x, distance, localScale.z);
                _laserTransform.localScale = localScale;
                if (!hit.collider.TryGetComponent<IDamageable>(out var damageable)) return;
                _damaging = true;
                if (!(_time >= _timeBeforeDamage)) return;
                _time = 0f;
                damageable.GetDamage(_laserDamage.Value);
            }
            else
            {
                _damaging = false;
                _time = 0f;
                _laserTransform.localScale = Vector3.zero;
            }
        }
    }
}