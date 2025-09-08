using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Books.Menu.View.ParticlesView
{
    public class ParticleBehavior : MonoBehaviour
    {
        [Header("Продолжительность")] [FormerlySerializedAs("duration")] 
        [SerializeField] protected float _duration = 5f;

        [Header("Дистанция передвижения")] 
        [SerializeField] private float _distance = 50f;
        [SerializeField] private Ease _moveEase = Ease.Linear;
        
        [Header("Скорость вращения")]
        [Range(0, 5)] [SerializeField] private int _rotationStrength = 3;

        [Header("Величина и тип искривление траектории")]
        [Range(0, 10)] [SerializeField] private int _curveStrength = 5;

        [Header("Начальная прозрачность")] [FormerlySerializedAs("startFade")] 
        [Range(0, 1)] [SerializeField] protected float _startFade = 0.7f;

        [Header("Время ПОЯВЛЕНИЯ в процентах от общего")] [FormerlySerializedAs("fadeInPercentage")] 
        [Range(0, 0.5f)] [SerializeField] protected float _fadeInPercentage = 0.2f;
        
        [Header("Время УГАСАНИЯ в процентах от общего")] [FormerlySerializedAs("fadeOutPercentage")] 
        [Range(0, 0.5f)] [SerializeField] protected float _fadeOutPercentage = 0.2f;

        private readonly System.Random _random = new ();
        
        protected RectTransform Target { get; private set; }
        
        private Sequence _mainSequence;
        private Image _image;
        private Canvas _canvas;

        public void Init(Canvas canvas)
        {
            _canvas = canvas;
            Target = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
        }

        public virtual void ActivateAnimation(Action callback = null)
        {
            _mainSequence?.Kill();
            _mainSequence = DOTween.Sequence();
            _mainSequence.Pause();
            
            Vector2 startPos = GetRandomStartPosition();
            Vector2 direction = GetRandomDirection().normalized;
            Vector2 finishPos = startPos + direction * _distance;

            Target.anchoredPosition = startPos;
            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0f);
            
            _mainSequence.Append(Target.DoCurveAnchorPos(startPos, finishPos, _curveStrength, _duration).SetEase(_moveEase));
            _mainSequence.Join(_image.DOFade(_startFade, _duration * _fadeInPercentage).SetEase(Ease.OutQuad));
            _mainSequence.Join(Target.DORotate(new Vector3(0, 0, _rotationStrength * 90f),
                _duration, RotateMode.FastBeyond360).SetEase(Ease.Linear));
            _mainSequence.Insert(_duration * (1 - _fadeOutPercentage), 
                _image.DOFade(0f, _duration * _fadeOutPercentage).SetEase(Ease.InQuad));
            
            _mainSequence.OnComplete(() => callback?.Invoke());
            _mainSequence.Play();
        }

        protected void InsertInMainSequence(float atTimePosition, Tween tween)
        {
            _mainSequence?.Insert(atTimePosition, tween);
        }

        protected void PlayMainSequence()
        {
            _mainSequence?.Play();
        }

        protected Tween DoFadeParticleImage(float endValue, float duration)
        {
            return _image.DOFade(endValue, duration);
        }

        private void OnDisable()
        {
            _mainSequence?.Kill();
        }

        private void OnDestroy()
        {
            _mainSequence?.Kill();
        }

        private Vector2 GetRandomStartPosition()
        {
            if (_canvas == null) return Vector2.zero;
            
            RectTransform canvasRect = _canvas.GetComponent<RectTransform>();
            float width = canvasRect.rect.width;
            float height = canvasRect.rect.height;

            return new Vector2(
                (float)_random.NextDouble() * width - width / 2,
                (float)_random.NextDouble() * height - height / 2
            );
        }

        private Vector2 GetRandomDirection()
        {
            float angle = (float)_random.NextDouble() * 2 * Mathf.PI;
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
