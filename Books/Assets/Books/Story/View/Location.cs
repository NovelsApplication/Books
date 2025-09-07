using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Books.Story.View 
{
    public class Location : MonoBehaviour
    {
        [SerializeField] private VideoPlayer _player;
        [SerializeField] private RawImage _image;
        [SerializeField] private float _showHideDuration;

        public async UniTask Show(Texture image, string videoPath) 
        {
            if (image is RenderTexture rt) 
            {
                Debug.Log($"Show 1 {videoPath}");
                _player.targetTexture = rt;
                _player.url = $"file:///{videoPath}";
                Debug.Log($"Show 2 {_player.url}");
            }

            _image.color = Color.black;
            _image.texture = image;

            if (image == null) return;

            var delayMs = 50;
            var deltaTime = delayMs / 1000f;

            var timer = _showHideDuration;
            while (timer >= 0f)
            {
                _image.color = Color.Lerp(Color.white, Color.black, timer / _showHideDuration);
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
                _image.color = Color.Lerp(Color.black, startColor, timer / _showHideDuration);
                timer -= deltaTime;
                await UniTask.Delay(delayMs, true);
            }

            _image.color = Color.black;
        }

        public void HideImmediate()
        {
            _image.color = Color.black;
            _image.texture = null;
        }
    }
}

