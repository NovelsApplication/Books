using System;
using System.Collections.Generic;
using Books.Menu.MenuPopup.Contents;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
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

        [SerializeField]
        private RectTransform _popupContentTransform;
        private IPopupContent _popupContent; 
        private Tween _fadeTween;
        
        public static (UniversalPopup root, IPopupContent content, IObservable<Unit> closeRequest) OpenPopup(PopupType popupType, UniversalPopup popupRoot, List<Data> popupConfigs, IPopupContentData data = null)
        {
            Data config = popupConfigs.Find(i => i.PopupType == popupType);

            if (config.PopupContentPrefab == null)
            {
                throw new Exception($"Не найдет попап с типом [{popupType}] в данных");
            }
            
            RectTransform contentTransform = Instantiate(config.PopupContentPrefab);
            IPopupContent content = contentTransform.GetComponent<IPopupContent>();
            
            var newPopupRoot = CreatePopupOnTop(popupRoot, popupRoot.transform.parent);

            newPopupRoot.SetPopupContent(contentTransform, content);

            IObservable<Unit> closeRequest = null;
            if (data != null)
                closeRequest = content.Configure(data, newPopupRoot, popupConfigs);
            
            newPopupRoot.Show().Forget();
            
            return (newPopupRoot, content, closeRequest);
        }

        private static UniversalPopup CreatePopupOnTop(UniversalPopup prefab, Transform root)
        {
            UniversalPopup popupComponent = Instantiate(prefab, root);
            
            // мне не очень нравится. Возможно, нужно передалать
            if (popupComponent._popupContentTransform != null)
            {
                IPopupContent content = popupComponent._popupContentTransform.GetComponent<IPopupContent>();
                popupComponent._popupContent = content;
                popupComponent.ClearPopupContent();
            }
            
            return popupComponent;
        }

        private void SetPopupContent(RectTransform popupTransform, IPopupContent content)
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