using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Books.Menu.View.ParticlesView
{
    public class ParticleBehavior : MonoBehaviour
    {
        [Header("Продолжительность")] 
        [SerializeField] private float _duration = 5f;
        
        [Header("Изменение размера от текущего")]
        [Range(-1.5f, 1.5f)] [SerializeField] private float _scaleChangeBorder;
        [SerializeField] private Ease _scaleEase = Ease.Linear;

        [Header("Дистанция передвижения")] 
        [SerializeField] private float _distance = 50f;
        [SerializeField] private Ease _moveEase = Ease.Linear;

        [Header("Величина и тип искривление траектории")]
        [Range(0, 10)] [SerializeField] private int _curveStrength = 5;

        private Sequence _sequence;
        
        private RectTransform _rectTransform;
        private Image _image;
        private Canvas _canvas;

        public void Init(Canvas canvas)
        {
            _canvas = canvas;
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
            
            if (_sequence == null)
            {
                _sequence = DOTween.Sequence();
                _sequence.Pause();
            }
        }

        private void OnEnable()
        {
            if (_sequence != null)
            {
                _sequence.Play();
            }
        }

        private void OnDisable()
        {
            if (_sequence != null)
            {
                _sequence.Pause();
            }
        }

        public void ActivateAnimation(Action callback)
        {
            _sequence.Rewind();
            _sequence.Kill();
            _sequence = DOTween.Sequence();
            _sequence.Pause();

            Vector2 startPos = GetRandomStartPosition();
            Vector2 direction = GetRandomDirection().normalized;
            Vector2 finishPos = startPos + direction * _distance;

            _rectTransform.anchoredPosition = startPos;
            
            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0f);
                
            _sequence.Append(_image.DOFade(1f, 1).SetEase(Ease.OutQuad));
            //_sequence.AppendInterval(_duration * 0.6f);
            _sequence.Append(_image.DOFade(0f, 1).SetEase(Ease.InQuad));

            DoMoveAnimate(startPos, finishPos);

            _sequence.OnComplete(() => callback?.Invoke());
            
            _sequence.Play();
        }

        private void DoMoveAnimate(Vector2 startPos, Vector2 finishPos)
        {
            if (_curveStrength == 0)
            {
                _sequence.Join(_rectTransform.DOAnchorPos(finishPos, _duration).SetEase(_moveEase));
            }
            else
            {
                Vector2 midPoint = (startPos + finishPos) * 0.5f;
                Vector2 direction = (finishPos - startPos).normalized;
                Vector2 perpendicular = new Vector2(-direction.y, direction.x);
                Vector2 controlPoint = midPoint + perpendicular * _curveStrength * 10f;

                _sequence.Join(DOVirtual.Float(0, 1, _duration, (t) =>
                {
                    Vector2 curvedPosition = Mathf.Pow(1 - t, 2) * startPos + 
                                             2 * (1 - t) * t * controlPoint + 
                                             Mathf.Pow(t, 2) * finishPos;
                    _rectTransform.anchoredPosition = curvedPosition;
                }).SetEase(_moveEase));
            }
        }

        private Vector2 GetRandomStartPosition()
        {
            if (_canvas == null) return Vector2.zero;
            
            RectTransform canvasRect = _canvas.GetComponent<RectTransform>();
            float width = canvasRect.rect.width;
            float height = canvasRect.rect.height;

            float x = Random.Range(-width/2, width/2);
            float y = Random.Range(-height/2, height/2);

            return new Vector2(x, y);
        }

        private Vector2 GetRandomDirection()
        {
            float angle = Random.Range(0f, 2f * Mathf.PI);
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }
}
