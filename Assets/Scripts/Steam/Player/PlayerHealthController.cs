using System;
using UnityEngine;

namespace Steam.Player
{
    public class PlayerHealthController
    {
        private readonly float _maxHealth;
        
        private float _currentHealth;
        public event Action OnDeath;

        public PlayerHealthController(float maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
        }

        public float GetDamage(float dmg)
        {
            _currentHealth = Mathf.Max(0, _currentHealth - dmg);
            if(_currentHealth <= 0)
                OnDeath?.Invoke();
            return _currentHealth;
        }

        public float GetHeal(float heal) => _currentHealth = Mathf.Min(_maxHealth, _currentHealth + heal);
    }
}