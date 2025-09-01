using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Menu.View.SnapControllers
{
    public class SnapController : MonoBehaviour
    {
        [Header("Настройки магнита")]
        [SerializeField] protected float snapSpeed = 6f;
        
        [Header("Уже созданный контент для работы магнита")]
        [SerializeField] protected List<RectTransform> followedContent;

        [Header("Зависимости")]
        [SerializeField] protected RectTransform contentContainer;
        [SerializeField] protected RectTransform viewPort;

        protected virtual int StartTargetElementIndex => 0;
        
        public IReadOnlyReactiveProperty<int> TargetElementIndexRP => targetElementIndex;
        protected ReactiveProperty<int> targetElementIndex = new();

        protected List<RectTransform> scrollElements = new ();
        protected bool isInitialized;

        private void Start()
        {
            if (followedContent == null || followedContent.Count == 0)
                return;
            
            foreach (var element in followedContent)
            {
                FollowElement(element);
            }
        }

        public virtual void FollowElement(RectTransform element)
        {
            if (element == null)
                return;
            
            scrollElements.Add(element);

            if (!isInitialized)
            {
                isInitialized = true;
            }
        }

        protected IEnumerator SmoothSnapToElement(int targetElementIndex)
        {
            Vector2 targetPosition = GetContentContainerTargetPosition(targetElementIndex);
            Vector2 startPosition = contentContainer.anchoredPosition;
            
            float elapsedTime = 0f;

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * snapSpeed;
                contentContainer.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, elapsedTime);
                yield return null;
            }

            contentContainer.anchoredPosition = targetPosition;
            this.targetElementIndex.Value = targetElementIndex;
        }

        private void OnEnable()
        {
            if (isInitialized)
                StartCoroutine(StartCenteringOnTargetElement());
        }

        private Vector2 GetContentContainerTargetPosition(int targetElementIndex)
        {
            if (targetElementIndex < 0 || targetElementIndex >= scrollElements.Count)
                throw new ArgumentException("Некорректный индекс");

            RectTransform targetElement = scrollElements[targetElementIndex];
            if (targetElement == null) throw new Exception("Целевой элемент равен null");
            
            Vector2 targetWorldCenter =
                targetElement.TransformPoint(new Vector2(targetElement.rect.center.x, targetElement.rect.center.y));
            Vector2 viewportWorldCenter =
                viewPort.TransformPoint(new Vector2(viewPort.rect.center.x, viewPort.rect.center.y));

            Vector2 worldDiff = targetWorldCenter - viewportWorldCenter;
            Vector2 localDiff = viewPort.InverseTransformVector(worldDiff);

            return new Vector2(contentContainer.anchoredPosition.x - localDiff.x, contentContainer.anchoredPosition.y);
        }

        // Может выдавать ArgumentException("Некорректный индекс"). Не критичный момент
        private IEnumerator StartCenteringOnTargetElement()
        {
            yield return null;
            contentContainer.anchoredPosition = 
                GetContentContainerTargetPosition(StartTargetElementIndex);

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer);
            yield return null;

            targetElementIndex.Value = StartTargetElementIndex;
        }
    }
}