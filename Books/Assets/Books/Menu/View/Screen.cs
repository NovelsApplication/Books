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
        public UniTask Show();
        public void HideImmediate();
        public UniTask AddBookAsync(Entity.StoryManifest storyManifest, Action onClick);
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

        public async UniTask Show()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(true);
            await UniTask.Delay(50);
            _canvasGroup.gameObject.SetActive(false);
            await UniTask.Delay(50);
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

        public async UniTask AddBookAsync(Entity.StoryManifest storyManifest, Action onClick) 
        {
            var storyText = await Cacher.GetTextAsync($"{storyManifest.StoryPath}/Story.json");
            var story = new Ink.Runtime.Story(storyText);

            var posterPath = string.Empty;
            var label = Entity.Labels.Next;
            var storyHeader = string.Empty;
            var description = string.Empty;
            var tags = new string[0];
            var mainTags = new Entity.MainTags[0];
            while (story.canContinue) 
            {
                if (!story.Continue().TryProcessLine(out var header, out var attributes, out var body))
                    continue;

                if (header.ToLower() == "название") storyHeader = body;
                if (header.ToLower() == "жанры") tags = body.Split(",", StringSplitOptions.RemoveEmptyEntries);
                if (header.ToLower() == "бирка" && Enum.TryParse<Entity.Labels>(body, out var l)) label = l;
                if (header.ToLower() == "раздел") 
                {
                    mainTags = body.Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Where(b => Enum.TryParse<Entity.MainTags>(b, out _))
                        .Select(b => Enum.Parse<Entity.MainTags>(b))
                        .ToArray();
                }
                if (header.ToLower() == "постер") posterPath = $"{storyManifest.StoryPath}/{body}";
                if (header.ToLower() == "аннотаци€") description = body;
            }

            var posterImage = await Cacher.GetTextureAsync(posterPath, "poster");

            _mainScreenBook.gameObject.SetActive(false);
            var screenBooks = await UnityEngine.Object.InstantiateAsync<ScreenBook>(_mainScreenBook, _mainScreenBook.transform.parent);
            foreach (var screenBook in screenBooks) 
            { 
                screenBook.gameObject.SetActive(true);
                screenBook.SetLabels(label);
                screenBook.SetHeader(storyHeader);
                screenBook.SetDescription(description);
                screenBook.SetTags(tags);
                screenBook.SetImage(posterImage);
                screenBook.SetButton(() => 
                {
                    OpenPopUp(posterImage, storyHeader, description, onClick);
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
                screenLittleBook.SetImage(posterImage);
                screenLittleBook.SetButton(() =>
                {
                    OpenPopUp(posterImage, storyHeader, description, onClick);
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

