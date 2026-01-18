using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Books.Wardrobe.View
{
    [RequireComponent(typeof(RectTransform))]
    public class OpenClose_Animation : MonoBehaviour
    {
        [SerializeField] private float _animationDuration = 1f;
        [SerializeField] private RectTransform _openPoint;
        [SerializeField] private RectTransform _closePoint;

        public Vector2 OpenPos
        {
            get { return _openPos; }
            set { _openPos = value; }
        }

        public Vector2 ClosePos
        {
            get { return _closePos; }
            set { _closePos = value; }
        }
        
        private RectTransform _currentRectTransform;
        private CancellationTokenSource _cancellationTokenSource = null;

        private Vector2 _openPos = Vector2.zero;
        private Vector2 _closePos = Vector2.zero;

        private void Start()
        {
            _currentRectTransform = GetComponent<RectTransform>();

            if (_openPoint != null) _openPos = _openPoint.anchoredPosition;
            if (_closePoint != null) _closePos = _closePoint.anchoredPosition;
        }

        public async UniTask Open()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
            
            _cancellationTokenSource = new CancellationTokenSource();
            
            await AnimateToRectTransform(_openPos, _cancellationTokenSource.Token);
        }

        public async UniTask Close()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
            
            _cancellationTokenSource = new CancellationTokenSource();
            
            await AnimateToRectTransform(_closePos, _cancellationTokenSource.Token);
        }

        public bool IsOpen()
        {
            return IsPositionsEqual(_currentRectTransform.anchoredPosition, _openPos);
        }

        public bool IsAnimating()
        {
            return _cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested;
        }

        private async UniTask AnimateToRectTransform(Vector2 targetPosition, CancellationToken cancellationToken)
        {
            Vector2 startPosition = _currentRectTransform.anchoredPosition;
            
            if (IsPositionsEqual(startPosition, targetPosition))
            {
                return;
            }
            
            float elapsedTime = 0f;
            while (elapsedTime < _animationDuration)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / _animationDuration);

                Vector2 interpolatedPos = Vector2.Lerp(startPosition, targetPosition, progress);
                _currentRectTransform.anchoredPosition = interpolatedPos;

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            _currentRectTransform.anchoredPosition = targetPosition;
        }

        private bool IsPositionsEqual(Vector2 v1, Vector2 v2)
        {
            return Vector2.Distance(v1, v2) < 0.01f;
        }
    }
}