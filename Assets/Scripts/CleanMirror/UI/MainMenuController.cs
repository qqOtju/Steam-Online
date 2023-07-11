using UnityEngine;

namespace CleanMirror.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private NetworkManagerLobby _networkManager = null;
        [Header("UI")]
        [SerializeField] private GameObject _landingPagePanel = null;

        public void HostLobby()
        {
            _networkManager.StartHost();

            _landingPagePanel.SetActive(false);
        }
    }
}