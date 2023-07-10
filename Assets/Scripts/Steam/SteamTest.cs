using Steamworks;
using UnityEngine;

namespace Steam
{
    public class SteamTest : MonoBehaviour
    {
        private void Start()
        {
            if(!SteamManager.Initialized) return;

            var name = SteamFriends.GetPersonaName();
            Debug.Log(name);
        }
    }
}