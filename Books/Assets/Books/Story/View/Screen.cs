using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Story.View 
{
    public interface IScreen
    {
        public void SetCloseAction(Action onClick);

        public void ShowImmediate();
        public void HideImmediate();

        public UniTask ShowBubble(Action<int> onClick, string mainCharacter, string header, string body, params (string header, int index)[] buttons);
        public void HideBubble();

        public UniTask ShowLocation(Texture2D image);
        public UniTask HideLocation();
        public void HideLocationImmediate();

        public void Release();
    }

    public class Screen : MonoBehaviour, IScreen
    {
        [SerializeField] private Bubble _bubble;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Location _location;

        public void SetCloseAction(Action onClick) 
        {
            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(() => onClick.Invoke());
        }

        public void ShowImmediate()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.gameObject.SetActive(true);
        }

        public void HideImmediate() 
        {
            if (_canvasGroup != null) 
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.gameObject.SetActive(false);
            }
        }

        public async UniTask ShowBubble(Action<int> onClick, string mainCharacter, string header, string body, params (string header, int index)[] buttons) 
        {
            await _bubble.ShowBubble(onClick, mainCharacter, header, body, buttons);
        }

        public void HideBubble() 
        {
            _bubble.gameObject.SetActive(false);
        }

        public async UniTask ShowLocation(Texture2D image) 
        {
            await _location.Show(image);
        }

        public async UniTask HideLocation() 
        {
            await _location.Hide();
        }

        public void HideLocationImmediate() 
        {
            _location.HideImmediate();
        }

        public void Release() 
        {
            if (this == null) return;

            GameObject.Destroy(gameObject);
        }
    }
}

