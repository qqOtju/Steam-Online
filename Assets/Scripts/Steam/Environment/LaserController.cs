using Mirror;
using Steam.Interface;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Steam.Environment
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserController : NetworkBehaviour
    {
        [SerializeField] private FloatConstant _laserDamage;
        [SerializeField] private GameObject _startPoint;
        [SerializeField] private float _timeBeforeDamage = 1f;
        
        private LineRenderer _lineRenderer;
        private Transform _startTransform;
        private bool _damaging;
        private float _time;

        [ServerCallback]
        private void Start()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _startTransform = _startPoint.transform;
        }
        
        [ServerCallback]
        private void FixedUpdate()
        {
            if(_damaging)
                _time += Time.deltaTime;
            _lineRenderer.SetPosition(0, _startTransform.position);
            if (Physics.Raycast(_startTransform.position, _startTransform.forward, out var hit))
            {
                _lineRenderer.SetPosition(1, hit.point);
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
                _lineRenderer.SetPosition(1, _startTransform.position + _startTransform.forward * 100);
            }
        }
    }
}