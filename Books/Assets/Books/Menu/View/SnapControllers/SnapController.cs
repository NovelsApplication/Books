using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Books.Menu.View.SnapControllers
{
    public class SnapController : MonoBehaviour
    {
        [Header("Настройки магнита")]
        [SerializeField] protected float snapSpeed = 6f;

        [Header("Зависимости")]
        [SerializeField] protected RectTransform contentContainer;
        [SerializeField] protected RectTransform viewPort;
        
        public IReadOnlyReactiveProperty<int> TargetElementIndexRP => targetElementIndex;
        protected ReactiveProperty<int> targetElementIndex = new(0);

        protected List<RectTransform> scrollElements = new ();
        protected bool isInitialized;

        private void OnEnable()
        {
            if (isInitialized)
                InstantlyCenteringOnElement(targetElementIndex.Value);
        }

        public void InstantlyCenteringOnElement(int index)
        {
            StartCoroutine(SmoothSnapToElement(index, true));
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

        protected IEnumerator SmoothSnapToElement(int index, bool isInstantly = false)
        {
            Vector2 targetPosition = GetContentContainerTargetPosition(index);
            Vector2 startPosition = contentContainer.anchoredPosition;
            
            if (!isInstantly)
            {
                float elapsedTime = 0f;
                
                while (elapsedTime < 1f)
                {
                    elapsedTime += Time.deltaTime * snapSpeed;
                    contentContainer.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, elapsedTime);
                    yield return null;
                }
            }

            contentContainer.anchoredPosition = targetPosition;
            targetElementIndex.Value = index;
        }

        private Vector2 GetContentContainerTargetPosition(int index)
        {
            if (index < 0 || index >= scrollElements.Count)
                throw new ArgumentException("Некорректный индекс");

            RectTransform targetElement = scrollElements[index];
            if (targetElement == null) throw new Exception("Целевой элемент равен null");
            
            Vector2 targetWorldCenter =
                targetElement.TransformPoint(new Vector2(targetElement.rect.center.x, targetElement.rect.center.y));
            Vector2 viewportWorldCenter =
                viewPort.TransformPoint(new Vector2(viewPort.rect.center.x, viewPort.rect.center.y));

            Vector2 worldDiff = targetWorldCenter - viewportWorldCenter;
            Vector2 localDiff = viewPort.InverseTransformVector(worldDiff);

            return new Vector2(contentContainer.anchoredPosition.x - localDiff.x, contentContainer.anchoredPosition.y);
        }

        // private IEnumerator CenteringOnElementCoroutine(int index)
        // {
        //     yield return null;
        //     contentContainer.anchoredPosition = 
        //         GetContentContainerTargetPosition(index);
        //
        //     LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer);
        //     yield return null;
        //
        //     targetElementIndex.Value = index;
        // }
    }
}