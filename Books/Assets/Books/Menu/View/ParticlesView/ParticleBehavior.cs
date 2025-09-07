using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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

        [FormerlySerializedAs("startFade")]
        [Header("Начальная прозрачность")] 
        [Range(0, 1)] [SerializeField] protected float _startFade = 0.7f;

        [FormerlySerializedAs("fadeInPercentage")]
        [Header("Время ПОЯВЛЕНИЯ в процентах от общего")] 
        [Range(0, 0.5f)] [SerializeField] protected float _fadeInPercentage = 0.2f;
        
        [FormerlySerializedAs("fadeOutPercentage")]
        [Header("Время УГАСАНИЯ в процентах от общего")] 
        [Range(0, 0.5f)] [SerializeField] protected float _fadeOutPercentage = 0.2f;

        // можно отказаться от свойст и заменить функциями
        protected Sequence MainSequence { get; private set; }
        protected RectTransform Target { get; private set; }
        protected Image Image { get; private set; }
        protected Canvas Canvas { get; private set; }

        public void Init(Canvas canvas)
        {
            Canvas = canvas;
            Target = GetComponent<RectTransform>();
            Image = GetComponent<Image>();
        }

        public virtual void ActivateAnimation(Action callback = null)
        {
            CleanupSequence();
            MainSequence = DOTween.Sequence();
            MainSequence.Pause();
            
            Vector2 startPos = GetRandomStartPosition();
            Vector2 direction = GetRandomDirection().normalized;
            Vector2 finishPos = startPos + direction * _distance;

            Target.anchoredPosition = startPos;
            Image.color = new Color(Image.color.r, Image.color.g, Image.color.b, 0f);
            
            MainSequence.Append(Target.DoCurveAnchorPos(startPos, finishPos, _curveStrength, _duration).SetEase(_moveEase));
            MainSequence.Join(Image.DOFade(_startFade, _duration * _fadeInPercentage).SetEase(Ease.OutQuad));
            MainSequence.Join(Target.DORotate(new Vector3(0, 0, _rotationStrength * 90f),
                _duration, RotateMode.FastBeyond360).SetEase(Ease.Linear));
            MainSequence.Insert(_duration * (1 - _fadeOutPercentage), 
                Image.DOFade(0f, _duration * _fadeOutPercentage).SetEase(Ease.InQuad));
            
            MainSequence.OnComplete(() => callback?.Invoke());
            MainSequence.Play();
        }

        private void OnEnable()
        {
            MainSequence?.Play();
        }

        private void OnDisable()
        {
            CleanupSequence();
        }

        private void OnDestroy()
        {
            CleanupSequence();
        }

        private void CleanupSequence()
        {
            MainSequence?.Kill();
        }

        private Vector2 GetRandomStartPosition()
        {
            if (Canvas == null) return Vector2.zero;
            
            RectTransform canvasRect = Canvas.GetComponent<RectTransform>();
            float width = canvasRect.rect.width;
            float height = canvasRect.rect.height;

            return new Vector2(
                Random.Range(-width/2, width/2),
                Random.Range(-height/2, height/2)
            );
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
