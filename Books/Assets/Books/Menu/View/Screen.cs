using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.LocalCache;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Books.Menu.View 
{
    public interface IScreen
    {
        public void SetTheme(bool isLightTheme);
        public void ShowImmediate();
        public void HideImmediate();
        public UniTask AddBookAsync(string storyText, Texture2D poster, Entity.StoryManifest storyManifest, Action onClick);
        public void Release();
    }

    public sealed class Screen : MonoBehaviour, IScreen
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private ScreenBook _mainScreenBook;
        [SerializeField] private Dot _mainScreenDot;
        [SerializeField] private MainTag[] _mainTags;
        [SerializeField] private ScreenBook _mainScreenLittleBook;
        [SerializeField] private PopUp _popUp;

        [SerializeField] private GameObject[] _lightElements;
        [SerializeField] private GameObject[] _darkElements;

        private readonly Stack<GameObject> _objects = new ();

        public void SetTheme(bool isLightTheme) 
        {
            foreach (var element in _lightElements) element.SetActive(isLightTheme);
            foreach (var element in _darkElements) element.SetActive(!isLightTheme);
        }

        public void ShowImmediate()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(true);
            _canvasGroup.gameObject.SetActive(false);
            _canvasGroup.gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;

            _popUp.HideImmediate();
        }

        public void HideImmediate()
        {
            if (_canvasGroup != null) 
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.gameObject.SetActive(false);
            }
        }

        public async UniTask AddBookAsync(string storyText, Texture2D poster, Entity.StoryManifest storyManifest, Action onClick) 
        {
            var story = new Ink.Runtime.Story(storyText);

            var label = Entity.Labels.Next;
            var storyHeader = string.Empty;
            var description = string.Empty;
            var tags = new string[0];
            var mainTags = new Entity.MainTags[0];
            while (story.canContinue) 
            {
                if (!story.Continue().TryProcessLine(out var header, out var attributes, out var body))
                    continue;

                if (header.ToLower() == "��������") storyHeader = body;
                if (header.ToLower() == "�����") tags = body.Split(",", StringSplitOptions.RemoveEmptyEntries);
                if (header.ToLower() == "�����" && Enum.TryParse<Entity.Labels>(body, out var l)) label = l;
                if (header.ToLower() == "������") 
                {
                    mainTags = body.Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Where(b => Enum.TryParse<Entity.MainTags>(b, out _))
                        .Select(b => Enum.Parse<Entity.MainTags>(b))
                        .ToArray();
                }
                if (header.ToLower() == "���������") description = body;
            }

            _mainScreenBook.gameObject.SetActive(false);
            var screenBooks = await UnityEngine.Object.InstantiateAsync<ScreenBook>(_mainScreenBook, _mainScreenBook.transform.parent);
            foreach (var screenBook in screenBooks) 
            { 
                screenBook.gameObject.SetActive(true);
                screenBook.SetLabels(label);
                screenBook.SetHeader(storyHeader);
                screenBook.SetDescription(description);
                screenBook.SetTags(tags);
                screenBook.SetImage(poster);
                screenBook.SetButton(() => 
                {
                    OpenPopUp(poster, storyHeader, description, onClick);
                });
                _objects.Push(screenBook.gameObject);
            }

            _mainScreenDot.gameObject.SetActive(false);
            var dots = await UnityEngine.Object.InstantiateAsync<Dot>(_mainScreenDot, _mainScreenDot.transform.parent);
            foreach (var dot in dots)
            {
                dot.gameObject.SetActive(true);
                dot.SetSelected(false);
                _objects.Push(dot.gameObject);
            }

            foreach (var mainTag in _mainTags) 
            {
                mainTag.SetSelected(false);
            }

            _mainScreenLittleBook.gameObject.SetActive(false);
            var screenLittleBooks = await UnityEngine.Object.InstantiateAsync<ScreenBook>(_mainScreenLittleBook, _mainScreenLittleBook.transform.parent);
            foreach (var screenLittleBook in screenLittleBooks) 
            { 
                screenLittleBook.gameObject.SetActive(true);
                screenLittleBook.SetLabels(label);
                screenLittleBook.SetImage(poster);
                screenLittleBook.SetButton(() =>
                {
                    OpenPopUp(poster, storyHeader, description, onClick);
                });
                _objects.Push(screenLittleBook.gameObject);
            }
        }

        private void OpenPopUp(Texture2D texture, string header, string description, Action onClick)
        {
            _popUp.SetBackgroundButton(() => _popUp.Hide().Forget());
            _popUp.SetImage(texture);
            _popUp.SetHeader(header);
            _popUp.SetDescription(description);
            _popUp.SetReadButton(() =>
            {
                _popUp.HideImmediate();
                onClick.Invoke();
            });

            _popUp.Show().Forget();
        }

        public void Release() 
        {
            while(_objects.Count > 0) 
            {
                GameObject.Destroy(_objects.Pop());
            }

            if (this != null)
            {
                GameObject.Destroy(gameObject);
            }
        }
    }
}

