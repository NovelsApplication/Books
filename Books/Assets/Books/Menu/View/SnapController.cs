using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Books.Menu.View
{
    public class SnapController : MonoBehaviour
    {
        [Header("Зависимости")]
        [SerializeField] private ScrollRectNested _scrollRectNested;
        [SerializeField] private RectTransform _contentContainer;
        [SerializeField] private RectTransform _viewPort;
        
        [Header("Настройки магнита")]
        [SerializeField] private float _scrollSensitivity = 0.3f;
        [SerializeField] private float _snapSpeed = 10f;

        public IReadOnlyReactiveProperty<int> TargetElementIndexRP => _targetElementIndex;
        private ReactiveProperty<int> _targetElementIndex = new(1);

        private List<RectTransform> _scrollElements = new ();
        private bool _isInitialized = false;

        public void Initialize()
        {
            for (int i = 0; i < _contentContainer.childCount - 1; i++)
            {
                Transform element = _contentContainer.GetChild(i);
                if (element.TryGetComponent(out ScreenBook screenBook) && element.gameObject.activeSelf)
                {
                    _scrollElements.Add(screenBook.GetComponent<RectTransform>());
                }
            }
            
            _scrollRectNested.OnEndDragEvent += OnEndDrag;
            _isInitialized = true;
        }

        private void OnEnable()
        {
            if (_isInitialized)
                StartCoroutine(CenteringOnTargetElement());
        }

        private IEnumerator CenteringOnTargetElement()
        {
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_contentContainer);
            yield return null;

            var anchoredPosition = GetContentContainerTargetPosition(_targetElementIndex.Value);

            _contentContainer.anchoredPosition = anchoredPosition;
        }

        private void OnEndDrag(PointerEventData eventData)
        {
            float dragDistance = eventData.position.x - eventData.pressPosition.x;

            int targetIndex = _targetElementIndex.Value;

            if (Mathf.Abs(dragDistance) / UnityEngine.Screen.width < _scrollSensitivity)
            {
                StartCoroutine(SnapToElement(targetIndex));
                return;
            }

            if (dragDistance < 0)
            {
                targetIndex = Mathf.Min(targetIndex + 1, _scrollElements.Count - 1);
            }
            else
            {
                targetIndex = Mathf.Max(targetIndex - 1, 0);
            }

            StartCoroutine(SnapToElement(targetIndex));
        }

        private IEnumerator SnapToElement(int targetElementIndex)
        {
            Vector2 targetPosition = GetContentContainerTargetPosition(targetElementIndex);
            Vector2 startPosition = _contentContainer.anchoredPosition;
            
            float elapsedTime = 0f;

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * _snapSpeed;
                _contentContainer.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, elapsedTime);
                yield return null;
            }

            _contentContainer.anchoredPosition = targetPosition;
            _targetElementIndex.Value = targetElementIndex;
        }

        private Vector2 GetContentContainerTargetPosition(int targetElementIndex)
        {
            if (targetElementIndex < 0 || targetElementIndex >= _scrollElements.Count)
                throw new ArgumentException("Некорректный индекс");

            RectTransform targetElement = _scrollElements[targetElementIndex];
            if (targetElement == null) throw new Exception("Целевой элемент равен null");
            
            Vector2 targetWorldCenter =
                targetElement.TransformPoint(new Vector2(targetElement.rect.center.x, targetElement.rect.center.y));
            Vector2 viewportWorldCenter =
                _viewPort.TransformPoint(new Vector2(_viewPort.rect.center.x, _viewPort.rect.center.y));

            Vector2 worldDiff = targetWorldCenter - viewportWorldCenter;
            Vector2 localDiff = _viewPort.InverseTransformVector(worldDiff);

            return new Vector2(_contentContainer.anchoredPosition.x - localDiff.x, _contentContainer.anchoredPosition.y);
        }

        private void OnDestroy()
        {
            _scrollRectNested.OnEndDragEvent -= OnEndDrag;
        }
    }
}