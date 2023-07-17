using System;
using InputSystem;
using TMPro;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;

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

        private Controls _controls;
        private UIPlayerStatus _playerStatus;

        #region Mono
        
        private void Awake()
        {
            _controls = new Controls();
            _controls.Player.Escape.performed += OnEscapePerformed;
            _playerStatus = new(_groundedChange, 
                _sprintChange, _groundedStatusText, _sprintStatusText);
        }

        private void OnEnable() => _controls.Enable();
        
        private void OnDisable() => _controls.Disable();

        private void OnDestroy()
        {
            _controls.Player.Escape.performed -= OnEscapePerformed;
            _playerStatus.OnDestroy();
        }
        
        #endregion

        private void OnEscapePerformed(InputAction.CallbackContext obj) => 
            _settingsPanel.SetActive(!_settingsPanel.activeSelf);
    }
}