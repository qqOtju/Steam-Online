using System;
using TMPro;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Events;

namespace Steam.UI
{
    public class UISettingsController : MonoBehaviour
    {
        [Header("InputFields")]
        [SerializeField] private TMP_InputField _mouseSensitivityInputField;
        [Header("Vars")]
        [SerializeField] private FloatVariable _mouseSensitivity;
        
        private const string Sensitivity = "sensitivity";
        
        private void Awake()
        {
            var sensitivity = PlayerPrefs.GetFloat(Sensitivity);
            if(sensitivity != 0) _mouseSensitivity.Value = sensitivity;
            _mouseSensitivityInputField.text = _mouseSensitivity.Value.ToString();
            _mouseSensitivityInputField.onValueChanged.AddListener(value =>
            {
                float.TryParse(value, out var sensitivity);
                PlayerPrefs.SetFloat(Sensitivity, sensitivity);
                _mouseSensitivity.Value = sensitivity;
            });
        }

        private void OnEnable()
        {
            _mouseSensitivityInputField.text = _mouseSensitivity.Value.ToString();
        }
        
        public void ExitGame() => Application.Quit();

        public void Toggle(GameObject obj) => obj.SetActive(!obj.activeSelf);
    }
}