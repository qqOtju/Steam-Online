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
        [SerializeField] private TextMeshProUGUI _nicknameText;
        [SerializeField] private TextMeshProUGUI _idText;
        [SerializeField] private FloatEvent _onHealthChange;
        [SerializeField] private FloatConstant _maxHealth;
        [SerializeField] private IntVariable _playerId;
        [SerializeField] private StringVariable _playerName;

        private UIPlayerStatus _playerStatus;
        private Controls _controls;

        #region Mono
        
        private void Awake()
        {
            _controls = new Controls();
            _onHealthChange.Register(UpdateHealthBar);
            SetCursorState(true);
            _controls.Player.Escape.performed += OnEscapePerformed;
            _playerStatus = new(_groundedChange, 
                _sprintChange, _groundedStatusText, _sprintStatusText);
        }

        private void Start()
        {
            _nicknameText.text = _playerName.Value;
            _idText.text = _playerId.Value.ToString();
        }

        private void OnDestroy()
        {
            _controls.Player.Escape.performed -= OnEscapePerformed;
            _playerStatus.OnDestroy();
        }
        private void OnEnable() => _controls.Enable();
        
        private void OnDisable() => _controls.Disable();

        #endregion

        private void OnEscapePerformed(InputAction.CallbackContext obj) => CloseMenu();
        
        public void CloseMenu()
        {
            
            _settingsPanel.SetActive(!_settingsPanel.activeSelf);
            SetCursorState(!_settingsPanel.activeSelf);
        }
        
        private void SetCursorState(bool newState) =>
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        
        private void UpdateHealthBar(float newValue) =>
            _healthBar.fillAmount = newValue / _maxHealth.Value;
    }
}