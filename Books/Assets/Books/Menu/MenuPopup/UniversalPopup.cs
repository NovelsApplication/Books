using System;
using System.Collections.Generic;
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
        private IPopupContent _popupContent; 
        private Tween _fadeTween;

        private UniversalPopup _root;
        
        public static (UniversalPopup root, IPopupContent content) OpenPopup(PopupType popupType, UniversalPopup popupRoot, List<Data> popupData, IPopupContentData data = null)
        {
            Data popupConfig = popupData.Find(i => i.PopupType == popupType);

            if (popupConfig.PopupContentPrefab == null)
            {
                throw new Exception($"Не найдет попап с типом [{popupType}] в данных");
            }

            RectTransform contentTransform = Instantiate(popupConfig.PopupContentPrefab);
            IPopupContent content = contentTransform.GetComponent<IPopupContent>();

            var newPopupRoot = Instantiate(popupRoot, popupRoot.transform.parent);
            
            newPopupRoot.SetPopupContent(contentTransform, content);
            
            if (data != null)
                content.Configure(data, newPopupRoot, () => OpenPopup(popupType, popupRoot, popupData, data));
            
            newPopupRoot.Show().Forget();
            
            return (newPopupRoot, content);
        }

        private void SetPopupContent(RectTransform popupTransform, IPopupContent content, IPopupContentData data = null)
        {
            ClearPopupContent();

            popupTransform.SetParent(transform, false);

            _backgroundTransform.sizeDelta = popupTransform.rect.size;
            _backgroundTransform.anchoredPosition = popupTransform.anchoredPosition;

            _popupContentTransform = popupTransform;
            _popupContent = content;
        }

        private void ClearPopupContent()
        {
            if (_popupContentTransform == null)
                return;

            _popupContent.ClearContent();
            
            _popupContentTransform.SetParent(null);
            Destroy(_popupContentTransform.gameObject);

            _popupContentTransform = null;
            _popupContent = null;
        }

        public async UniTask Show() 
        {
            _canvasGroup.alpha = 0f;
            // так пришлось
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
            Destroy(gameObject);
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
            Destroy(gameObject);
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