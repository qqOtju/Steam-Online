using System;
using System.Collections;
using Mirror;
using Steam.Event;
using UnityEngine;
using UnityEngine.Events;

namespace Steam.Environment
{
    [SelectionBase]
    [RequireComponent(typeof(Animator))]
    public class DoorController : SubscribeObject
    {
        [SerializeField] private float _openTime = 1.0f;
        [SerializeField] private bool _singleUse = false;
        [SerializeField] private UnityEvent _subscribeEvent;
        private readonly int _value = Animator.StringToHash("Value");
        private Animator _animator;
        private bool _isOpen;
        private bool _isOpening;
        private bool _isActivated;

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        [Server]
        public override void Subscribe(ref Action action) => 
            action += () => _subscribeEvent?.Invoke();
        

        public void Door()
        {
            Debug.Log("Door");
            if(_singleUse) 
                if (_isActivated) return;
                else _isActivated = true;
            ToggleDoor();
        }
        
        public void OpenDoor() =>
            StartCoroutine(DoorCoroutine(true));
        
        public void CloseDoor() =>
            StartCoroutine(DoorCoroutine(false));
        
        public void ToggleDoor() =>
            StartCoroutine(DoorCoroutine(!_isOpen));
        
        [Server]
        private IEnumerator DoorCoroutine(bool open)
        {
            if (_isOpening) yield break;
            _isOpening = true;
            var timer = 0f;
            while (timer < _openTime)
            {
                timer += Time.deltaTime;
                _animator.SetFloat(_value, open? timer / _openTime : 1 - timer / _openTime);
                yield return null;
            }
            _isOpen = open;
            _animator.SetFloat(_value,  open? 1 : 0);
            _isOpening = false;
        }
    }
}