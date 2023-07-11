using Mirror;

namespace Steam
{
    public class PlayerController : NetworkBehaviour
    {
        public override void OnStartClient()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}