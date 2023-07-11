using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CleanMirror.UI
{
    public class JoinLobbyMenuController : MonoBehaviour
    {
        [SerializeField] private NetworkManagerLobby _networkManager = null;

        [Header("UI")]
        [SerializeField] private GameObject _landingPagePanel = null;
        [SerializeField] private TMP_InputField _ipAddressInputField = null;
        [SerializeField] private Button _joinButton = null;

        private void Start()
        {
            _ipAddressInputField.text = "localhost";
        }

        private void OnEnable()
        {
            _joinButton.interactable = true;
            NetworkManagerLobby.OnClientConnected += HandleClientConnected;
            NetworkManagerLobby.OnClientDisconnected += HandleClientDisconnected;
        }

        private void OnDisable()
        {
            NetworkManagerLobby.OnClientConnected -= HandleClientConnected;
            NetworkManagerLobby.OnClientDisconnected -= HandleClientDisconnected;
        }

        public void JoinLobby()
        {
            _networkManager.networkAddress = _ipAddressInputField.text;
            _networkManager.StartClient();
            
            //prevents players from spamming join btn
            _joinButton.interactable = false;
        }

        private void HandleClientConnected()
        {
            gameObject.SetActive(false);
            _landingPagePanel.SetActive(false);
        }

        private void HandleClientDisconnected()
        {
            _joinButton.interactable = true;
        }
        
    }
}