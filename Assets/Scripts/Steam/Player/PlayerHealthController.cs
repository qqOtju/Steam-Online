using System;

namespace Steam.Player
{
    public class PlayerHealthController
    {
        public event Action OnDeath;

        private readonly float _maxHealth;
        private float _currentHealth;

        public PlayerHealthController(float maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
        }

        public float GetDamage(float dmg)
        {
            if(_currentHealth <= 0)
                return _currentHealth;
            if(_currentHealth - dmg <= 0)
            {
                _currentHealth -= dmg;
                OnDeath?.Invoke();
            }
            else
                _currentHealth -= dmg;
            return _currentHealth;
        }

        public float GetHeal(float heal)
        {
            if(_currentHealth >= _maxHealth)
                return _currentHealth;
            if(_currentHealth + heal >= _maxHealth)
                _currentHealth = _maxHealth;
            else
                _currentHealth += heal;
            return _currentHealth;
        }
    }
}