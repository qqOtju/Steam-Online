using Mirror;
using Steam.Interfaces;
using UnityEngine;

namespace Steam.Environment
{
    [SelectionBase]
    public class NewLaserController : NetworkBehaviour
    {
        [Header("Values")]
        [SerializeField] private float _laserDamage = 0.5f;
        [SerializeField] private float _timeBeforeDamage = 1f;
        [SerializeField] private float _moveTime = 2f;
        [Header("Movement")]
        [SerializeField] private bool _isStatic = true;
        [SerializeField] private Transform _startTransform;
        [SerializeField] private Transform[] _movePositions;
        [Header("Other")]
        [SerializeField] private GameObject _laserObj;
        [SerializeField] private LayerMask _ignore;
        [SerializeField] private ParticleSystem _particle;
        
        private const int MaxDistance = 100;

        private Transform _laserTransform;
        private Vector3 _scale;
        private bool _damaging;
        private float _time;

        [ServerCallback]
        private void Start()
        {
            _laserTransform = _laserObj.transform;
            _scale = _laserObj.transform.lossyScale;
            if(!_isStatic)
                Move(0);
        }

        [ServerCallback]
        private void FixedUpdate()
        {
            if(_damaging) _time += Time.deltaTime;
            if (Physics.Raycast(_startTransform.position, _startTransform.forward, out var hit, MaxDistance, _ignore))
            {
                var position = _startTransform.position;
                _particle.transform.position = hit.point;
                var distance = Vector3.Distance(position, hit.point) / 2;
                _laserTransform.position = Vector3.Lerp(position, hit.point, 0.5f);
                var localScale = new Vector3(_scale.x, distance, _scale.z);
                _laserTransform.localScale = localScale;
                if (!hit.collider.TryGetComponent<IDamageable>(out var damageable)) return;
                _damaging = true;
                if (!(_time >= _timeBeforeDamage)) return;
                _time = 0f;
                damageable.GetDamage(_laserDamage);
            }
            else
            {
                _damaging = false;
                _time = 0f;
                _laserTransform.localScale = Vector3.zero;
            }
        }

        private void OnDestroy() => LeanTween.cancelAll();

        private void Move(int index)
        {
            if (index == _movePositions.Length)
                index = 0;
            var startPos = transform.position;
            var endPos = _movePositions[index].position;
            LeanTween.value(0, 1, _moveTime).setOnUpdate(value =>
                transform.position = Vector3.Lerp(startPos, endPos, value)
            ).setOnComplete(_ => Move(index + 1));
        }
    }
}