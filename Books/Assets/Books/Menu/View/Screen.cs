using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Books.Menu.View 
{
    public interface IScreen
    {
        public void SetTheme(bool isLightTheme);
        public void ShowImmediate();
        public void HideImmediate();
        public UniTask AddBookAsync(string storyText, Texture2D poster, Entity.StoryManifest storyManifest, Action onClick, Func<string, (string header, string attributes, string body)?> processLine);
        public void OnAllBooksAdded();
        public void Release();
    }

    public sealed class Screen : MonoBehaviour, IScreen
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private ScreenBook _mainScreenBook;
        [SerializeField] private SnapController _snapController;
        [SerializeField] private BackgroundAnimation _backgroundAnimation;
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

        public async UniTask AddBookAsync(string storyText, Texture2D poster, Entity.StoryManifest storyManifest, Action onClick, Func<string, (string header, string attributes, string body)?> processLine) 
        {
            var story = new Ink.Runtime.Story(storyText);

            var label = Entity.Labels.Next;
            var storyHeader = string.Empty;
            var description = string.Empty;
            var tags = new string[0];
            var mainTags = new Entity.MainTags[0];
            while (story.canContinue) 
            {
                var lineData = processLine.Invoke(story.Continue());
                if (!lineData.HasValue) continue;

                if (lineData.Value.header.ToLower() == "название") storyHeader = lineData.Value.body;
                if (lineData.Value.header.ToLower() == "жанры") tags = lineData.Value.body.Split(",", StringSplitOptions.RemoveEmptyEntries);
                if (lineData.Value.header.ToLower() == "бирка" && Enum.TryParse<Entity.Labels>(lineData.Value.body, out var l)) label = l;
                if (lineData.Value.header.ToLower() == "раздел") 
                {
                    mainTags = lineData.Value.body.Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Where(b => Enum.TryParse<Entity.MainTags>(b, out _))
                        .Select(b => Enum.Parse<Entity.MainTags>(b))
                        .ToArray();
                }
                if (lineData.Value.header.ToLower() == "аннотация") description = lineData.Value.body;
            }

            _mainScreenBook.gameObject.SetActive(false);
            var screenBook = UnityEngine.Object.Instantiate<ScreenBook>(_mainScreenBook, _mainScreenBook.transform.parent);
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

            _mainScreenDot.gameObject.SetActive(false);
            var dot = UnityEngine.Object.Instantiate<Dot>(_mainScreenDot, _mainScreenDot.transform.parent);
            dot.gameObject.SetActive(true);
            dot.SetSelected(false);
            _objects.Push(dot.gameObject);

            foreach (var mainTag in _mainTags) 
            {
                mainTag.SetSelected(false);
            }

            _mainScreenLittleBook.gameObject.SetActive(false);
            var screenLittleBook = UnityEngine.Object.Instantiate<ScreenBook>(_mainScreenLittleBook, _mainScreenLittleBook.transform.parent);
            screenLittleBook.gameObject.SetActive(true);
            screenLittleBook.SetLabels(label);
            screenLittleBook.SetImage(poster);
            screenLittleBook.SetButton(() =>
            {
                OpenPopUp(poster, storyHeader, description, onClick);
            });
            _objects.Push(screenLittleBook.gameObject);
        }

        public void OnAllBooksAdded()
        {
            Vector2 targetSize = _mainScreenBook.GetComponent<RectTransform>().sizeDelta;
            Transform parent = _mainScreenBook.transform.parent;

            GameObject firstBorderObject = new GameObject("EmptyBorder_Start");
            RectTransform firstBorderRect = firstBorderObject.AddComponent<RectTransform>();
            firstBorderRect.sizeDelta = targetSize;
            firstBorderRect.SetParent(parent, false);
            firstBorderRect.SetSiblingIndex(1); // 1 потому что там первый объект пустышка стоит 

            GameObject secondBorderObject = new GameObject("EmptyBorder_End");
            RectTransform secondBorderRect = secondBorderObject.AddComponent<RectTransform>();
            secondBorderRect.sizeDelta = targetSize;
            secondBorderRect.SetParent(parent, false);
            secondBorderRect.SetSiblingIndex(parent.childCount - 1);
            
            _snapController.Initialize();
            _backgroundAnimation.InitializeParticles();
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

