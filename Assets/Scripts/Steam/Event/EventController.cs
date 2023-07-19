using Mirror;
using UnityEngine;

namespace Steam.Event
{
    public class EventController : MonoBehaviour
    {
        [SerializeField] private Connection[] _connections;
        
        [ServerCallback]
        private void Awake()
        {
            foreach (var connection in _connections)
            foreach (var subscriber in connection.Subscribers)
                    subscriber.Subscribe(ref connection.Event.Event);
        }
    }
}