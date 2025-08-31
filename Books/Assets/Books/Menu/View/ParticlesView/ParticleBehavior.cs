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
        [Header("Продолжительность")] 
        [SerializeField] protected float duration = 5f;

        [Header("Дистанция передвижения")] 
        [SerializeField] private float _distance = 50f;
        [SerializeField] private Ease _moveEase = Ease.Linear;
        
        [Header("Скорость вращения")]
        [Range(0, 5)] [SerializeField] private int _rotationStrength = 3;

        [Header("Величина и тип искривление траектории")]
        [Range(0, 10)] [SerializeField] private int _curveStrength = 5;

        [Header("Начальная прозрачность")] 
        [Range(0, 1)] [SerializeField] protected float startFade = 0.7f;

        [Header("Время ПОЯВЛЕНИЯ в процентах от общего")] 
        [Range(0, 0.5f)] [SerializeField] protected float fadeInPercentage = 0.2f;
        
        [Header("Время УГАСАНИЯ в процентах от общего")] 
        [Range(0, 0.5f)] [SerializeField] protected float fadeOutPercentage = 0.2f;

        protected Sequence Sequence;
        
        protected RectTransform Target;
        protected Image Image;
        protected Canvas Canvas;

        public void Init(Canvas canvas)
        {
            Canvas = canvas;
            Target = GetComponent<RectTransform>();
            Image = GetComponent<Image>();
        }

        public virtual void ActivateAnimation(Action callback = null)
        {
            Sequence.Rewind();
            Sequence.Kill();
            Sequence = DOTween.Sequence();
            Sequence.Pause();
            
            Vector2 startPos = GetRandomStartPosition();
            Vector2 direction = GetRandomDirection().normalized;
            Vector2 finishPos = startPos + direction * _distance;

            Target.anchoredPosition = startPos;
            
            Image.color = new Color(Image.color.r, Image.color.g, Image.color.b, 0f);
            
            Sequence.Append(Target.DoCurveAnchorPos(startPos, finishPos, _curveStrength, duration)).SetEase(_moveEase);
            Sequence.Join(Image.DOFade(startFade, duration * fadeInPercentage).SetEase(Ease.OutQuad));
            Sequence.Join(Target.DORotate(new Vector3(0, 0, _rotationStrength * 90f),
                duration, RotateMode.FastBeyond360).SetEase(Ease.Linear));
            Sequence.Insert(duration * (1 - fadeOutPercentage), 
                Image.DOFade(0f, duration * fadeOutPercentage).SetEase(Ease.InQuad));
            
            Sequence.OnComplete(() => callback?.Invoke());
            Sequence.Play();
        }

        private void OnEnable()
        {
            if (Sequence != null)
            {
                Sequence.Play();
            }
        }

        private void OnDisable()
        {
            if (Sequence != null)
            {
                Sequence.Kill();
                Sequence = null;
            }
        }

        private Vector2 GetRandomStartPosition()
        {
            if (Canvas == null) return Vector2.zero;
            
            RectTransform canvasRect = Canvas.GetComponent<RectTransform>();
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
