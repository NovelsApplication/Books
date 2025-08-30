using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Story.View 
{
    public interface IScreen
    {
        public void SetCloseAction(Action<bool> onClick);

        public void ShowImmediate();
        public void HideImmediate();

        public UniTask ShowBubble(Action<int> onClick, string mainCharacter, string header, string body, Texture2D characterImage, params (string header, int index)[] buttons);
        public void HideBubbleImmediate();

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
        [SerializeField] private Button _closeButtonWithClearSave;
        [SerializeField] private Location _location;
        [SerializeField] private Character _character;

        public void SetCloseAction(Action<bool> onClick) 
        {
            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(() => onClick.Invoke(false));

            _closeButtonWithClearSave.onClick.RemoveAllListeners();
            _closeButtonWithClearSave.onClick.AddListener(() => onClick.Invoke(true));
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

        public async UniTask ShowBubble(Action<int> onClick, string mainCharacter, string header, string body, Texture2D characterImage, params (string header, int index)[] buttons) 
        {
            await _bubble.ShowBubble(onClick, mainCharacter, header, body, buttons);

            if (characterImage != null)
            {
                await _character.Show(characterImage, (header.ToLower() != mainCharacter.ToLower()));
            }
        }

        public void HideBubbleImmediate() 
        {
            _character.HideImmediate();
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

