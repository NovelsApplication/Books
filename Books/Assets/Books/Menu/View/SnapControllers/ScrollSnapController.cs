using UnityEngine;
using UnityEngine.EventSystems;

namespace Books.Menu.View.SnapControllers
{
    public class ScrollSnapController : SnapController
    {
        [SerializeField] private ScrollRectNested _scrollRectNested;

        [Header("Настройка магнита - скролла")]
        [SerializeField] private float _scrollSensitivity = 0.3f;
        
        protected override void OnFollowElement(RectTransform element, int index)
        {
            if (!_isInitialized)
            {
                _scrollRectNested.OnEndDragEvent += OnEndDrag;
            }
        }

        private void OnEndDrag(PointerEventData eventData)
        {
            float dragDistance = eventData.position.x - eventData.pressPosition.x;

            int targetIndex = _targetElementIndex.Value;

            if (Mathf.Abs(dragDistance) / UnityEngine.Screen.width < _scrollSensitivity)
            {
                StartCoroutine(SmoothSnapToElement(targetIndex));
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

            StartCoroutine(SmoothSnapToElement(targetIndex));
        }

        private void OnDestroy()
        {
            _scrollRectNested.OnEndDragEvent -= OnEndDrag;
        }
    }
}