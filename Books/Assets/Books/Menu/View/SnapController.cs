using System;
using System.Collections;
using System.Collections.Generic;
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

        private List<RectTransform> _scrollElements = new ();
        private int _targetElementIndex = 1;
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
            
            Debug.Log("Инициализация прошла");
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

            if (_scrollElements.Count <= _targetElementIndex)
            {
                Debug.LogWarning("Not enough children in content container");
                yield break;
            }

            RectTransform targetElement = _scrollElements[_targetElementIndex];

            Vector3 elementWorldCenter =
                targetElement.TransformPoint(new Vector3(targetElement.rect.center.x, targetElement.rect.center.y, 0));
            Vector3 viewportWorldCenter =
                _viewPort.TransformPoint(new Vector3(_viewPort.rect.center.x, _viewPort.rect.center.y, 0));

            Vector3 worldDiff = elementWorldCenter - viewportWorldCenter;
            Vector3 localDiff = _viewPort.InverseTransformVector(worldDiff);

            var anchoredPosition = _contentContainer.anchoredPosition;
            anchoredPosition = new Vector2(anchoredPosition.x - localDiff.x, anchoredPosition.y);

            _contentContainer.anchoredPosition = anchoredPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            float dragDistance = eventData.position.x - eventData.pressPosition.x;

            int targetIndex = _targetElementIndex;

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
            if (targetElementIndex < 0 || targetElementIndex >= _scrollElements.Count)
                yield break;

            RectTransform targetElement = _scrollElements[targetElementIndex] as RectTransform;
            if (targetElement == null) yield break;

            //int currentCenterElement = FindClosestElementToCenter();

            Vector3 elementWorldCenter =
                targetElement.TransformPoint(new Vector3(targetElement.rect.center.x, targetElement.rect.center.y, 0));
            Vector3 viewportWorldCenter =
                _viewPort.TransformPoint(new Vector3(_viewPort.rect.center.x, _viewPort.rect.center.y, 0));

            Vector3 worldDiff = elementWorldCenter - viewportWorldCenter;
            Vector3 localDiff = _contentContainer.parent.InverseTransformVector(worldDiff);

            var anchoredPosition = _contentContainer.anchoredPosition;
            Vector2 targetPosition = new Vector2(anchoredPosition.x - localDiff.x, anchoredPosition.y);
            Vector2 startPosition = anchoredPosition;
            
            float elapsedTime = 0f;

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * _snapSpeed;
                _contentContainer.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, elapsedTime);
                yield return null;
            }

            _contentContainer.anchoredPosition = targetPosition;
            _targetElementIndex = targetElementIndex;
        }

        private int FindClosestElementToCenter()
        {
            float closestDistance = float.MaxValue;
            int closestIndex = 0;

            for (int i = 0; i < _scrollElements.Count; i++)
            {
                RectTransform element = _scrollElements[i];
                if (element == null) continue;

                Vector3 elementWorldCenter =
                    element.TransformPoint(new Vector3(element.rect.center.x, element.rect.center.y, 0));
                Vector3 viewportWorldCenter =
                    _viewPort.TransformPoint(new Vector3(_viewPort.rect.center.x, _viewPort.rect.center.y, 0));

                float distance = Vector3.Distance(elementWorldCenter, viewportWorldCenter);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        private void OnDestroy()
        {
            _scrollRectNested.OnEndDragEvent -= OnEndDrag;
        }
    }
}