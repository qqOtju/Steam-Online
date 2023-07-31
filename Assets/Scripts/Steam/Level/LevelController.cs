using UnityEngine;

namespace Steam.Level
{
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private Connection[] _connection;
        
        private void Awake()
        {
            foreach (var connection in _connection)
            {
                var conn = connection;
                foreach (var trigger in conn.triggers)
                {
                    trigger.Register(value =>
                    {
                        if (value) conn.currentTriggers++;
                        else conn.currentTriggers--;
                        if(conn.currentTriggers == conn.triggersCount)
                            conn.events?.Invoke();
                    });
                }
            }
        }
    }
}