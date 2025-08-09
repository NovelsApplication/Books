using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Story.View 
{
    public class Character : MonoBehaviour
    {
        [SerializeField] private RawImage _image;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private float _showHideDuration;

        public async UniTask Show(Texture2D image, bool isRight)
        {
            if (isRight)
            {
                _rectTransform.anchorMin = new Vector2(0.4f, 0f);
                _rectTransform.anchorMax = new Vector2(1f, 1f);
                _rectTransform.localScale = new Vector3(-1f, 1f, 1f);
            }
            else
            {
                _rectTransform.anchorMin = new Vector2(0.0f, 0f);
                _rectTransform.anchorMax = new Vector2(0.6f, 1f);
                _rectTransform.localScale = new Vector3(1f, 1f, 1f);
            }

                _image.color = Color.clear;
            _image.texture = image;

            var delayMs = 50;
            var deltaTime = delayMs / 1000f;

            var timer = _showHideDuration;
            while (timer >= 0f)
            {
                _image.color = Color.Lerp(Color.white, Color.clear, timer / _showHideDuration);
                timer -= deltaTime;
                await UniTask.Delay(delayMs, true);
            }

            _image.color = Color.white;
        }

        public async UniTask Hide()
        {
            var startColor = _image.color;

            var delayMs = 50;
            var deltaTime = delayMs / 1000f;

            var timer = _showHideDuration;
            while (timer >= 0f)
            {
                _image.color = Color.Lerp(Color.clear, startColor, timer / _showHideDuration);
                timer -= deltaTime;
                await UniTask.Delay(delayMs, true);
            }

            _image.color = Color.clear;
        }

        public void HideImmediate()
        {
            _image.color = Color.clear;
            _image.texture = null;
        }
    }
}

