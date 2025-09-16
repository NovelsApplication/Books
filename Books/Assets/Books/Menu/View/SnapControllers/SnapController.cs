using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Books.Menu.View.SnapControllers
{
    public class SnapController : MonoBehaviour
    {
        [Header("Настройки магнита")]
        [SerializeField] [FormerlySerializedAs("snapSpeed")]
        protected float _snapSpeed = 6f;

        [Header("Зависимости")]
        [SerializeField] [FormerlySerializedAs("contentContainer")]
        protected RectTransform _contentContainer;
        
        [SerializeField] [FormerlySerializedAs("viewPort")]  
        protected RectTransform _viewPort;
        
        public IReadOnlyReactiveProperty<int> TargetElementIndexRP => _targetElementIndex;
        protected ReactiveProperty<int> _targetElementIndex = new(0);

        protected readonly List<RectTransform> _scrollElements = new ();
        protected bool _isInitialized;

        private void OnEnable()
        {
            if (_isInitialized)
                InstantlyCenteringOnElement(_targetElementIndex.Value);
        }

        public void InstantlyCenteringOnElement(int index)
        {
            StartCoroutine(SmoothSnapToElement(index, true));
        }
        
        public void FollowElement(RectTransform element)
        {
            if (element == null)
                return;
            
            _scrollElements.Add(element);
            int index = _scrollElements.Count - 1;
            OnFollowElement(element, index);
            
            if (!_isInitialized)
            {
                _isInitialized = true;
            }
        }
        
        protected virtual void OnFollowElement(RectTransform element, int index) {}

        protected IEnumerator SmoothSnapToElement(int index, bool isInstantly = false)
        {
            Vector2 targetPosition = GetContentContainerTargetPosition(index);
            Vector2 startPosition = _contentContainer.anchoredPosition;
            
            if (!isInstantly)
            {
                float elapsedTime = 0f;
                
                while (elapsedTime < 1f)
                {
                    elapsedTime += Time.deltaTime * _snapSpeed;
                    _contentContainer.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, elapsedTime);
                    yield return null;
                }
            }

            _contentContainer.anchoredPosition = targetPosition;
            _targetElementIndex.Value = index;
        }

        private Vector2 GetContentContainerTargetPosition(int index)
        {
            if (index < 0 || index >= _scrollElements.Count)
                throw new ArgumentException("Некорректный индекс");

            RectTransform targetElement = _scrollElements[index];
            if (targetElement == null) throw new Exception("Целевой элемент равен null");
            
            Vector2 targetWorldCenter =
                targetElement.TransformPoint(new Vector2(targetElement.rect.center.x, targetElement.rect.center.y));
            Vector2 viewportWorldCenter =
                _viewPort.TransformPoint(new Vector2(_viewPort.rect.center.x, _viewPort.rect.center.y));

            Vector2 worldDiff = targetWorldCenter - viewportWorldCenter;
            Vector2 localDiff = _viewPort.InverseTransformVector(worldDiff);

            return new Vector2(_contentContainer.anchoredPosition.x - localDiff.x, _contentContainer.anchoredPosition.y);
        }
    }
}