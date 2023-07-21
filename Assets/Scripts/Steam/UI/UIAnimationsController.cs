using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steam.UI
{
    public class UIAnimationsController : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _buttons;
        [SerializeField] private float _animTime = 0.15f;
        [SerializeField] private float _difference = 0.07f;

        private readonly Dictionary<GameObject, ButtonAnim> _buttonStats = new();
        private WaitForSeconds _wfs;

        private void Start()
        {
            _wfs = new(_animTime * 2);
            
            foreach (var btn in _buttons)
            {
                var rectTransform = btn.transform.GetChild(1).GetComponent<RectTransform>();
                _buttonStats.Add(btn, new ButtonAnim()
                {
                    rectTransform = rectTransform,
                    baseAnchorMax = rectTransform.anchorMax,
                    baseAnchorMin = rectTransform.anchorMin,
                    clickedAnchorMax = rectTransform.anchorMax - new Vector2(0,_difference),
                    clickedAnchorMin =  rectTransform.anchorMin - new Vector2(0,_difference),
                    interactable = true
                });
            }
        }

        public void BtnPress(GameObject btn)
        {
            StartCoroutine(Animate(btn));
        }
        
        private IEnumerator Animate(GameObject btn)
        {
            var info = _buttonStats[btn];
            if(!info.interactable) yield break;
            info.interactable = false;
            LeanTween.value(0, 1, _animTime).setOnUpdate(value =>
            {
                info.rectTransform.anchorMax = new Vector2(info.rectTransform.anchorMax.x,
                    Mathf.Lerp(info.baseAnchorMax.y,  info.clickedAnchorMax.y, value));
                info.rectTransform.anchorMin = new Vector2(info.rectTransform.anchorMin.x,
                    Mathf.Lerp(info.baseAnchorMin.y, info.clickedAnchorMin.y, value));
            }).setOnComplete(_ =>
            {
                LeanTween.value(0, 1, _animTime).setOnUpdate(value =>
                {
                    info.rectTransform.anchorMax = new Vector2( info.rectTransform.anchorMax.x,
                        Mathf.Lerp(info.clickedAnchorMax.y, info.baseAnchorMax.y, value));
                    info.rectTransform.anchorMin = new Vector2( info.rectTransform.anchorMin.x,
                        Mathf.Lerp(info.clickedAnchorMin.y, info.baseAnchorMin.y, value));
                }).setOnComplete(_ =>
                {
                    info.interactable = true;
                });
            });
            yield return _wfs;
        }
    }

    public struct ButtonAnim
    {
        public RectTransform rectTransform;
        public Vector2 baseAnchorMax;
        public Vector2 baseAnchorMin;
        public Vector2 clickedAnchorMax;
        public Vector2 clickedAnchorMin;
        public bool interactable;
    }
}