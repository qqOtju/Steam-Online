using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Steam.Level
{
    [CreateAssetMenu(menuName = "Level/Level Info", fileName = "Level Info")]
    public class SOLevelInfo : ScriptableObject
    {
        [SerializeField] private Sprite _levelImage;
        [SerializeField] private string _levelName;
        [SerializeField] [Scene] private string _levelScene;

        public Sprite LevelImage => _levelImage;
        public string LevelName => _levelName;
        public string LevelScene => _levelScene;
    }
}