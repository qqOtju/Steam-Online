using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Steam.UI
{
    public class UIPlayerInfo : MonoBehaviour
    {
        [Header("Info")]
        [SerializeField] private RawImage _displayProfileImage;
        [SerializeField] private TextMeshProUGUI _displayNicknameText;
        [Header("Status")]
        [SerializeField] private Image _displayStatusImage;
        [SerializeField] private Color _readyColor;
        [SerializeField] private Color _notReadyColor;
        
        private string _playerName;
        private int _connectionId;
        private ulong _playerSteamId;
        private bool _avatarReceived = false;
        private bool _ready;

        public int ConnectionId => _connectionId;
        
        private void Start()
        {
            Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
        }

        public void Init(string playerName, int connId, ulong steamId, bool ready)
        {
            _playerName = playerName;
            _connectionId = connId;
            _playerSteamId = steamId;
            _ready = ready;
        }

        public void Init(string playerName, bool ready)
        {
            _playerName = playerName;
            _ready = ready;
        }
        
        public void UpdateInfo()
        {
            _displayNicknameText.text = _playerName;
            ChangePlayerStatus();
            if(!_avatarReceived) GetPlayerIcon();
        }

        private void ChangePlayerStatus() => 
            _displayStatusImage.color = _ready ? _readyColor : _notReadyColor;
        
        

        private void GetPlayerIcon()
        {
            var imageId = SteamFriends.GetLargeFriendAvatar((CSteamID)_playerSteamId);
            if(imageId == -1) return;
            _displayProfileImage.texture = GetSteamImageAsTexture(imageId);
        }
        
        private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
        {
            if(callback.m_steamID.m_SteamID != _playerSteamId) return;
            _displayProfileImage.texture = GetSteamImageAsTexture(callback.m_iImage);
        }
        
        private Texture2D GetSteamImageAsTexture(int imageId)
        {
            Texture2D texture = null;
            var isValid = SteamUtils.GetImageSize(imageId, out var width, out var height);
            if (isValid)
            {
                var image = new byte[width * height * 4];
                isValid = SteamUtils.GetImageRGBA(imageId, image, (int)(width * height * 4));
                if (isValid)
                {
                    texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                    texture.LoadRawTextureData(image);
                    texture.Apply();
                }
            }
            _avatarReceived = true;
            return texture;
        }
    }
}