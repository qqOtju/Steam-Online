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
        [SerializeField] private GameObject _menuPanel;
        [Header("HealthBar")]
        [SerializeField] private Image _healthBar;
        [SerializeField] private TextMeshProUGUI _nicknameText;
        [SerializeField] private TextMeshProUGUI _idText;
        [SerializeField] private FloatEvent _onHealthChange;
        [SerializeField] private FloatConstant _maxHealth;
        [SerializeField] private IntVariable _playerId;
        [SerializeField] private StringVariable _playerName;
        [Header("Events")]
        [SerializeField] private BoolEvent _onMenuToggle;
        [SerializeField] private VoidEvent _onLobbyLeave;

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
            _onHealthChange.Unregister(UpdateHealthBar);
            _playerStatus.OnDestroy();
        }
        private void OnEnable() => _controls.Enable();
        
        private void OnDisable() => _controls.Disable();

        #endregion
        
        public void ToggleMenu()
        {
            _menuPanel.SetActive(!_menuPanel.activeSelf);
            _onMenuToggle.Raise(!_menuPanel.activeSelf);
            SetCursorState(!_menuPanel.activeSelf);
        }

        public void LeaveLobby() =>
            _onLobbyLeave.Raise();

        private void OnEscapePerformed(InputAction.CallbackContext obj) => ToggleMenu();

        private void SetCursorState(bool newState) =>
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        
        private void UpdateHealthBar(float newValue) =>
            _healthBar.fillAmount = newValue / _maxHealth.Value;
    }
}