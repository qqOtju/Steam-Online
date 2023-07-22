using Mirror;
using MyMirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Steam
{
    public class LocalLobby : MonoBehaviour
    {
        [Header("Panels")] 
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private GameObject _lobbyPanel;
        [Header("Other")]
        [SerializeField] [Scene] private string _menuScene = null;
        [SerializeField] private MyNetworkManager _networkManager;
        
        public void HostLobby()
        {
            _menuPanel.SetActive(false);
            _lobbyPanel.SetActive(true);
            _networkManager.StartHost();
        }

        public void JoinLobby()
        {
            _menuPanel.SetActive(false);
            _lobbyPanel.SetActive(true);
            _networkManager.StartClient();
        }
        
        public void StartGame()
        {
            if (SceneManager.GetActiveScene().name == _menuScene.SceneName())
                _networkManager.ServerChangeScene("Scene_Map_01");
        }
        
        /*public void LeaveLobby()
        {
            if (isServer) _networkManager.StopHost();
            else _networkManager.StopClient();
            _menuPanel.gameObject.SetActive(true);
            _lobbyPanel.gameObject.SetActive(false);
            gameObject.SetActive(true);
        }*/
    }
}