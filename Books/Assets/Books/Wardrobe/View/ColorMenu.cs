using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Wardrobe.View
{
    public class ColorMenu : MonoBehaviour
    {
        [SerializeField] private Button _menuButton;
        [SerializeField] private ColorSelectorView _colorSelectorPrefab;
        [SerializeField] private RectTransform _containerTransform;
        [SerializeField] private OpenClose_Animation _animation;
        [SerializeField] private CanvasGroup _viewPortCG;

        private bool _isOpen;
        private ColorSelectorView[] _objects;

        private void Start()
        {
            _menuButton.onClick.AddListener(OnMenuButtonClick);
            _colorSelectorPrefab.gameObject.SetActive(false);
            _containerTransform.gameObject.SetActive(true);
            _viewPortCG.alpha = 0;
        }

        public async void InitColors(Sprite[] colors, Action<int> onColorSelectAction, int currentColorInx = 0)
        {
            Clear();
            
            if (colors.Length <= 1 || Array.Exists(colors, i => i == null))
            {
                HideImmediate();
                return;
            }

            _viewPortCG.alpha = 0;
            
            _objects = new ColorSelectorView[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                var colorSelector = GameObject.Instantiate(_colorSelectorPrefab, _containerTransform);
                colorSelector.gameObject.SetActive(true);
                colorSelector.SetSprite(colors[i]);

                int contextIndex = i;
                var btn = colorSelector.GetComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    foreach (var obj in _objects) 
                        obj.Select(false);
                    colorSelector.Select(true);
                    onColorSelectAction?.Invoke(contextIndex);
                });
                
                _objects[i] = colorSelector;
            }
            
            foreach (var obj in _objects) 
                obj.Select(false);
            _objects[currentColorInx].Select(true);
            
            _menuButton.gameObject.SetActive(true);
            await UniTask.Yield();
            UpdateAnimationPositions();
            _viewPortCG.alpha = 1;
        }

        public void Clear()
        {
            if (_objects == null)
                return;

            foreach (var obj in _objects)
            {
                var btn = obj.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
            
                Destroy(obj.gameObject);
            }

            _objects = null;
        }

        public async UniTask ShowColorsMenu()
        {
            if (_isOpen) await _animation.Open();;
        }

        public async UniTask HideColorsMenu()
        {
            await _animation.Close();
        }

        public void HideImmediate()
        {
            _viewPortCG.alpha = 0;
            _containerTransform.anchoredPosition = _animation.ClosePos;
            _menuButton.gameObject.SetActive(false);
        }

        private void OnMenuButtonClick()
        {
            _isOpen = !_isOpen;
            if (_isOpen)
                ShowColorsMenu().Forget();
            else
                HideColorsMenu().Forget();
        }

        private void UpdateAnimationPositions()
        {
            var openPosition = _animation.OpenPos;
            var closePosition = new Vector2(openPosition.x, openPosition.y - _containerTransform.rect.height - 15);

            if (_isOpen) _containerTransform.anchoredPosition = openPosition;
            else _containerTransform.anchoredPosition = closePosition;

            _animation.ClosePos = closePosition;
        }
    }
}