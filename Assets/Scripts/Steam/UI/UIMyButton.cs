using GridLayout;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Steam.UI
{
    public class UIMyButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        [SerializeField] private Graphic _targetGraphic;
        [SerializeField] private AbstractGridLayout _container;
        [SerializeField] private Image _image;
        [SerializeField] private bool _interactable = true;
        [SerializeField] private UnityEvent _onClick;

        private static Color _fColor = new(0.1f, 0.1f, 0.1f, 0.7f);
        private static Color _sColor = new(0.1f, 0.1f, 0.1f, 0.5f);
        private static Color _thColor = new(0.1f, 0.1f, 0.1f, 0.3f);
        private static Color _pressColor = new(0.1f, .1f, .1f, .1f);
        private static Color[] _colors = { _fColor, _sColor, _thColor };

        private readonly GameObject[] _objects = new GameObject[_colors.Length];

        private const float FadeTime = 0.1f;

        private Color _baseColor;

        public bool Interactable
        {
            get => _interactable;
            set => _interactable = value;
        }

        private void Awake()
        {
            _baseColor = _targetGraphic.color;
            for (int i = 0; i < _colors.Length; i++)
            {
                var foo = Instantiate(_image, _container.transform);
                foo.color = _colors[i];
                foo.gameObject.SetActive(false);
                _objects[i] = foo.gameObject;
            }

            _container.Align(true);
        }

        public void OnPointerClick(PointerEventData eventData) => Press();

        private void Press()
        {
            if (!gameObject.activeSelf || !Interactable)
                return;
            Foo();
            _onClick?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!gameObject.activeSelf || !Interactable)
                return;
            for (int i = 0; i < _colors.Length; i++)
                _objects[i].gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!gameObject.activeSelf || !Interactable)
                return;
            for (int i = 0; i < _colors.Length; i++)
                _objects[i].gameObject.SetActive(false);
            _targetGraphic.color = _baseColor;
        }

        private void Foo()
        {
            LeanTween.value(0, 1f, FadeTime).setOnStart(() => _targetGraphic.color = _baseColor)
                .setOnUpdate(value => _targetGraphic.color = Color.Lerp(_baseColor, _pressColor, value)).setOnComplete(_ =>
                    {
                        _targetGraphic.color = _pressColor;
                        LeanTween.value(0, 1f, FadeTime).setOnUpdate(value =>
                        {
                            if (_targetGraphic != null)
                                _targetGraphic.color = Color.Lerp(_pressColor, _baseColor, value);
                        }).setOnComplete(_ =>
                        {
                            if (_targetGraphic != null)
                                _targetGraphic.color = _baseColor;
                        });
                    });
        }
    }
}