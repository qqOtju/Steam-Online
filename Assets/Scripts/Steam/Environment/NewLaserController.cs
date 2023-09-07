using System;
using System.Collections.Generic;
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
        [SerializeField] private Transform _startTransform;
        [SerializeField] private List<Transform> _points;
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
            if(_points.Count > 1)
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

        [ContextMenu("Create Point")]
        private void CreatePoint()
        {
            var point = new GameObject($"Point #{_points.Count}");
            point.transform.parent = transform.parent;
            point.transform.position = transform.position;
            _points.Add(point.transform);
        }
        
        private void Move(int index)
        {
            if (index == _points.Count)
                index = 0;
            var startPos = transform.position;
            var endPos = _points[index].position;
            LeanTween.value(0, 1, _moveTime).setOnUpdate(value =>
                transform.position = Vector3.Lerp(startPos, endPos, value)
            ).setOnComplete(_ => Move(index + 1));
        }

        private void OnDrawGizmos()
        {
            if (_points.Count <= 0) return;
            const float radius = 0.2f;
            foreach (var tr in _points)
                Gizmos.DrawSphere(tr.position, radius);
        }
    }
}