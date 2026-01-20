using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Books.Wardrobe.View
{
    public class SwipeDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private bool _horizontal;
        [SerializeField] private bool _vertical;
        [Range(0, 1)] [SerializeField] private float _sensitivityX = 0.2f;
        [Range(0, 1)] [SerializeField] private float _sensitivityY = 0.2f;

        public event Action<int> OnHorizontalSwipe;
        public event Action<int> OnVerticalSwipe;

        private Vector2 _startPosition;
        private Vector2 _endPosition;

        private float _pastDeltaX;
        private float _pastDeltaY;

        private bool _isHorizontalSwipeValid;
        private bool _isVerticalSwipeValid;

        public void OnPointerDown(PointerEventData eventData)
        {
            _isHorizontalSwipeValid = _horizontal;
            _isVerticalSwipeValid = _vertical;
            
            _pastDeltaX = 0;
            _pastDeltaY = 0;
            
            _startPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_horizontal)
                UpdateHorizontalSwipe(eventData);
            
            if (_vertical)
                UpdateVerticalSwipe(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _endPosition = eventData.position;
            Vector2 resultVector = _endPosition - _startPosition;

            if (Math.Abs(resultVector.x) / UnityEngine.Screen.width < _sensitivityX)
                _isHorizontalSwipeValid = false;
            
            if (Math.Abs(resultVector.y) / UnityEngine.Screen.height < _sensitivityY)
                _isVerticalSwipeValid = false;

            //Debug.Log("Вертикальный - " + _isVerticalSwipeValid + " ; Горизонтальный - " + _isHorizontalSwipeValid);

            if (_isHorizontalSwipeValid)
            {
                int direction = resultVector.x > 0 ? 1 : -1;
                OnHorizontalSwipe?.Invoke(direction);
            }

            if (_isVerticalSwipeValid)
            {
                int direction = resultVector.y > 0 ? 1 : -1;
                OnVerticalSwipe?.Invoke(direction);
            }
        }

        private void UpdateHorizontalSwipe(PointerEventData eventData)
        {
            Vector2 delta = eventData.delta;
            
            if (Math.Abs(delta.x) < Math.Abs(delta.y))
                _isHorizontalSwipeValid = false;
            
            if (_pastDeltaX * delta.x < 0)
                _isHorizontalSwipeValid = false;

            _pastDeltaX = delta.x;
        }

        private void UpdateVerticalSwipe(PointerEventData eventData)
        {
            Vector2 delta = eventData.delta;
            
            if (Math.Abs(delta.y) < Math.Abs(delta.x))
                _isVerticalSwipeValid = false;
            
            if (_pastDeltaY * delta.y < 0)
                _isVerticalSwipeValid = false;

            _pastDeltaY = delta.y;
        }

        public void ClearAllSubscribers()
        {
            OnHorizontalSwipe = null;
            OnVerticalSwipe = null;
        }
    }
}