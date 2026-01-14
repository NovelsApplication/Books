using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Books.Wardrobe.View
{
    public class ColorMenu : MonoBehaviour
    {
        [SerializeField] private Button _menuButton;
        [SerializeField] private ColorSelectorView _colorSelectorPrefab;
        [SerializeField] private RectTransform _containerTransform;
        
        public void Clear() => _clear?.Invoke();
        private Action _clear;
        
        private bool _isOpen;
        
        private void Start()
        {
            _menuButton.onClick.AddListener(OnMenuButtonClick);
            _colorSelectorPrefab.gameObject.SetActive(false);
        }

        public void InitColors(Sprite[] colors, Action<int> onColorSelectAction, int currentColorInx = 0)
        {
            _clear?.Invoke();
            
            if (colors.Length <= 1 || Array.Exists(colors, i => i == null))
            {
                HideImmediate();
                return;
            }

            ShowImmediate();

            var objects = new ColorSelectorView[colors.Length];
            
            for (int i = 0; i < colors.Length; i++)
            {
                var colorSelector = GameObject.Instantiate(_colorSelectorPrefab, _containerTransform);
                colorSelector.gameObject.SetActive(true);
                colorSelector.SetSprite(colors[i]);

                int contextIndex = i;
                var btn = colorSelector.GetComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    foreach (var obj in objects) 
                        obj.Select(false);
                    colorSelector.Select(true);
                    onColorSelectAction?.Invoke(contextIndex);
                });
                
                objects[i] = colorSelector;
            }
            
            foreach (var obj in objects) 
                obj.Select(false);
            objects[currentColorInx].Select(true);

            _clear = () =>
            {
                foreach (var obj in objects) 
                    Destroy(obj.gameObject);
                
                _clear = null;
            };
        }

        private void OnMenuButtonClick()
        {
            _isOpen = !_isOpen;
            if (_isOpen)
                ShowColorsMenu();
            else
                HideColorsMenu();
        }

        private void ShowColorsMenu()
        {
            _containerTransform.gameObject.SetActive(true);
        }
        
        private void HideColorsMenu()
        {
            _containerTransform.gameObject.SetActive(false);
        }

        public void ShowImmediate()
        {
            _menuButton.gameObject.SetActive(true);
            _containerTransform.gameObject.SetActive(_isOpen);
        }

        public void HideImmediate()
        {
            _menuButton.gameObject.SetActive(false);
            _containerTransform.gameObject.SetActive(false);
        }
    }
}