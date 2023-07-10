using Mirror;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Steam
{
    public class PlayerInfoDisplay : NetworkBehaviour
    {
        [SerializeField] private RawImage _profileImage = null;
        [SerializeField] private TextMeshProUGUI _displayNameText = null;

        [SyncVar(hook = nameof(HandleSteamIdUpdated))]
        private ulong _steamId;

        protected Callback<AvatarImageLoaded_t> avatarImageLoaded;

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
            avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
        }

        private void HandleSteamIdUpdated(ulong oldId, ulong newId)
        {
            var cSteamId = new CSteamID(newId);
            _displayNameText.text = SteamFriends.GetFriendPersonaName(cSteamId);
            var imageId = SteamFriends.GetLargeFriendAvatar(cSteamId);
            if (imageId == -1) return;
            _profileImage.texture = GetSteamImageAsTexture(imageId);
        }

        private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
        {
            if(callback.m_steamID.m_SteamID != _steamId) return;
            _profileImage.texture = GetSteamImageAsTexture(callback.m_iImage);
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

        #endregion
    }
}