using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Menu.View 
{
    public class PopUp : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _backgroudButton;
        [SerializeField] private RawImage _image;
        [SerializeField] private TMP_Text _headerArea;
        [SerializeField] private TMP_Text _descriptionArea;
        [SerializeField] private Button _readButton;
        [SerializeField] private float _showHideDuration;

        public async UniTask Show()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(true);
            await UniTask.Delay(50);
            _canvasGroup.gameObject.SetActive(false);
            await UniTask.Delay(50);
            _canvasGroup.gameObject.SetActive(true);

            var delayMs = 50;
            var deltaTime = delayMs / 1000f;

            var timer = _showHideDuration;
            while (timer >= 0f)
            {
                _canvasGroup.alpha = 1f - (timer / _showHideDuration);
                timer -= deltaTime;
                await UniTask.Delay(delayMs, true);
            }

            _canvasGroup.alpha = 1f;
        }

        public async UniTask Hide()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.gameObject.SetActive(true);

            var delayMs = 50;
            var deltaTime = delayMs / 1000f;

            var timer = _showHideDuration;
            while (timer >= 0f)
            {
                _canvasGroup.alpha = timer / _showHideDuration;
                timer -= deltaTime;
                await UniTask.Delay(delayMs, true);
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(false);
        }

        public void ShowImmediate() 
        {
            _canvasGroup.gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
        }

        public void HideImmediate()
        {
            _canvasGroup.gameObject.SetActive(false);
            _canvasGroup.alpha = 0f;
        }

        public void SetBackgroundButton(Action onClick) 
        {
            _backgroudButton.onClick.RemoveAllListeners();
            _backgroudButton.onClick.AddListener(onClick.Invoke);
        }

        public void SetImage(Texture2D texture) 
        {
            _image.texture = texture;
        }

        public void SetHeader(string text) 
        {
            _headerArea.text = text;
        }

        public void SetDescription(string text)
        {
            _descriptionArea.text = text;
        }

        public void SetReadButton(Action onClick)
        {
            _readButton.onClick.RemoveAllListeners();
            _readButton.onClick.AddListener(onClick.Invoke);
        }
    }
}

