using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Books.Menu.View.ParticlesView
{
    public class ParticleBehavior : MonoBehaviour
    {
        [Header("Продолжительность")] 
        [SerializeField] private float _duration = 5f;

        [Header("Дистанция передвижения")] 
        [SerializeField] private float _distance = 50f;
        [SerializeField] private Ease _moveEase = Ease.Linear;
        
        [Header("Скорость вращения")]
        [Range(0, 5)] [SerializeField] private int _rotationStrength = 3;

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
                _sequence.Kill();
                _sequence = null;
            }
        }

        public virtual void ActivateAnimation(Action callback = null)
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

            float visibleTimePart = 0.75f;

            _sequence.Append(_rectTransform.DoCurveAnchorPos(startPos, finishPos, _curveStrength, _duration)).SetEase(_moveEase);
            _sequence.Join(_image.DOFade(1f, _duration * (1 - visibleTimePart)).SetEase(Ease.OutQuad));
            _sequence.Join(_rectTransform.DORotate(new Vector3(0, 0, _rotationStrength * 90f), _duration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear));
            _sequence.Insert(_duration * visibleTimePart, 
                _image.DOFade(0f, _duration - _duration * visibleTimePart).SetEase(Ease.InQuad));

            _sequence.OnComplete(() => callback?.Invoke());
            
            _sequence.Play();
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

    public static class RectTransformParticleExtension
    {
        public static Tweener DoCurveAnchorPos(this RectTransform target, Vector2 startPos, Vector2 finishPos, 
            float curveStrength, float duration)
        {
            Tweener tween;
            
            if (curveStrength == 0)
            {
                tween = target.DOAnchorPos(finishPos, duration);
                return tween;
            }
            
            Vector2 midPoint = (startPos + finishPos) * 0.5f;
            Vector2 direction = (finishPos - startPos).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            Vector2 controlPoint = midPoint + perpendicular * curveStrength * 20f;

            Func<float, Vector2> getCurvedPosition = (t) =>
            {
                Vector2 curvedPosition = Mathf.Pow(1 - t, 2) * startPos +
                                         2 * (1 - t) * t * controlPoint +
                                         Mathf.Pow(t, 2) * finishPos;
                
                return curvedPosition;
            };
            
            Vector2 pathDirection = finishPos - startPos;
            Vector2 toCurrentPosDir = target.anchoredPosition - startPos;
            float projectionLength = Vector2.Dot(toCurrentPosDir, pathDirection);
            float progress = Mathf.Clamp01(projectionLength / pathDirection.magnitude);

            float t = progress;
            
            tween = DOTween.To(() => t,
                t => target.anchoredPosition = getCurvedPosition(t),
                1, duration);

            tween.SetTarget(target);

            return tween;
        }
    }
}
