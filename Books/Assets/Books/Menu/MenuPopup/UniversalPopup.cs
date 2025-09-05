using System;
using Books.Menu.MenuPopup.Contents;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Menu.MenuPopup
{
    public class UniversalPopup : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _backgroudButton;
        [SerializeField] private RawImage _backgroundImage;
        [SerializeField] private RectTransform _backgroundTransform;
        [SerializeField] private float _showHideDuration;

        private RectTransform _popupContentTransform;
        private IPopupContent _popupContent; // Для оптимизации. Чтобы не вызывать лишний GetComp.
        private Tween _fadeTween;

        public void SetPopupContent(RectTransform popupTransform, IPopupContent content = null)
        {
            ClearPopupContent();

            popupTransform.SetParent(transform);

            _backgroundTransform.sizeDelta = popupTransform.rect.size;
            _backgroundTransform.anchoredPosition = popupTransform.anchoredPosition;

            _popupContentTransform = popupTransform;
            _popupContent = content;
        }

        public void ClearPopupContent()
        {
            if (_popupContentTransform == null)
                return;
            
            if (_popupContent == null)
                _popupContent = _popupContentTransform.GetComponent<IPopupContent>();
            
            _popupContent.ClearContent();
            
            _popupContentTransform.SetParent(null);
            Destroy(_popupContentTransform.gameObject);
            
            _popupContentTransform = null;
            _popupContent = null;
        }

        public async UniTask Show() 
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(true);
            await UniTask.Delay(50);
            _canvasGroup.gameObject.SetActive(false);
            await UniTask.Delay(50);
            _canvasGroup.gameObject.SetActive(true);

            _fadeTween?.Kill();
            _fadeTween = _canvasGroup.DOFade(1f, _showHideDuration).SetUpdate(true);
            await _fadeTween.AsyncWaitForCompletion();
        }

        public async UniTask Hide()
        {
            _fadeTween?.Kill();
            
            _canvasGroup.alpha = 1f;
            _fadeTween = _canvasGroup.DOFade(0f, _showHideDuration).SetUpdate(true);
            await _fadeTween.AsyncWaitForCompletion();
            
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

        public void SetBackgroundTexture(Texture2D texture) 
        {
            if (texture != null)
                _backgroundImage.texture = texture;
        }
    }
}