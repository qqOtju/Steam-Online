using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Steam.Level
{
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private List<LevelEvent> _events;

        [Server]        
        public void Add(int index)
        {
            var levelEvent = _events[index];
            Debug.Log($"{levelEvent.MaxCount}");
            levelEvent.Add();
            if(levelEvent.Count == levelEvent.MaxCount)
                levelEvent.Event?.Invoke();
        }
    }

    [Serializable]
    public class LevelEvent
    {
        [SerializeField] private int _count;
        [SerializeField] private UnityEvent _event;
        private int _currentCount;
        
        public int Count => _currentCount;
        public int MaxCount => _count;
        public UnityEvent Event => _event;
        public void Add() => _currentCount++;
    }
}