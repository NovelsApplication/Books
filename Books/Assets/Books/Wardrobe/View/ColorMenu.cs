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

        private bool _isOpen;
        private ColorSelectorView[] _objects;

        private void Start()
        {
            _menuButton.onClick.AddListener(OnMenuButtonClick);
            _colorSelectorPrefab.gameObject.SetActive(false);
            _containerTransform.gameObject.SetActive(true);
        }

        public async void InitColors(Sprite[] colors, Action<int> onColorSelectAction, int currentColorInx = 0)
        {
            Clear();
            
            if (colors.Length <= 1 || Array.Exists(colors, i => i == null))
            {
                HideImmediate();
                return;
            }
            
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
            
            ShowImmediate();
            await UniTask.Yield();
            UpdateAnimationPositions();
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

        private void OnMenuButtonClick()
        {
            _isOpen = !_isOpen;
            if (_isOpen)
                ShowColorsMenu().Forget();
            else
                HideColorsMenu().Forget();
        }

        // public async UniTask Show()
        // {
        //     await ShowColorsMenu();
        //     _containerTransform.gameObject.SetActive(true);
        //     _menuButton.gameObject.SetActive(true);
        // }
        //
        // public async UniTask Hide()
        // {
        //     await HideColorsMenu();
        //     _containerTransform.gameObject.SetActive(false);
        //     _menuButton.gameObject.SetActive(false);
        // }

        private async UniTask ShowColorsMenu()
        {
            //_isOpen = true;
            await _animation.Open();
        }

        private async UniTask HideColorsMenu()
        {
           // _isOpen = false;
            await _animation.Close();
        }

        private void UpdateAnimationPositions()
        {
            var animOpenPosition = _animation.OpenPos;
            var animClosePosition = new Vector2(animOpenPosition.x, animOpenPosition.y - _containerTransform.rect.height);

            if (_isOpen) _containerTransform.anchoredPosition = animOpenPosition;
            else _containerTransform.anchoredPosition = animClosePosition;

            _animation.ClosePos = animClosePosition;
        }

        public void ShowImmediate()
        {
            //_containerTransform.anchoredPosition = _animation.OpenPos;
            _containerTransform.gameObject.SetActive(true);
            _menuButton.gameObject.SetActive(true);
            //_isOpen = true;
        }

        public void HideImmediate()
        {
            //_containerTransform.anchoredPosition = _animation.ClosePos;
            _containerTransform.gameObject.SetActive(false);
            _menuButton.gameObject.SetActive(false);
            //_isOpen = false;
        }
    }
}