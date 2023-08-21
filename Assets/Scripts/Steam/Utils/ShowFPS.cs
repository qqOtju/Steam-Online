using UnityEngine;

namespace Steam.Utils
{
    public class ShowFPS : MonoBehaviour {
 
        private static float _fps;
        private void Awake()
        {
            Application.targetFrameRate = 120;
        }

        void OnGUI()
        {
            _fps = 1.0f / Time.deltaTime;
            GUI.skin.label.fontSize = 40;
            GUILayout.Label("FPS: " + (int)_fps);
        }
    }
}