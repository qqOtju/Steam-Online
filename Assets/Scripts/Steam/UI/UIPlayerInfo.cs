using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Steam.UI
{
    public class UIPlayerInfo : MonoBehaviour
    {
        [SerializeField] private RawImage _displayProfileImage;
        [SerializeField] private TextMeshProUGUI _displayNicknameText;
        [SerializeField] private TextMeshProUGUI _displayStatusText;

        public string playerName;
        public int connectionId;
        public ulong playerSteamId;
        public bool avatarReceived = false;
        public bool ready;
        
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

        public void ChangePlayerStatus() =>
            _displayStatusText.text = ready ? "<color=green>Ready</color>" : "<color=red>Not ready</color>";
        

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

        /*[SyncVar(hook = nameof(HandleSteamIdUpdated))]
        private ulong _steamId;

        
        #region Server

        public void SetSteamId(ulong steamId)
        {
            this._steamId = steamId;
        }

        #endregion

        #region Client

        public override void OnStartClient()
        {
            base.OnStartClient();
            Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
        }

        private void HandleSteamIdUpdated(ulong oldId, ulong newId)
        {
            var cSteamId = new CSteamID(newId);
            _displayNicknameText.text = SteamFriends.GetFriendPersonaName(cSteamId);
            var imageId = SteamFriends.GetLargeFriendAvatar(cSteamId);
            if (imageId == -1) return;
            _displayProfileImage.texture = GetSteamImageAsTexture(imageId);
        }

        private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
        {
            if(callback.m_steamID.m_SteamID != _steamId) return;
            _displayProfileImage.texture = GetSteamImageAsTexture(callback.m_iImage);
        }
        
        private Texture2D GetSteamImageAsTexture(int imageId)
        {
            Texture2D texture = null;
            bool isValid = SteamUtils.GetImageSize(imageId, out var width, out var height);
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
            return texture;
        }

        #endregion*/

    }
}