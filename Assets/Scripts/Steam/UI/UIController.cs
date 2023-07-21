using System;
using InputSystem;
using TMPro;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Steam.UI
{
    public class UIController : MonoBehaviour
    {
        [Header("Grounded")]
        [SerializeField] private BoolEvent _groundedChange;
        [SerializeField] private TextMeshProUGUI _groundedStatusText;
        [Header("Sprint")]
        [SerializeField] private BoolEvent _sprintChange;
        [SerializeField] private TextMeshProUGUI _sprintStatusText;
        [Header("Settings")] 
        [SerializeField] private GameObject _settingsPanel;
        [Header("HealthBar")]
        [SerializeField] private Image _healthBar;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private FloatEvent _onHealthChange;
        [SerializeField] private FloatConstant _maxHealth;
        

        private UIPlayerStatus _playerStatus;
        private bool _cursorState;
        private Controls _controls;

        #region Mono
        
        private void Awake()
        {
            _controls = new Controls();
            _cursorState = true;
            _onHealthChange.Register(UpdateHealthBar);
            SetCursorState(_cursorState);
            _controls.Player.Escape.performed += OnEscapePerformed;
            _playerStatus = new(_groundedChange, 
                _sprintChange, _groundedStatusText, _sprintStatusText);
        }
        private void OnDestroy()
        {
            _controls.Player.Escape.performed -= OnEscapePerformed;
            _playerStatus.OnDestroy();
        }
        private void OnEnable() => _controls.Enable();
        
        private void OnDisable() => _controls.Disable();

        #endregion

        private void OnEscapePerformed(InputAction.CallbackContext obj)
        { 
            _settingsPanel.SetActive(!_settingsPanel.activeSelf);
            _cursorState = !_cursorState;
            SetCursorState(_cursorState);
        }
        
        private void SetCursorState(bool newState) =>
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        
        private void UpdateHealthBar(float newValue)
        {
            _healthBar.fillAmount = newValue / _maxHealth.Value;
            _healthText.text = $"{Math.Round(newValue, 2)}/{_maxHealth.Value}";
        }
    }
}