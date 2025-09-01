using UnityEngine;
using UnityEngine.EventSystems;

namespace Books.Menu.View.SnapControllers
{
    public class ScrollSnapController : SnapController
    {
        [SerializeField] private ScrollRectNested _scrollRectNested;

        [Header("Настройка магнита - скролла")]
        [SerializeField] private float _scrollSensitivity = 0.3f;
        
        public override void FollowElement(RectTransform element)
        {
            if (element == null)
                return;
            
            scrollElements.Add(element);

            if (!isInitialized)
            {
                _scrollRectNested.OnEndDragEvent += OnEndDrag;
                isInitialized = true;
            }
        }

        private void OnEndDrag(PointerEventData eventData)
        {
            float dragDistance = eventData.position.x - eventData.pressPosition.x;

            int targetIndex = targetElementIndex.Value;

            if (Mathf.Abs(dragDistance) / UnityEngine.Screen.width < _scrollSensitivity)
            {
                StartCoroutine(SmoothSnapToElement(targetIndex));
                return;
            }

            if (dragDistance < 0)
            {
                targetIndex = Mathf.Min(targetIndex + 1, scrollElements.Count - 1);
            }
            else
            {
                targetIndex = Mathf.Max(targetIndex - 1, 0);
            }

            StartCoroutine(SmoothSnapToElement(targetIndex));
        }

        private void OnDestroy()
        {
            _scrollRectNested.OnEndDragEvent -= OnEndDrag;
        }
    }
}