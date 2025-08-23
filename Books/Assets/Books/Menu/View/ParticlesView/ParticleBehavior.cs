using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Books.Menu.View.ParticlesView
{
    public class ParticleBehavior : MonoBehaviour
    {
        [Header("Продолжительность")] 
        [SerializeField] private int _duration = 5;
        
        [Header("Изменение размера от текущего")]
        [Range(-1.5f, 1.5f)] [SerializeField] private float _scaleChangeBorder;
        [SerializeField] private Ease _scaleEase = Ease.Linear;

        [Header("Дистанция передвижения")] 
        [SerializeField] private int _distance = 50;
        [SerializeField] private Ease _moveEase = Ease.Linear;

        [Header("Величина и тип искривление траектории")]
        [Range(0, 10)] [SerializeField] private int _curveStength = 5;

        private Sequence _sequence;
        private RectTransform _rectTransform;
        private Graphic _image;

        private void Start()
        {
            _sequence = DOTween.Sequence();
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
            _curveStength *= 10;

            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            Vector2 startPos = GetRandomStartPosition();
            Vector2 direction = GetRandomDirection();

            Vector2 finishPos = startPos + direction * _distance;

            _rectTransform.anchoredPosition = startPos;
            
            //_sequence.Join(_image.DoF)
            DoMoveAnimate(startPos, finishPos);
        }
        
        private void DoMoveAnimate(Vector2 startPos, Vector2 finishPos)
        {
            if (_curveStength == 0)
            {
                _sequence.Join(DOVirtual.Float(0, 1, _duration, (float t) =>
                {
                    Vector2 position = Vector2.Lerp(startPos, finishPos, t);
                    _rectTransform.anchoredPosition = position;
                }));

                return;
            }
            
            Vector2 midPoint = (startPos + finishPos) * 0.5f;
            Vector2 direction = (finishPos - startPos).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
    
            Vector2 controlPoint = midPoint + perpendicular * _curveStength;
    
            _sequence.Join(DOVirtual.Float(0, 1, _duration, (t) =>
            {
                Vector2 curvedPosition = Mathf.Pow(1 - t, 2) * startPos + 
                                         2 * (1 - t) * t * controlPoint + 
                                         Mathf.Pow(t, 2) * finishPos;
                
                _rectTransform.anchoredPosition = curvedPosition;
            }).SetEase(Ease.Linear));
        }

        private void OnDisable()
        {
            _sequence.Kill();
        }

        private Vector2 GetRandomStartPosition()
        {
            int heightScreen = UnityEngine.Screen.height;
            int widthScreen = UnityEngine.Screen.width;

            float x = Random.Range(0, widthScreen * 0.95f);
            float y = Random.Range(0, heightScreen * 0.95f);

            return new Vector2(x, y);
        }

        private Vector2 GetRandomDirection()
        {
            int xDir = Random.Range(0, 2);
            int yDir = Random.Range(0, 2);

            if (xDir == 0) xDir *= -1;
            if (yDir == 0) yDir *= -1;
            
            float x = Random.Range(0f, 1f);
            float y = Random.Range(0f, 1f);
            
            return new Vector2(x * xDir, y * yDir);
        }
    }
}