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
        
        [HideInInspector] public string playerName;
        [HideInInspector] public int connectionId;
        [HideInInspector] public ulong playerSteamId;
        [HideInInspector] public bool avatarReceived = false;
        [HideInInspector] public bool ready;
        
        private void Start()
        {
            Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
        }
       
        public void Init()
        {
            _displayNicknameText.text = playerName;
            ChangePlayerStatus();
            if(!avatarReceived) GetPlayerIcon();
        }

        private void ChangePlayerStatus() => 
            _displayStatusImage.color = ready ? _readyColor : _notReadyColor;
        
        

        private void GetPlayerIcon()
        {
            var imageId = SteamFriends.GetLargeFriendAvatar((CSteamID)playerSteamId);
            if(imageId == -1) return;
            _displayProfileImage.texture = GetSteamImageAsTexture(imageId);
        }
        
        private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
        {
            if(callback.m_steamID.m_SteamID != playerSteamId) return;
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
            avatarReceived = true;
            return texture;
        }
    }
}